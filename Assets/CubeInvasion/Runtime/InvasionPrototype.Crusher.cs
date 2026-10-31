using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("Crusher / GDD phase objectives")]
        [SerializeField] CrusherPhaseSettings crusherPhases;
        Foe crusherBoss;
        float crusherPhaseStartHealth;
        LineRenderer crusherSupportLinks;
        readonly Foe[] crusherDrones=new Foe[3];
        readonly Transform[] crusherArmor=new Transform[2];
        readonly float[] crusherArmorHealth=new float[2];
        bool HasCrusherObjectives=>crusherPhases&&crusherPhases.enabled&&IsBoss&&Mission.boss==BossIdentity.Crusher;
        void ResetCrusherPhase()
        {
            if(crusherSupportLinks)Destroy(crusherSupportLinks.gameObject);crusherSupportLinks=null;
            crusherBoss=null;
            for(int i=0;i<3;i++)crusherDrones[i]=null;
            for(int i=0;i<2;i++) {crusherArmor[i]=null;crusherArmorHealth[i]=0;}
        }
        void SetupCrusherPhase()
        {
            ResetCrusherPhase();if(!HasCrusherObjectives)return;
            crusherBoss=foes.Find(f=>f.kind==EnemyKind.Crusher);if(crusherBoss==null)return;
            crusherPhaseStartHealth=bossHealth;
            if(crusherBoss.animation)
                foreach(var part in crusherBoss.animation.VisualRoot.GetComponentsInChildren<Transform>(true))
                {
                    if(!part.name.EndsWith("Armor"))continue;
                    int index=part.name.StartsWith("Left")?0:1;crusherArmor[index]=part;
                    part.gameObject.SetActive(stage==1);
                }
            if(stage==1)
            {
                crusherArmorHealth[0]=crusherArmorHealth[1]=BossPhaseHealth(1)*.5f;
                RecordValidation("boss_objective_started","armor:2");
            }
            else if(stage==2)
            {
                // The three drones share the existing 84 HP phase budget. No invulnerable-body HP tax.
                var support=new List<Foe>();foreach(var foe in foes)if(foe!=crusherBoss)support.Add(foe);
                while(support.Count<3)
                {
                    var at=centre+new Vector3(support.Count==0?-2.8f:support.Count==1?2.8f:0,.6f,2.8f);
                    support.Add(SpawnContentEnemy(ContentEnemy.Gunner,SafeBranchSpawn(at,ContentEnemy.Gunner)));
                }
                float weight=0;for(int i=0;i<3;i++)weight+=support[i].attackPower>1?1.35f:1;
                for(int i=0;i<3;i++) {crusherDrones[i]=support[i];crusherDrones[i].hp=BossPhaseHealth(2)*(support[i].attackPower>1?1.35f:1)/weight;crusherDrones[i].maxHp=crusherDrones[i].hp;}
                // Only three support targets are required even if a designer adds extra entries.
                for(int i=3;i<support.Count;i++) {var foe=support[i];foes.Remove(foe);if(foe.warning)Destroy(foe.warning.gameObject);Destroy(foe.body.gameObject);}
                var links=new GameObject("Crusher protected support links");links.transform.SetParent(dynamicRoot,false);
                crusherSupportLinks=links.AddComponent<LineRenderer>();crusherSupportLinks.sharedMaterial=RouteIconMaterial();crusherSupportLinks.useWorldSpace=true;crusherSupportLinks.positionCount=7;crusherSupportLinks.widthMultiplier=.025f;
                crusherSupportLinks.startColor=crusherSupportLinks.endColor=VoidToyPalette.WithAlpha(VoidToyPalette.Cyan,.65f);crusherSupportLinks.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                RecordValidation("boss_objective_started","support:3");
            }
            else RecordValidation("boss_objective_started","core");
        }
        void RefreshCrusherLinks()
        {
            if(!crusherSupportLinks)return;
            bool visible=HasCrusherObjectives&&stage==2&&phase==Phase.Combat&&crusherBoss!=null&&crusherBoss.body&&crusherBoss.hp>0;
            crusherSupportLinks.gameObject.SetActive(visible);if(!visible)return;
            var origin=crusherBoss.body.position+Vector3.up*.4f;crusherSupportLinks.SetPosition(0,origin);
            for(int i=0;i<3;i++) {var drone=crusherDrones[i];var end=drone!=null&&drone.body&&drone.hp>0?drone.body.position+Vector3.up*.3f:origin;crusherSupportLinks.SetPosition(i*2+1,end);crusherSupportLinks.SetPosition(i*2+2,origin);}
        }
        bool CrusherTargetable(Foe foe)=>!HasCrusherObjectives||stage!=2||foe!=crusherBoss;
        Vector3 CrusherArmorPoint(int index)
        {
            if(crusherArmor[index])return crusherArmor[index].GetComponent<Renderer>().bounds.center;
            return crusherBoss.body.TransformPoint(new Vector3(index==0?-.55f:.55f,.3f,.35f));
        }
        int ClosestCrusherArmor()
        {
            int choice=-1;float distance=float.PositiveInfinity;
            for(int i=0;i<2;i++)if(crusherArmorHealth[i]>0)
            {float d=(CrusherArmorPoint(i)-player.position).sqrMagnitude;if(d<distance) {distance=d;choice=i;}}
            return choice;
        }
        Vector3 EnemyTargetPoint(Foe foe)
        {
            if(HasCrusherObjectives&&stage==1&&foe==crusherBoss)
            {int index=ClosestCrusherArmor();if(index>=0)return CrusherArmorPoint(index);}
            return foe.body.position;
        }
        bool PlayerProjectileHit(Foe foe,Vector3 start,Vector3 end,out float entry,out int panel)
        {
            entry=float.PositiveInfinity;panel=-1;if(!CrusherTargetable(foe)||!ExpandedBossTargetable(foe))return false;
            if(HasCrusherObjectives&&stage==1&&foe==crusherBoss)
            {
                for(int i=0;i<2;i++)if(crusherArmorHealth[i]>0&&CircleHit(start,end,CrusherArmorPoint(i),.44f,out float hit)&&hit<entry) {entry=hit;panel=i;}
                return panel>=0;
            }
            return CircleHit(start,end,foe.body.position,.55f,out entry);
        }
        float ResolveCrusherHit(Foe foe,float damage,int panel)
        {
            if(!HasCrusherObjectives||foe!=crusherBoss||stage!=1)return damage;
            if(panel<0)panel=ClosestCrusherArmor();
            if(panel<0||panel>1||crusherArmorHealth[panel]<=0)return 0;
            float applied=Mathf.Min(Mathf.Max(0,damage),crusherArmorHealth[panel]);crusherArmorHealth[panel]-=applied;
            if(crusherArmorHealth[panel]<=.0001f)
            {
                crusherArmorHealth[panel]=0;RecordValidation("boss_armor_broken",panel==0?"left":"right");
                feedback.Play(CombatFeedback.Cue.BossPhase);feedback.Burst(CrusherArmorPoint(panel),VoidToyPalette.Danger,1,.3f,true);
                if(crusherArmor[panel])
                {
                    var source=crusherArmor[panel];var part=Instantiate(source,dynamicRoot,true);source.gameObject.SetActive(false);part.gameObject.SetActive(true);
                    WorldTween(DOTween.Sequence().Join(part.DOMove(part.position+Vector3.down*2+crusherBoss.body.right*(panel==0?-1:1),.45f).SetEase(Ease.InQuad))
                        .Join(part.DORotate(part.eulerAngles+new Vector3(70,30,60),.45f)).OnComplete(()=> {if(part)Destroy(part.gameObject);})).SetLink(part.gameObject);
                }
            }
            return applied;
        }
        void SyncCrusherArmorHealth(Foe foe)
        {
            if(!HasCrusherObjectives||stage!=1||foe!=crusherBoss)return;
            // Panels are the phase objective. Independent float subtraction on the body
            // can leave untargetable HP after the last panel has broken.
            foe.hp=crusherArmorHealth[0]+crusherArmorHealth[1];
            bossHealth=Mathf.Max(0,crusherPhaseStartHealth-BossPhaseHealth(1)+foe.hp);
        }
        void CrusherDroneDamaged(Foe foe,float applied)
        {
            if(!HasCrusherObjectives||stage!=2||crusherBoss==null)return;
            for(int i=0;i<3;i++)if(crusherDrones[i]==foe)
            {bossHealth=Mathf.Max(0,bossHealth-applied);crusherBoss.hp=Mathf.Max(0,crusherBoss.hp-applied);break;}
        }
        void CrusherEnemyEliminated(Foe foe)
        {
            if(!HasCrusherObjectives||stage!=2||foe==crusherBoss||crusherBoss==null)return;
            bool drone=false;for(int i=0;i<3;i++)if(crusherDrones[i]==foe)drone=true;
            if(!drone)return;RecordValidation("boss_support_destroyed");
            if(CrusherSupportLeft>0)return;
            bossHealth=Mathf.Max(0,crusherPhaseStartHealth-BossPhaseHealth(2));crusherBoss.hp=0;RecordValidation("boss_support_disabled");
            if(foes.Contains(crusherBoss))EliminateEnemy(crusherBoss);
        }
        int CrusherSupportLeft
        {get {int count=0;foreach(var foe in crusherDrones)if(foe!=null&&foe.hp>0&&foes.Contains(foe))count++;return count;}}
        string CrusherObjective
        {
            get
            {
                if(!HasCrusherObjectives||crusherBoss==null)return null;
                if(stage==1) {int left=0;foreach(float hp in crusherArmorHealth)if(hp>0)left++;return left>0?$"ZIRH · {left}/2 PANEL":"KALAN DESTEKLERİ TEMİZLE";}
                if(stage==2)return $"KALKAN · {CrusherSupportLeft} DRONE";
                return crusherBoss.hp>0?$"ÇEKİRDEK · {Mathf.CeilToInt(crusherBoss.hp)} CAN":"KALAN DESTEKLERİ TEMİZLE";
            }
        }
        void TickCrusherPush(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(foe.windup>0)
            {
                foe.windup-=dt;
                if(foe.windup<=0)
                {
                    foe.warning.gameObject.SetActive(false);var offset=player.position-foe.body.position;offset.y=0;
                    if(offset.magnitude<=crusherPhases.pushRange&&Vector3.Dot(offset.normalized,foe.aim)>.42f)
                        DamagePlayer(crusherPhases.pushDamage*EnemyDamageMultiplier,foe.aim*crusherPhases.pushStrength);
                    if(foe.animation)foe.animation.Slam();feedback.Play(CombatFeedback.Cue.BossSlam);
                    feedback.Burst(foe.body.position,VoidToyPalette.Danger,crusherPhases.pushRange,.25f,true);
                }
                return;
            }
            if(foe.attack<=0&&distance<crusherPhases.pushRange+.4f&&!CoverHit(foe.body.position,player.position,.1f,out _))
            {
                foe.aim=toward.sqrMagnitude>.01f?toward.normalized:foe.body.forward;foe.body.rotation=Quaternion.LookRotation(foe.aim);
                foe.windup=crusherPhases.pushWindup;foe.attack=foe.windup+crusherPhases.pushRecovery*EnemyRecoveryMultiplier;
                if(foe.animation)foe.animation.Telegraph(foe.windup);feedback.Play(CombatFeedback.Cue.Telegraph);
                var line=foe.warning.GetComponent<LineRenderer>();foe.warning.gameObject.SetActive(true);line.loop=true;line.useWorldSpace=true;line.positionCount=4;
                var origin=foe.body.position;origin.y=.1f;line.SetPosition(0,origin);
                line.SetPosition(1,origin+Quaternion.Euler(0,-65,0)*foe.aim*crusherPhases.pushRange);
                line.SetPosition(2,origin+foe.aim*crusherPhases.pushRange);line.SetPosition(3,origin+Quaternion.Euler(0,65,0)*foe.aim*crusherPhases.pushRange);
            }
            else if(distance>1.8f)MoveEnemy(foe,toward.normalized*foe.speed*dt);
        }
    }
}
