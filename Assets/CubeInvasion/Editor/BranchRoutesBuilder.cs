using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
public static class BranchRoutesBuilder
{
    [MenuItem("Cube Invasion/Content/Install GDD Branch Routes")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var game=Object.FindAnyObjectByType<InvasionPrototype>();
        const string path="Assets/CubeInvasion/Resources/BranchRouteSettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<BranchRouteSettings>(path);
        if(!settings) {settings=ScriptableObject.CreateInstance<BranchRouteSettings>();AssetDatabase.CreateAsset(settings,path);}
        settings.rewards=new CubeRouteDefinition[8];
        float[] values={30,15,12,40,40,20,35,75};int[] risk={1,2,2,1,3,2,1,3};
        for(int i=0;i<8;i++)
        {
            if(i<2) {settings.rewards[i]=i==0?game.rightRoute:game.forwardRoute;continue;}
            string routePath=$"Assets/CubeInvasion/Resources/Branch{(RouteReward)i}.asset";
            var reward=AssetDatabase.LoadAssetAtPath<CubeRouteDefinition>(routePath);
            if(!reward) {reward=ScriptableObject.CreateInstance<CubeRouteDefinition>();AssetDatabase.CreateAsset(reward,routePath);}
            reward.title=((RouteReward)i).ToString();reward.reward=(RouteReward)i;reward.value=values[i];reward.risk=risk[i];reward.duration=i==2||i==6?2:1;
            EditorUtility.SetDirty(reward);settings.rewards[i]=reward;
        }
        EditorUtility.SetDirty(settings);var data=new SerializedObject(game);data.FindProperty("branchSettings").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log("CUBE_BRANCH_INSTALL_OK");
    }
}
