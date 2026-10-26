using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
public static class BomberBuilder
{
    [MenuItem("Cube Invasion/Content/Install Bomber Encounters")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");const string path="Assets/CubeInvasion/Resources/BomberSettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<BomberSettings>(path);if(!settings) {settings=ScriptableObject.CreateInstance<BomberSettings>();AssetDatabase.CreateAsset(settings,path);}
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);data.FindProperty("bomberSettings").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);Debug.Log("CUBE_BOMBER_INSTALL_OK");
    }
}
