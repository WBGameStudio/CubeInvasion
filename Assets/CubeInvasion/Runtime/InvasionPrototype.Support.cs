using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [SerializeField] SupportEnemySettings supportSettings;
        sealed class SupportState {public float clock=1.8f,recovery;public Foe target;public LineRenderer beam;}
        readonly Dictionary<Foe,SupportState> supportStates=new Dictionary<Foe,SupportState>();
        MissionSpec supportSource,supportMission;
        MissionSpec MissionWithSupport(MissionSpec source)
        {
            if(!supportSettings||!supportSettings.enabled||missionIndex!=supportSettings.missionIndex||source.mode!=MissionMode.Combat)return source;
            if(supportSource!=source||supportMission==null) {supportSource=source;supportMission=SupportEnemySettings.Expand(source);}return supportMission;
        }
        void EquipSupport(Foe foe)
        {
            if(foe.kind!=EnemyKind.Shielder&&foe.kind!=EnemyKind.Healer)return;
            var state=new SupportState {clock=HealInterval};supportStates[foe]=state;
            var root=foe.animation?foe.animation.VisualRoot.Find("DOTween Reactions"):foe.body;if(!root)root=foe.body;
            if(foe.kind==EnemyKind.Healer&&!HasDedicatedModel("Healer")&&!root.Find("Health cross H"))
            {
                Shape("Health cross H",PrimitiveType.Cube,new Vector3(0,1.05f,0),new Vector3(.65f,.18f,.18f),VoidToyPalette.Cyan,root);
                Shape("Health cross V",PrimitiveType.Cube,new Vector3(0,1.05f,0),new Vector3(.18f,.65f,.18f),VoidToyPalette.Cyan,root);
            }
            var visual=new GameObject(foe.kind==EnemyKind.Shielder?"Shielder frontal guard":"Healer telegraph link");visual.transform.SetParent(foe.kind==EnemyKind.Shielder?foe.body:dynamicRoot,false);
            var beam=visual.AddComponent<LineRenderer>();beam.sharedMaterial=RouteIconMaterial();beam.widthMultiplier=.035f;beam.startColor=beam.endColor=VoidToyPalette.CyanText;beam.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;state.beam=beam;
            if(foe.kind==EnemyKind.Shielder)
            {
                beam.useWorldSpace=false;beam.positionCount=13;for(int i=0;i<13;i++) {float a=Mathf.Lerp(-60,60,i/12f)*Mathf.Deg2Rad;beam.SetPosition(i,new Vector3(Mathf.Sin(a)*.9f,.25f,Mathf.Cos(a)*.9f));}
            }
            else {beam.useWorldSpace=true;beam.positionCount=2;visual.SetActive(false);}
        }
        float HealInterval=>supportSettings?Mathf.Max(.5f,supportSettings.healInterval):1.8f;
        float HealWindup=>supportSettings?Mathf.Min(supportSettings.healWindup,HealInterval):.55f;
        float HealRadius=>supportSettings?supportSettings.healRadius:3.2f;
        float ShieldDamage(Foe foe,float damage,Vector3 direction,bool area)
        {
            if(foe.kind!=EnemyKind.Shielder||area)return damage;
            direction.y=0;
            if(direction.sqrMagnitude>.00001f&&Vector3.Dot(foe.body.forward,-direction.normalized)>=.5f)
            {feedback.Play(CombatFeedback.Cue.ShieldBlock);feedback.Burst(foe.body.position,VoidToyPalette.Cyan,.35f,.1f);return damage*.35f;}
            return damage;
        }
        bool CanHeal(Foe healer,Foe target)
            =>target!=null&&target!=healer&&target.kind!=EnemyKind.Healer&&target.kind!=EnemyKind.Crusher&&target.kind!=EnemyKind.ObjectiveNode&&!IsBoss&&foes.Contains(target)&&target.body&&target.hp>0&&target.hp<target.maxHp&&FlatDistance(healer.body.position,target.body.position)<=HealRadius&&!CoverHit(healer.body.position,target.body.position,.06f,out _);
        void TickHealer(Foe foe,SupportState state,float dt,Vector3 toward,float distance)
        {
            state.clock=Mathf.Max(0,state.clock-dt);
            if(foe.windup>0)
            {
                if(!CanHeal(foe,state.target)) {foe.windup=0;state.target=null;state.clock=Mathf.Max(state.clock,.4f);state.beam.gameObject.SetActive(false);return;}
                foe.windup-=dt;state.beam.SetPosition(0,foe.body.position+Vector3.up*.3f);state.beam.SetPosition(1,state.target.body.position+Vector3.up*.3f);
                if(foe.windup<=0)
                {
                    float amount=Mathf.Min(supportSettings?supportSettings.healing:5,state.target.maxHp-state.target.hp);state.target.hp+=Mathf.Max(0,amount);
                    feedback.Burst(state.target.body.position,VoidToyPalette.Cyan,.45f,.2f);feedback.Play(CombatFeedback.Cue.Repair);RecordValidation("enemy_healed",state.target.kind.ToString());state.target=null;state.clock=HealInterval;state.beam.gameObject.SetActive(false);
                }
                return;
            }
            if(state.clock<=HealWindup)
            {
                Foe target=null;float ratio=1;foreach(var ally in foes)if(CanHeal(foe,ally)&&ally.hp/ally.maxHp<ratio) {target=ally;ratio=ally.hp/ally.maxHp;}
                if(target!=null)
                {state.target=target;foe.windup=HealWindup;state.clock=HealWindup;state.beam.gameObject.SetActive(true);state.beam.SetPosition(0,foe.body.position+Vector3.up*.3f);state.beam.SetPosition(1,target.body.position+Vector3.up*.3f);if(foe.animation)foe.animation.Telegraph(HealWindup);return;}
            }
            Foe follow=null;float best=float.PositiveInfinity;
            foreach(var ally in foes)if(ally!=foe&&ally.kind!=EnemyKind.Healer&&ally.body) {float d=FlatDistance(foe.body.position,ally.body.position);if(d<best) {best=d;follow=ally;}}
            var destination=distance<2?foe.body.position-toward.normalized*2:follow!=null&&best>2?follow.body.position:foe.body.position;
            MoveSupport(foe,destination,dt);
        }
        void MoveSupport(Foe foe,Vector3 destination,float dt)
        {
            var relative=destination-centre;relative.x=Mathf.Clamp(relative.x,-ArenaHalfX+.6f,ArenaHalfX-.6f);relative.z=Mathf.Clamp(relative.z,-3.9f,3.9f);destination=EnemyWaypoint(foe.body.position,centre+relative);
            var delta=destination-foe.body.position;delta.y=0;if(delta.sqrMagnitude<.001f)return;
            var next=foe.body.position+delta.normalized*Mathf.Min(delta.magnitude,foe.speed*EnemyMoveMultiplier*dt);
            if(!CoverHit(foe.body.position,next,.3f,out _)) {foe.body.position=next;foe.body.rotation=Quaternion.RotateTowards(foe.body.rotation,Quaternion.LookRotation(delta),90*dt);}
        }
        bool TickSupportEnemy(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(!supportStates.TryGetValue(foe,out var state))return false;
            if(foe.kind==EnemyKind.Healer) {TickHealer(foe,state,dt,toward,distance);return true;}
            if(foe.windup>0)
            {
                foe.windup-=dt;foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
                if(foe.windup<=0) {foe.warning.gameObject.SetActive(false);state.recovery=1.2f;var offset=player.position-foe.body.position;offset.y=0;if(distance<1.5f&&Vector3.Dot(offset.normalized,foe.aim)>.5f)DamagePlayer(5*EnemyDamageMultiplier*foe.attackPower,foe.aim*1.2f);}
                return true;
            }
            if(state.recovery>0) {state.recovery=Mathf.Max(0,state.recovery-dt);return true;}
            if(toward.sqrMagnitude>.01f)foe.body.rotation=Quaternion.RotateTowards(foe.body.rotation,Quaternion.LookRotation(toward), (supportSettings?supportSettings.shieldTurnSpeed:45)*dt);
            if(foe.attack<=0&&distance<1.4f&&!CoverHit(foe.body.position,player.position,.1f,out _))
            {foe.aim=foe.body.forward;foe.windup=.85f;foe.attack=foe.windup+1.8f*EnemyRecoveryMultiplier;foe.warning.gameObject.SetActive(true);if(foe.animation)foe.animation.Telegraph(foe.windup);}
            else if(distance>.9f)MoveEnemy(foe,toward.normalized*foe.speed*dt);
            return true;
        }
        void SupportEliminated(Foe foe)
        {
            if(supportStates.TryGetValue(foe,out var removed)) {if(removed.beam)Destroy(removed.beam.gameObject);supportStates.Remove(foe);}
            foreach(var pair in supportStates)if(pair.Value.target==foe) {pair.Value.target=null;pair.Key.windup=0;pair.Value.clock=Mathf.Max(pair.Value.clock,.4f);if(pair.Value.beam)pair.Value.beam.gameObject.SetActive(false);}
        }
        void ClearSupportEffects()
        {foreach(var state in supportStates.Values)if(state.beam)Destroy(state.beam.gameObject);supportStates.Clear();supportSource=null;supportMission=null;}
    }
}
