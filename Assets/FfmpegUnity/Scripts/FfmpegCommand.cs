using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace FfmpegUnity
{
    public class FfmpegCommand : MonoBehaviour
    {
        public bool ExecuteOnStart = true;
        public bool UseBuiltIn = true;
        public string Options = "";
        public bool PrintStdErr = false;

        public bool IsRunning
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                return !loopExit_ && isAlreadyBuild_;
#elif UNITY_ANDROID
                if (ffmpegSession_ == null)
                {
                    return false;
                }
                var state = ffmpegSession_.Call<AndroidJavaObject>("getReturnCode");
                return state == null;
#elif UNITY_IOS
                if (session_ == IntPtr.Zero)
                {
                    return false;
                }
                return ffmpeg_isRunnning(session_);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
                return !loopExit_ && isAlreadyBuild_;  
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                return !isFinished_ && isAlreadyBuild_;
#else
                return false;
#endif
            }
        }

        protected bool IsGetStdErr = false;
        protected string ParsedOptionInStreamingAssetsCopy = "";
        protected string PathInStreamingAssetsCopy = "";

        bool isAlreadyBuild_ = false;
        bool sendQCommand_ = false;
        bool loopExit_ = false;

        List<string> deleteAssets_ = new List<string>();

        List<string> stdErrListForGetLine_ = new List<string>();

        protected bool IsFinishedBuild
        {
            get;
            set;
        } = false;

        protected string RunOptions
        {
            get;
            set;
        } = "";

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
        StreamReader stdErr_ = null;
        Thread stdErrThread_ = null;
        List<string> stdErrList_ = new List<string>();
        Process process_ = null;
#elif UNITY_ANDROID
        AndroidJavaObject ffmpegSession_ = null;
        string[] outputAllLine_ = new string[0];
        int outputAllLinePos_ = 0;
        int stdErrPos_ = 0;
#elif UNITY_IOS
        [DllImport("__Internal")]
        static extern void ffmpeg_setup();
        [DllImport("__Internal")]
        static extern IntPtr ffmpeg_executeAsync(string command);
        [DllImport("__Internal")]
        static extern void ffmpeg_cancel(IntPtr session);
        [DllImport("__Internal")]
        static extern int ffmpeg_getOutputLength(IntPtr session);
        [DllImport("__Internal")]
        static extern void ffmpeg_getOutput(IntPtr session, int startIndex, IntPtr output, int outputLength);
        [DllImport("__Internal")]
        static extern bool ffmpeg_isRunnning(IntPtr session);

        IntPtr session_ = IntPtr.Zero;
        string[] outputAllLine_ = new string[0];
        int outputAllLinePos_ = 0;
        int stdErrPos_ = 0;
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
        StreamReader stdErr_ = null;
        Thread stdErrThread_ = null;
        List<string> stdErrList_ = new List<string>();
        public FfmpegExecuteIL2CPPWin ExecuteObj = null;
        public List<FfmpegExecuteIL2CPPWin.PipeOption> PipeOptionsList = new List<FfmpegExecuteIL2CPPWin.PipeOption>();
        public List<FfmpegExecuteIL2CPPWin.PipeOption> LastPipeOptions = new List<FfmpegExecuteIL2CPPWin.PipeOption>();
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
        [DllImport("__Internal")]
        static extern int unity_system(string command);
        [DllImport("__Internal")]
        static extern IntPtr unity_popen(string command, string type);
        [DllImport("__Internal")]
        static extern int unity_pclose(IntPtr stream);
        [DllImport("__Internal")]
        static extern IntPtr unity_fgets(IntPtr s, int n, IntPtr stream);
        [DllImport("__Internal")]
        static extern void unity_ignoreSignals();
        [DllImport("__Internal")]
        static extern int unity_fputc(int c, IntPtr fp);

        List<string> stdErrStrs_ = new List<string>();
        string tempStr_ = "";
        Thread stdErrThread_ = null;
        bool isFinished_ = false;
#endif

        protected virtual void Build()
        {
            RunOptions = Options;
            IsFinishedBuild = true;
        }

        protected virtual void Clean()
        {

        }

        // Get outputs from stderr.
        public string GetStdErrLine()
        {
            if (stdErrListForGetLine_.Count <= 0)
            {
                return null;
            }
            string ret = stdErrListForGetLine_[0];
            stdErrListForGetLine_.RemoveAt(0);
            return ret;
        }

        string stdErrLine()
        {
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || UNITY_STANDALONE_WIN || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string ret;
            if (stdErrList_ == null)
            {
                return null;
            }
            lock (stdErrList_)
            {
                if (stdErrList_.Count <= 0)
                {
                    return null;
                }
                ret = stdErrList_[0];
                stdErrList_.RemoveAt(0);
            }
            stdErrListForGetLine_.Add(ret);
            return ret;
#elif UNITY_ANDROID
            if (ffmpegSession_ == null)
            {
                return null;
            }
            if (outputAllLine_.Length <= outputAllLinePos_)
            {
                string outputAll = ffmpegSession_.Call<string>("getOutput").Substring(stdErrPos_);
                if (string.IsNullOrWhiteSpace(outputAll))
                {
                    return null;
                }
                outputAllLine_ = outputAll.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                outputAllLinePos_ = 0;
            }
            string ret = outputAllLine_[outputAllLinePos_];
            outputAllLinePos_++;
            if (outputAllLine_.Length == outputAllLinePos_)
            {
                stdErrPos_ += ret.Length;
            }
            else
            {
                stdErrPos_ += ret.Length + Environment.NewLine.Length;
            }
            stdErrListForGetLine_.Add(ret);
            return ret;
#elif UNITY_IOS
            if (session_ == IntPtr.Zero)
            {
                return null;
            }

            if (outputAllLine_.Length <= outputAllLinePos_)
            {
                int allLength = ffmpeg_getOutputLength(session_);
                if (allLength <= stdErrPos_)
                {
                    return null;
                }

                int allocSize = allLength + 1 - stdErrPos_;
                IntPtr hglobal = Marshal.AllocHGlobal(allocSize);

                ffmpeg_getOutput(session_, stdErrPos_, hglobal, allocSize);

                string outputStr = Marshal.PtrToStringAuto(hglobal);
                outputAllLine_ = outputStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                outputAllLinePos_ = 0;

                Marshal.FreeHGlobal(hglobal);
            }

            string ret = outputAllLine_[outputAllLinePos_];
            outputAllLinePos_++;
            if (outputAllLine_.Length == outputAllLinePos_)
            {
                stdErrPos_ += ret.Length;
            }
            else
            {
                stdErrPos_ += ret.Length + Environment.NewLine.Length;
            }
            stdErrListForGetLine_.Add(ret);
            return ret;
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            if (stdErrStrs_.Count <= 0)
            {
                return null;
            }
            string ret = stdErrStrs_[0];
            stdErrStrs_.RemoveAt(0);
            stdErrListForGetLine_.Add(ret);
            return ret;
#else
            return null;
#endif
        }

        IEnumerator Start()
        {
            yield return null;

            if (ExecuteOnStart)
            {
                StartFfmpeg();
            }
        }

        void OnDestroy()
        {
            StopFfmpeg();

            foreach (var file in deleteAssets_)
            {
                File.Delete(file);
            }
            deleteAssets_.Clear();
        }

        // Start ffmpeg commands. (Continuous)
        // If you want stop commands, call StopFfmpeg().
        public void StartFfmpeg()
        {
            StartCoroutine(startFfmpegCoroutine());
        }

        protected IEnumerator StreamingAssetsCopyPath(string path)
        {
            path = path.Replace(Application.streamingAssetsPath, "{STREAMING_ASSETS_PATH}");

            string searchPath = Regex.Replace(path, @"\%[0\#\ \+\-]*[diouxXfeEgGcspn\%]", "*");
            searchPath = Regex.Escape(searchPath).Replace(@"\*", ".*");
            List<string> paths = new List<string>();
            using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/_FfmpegUnity_files.txt"))
            {
                yield return unityWebRequest.SendWebRequest();
                string[] allPaths = unityWebRequest.downloadHandler.text.Replace("\r\n", "\n").Split('\n');
                foreach (string singlePath in allPaths)
                {
                    string addPath = "{STREAMING_ASSETS_PATH}" + singlePath.Replace("\\", "/");
                    if (Regex.IsMatch(addPath, searchPath))
                    {
                        paths.Add(addPath);
                    }
                }
            }

            foreach (var loopPath in paths)
            {
                string streamingAssetPath = loopPath.Replace("{STREAMING_ASSETS_PATH}", Application.streamingAssetsPath);

                string targetItem = loopPath.Replace("{STREAMING_ASSETS_PATH}", Application.temporaryCachePath + "/FfmpegUnity_temp/");

                if (!File.Exists(targetItem))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetItem));

                    byte[] data;
                    using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(streamingAssetPath))
                    {
                        yield return unityWebRequest.SendWebRequest();
                        data = unityWebRequest.downloadHandler.data;
                    }
                    File.WriteAllBytes(targetItem, data);

                    deleteAssets_.Add(targetItem);
                }
            }

            PathInStreamingAssetsCopy = path.Replace("{STREAMING_ASSETS_PATH}", Application.temporaryCachePath + "/FfmpegUnity_temp/");
        }

        void connectSplitStr(List<string> optionsSplit, char targetChar)
        {
            int countChar(string baseStr, char c)
            {
                string s = baseStr.Replace("\\" + c.ToString(), "");
                return s.Length - s.Replace(c.ToString(), "").Length;
            }

            for (int loop = 0; loop < optionsSplit.Count - 1; loop++)
            {
                if (countChar(optionsSplit[loop], targetChar) % 2 == 1)
                {
                    int loop2;
                    for (loop2 = loop + 1;
                        loop2 < optionsSplit.Count - 1 && optionsSplit[loop2] != null && countChar(optionsSplit[loop2], targetChar) % 2 == 1;
                        loop2++)
                    {
                        optionsSplit[loop] += " " + optionsSplit[loop2];
                    }
                    optionsSplit[loop] += " " + optionsSplit[loop2];

                    optionsSplit.RemoveRange(loop + 1, loop2 - loop);
                    optionsSplit[loop] = optionsSplit[loop].Replace("\\" + targetChar.ToString(), "\n")
                        .Replace(targetChar.ToString(), "")
                        .Replace("\n", "\\" + targetChar.ToString());
                }
            }
        }

        protected string[] CommandSplit(string command)
        {
            var optionsSplit = command.Split().ToList();
            connectSplitStr(optionsSplit, '\"');
            connectSplitStr(optionsSplit, '\'');

            return optionsSplit.ToArray();
        }

        protected IEnumerator StreamingAssetsCopyOptions(string options)
        {
            options = options.Replace(Application.streamingAssetsPath, "{STREAMING_ASSETS_PATH}");

            var optionsSplit = CommandSplit(options);

            ParsedOptionInStreamingAssetsCopy = "";
            foreach (var optionItem in optionsSplit)
            {
                if (optionItem != null)
                {
                    string targetItem = optionItem;
                    if (optionItem.Contains("{STREAMING_ASSETS_PATH}"))
                    {
                        yield return StreamingAssetsCopyPath(optionItem);
                        targetItem = PathInStreamingAssetsCopy;
                    }

                    ParsedOptionInStreamingAssetsCopy += " " + targetItem;
                }
            }
        }

        IEnumerator startFfmpegCoroutine()
        {
            if (isAlreadyBuild_)
            {
                yield break;
            }

            isAlreadyBuild_ = true;

            loopExit_ = false;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP) || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

#elif UNITY_ANDROID
            if (ffmpegSession_ != null)
            {
                ffmpegSession_.Dispose();
                ffmpegSession_ = null;
            }

            using (AndroidJavaClass configClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKitConfig"))
            {
                AndroidJavaObject paramVal = new AndroidJavaClass("com.arthenica.ffmpegkit.Signal").GetStatic<AndroidJavaObject>("SIGXCPU");
                configClass.CallStatic("ignoreSignal", paramVal);
            }

            stdErrPos_ = 0;
#elif UNITY_IOS
            if (session_ != IntPtr.Zero)
            {
                ffmpeg_cancel(session_);
                session_ = IntPtr.Zero;
            }

            ffmpeg_setup();
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            if (ExecuteObj != null)
            {
                ExecuteObj.Dispose();
                ExecuteObj = null;
            }
#endif

            IsFinishedBuild = false;
            Build();
            while (!IsFinishedBuild)
            {
                yield return null;
            }

            if (string.IsNullOrWhiteSpace(RunOptions))
            {
                yield break;
            }

            string options = RunOptions.Replace("{PERSISTENT_DATA_PATH}", Application.persistentDataPath)
                .Replace("{TEMPORARY_CACHE_PATH}", Application.temporaryCachePath)
                .Replace("\r\n", "\n").Replace("\\\n", " ").Replace("^\n", " ").Replace("\n", " ");

            UnityEngine.Debug.Log(options);

            if (!Application.streamingAssetsPath.Contains("://"))
            {
                options = options.Replace("{STREAMING_ASSETS_PATH}", Application.streamingAssetsPath);
            }
            else if (options.Contains("{STREAMING_ASSETS_PATH}"))
            {
                yield return StreamingAssetsCopyOptions(options);
                //options = ParsedOptionInStreamingAssetsCopy;
                options = options.Replace("{STREAMING_ASSETS_PATH}", Application.temporaryCachePath + "/FfmpegUnity_temp/");
            }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string fileName = "ffmpeg";
            if (UseBuiltIn)
            {
#if UNITY_EDITOR_WIN
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Windows/ffmpeg.exe");
#elif UNITY_EDITOR_OSX
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffmpeg");
#elif UNITY_EDITOR_LINUX
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Linux/ffmpeg");
#elif UNITY_STANDALONE_WIN
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffmpeg.exe";
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffmpeg";
#endif
            }

            ProcessStartInfo psInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                Arguments = options,
            };

            process_ = Process.Start(psInfo);
            stdErr_ = process_.StandardError;
            stdErrThread_ = new Thread(() =>
            {
                while (!loopExit_)
                {
                    try
                    {
                        Task<string> strTask = stdErr_.ReadLineAsync();
                        while (!strTask.IsCompleted && loopExit_)
                        {
                            Thread.Sleep(16);
                        }
                        if (loopExit_)
                        {
                            break;
                        }
                        string str = strTask.Result;
                        if (str == null)
                        {
                            loopExit_ = true;
                            break;
                        }
                        if (str.StartsWith("Press [q] to stop"))
                        {
                            sendQCommand_ = true;
                        }
                        stdErrList_.Add(str);
                    }
                    catch (Exception)
                    {
                        loopExit_ = true;
                        break;
                    }
                }
            });
            stdErrThread_.Start();
#elif UNITY_ANDROID
            using (AndroidJavaClass ffmpeg = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit"))
            {
                ffmpegSession_ = ffmpeg.CallStatic<AndroidJavaObject>("executeAsync", options);
            }
#elif UNITY_IOS
            session_ = ffmpeg_executeAsync(options);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            ExecuteObj = new FfmpegExecuteIL2CPPWin();
            //if (IsGetStdErr)
            {
                var pipeOption = new FfmpegExecuteIL2CPPWin.PipeOption();
                pipeOption.BlockSize = -1;
                pipeOption.BufferSize = 1024;
                pipeOption.PipeName = "FfmpegUnity_" + Guid.NewGuid().ToString();
                pipeOption.StdMode = 2;
                PipeOptionsList.Add(pipeOption);
            }
            int streamPosId = PipeOptionsList.Count - 1;
            if (LastPipeOptions.Count > 0)
            {
                PipeOptionsList.AddRange(LastPipeOptions);
            }
            ExecuteObj.Execute(UseBuiltIn, "ffmpeg", options, PipeOptionsList.ToArray());
            while (ExecuteObj.GetStream(streamPosId) == null)
            {
                yield return null;
            }
            stdErr_ = new StreamReader(ExecuteObj.GetStream(streamPosId));
            stdErrThread_ = new Thread(() =>
            {
                while (!loopExit_)
                {
                    Task<string> strTask = stdErr_.ReadLineAsync();
                    while (!strTask.IsCompleted && loopExit_)
                    {
                        Thread.Sleep(16);
                    }
                    if (loopExit_)
                    {
                        break;
                    }
                    string str = strTask.Result;
                    if (str == null)
                    {
                        loopExit_ = true;
                        break;
                    }
                    if (str.StartsWith("Press [q] to stop"))
                    {
                        sendQCommand_ = true;
                    }
                    stdErrList_.Add(str);
                }
            });
            stdErrThread_.Start();
#elif UNITY_STANDALONE_OSX && ENABLE_IL2CPP
            unity_ignoreSignals();
            isFinished_ = false;

            string fileName = "ffmpeg";
            if (UseBuiltIn)
            {
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffmpeg";
            }
            stdErrThread_ = new Thread(() =>
            {
                IntPtr stdErrFp = unity_popen("\"" + fileName + "\" " + options + " 2>&1 >/dev/null", "r+");
                IntPtr stdErrBufferHandler = Marshal.AllocHGlobal(1024);

                while (!loopExit_)
                {
                    IntPtr retPtr = unity_fgets(stdErrBufferHandler, 1024, stdErrFp);
                    if (retPtr == IntPtr.Zero)
                    {
                        loopExit_ = true;
                        break;
                    }

                    string outputStr = tempStr_ + Marshal.PtrToStringAuto(stdErrBufferHandler);
                    if (outputStr.EndsWith("\n"))
                    {
                        stdErrStrs_.Add(outputStr);
                        if (outputStr.StartsWith("Press [q] to stop"))
                        {
                            sendQCommand_ = true;
                        }
                        tempStr_ = "";
                    }
                    else
                    {
                        tempStr_ = outputStr;
                    }
                }

                if (sendQCommand_)
                {
                    unity_fputc((int)'q', stdErrFp);
                }

                IntPtr waitPtr;
                do
                {
                    waitPtr = unity_fgets(stdErrBufferHandler, 1024, stdErrFp);
                } while (waitPtr != IntPtr.Zero);

                Marshal.FreeHGlobal(stdErrBufferHandler);
                unity_pclose(stdErrFp);

                isFinished_ = true;
            });
            stdErrThread_.Start();
#elif UNITY_STANDALONE_LINUX && ENABLE_IL2CPP
            unity_ignoreSignals();
            isFinished_ = false;

            string fileName = "ffmpeg";
            if (UseBuiltIn)
            {
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffmpeg";
            }
            stdErrThread_ = new Thread(() =>
            {
                IntPtr stdErrFp = unity_popen("\"" + fileName + "\" " + options + " 2>&1 >/dev/null", "r");
                IntPtr stdErrBufferHandler = Marshal.AllocHGlobal(1024);

                while (!loopExit_)
                {
                    IntPtr retPtr = unity_fgets(stdErrBufferHandler, 1024, stdErrFp);
                    if (retPtr == IntPtr.Zero)
                    {
                        loopExit_ = true;
                        break;
                    }

                    string outputStr = tempStr_ + Marshal.PtrToStringAuto(stdErrBufferHandler);
                    if (outputStr.EndsWith("\n"))
                    {
                        stdErrStrs_.Add(outputStr);
                        if (outputStr.StartsWith("Press [q] to stop"))
                        {
                            sendQCommand_ = true;
                        }
                        tempStr_ = "";
                    }
                    else
                    {
                        tempStr_ = outputStr;
                    }
                }

                unity_pclose(stdErrFp);

                Marshal.FreeHGlobal(stdErrBufferHandler);

                isFinished_ = true;
            });
            stdErrThread_.Start();
#endif
        }

        // Stop ffmpeg commands.
        public void StopFfmpeg()
        {
            loopExit_ = true;

            if (isAlreadyBuild_)
            {
                Clean();
            }

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            if (process_ != null)
            {
                if (!process_.HasExited && sendQCommand_)
                {
                    process_.StandardInput.Write("q");
                    process_.WaitForExit(1000);
                }
                if (!process_.HasExited)
                {
                    process_.CloseMainWindow();
                    process_.WaitForExit();
                }
                process_.Dispose();
                process_ = null;
            }

            if (stdErrThread_ != null)
            {
                stdErrThread_.Join();
                stdErrThread_ = null;
            }

            if (stdErr_ != null)
            {
                stdErr_.Dispose();
                stdErr_ = null;
            }
#elif UNITY_ANDROID
            if (ffmpegSession_ != null)
            {
                ffmpegSession_.Call("cancel");
                ffmpegSession_.Dispose();
                ffmpegSession_ = null;
            }
#elif UNITY_IOS
            if (session_ != IntPtr.Zero)
            {
                if (IsRunning)
                {
                    ffmpeg_cancel(session_);
                    while (IsRunning)
                    {
                        Thread.Sleep(1);
                    }
                }
                session_ = IntPtr.Zero;
            }
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            if (ExecuteObj != null)
            {
                ExecuteObj.Dispose();
                ExecuteObj = null;
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            if (stdErrThread_ != null)
            {
                stdErrThread_.Join();
                stdErrThread_ = null;
            }
#endif

            PathInStreamingAssetsCopy = "";

            sendQCommand_ = false;

            isAlreadyBuild_ = false;
        }

        // Execute ffmpeg. (Once)
        // If you want to use StopFfmpeg(), call StartFfmpeg() instead.
        public void ExecuteFfmpeg()
        {
            StartFfmpeg();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            process_.WaitForExit();
            process_.Dispose();
            process_ = null;
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            if (stdErrThread_ != null)
            {
                loopExit_ = true;
                stdErrThread_.Join();
                stdErrThread_ = null;
            }
#endif
        }

        protected virtual void Update()
        {
            string stdErrLoopResult;
            do
            {
                stdErrLoopResult = stdErrLine();
                if (PrintStdErr && stdErrLoopResult != null)
                {
                    UnityEngine.Debug.Log(stdErrLoopResult);
                }
            } while (stdErrLoopResult != null);
        }
    }
}
