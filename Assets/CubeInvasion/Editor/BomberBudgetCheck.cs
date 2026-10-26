using System;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class BomberBudgetCheck
{
    public static void Run()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");string before=JsonUtility.ToJson(catalog);
        var settings=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));
        var mission=BomberEncounters.Expand(catalog.missions[4]);int count=0;
        foreach(var weapon in catalog.weapons)for(int seed=1;seed<=128;seed++)for(int stage=2;stage<=3;stage++)
        {
            settings.seed=seed;var offers=RouteDirector.Pair(settings,mission,4,stage,1,weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1));
            foreach(var offer in offers)
            {
                if(offer.expectedSeconds>20.001f)throw new Exception("Bomber route exceeds budget "+offer.Summary);
                if(Array.FindAll(offer.encounter.enemies,e=>e==ContentEnemy.Bomber).Length==0)throw new Exception("Bomber introduction removed");
                var kinds=new System.Collections.Generic.HashSet<ContentEnemy>(offer.encounter.enemies);if(kinds.Count>3)throw new Exception("More than three archetypes");count++;
            }
        }
        if(before!=JsonUtility.ToJson(catalog))throw new Exception("Authored content changed");Debug.Log($"CUBE_BOMBER_BUDGET_OK: {count} offers, four weapons, <=20s, <=3 archetypes, bomber introduction and source catalog preserved.");
    }
}
