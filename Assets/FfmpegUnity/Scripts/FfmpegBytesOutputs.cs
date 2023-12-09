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
    public class FfmpegBytesOutputs : IDisposable
    {
        const int BUFFER_SIZE = 5000000;
        const int BLOCK_SIZE = 1024;

        string[] outputOptions_;
        FfmpegCommand command_;

        string[] outputPipeNames_;
        List<byte>[] outputBytes_ = null;
        List<Thread> threads_ = new List<Thread>();
        bool isEnd_ = false;

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

        void resetOutput()
        {
            outputBytes_ = new List<byte>[outputOptions_.Length];
            for (int loop = 0; loop < outputBytes_.Length; loop++)
            {
                outputBytes_[loop] = new List<byte>();
            }
        }

        public FfmpegBytesOutputs(string[] outputOptions, FfmpegCommand command)
        {
            outputOptions_ = outputOptions;
            command_ = command;

            resetOutput();
        }

        string getDataDir()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || (!UNITY_ANDROID && !UNITY_EDITOR_WIN)
            return Application.temporaryCachePath;
#elif UNITY_ANDROID && !UNITY_EDITOR_WIN
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject info = context.Call<AndroidJavaObject>("getApplicationInfo"))
            {
                return info.Get<string>("dataDir");
            }
#else
            return null;
#endif
        }

        public string BuildAndStart()
        {
            string options = " ";

            outputPipeNames_ = new string[outputOptions_.Length];

            for (int loop = 0; loop < outputOptions_.Length; loop++)
            {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
                outputPipeNames_[loop] = @"\\.\pipe\FfmpegUnity_" + Guid.NewGuid().ToString();
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                {
                    string fileNameFifo = getDataDir() + "/FfmpegUnity_" + Guid.NewGuid().ToString();

                    ProcessStartInfo psInfoMkFifo = new ProcessStartInfo()
                    {
                        FileName = "mkfifo",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Arguments = "\"" + fileNameFifo + "\"",
                    };
                    using (Process process = Process.Start(psInfoMkFifo))
                    {
                        process.WaitForExit();
                    }

                    outputPipeNames_[loop] = fileNameFifo;
                }
#elif UNITY_ANDROID
                string fileName = getDataDir() + "/FfmpegUnity_" + Guid.NewGuid().ToString();

                using (AndroidJavaClass os = new AndroidJavaClass("android.system.Os"))
                {
                    os.CallStatic("mkfifo", fileName, Convert.ToInt32("777", 8));
                }

                outputPipeNames_[loop] = fileName;
#elif UNITY_IOS
                IntPtr hglobalPipe = Marshal.AllocHGlobal(1024);
                ffmpeg_mkpipe(hglobalPipe, 1024);
                string fileNameFifo = Marshal.PtrToStringAuto(hglobalPipe);
                Marshal.FreeHGlobal(hglobalPipe);

                outputPipeNames_[loop] = fileNameFifo;
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
                var pipeOption2 = new FfmpegExecuteIL2CPPWin.PipeOption();
                pipeOption2.BlockSize = BLOCK_SIZE;
                pipeOption2.BufferSize = BUFFER_SIZE;
                pipeOption2.PipeName = "FfmpegUnity_" + Guid.NewGuid().ToString();
                pipeOption2.StdMode = 0;
                command_.PipeOptionsList.Add(pipeOption2);
                outputPipeNames_[loop] = @"\\.\pipe\" + pipeOption2.PipeName;
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                string fileNameFifo = "/tmp/FfmpegUnity_" + Guid.NewGuid().ToString();
                unity_system("mkfifo \"" + fileNameFifo + "\"");
                outputPipeNames_[loop] = fileNameFifo;
#endif

                int streamId = loop;
                var thread = new Thread(() => { read(outputPipeNames_[streamId], streamId); });
                thread.Start();
                threads_.Add(thread);
            }

            for (int loop = 0; loop < outputOptions_.Length; loop++)
            {
                options += " " + outputOptions_[loop] + " \"" + outputPipeNames_[loop] + "\" ";
            }

            return options;
        }

        void streamRead(BinaryReader reader, int streamId)
        {
            while (!isEnd_)
            {
                try
                {
                    var bytes = reader.ReadBytes(BLOCK_SIZE);
                    lock (outputBytes_[streamId])
                    {
                        outputBytes_[streamId].AddRange(bytes);
                    }
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                    continue;
                }
            }
        }

        void read(string pipeFileName, int streamId)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream(pipeFileName.Replace(@"\\.\pipe\", ""),
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                BUFFER_SIZE, BUFFER_SIZE))
            {
                Thread thread = new Thread(() =>
                {
                    while (!isEnd_ && !stream.IsConnected)
                    {
                        Thread.Sleep(1);
                    }
                    if (!stream.IsConnected)
                    {
                        using (var dummyStream = new NamedPipeClientStream(".", pipeFileName.Replace(@"\\.\pipe\", ""), PipeDirection.Out))
                        {
                            dummyStream.Connect();
                        }
                    }
                });
                thread.Start();
                threads_.Add(thread);

                stream.WaitForConnection();

                using (var reader = new BinaryReader(stream))
                {
                    streamRead(reader, streamId);
                }
            }
#elif UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string fileName = pipeFileName;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamRead(reader, streamId);
            }

            File.Delete(fileName);
#elif UNITY_IOS
            string fileName = pipeFileName;

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamRead(reader, streamId);
            }

            ffmpeg_closePipe(fileName);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            while (command_.ExecuteObj == null || command_.ExecuteObj.GetStream(streamId) == null)
            {
                Thread.Sleep(1);
            }

            var stream = command_.ExecuteObj.GetStream(streamId);
            using (var reader = new BinaryReader(stream))
            {
                streamRead(reader, streamId);
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            string fileName = pipeFileName;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamRead(reader, streamId);
            }

            File.Delete(fileName);
#endif
        }

        public byte[] GetOutputBytes(int outputNo = 0)
        {
            if (outputBytes_[outputNo].Count <= 0)
            {
                return null;
            }

            byte[] ret;
            lock (outputBytes_[outputNo])
            {
                ret = outputBytes_[outputNo].ToArray();
                outputBytes_[outputNo].Clear();
            }

            return ret;
        }

        public void Dispose()
        {
            isEnd_ = true;

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

        public interface IOutputControl
        {
            byte[] GetOutputBytes(int outputNo = 0);
            int OutputOptionsCount { get; }
        }
    }
}
