using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        enum EnemyKind { Grunt, Sprinter, Gunner, Pusher, Sniper, Bruiser, Crusher, Bomber, Shielder, Healer, Summoner, Phaser, Elite, ObjectiveNode }
        sealed class Barrel { public Transform body, warning; public float hp=12, fuse=-1; }
        sealed class Pickup { public Transform body; public bool healing; public int value; public Vector3 origin; public float age; }
        sealed class EnemyShot { public Transform body; public Vector3 direction; public float life=2, damage=6, speed=7; }
        readonly List<Barrel> barrels=new List<Barrel>();
        readonly List<Pickup> pickups=new List<Pickup>();
        readonly List<EnemyShot> enemyShots=new List<EnemyShot>();
        readonly Stack<Transform> pulsePool=new Stack<Transform>(), enemyShotPool=new Stack<Transform>();
        readonly Dictionary<Color,Material> materialCache=new Dictionary<Color,Material>();
        [SerializeField] CombatFeedback feedback;
        [SerializeField] MobileCombatHud hud;
        Transform cover;
        Vector3 knockback;
        int pendingGold, alarmSecond=21;
        float hurtFlash;
        MaterialPropertyBlock hitProperties;

        void InitializeFeel()
        {
            Application.targetFrameRate=60;
            hitProperties=new MaterialPropertyBlock();
            if(!feedback) { feedback=new GameObject("Combat feedback").AddComponent<CombatFeedback>();feedback.transform.SetParent(world,false); }
            feedback.Initialize();
            if(!hud) { hud=new GameObject("Mobile Combat HUD",typeof(RectTransform)).AddComponent<MobileCombatHud>();hud.transform.SetParent(world,false); }
            hud.Initialize(view,()=>SetPaused(!paused),Retry,()=>feedback.ToggleMute());
        }
        void SetPaused(bool value) { if(phase==Phase.SaveBlocked&&!value&&retrySave!=null)return;paused=value;dragging=false;joystickFinger=-1;stick=Vector2.zero;if(hud)hud.ResetJoystick();if(feedback)feedback.SetPaused(value);if(value)ResetCountdownPresentation(); }
        void Retry() { if(HasContent) { StartMission(missionIndex);return; } UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); }
        void LateUpdate()
        {
            FlushMenuSettings();
            TickHitStop();
            TickAnimations(Time.deltaTime);
            if(!paused)TickHeroFace(Time.unscaledDeltaTime);
            if(!hud)return;
            feedback.SetPaused(paused);
            if(!paused)feedback.Tick(Time.deltaTime);
            hud.SetMaxHealth(MaxHealth);
            if(player)hud.SetPlayerAnchor(player.position+Vector3.up*1.35f);
            hud.Refresh(remaining,health,stage,foes.Count,wallet,pendingGold,damageCubes,damageBonus,paused,feedback.Muted,phase.ToString(),reason);
            hud.SetFireRhythm(FireMovementWeight,HasAimTarget,phase==Phase.Combat&&!paused);
            RefreshContentHud();
            feedback.SetPressure(Expanded&&phase==Phase.Combat&&!paused?1-Mathf.Clamp01(remaining/5):0);
            TickOnboarding();
            RefreshRouteHud();
            RefreshCrusherLinks();
            RefreshCountdownHud();
        }
        void ClearPickups()
        {
            foreach(var pickup in pickups)RecyclePickup(pickup);
            pickups.Clear();
        }
        void SetupEncounterFeel()
        {
            pendingGold=0;alarmSecond=21;knockback=Vector3.zero;moveVelocity=Vector3.zero;
            if(cover)Destroy(cover.gameObject);cover=null;
            barrels.Clear();ClearPickups();ClearEnemyShots();
            if(HasContent?Encounter.cover||Encounter.barrels:stage>=2)
            {
                if(!HasContent||Encounter.barrels)
                {
                    CreateBarrel(centre+new Vector3(-2.1f,.5f,2.1f));
                    CreateBarrel(centre+new Vector3(2.1f,.5f,2.1f));
                }
                if(!HasContent||Encounter.cover)
                {
                cover=Shape("Low cover",PrimitiveType.Cube,new Vector3(0,.45f,.6f),new Vector3(1.35f,.9f,.8f),new Color(.25f,.34f,.46f),platform);
                // Shape uses local coordinates; the cover is attached to its own cube.
                var offset=HasContent?Encounter.coverOffset:new Vector2(0,.6f);
                cover.position=centre+new Vector3(offset.x,.45f,offset.y);
                }

            }
            else cover=null;
            if(!IsGoldRush&&HasContent&&Encounter.healing)SpawnPickup(centre+new Vector3(Encounter.healOffset.x,.45f,Encounter.healOffset.y),true,20);
        }
        void CreateBarrel(Vector3 at)
        {
            var body=Shape("Explosive barrel",PrimitiveType.Cylinder,at,new Vector3(.55f,.55f,.55f),new Color(1,.38f,.16f),world);
            Shape("Explosive band",PrimitiveType.Cube,new Vector3(0,.2f,0),new Vector3(1.08f,.2f,1.08f),danger,body);
            var warning=WarningRing(body.position,2.2f,danger);warning.gameObject.SetActive(false);
            barrels.Add(new Barrel {body=body,warning=warning});
        }
        Transform WarningRing(Vector3 at,float radius,Color color)
        {
            var root=new GameObject("Attack warning").transform;root.SetParent(dynamicRoot?dynamicRoot:world,false);root.position=new Vector3(at.x,.09f,at.z);
            var line=root.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=Mat(color);line.useWorldSpace=false;line.loop=true;line.widthMultiplier=.045f;line.positionCount=40;
            for(int i=0;i<40;i++) {float a=i*Mathf.PI*2/40;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
            return root;
        }
        void SpawnPickup(Vector3 at,bool healing,int value)
        {
            var color=healing?new Color(.09f,.8f,.57f):new Color(1,.72f,.2f);
            var pool=healing?healthPool:coinPool;Transform body=null;
            while(Expanded&&pool.Count>0&&!body)body=pool.Pop();
            bool reused=body;
            if(!body)body=Shape(healing?"Health +20":"Gold pickup",healing?PrimitiveType.Cube:PrimitiveType.Cylinder,at,healing?Vector3.one*.35f:new Vector3(.28f,.07f,.28f),color,world);
            else {body.SetParent(dynamicRoot,false);body.position=at;body.gameObject.SetActive(true);}
            if(healing&&!reused) { Shape("Health cross H",PrimitiveType.Cube,new Vector3(0,.6f,0),new Vector3(1,.16f,.3f),Color.white,body);Shape("Health cross V",PrimitiveType.Cube,new Vector3(0,.6f,0),new Vector3(.3f,.16f,1),Color.white,body); }
            pickups.Add(new Pickup {body=body,healing=healing,value=value,origin=at});
            AnimatePickup(body);
        }
        void CollectPickups(float dt,bool magnet)
        {
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];pickup.age+=dt;
                if(!pickup.body) { pickups.RemoveAt(i);continue; }
                if(pickup.healing&&health>=MaxHealth&&!magnet)continue;
                float distance=Vector3.Distance(player.position,pickup.body.position);
                if((magnet&&!pickup.healing) || distance<PickupRadius*(pickup.healing?1:temporaryMagnet>0?3:1+RouteMagnet))
                {
                    if(pickup.healing) { health=Mathf.Min(MaxHealth,health+pickup.value);feedback.Play(CombatFeedback.Cue.Heal); }
                    else { pendingGold+=pickup.value;if(IsGoldRush)quotaGold=Mathf.Min(Encounter.goldQuota,quotaGold+pickup.value);feedback.Play(CombatFeedback.Cue.Gold);Face(FaceHappy,.25f,2); }
                    feedback.Burst(pickup.body.position,pickup.healing?Color.green:Color.yellow,.5f);
                    RecyclePickup(pickup);pickups.RemoveAt(i);
                }
                else
                {
                    // DOTween owns pickup hover and rotation; collection only reads the position.
                }
            }
        }
        void HitEnemy(Foe foe,float damage,Vector3 direction)=>HitEnemyAt(foe,damage,direction,-1);
        void HitEnemyAt(Foe foe,float damage,Vector3 direction,int panel)=>ApplyEnemyDamage(foe,damage,direction,panel,false);
        void HitEnemyArea(Foe foe,float damage,Vector3 direction)=>ApplyEnemyDamage(foe,damage,direction,-1,true);
        void ApplyEnemyDamage(Foe foe,float damage,Vector3 direction,int panel,bool area)
        {
            if(!foes.Contains(foe)||!CrusherTargetable(foe))return;
            damage=ResolveExpandedBossHit(foe,AdvancedDamage(foe,ResolveCrusherHit(foe,ShieldDamage(foe,Mathf.Max(0,damage),direction,area),panel)));if(damage<=0)return;
            float applied=Mathf.Min(foe.hp,damage);
            foe.hp-=damage;foe.flash=.09f;
            if(HasContent && foe.kind==EnemyKind.Crusher)bossHealth=Mathf.Max(0,bossHealth-applied);
            SyncCrusherArmorHealth(foe);
            CrusherDroneDamaged(foe,applied);
            if(foe.animation)foe.animation.Hit();
            feedback.Play(CombatFeedback.Cue.Hit);feedback.Burst(foe.body.position,Color.white,.3f);
            if(foe.hp>0)return;
            EliminateEnemy(foe);
        }
        void EliminateEnemy(Foe foe)
        {
            if(!foes.Contains(foe))return;
            if(HasMeta)runKills++;
            var at=foe.body.position;
            if(foe.warning)Destroy(foe.warning.gameObject);
            if(!RecycleActor(foe,true)) {AnimateDeath(foe);Destroy(foe.body.gameObject);}foes.Remove(foe);
            bool outside=Expanded&&(Mathf.Abs(at.x-centre.x)>ArenaHalfX||Mathf.Abs(at.z-centre.z)>Half);
            SpawnPickup(outside?player.position:at,false,EnemyGold(foe));
            feedback.Burst(new Vector3(at.x,.1f,at.z),danger,.9f,.25f,true);
            var away=at-player.position;away.y=0;KillImpact(at,away);
            ExpandedEnemyEliminated(foe);advancedBrains.Remove(foe);teleportDestinations.Remove(foe);CrusherEnemyEliminated(foe);GoldEnemyEliminated(foe);BomberEliminated(foe,at);SupportEliminated(foe);
        }
        void DamagePlayer(float damage,Vector3 push)
        {
            if(phase!=Phase.Combat||invulnerability>0)return;
            float absorbed=Mathf.Min(routeShield,Mathf.Max(0,damage));routeShield-=absorbed;damage-=absorbed;
            cubeDamageTaken+=Mathf.Min(health,Mathf.Max(0,damage));
            health=Mathf.Max(0,health-damage);invulnerability=.55f;hurtFlash=.18f;knockback=push;
            if(playerAnimation)playerAnimation.Hit();
            feedback.Play(CombatFeedback.Cue.Hurt);feedback.Burst(player.position,danger,.8f,.2f,true);HurtImpact(push);
            if(health<=0)Fail("Canın tükendi");
        }
        void TickEnemies(float dt)
        {
            enemyTickBuffer.Clear();enemyTickBuffer.AddRange(foes);
            foreach(var foe in enemyTickBuffer)
            {
                if(!foes.Contains(foe))continue;
                float originalSpeed=foe.speed;foe.slow=Mathf.Max(0,foe.slow-dt);
                if(foe.slow>0)foe.speed*=foe.kind==EnemyKind.Crusher?.8f:.55f;
                TickEnemyFrame(foe,dt);foe.speed=originalSpeed;
            }
            FlushSummons();
        }
        void TickEnemyFrame(Foe foe,float dt)
        {
            if(Expanded&&foe.kind!=EnemyKind.Crusher&&(Mathf.Abs(foe.body.position.x-centre.x)>ArenaHalfX+.25f||Mathf.Abs(foe.body.position.z-centre.z)>Half+.25f)) {EliminateEnemy(foe);return;}
            {
                if(foe.flash>0)
                {
                    foe.flash=Mathf.Max(0,foe.flash-dt);
                    if(foe.flash>0) { hitProperties.SetColor("_Color",Color.white);foe.visual.SetPropertyBlock(hitProperties); }
                    else foe.visual.SetPropertyBlock(null);
                }
                if(foe.spawn>0) { foe.spawn-=dt;return; }
                foe.attack-=dt;
                var toward=player.position-foe.body.position;toward.y=0;float distance=toward.magnitude;
                if(TickSpecialEnemy(foe,dt,toward,distance))return;
                if(foe.windup>0)
                {
                    foe.windup-=dt;
                    if(foe.kind!=EnemyKind.Gunner)foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
                    if(foe.windup<=0)
                    {
                        foe.warning.gameObject.SetActive(false);
                        if(foe.kind==EnemyKind.Sprinter) { foe.dash=.32f;foe.dashHit=false; }
                        else if(foe.kind==EnemyKind.Gunner) { foe.burst=3;foe.burstClock=0; }
                        else if(distance<1.5f)DamagePlayer(5*EnemyDamageMultiplier*foe.attackPower,foe.aim*1.2f);
                    }
                    return;
                }
                if(foe.dash>0)
                {
                    Vector3 previous=foe.body.position;
                    MoveEnemy(foe,foe.aim*7.5f*dt);
                    if(!foe.dashHit && SegmentDistance(player.position,previous,foe.body.position)<.8f)
                    { DamagePlayer(7*EnemyDamageMultiplier*foe.attackPower,foe.aim*3);foe.dashHit=true; }
                    foe.dash-=dt;return;
                }
                if(foe.burst>0)
                {
                    foe.burstClock-=dt;
                    if(foe.burstClock<=0) { if(foe.animation)foe.animation.Shoot();FireEnemyShot(foe.body.position,foe.aim,6*foe.attackPower);foe.burst--;foe.burstClock=.13f; }
                    return;
                }
                float range=foe.kind==EnemyKind.Grunt?1.3f:foe.kind==EnemyKind.Sprinter?4.3f:7;
                if(foe.attack<=0 && distance<range&&!CoverHit(foe.body.position,player.position,.1f,out _))
                {
                    foe.aim=toward.sqrMagnitude>.01f?toward.normalized:Vector3.forward;
                    foe.windup=foe.kind==EnemyKind.Grunt?.5f:foe.kind==EnemyKind.Sprinter?.65f:1.0f;
                    foe.attack=foe.windup+(foe.kind==EnemyKind.Grunt?1.1f:1.8f)*EnemyRecoveryMultiplier;
                    if(foe.animation)foe.animation.Telegraph(foe.windup);
                    foe.warning.gameObject.SetActive(true);
                    foe.body.rotation=Quaternion.LookRotation(foe.aim);
                    if(foe.kind!=EnemyKind.Grunt)
                    {
                        var line=foe.warning.GetComponent<LineRenderer>();line.loop=false;line.useWorldSpace=true;line.positionCount=2;
                        line.SetPosition(0,foe.body.position);line.SetPosition(1,foe.body.position+foe.aim*(foe.kind==EnemyKind.Gunner?8:2.4f));
                    }
                }
                else if(CoverHit(foe.body.position,player.position,.1f,out _)||distance>(foe.kind==EnemyKind.Gunner?3.8f:.85f))MoveEnemy(foe,toward.normalized*foe.speed*dt);
            }
        }
        void MoveEnemy(Foe foe,Vector3 delta)
        {
            delta*=EnemyMoveMultiplier;
            // Committed dashes retain their telegraphed direction; ordinary pursuit routes around cover.
            if(foe.dash<=0&&delta.sqrMagnitude>.000001f)
            {
                var waypoint=EnemyWaypoint(foe.body.position,player.position);var toward=waypoint-foe.body.position;toward.y=0;
                delta=toward.normalized*Mathf.Min(delta.magnitude,toward.magnitude);
            }
            Vector3 next=foe.body.position+delta;
            if(CoverHit(foe.body.position,next,.3f,out _))return;
            var relative=next-centre;relative.x=Mathf.Clamp(relative.x,-ArenaHalfX+.5f,ArenaHalfX-.5f);relative.z=Mathf.Clamp(relative.z,-4,4);foe.body.position=centre+relative;
        }
        void TickWorldFeel(float dt)
        {
            if(hurtFlash>0)
            {
                hurtFlash=Mathf.Max(0,hurtFlash-dt);
                bool visible=hurtFlash<=0||((int)(hurtFlash*40)%2)==0;
                if(playerAnimation)playerAnimation.SetVisible(visible);else player.GetChild(0).GetComponent<Renderer>().enabled=visible;
            }
            Vector3 before=player.position;player.position+=knockback*dt*(Expanded&&Mission.special==SpecialRule.LowGravity?1.5f:1);knockback=Vector3.Lerp(knockback,Vector3.zero,dt*10);
            if(CoverHit(before,player.position,.3f,out _))player.position=before;
            for(int i=barrels.Count-1;i>=0;i--)
            {
                var barrel=barrels[i];if(barrel.fuse<0)continue;
                barrel.fuse-=dt;
                if(barrel.fuse<=0) { Explode(barrel);if(phase!=Phase.Combat)return; }
            }
            TickDeathCharges(dt);if(phase!=Phase.Combat)return;
            TickEnemyShots(dt);if(phase!=Phase.Combat)return;CollectPickups(dt,false);
            int second=Mathf.CeilToInt(remaining);
            if(second<=5&&second>0&&second!=alarmSecond) { alarmSecond=second;feedback.Burst(centre+Vector3.up*.1f,danger,4.4f,.3f,true); }
        }
        void HitBarrel(Barrel barrel,float damage)
        {
            if(barrel.fuse>=0)return;
            WorldTween(barrel.body.DOPunchScale(Vector3.one*.035f,.18f,3,.4f)).SetLink(barrel.body.gameObject);
            barrel.hp-=damage;feedback.Burst(barrel.body.position,Color.yellow,.5f);
            if(barrel.hp<=0) { barrel.fuse=.45f;barrel.warning.gameObject.SetActive(true);feedback.Play(CombatFeedback.Cue.Alarm); }
        }
        void Explode(Barrel barrel)
        {
            ExpandedBarrelExploded(barrel.body.position);
            Vector3 at=barrel.body.position;barrels.Remove(barrel);
            Destroy(barrel.body.gameObject);Destroy(barrel.warning.gameObject);
            feedback.Play(CombatFeedback.Cue.Explosion);feedback.Burst(new Vector3(at.x,.12f,at.z),new Color(1,.45f,.1f),2.2f,.3f,true);
            feedback.Shatter(at+Vector3.up*.3f,new Color(1,.38f,.16f),VoidToyPalette.Gold,Vector3.zero,9);Impact(BlastStop,1,Vector3.up);
            // Chained barrels receive their own visible fuse.
            // A final support kill can retire the protected boss too; visit each original victim once.
            var victims=foes.ToArray();
            for(int i=victims.Length-1;i>=0;i--)if(foes.Contains(victims[i])&&FlatDistance(victims[i].body.position,at)<=2.2f)HitEnemyArea(victims[i],30*(1+RouteBlast),(victims[i].body.position-at).normalized);
            var crates=goldCrates.ToArray();foreach(var crate in crates)if(crate.body&&FlatDistance(crate.body.position,at)<=2.2f)HitGoldCrate(crate,30*(1+RouteBlast));
            foreach(var other in barrels)if(FlatDistance(other.body.position,at)<=2.2f)HitBarrel(other,100);
            if(FlatDistance(player.position,at)<=2.2f)DamagePlayer(10*(1+RouteBlast),(player.position-at).normalized*5);
        }
        static float FlatDistance(Vector3 a,Vector3 b) { a.y=b.y=0;return Vector3.Distance(a,b); }
        static float SegmentDistance(Vector3 point,Vector3 start,Vector3 end)
        {
            point.y=start.y=end.y=0;var segment=end-start;float t=segment.sqrMagnitude>.00001f?Mathf.Clamp01(Vector3.Dot(point-start,segment)/segment.sqrMagnitude):0;
            return Vector3.Distance(point,start+segment*t);
        }
        Transform RentProjectile(Stack<Transform> pool,string name,Vector3 at,Color color,float size)
        {
            Transform body;
            if(pool.Count>0)body=pool.Pop();
            else {body=new GameObject(name,typeof(LineRenderer),typeof(LaserBoltVisual)).transform;body.SetParent(dynamicRoot?dynamicRoot:world,false);}
            body.position=at;body.GetComponent<LaserBoltVisual>().ResetVisual(color,size);body.gameObject.SetActive(true);return body;
        }
        void FireEnemyShot(Vector3 at,Vector3 direction,float damage=6,float speed=7)
        {
            var body=RentProjectile(enemyShotPool,"Enemy pulse",at,new Color(1,.18f,.055f),.2f);body.GetComponent<LaserBoltVisual>().Fly(direction,0);
            enemyShots.Add(new EnemyShot {body=body,direction=direction,damage=damage,speed=speed});EnemyMuzzle(at,direction);feedback.Play(CombatFeedback.Cue.Shot);
        }
        void TickEnemyShots(float dt)
        {
            for(int i=enemyShots.Count-1;i>=0;i--)
            {
                var shot=enemyShots[i];var start=shot.body.position;var end=start+shot.direction*shot.speed*Mathf.Min(dt,Mathf.Max(0,shot.life));shot.body.position=end;shot.life-=dt;
                shot.body.GetComponent<LaserBoltVisual>().Fly(shot.direction,Vector3.Distance(start,end));
                bool blocked=CoverHit(start,end,.1f,out float wallT);
                bool hit=CircleHit(start,end,player.position,.5f,out float playerT)&&(!blocked||playerT<wallT);
                if(hit) { DamagePlayer(shot.damage*EnemyDamageMultiplier,shot.direction);if(phase!=Phase.Combat)return; }
                if(blocked||hit||shot.life<=0) { shot.body.gameObject.SetActive(false);enemyShotPool.Push(shot.body);enemyShots.RemoveAt(i); }
            }
        }
        void ClearEnemyShots() { foreach(var shot in enemyShots) { shot.body.gameObject.SetActive(false);enemyShotPool.Push(shot.body); }enemyShots.Clear(); }
        void FinishEncounterFeel(bool clear)
        {
            ClearEnemyShots();ClearWeaponBlasts();ClearExpandedArena();feedback.ClearMuzzles();ClearSupportEffects();ClearDeathCharges(clear);FinishGoldObjectives(clear);
            if(clear)CollectPickups(0,true);else { pendingGold=0;foreach(var p in pickups)RecyclePickup(p);pickups.Clear(); }
            foreach(var barrel in barrels) { if(barrel.body)Destroy(barrel.body.gameObject);if(barrel.warning)Destroy(barrel.warning.gameObject); }barrels.Clear();
            foreach(var foe in foes)if(foe.warning)foe.warning.gameObject.SetActive(false);
            if(clear && IsGoldRush)
            {
                foreach(var foe in foes) { if(foe.warning)Destroy(foe.warning.gameObject);if(foe.body) { AnimateDeath(foe);Destroy(foe.body.gameObject); } }
                foes.Clear();
            }
            knockback=Vector3.zero;
            if(playerAnimation)playerAnimation.SetVisible(true);else player.GetChild(0).GetComponent<Renderer>().enabled=true;
            feedback.Play(clear?CombatFeedback.Cue.Clear:CombatFeedback.Cue.Fail);
        }
    }
}
