using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [SerializeField] CampaignExpansionSettings campaignExpansion;
        EncounterSpec initialExpansionEncounter;
        void PrepareInitialExpandedEncounter()
        {
            initialExpansionEncounter=null;if(Expanded&&HasBranches&&stage==1)initialExpansionEncounter=RouteDirector.Build(branchSettings,Mission,1,rightRoute,campaignExpansion.seed+missionIndex,RouteBudgetDps,HasCrusherObjectives,HasGoldRushVariants?goldRushVariants:null).encounter;
        }
        bool Expanded=>campaignExpansion&&campaignExpansion.enabled;
        readonly Dictionary<Foe,Vector3> teleportDestinations=new Dictionary<Foe,Vector3>();
        readonly Dictionary<Foe,AdvancedEnemyController> advancedBrains=new Dictionary<Foe,AdvancedEnemyController>();
        readonly List<Vector3> summonQueue=new List<Vector3>();
        readonly List<Foe> enemyTickBuffer=new List<Foe>();
        void InitializeExpansion()
        {
            if(!Expanded||!HasContent)return;
            content=campaignExpansion.catalog?Instantiate(campaignExpansion.catalog):CampaignExpansionSettings.Expand(content,campaignExpansion.seed);
            if(metaSettings) {metaSettings=Instantiate(metaSettings);metaSettings.unlockPrices=(int[])(campaignExpansion.unlockPrices!=null&&campaignExpansion.unlockPrices.Length==12?campaignExpansion.unlockPrices:CampaignExpansionSettings.Prices).Clone();}
            var presentation=Resources.Load<CampaignPresentation>("CampaignPresentation");
            if(presentation&&presentation.art) {artLibrary=Instantiate(artLibrary);var all=new List<ToyArtLibrary.Model>(artLibrary.models);all.AddRange(presentation.art.models);artLibrary.models=all.ToArray();artLibrary.InvalidateLookup();}
            if(frontEndArt) {frontEndArt=Instantiate(frontEndArt);var sprites=new Sprite[12];for(int i=0;i<12;i++)sprites[i]=i>=4&&presentation&&presentation.weapons.Length>=8?presentation.weapons[i-4]:frontEndArt.weapons[i%frontEndArt.weapons.Length];frontEndArt.weapons=sprites;}
        }
        void EquipAdvancedEnemy(Foe foe)
        {
            if((int)foe.kind<(int)EnemyKind.Summoner||foe.kind==EnemyKind.ObjectiveNode)return;
            advancedBrains[foe]=new AdvancedEnemyController((ContentEnemy)(int)foe.kind);
            var color=foe.kind==EnemyKind.Summoner?VoidToyPalette.Cyan:foe.kind==EnemyKind.Phaser?new Color(.7f,.3f,1):VoidToyPalette.Gold;
            var parent=foe.animation?foe.animation.VisualRoot:foe.body;
            if(parent.Find("Expansion identity"))return;
            var identity=new GameObject("Expansion identity").transform;identity.SetParent(parent,false);parent=identity;
            int count=foe.kind==EnemyKind.Elite?2:foe.kind==EnemyKind.Summoner?3:1;
            for(int i=0;i<count;i++)Shape("Threat crest",PrimitiveType.Cube,new Vector3((i-(count-1)*.5f)*.3f,1,.1f),new Vector3(.14f,.32f,.14f),color,parent);
        }
        bool TickAdvancedEnemy(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(!advancedBrains.TryGetValue(foe,out var brain))return false;
            var order=brain.Tick(dt,remaining);
            if(order==AdvancedEnemyOrder.PrepareSummon||order==AdvancedEnemyOrder.PrepareShot||order==AdvancedEnemyOrder.PreparePush)
            {foe.aim=toward.normalized;foe.warning.gameObject.SetActive(true);foe.warning.position=foe.body.position;foe.animation?.Telegraph(brain.windup);feedback.Play(CombatFeedback.Cue.Telegraph);}
            else if(order==AdvancedEnemyOrder.Summon)
            {foe.warning.gameObject.SetActive(false);for(int i=0;i<2;i++)summonQueue.Add(foe.body.position+new Vector3(i==0?-.9f:.9f,0,-.6f));feedback.Play(CombatFeedback.Cue.Summon);}
            else if(order==AdvancedEnemyOrder.PrepareTeleport)
            {
                var destination=centre+new Vector3(Mathf.Sin(foe.pattern*2.4f)*2.8f,.6f,Mathf.Cos(foe.pattern*2.4f)*2.8f);foe.pattern++;
                destination=SafeBranchSpawn(destination,ContentEnemy.Phaser);teleportDestinations[foe]=destination;foe.warning.position=new Vector3(destination.x,.1f,destination.z);foe.warning.gameObject.SetActive(true);foe.animation?.Telegraph(brain.windup);feedback.Play(CombatFeedback.Cue.Telegraph);
            }
            else if(order==AdvancedEnemyOrder.Teleport)
            {feedback.Burst(foe.body.position,foe.color,1,.2f,true);if(teleportDestinations.TryGetValue(foe,out var destination))foe.body.position=destination;teleportDestinations.Remove(foe);foe.warning.gameObject.SetActive(false);foe.spawn=.15f;feedback.Play(CombatFeedback.Cue.Teleport);}
            else if(order==AdvancedEnemyOrder.Shoot||order==AdvancedEnemyOrder.Push)
            {
                foe.warning.gameObject.SetActive(false);
                if(order==AdvancedEnemyOrder.Shoot)for(int i=-1;i<=1;i++)FireEnemyShot(foe.body.position,Quaternion.Euler(0,i*14,0)*foe.aim,6*foe.attackPower,6);
                else if(distance<2.5f&&Vector3.Dot(toward.normalized,foe.aim)>.5f)DamagePlayer(7*EnemyDamageMultiplier*foe.attackPower,foe.aim*7);
                foe.pattern++;foe.animation?.Shoot();
            }
            else if(brain.windup<=0)
            {
                if(foe.kind==EnemyKind.Summoner) {if(distance<2.5f)MoveSupport(foe,foe.body.position-toward.normalized*2,dt);}
                else if(foe.kind==EnemyKind.Phaser)
                {if(distance>1.25f)MoveEnemy(foe,toward.normalized*foe.speed*dt);if(foe.attack<=0&&distance<1.4f) {DamagePlayer(6*EnemyDamageMultiplier*foe.attackPower,toward.normalized);foe.attack=1.5f;foe.animation?.Shoot();}}
                else if(distance>2)MoveEnemy(foe,toward.normalized*foe.speed*dt);
            }
            return true;
        }

        void FlushSummons()
        {
            foreach(var position in summonQueue)
            {
                if(phase!=Phase.Combat||remaining<=3)break;
                var minion=SpawnContentEnemy(ContentEnemy.Grunt,SafeBranchSpawn(position,ContentEnemy.Grunt));minion.hp=minion.maxHp=5;minion.speed=1.25f;minion.spawn=.8f;minion.body.localScale*=.65f;
            }
            summonQueue.Clear();
        }
        float AdvancedDamage(Foe foe,float damage)=>advancedBrains.TryGetValue(foe,out var brain)&&brain.weak>0?damage*1.3f:damage;
        string FailureGuidance(FailureKind kind)
        {
            if(kind==FailureKind.EdgeFall)return "İtme işaretini görünce merkeze doğru hareket et.";
            if(kind==FailureKind.Timeout)return "Joystiği merkeze yaklaştır: koşmayı azalt, daha sık ateş et.";
            if(Mission.special==SpecialRule.ChainReactor)return "Parlayan enerji düğümünü sırayla vur.";
            return "Siper kullan; saldırı uyarısı sırasında yön değiştir.";
        }
        void ResetExpansionCombat()
        {advancedBrains.Clear();teleportDestinations.Clear();summonQueue.Clear();ClearExpandedArena();ClearExpandedBoss();pickupBuffs.Clear();}
    }
}
