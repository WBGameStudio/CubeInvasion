using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
public static class GoldRushVariantBuilder
{
    [MenuItem("Cube Invasion/Content/Install Gold Rush Variants")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        const string path="Assets/CubeInvasion/Resources/GoldRushVariantSettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<GoldRushVariantSettings>(path);if(!settings) {settings=ScriptableObject.CreateInstance<GoldRushVariantSettings>();AssetDatabase.CreateAsset(settings,path);}
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);data.FindProperty("goldRushVariants").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);Debug.Log("CUBE_GOLDRUSH_INSTALL_OK");
    }
}
