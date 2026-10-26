using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [SerializeField] BomberSettings bomberSettings;
        sealed class DeathCharge {public Transform body,warning;public Vector3 position;public float fuse;}
        readonly List<DeathCharge> deathCharges=new List<DeathCharge>();
        MissionSpec bomberSource,bomberMission;
        MissionSpec MissionWithBombers(MissionSpec source)
        {
            if(!bomberSettings||!bomberSettings.enabled||missionIndex!=bomberSettings.missionIndex||source.mode!=MissionMode.Combat)return source;
            if(bomberSource!=source||bomberMission==null) {bomberSource=source;bomberMission=BomberEncounters.Expand(source);}
            return bomberMission;
        }
        void EquipBomber(Foe foe)
        {
            if(foe.kind!=EnemyKind.Bomber)return;
            var root=foe.animation?foe.animation.VisualRoot.Find("DOTween Reactions"):foe.body;
            if(!root)root=foe.body;
            if(!HasDedicatedModel("Bomber")&&!root.Find("Bomber explosive backpack"))Shape("Explosive barrel",PrimitiveType.Cylinder,new Vector3(0,.25f,-.6f),new Vector3(.4f,.4f,.4f),VoidToyPalette.Warm,root).name="Bomber explosive backpack";
            foe.body.name="Bomber";
        }
        void TickBomber(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(foe.windup>0)
            {
                foe.windup-=dt;foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);
                if(foe.windup<=0) {foe.warning.gameObject.SetActive(false);if(distance<1.5f)DamagePlayer(5*EnemyDamageMultiplier*foe.attackPower,foe.aim*1.2f);}
                return;
            }
            if(foe.attack<=0&&distance<1.3f&&!CoverHit(foe.body.position,player.position,.1f,out _))
            {
                foe.aim=toward.sqrMagnitude>.01f?toward.normalized:Vector3.forward;foe.windup=.65f;foe.attack=foe.windup+1.2f*EnemyRecoveryMultiplier;
                foe.body.rotation=Quaternion.LookRotation(foe.aim);foe.warning.gameObject.SetActive(true);if(foe.animation)foe.animation.Telegraph(foe.windup);
            }
            else if(distance>.85f)MoveEnemy(foe,toward.normalized*foe.speed*dt);
        }
        void BomberEliminated(Foe foe,Vector3 at)
        {
            if(foe.kind!=EnemyKind.Bomber)return;
            var body=Shape("Explosive barrel",PrimitiveType.Cylinder,at,new Vector3(.3f,.3f,.3f),VoidToyPalette.Warm,platform);body.position=at;body.name="Bomber dropped charge";
            var warning=WarningRing(at,BomberRadius,VoidToyPalette.Danger);
            deathCharges.Add(new DeathCharge {body=body,warning=warning,position=at,fuse=bomberSettings?bomberSettings.fuse:.8f});
            feedback.Play(CombatFeedback.Cue.Alarm);RecordValidation("bomber_fuse_started");
        }
        float BomberRadius=>bomberSettings?bomberSettings.radius:2.2f;
        float BomberBlast=>(bomberSettings?bomberSettings.blastDamage:10)*(1+RouteBlast);
        void ClearDeathCharges(bool success=false)
        {
            foreach(var charge in deathCharges)
            {
                if(success)feedback.Burst(charge.position,VoidToyPalette.Warm,.7f,.2f,true);
                if(charge.body)Destroy(charge.body.gameObject);if(charge.warning)Destroy(charge.warning.gameObject);
            }
            deathCharges.Clear();
        }
        void TickDeathCharges(float dt)
        {
            if(phase!=Phase.Combat||paused)return;
            // Completing the actual objective wins immediately; a corpse never adds mandatory timer debt.
            if(ObjectiveComplete) {ClearDeathCharges(true);return;}
            for(int i=deathCharges.Count-1;i>=0;i--)
            {
                var charge=deathCharges[i];charge.fuse-=dt;if(charge.fuse>0)continue;
                deathCharges.RemoveAt(i);if(charge.body)Destroy(charge.body.gameObject);if(charge.warning)Destroy(charge.warning.gameObject);
                feedback.Play(CombatFeedback.Cue.Explosion);feedback.Burst(charge.position,VoidToyPalette.Warm,BomberRadius,.3f,true);RecordValidation("bomber_exploded");
                var victims=foes.ToArray();foreach(var foe in victims)if(foes.Contains(foe)&&FlatDistance(foe.body.position,charge.position)<=BomberRadius)HitEnemyArea(foe,BomberBlast,(foe.body.position-charge.position).normalized);
                var crates=goldCrates.ToArray();foreach(var crate in crates)if(crate.body&&FlatDistance(crate.body.position,charge.position)<=BomberRadius)HitGoldCrate(crate,BomberBlast);
                foreach(var barrel in barrels)if(FlatDistance(barrel.body.position,charge.position)<=BomberRadius)HitBarrel(barrel,100);
                if(FlatDistance(player.position,charge.position)<=BomberRadius)DamagePlayer(BomberBlast*EnemyDamageMultiplier,(player.position-charge.position).normalized*1.8f);
                if(phase!=Phase.Combat)return;
            }
        }
    }
}
