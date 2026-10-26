using System;
using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed class RouteOffer
    {
        public readonly CubeRouteDefinition reward;
        public readonly EncounterSpec encounter;
        public readonly int stage,seed,eliteIndex;
        public readonly float expectedSeconds;
        public RouteOffer(CubeRouteDefinition reward,EncounterSpec encounter,int stage,int seed,int eliteIndex,float expectedSeconds)
        {this.reward=reward;this.encounter=encounter;this.stage=stage;this.seed=seed;this.eliteIndex=eliteIndex;this.expectedSeconds=expectedSeconds;}
        public string Summary=>$"{reward.reward} / R{reward.risk} / {encounter.enemies.Length} hedef / küp {stage}";
    }
    public static class RouteDirector
    {
        static uint Next(ref uint state) {state^=state<<13;state^=state>>17;state^=state<<5;return state;}
        public static RouteOffer[] Pair(BranchRouteSettings settings,MissionSpec mission,int missionIndex,int targetStage,float hpFraction,float nominalDps)
        {
            if(settings==null||settings.rewards==null||settings.rewards.Length<8)throw new ArgumentException("Eight route rewards are required");
            uint random=unchecked((uint)(settings.seed^(missionIndex+1)*73856093^targetStage*19349663^Mathf.RoundToInt(nominalDps*100)*83492791));if(random==0)random=17;
            CubeRouteDefinition first=null,second=null;
            if(missionIndex==0&&targetStage==2)
            {foreach(var reward in settings.rewards) {if(reward.reward==RouteReward.Health)first=reward;if(reward.reward==RouteReward.Damage)second=reward;}}
            else
            {
                first=Pick(settings,mission,hpFraction,true,null,ref random);
                second=Pick(settings,mission,hpFraction,false,first,ref random);
            }
            int pairSeed=unchecked((int)random);
            return new[]{Build(settings,mission,targetStage,first,pairSeed,nominalDps),Build(settings,mission,targetStage,second,pairSeed^0x31f4a271,nominalDps)};
        }
        static CubeRouteDefinition Pick(BranchRouteSettings settings,MissionSpec mission,float health,bool safe,CubeRouteDefinition exclude,ref uint random)
        {
            int total=0;foreach(var r in settings.rewards)if(Allowed(r,mission,safe,exclude))total+=r.reward==RouteReward.Health&&health<.25f?4:1;
            if(total==0)throw new ArgumentException("No valid distinct route pair");int roll=(int)(Next(ref random)%(uint)total);
            foreach(var r in settings.rewards)if(Allowed(r,mission,safe,exclude)) {roll-=r.reward==RouteReward.Health&&health<.25f?4:1;if(roll<0)return r;}
            throw new InvalidOperationException();
        }
        static bool Allowed(CubeRouteDefinition r,MissionSpec mission,bool safe,CubeRouteDefinition exclude)
            =>r&&(!exclude||r.reward!=exclude.reward)&&(!safe||r.risk<=2)&&(mission.mode!=MissionMode.GoldRush||r.reward!=RouteReward.Elite);
        public static float Hp(ContentEnemy kind,int stage)
        {switch(kind) {case ContentEnemy.Grunt:return 18;case ContentEnemy.Sprinter:return 12;case ContentEnemy.Gunner:return 21;case ContentEnemy.Pusher:return 24;case ContentEnemy.Sniper:return 14;case ContentEnemy.Bruiser:return 45;default:return stage==1?72:stage==2?84:96;}}
        public static RouteOffer Build(BranchRouteSettings settings,MissionSpec mission,int targetStage,CubeRouteDefinition reward,int seed,float nominalDps)
        {
            var source=mission.cubes[Mathf.Clamp(targetStage-1,0,mission.cubes.Length-1)];var enemies=new List<ContentEnemy>(source.enemies);
            if(reward.risk==1&&enemies.Count>4)RemoveStrongest(enemies);
            if(reward.reward==RouteReward.FireRate||reward.reward==RouteReward.Magnet)
            {int i=enemies.FindIndex(e=>e==ContentEnemy.Grunt);if(i>=0)enemies[i]=ContentEnemy.Sprinter;}
            if(reward.reward==RouteReward.Shield)
            {int i=enemies.FindIndex(e=>e==ContentEnemy.Grunt);if(i>=0)enemies[i]=ContentEnemy.Pusher;}
            if(reward.reward==RouteReward.Gold&&mission.mode!=MissionMode.Boss)enemies.Add(ContentEnemy.Sprinter);
            // The mandatory boss and objective stay unchanged; only ordinary threats can be trimmed.
            float dps=Mathf.Max(.1f,nominalDps*settings.effectiveDps);float budget=dps*(20-settings.movementAllowance);
            if(reward.reward==RouteReward.Elite&&!enemies.Exists(e=>e!=ContentEnemy.Crusher))enemies.Add(ContentEnemy.Grunt);
            int elite=Elite(enemies,reward);
            while(Total(enemies,targetStage,elite,settings.eliteHealth)>budget&&enemies.Count>1)
            {if(reward.reward==RouteReward.Elite&&enemies.FindAll(e=>e!=ContentEnemy.Crusher).Count==1)break;
                if(!RemoveStrongest(enemies))break;elite=Elite(enemies,reward);}
            var encounter=new EncounterSpec {enemies=enemies.ToArray(),cover=reward.risk>1&&source.cover,barrels=source.barrels||reward.reward==RouteReward.Blast,
                healing=source.healing,coverOffset=new Vector2((seed&1)==0?-.65f:.65f,source.coverOffset.y),healOffset=source.healOffset,goldQuota=source.goldQuota};
            return new RouteOffer(reward,encounter,targetStage,seed,elite,Total(enemies,targetStage,elite,settings.eliteHealth)/dps+settings.movementAllowance);
        }
        static int Elite(List<ContentEnemy> enemies,CubeRouteDefinition reward)
            =>reward.reward==RouteReward.Elite?enemies.FindIndex(e=>e!=ContentEnemy.Crusher):-1;
        static float Total(List<ContentEnemy> enemies,int stage,int elite,float multiplier)
        {float total=0;for(int i=0;i<enemies.Count;i++)total+=Hp(enemies[i],stage)*(i==elite?multiplier:1);return total;}
        static bool RemoveStrongest(List<ContentEnemy> enemies)
        {
            int index=-1;float hp=0;
            for(int i=0;i<enemies.Count;i++)if(enemies[i]!=ContentEnemy.Crusher&&Hp(enemies[i],1)>hp) {hp=Hp(enemies[i],1);index=i;}
            if(index<0)return false;enemies.RemoveAt(index);return true;
        }
    }
}
