using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FfmpegUnity
{
    public class FfmpegPlayerCommand : FfmpegCommand
    {
        public string InputOptions = "";

        public FfmpegPath.DefaultPath DefaultPath = FfmpegPath.DefaultPath.NONE;
        public string InputPath = "";

        public bool AutoStreamSettings = true;
        public FfmpegStream[] Streams = new FfmpegStream[] {
            new FfmpegStream()
            {
                CodecType = FfmpegStream.Type.VIDEO,
                Width = 640,
                Height = 480,
            },
            new FfmpegStream()
            {
                CodecType = FfmpegStream.Type.AUDIO,
                Channels = 2,
                SampleRate = 48000,
            },
        };
        public float FrameRate = 30f;

        public string PlayerOptions = "";

        public FfmpegPlayerVideoTexture[] VideoTextures;
        public AudioSource[] AudioSources;

        public bool SyncFrameRate = true;

        float time_ = 0f;
        bool addDeltaTime_ = false;

        // Current play time of video.
        // If you want to set, call SetTime().
        public float Time
        {
            get
            {
                if (time_ > Duration && Duration > 0f)
                {
                    return Duration;
                }
                return time_;
            }
            private set
            {
                time_ = value;
                addDeltaTime_ = false;
            }
        }

        // Duration of video.
        public float Duration
        {
            get;
            private set;
        } = 0f;
        public bool IsPlaying
        {
            get
            {
                return !isEnd_;
            }
        }

        bool isEnd_ = true;
        List<Thread> threads_ = new List<Thread>();

        Dictionary<int, byte[]> videoBuffers_ = new Dictionary<int, byte[]>();
        Dictionary<int, int> widths_ = new Dictionary<int, int>();
        Dictionary<int, int> heights_ = new Dictionary<int, int>();

        Dictionary<int, List<float>> audioBuffers_ = new Dictionary<int, List<float>>();

        double timeBase_ = 0.0;

        bool isSettingTime_ = false;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || !UNITY_IOS
        string pipeId_;
#else
        List<string> pipeNames_ = new List<string>();
#endif

#if !UNITY_EDITOR_WIN
        string dataDir_;
#endif

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX

#elif UNITY_IOS
        [DllImport("__Internal")]
        static extern IntPtr ffmpeg_ffprobeExecuteAsync(string command);
        [DllImport("__Internal")]
        static extern bool ffmpeg_isRunnning(IntPtr session);
        [DllImport("__Internal")]
        static extern int ffmpeg_getOutputLength(IntPtr session);
        [DllImport("__Internal")]
        static extern void ffmpeg_getOutput(IntPtr session, int startIndex, IntPtr output, int outputLength);
        [DllImport("__Internal")]
        static extern void ffmpeg_mkpipe(IntPtr output, int outputLength);
        [DllImport("__Internal")]
        static extern void ffmpeg_closePipe(string pipeName);
#endif

#if (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
        [DllImport("__Internal")]
        static extern int unity_system(string command);
        [DllImport("__Internal")]
        static extern IntPtr unity_popen(string command, string type);
        [DllImport("__Internal")]
        static extern int unity_pclose(IntPtr stream);
        [DllImport("__Internal")]
        static extern IntPtr unity_fgets(IntPtr s, int n, IntPtr stream);
#endif

        [Serializable]
        public class FfmpegStream
        {
            public enum Type
            {
                VIDEO,
                AUDIO
            }
            public Type CodecType;
            public int Width;
            public int Height;
            public int Channels;
            public int SampleRate;
        }

        // Set play time of video.
        public void SetTime(float val)
        {
            if (!isSettingTime_ && IsRunning)
            {
                isSettingTime_ = true;
                StartCoroutine(setTimeCoroutine(val));
            }
            else
            {
                Time = val;
            }
        }

        IEnumerator setTimeCoroutine(float val)
        {
            StopFfmpeg();

            while (IsRunning || IsPlaying)
            {
                yield return null;
            }

            Time = val;
            StartFfmpeg();

            while (!IsRunning || !IsPlaying)
            {
                yield return null;
            }

            isSettingTime_ = false;
        }

        protected override void Build()
        {
            RunOptions = "";
            if (AutoStreamSettings)
            {
                Streams = new FfmpegStream[0];
                StartCoroutine(allCoroutine());
            }
            else
            {
                StartCoroutine(ResetCoroutine());
            }
        }

        IEnumerator allCoroutine()
        {
            yield return FfprobeCoroutine(FfmpegPath.PathWithDefault(DefaultPath, InputPath));
            yield return ResetCoroutine();
        }

        void getDataDir()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || (!UNITY_ANDROID && !UNITY_EDITOR_WIN)
            dataDir_ = Application.temporaryCachePath;
#elif UNITY_ANDROID && !UNITY_EDITOR_WIN
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaObject info = context.Call<AndroidJavaObject>("getApplicationInfo"))
            {
                dataDir_ = info.Get<string>("dataDir");
            }
#endif
        }

        protected IEnumerator FfprobeCoroutine(string inputPathAll)
        {
            if (Application.streamingAssetsPath.Contains("://") && DefaultPath == FfmpegPath.DefaultPath.STREAMING_ASSETS_PATH)
            {
                yield return StreamingAssetsCopyPath(inputPathAll);
                inputPathAll = PathInStreamingAssetsCopy;
            }

            getDataDir();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string fileName = "ffprobe";
            if (UseBuiltIn)
            {
#if UNITY_EDITOR_WIN
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Windows/ffprobe.exe");
#elif UNITY_EDITOR_OSX
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffprobe");
#elif UNITY_EDITOR_LINUX
                fileName = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Linux/ffprobe");
#elif UNITY_STANDALONE_WIN
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffprobe.exe";
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffprobe";
#endif
            }

            ProcessStartInfo psInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = "-i \"" + inputPathAll + "\" -show_streams",
            };
#elif UNITY_ANDROID
            string outputStr;

            using (AndroidJavaClass ffprobe = new AndroidJavaClass("com.arthenica.ffmpegkit.FFprobeKit"))
            using (AndroidJavaObject ffprobeSession = ffprobe.CallStatic<AndroidJavaObject>("execute", "-i \"" + inputPathAll + "\" -show_streams"))
            {
                outputStr = ffprobeSession.Call<string>("getOutput");
            }
#elif UNITY_IOS
            IntPtr ffprobeSession = ffmpeg_ffprobeExecuteAsync("-i \"" + inputPathAll + "\" -show_streams");

            while (ffmpeg_isRunnning(ffprobeSession))
            {
                yield return null;
            }

            int allocSize = ffmpeg_getOutputLength(ffprobeSession) + 1;
            IntPtr hglobal = Marshal.AllocHGlobal(allocSize);
            ffmpeg_getOutput(ffprobeSession, 0, hglobal, allocSize);
            string outputStr = Marshal.PtrToStringAuto(hglobal);
            Marshal.FreeHGlobal(hglobal);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            var execute = new FfmpegExecuteIL2CPPWin();
            var pipeOption = new FfmpegExecuteIL2CPPWin.PipeOption();
            pipeOption.BlockSize = -1;
            pipeOption.BufferSize = 1024;
            pipeOption.PipeName = "FfmpegUnity_" + Guid.NewGuid().ToString();
            pipeOption.StdMode = 1;
            execute.ExecuteSingle(UseBuiltIn, "ffprobe", "-i \"" + inputPathAll + "\" -show_streams", pipeOption);
            while (execute.GetStream(0) == null)
            {
                yield return null;
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            string fileName = "ffprobe";
            if (UseBuiltIn)
            {
                fileName = Application.streamingAssetsPath + "/_FfmpegUnity_temp/ffprobe";
            }
            IntPtr stdOutFp = unity_popen("\"" + fileName + "\" -i \"" + inputPathAll + "\" -show_streams", "r");
            string outputStr = "";
            IntPtr bufferHandler = Marshal.AllocHGlobal(1024);
            for (; ; )
            {
                IntPtr retPtr = unity_fgets(bufferHandler, 1024, stdOutFp);
                if (retPtr == IntPtr.Zero)
                {
                    break;
                }

                outputStr += Marshal.PtrToStringAuto(bufferHandler);
            }
            Marshal.FreeHGlobal(bufferHandler);
            unity_pclose(stdOutFp);
#endif

            List<FfmpegStream> ffmpegStreams = new List<FfmpegStream>();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            using (Process process = Process.Start(psInfo))
            using (StreamReader reader = process.StandardOutput)
#elif UNITY_ANDROID || UNITY_IOS || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP)
            using (StringReader reader = new StringReader(outputStr))
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            using (StreamReader reader = new StreamReader(execute.GetStream(0)))
#endif
            {
                Thread ffprobeThread = new Thread(() =>
                {
                    bool breakFlag = false;
                    do
                    {
                        string readStr;
                        do
                        {
                            readStr = reader.ReadLine();
                            if (readStr == null)
                            {
                                breakFlag = true;
                            }
                        } while (!breakFlag && readStr.Replace("\r\n", "").Replace("\n", "") != "[STREAM]");

                        if (breakFlag)
                        {
                            break;
                        }

                        FfmpegStream ffmpegStream = new FfmpegStream();

                        bool innerBreakFlag = false;
                        do
                        {
                            readStr = reader.ReadLine().Replace("\r\n", "").Replace("\n", "");
                            if (readStr.StartsWith("codec_type="))
                            {
                                string type = readStr.Substring("codec_type=".Length);
                                if (string.IsNullOrWhiteSpace(type))
                                {
                                    type = reader.ReadLine().Replace("\r\n", "").Replace("\n", "");
                                }
                                ffmpegStream.CodecType = type == "audio" ? FfmpegStream.Type.AUDIO : FfmpegStream.Type.VIDEO;
                            }
                            else if (readStr.StartsWith("width="))
                            {
                                int val;
                                if (!int.TryParse(readStr.Substring("width=".Length), out val))
                                {
                                    if (!int.TryParse(reader.ReadLine().Replace("\r\n", "").Replace("\n", ""), out val))
                                    {
                                    }
                                }
                                ffmpegStream.Width = val;
                            }
                            else if (readStr.StartsWith("height="))
                            {
                                int val;
                                if (!int.TryParse(readStr.Substring("height=".Length), out val))
                                {
                                    if (!int.TryParse(reader.ReadLine().Replace("\r\n", "").Replace("\n", ""), out val))
                                    {
                                    }
                                }
                                ffmpegStream.Height = val;
                            }
                            else if (readStr.StartsWith("channels="))
                            {
                                int val;
                                if (!int.TryParse(readStr.Substring("channels=".Length), out val))
                                {
                                    if (!int.TryParse(reader.ReadLine().Replace("\r\n", "").Replace("\n", ""), out val))
                                    {
                                    }
                                }
                                ffmpegStream.Channels = val;
                            }
                            else if (readStr.StartsWith("duration="))
                            {
                                float val;
                                if (!float.TryParse(readStr.Substring("duration=".Length), out val))
                                {
                                    if (!float.TryParse(reader.ReadLine().Replace("\r\n", "").Replace("\n", ""), out val))
                                    {
                                    }
                                }
                                if (Duration > val || Duration <= 0f)
                                {
                                    Duration = val;
                                }
                            }
                            else if (readStr.StartsWith("sample_rate="))
                            {
                                int val;
                                if (!int.TryParse(readStr.Substring("sample_rate=".Length), out val))
                                {
                                    if (!int.TryParse(reader.ReadLine().Replace("\r\n", "").Replace("\n", ""), out val))
                                    {
                                    }
                                }
                                ffmpegStream.SampleRate = val;
                            }
                            else if (readStr.StartsWith("r_frame_rate="))
                            {
                                if (ffmpegStream.CodecType == FfmpegStream.Type.VIDEO)
                                {
                                    string[] baseStr = readStr.Substring("r_frame_rate=".Length).Split('/');
                                    float numerator, denominator;
                                    if (float.TryParse(baseStr[0].Replace("\r\n", "").Replace("\n", ""), out numerator))
                                    {
                                        if (float.TryParse(baseStr[1].Replace("\r\n", "").Replace("\n", ""), out denominator))
                                        {
                                            timeBase_ = denominator / numerator;
                                        }
                                    }
                                }
                            }
                            else if (readStr == "[/STREAM]")
                            {
                                innerBreakFlag = true;
                            }
                        } while (!innerBreakFlag);

                        ffmpegStreams.Add(ffmpegStream);

                    } while (!breakFlag);

                    reader.ReadToEnd();
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                    process.WaitForExit();
#endif
                });
                ffprobeThread.Start();

                while (ffprobeThread.IsAlive)
                {
                    yield return null;
                }
            }

#if UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            execute.Dispose();
#endif

            Streams = Streams.Concat(ffmpegStreams).ToArray();
        }

        protected IEnumerator ResetCoroutine()
        {
            isEnd_ = false;

            if (timeBase_ <= 0.0 && FrameRate > 0f)
            {
                timeBase_ = 1.0 / FrameRate;
            }

            getDataDir();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || !UNITY_IOS
            pipeId_ = Guid.NewGuid().ToString();
#else
            pipeNames_.Clear();
#endif

            if (string.IsNullOrEmpty(RunOptions))
            {
                if (!SyncFrameRate)
                {
                    RunOptions += " -fflags nobuffer ";
                }

                RunOptions += " -y -dump ";
                if (Time < Duration)
                {
                    RunOptions += " -ss " + Time + " ";
                }
                RunOptions += " " + InputOptions + " -i \"";
                if (Application.streamingAssetsPath.Contains("://") && DefaultPath == FfmpegPath.DefaultPath.STREAMING_ASSETS_PATH)
                {
                    if (string.IsNullOrEmpty(PathInStreamingAssetsCopy))
                    {
                        yield return StreamingAssetsCopyPath(FfmpegPath.PathWithDefault(DefaultPath, InputPath));
                    }
                    RunOptions += PathInStreamingAssetsCopy;
                }
                else
                {
                    RunOptions += FfmpegPath.PathWithDefault(DefaultPath, InputPath);
                }
                RunOptions += "\" " + PlayerOptions + " ";
            }
            else
            {
                if (!SyncFrameRate)
                {
                    RunOptions += " -fflags nobuffer ";
                }

                RunOptions += " -y -dump ";
                if (Time < Duration)
                {
                    RunOptions += " -ss " + Time + " ";
                }
                RunOptions += " " + PlayerOptions + " ";
            }

            int streamCount = 0;
            int videoStreamCount = 0;
            int audioStreamCount = 0;
            foreach (var ffmpegStream in Streams)
            {
                if (ffmpegStream.CodecType == FfmpegStream.Type.VIDEO)
                {
                    if (videoStreamCount >= VideoTextures.Length)
                    {
                        continue;
                    }
                    var videoTexture = VideoTextures[videoStreamCount];

#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
                    RunOptions += " -f rawvideo -pix_fmt rgba " + @"\\.\pipe\FfmpegUnity_" + pipeId_ + "_" + streamCount;
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                    {
                        string fileNameFifo = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamCount;

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

                        RunOptions += " -f rawvideo -pix_fmt rgba \"" + fileNameFifo + "\"";
                    }
#elif UNITY_ANDROID
                    string fileName = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamCount;

                    using (AndroidJavaClass os = new AndroidJavaClass("android.system.Os"))
                    {
                        os.CallStatic("mkfifo", fileName, Convert.ToInt32("777", 8));
                    }

                    RunOptions += " -f rawvideo -pix_fmt rgba \"" + fileName +"\"";
#elif UNITY_IOS
                    IntPtr hglobalPipe = Marshal.AllocHGlobal(1024);
                    ffmpeg_mkpipe(hglobalPipe, 1024);
                    string fileNameFifo = Marshal.PtrToStringAuto(hglobalPipe);
                    Marshal.FreeHGlobal(hglobalPipe);

                    pipeNames_.Add(fileNameFifo);

                    RunOptions += " -frame_drop_threshold 5 -f rawvideo -pix_fmt rgba \"" + fileNameFifo +"\"";
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
                    var pipeOption2 = new FfmpegExecuteIL2CPPWin.PipeOption();
                    pipeOption2.BlockSize = ffmpegStream.Width * ffmpegStream.Height * 4;
                    pipeOption2.BufferSize = ffmpegStream.Width * ffmpegStream.Height * 4;
                    pipeOption2.PipeName = "FfmpegUnity_" + Guid.NewGuid().ToString();
                    pipeOption2.StdMode = 0;
                    PipeOptionsList.Add(pipeOption2);
                    RunOptions += " -f rawvideo -pix_fmt rgba \"" + @"\\.\pipe\" + pipeOption2.PipeName +"\"";
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                    string fileNameFifo = "/tmp/FfmpegUnity_" + pipeId_ + "_" + streamCount;
                    unity_system("mkfifo \"" + fileNameFifo + "\"");
                    RunOptions += " -f rawvideo -pix_fmt rgba \"" + fileNameFifo + "\"";
#endif
                    if (videoTexture.VideoTexture == null)
                    {
                        videoTexture.VideoTexture = new Texture2D(ffmpegStream.Width, ffmpegStream.Height, TextureFormat.RGBA32, false);
                    }
                    else if (videoTexture.VideoTexture is Texture2D && (videoTexture.VideoTexture.width != ffmpegStream.Width || videoTexture.VideoTexture.height != ffmpegStream.Height))
                    {
                        Texture2D oldTexture = (Texture2D)videoTexture.VideoTexture;
                        videoTexture.VideoTexture = new Texture2D(ffmpegStream.Width, ffmpegStream.Height, oldTexture.format, false);
                        Destroy(oldTexture);
                    }

                    widths_[streamCount] = ffmpegStream.Width;
                    heights_[streamCount] = ffmpegStream.Height;

                    int streamId = streamCount;
                    var thread = new Thread(() => { readVideo(streamId); });
                    thread.Start();
                    threads_.Add(thread);

                    videoStreamCount++;
                }
                else
                {
                    if (AudioSources.Length <= audioStreamCount)
                    {
                        continue;
                    }

                    var audioSource = AudioSources[audioStreamCount];

                    if (ffmpegStream.SampleRate != AudioSettings.outputSampleRate)
                    {
                        RunOptions += " -af asetrate=" + ffmpegStream.SampleRate + " -ar " + AudioSettings.outputSampleRate + " ";
                    }
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
                    RunOptions += " -f f32le " + @"\\.\pipe\FfmpegUnity_" + pipeId_ + "_" + streamCount;
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                    {
                        string fileNameFifo = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamCount;

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

                        RunOptions += " -f f32le \"" + fileNameFifo + "\"";
                    }
#elif UNITY_ANDROID
                    string fileName = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamCount;

                    using (AndroidJavaClass os = new AndroidJavaClass("android.system.Os"))
                    {
                        os.CallStatic("mkfifo", fileName, Convert.ToInt32("777", 8));
                    }

                    RunOptions += " -f f32le \"" + fileName + "\"";
#elif UNITY_IOS
                    IntPtr hglobalPipe = Marshal.AllocHGlobal(1024);
                    ffmpeg_mkpipe(hglobalPipe, 1024);
                    string fileNameFifo = Marshal.PtrToStringAuto(hglobalPipe);
                    Marshal.FreeHGlobal(hglobalPipe);

                    pipeNames_.Add(fileNameFifo);

                    RunOptions += " -f f32le \"" + fileNameFifo + "\"";
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
                    var pipeOption2 = new FfmpegExecuteIL2CPPWin.PipeOption();
                    pipeOption2.BlockSize = 1024;
                    pipeOption2.BufferSize = 48000 * 4;
                    pipeOption2.PipeName = "FfmpegUnity_" + Guid.NewGuid().ToString();
                    pipeOption2.StdMode = 0;
                    PipeOptionsList.Add(pipeOption2);
                    RunOptions += " -f f32le \"" + @"\\.\pipe\" + pipeOption2.PipeName + "\"";
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
                    string fileNameFifo = "/tmp/FfmpegUnity_" + pipeId_ + "_" + streamCount;
                    unity_system("mkfifo \"" + fileNameFifo + "\"");
                    RunOptions += " -f f32le \"" + fileNameFifo + "\"";
#endif

                    audioSource.clip = AudioClip.Create("", AudioSettings.outputSampleRate * 2, ffmpegStream.Channels, AudioSettings.outputSampleRate, true);
                    audioSource.loop = true;
                    if (audioSource.playOnAwake)
                    {
                        audioSource.Play();
                    }

                    audioBuffers_[streamCount] = new List<float>();

                    var playerAudio = audioSource.GetComponent<FfmpegPlayerAudio>();
                    if (playerAudio == null)
                    {
                        playerAudio = audioSource.gameObject.AddComponent<FfmpegPlayerAudio>();
                    }
                    playerAudio.StreamId = streamCount;
                    playerAudio.Player = this;

                    int streamId = streamCount;
                    var thread = new Thread(() => { readAudio(streamId); });
                    thread.Start();
                    threads_.Add(thread);

                    audioStreamCount++;
                }

                streamCount++;
            }

            IsGetStdErr = true;
            IsFinishedBuild = true;
            yield return startReadTime();
        }

        protected override void Clean()
        {
            isEnd_ = true;

            foreach (var thread in threads_)
            {
                bool exited = thread.Join(1);
                while (!exited && IsRunning)
                {
                    exited = thread.Join(1);
                }
                if (!exited && !IsRunning)
                {
                    thread.Abort();
                }
            }
            threads_.Clear();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || !UNITY_IOS
            pipeId_ = "";
#endif

#if !UNITY_EDITOR_WIN
            dataDir_ = "";
#endif
        }

        void streamReadVideo(BinaryReader reader, int streamId)
        {
            var stopWatch = new Stopwatch();
            double frameTime = 0.0;
            if (SyncFrameRate)
            {
                stopWatch.Start();
            }

#if !(UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX) && UNITY_ANDROID
            while (!isEnd_)
#else
            while (!isEnd_ && IsRunning)
#endif
            {
                var videoBufferStart = reader.ReadBytes(widths_[streamId] * heights_[streamId] * 4);
                if (videoBufferStart == null)
                {
#if !(UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX) && UNITY_ANDROID
                    while (!isEnd_)
#else
                    while (!isEnd_ && IsRunning)
#endif
                    {
                        stopWatch.Stop();
                        return;
                    }
                    Thread.Sleep(1);
                    continue;
                }
                int pos = videoBufferStart.Length;
                byte[] videoBuffer;
                if (videoBufferStart.Length < widths_[streamId] * heights_[streamId] * 4)
                {
                    videoBuffer = new byte[widths_[streamId] * heights_[streamId] * 4];
                    Array.Copy(videoBufferStart, 0, videoBuffer, 0, videoBufferStart.Length);
                }
                else
                {
                    videoBuffer = videoBufferStart;
                }
                while (pos < widths_[streamId] * heights_[streamId] * 4)
                {
                    var addBuffer = reader.ReadBytes(widths_[streamId] * heights_[streamId] * 4 - pos);
                    if (addBuffer == null)
                    {
#if !(UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX) && UNITY_ANDROID
                        while (!isEnd_)
#else
                        while (!isEnd_ && IsRunning)
#endif
                        {
                            stopWatch.Stop();
                            return;
                        }
                        Thread.Sleep(1);
                        continue;
                    }
                    Array.Copy(addBuffer, 0, videoBuffer, pos, addBuffer.Length);
                    pos += addBuffer.Length;
                }

                var newVideoBuffer = new byte[videoBuffer.Length];
                for (int y = 0; y < heights_[streamId]; y++)
                {
                    Array.Copy(videoBuffer, y * widths_[streamId] * 4,
                        newVideoBuffer, (heights_[streamId] - y - 1) * widths_[streamId] * 4,
                        widths_[streamId] * 4);
                }
                lock (videoBuffers_)
                {
                    videoBuffers_[streamId] = newVideoBuffer;
                }

                if (SyncFrameRate)
                {
                    stopWatch.Stop();
                    double time = stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Start();

                    frameTime += timeBase_;
                    if (time < frameTime)
                    {
                        Thread.Sleep((int)((frameTime - time) * 1000.0));
                    }
                }
            }

            stopWatch.Stop();
        }

        void readVideo(int streamId)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream("FfmpegUnity_" + pipeId_ + "_" + streamId,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                widths_[streamId] * heights_[streamId] * 4, widths_[streamId] * heights_[streamId] * 4))
            {
                Thread thread = new Thread(() =>
                {
                    while (!isEnd_ && !stream.IsConnected)
                    {
                        Thread.Sleep(1);
                    }
                    if (!stream.IsConnected)
                    {
                        using (var dummyStream = new NamedPipeClientStream(".", "FfmpegUnity_" + pipeId_ + "_" + streamId, PipeDirection.Out))
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
                    streamReadVideo(reader, streamId);
                }
            }
#elif UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string fileName = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamId;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadVideo(reader, streamId);
            }

            File.Delete(fileName);
#elif UNITY_IOS
            string fileName = pipeNames_[streamId];

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadVideo(reader, streamId);
            }

            ffmpeg_closePipe(fileName);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            while (ExecuteObj == null || ExecuteObj.GetStream(streamId) == null)
            {
                Thread.Sleep(1);
            }

            using (var reader = new BinaryReader(ExecuteObj.GetStream(streamId)))
            {
                streamReadVideo(reader, streamId);
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            string fileName = "/tmp/FfmpegUnity_" + pipeId_ + "_" + streamId;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadVideo(reader, streamId);
            }

            File.Delete(fileName);
#endif
        }

        void streamReadAudio(BinaryReader reader, int streamId)
        {
            float[] buffer = new float[2048];

#if !(UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX) && UNITY_ANDROID
            while (!isEnd_)
#else
            while (!isEnd_ && IsRunning)
#endif
            {
                try
                {
                    for (int loop = 0; loop < buffer.Length; loop++)
                    {
                        buffer[loop] = reader.ReadSingle();
                    }
                    audioBuffers_[streamId].AddRange(buffer);
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                    continue;
                }
            }
        }

        void readAudio(int streamId)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream("FfmpegUnity_" + pipeId_ + "_" + streamId,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                48000 * 4, 48000 * 4))
            {
                Thread thread = new Thread(() =>
                {
                    while (!isEnd_ && !stream.IsConnected)
                    {
                        Thread.Sleep(1);
                    }
                    if (!stream.IsConnected)
                    {
                        using (var dummyStream = new NamedPipeClientStream(".", "FfmpegUnity_" + pipeId_ + "_" + streamId, PipeDirection.Out))
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
                    streamReadAudio(reader, streamId);
                }
            }
#elif UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
            string fileName = dataDir_ + "/FfmpegUnity_" + pipeId_ + "_" + streamId;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadAudio(reader, streamId);
            }

            File.Delete(fileName);
#elif UNITY_IOS
            string fileName = pipeNames_[streamId];

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadAudio(reader, streamId);
            }

            ffmpeg_closePipe(fileName);
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            while (ExecuteObj == null || ExecuteObj.GetStream(streamId) == null)
            {
                Thread.Sleep(1);
            }

            using (var reader = new BinaryReader(ExecuteObj.GetStream(streamId)))
            {
                streamReadAudio(reader, streamId);
            }
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && ENABLE_IL2CPP
            string fileName = "/tmp/FfmpegUnity_" + pipeId_ + "_" + streamId;

            while (!File.Exists(fileName))
            {
                Thread.Sleep(1);
            }

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                streamReadAudio(reader, streamId);
            }

            File.Delete(fileName);
#endif
        }

        IEnumerator startReadTime()
        {
            string readStr;

            do
            {
                readStr = GetStdErrLine();
                if (readStr == null)
                {
                    yield return null;
                }
                if (isEnd_)
                {
                    yield break;
                }
            } while (readStr == null || !readStr.StartsWith("stream #0:"));

            while (!isEnd_)
            {
                readStr = GetStdErrLine();
                if (readStr == null)
                {
                    yield return null;
                    continue;
                }

                if (!readStr.StartsWith("stream #0:"))
                {
                    continue;
                }

                do
                {
                    readStr = GetStdErrLine();
                    if (readStr == null)
                    {
                        yield return null;
                    }
                    if (isEnd_)
                    {
                        yield break;
                    }
                } while (readStr == null || !readStr.StartsWith("  dts="));

                string timeStr = readStr.Substring("  dts=".Length).Split(new string[] { "  pts=" }, StringSplitOptions.None)[0];
                float time;
                if (float.TryParse(timeStr, out time) && Time < time)
                {
                    Time = time;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (time_ >= Duration && Duration > 0f)
            {
                StopFfmpeg();
                Time = 0f;
                return;
            }

            if (!IsPlaying)
            {
                return;
            }

            if (!addDeltaTime_)
            {
                addDeltaTime_ = true;
            }
            else if (Duration > 0f)
            {
                time_ += UnityEngine.Time.deltaTime;
            }

            if (!IsRunning)
            {
                return;
            }

            lock (videoBuffers_)
            {
                if (videoBuffers_ == null || videoBuffers_.Count <= 0)
                {
                    return;
                }

                int videoLoop = 0;
                foreach (var videoBuffer in videoBuffers_)
                {
                    if (videoBuffer.Value.Length <= 0)
                    {
                        videoLoop++;
                        continue;
                    }

                    Texture2D videoTexture;
                    if (VideoTextures[videoLoop].VideoTexture == null)
                    {
                        continue;
                    }
                    else if (VideoTextures[videoLoop].VideoTexture is RenderTexture)
                    {
                        videoTexture = new Texture2D(widths_[videoBuffer.Key], heights_[videoBuffer.Key], TextureFormat.RGBA32, false);
                    }
                    else
                    {
                        videoTexture = VideoTextures[videoLoop].VideoTexture as Texture2D;
                    }
                    if (videoTexture == null)
                    {
                        continue;
                    }

                    videoTexture.LoadRawTextureData(videoBuffer.Value);
                    videoTexture.Apply();

                    if (VideoTextures[videoLoop].VideoTexture is RenderTexture)
                    {
                        Graphics.Blit(videoTexture, VideoTextures[videoLoop].VideoTexture as RenderTexture);
                        Destroy(videoTexture);
                    }

                    videoLoop++;
                }

                videoBuffers_.Clear();
            }
        }

        public void OnAudioFilterReadFromPlayerAudio(float[] data, int channels, int streamId)
        {
            int length = audioBuffers_[streamId].Count < data.Length ? audioBuffers_[streamId].Count : data.Length;

            if (audioBuffers_[streamId].Count > 48000 * channels)
            {
                int delTempLength = audioBuffers_[streamId].Count - length * 2;
                delTempLength = audioBuffers_[streamId].Count < delTempLength ? audioBuffers_[streamId].Count : delTempLength;
                if (delTempLength > 0)
                {
                    audioBuffers_[streamId].RemoveRange(0, delTempLength);
                }
            }

            for (int loop = 0; loop < length; loop++)
            {
                data[loop] = audioBuffers_[streamId][loop];
            }
            if (length <= audioBuffers_[streamId].Count)
            {
                audioBuffers_[streamId].RemoveRange(0, length);
            }
        }
    }
}
