using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Playgama WebGL release: only the WebGL player settings change; Android/iOS settings are untouched.
public static class PlaygamaWebBuild
{
    public const string Version="1.2";
    const string Scene="Assets/CubeInvasion/CubeInvasionPrototype.unity";
    static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"..","Builds","Playgama"));
    public static string Output=>Path.Combine(Root,"WebGL");
    public static string Archive=>Path.Combine(Root,$"CubeInvasion_Playgama_v{Version}.zip");

    [MenuItem("Cube Invasion/Playgama/Configure WebGL")]
    public static void Configure()
    {
        var web=NamedBuildTarget.WebGL;
        PlayerSettings.WebGL.template="PROJECT:Bridge";
        // No engine splash: the game opens straight into its menu (Unity 6 allows this on every license tier).
        PlayerSettings.SplashScreen.show=false;PlayerSettings.SplashScreen.showUnityLogo=false;
        // Playgama 5.1.1: decompression fallback on. Gzip keeps the JS fallback fast on hosts without encoding headers.
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true;
        // Off: Unity's asset cache writes to IndexedDB, and Playgama 3.2.1 allows no browser storage besides Bridge.
        PlayerSettings.WebGL.dataCaching=false;
        PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.showDiagnostics=false;
        PlayerSettings.WebGL.nameFilesAsHashes=false;
        PlayerSettings.WebGL.initialMemorySize=64;
        PlayerSettings.WebGL.maximumMemorySize=1024;
        PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.powerPreference=WebGLPowerPreference.HighPerformance;
        PlayerSettings.SetManagedStrippingLevel(web,ManagedStrippingLevel.Medium);
        PlayerSettings.SetIl2CppCodeGeneration(web,Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetIl2CppCompilerConfiguration(web,Il2CppCompilerConfiguration.Master);
        CapWebTextures();
        AssetDatabase.SaveAssets();
        Debug.Log("CUBE_PLAYGAMA_CONFIGURE_OK");
    }

    // Oversized uncompressed UI art was 30-50% of the download. WebGL-only caps; the sprites scale, layouts are unchanged.
    static readonly (string path,int size)[] WebTextureCaps=
    {
        ("Assets/CubeInvasion/Art/UI/CasualButton.png",512),
        ("Assets/CubeInvasion/Art/UI/Reference/Coin.png",256),("Assets/CubeInvasion/Art/UI/Reference/Arsenal.png",256),
        ("Assets/CubeInvasion/Art/UI/Reference/Settings.png",256),("Assets/CubeInvasion/Art/UI/Reference/Pilot.png",256),
        ("Assets/CubeInvasion/Art/UI/Reference/Journey.png",256)
    };
    static void CapWebTextures()
    {
        foreach(var (path,size) in WebTextureCaps)
        {
            if(!(AssetImporter.GetAtPath(path) is TextureImporter importer))throw new FileNotFoundException("Missing web texture: "+path);
            var web=importer.GetPlatformTextureSettings("WebGL");
            if(web.overridden&&web.maxTextureSize==size)continue;
            web.overridden=true;web.maxTextureSize=size;web.format=TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(web);importer.SaveAndReimport();
        }
    }

    [MenuItem("Cube Invasion/Playgama/Build WebGL + ZIP")]
    public static void Build()
    {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))
            throw new InvalidOperationException("WebGL Build Support is not installed for this editor.");
        Configure();
        if(Directory.Exists(Output))Directory.Delete(Output,true);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes=new[]{Scene},locationPathName=Output,target=BuildTarget.WebGL,targetGroup=BuildTargetGroup.WebGL,options=BuildOptions.None
        });
        var summary=report.summary;
        Debug.Log($"CUBE_PLAYGAMA_BUILD {summary.result} size={summary.totalSize} errors={summary.totalErrors} warnings={summary.totalWarnings} time={summary.totalTime}");
        if(summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("WebGL build failed: "+summary.result);
        Package();
    }

    // Diagnostics only: full stack traces in the browser console; never packaged.
    public static void BuildDevelopment()
    {
        Configure();
        string output=Path.Combine(Root,"WebGL-dev");
        if(Directory.Exists(output))Directory.Delete(output,true);
        PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.FullWithStacktrace;
        try
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{Scene},locationPathName=output,target=BuildTarget.WebGL,targetGroup=BuildTargetGroup.WebGL,options=BuildOptions.Development});
            Debug.Log($"CUBE_PLAYGAMA_DEV_BUILD {report.summary.result}");
        }
        finally { PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;AssetDatabase.SaveAssets(); }
    }

    [MenuItem("Cube Invasion/Playgama/Package ZIP")]
    public static void Package()
    {
        foreach(var required in new[]{"index.html","playgama-bridge-config.json","playgama-bridge.js","playgama-bridge-unity.js"})
            if(!File.Exists(Path.Combine(Output,required)))throw new FileNotFoundException("Build is missing "+required);
        var bad=Directory.GetFiles(Output,"*",SearchOption.AllDirectories).Select(f=>f.Substring(Output.Length+1)).Where(f=>f.Any(c=>c>127)).ToArray();
        if(bad.Length>0)throw new InvalidOperationException("Non-Latin file names: "+string.Join(", ",bad));
        if(File.Exists(Archive))File.Delete(Archive);
        // index.html sits at the archive root (Playgama 1.3.2).
        ZipFile.CreateFromDirectory(Output,Archive,System.IO.Compression.CompressionLevel.Optimal,false);
        Debug.Log($"CUBE_PLAYGAMA_ZIP_OK {Archive} bytes={new FileInfo(Archive).Length}");
    }
}
