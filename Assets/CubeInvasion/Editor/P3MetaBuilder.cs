using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;

public static class P3MetaBuilder
{
    [MenuItem("Cube Invasion/P3/Install Progression In Current Scene")]
    public static void Install()
    {
        const string path="Assets/CubeInvasion/Resources/P3MetaSettings.asset";
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>(path);
        if(!rules) { rules=ScriptableObject.CreateInstance<P3MetaSettings>();AssetDatabase.CreateAsset(rules,path); }
        InstallSounds();
        var runtime=UnityEngine.Object.FindAnyObjectByType<InvasionPrototype>();
        if(!runtime)throw new System.InvalidOperationException("Open the Cube Invasion gameplay scene first.");
        var mobile=UnityEngine.Object.FindAnyObjectByType<MobileCombatHud>();
        var presentation=UnityEngine.Object.FindAnyObjectByType<P3MetaHud>(FindObjectsInactive.Include);
        if(!presentation)presentation=new GameObject("P3 Meta HUD",typeof(RectTransform)).AddComponent<P3MetaHud>();
        presentation.Initialize(mobile.SafeRoot);
        runtime.ConfigureMeta(rules,presentation);
        var missionHud=UnityEngine.Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);missionHud.SetMetaMode(true);
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        var preview=new MetaProfile {weapons=new WeaponProgress[4]};for(int i=0;i<4;i++)preview.weapons[i]=new WeaponProgress {owned=i==0};
        missionHud.Refresh(catalog.missions[0],catalog.weapons[0],0,catalog.missions.Length,0,"Loadout","",0);
        missionHud.SetProgression(preview,rules,0,0);presentation.Refresh(preview,rules,catalog.weapons[0],0,0,"Loadout",null);
        EditorUtility.SetDirty(runtime);EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(missionHud);
        EditorSceneManager.MarkSceneDirty(runtime.gameObject.scene);EditorSceneManager.SaveScene(runtime.gameObject.scene);AssetDatabase.SaveAssets();
        Debug.Log("CUBE_P3_INSTALL_OK: persistent wallet, arsenal upgrades and progression UI saved without resetting scene.");
    }
    // Unlock/upgrade/denied cues come from recorded CC0 audio (CubeAudioLibrary); nothing is synthesized.
    static void InstallSounds() { CubeAudioLibrary.Install(); }
    public static void InstallGameplayScene() { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
}
