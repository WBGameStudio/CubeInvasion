using System;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class CrusherBudgetCheck
{
    public static void Run()
    {
        var routes=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");var boss=catalog.missions[6];int count=0;
        foreach(var weapon in catalog.weapons)for(int seed=1;seed<=128;seed++)for(int phase=2;phase<=3;phase++)
        {
            routes.seed=seed;float dps=weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1);
            var offers=RouteDirector.Pair(routes,boss,6,phase,1,dps,true);
            foreach(var offer in offers)
            {
                if(offer.expectedSeconds>20.001f)throw new Exception("Boss phase exceeds budget: "+offer.Summary);
                if(Array.FindAll(offer.encounter.enemies,e=>e==ContentEnemy.Crusher).Length!=1)throw new Exception("Mandatory boss removed");
                if(phase==2&&offer.encounter.enemies.Length!=4)throw new Exception("Support phase must expose three drones in preview");
                count++;
            }
        }
        Debug.Log($"CUBE_CRUSHER_BUDGET_OK: {count} offers / four weapons; mandatory boss, three-drone previews, <=20s.");
    }
}
