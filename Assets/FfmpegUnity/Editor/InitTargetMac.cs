using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace FfmpegUnity
{
    public class InitTargetMac : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && UNITY_EDITOR)
            string[] commands = new[]
            {
                FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffmpeg"),
                FfmpegFileManager.GetManagedFilePath(Application.dataPath + "/FfmpegUnity/Bin/Mac/ffprobe")
            };

            foreach (var command in commands)
            {
                ProcessStartInfo psInfo = new ProcessStartInfo()
                {
                    FileName = "chmod",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = "a+x \"" + command + "\"",
                };
                using (Process process = Process.Start(psInfo))
                {
                    process.WaitForExit();
                }
            }
#endif
        }
    }
}
