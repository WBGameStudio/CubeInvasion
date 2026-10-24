using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;

public static class PrototypeBuilder
{
    [MenuItem("Cube Invasion/Build P0 Playground")]
    public static void Build()
    {
        const string folder="Assets/CubeInvasion";
        var right=Route(folder+"/HealthRoute.asset", "Şifa",RouteReward.Health,30,3,0);
        var forward=Route(folder+"/DamageRoute.asset", "Cephanelik",RouteReward.Damage,15,2,1);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var runtime=new GameObject("Cube Invasion P0").AddComponent<InvasionPrototype>();
        runtime.rightRoute=right; runtime.forwardRoute=forward;
        EditorSceneManager.SaveScene(scene,folder+"/CubeInvasionPrototype.unity");
        EditorBuildSettings.scenes=new[] {
            new EditorBuildSettingsScene(folder+"/CubeInvasionPrototype.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity",false),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",false)
        };
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        PlayerSettings.defaultScreenWidth=540; PlayerSettings.defaultScreenHeight=960;
        AssetDatabase.SaveAssets();
        Debug.Log("CUBE_P0_BUILD_OK: scene and route assets generated.");
    }
    [MenuItem("Cube Invasion/Configure Android Portrait")]
    public static void ConfigureAndroid()
    {
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait=false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=false;
        PlayerSettings.allowedAutorotateToLandscapeRight=false;
        PlayerSettings.defaultScreenWidth=540;
        PlayerSettings.defaultScreenHeight=960;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        AssetDatabase.SaveAssets();
        if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android &&
           !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))
            throw new System.InvalidOperationException("Android platform switch failed. Check Android Build Support installation.");
        Debug.Log("CUBE_ANDROID_PORTRAIT_OK: "+EditorUserBuildSettings.activeBuildTarget);
    }
    static CubeRouteDefinition Route(string path,string title,RouteReward reward,float value,int grunts,int sprinters)
    {
        var data=AssetDatabase.LoadAssetAtPath<CubeRouteDefinition>(path);
        if(!data) { data=ScriptableObject.CreateInstance<CubeRouteDefinition>(); AssetDatabase.CreateAsset(data,path); }
        data.title=title; data.reward=reward; data.value=value; data.duration=2;
        data.risk=reward==RouteReward.Health?1:2; data.grunts=grunts;data.sprinters=sprinters;
        EditorUtility.SetDirty(data); return data;
    }
}
