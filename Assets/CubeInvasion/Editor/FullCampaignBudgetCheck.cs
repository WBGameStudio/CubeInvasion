using System;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class FullCampaignBudgetCheck
{
    public static void Run()
    {
        var source=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");string original=JsonUtility.ToJson(source);var stored=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/CampaignContent.asset");var catalog=stored?UnityEngine.Object.Instantiate(stored):CampaignExpansionSettings.Expand(source,2601026);var routes=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));var gold=AssetDatabase.LoadAssetAtPath<GoldRushVariantSettings>("Assets/CubeInvasion/Resources/GoldRushVariantSettings.asset");int count=0;
        for(int level=0;level<60;level++)
        {
            var mission=catalog.missions[level];if(level==4)mission=SupportEnemySettings.Expand(BomberEncounters.Expand(mission));
            if(mission.cubes.Length<3||mission.cubes.Length>5)throw new Exception("Invalid cube count");
            if(level>=7&&(level+1)%10==0&&(mission.mode!=MissionMode.Boss||(int)mission.boss!=(level+1)/10-1))throw new Exception("Boss cadence");
            foreach(var weapon in catalog.weapons)for(int seed=1;seed<=16;seed++)for(int stage=1;stage<=mission.cubes.Length;stage++)
            {
                routes.seed=seed;var pair=RouteDirector.Pair(routes,mission,level,stage,.5f,weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1),mission.mode==MissionMode.Boss&&mission.boss==BossIdentity.Crusher,gold);
                foreach(var offer in pair)
                {
                    if(offer.expectedSeconds>20.01f)throw new Exception($"Over budget L{level+1} C{stage} {weapon.title} {offer.Summary} {offer.expectedSeconds}");
                    if(mission.mode!=MissionMode.Boss&&new System.Collections.Generic.HashSet<ContentEnemy>(offer.encounter.enemies).Count>3)throw new Exception("More than three enemy types L"+(level+1)+" C"+stage+" "+string.Join(",",offer.encounter.enemies));
                    if(offer.encounter.layout!=mission.cubes[stage-1].layout||offer.encounter.hazard!=mission.cubes[stage-1].hazard)throw new Exception("Route lost geometry/hazard");count++;
                }
            }
        }
        if(original!=JsonUtility.ToJson(source))throw new Exception("Source mutated");Debug.Log("CUBE_FULL_BUDGET_OK: "+count+" route offers / 60 levels / 12 weapons / 16 seeds / <=20 seconds / geometry retained / source unchanged");
    }
}
