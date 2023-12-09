using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FfmpegUnity
{
    public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        bool isFirst_ = true;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!isFirst_)
            {
                return;
            }
            isFirst_ = false;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX

            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets"))
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }
            if (!AssetDatabase.IsValidFolder("Assets/StreamingAssets/_FfmpegUnity_temp"))
            {
                AssetDatabase.CreateFolder("Assets/StreamingAssets", "_FfmpegUnity_temp");
            }

#if UNITY_STANDALONE_WIN
            string ffmpegPath = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Windows/ffmpeg.exe");
            string ffprobePath = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Windows/ffprobe.exe");
            if (string.IsNullOrEmpty(ffmpegPath) || string.IsNullOrEmpty(ffprobePath))
            {
                throw new BuildFailedException(InitTargetWin.WarningImportFile);
            }

            File.Copy(ffmpegPath,
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffmpeg.exe");
            File.Copy(ffprobePath,
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffprobe.exe");

            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) == ScriptingImplementation.IL2CPP)
            {
                string pipePath = FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Windows/NamedPipeConnecter.exe");
                if (string.IsNullOrEmpty(pipePath))
                {
                    throw new BuildFailedException(InitTargetWin.WarningImportFile);
                }

                File.Copy(pipePath,
                    "Assets/StreamingAssets/_FfmpegUnity_temp/NamedPipeConnecter.exe");
            }
#elif UNITY_STANDALONE_OSX
            File.Copy(FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffmpeg"),
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffmpeg");
            File.Copy(FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffprobe"),
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffprobe");
#elif UNITY_STANDALONE_LINUX
            File.Copy(FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Linux/ffmpeg"),
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffmpeg");
            File.Copy(FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Linux/ffprobe"),
                "Assets/StreamingAssets/_FfmpegUnity_temp/ffprobe");
#endif
            AssetDatabase.Refresh();
#endif

#if UNITY_ANDROID
            var files = Directory.GetFiles("Assets/StreamingAssets", "*", SearchOption.AllDirectories).Where(x => !x.EndsWith(".meta")).ToArray();
            for (int loop = 0; loop < files.Length; loop++)
            {
                files[loop] = files[loop].Replace("Assets/StreamingAssets", "");
            }
            File.WriteAllLines("Assets/StreamingAssets/_FfmpegUnity_files.txt", files);

            AssetDatabase.Refresh();
#endif

            EditorApplication.update += postProcess;
        }

        void postProcess()
        {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            AssetDatabase.DeleteAsset("Assets/StreamingAssets/_FfmpegUnity_temp");
#endif

#if UNITY_ANDROID
            AssetDatabase.DeleteAsset("Assets/StreamingAssets/_FfmpegUnity_files.txt");
#endif

            EditorApplication.update -= postProcess;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            postProcess();
        }
    }
}
