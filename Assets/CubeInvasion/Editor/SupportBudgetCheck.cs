using System;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class SupportBudgetCheck
{
    public static void Run()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");string before=JsonUtility.ToJson(catalog);
        var mission=SupportEnemySettings.Expand(BomberEncounters.Expand(catalog.missions[4]));var settings=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));int count=0;
        foreach(var weapon in catalog.weapons)for(int seed=1;seed<=128;seed++)for(int stage=4;stage<=5;stage++)
        {
            settings.seed=seed;var offers=RouteDirector.Pair(settings,mission,4,stage,.5f,weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1));bool healer=false;
            foreach(var offer in offers)
            {
                if(offer.expectedSeconds>20.001f)throw new Exception("Support budget exceeds 20s "+offer.Summary+" "+offer.expectedSeconds);
                if(!Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Shielder))throw new Exception("Shield introduction removed");
                if(new System.Collections.Generic.HashSet<ContentEnemy>(offer.encounter.enemies).Count>3)throw new Exception("Too many archetypes");
                healer|=Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Healer);count++;
            }
            if(stage==5&&!healer)throw new Exception("Both routes remove healer introduction");
        }
        if(before!=JsonUtility.ToJson(catalog))throw new Exception("Source catalog changed");Debug.Log($"CUBE_SUPPORT_BUDGET_OK: {count} offers, full-frontal shield budget plus one heal reserve, four weapons, <=20s, <=3 archetypes, source unchanged.");
    }
}
