using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
public static class SupportEnemyBuilder
{
    [MenuItem("Cube Invasion/Content/Install Support Enemies")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");const string path="Assets/CubeInvasion/Resources/SupportEnemySettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<SupportEnemySettings>(path);if(!settings) {settings=ScriptableObject.CreateInstance<SupportEnemySettings>();AssetDatabase.CreateAsset(settings,path);}
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);data.FindProperty("supportSettings").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);Debug.Log("CUBE_SUPPORT_INSTALL_OK");
    }
}
