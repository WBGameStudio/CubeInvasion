using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        sealed class WeaponBlast {public Vector3 at;public float delay,damage,radius;public Transform warning;}
        readonly List<WeaponBlast> weaponBlasts=new List<WeaponBlast>();
        readonly List<Foe> weaponFoeScratch=new List<Foe>();
        readonly List<GoldCrate> weaponCrateScratch=new List<GoldCrate>();
        readonly List<Barrel> weaponBarrelScratch=new List<Barrel>();
        readonly List<Transform> orbitDrones=new List<Transform>();
        float orbitClock;
        float WeaponDamage=>ResolvedWeapon.Damage;
        int SpecialRank=>ResolvedWeapon.SpecialRank;
        bool FireAdvancedWeapon()
        {
            if((int)Weapon.kind<4)return false;
            var origin=SafeMuzzle(gun.position+player.forward*.35f);var direction=AimPoint-origin;direction.y=0;direction.Normalize();
            if(Weapon.kind==ContentWeapon.ArcLauncher) {if(currentTarget!=null)ChainWeapon(currentTarget,WeaponDamage,3+SpecialRank,2.5f,origin,true);else HitAdvancedAimObject(origin,direction,WeaponDamage);}
            else if(Weapon.kind==ContentWeapon.FlameSprayer)
            {
                weaponFoeScratch.Clear();weaponFoeScratch.AddRange(foes);
                foreach(var foe in weaponFoeScratch)
                {
                    var delta=foe.body.position-origin;delta.y=0;
                    if(delta.magnitude<2.3f*(1+MetaRange)&&Vector3.Dot(direction,delta.normalized)>.8f&&!CoverHit(origin,foe.body.position,.05f,out _))HitEnemy(foe,WeaponDamage,direction);
                }
                weaponCrateScratch.Clear();weaponCrateScratch.AddRange(goldCrates);foreach(var crate in weaponCrateScratch)if(FlameReaches(origin,direction,crate.body.position))HitGoldCrate(crate,WeaponDamage);
                weaponBarrelScratch.Clear();weaponBarrelScratch.AddRange(barrels);foreach(var barrel in weaponBarrelScratch)if(FlameReaches(origin,direction,barrel.body.position))HitBarrel(barrel,WeaponDamage);
                feedback.Burst(origin+direction,Weapon.color,1,.15f,true);
            }
            else if(Weapon.kind==ContentWeapon.CryoBeam)
            {if(currentTarget!=null) {var target=currentTarget;WeaponLink(origin,EnemyTargetPoint(target),Weapon.color);HitEnemy(target,WeaponDamage,direction);target.slow=target.kind==EnemyKind.Crusher?.25f:.65f+SpecialRank*.15f;}else HitAdvancedAimObject(origin,direction,WeaponDamage);}
            else
            {
                if(Weapon.kind==ContentWeapon.OrbitDrones&&orbitDrones.Count>0)origin=SafeMuzzle(orbitDrones[(int)(orbitClock*2)%orbitDrones.Count].position);
                direction=AimPoint-origin;direction.y=0;direction.Normalize();
                var body=RentProjectile(pulsePool,"Forward pulse",origin,Weapon.color,Weapon.kind==ContentWeapon.NovaCannon?.45f:.18f);body.GetComponent<LaserBoltVisual>().Fly(direction,0);
                pulses.Add(new Pulse {body=body,direction=direction,life=Weapon.projectileLife*(1+MetaRange),damage=WeaponDamage,kind=Weapon.kind,remainingHits=Weapon.kind==ContentWeapon.PierceRifle?3+SpecialRank:Weapon.kind==ContentWeapon.NovaCannon?8:1});
            }
            PlayerMuzzle(Weapon.color,false,1.2f);feedback.Play((CombatFeedback.Cue)((int)CombatFeedback.Cue.Pierce+(int)Weapon.kind-4));return true;
        }
        bool FlameReaches(Vector3 origin,Vector3 direction,Vector3 point)
        {var delta=point-origin;delta.y=0;return delta.magnitude<2.3f*(1+MetaRange)&&Vector3.Dot(direction,delta.normalized)>.8f&&!CoverHit(origin,point,.05f,out _);}
        void HitAdvancedAimObject(Vector3 origin,Vector3 direction,float damage)
        {
            var point=AimPoint;if(CoverHit(origin,point,.06f,out _))return;WeaponLink(origin,point,Weapon.color);
            if(goldTarget!=null)HitGoldCrate(goldTarget,damage);else if(barrelTarget!=null)HitBarrel(barrelTarget,damage);
        }
        void ChainWeapon(Foe first,float damage,int count,float radius,Vector3 start,bool includeFirst)
        {
            var visited=new HashSet<Foe>();var current=first;var at=start;
            for(int i=0;i<count;i++)
            {
                if(current==null||!current.body||CoverHit(at,EnemyTargetPoint(current),.06f,out _))break;
                var point=EnemyTargetPoint(current);visited.Add(current);WeaponLink(at,point,Weapon.color);
                if(i>0||includeFirst)HitEnemy(current,damage*(i==0?1:.7f),(point-at).normalized);
                at=point;current=null;float best=radius;
                foreach(var foe in foes)if(!visited.Contains(foe)&&CrusherTargetable(foe)&&ExpandedBossTargetable(foe)&&!CoverHit(at,foe.body.position,.06f,out _))
                {float d=FlatDistance(at,foe.body.position);if(d<best) {best=d;current=foe;}}
            }
        }
        void WeaponLink(Vector3 from,Vector3 to,Color color)
        {
            var steps=Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(from,to)*5),1,30);
            for(int i=0;i<=steps;i++)feedback.Burst(Vector3.Lerp(from,to,i/(float)steps),color,.08f,.12f);
        }
        bool PlayerPulseHit(Pulse pulse,Foe foe,Vector3 start,Vector3 end,out float entry,out int panel)
        {
            if(pulse.kind==ContentWeapon.NovaCannon&&!(HasCrusherObjectives&&stage==1&&foe==crusherBoss))
            {panel=-1;entry=0;return CrusherTargetable(foe)&&ExpandedBossTargetable(foe)&&CircleHit(start,end,foe.body.position,.95f,out entry);}
            return PlayerProjectileHit(foe,start,end,out entry,out panel);
        }
        bool AdvancedPulseHit(Pulse pulse,Foe hit,int panel)
        {
            if(pulse.kind==ContentWeapon.RocketPod)
            {var at=pulse.body.position;var ring=WarningRing(at,1.65f+SpecialRank*.2f,Weapon.color);weaponBlasts.Add(new WeaponBlast {at=at,delay=.35f,damage=pulse.damage,radius=1.65f+SpecialRank*.2f,warning=ring});return false;}
            if(hit==null)return false;
            pulse.hitEnemies.Add(hit);
            if(pulse.kind==ContentWeapon.PierceRifle)ApplyEnemyDamage(hit,pulse.damage,pulse.direction,panel,true);
            else HitEnemyAt(hit,pulse.damage,pulse.direction,panel);
            if(pulse.kind==ContentWeapon.RicochetGun)ChainWeapon(hit,pulse.damage,3+SpecialRank,3,pulse.body.position,false);
            if(pulse.kind==ContentWeapon.NovaCannon&&foes.Contains(hit))
            {var next=hit.body.position+pulse.direction*(hit.kind==EnemyKind.Crusher?.05f:.9f);if(!CoverHit(hit.body.position,next,.3f,out _))hit.body.position=next;}
            if(pulse.kind==ContentWeapon.PierceRifle||pulse.kind==ContentWeapon.NovaCannon) {pulse.remainingHits--;return pulse.remainingHits>0;}
            return false;
        }
        void TickAdvancedWeapons(float dt)
        {
            orbitClock+=dt;
            for(int i=0;i<orbitDrones.Count;i++)if(orbitDrones[i]) {float a=orbitClock*2.5f+i*Mathf.PI;orbitDrones[i].position=player.position+new Vector3(Mathf.Sin(a)*1.2f,.6f,Mathf.Cos(a)*1.2f);orbitDrones[i].rotation=Quaternion.LookRotation(player.forward);}
            for(int i=weaponBlasts.Count-1;i>=0;i--)
            {
                var blast=weaponBlasts[i];blast.delay-=dt;if(blast.delay>0)continue;
                weaponBlasts.RemoveAt(i);if(blast.warning)Destroy(blast.warning.gameObject);feedback.Play(CombatFeedback.Cue.Explosion);feedback.Burst(blast.at,Weapon.color,blast.radius,.3f,true);
                foreach(var foe in new List<Foe>(foes))if(FlatDistance(blast.at,foe.body.position)<blast.radius&&!CoverHit(blast.at,foe.body.position,.05f,out _))HitEnemyArea(foe,blast.damage,(foe.body.position-blast.at).normalized);
                weaponCrateScratch.Clear();weaponCrateScratch.AddRange(goldCrates);foreach(var crate in weaponCrateScratch)if(crate.body&&FlatDistance(blast.at,crate.body.position)<blast.radius&&!CoverHit(blast.at,crate.body.position,.05f,out _))HitGoldCrate(crate,blast.damage);
                weaponBarrelScratch.Clear();weaponBarrelScratch.AddRange(barrels);foreach(var barrel in weaponBarrelScratch)if(barrel.body&&FlatDistance(blast.at,barrel.body.position)<blast.radius&&!CoverHit(blast.at,barrel.body.position,.05f,out _))HitBarrel(barrel,blast.damage);
                if(FlatDistance(player.position,blast.at)<blast.radius)DamagePlayer(5,(player.position-blast.at).normalized*2);
            }
        }
        void SetupWeaponAccessories()
        {
            foreach(var drone in orbitDrones)if(drone)Destroy(drone.gameObject);orbitDrones.Clear();
            if(Weapon.kind!=ContentWeapon.OrbitDrones)return;
            for(int i=0;i<2;i++)orbitDrones.Add(Shape("Orbital drone",PrimitiveType.Cube,player.position,new Vector3(.55f,.55f,.55f),Weapon.color,dynamicRoot));
        }
        void ClearWeaponBlasts()
        {foreach(var blast in weaponBlasts)if(blast.warning)Destroy(blast.warning.gameObject);weaponBlasts.Clear();}
    }
}
