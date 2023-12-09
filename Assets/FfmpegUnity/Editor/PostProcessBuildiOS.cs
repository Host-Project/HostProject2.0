using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace FfmpegUnity
{
    public static class PostProcessBuildiOS
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
#if UNITY_IOS
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject pbxProject = new PBXProject();

            pbxProject.ReadFromString(File.ReadAllText(projectPath));

            string target = pbxProject.GetUnityFrameworkTargetGuid();

            pbxProject.AddFileToBuild(target, pbxProject.AddFile("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
            pbxProject.AddFileToBuild(target, pbxProject.AddFile("usr/lib/libbz2.tbd", "Frameworks/libbz2.tbd", PBXSourceTree.Sdk));
            pbxProject.AddFileToBuild(target, pbxProject.AddFile("System/Library/Frameworks/VideoToolbox.framework", "Frameworks/VideoToolbox.framework", PBXSourceTree.Sdk));

            File.WriteAllText(projectPath, pbxProject.WriteToString());
#endif
        }
    }
}
