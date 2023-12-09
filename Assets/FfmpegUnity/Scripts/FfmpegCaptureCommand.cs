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
    public class FfmpegCaptureCommand : FfmpegCommand
    {
        [Serializable]
        public class CaptureSource
        {
            public enum SourceType
            {
                Video_GameView,
                Video_Camera,
                Video_RenderTexture,
                Audio_AudioListener,
                Audio_AudioSource,
            }

            public SourceType Type = SourceType.Video_GameView;
            public int Width = -1;
            public int Height = 480;
            public int FrameRate = 30;
            public Camera SourceCamera = null;
            public RenderTexture SourceRenderTexture = null;
            public AudioSource SourceAudio = null;
        }

        public CaptureSource[] CaptureSources = new CaptureSource[]
        {
            new CaptureSource()
            {
                Type = CaptureSource.SourceType.Video_GameView,
            },
            new CaptureSource()
            {
                Type = CaptureSource.SourceType.Audio_AudioListener,
            },
        };

        public string CaptureOptions = "";

        Dictionary<int, Texture2D> tempTextures_ = new Dictionary<int, Texture2D>();
        Dictionary<int, byte[]> videoBuffers_ = new Dictionary<int, byte[]>();

        Dictionary<int, List<float>> audioBuffers_ = new Dictionary<int, List<float>>();
        Dictionary<int, int> audioChannels_ = new Dictionary<int, int>();

        bool isEnd_ = false;
        List<Thread> threads_ = new List<Thread>();

        FfmpegCaptureAudioListener captureAudioListener_ = null;

        bool isStop_ = false;

        Dictionary<int, RenderTexture> tempRenderTextures_ = new Dictionary<int, RenderTexture>();
        Dictionary<int, bool> reverse_ = new Dictionary<int, bool>();

        Shader flipShader_;
        Material flipMaterial_;

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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        int pipeOpenedCount_ = 0;
#endif

        protected override void Build()
        {
            StartCoroutine(captureCoroutine());
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

            foreach (RenderTexture tex in tempRenderTextures_.Values)
            {
                Destroy(tex);
            }
            tempRenderTextures_.Clear();

            foreach (Texture2D tex in tempTextures_.Values)
            {
                Destroy(tex);
            }
            tempTextures_.Clear();

            videoBuffers_.Clear();
            audioBuffers_.Clear();

            if (captureAudioListener_ != null)
            {
                Destroy(captureAudioListener_.gameObject);
                captureAudioListener_ = null;
            }

            isEnd_ = false;
        }

        protected virtual void ByteStart()
        {

        }

        IEnumerator captureCoroutine()
        {
            flipShader_ = Resources.Load<Shader>("FfmpegUnity/Shaders/FlipShader");
            flipMaterial_ = new Material(flipShader_);

            FindObjectOfType<AudioListener>().velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;

            RunOptions = " -y -re ";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            pipeOpenedCount_ = 0;
#endif

            for (int captureLoop = 0; captureLoop < CaptureSources.Length; captureLoop++)
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                string fileName = @"\\.\pipe\FfmpegUnity_" + Guid.NewGuid().ToString();
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP)
                string fileName = Application.temporaryCachePath + "/FfmpegUnity_" + Guid.NewGuid().ToString();
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
                string fileName = "/tmp/FfmpegUnity_" + Guid.NewGuid().ToString();
                unity_system("mkfifo \"" + fileName + "\"");
#elif UNITY_ANDROID
                string dataDir;

                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaObject info = context.Call<AndroidJavaObject>("getApplicationInfo"))
                {
                    dataDir = info.Get<string>("dataDir");
                }

                string fileName = dataDir + "/FfmpegUnity_" + Guid.NewGuid().ToString();

                using (AndroidJavaClass os = new AndroidJavaClass("android.system.Os"))
                {
                    os.CallStatic("mkfifo", fileName, Convert.ToInt32("777", 8));
                }
#elif UNITY_IOS
                IntPtr hglobalPipe = Marshal.AllocHGlobal(1024);
                ffmpeg_mkpipe(hglobalPipe, 1024);
                string fileName = Marshal.PtrToStringAuto(hglobalPipe);
                Marshal.FreeHGlobal(hglobalPipe);
#endif

                switch (CaptureSources[captureLoop].Type)
                {
                    case CaptureSource.SourceType.Video_GameView:
                        {
#if UNITY_EDITOR_LINUX || (UNITY_STANDALONE_LINUX && !(UNITY_EDITOR_WIN || UNITY_EDITOR_OSX))
                            reverse_[captureLoop] = true;
#else
                            reverse_[captureLoop] = false;
#endif

                            int width;
                            int height;
                            if (CaptureSources[captureLoop].Width <= 0 && CaptureSources[captureLoop].Height <= 0)
                            {
                                width = Screen.width;
                                height = Screen.height;
                            }
                            else if (CaptureSources[captureLoop].Width <= 0)
                            {
                                width = Screen.width * CaptureSources[captureLoop].Height / Screen.height;
                                height = CaptureSources[captureLoop].Height;
                            }
                            else if (CaptureSources[captureLoop].Height <= 0)
                            {
                                width = CaptureSources[captureLoop].Width;
                                height = Screen.height * CaptureSources[captureLoop].Width / Screen.width;
                            }
                            else
                            {
                                width = CaptureSources[captureLoop].Width;
                                height = CaptureSources[captureLoop].Height;
                            }
                            tempTextures_[captureLoop] = new Texture2D(width, height, TextureFormat.RGBA32, false);

                            var captureId = captureLoop;
                            var thread = new Thread(() => { writeVideo(captureId, fileName, width, height); });
                            thread.Start();
                            threads_.Add(thread);

                            RunOptions += " -r " + CaptureSources[captureLoop].FrameRate + " -f rawvideo -s " + width + "x" + height + " -pix_fmt rgba -i \"" + fileName + "\" ";
                        }
                        break;
                    case CaptureSource.SourceType.Video_Camera:
                        {
                            reverse_[captureLoop] = true;

                            int baseWidth = (int)(Screen.width * CaptureSources[captureLoop].SourceCamera.rect.width);
                            int baseHeight = (int)(Screen.height * CaptureSources[captureLoop].SourceCamera.rect.height);
                            int width;
                            int height;
                            if (CaptureSources[captureLoop].Width <= 0 && CaptureSources[captureLoop].Height <= 0)
                            {
                                width = baseWidth;
                                height = baseHeight;
                            }
                            else if (CaptureSources[captureLoop].Width <= 0)
                            {
                                width = baseWidth * CaptureSources[captureLoop].Height / baseHeight;
                                height = CaptureSources[captureLoop].Height;
                            }
                            else if (CaptureSources[captureLoop].Height <= 0)
                            {
                                width = CaptureSources[captureLoop].Width;
                                height = baseHeight * CaptureSources[captureLoop].Width / baseWidth;
                            }
                            else
                            {
                                width = CaptureSources[captureLoop].Width;
                                height = CaptureSources[captureLoop].Height;
                            }
                            tempRenderTextures_[captureLoop] = new RenderTexture(width, height, 16);
                            tempTextures_[captureLoop] = new Texture2D(width, height, TextureFormat.RGBA32, false);

                            var captureId = captureLoop;
                            var thread = new Thread(() => { writeVideo(captureId, fileName, width, height); });
                            thread.Start();
                            threads_.Add(thread);

                            RunOptions += " -r " + CaptureSources[captureLoop].FrameRate + " -f rawvideo -s " + width + "x" + height + " -pix_fmt rgba -i \"" + fileName + "\" ";
                        }
                        break;
                    case CaptureSource.SourceType.Video_RenderTexture:
                        {
                            reverse_[captureLoop] = true;

                            RenderTexture renderTexture = CaptureSources[captureLoop].SourceRenderTexture;
                            int width = renderTexture.width;
                            int height = renderTexture.height;
                            tempTextures_[captureLoop] = new Texture2D(width, height, TextureFormat.RGBA32, false);

                            var captureId = captureLoop;
                            var thread = new Thread(() => { writeVideo(captureId, fileName, width, height); });
                            thread.Start();
                            threads_.Add(thread);

                            RunOptions += " -r " + CaptureSources[captureLoop].FrameRate + " -f rawvideo -s " + width + "x" + height + " -pix_fmt rgba -i \"" + fileName + "\" ";
                        }
                        break;
                    case CaptureSource.SourceType.Audio_AudioListener:
                        {
                            var channelMode = AudioSettings.GetConfiguration().speakerMode;
                            int channels = 1;
                            switch (channelMode)
                            {
                                case AudioSpeakerMode.Mono:
                                    channels = 1;
                                    break;
                                case AudioSpeakerMode.Stereo:
                                case AudioSpeakerMode.Prologic:
                                    channels = 2;
                                    break;
                                case AudioSpeakerMode.Quad:
                                    channels = 4;
                                    break;
                                case AudioSpeakerMode.Surround:
                                    channels = 5;
                                    break;
                                case AudioSpeakerMode.Mode5point1:
                                    channels = 6;
                                    break;
                                case AudioSpeakerMode.Mode7point1:
                                    channels = 8;
                                    break;
                            }

                            GameObject captureAudioListenerGameObj = new GameObject();
                            var audioSource = captureAudioListenerGameObj.AddComponent<AudioSource>();
                            audioSource.clip = AudioClip.Create("", AudioSettings.outputSampleRate, channels, AudioSettings.outputSampleRate, true);
                            audioSource.loop = true;

                            captureAudioListener_ = captureAudioListenerGameObj.AddComponent<FfmpegCaptureAudioListener>();
                            captureAudioListener_.StreamId = captureLoop;
                            captureAudioListener_.Channels = channels;

                            audioSource.Play();

                            var captureId = captureLoop;
                            var thread = new Thread(() => { writeAudio(captureId, fileName); });
                            thread.Start();
                            threads_.Add(thread);

                            RunOptions += " -f f32le -ar " + AudioSettings.outputSampleRate + " -ac " + channels + " -i \"" + fileName + "\" ";
                        }
                        break;
                    case CaptureSource.SourceType.Audio_AudioSource:
                        {
                            var captureAudio = CaptureSources[captureLoop].SourceAudio.gameObject.AddComponent<FfmpegCaptureAudio>();
                            captureAudio.StreamId = captureLoop;
                            captureAudio.Capture = this;

                            while (!audioChannels_.ContainsKey(captureLoop))
                            {
                                yield return null;
                            }

                            var captureId = captureLoop;
                            var thread = new Thread(() => { writeAudio(captureId, fileName); });
                            thread.Start();
                            threads_.Add(thread);

                            RunOptions += " -f f32le -ar " + AudioSettings.outputSampleRate + " -ac " + audioChannels_[captureLoop] + " -i \"" + fileName + "\" ";
                        }
                        break;
                }
            }

            RunOptions += " " + CaptureOptions;

            ByteStart();

#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            while (pipeOpenedCount_ < CaptureSources.Length)
            {
                yield return null;
            }
#endif

            IsFinishedBuild = true;

            while (!isEnd_ && !isStop_)
            {
                yield return new WaitForEndOfFrame();

                for (int captureLoop = 0; captureLoop < CaptureSources.Length; captureLoop++)
                {
                    if (CaptureSources[captureLoop].Type <= CaptureSource.SourceType.Video_RenderTexture)
                    {
                        if (!tempTextures_.ContainsKey(captureLoop))
                        {
                            continue;
                        }

                        RenderTexture srcTexture = null;
                        switch (CaptureSources[captureLoop].Type)
                        {
                            case CaptureSource.SourceType.Video_GameView:
                                {
                                    RenderTexture screenTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
                                    ScreenCapture.CaptureScreenshotIntoRenderTexture(screenTexture);
                                    srcTexture = RenderTexture.GetTemporary(tempTextures_[captureLoop].width, tempTextures_[captureLoop].height);
                                    Graphics.Blit(screenTexture, srcTexture);
                                    RenderTexture.ReleaseTemporary(screenTexture);
                                }
                                break;
                            case CaptureSource.SourceType.Video_Camera:
                                {
                                    var camera = CaptureSources[captureLoop].SourceCamera;

                                    srcTexture = tempRenderTextures_[captureLoop];
                                    var tempTexture = camera.targetTexture;

                                    camera.targetTexture = srcTexture;

                                    camera.Render();

                                    camera.targetTexture = tempTexture;
                                }
                                break;
                            case CaptureSource.SourceType.Video_RenderTexture:
                                {
                                    srcTexture = CaptureSources[captureLoop].SourceRenderTexture;
                                    if (srcTexture == null)
                                    {
                                        UnityEngine.Debug.LogError("Error: SourceRenderTexture is not set.");
                                        yield break;
                                    }
                                }
                                break;
                        }

                        RenderTexture filpedTexture = srcTexture;
                        if (reverse_[captureLoop])
                        {
                            filpedTexture = RenderTexture.GetTemporary(srcTexture.width, srcTexture.height);
                            Graphics.Blit(srcTexture, filpedTexture, flipMaterial_);
                        }

                        var tempTextureActive = RenderTexture.active;

                        RenderTexture.active = filpedTexture;

                        tempTextures_[captureLoop].ReadPixels(new Rect(0, 0, filpedTexture.width, filpedTexture.height), 0, 0);
                        tempTextures_[captureLoop].Apply();

                        RenderTexture.active = tempTextureActive;

                        if (reverse_[captureLoop])
                        {
                            RenderTexture.ReleaseTemporary(filpedTexture);
                        }

                        var textureData = tempTextures_[captureLoop].GetRawTextureData<byte>().ToArray();
                        byte[] bufferData = new byte[tempTextures_[captureLoop].width * tempTextures_[captureLoop].height * 4];

                        Array.Copy(textureData, 0, bufferData, 0, bufferData.Length);
                        lock (videoBuffers_)
                        {
                            videoBuffers_[captureLoop] = bufferData;
                        }

                        if (CaptureSources[captureLoop].Type == CaptureSource.SourceType.Video_GameView)
                        {
                            RenderTexture.ReleaseTemporary(srcTexture);
                        }
                    }
                }
            }

            isStop_ = false;
        }

        protected override void Update()
        {
            base.Update();

            if (isStop_)
            {
                StopFfmpeg();
                return;
            }

            if (captureAudioListener_ == null)
            {
                return;
            }
            if (!IsFinishedBuild || audioBuffers_ == null)
            {
                captureAudioListener_.ReadCount = 0;
                return;
            }

            if (!audioBuffers_.ContainsKey(captureAudioListener_.StreamId))
            {
                audioBuffers_[captureAudioListener_.StreamId] = new List<float>();
            }
            audioBuffers_[captureAudioListener_.StreamId].AddRange(captureAudioListener_.Read());
        }

        void streamWriteVideo(BinaryWriter writer, int streamId)
        {
            while (!isEnd_)
            {
                if (!videoBuffers_.ContainsKey(streamId))
                {
                    Thread.Sleep(1);
                    continue;
                }

                byte[] buffer;
                lock (videoBuffers_)
                {
                    buffer = videoBuffers_[streamId];
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

        void writeVideo(int streamId, string pipeFileName, int width, int height)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream(pipeFileName.Replace(@"\\.\pipe\", ""),
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                width * height * 4,
                width * height * 4))
            {
                pipeOpenedCount_++;

                stream.WaitForConnection();

                try
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        streamWriteVideo(writer, streamId);
                    }
                }
                catch (IOException)
                {
                    isStop_ = true;
                }
            }
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            var pipeOption = new FfmpegExecuteIL2CPPWin.PipeOption();
            pipeOption.BlockSize = width * height * 4;
            pipeOption.BufferSize = width * height * 4;
            pipeOption.PipeName = pipeFileName.Replace(@"\\.\pipe\", "");
            pipeOption.StdMode = 3;
            PipeOptionsList.Add(pipeOption);

            pipeOpenedCount_++;

            while (ExecuteObj == null || ExecuteObj.GetStream(streamId) == null)
            {
                Thread.Sleep(1);
            }

            try
            {
                using (var writer = new BinaryWriter(ExecuteObj.GetStream(streamId)))
                {
                    streamWriteVideo(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP) || UNITY_ANDROID || UNITY_IOS
            try
            {
                using (var stream = File.OpenWrite(pipeFileName))
                using (var writer = new BinaryWriter(stream))
                {
                    streamWriteVideo(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
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
                    streamWriteVideo(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
#endif
        }

        void streamWriteAudio(BinaryWriter writer, int streamId)
        {
            while (!isEnd_)
            {
                if (!audioBuffers_.ContainsKey(streamId) || audioBuffers_[streamId] == null || audioBuffers_[streamId].Count <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                int loopLength = audioBuffers_[streamId].Count;
                for (int loop = 0; loop < loopLength; loop++)
                {
                    if (writer.BaseStream.CanWrite)
                    {
                        writer.Write(audioBuffers_[streamId][loop]);
                    }
                }

                audioBuffers_[streamId].RemoveRange(0, loopLength);
            }
        }

        void writeAudio(int streamId, string pipeFileName)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !ENABLE_IL2CPP)
            using (var stream = new NamedPipeServerStream(pipeFileName.Replace(@"\\.\pipe\", ""),
                PipeDirection.Out,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.WriteThrough,
                48000 * 4, 48000 * 4))
            {
                pipeOpenedCount_++;

                stream.WaitForConnection();

                try
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        streamWriteAudio(writer, streamId);
                    }
                }
                catch (IOException)
                {
                    isStop_ = true;
                }
            }
#elif UNITY_STANDALONE_WIN && ENABLE_IL2CPP
            var pipeOption = new FfmpegExecuteIL2CPPWin.PipeOption();
            pipeOption.BlockSize = 1024;
            pipeOption.BufferSize = 48000 * 4;
            pipeOption.PipeName = pipeFileName.Replace(@"\\.\pipe\", "");
            pipeOption.StdMode = 3;
            PipeOptionsList.Add(pipeOption);

            pipeOpenedCount_++;

            while (ExecuteObj == null || ExecuteObj.GetStream(streamId) == null)
            {
                Thread.Sleep(1);
            }

            try
            {
                using (var writer = new BinaryWriter(ExecuteObj.GetStream(streamId)))
                {
                    streamWriteAudio(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX || ((UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX) && !ENABLE_IL2CPP) || UNITY_ANDROID || UNITY_IOS
            try
            {
                using (var stream = File.OpenWrite(pipeFileName))
                using (var writer = new BinaryWriter(stream))
                {
                    streamWriteAudio(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
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
                    streamWriteAudio(writer, streamId);
                }
            }
            catch (IOException)
            {
                isStop_ = true;
            }

            File.Delete(pipeFileName);
#endif
        }

        public void OnAudioFilterWriteToCaptureAudio(float[] data, int channels, int streamId)
        {
            audioChannels_[streamId] = channels;

            if (!audioBuffers_.ContainsKey(streamId))
            {
                audioBuffers_[streamId] = new List<float>();
            }
            audioBuffers_[streamId].AddRange(data);
        }
    }
}
