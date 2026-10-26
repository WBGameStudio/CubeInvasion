using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
public static class CrusherPhaseBuilder
{
    [MenuItem("Cube Invasion/Content/Install Crusher Phase Objectives")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var game=Object.FindAnyObjectByType<InvasionPrototype>();const string path="Assets/CubeInvasion/Resources/CrusherPhaseSettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<CrusherPhaseSettings>(path);
        if(!settings) {settings=ScriptableObject.CreateInstance<CrusherPhaseSettings>();AssetDatabase.CreateAsset(settings,path);}
        var serialized=new SerializedObject(game);serialized.FindProperty("crusherPhases").objectReferenceValue=settings;serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);Debug.Log("CUBE_CRUSHER_INSTALL_OK");
    }
}
