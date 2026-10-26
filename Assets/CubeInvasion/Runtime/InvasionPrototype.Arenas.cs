using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        sealed class HazardZone {public Transform marker;public Vector3 at;public ArenaHazard kind;public float clock;public bool active;}
        readonly List<HazardZone> hazardZones=new List<HazardZone>();
        readonly List<Transform> additionalCover=new List<Transform>();
        readonly Dictionary<Transform,float> coverHealth=new Dictionary<Transform,float>();
        readonly List<Foe> hazardScratch=new List<Foe>();
        readonly Dictionary<Foe,int> objectiveNodes=new Dictionary<Foe,int>();
        readonly TemporaryBuffController pickupBuffs=new TemporaryBuffController();
        float temporaryAmp {get=>pickupBuffs.DamageRemaining;set=>pickupBuffs.DamageRemaining=value;}
        float temporaryMagnet {get=>pickupBuffs.MagnetRemaining;set=>pickupBuffs.MagnetRemaining=value;}
        float specialClock;
        int nextObjectiveNode;
        bool survivalWavePending;
        Barrel barrelTarget;
        Vector3 iceVelocity;
        float ArenaHalfX=>Expanded&&HasContent&&Encounter.layout==ArenaLayout.Narrow?3:Half;
        float RightGateX=>ArenaHalfX-.35f;
        float ArenaMoveMultiplier=>Expanded&&Mission.special==SpecialRule.LowGravity?.9f:1;
        bool ExpandedObjectiveComplete=>!survivalWavePending&&(!HasExpandedBoss||ExpandedBossComplete);
        void SetupExpandedArena()
        {
            ClearExpandedArena();if(!Expanded)return;
            specialClock=0;nextObjectiveNode=0;survivalWavePending=Mission.special==SpecialRule.SurvivalFlip;
            DecorateBiome();
            var layout=Encounter.layout;
            if(layout==ArenaLayout.Narrow)
            {
                SetArenaWidth(platform,6);
                if(rightGate&&rightGate.parent==platform)rightGate.localPosition=new Vector3(RightGateX,0,0);
            }
            if(layout==ArenaLayout.Pillars) {AddCover(new Vector3(-2,.5f,.8f),new Vector3(.8f,1,.8f));AddCover(new Vector3(2,.5f,1.4f),new Vector3(.8f,1,.8f));}
            if(layout==ArenaLayout.SplitLane)AddCover(new Vector3(0,.4f,1.2f),new Vector3(.6f,.8f,3));
            if(layout==ArenaLayout.ExplosiveYard||Mission.special==SpecialRule.BarrelParty) {CreateBarrel(centre+new Vector3(-2,.5f,0));CreateBarrel(centre+new Vector3(2,.5f,0));}
            if(layout==ArenaLayout.HazardGrid||Encounter.hazard!=ArenaHazard.None)
            {
                var kind=Encounter.hazard==ArenaHazard.None?ArenaHazard.Spike:Encounter.hazard;
                for(int i=0;i<2;i++)
                {var at=centre+new Vector3(i==0?-2:2,.1f,1.1f);var marker=WarningRing(at,1,kind==ArenaHazard.Ice?VoidToyPalette.Cyan:danger);hazardZones.Add(new HazardZone {at=at,marker=marker,kind=kind,clock=i*.8f});SpawnHazardTile(hazardZones[hazardZones.Count-1]);}
            }
            if(Mission.special==SpecialRule.HealingStop) {SpawnPickup(centre+new Vector3(-2.3f,.45f,0),true,20);SpawnPickup(centre+new Vector3(2.3f,.45f,0),true,20);}
            if(missionIndex>=7&&(missionIndex+stage)%3==0)SpawnSpecialPickup(centre+new Vector3(-2,.45f,-.3f),true);
            if(missionIndex>=7&&(missionIndex+stage)%4==0)SpawnSpecialPickup(centre+new Vector3(2,.45f,-.3f),false);
            if(Mission.special==SpecialRule.ChainReactor)for(int i=0;i<3;i++)SpawnObjectiveNode(i,centre+new Vector3((i-1)*2.4f,.6f,2.7f),12,"Enerji düğümü");

        }
        void SetArenaWidth(Transform arena,float width)
        {
            var top=arena.Find("Arena surface");var sides=arena.Find("Deep sides");if(top)top.localScale=new Vector3(width,.16f,9);if(sides)sides.localScale=new Vector3(width,2.4f,9);
            foreach(Transform child in arena)if(child.name=="Edge light") {var position=child.localPosition;var size=child.localScale;if(Mathf.Abs(position.x)>1)position.x=Mathf.Sign(position.x)*(width*.5f-.05f);else size.x=width;child.localPosition=position;child.localScale=size;}
        }
        void PreparePreviewGeometry(Transform preview,int targetStage)
        {if(Expanded&&HasContent&&Mission.cubes[Mathf.Clamp(targetStage-1,0,CubeCount-1)].layout==ArenaLayout.Narrow)SetArenaWidth(preview,6);}
        void DecorateBiome()
        {
            if(Mission.biome==0)return;
            var color=CampaignExpansionSettings.Colors[Mathf.Clamp(Mission.biome,0,5)];
            foreach(int side in new[]{-1,1})
            {
                if(Mission.biome==1)for(int i=0;i<3;i++)Shape("Explosive band",PrimitiveType.Cube,new Vector3(side*3.7f,.1f+i*.18f,3.7f),new Vector3(.6f,.1f,.6f),color,platform);
                else if(Mission.biome==2)Shape("Edge light",PrimitiveType.Cube,new Vector3(0,.08f,side*3.5f),new Vector3(5,.03f,.045f),color,platform);
                else if(Mission.biome==3)Shape("Frozen patch",PrimitiveType.Cube,new Vector3(side*2.2f,.07f,1.1f),new Vector3(1.6f,.025f,1.6f),color,platform);
                else if(Mission.biome==4)for(int i=0;i<3;i++)Shape("Edge light",PrimitiveType.Cube,new Vector3(side*3.6f,.08f,i*.3f+2.7f),new Vector3(.6f,.035f,.09f),color,platform);
                else {Shape("Gate left post",PrimitiveType.Cube,new Vector3(side*3.7f,.4f,3.7f),new Vector3(.15f,.8f,.15f),color,platform);Shape("Edge light",PrimitiveType.Cube,new Vector3(side*3.7f,.85f,3.7f),new Vector3(.6f,.07f,.07f),color,platform);}
            }
        }
        void AddCover(Vector3 local,Vector3 size)
        {var block=Shape("Low cover",PrimitiveType.Cube,local,size,VoidToyPalette.Raised,platform);additionalCover.Add(block);coverHealth[block]=24;}
        void ClearExpandedArena()
        {
            foreach(var zone in hazardZones)if(zone.marker)Destroy(zone.marker.gameObject);hazardZones.Clear();
            foreach(var block in additionalCover)if(block)Destroy(block.gameObject);additionalCover.Clear();coverHealth.Clear();
            foreach(var pickup in specialPickups)if(pickup.body)Destroy(pickup.body.gameObject);specialPickups.Clear();
            objectiveNodes.Clear();barrelTarget=null;iceVelocity=Vector3.zero;survivalWavePending=false;
        }
        void TickExpandedArena(float dt)
        {
            if(!Expanded)return;
            pickupBuffs.Tick(dt);specialClock+=dt;
            if(survivalWavePending&&specialClock>=4)
            {survivalWavePending=false;for(int i=0;i<2;i++) {var foe=SpawnContentEnemy(ContentEnemy.Sprinter,SafeBranchSpawn(centre+new Vector3(i==0?-2.8f:2.8f,.6f,2.8f),ContentEnemy.Sprinter));foe.hp=foe.maxHp=8;}}
            hazardScratch.Clear();hazardScratch.AddRange(foes);
            foreach(var zone in hazardZones)
            {
                if(zone.kind==ArenaHazard.None)continue;
                zone.clock+=dt;float cycle=zone.clock%(zone.kind==ArenaHazard.Electric?2:5);bool active=zone.kind==ArenaHazard.Ice||(zone.kind==ArenaHazard.Electric?cycle>=1.3f&&cycle<1.5f:cycle>=1&&cycle<3);
                zone.active=active;zone.marker.localScale=Vector3.one*(active?1:Mathf.Lerp(.7f,1,cycle));
                var line=zone.marker.GetComponent<LineRenderer>();line.widthMultiplier=active?.08f:.025f;
                ShowHazardTile(zone,cycle);
                if(!active)continue;
                if(FlatDistance(player.position,zone.at)<1)
                {
                    if(zone.kind==ArenaHazard.Ice)
                    {var forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;var right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;iceVelocity=Vector3.Lerp(iceVelocity,(right*stick.x-forward*stick.y)*1.6f,dt*3);var next=player.position+iceVelocity*dt;if(!CoverHit(player.position,next,.3f,out _))player.position=next;}
                    else DamagePlayer(zone.kind==ArenaHazard.Lava?6:4,Vector3.zero);
                }
                if(zone.kind!=ArenaHazard.Ice)foreach(var foe in hazardScratch)if(foe.body&&foes.Contains(foe)&&FlatDistance(foe.body.position,zone.at)<1)HitEnemyArea(foe,dt*(zone.kind==ArenaHazard.Electric?30:3),Vector3.up);
            }
            for(int i=specialPickups.Count-1;i>=0;i--)
            {var pickup=specialPickups[i];if(FlatDistance(player.position,pickup.body.position)>PickupRadius)continue;if(pickup.amp)temporaryAmp=4;else temporaryMagnet=3;feedback.Play(CombatFeedback.Cue.Heal);Destroy(pickup.body.gameObject);specialPickups.RemoveAt(i);}
            TickExpandedBossWorld(dt);
        }
        void DamageExpandedCover(Vector3 start,Vector3 end,float damage)
        {
            Transform selected=null;float best=float.PositiveInfinity;
            foreach(var block in additionalCover)if(CoverTransformHit(block,start,end,.09f,out float t)&&t<best) {best=t;selected=block;}
            if(!selected||!coverHealth.TryGetValue(selected,out float hp))return;
            coverHealth[selected]=hp-damage;
            if(hp<=damage) {additionalCover.Remove(selected);coverHealth.Remove(selected);feedback.Burst(selected.position,VoidToyPalette.Raised,1,.2f);Destroy(selected.gameObject);}
        }
        sealed class SpecialPickup {public Transform body;public bool amp;}
        readonly List<SpecialPickup> specialPickups=new List<SpecialPickup>();
        void SpawnSpecialPickup(Vector3 at,bool amp)
        {var body=Shape(amp?"Damage amplifier":"Magnet core",PrimitiveType.Cube,at,Vector3.one*.3f,amp?danger:VoidToyPalette.Cyan,dynamicRoot);specialPickups.Add(new SpecialPickup {body=body,amp=amp});AnimatePickup(body);}
        void SpawnObjectiveNode(int index,Vector3 at,float hp,string label)
        {var node=SpawnContentEnemy(ContentEnemy.ObjectiveNode,at);node.body.name=label+" "+(index+1);node.hp=node.maxHp=hp;node.speed=0;objectiveNodes[node]=index;node.warning.gameObject.SetActive(true);node.warning.localScale=Vector3.one*.4f;RefreshObjectiveMarkers();
            var existing=node.body.Find("Objective order");var text=existing?existing.GetComponent<TextMesh>():new GameObject("Objective order").AddComponent<TextMesh>();text.transform.SetParent(node.body,false);text.transform.localPosition=Vector3.up; text.transform.rotation=view.transform.rotation;text.text=(index+1).ToString();text.characterSize=.15f;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;
            if(!node.body.Find("Reward beacon vertical"))Shape("Reward beacon vertical",PrimitiveType.Cube,new Vector3(0,1,0),new Vector3(.2f,.7f,.2f),VoidToyPalette.Cyan,node.body);}
        void RefreshObjectiveMarkers()
        {foreach(var pair in objectiveNodes)if(pair.Key.warning) {var line=pair.Key.warning.GetComponent<LineRenderer>();line.sharedMaterial=RouteIconMaterial();line.startColor=line.endColor=Mission.special!=SpecialRule.ChainReactor||pair.Value==nextObjectiveNode?VoidToyPalette.Cyan:VoidToyPalette.Muted;}}
        void ExpandedEnemyEliminated(Foe foe)
        {
            if(foe==expandedBoss) {foreach(var clone in bossDecoys)if(foes.Remove(clone)) {advancedBrains.Remove(clone);teleportDestinations.Remove(clone);if(clone.warning)Destroy(clone.warning.gameObject);if(clone.body)Destroy(clone.body.gameObject);}bossDecoys.Clear();}
            if(objectiveNodes.TryGetValue(foe,out int order)) {objectiveNodes.Remove(foe);if(Mission.special==SpecialRule.ChainReactor) {nextObjectiveNode=order+1;RefreshObjectiveMarkers();}}
            if(HasExpandedBoss&&foe!=expandedBoss)TryCompleteExpandedBoss();
        }
        void AimAtExpandedObject(ref float nearest)
        {
            if(!Expanded||!HasExpandedBoss||Mission.boss!=BossIdentity.FurnaceKing||stage!=1)return;
            currentTarget=null;goldTarget=null;
            foreach(var barrel in barrels)if(barrel.body&&barrel.fuse<0&&!CoverHit(player.position,barrel.body.position,.09f,out _))
            {float distance=(barrel.body.position-player.position).sqrMagnitude;if(((int)Weapon.kind<4||distance<=Mathf.Pow(Weapon.EffectiveRange*(1+MetaRange),2))&&distance<nearest) {nearest=distance;barrelTarget=barrel;}}
        }
        string ExpandedObjectiveText(string fallback)
        {
            if(HasExpandedBoss)return ExpandedBossObjective;
            if(Mission.special==SpecialRule.ChainReactor)return "ENERJİ DÜĞÜMLERİ · "+nextObjectiveNode+"/3";
            if(survivalWavePending)return "SON DALGA GELİYOR · "+Mathf.CeilToInt(4-specialClock);
            return fallback+(temporaryAmp>0?" · HASAR +%30":"")+(temporaryMagnet>0?" · MIKNATIS":"");
        }
    }
}
