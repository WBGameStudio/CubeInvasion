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
        var menu=Object.FindAnyObjectByType<MenuFrontEnd>(FindObjectsInactive.Include);
        if(menu) {VoidToyStyle.Restyle(menu.transform,art?art.uiFont:null);EditorUtility.SetDirty(menu);}
        var scene=hud.gameObject.scene;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("CUBE_VOID_TOY_UI_OK: combat HUD, result modal and menus use the Void Toy Arcade palette.");
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
}
