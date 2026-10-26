using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class P4AndroidBuild
{
    [MenuItem("Cube Invasion/P4/Build Android Development APK")]
    public static void Run()
    {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("Install Android Build Support for Unity 6000.4.8f1.");
        if(PlayerSettings.defaultInterfaceOrientation!=UIOrientation.Portrait)throw new InvalidOperationException("Portrait orientation is required.");
        string directory=Path.Combine(Path.GetTempPath(),"CubeInvasion-P4");Directory.CreateDirectory(directory);
        string path=Path.Combine(directory,"CubeInvasion-P4.apk");
        bool bundle=EditorUserBuildSettings.buildAppBundle;
        try
        {
            EditorUserBuildSettings.buildAppBundle=false;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/CubeInvasion/CubeInvasionPrototype.unity"},target=BuildTarget.Android,
                locationPathName=path,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("P4 Android development build failed: "+report.summary.result);
            Debug.Log("CUBE_P4_ANDROID_BUILD_OK: "+path+" / "+report.summary.totalSize+" bytes");
        }
        finally { EditorUserBuildSettings.buildAppBundle=bundle; }
    }
}
