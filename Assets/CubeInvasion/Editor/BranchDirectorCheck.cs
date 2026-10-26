using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class BranchDirectorCheck
{
    static void Check(bool condition,string message) {if(!condition)throw new Exception(message);}
    public static void Run()
    {
        var settings=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<BranchRouteSettings>("Assets/CubeInvasion/Resources/BranchRouteSettings.asset"));
        var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        string before=JsonUtility.ToJson(content);var seen=new HashSet<RouteReward>();int pairs=0,healthy=0,low=0;
        for(int seed=1;seed<=128;seed++)
        {
            settings.seed=seed;
            for(int m=0;m<content.missions.Length;m++)for(int cube=2;cube<=content.missions[m].cubes.Length;cube++)foreach(var weapon in content.weapons)
            {
                var mission=content.missions[m];float dps=weapon.NominalDps*(weapon.kind==ContentWeapon.ScatterShot?.6f:1);
                var pair=RouteDirector.Pair(settings,mission,m,cube,1,dps);var repeated=RouteDirector.Pair(settings,mission,m,cube,1,dps);
                Check(pair[0].reward.reward!=pair[1].reward.reward,"Distinct offers");Check(pair[0].reward.risk<=2||pair[1].reward.risk<=2,"Safe offer");
                for(int i=0;i<2;i++)
                {
                    var offer=pair[i];seen.Add(offer.reward.reward);Check(offer.Summary==repeated[i].Summary&&offer.seed==repeated[i].seed,"Deterministic offers");
                    Check(offer.expectedSeconds<=20.001f,"20 second budget: "+m+" "+cube+" "+weapon.title+" "+offer.Summary+" "+offer.expectedSeconds);
                    Check(offer.encounter.goldQuota==mission.cubes[cube-1].goldQuota,"Quota preserved");
                    int bosses=Array.FindAll(offer.encounter.enemies,e=>e==ContentEnemy.Crusher).Length;
                    Check(bosses==Array.FindAll(mission.cubes[cube-1].enemies,e=>e==ContentEnemy.Crusher).Length,"Mandatory boss preserved");
                    Check(offer.reward.reward!=RouteReward.Elite||offer.eliteIndex>=0,"Elite actually exists");
                }
                pairs++;
            }
            var high=RouteDirector.Pair(settings,content.missions[2],2,2,1,12);var hurt=RouteDirector.Pair(settings,content.missions[2],2,2,.2f,12);
            foreach(var r in high)if(r.reward.reward==RouteReward.Health)healthy++;foreach(var r in hurt)if(r.reward.reward==RouteReward.Health)low++;
        }
        Check(seen.Count==8,"All eight rewards offered");Check(low>healthy&&low<128,"Low health weights healing without guaranteeing it");Check(before==JsonUtility.ToJson(content),"Catalog remains immutable");
        Debug.Log($"CUBE_BRANCH_DIRECTOR_OK: {pairs} pairs, all rewards, deterministic budgets, boss/quota preservation; healing {healthy}/{low}");
    }
}
