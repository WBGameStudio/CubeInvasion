using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

// Writes the Void Toy Arcade styling into the authored scene so edit mode matches Play.
// Runtime applies the same styling on start, so this step is optional for playtests.
public static class VoidToyUiBuilder
{
    [MenuItem("Cube Invasion/UI/Apply Void Toy Style")]
    public static void Install()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode before restyling the authored UI.");
        var hud=Object.FindAnyObjectByType<MobileCombatHud>(FindObjectsInactive.Include);
        if(!hud||!hud.SafeRoot)throw new InvalidOperationException("Open the gameplay scene first.");
        var camera=hud.Canvas?hud.Canvas.worldCamera:Camera.main;
        hud.ApplyVoidToyStyle(camera);EditorUtility.SetDirty(hud);
        var mission=Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);
        if(mission) {mission.ApplyVoidToyStyle();EditorUtility.SetDirty(mission);}
        var art=AssetDatabase.LoadAssetAtPath<FrontEndArt>("Assets/CubeInvasion/Resources/FrontEndArt.asset");
        // The front end is rebuilt from MenuFrontEnd's layout code so Home/Weapons get the Void Toy composition.
        var game=Object.FindAnyObjectByType<InvasionPrototype>();
        var old=Object.FindAnyObjectByType<MenuFrontEnd>(FindObjectsInactive.Include);
        if(old)Object.DestroyImmediate(old.gameObject);
        var menu=new GameObject("Void Toy Front End",typeof(RectTransform)).AddComponent<MenuFrontEnd>();
        var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        menu.Initialize(hud.SafeRoot,art,content);if(game) {game.ConfigureFrontEnd(menu,art);EditorUtility.SetDirty(game);}
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");
        var profile=new MetaProfile {weapons=new WeaponProgress[4],completed=new bool[content.missions.Length]};
        for(int i=0;i<4;i++)profile.weapons[i]=new WeaponProgress {owned=i==0};
        menu.Refresh(profile,rules,content,0,0,0,"Loadout",null);menu.SyncSettings(1,1,false,false,true);menu.ShowPage(MenuPage.Home,false);
        EditorUtility.SetDirty(menu);
        var scene=hud.gameObject.scene;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("CUBE_VOID_TOY_UI_OK: combat HUD, result modal and menus use the Void Toy Arcade palette.");
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
}
