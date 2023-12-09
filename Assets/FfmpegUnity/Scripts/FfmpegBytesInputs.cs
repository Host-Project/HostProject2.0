using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace FfmpegUnity
{
    public class FfmpegBytesInputs : IDisposable
    {
        const int BUFFER_SIZE = 5000000;
        const int BLOCK_SIZE = 1024;

        public string[] InputPipeNames
        {
            get;
            private set;
        }

        public bool IsEmpty
        {
            get
            {
                if (inputBytes_ == null)
                {
                    return true;
                }

                for (int loop = 0; loop < inputBytes_.Length; loop++)
                {
                    if (inputBytes_[loop].Count > bytesPosition_[loop])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        string[] inputOptions_;

        List<byte[]>[] inputBytes_ = null;

        int bufferSize_;

        bool isStop_ = false;

        List<Thread> threads_ = new List<Thread>();

        bool keep_;
        int[] bytesPosition_;
        bool keepFile_ = false;

        FfmpegCommand command_;

#if (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
        [DllImport("__Internal")]
        static extern int unity_system(string command);
#endif

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

#elif UNITY_IOS
        [DllImport("__Internal")]
        static extern void ffmpeg_mkpipe(IntPtr output, int outputLength);
        [DllImport("__Internal")]
        static extern void ffmpeg_closePipe(string pipeName);
#endif

        public FfmpegBytesInputs(string[] inputOptions, FfmpegCommand command, int bufferSize = BUFFER_SIZE)
        {
            inputOptions_ = inputOptions;
            command_ = command;
            bufferSize_ = bufferSize;

            resetInputBytes();
        }

        void resetInputBytes()
        {
            inputBytes_ = new List<byte[]>[inputOptions_.Length];
            for (int loop = 0; loop < inputBytes_.Length; loop++)
            {
                inputBytes_[loop] = new List<byte[]>();
            }
        }

        public void AddInputBytes(byte[] bytes, int inputNo = 0)
        {
            lock (inputBytes_[inputNo])
            {
                inputBytes_[inputNo].Add(bytes);
            }
        }

        public string BuildAndStart(bool keep = false)
        {
            string options = "";

            if (!keepFile_)
            {
                InputPipeNames = new string[inputOptions_.Length];
            }
            keep_ = keep;
            bytesPosition_ = new int[inputOptions_.Length];

            for (int loop = 0; loop < inputOptions_.Length; loop++)
            {
                options += " " + inputOptions_[loop];

                string fileName = "";
                if (keepFile_)
                {
                    fileName = InputPipeNames[loop];
                }
                else
                {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    fileName = @"\\.\pipe\FfmpegUnity_" + Guid.NewGuid().ToString();
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                    fileName = Application.temporaryCachePath + "/FfmpegUnity_" + Guid.NewGuid().ToString();
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                    fileName = "/tmp/FfmpegUnity_" + Guid.NewGuid().ToString();
#elif UNITY_ANDROID
                    string dataDir;

                    using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                    using (AndroidJavaObject info = context.Call<AndroidJavaObject>("getApplicationInfo"))
                    {
                        dataDir = info.Get<string>("dataDir");
                    }

                    fileName = dataDir + "/FfmpegUnity_" + Guid.NewGuid().ToString();
#elif UNITY_IOS

#endif
                }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                ProcessStartInfo psInfoMkFifo = new ProcessStartInfo()
                {
                    FileName = "mkfifo",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = "\"" + fileName + "\"",
                };
                using (Process process = Process.Start(psInfoMkFifo))
                {
                    process.WaitForExit();
                }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                unity_system("mkfifo \"" + fileName + "\"");
#elif UNITY_ANDROID
                using (AndroidJavaClass os = new AndroidJavaClass("android.system.Os"))
                {
                    os.CallStatic("mkfifo", fileName, Convert.ToInt32("777", 8));
                }
#elif UNITY_IOS
                if (!keepFile_)
                {
                    IntPtr hglobalPipe = Marshal.AllocHGlobal(1024);
                    ffmpeg_mkpipe(hglobalPipe, 1024);
                    fileName = Marshal.PtrToStringAuto(hglobalPipe);
                    Marshal.FreeHGlobal(hglobalPipe);
                }
#endif

                InputPipeNames[loop] = fileName;
                options += " -i \"" + fileName + "\" ";

                int index = loop;
                var thread = new Thread(() => {
                    write(fileName, index);
                });
                thread.Start();
                threads_.Add(thread);
            }

            return options;
        }

        public string Restart(bool keep = false)
        {
            keepFile_ = keep;
            isStop_ = true;

            foreach (var thread in threads_)
            {
                thread.Join();
            }
            threads_.Clear();

            isStop_ = false;

            keep_ = keep;

            string ret = BuildAndStart(keep);

            keepFile_ = false;

            return ret;
        }

        public void Dispose()
        {
            isStop_ = true;

            foreach (var thread in threads_)
            {
                bool exited = thread.Join(1);
                while (!exited && command_.IsRunning)
                {
                    exited = thread.Join(1);
                }
                if (!exited && !command_.IsRunning)
                {
                    thread.Abort();
                }
            }
            threads_.Clear();
        }

        void streamWrite(BinaryWriter writer, int streamId)
        {
            while (!isStop_)
            {
                if (inputBytes_[streamId].Count <= bytesPosition_[streamId])
                {
                    Thread.Sleep(1);
                    continue;
                }

                byte[] buffer;
                lock (inputBytes_[streamId])
                {
                    buffer = inputBytes_[streamId][bytesPosition_[streamId]];
                    if (keep_)
                    {
                        bytesPosition_[streamId]++;
                    }
                    else
                    {
                        inputBytes_[streamId].RemoveAt(bytesPosition_[streamId]);
                    }
                }

                try
                {
                    if (writer.BaseStream.CanWrite)
                    {
                        writer.Write(buffer);
                    }
                }
                catch (IOException)
                {
                    isStop_ = true;
                    break;
                }
            }
        }

        void write(string pipeFileName, int streamId)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream(pipeFileName.Replace(@"\\.\pipe\", ""),
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                bufferSize_,
                bufferSize_))
            {
                Thread thread = new Thread(() =>
                {
                    while (!isStop_ && !stream.IsConnected)
                    {
                        Thread.Sleep(1);
                    }
                    if (!stream.IsConnected)
                    {
                        using (var dummyStream = new NamedPipeClientStream(".", pipeFileName.Replace(@"\\.\pipe\", ""), PipeDirection.In))
                        {
                            dummyStream.Connect();
                        }
                    }
                });
                thread.Start();
                threads_.Add(thread);

                stream.WaitForConnection();

                try
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        streamWrite(writer, streamId);
                    }
                }
                catch (IOException)
                {
                    isStop_ = true;
                }
            }
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            var pipeOption = new FfmpegExecuteIL2CPPWin.PipeOption();
            pipeOption.BlockSize = BLOCK_SIZE;
            pipeOption.BufferSize = bufferSize_;
            pipeOption.PipeName = pipeFileName.Replace(@"\\.\pipe\", "");
            pipeOption.StdMode = 3;
            command_.LastPipeOptions.Add(pipeOption);

            while (command_.ExecuteObj == null || command_.ExecuteObj.GetStream(streamId + command_.PipeOptionsList.Count - command_.LastPipeOptions.Count) == null)
            {
                Thread.Sleep(1);
            }

            try
            {
                using (var writer = new BinaryWriter(command_.ExecuteObj.GetStream(streamId + command_.PipeOptionsList.Count - command_.LastPipeOptions.Count)))
                {
                    streamWrite(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP) || UNITY_ANDROID
            try
            {
                using (var stream = File.OpenWrite(pipeFileName))
                using (var writer = new BinaryWriter(stream))
                {
                    streamWrite(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
#elif UNITY_IOS
            try
            {
                using (var stream = File.OpenWrite(pipeFileName))
                using (var writer = new BinaryWriter(stream))
                {
                    streamWrite(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            if (!keepFile_)
            {
                File.Delete(pipeFileName);
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            while (!File.Exists(pipeFileName))
            {
                Thread.Sleep(1);
            }

            try
            {
                using (var stream = File.OpenWrite(pipeFileName))
                using (var writer = new BinaryWriter(stream))
                {
                    streamWrite(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
#endif
        }

        public interface IInputControl
        {
            void AddInputBytes(byte[] bytes, int inputNo = 0);

            bool InputBytesIsEmpty { get; }
        }
    }
}
