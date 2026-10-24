using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        enum EnemyKind { Grunt, Sprinter, Gunner, Pusher, Sniper, Bruiser, Crusher }
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
        void SetPaused(bool value) { paused=value;dragging=false;joystickFinger=-1;stick=Vector2.zero;if(hud)hud.ResetJoystick();if(feedback)feedback.SetPaused(value); }
        void Retry() { if(HasContent) { StartMission(missionIndex);return; } UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); }
        void LateUpdate()
        {
            TickAnimations(Time.deltaTime);
            if(!hud)return;
            feedback.SetPaused(paused);
            if(!paused)feedback.Tick(Time.deltaTime);
            hud.Refresh(remaining,health,stage,foes.Count,wallet,pendingGold,damageCubes,damageBonus,paused,feedback.Muted,phase.ToString(),reason);
            RefreshContentHud();
        }
        void SetupEncounterFeel()
        {
            pendingGold=0;alarmSecond=21;knockback=Vector3.zero;
            if(cover)Destroy(cover.gameObject);cover=null;
            barrels.Clear();pickups.Clear();ClearEnemyShots();
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
                cover.position=centre+new Vector3(0,.45f,.6f);
                }
                if(!IsGoldRush)SpawnPickup(centre+new Vector3(0,.45f,-1.2f),true,20);
            }
            else cover=null;
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
            var body=Shape(healing?"Health +20":"Gold pickup",healing?PrimitiveType.Cube:PrimitiveType.Cylinder,at,healing?Vector3.one*.35f:new Vector3(.28f,.07f,.28f),color,world);
            if(healing) { Shape("Health cross H",PrimitiveType.Cube,new Vector3(0,.6f,0),new Vector3(1,.16f,.3f),Color.white,body);Shape("Health cross V",PrimitiveType.Cube,new Vector3(0,.6f,0),new Vector3(.3f,.16f,1),Color.white,body); }
            pickups.Add(new Pickup {body=body,healing=healing,value=value,origin=at});
            AnimatePickup(body);
        }
        void CollectPickups(float dt,bool magnet)
        {
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];pickup.age+=dt;
                if(!pickup.body) { pickups.RemoveAt(i);continue; }
                if(pickup.healing&&health>=100&&!magnet)continue;
                float distance=Vector3.Distance(player.position,pickup.body.position);
                if(magnet || distance<1)
                {
                    if(pickup.healing) { health=Mathf.Min(100,health+pickup.value);feedback.Play(CombatFeedback.Cue.Heal); }
                    else { pendingGold+=pickup.value;if(IsGoldRush)quotaGold=Mathf.Min(Encounter.goldQuota,quotaGold+pickup.value);feedback.Play(CombatFeedback.Cue.Gold); }
                    feedback.Burst(pickup.body.position,pickup.healing?Color.green:Color.yellow,.5f);
                    Destroy(pickup.body.gameObject);pickups.RemoveAt(i);
                }
                else
                {
                    // DOTween owns pickup hover and rotation; collection only reads the position.
                }
            }
        }
        void HitEnemy(Foe foe,float damage,Vector3 direction)
        {
            if(!foes.Contains(foe))return;
            float applied=Mathf.Min(foe.hp,damage);
            foe.hp-=damage;foe.flash=.09f;
            if(HasContent && foe.kind==EnemyKind.Crusher)bossHealth=Mathf.Max(0,bossHealth-applied);
            if(foe.animation)foe.animation.Hit();
            feedback.Play(CombatFeedback.Cue.Hit);feedback.Burst(foe.body.position,Color.white,.3f);
            if(foe.hp>0)return;
            var at=foe.body.position;
            if(foe.warning)Destroy(foe.warning.gameObject);
            AnimateDeath(foe);
            Destroy(foe.body.gameObject);foes.Remove(foe);
            SpawnPickup(at,false,3);
            feedback.Burst(new Vector3(at.x,.1f,at.z),danger,.9f,.25f,true);
        }
        void DamagePlayer(float damage,Vector3 push)
        {
            if(phase!=Phase.Combat||invulnerability>0)return;
            health=Mathf.Max(0,health-damage);invulnerability=.55f;hurtFlash=.18f;knockback=push;
            if(playerAnimation)playerAnimation.Hit();
            feedback.Play(CombatFeedback.Cue.Hurt);feedback.Burst(player.position,danger,.8f,.2f,true);
            if(health<=0)Fail("Canın tükendi");
        }
        void TickEnemies(float dt)
        {
            foreach(var foe in foes)
            {
                if(foe.flash>0)
                {
                    foe.flash=Mathf.Max(0,foe.flash-dt);
                    if(foe.flash>0) { hitProperties.SetColor("_Color",Color.white);foe.visual.SetPropertyBlock(hitProperties); }
                    else foe.visual.SetPropertyBlock(null);
                }
                if(foe.spawn>0) { foe.spawn-=dt;continue; }
                foe.attack-=dt;
                var toward=player.position-foe.body.position;toward.y=0;float distance=toward.magnitude;
                if(TickSpecialEnemy(foe,dt,toward,distance))continue;
                if(foe.windup>0)
                {
                    foe.windup-=dt;
                    if(foe.kind!=EnemyKind.Gunner)foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
                    if(foe.windup<=0)
                    {
                        foe.warning.gameObject.SetActive(false);
                        if(foe.kind==EnemyKind.Sprinter) { foe.dash=.32f;foe.dashHit=false; }
                        else if(foe.kind==EnemyKind.Gunner) { foe.burst=3;foe.burstClock=0; }
                        else if(distance<1.5f)DamagePlayer(5,foe.aim*1.2f);
                    }
                    continue;
                }
                if(foe.dash>0)
                {
                    Vector3 previous=foe.body.position;
                    MoveEnemy(foe,foe.aim*7.5f*dt);
                    if(!foe.dashHit && SegmentDistance(player.position,previous,foe.body.position)<.8f)
                    { DamagePlayer(7,foe.aim*3);foe.dashHit=true; }
                    foe.dash-=dt;continue;
                }
                if(foe.burst>0)
                {
                    foe.burstClock-=dt;
                    if(foe.burstClock<=0) { if(foe.animation)foe.animation.Shoot();FireEnemyShot(foe.body.position,foe.aim);foe.burst--;foe.burstClock=.13f; }
                    continue;
                }
                float range=foe.kind==EnemyKind.Grunt?1.3f:foe.kind==EnemyKind.Sprinter?4.3f:7;
                if(foe.attack<=0 && distance<range)
                {
                    foe.aim=toward.sqrMagnitude>.01f?toward.normalized:Vector3.forward;
                    foe.windup=foe.kind==EnemyKind.Grunt?.5f:foe.kind==EnemyKind.Sprinter?.65f:1.0f;
                    foe.attack=foe.windup+(foe.kind==EnemyKind.Grunt?1.1f:1.8f);
                    if(foe.animation)foe.animation.Telegraph(foe.windup);
                    foe.warning.gameObject.SetActive(true);
                    foe.body.rotation=Quaternion.LookRotation(foe.aim);
                    if(foe.kind!=EnemyKind.Grunt)
                    {
                        var line=foe.warning.GetComponent<LineRenderer>();line.loop=false;line.useWorldSpace=true;line.positionCount=2;
                        line.SetPosition(0,foe.body.position);line.SetPosition(1,foe.body.position+foe.aim*(foe.kind==EnemyKind.Gunner?8:2.4f));
                    }
                }
                else if(distance>(foe.kind==EnemyKind.Gunner?3.8f:.85f))MoveEnemy(foe,toward.normalized*foe.speed*dt);
            }
        }
        void MoveEnemy(Foe foe,Vector3 delta)
        {
            Vector3 next=foe.body.position+delta;
            if(cover && FlatDistance(next,cover.position)<1.05f)return;
            var relative=next-centre;relative.x=Mathf.Clamp(relative.x,-4,4);relative.z=Mathf.Clamp(relative.z,-4,4);foe.body.position=centre+relative;
        }
        void TickWorldFeel(float dt)
        {
            if(hurtFlash>0)
            {
                hurtFlash=Mathf.Max(0,hurtFlash-dt);
                bool visible=hurtFlash<=0||((int)(hurtFlash*40)%2)==0;
                if(playerAnimation)playerAnimation.SetVisible(visible);else player.GetChild(0).GetComponent<Renderer>().enabled=visible;
            }
            Vector3 before=player.position;player.position+=knockback*dt;knockback=Vector3.Lerp(knockback,Vector3.zero,dt*10);
            if(cover && FlatDistance(player.position,cover.position)<1.05f)player.position=before;
            for(int i=barrels.Count-1;i>=0;i--)
            {
                var barrel=barrels[i];if(barrel.fuse<0)continue;
                barrel.fuse-=dt;
                if(barrel.fuse<=0) { Explode(barrel);if(phase!=Phase.Combat)return; }
            }
            TickEnemyShots(dt);if(phase!=Phase.Combat)return;CollectPickups(dt,false);
            int second=Mathf.CeilToInt(remaining);
            if(second<=5&&second>0&&second!=alarmSecond) { alarmSecond=second;feedback.Play(CombatFeedback.Cue.Alarm);feedback.Burst(centre+Vector3.up*.1f,danger,4.4f,.3f,true); }
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
            Vector3 at=barrel.body.position;barrels.Remove(barrel);
            Destroy(barrel.body.gameObject);Destroy(barrel.warning.gameObject);
            feedback.Play(CombatFeedback.Cue.Explosion);feedback.Burst(new Vector3(at.x,.12f,at.z),new Color(1,.45f,.1f),2.2f,.3f,true);
            // Reverse iteration survives deaths; chained barrels receive their own visible fuse.
            for(int i=foes.Count-1;i>=0;i--)if(FlatDistance(foes[i].body.position,at)<=2.2f)HitEnemy(foes[i],30,(foes[i].body.position-at).normalized);
            foreach(var other in barrels)if(FlatDistance(other.body.position,at)<=2.2f)HitBarrel(other,100);
            if(FlatDistance(player.position,at)<=2.2f)DamagePlayer(10,(player.position-at).normalized*5);
        }
        static float FlatDistance(Vector3 a,Vector3 b) { a.y=b.y=0;return Vector3.Distance(a,b); }
        static float SegmentDistance(Vector3 point,Vector3 start,Vector3 end)
        {
            point.y=start.y=end.y=0;var segment=end-start;float t=segment.sqrMagnitude>.00001f?Mathf.Clamp01(Vector3.Dot(point-start,segment)/segment.sqrMagnitude):0;
            return Vector3.Distance(point,start+segment*t);
        }
        Transform RentProjectile(Stack<Transform> pool,string name,Vector3 at,Color color,float size)
        {
            Transform body=pool.Count>0?pool.Pop():Shape(name,PrimitiveType.Sphere,at,Vector3.one*size,color,world);
            body.position=at;body.gameObject.SetActive(true);return body;
        }
        void FireEnemyShot(Vector3 at,Vector3 direction,float damage=6,float speed=7)
        { enemyShots.Add(new EnemyShot {body=RentProjectile(enemyShotPool,"Enemy pulse",at,danger,.2f),direction=direction,damage=damage,speed=speed});feedback.Play(CombatFeedback.Cue.Shot); }
        void TickEnemyShots(float dt)
        {
            for(int i=enemyShots.Count-1;i>=0;i--)
            {
                var shot=enemyShots[i];var start=shot.body.position;var end=start+shot.direction*shot.speed*dt;shot.body.position=end;shot.life-=dt;
                bool blocked=cover && SegmentDistance(cover.position,start,end)<.85f;
                bool hit=SegmentDistance(player.position,start,end)<.5f;
                if(blocked && hit && FlatDistance(player.position,start)<FlatDistance(cover.position,start))blocked=false;
                hit=hit&&!blocked;
                if(hit) { DamagePlayer(shot.damage,shot.direction);if(phase!=Phase.Combat)return; }
                if(blocked||hit||shot.life<=0) { shot.body.gameObject.SetActive(false);enemyShotPool.Push(shot.body);enemyShots.RemoveAt(i); }
            }
        }
        void ClearEnemyShots() { foreach(var shot in enemyShots) { shot.body.gameObject.SetActive(false);enemyShotPool.Push(shot.body); }enemyShots.Clear(); }
        void FinishEncounterFeel(bool clear)
        {
            ClearEnemyShots();
            if(clear)CollectPickups(0,true);else { pendingGold=0;foreach(var p in pickups)if(p.body)Destroy(p.body.gameObject);pickups.Clear(); }
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
