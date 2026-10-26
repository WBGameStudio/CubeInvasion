using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class ExpansionTestFixture
{
    public static void Legacy()
    {var game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game)return;var data=new SerializedObject(game);data.FindProperty("campaignExpansion").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();}
}
