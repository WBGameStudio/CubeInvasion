using System;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class GoldRushBudgetCheck
{
    public static void Run()
    {
        var settings=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));
        var gold=AssetDatabase.LoadAssetAtPath<GoldRushVariantSettings>("Assets/CubeInvasion/Resources/GoldRushVariantSettings.asset");var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");int count=0;
        string before=JsonUtility.ToJson(content);
        foreach(var weapon in content.weapons)for(int seed=1;seed<=128;seed++)for(int stage=2;stage<=3;stage++)
        {
            settings.seed=seed;var offers=RouteDirector.Pair(settings,content.missions[5],5,stage,1,weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1),false,gold);
            if(stage==3&&offers[0].encounter.goldVariant==offers[1].encounter.goldVariant)throw new Exception("Final gates must offer different objectives");
            foreach(var offer in offers)
            {
                if(offer.expectedSeconds>20.001f)throw new Exception("Gold objective exceeds budget: "+offer.Summary+" / "+offer.expectedSeconds);
                if(offer.encounter.goldVariant==GoldRushVariant.GoldenSwarm&&offer.encounter.enemies.Length!=5)throw new Exception("Five carriers required");
                if(offer.encounter.goldVariant==GoldRushVariant.Jackpot&&offer.encounter.enemies.Length!=3)throw new Exception("Three guards required");count++;
            }
        }
        if(before!=JsonUtility.ToJson(content))throw new Exception("Content catalog mutated");Debug.Log($"CUBE_GOLDRUSH_BUDGET_OK: {count} offers / four weapons, distinct final objectives, required counts, unchanged catalog, <=20s.");
    }
}
