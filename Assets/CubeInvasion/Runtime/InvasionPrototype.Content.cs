using UnityEngine;
using DG.Tweening;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("P2 content (empty keeps the P0/P1 playground)")]
        [SerializeField] P2ContentCatalog content;
        [SerializeField] P2MissionHud missionHud;
        int missionIndex, weaponIndex, quotaGold;
        float bossHealth, bossTotal;
        bool contentReady;
        bool HasContent => content && content.missions!=null && content.missions.Length>0;
        MissionSpec Mission => content.missions[missionIndex];
        WeaponSpec Weapon => content.weapons[weaponIndex];
        EncounterSpec Encounter => HasBranches&&activeOffer!=null&&activeOffer.stage==stage?activeOffer.encounter:Mission.cubes[Mathf.Clamp(stage-1,0,Mission.cubes.Length-1)];
        int CubeCount => HasContent?Mission.cubes.Length:3;
        bool IsGoldRush => HasContent && Mission.mode==MissionMode.GoldRush;
        bool IsBoss => HasContent && Mission.mode==MissionMode.Boss;
        float FireInterval => HasContent?Weapon.interval/((1+MetaRate)*(1+RouteRate)):.5f;
        public void ConfigureContent(P2ContentCatalog catalog,P2MissionHud presentation)
        { content=catalog;missionHud=presentation; }
        void InitializeContent()
        {
            if(!HasContent)return;
            contentReady=true;
            hud.SetCampaignMode(true);
            if(!missionHud)missionHud=new GameObject("P2 Mission HUD",typeof(RectTransform)).AddComponent<P2MissionHud>();
            missionHud.gameObject.SetActive(true);
            missionHud.Initialize(hud.SafeRoot);
            feedback.SetMusic(MissionMode.Combat);
            missionHud.Bind(ChangeMission,ChangeWeapon,()=>StartMission(missionIndex),
                ()=>StartMission(missionIndex),()=>StartMission(missionIndex+1),OpenMissionMenu);
            phase=Phase.Loadout;
            InitializeMeta();
            InitializeFrontEnd();
            RefreshContentHud();
        }
        void ChangeMission(int delta)
        {
            if(phase!=Phase.Loadout)return;
            int index=(missionIndex+delta+content.missions.Length)%content.missions.Length;
            if(HasMeta&&index<=profile.Data.highestMission&&!profile.Select(index,weaponIndex)) { metaNotice=profile.Message;RefreshContentHud();return; }
            missionIndex=index;metaNotice=null;RefreshContentHud();
        }
        void ChangeWeapon(int index)
        {
            if(phase!=Phase.Loadout)return;index=Mathf.Clamp(index,0,content.weapons.Length-1);
            inspectedWeapon=index;metaNotice=null;
            if(!HasMeta)weaponIndex=index;
            else if(profile.IsOwned(index)) { if(profile.Select(Mathf.Min(missionIndex,profile.Data.highestMission),index))weaponIndex=index;else metaNotice=profile.Message; }
            RefreshContentHud();
        }
        void OpenMissionMenu() { if(phase==Phase.Combat||phase==Phase.Transfer)return;SetPaused(false);phase=Phase.Loadout;feedback.SetMusic(MissionMode.Combat);RefreshContentHud(); }
        void StartMission(int index)
        {
            if(!HasContent)return;
            if(HasMeta && phase!=Phase.Loadout&&phase!=Phase.Won&&phase!=Phase.Failed)return;
            if(!BeginMetaRun(index))return;
            BeginValidationRun(index);
            StopAllCoroutines();ClearAnimationTweens();ClearPulses();ClearEnemyShots();feedback.ClearMuzzles();
            foreach(Transform child in dynamicRoot)Destroy(child.gameObject);
            foes.Clear();barrels.Clear();pickups.Clear();pulses.Clear();enemyShots.Clear();pulsePool.Clear();enemyShotPool.Clear();
            cover=null;rightGate=null;forwardGate=null;rightBarrier=null;forwardBarrier=null;rightPreview=null;forwardPreview=null;
            missionIndex=Mathf.Clamp(index,0,content.missions.Length-1);
            ResetRouteRun();centre=Vector3.zero;stage=1;health=MaxHealth;remaining=20;pendingGold=0;quotaGold=0;
            bossHealth=0;bossTotal=0;damageCubes=0;damageBonus=0;reason="";routesReady=false;settledPreviews=0;
            SetPaused(false);player.position=new Vector3(0,.65f,-2.5f);player.rotation=Quaternion.identity;
            if(playerAnimation)playerAnimation.SetVisible(true);
            EquipWeapon();
            feedback.SetMusic(Mission.mode);
            platform=Arena(centre,false,VoidToyPalette.Cyan);
            view.backgroundColor=Color.Lerp(VoidToyPalette.Void,Mission.accent,.04f);
            BeginCombat(null);
            if(CubeCount>1)PrepareNextCubes(centre,platform);
            RefreshContentHud();
        }
        void EquipWeapon()
        {
            // The authored muzzle stays attached to the same player rig.
            if(!artLibrary.TryGet(Weapon.model,out var model))return;
            gun.GetComponent<MeshFilter>().sharedMesh=model.mesh;
            gun.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(model.surface?null:Mat(Weapon.color));
            gun.name="Pulse pistol"; // Stable authored reference; model comes from the selected weapon.
        }
        void SpawnContentEncounter()
        {
            quotaGold=0;
            if(IsGoldRush)
            {
                // Reachable interior grid; no mandatory pickup can appear on an edge.
                int count=Mathf.Min(16,Encounter.goldQuota/5+4);
                for(int i=0;i<count;i++)
                {
                    var position=centre+new Vector3(-2.7f+(i%4)*1.8f,.5f,-2.7f+(i/4)*1.8f);
                    SpawnPickup(position,false,5);
                }
            }
            var enemies=Encounter.enemies;
            if(enemies!=null)for(int i=0;i<enemies.Length;i++)
            {
                // Opposing side lanes keep melee and ranged pressure from forming one straight queue.
                float x=i==0?-2.8f:i==1?2.8f:i==2?-.9f:i==3?2.6f:i==4?-2.6f:.9f;
                float z=i==0?1.2f:i==1?2.1f:i==2?3.2f:i==3?-.2f:i==4?-.2f:3.1f;
                if(enemies[i]==ContentEnemy.Crusher) { x=0;z=1.8f; }
                var foe=SpawnContentEnemy(enemies[i],SafeBranchSpawn(centre+new Vector3(x,.6f,z),enemies[i]));
                ConfigureRouteEnemy(foe,i);
            }
            if(IsBoss)
            {
                if(bossTotal==0)
                {
                    foreach(var cube in Mission.cubes)foreach(var kind in cube.enemies)if(kind==ContentEnemy.Crusher)bossTotal+=BossPhaseHealth(System.Array.IndexOf(Mission.cubes,cube)+1);
                    bossHealth=bossTotal;
                }
                feedback.Play(CombatFeedback.Cue.BossPhase);
            }
        }
        static float BossPhaseHealth(int phaseIndex) => phaseIndex==1?72:phaseIndex==2?84:96;
        Foe SpawnContentEnemy(ContentEnemy kind,Vector3 position)
        {
            var enemy=(EnemyKind)(int)kind;
            string key=kind.ToString();
            // Invaders share one graphite body family; the intent light (pink melee, orange ranged) marks the threat.
            Color color=VoidToyPalette.Intent(kind);
            Color shell=Color.Lerp(VoidToyPalette.Graphite,color,kind==ContentEnemy.Crusher?.22f:.3f);
            float hp=kind==ContentEnemy.Grunt?18:kind==ContentEnemy.Sprinter?12:kind==ContentEnemy.Gunner?21:kind==ContentEnemy.Pusher?24:kind==ContentEnemy.Sniper?14:kind==ContentEnemy.Bruiser?45:BossPhaseHealth(stage);
            float speed=kind==ContentEnemy.Sprinter?2.2f:kind==ContentEnemy.Bruiser?.7f:kind==ContentEnemy.Crusher?.5f:1.05f;
            Vector3 scale=kind==ContentEnemy.Crusher?Vector3.one*1.05f:kind==ContentEnemy.Bruiser?Vector3.one*.85f:Vector3.one*.7f;
            if(kind==ContentEnemy.Crusher)position.y=.95f;
            var body=Shape(key,PrimitiveType.Cube,position,scale,shell,world);
            Transform crest=null,cannon=null;
            if(kind==ContentEnemy.Grunt||kind==ContentEnemy.Sprinter||kind==ContentEnemy.Gunner)
                crest=Shape("Threat crest",PrimitiveType.Cube,new Vector3(0,.65f,0),new Vector3(.18f,.4f,.18f),color,body);
            if(kind==ContentEnemy.Gunner)cannon=Shape("Gunner barrel",PrimitiveType.Cube,new Vector3(0,0,.7f),new Vector3(.25f,.25f,.9f),Color.white,body);
            var animation=AnimateActor(body,body,key,crest,cannon);if(animation)animation.Spawn();
            var warning=WarningRing(position,kind==ContentEnemy.Crusher?2.5f:kind==ContentEnemy.Pusher?2.1f:1.5f,color);warning.gameObject.SetActive(false);
            var foe=new Foe {body=body,hp=hp,speed=speed,attack=.6f,kind=enemy,warning=warning,color=color,
                animation=animation,visual=animation?animation.CoreRenderer:body.GetComponent<Renderer>()};
            foes.Add(foe);feedback.Burst(position,color,.8f,.35f,true);return foe;
        }
        bool ObjectiveComplete => IsGoldRush?quotaGold>=Encounter.goldQuota:foes.Count==0;
        void RefreshContentHud()
        {
            if(!HasContent||!contentReady||!missionHud)return;
            string objective=IsGoldRush?$"ALTIN {quotaGold}/{Encounter.goldQuota}":IsBoss?$"FAZ {stage}/3 · {Mathf.CeilToInt(bossHealth)} CAN":$"{foes.Count} HEDEF";
            hud.SetMissionInfo(stage,CubeCount,objective,IsBoss&&phase==Phase.Combat);
            missionHud.SetBoss(bossHealth,bossTotal,IsBoss&&phase==Phase.Combat,stage);
            missionHud.Refresh(Mission,HasMeta&&phase==Phase.Loadout?content.weapons[inspectedWeapon]:Weapon,missionIndex,content.missions.Length,weaponIndex,phase.ToString(),reason,wallet);
            RefreshMetaHud();
            RefreshFrontEnd();
        }
        bool TickSpecialEnemy(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(foe.kind==EnemyKind.Grunt||foe.kind==EnemyKind.Sprinter||foe.kind==EnemyKind.Gunner)return false;
            if(foe.windup>0)
            {
                foe.windup-=dt;
                if(foe.windup<=0)
                {
                    foe.warning.gameObject.SetActive(false);
                    if(foe.animation)
                    {
                        if(foe.kind==EnemyKind.Bruiser||foe.kind==EnemyKind.Crusher)foe.animation.Slam();
                        else foe.animation.Shoot();
                    }
                    if(foe.kind==EnemyKind.Sniper)FireEnemyShot(foe.body.position,foe.aim,9*foe.attackPower,13);
                    else if(foe.kind==EnemyKind.Pusher)
                    {
                        var offset=player.position-foe.body.position;offset.y=0;
                        if(offset.magnitude<=2.5f && Vector3.Dot(offset.normalized,foe.aim)>.35f)DamagePlayer(5*EnemyDamageMultiplier*foe.attackPower,foe.aim*9);
                        feedback.Burst(foe.body.position,new Color(1,.6f,.2f),2.3f,.25f,true);
                    }
                    else if(foe.kind==EnemyKind.Bruiser)
                    {
                        if(FlatDistance(player.position,foe.body.position)<=1.65f)DamagePlayer(13*EnemyDamageMultiplier*foe.attackPower,foe.aim*3);
                        feedback.Burst(foe.body.position,danger,1.5f,.25f,true);
                    }
                    else
                    {
                        if(FlatDistance(player.position,foe.body.position)<2.5f)DamagePlayer(10*EnemyDamageMultiplier*foe.attackPower,foe.aim*4);
                        if(stage>1)
                        {
                            bool radial=stage==3 && foe.pattern%2==0;int shots=radial?12:stage==2?7:9;
                            for(int i=0;i<shots;i++)FireEnemyShot(foe.body.position,Quaternion.Euler(0,radial?i*360f/shots:(i-(shots-1)*.5f)*18,0)*foe.aim,7*foe.attackPower,5);
                        }
                        foe.pattern++;
                        feedback.Play(CombatFeedback.Cue.BossSlam);feedback.Burst(foe.body.position,danger,2.5f,.3f,true);
                    }
                }
                return true;
            }
            float range=foe.kind==EnemyKind.Sniper?12:foe.kind==EnemyKind.Pusher?3:foe.kind==EnemyKind.Crusher?(stage==1?2.8f:8):1.7f;
            if(foe.attack<=0 && distance<range&&!CoverHit(foe.body.position,player.position,.1f,out _))
            {
                foe.aim=toward.sqrMagnitude>.01f?toward.normalized:Vector3.forward;
                foe.windup=foe.kind==EnemyKind.Sniper?1.1f:foe.kind==EnemyKind.Pusher?.85f:foe.kind==EnemyKind.Crusher?1.2f:.8f;
                foe.attack=foe.windup+(foe.kind==EnemyKind.Crusher?1.8f:foe.kind==EnemyKind.Sniper?2.3f:1.6f)*EnemyRecoveryMultiplier;
                foe.body.rotation=Quaternion.LookRotation(foe.aim);foe.warning.gameObject.SetActive(true);
                if(foe.animation)foe.animation.Telegraph(foe.windup);
                feedback.Play(CombatFeedback.Cue.Telegraph);
                var line=foe.warning.GetComponent<LineRenderer>();
                if(foe.kind==EnemyKind.Sniper)
                { line.loop=false;line.useWorldSpace=true;line.positionCount=2;line.SetPosition(0,foe.body.position);line.SetPosition(1,foe.body.position+foe.aim*14); }
                else if(foe.kind==EnemyKind.Pusher)
                {
                    line.loop=true;line.useWorldSpace=true;line.positionCount=4;
                    var origin=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
                    line.SetPosition(0,origin);line.SetPosition(1,origin+Quaternion.Euler(0,-65,0)*foe.aim*2.5f);
                    line.SetPosition(2,origin+foe.aim*2.5f);line.SetPosition(3,origin+Quaternion.Euler(0,65,0)*foe.aim*2.5f);
                }
                else if(foe.kind==EnemyKind.Crusher && stage>1)
                {
                    bool radial=stage==3 && foe.pattern%2==0;int rays=radial?12:stage==2?7:9;
                    line.loop=false;line.useWorldSpace=true;line.positionCount=rays*2+1;
                    var origin=new Vector3(foe.body.position.x,.1f,foe.body.position.z);line.SetPosition(0,origin);
                    for(int i=0;i<rays;i++)
                    {
                        var ray=Quaternion.Euler(0,radial?i*360f/rays:(i-(rays-1)*.5f)*18,0)*foe.aim;
                        line.SetPosition(i*2+1,origin+ray*6);line.SetPosition(i*2+2,origin);
                    }
                }
                else foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
            }
            else if(CoverHit(foe.body.position,player.position,.1f,out _)||distance>(foe.kind==EnemyKind.Sniper?4.5f:foe.kind==EnemyKind.Crusher?(stage==1?1.8f:3):1.1f))MoveEnemy(foe,toward.normalized*foe.speed*dt);
            return true;
        }
        void FireContentWeapon()
        {
            if(!AimAtNearestEnemy())return;
            if(playerAnimation)playerAnimation.Shoot();
            var spec=Weapon;
            for(int i=0;i<spec.pellets;i++)
            {
                var origin=gun.position+player.forward*.35f;
                if(spec.kind==ContentWeapon.TwinBlasters)origin+=player.right*(i==0?-.12f:.12f);
                origin=SafeMuzzle(origin);
                var aim=currentTarget.body.position-origin;aim.y=0;
                var direction=aim.sqrMagnitude>.0001f?aim.normalized:player.forward;
                float angle=spec.pellets<=1?0:Mathf.Lerp(-spec.spread*.5f,spec.spread*.5f,i/(float)(spec.pellets-1));
                var body=RentProjectile(pulsePool,"Forward pulse",origin,spec.color,.18f);
                direction=Quaternion.Euler(0,angle,0)*direction;body.GetComponent<LaserBoltVisual>().Fly(direction,0);
                pulses.Add(new Pulse {body=body,direction=direction,life=spec.projectileLife*(1+MetaRange),damage=spec.damage*(1+MetaDamage)*(1+(damageCubes>0?damageBonus:0))});
            }
            feedback.Play(spec.kind==ContentWeapon.ScatterShot?CombatFeedback.Cue.Scatter:spec.kind==ContentWeapon.RapidSmg?CombatFeedback.Cue.Rapid:spec.kind==ContentWeapon.TwinBlasters?CombatFeedback.Cue.Twin:CombatFeedback.Cue.Shot);
            PlayerMuzzle(spec.color,spec.kind==ContentWeapon.TwinBlasters,spec.kind==ContentWeapon.ScatterShot?1.4f:1);
        }
    }
}
