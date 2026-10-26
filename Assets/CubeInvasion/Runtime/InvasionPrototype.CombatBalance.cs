using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("Combat pressure / one-thumb fire rhythm")]
        [SerializeField] CombatBalanceSettings combatBalance;
        float EnemyMoveMultiplier=>combatBalance?combatBalance.enemyMoveMultiplier:1.2f;
        float EnemyRecoveryMultiplier=>combatBalance?combatBalance.enemyRecoveryMultiplier:.8f;
        float EnemyDamageMultiplier=>combatBalance?combatBalance.enemyDamageMultiplier:1.3f;
        float FireMovementWeight=>Mathf.InverseLerp(combatBalance?combatBalance.focusRadius:.25f,combatBalance?combatBalance.runningRadius:.85f,stick.magnitude);
        float FireCadenceMultiplier=>Mathf.Lerp(1,combatBalance?combatBalance.runningShotIntervalMultiplier:2.1f,FireMovementWeight);
        void TickAutoFire(float dt)
        {
            // Normalized recovery makes moving back toward center immediately accelerate the next shot.
            // No pointer-up, tap, extra button or cooldown reset is required.
            shot=Mathf.Max(0,shot-dt/FireCadenceMultiplier);
            if(AimAtNearestEnemy()&&shot<=0) { shot=FireInterval;FirePulse(); }
        }
        bool CoverHit(Vector3 start,Vector3 end,float radius,out float entry)
        {
            entry=float.PositiveInfinity;bool hit=false;
            if(CoverTransformHit(cover,start,end,radius,out float t)) {entry=t;hit=true;}
            foreach(var block in additionalCover)if(block&&CoverTransformHit(block,start,end,radius,out t)&&t<entry) {entry=t;hit=true;}
            if(!hit)entry=0;return hit;
        }
        bool CoverTransformHit(Transform block,Vector3 start,Vector3 end,float radius,out float entry)
        {
            entry=0;if(!block||!block.gameObject.activeInHierarchy)return false;
            var filter=block.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)return false;
            var bounds=filter.sharedMesh.bounds;var a=block.InverseTransformPoint(start);var b=block.InverseTransformPoint(end);var scale=block.lossyScale;float exit=1;
            return Slab(a.x,b.x-a.x,bounds.min.x-radius/Mathf.Abs(scale.x),bounds.max.x+radius/Mathf.Abs(scale.x),ref entry,ref exit)
                &&Slab(a.z,b.z-a.z,bounds.min.z-radius/Mathf.Abs(scale.z),bounds.max.z+radius/Mathf.Abs(scale.z),ref entry,ref exit);
        }
        static bool Slab(float origin,float direction,float min,float max,ref float entry,ref float exit)
        {
            if(Mathf.Abs(direction)<.000001f)return origin>=min&&origin<=max;
            float a=(min-origin)/direction,b=(max-origin)/direction;if(a>b) {float t=a;a=b;b=t;}
            entry=Mathf.Max(entry,a);exit=Mathf.Min(exit,b);return entry<=exit;
        }
        static bool CircleHit(Vector3 start,Vector3 end,Vector3 point,float radius,out float entry)
        {
            var d=end-start;d.y=0;var o=start-point;o.y=0;
            float c=o.sqrMagnitude-radius*radius;entry=0;if(c<=0)return true;
            float a=d.sqrMagnitude;if(a<.000001f)return false;
            float b=Vector3.Dot(o,d),disc=b*b-a*c;if(disc<0)return false;
            entry=(-b-Mathf.Sqrt(disc))/a;return entry>=0&&entry<=1;
        }
        Vector3 SafeMuzzle(Vector3 nominal)
        {
            // A long muzzle must never teleport a projectile through a nearby barrier.
            return CoverHit(player.position,nominal,.09f,out float t)?Vector3.Lerp(player.position,nominal,Mathf.Max(0,t-.01f)):nominal;
        }
        readonly Vector3[] coverCorners=new Vector3[4];
        Vector3 EnemyWaypoint(Vector3 start,Vector3 target)
        {
            var obstacle=cover;foreach(var block in additionalCover)if(CoverTransformHit(block,start,target,.34f,out _)) {obstacle=block;break;}
            if(!obstacle)return target;
            // A player hugging the barrier can be just inside the enemy's wider clearance.
            // Route toward the adjacent free space rather than asking for an unreachable endpoint.
            if(CoverHit(target,target,.34f,out _))
            {var away=target-obstacle.position;away.y=0;target+=away.normalized*.12f;}
            if(!CoverHit(start,target,.32f,out _))return target;
            var bounds=obstacle.GetComponent<MeshFilter>().sharedMesh.bounds;var scale=obstacle.lossyScale;
            float x=bounds.extents.x+.4f/Mathf.Abs(scale.x),z=bounds.extents.z+.4f/Mathf.Abs(scale.z);
            for(int i=0;i<4;i++)coverCorners[i]=obstacle.TransformPoint(bounds.center+new Vector3((i==0||i==3)?-x:x,0,i<2?-z:z));
            float best=float.PositiveInfinity;var result=start;
            for(int i=0;i<4;i++)
            {
                var first=coverCorners[i];if(CoverHit(start,first,.32f,out _))continue;
                float distance=FlatDistance(start,first);
                if(!CoverHit(first,target,.32f,out _))
                {float cost=distance+FlatDistance(first,target);if(cost<best) {best=cost;result=first;}continue;}
                for(int j=0;j<4;j++)
                {
                    var second=coverCorners[j];if(i==j||CoverHit(first,second,.32f,out _)||CoverHit(second,target,.32f,out _))continue;
                    float cost=distance+FlatDistance(first,second)+FlatDistance(second,target);
                    if(cost<best) {best=cost;result=first;}
                }
            }
            result.y=start.y;return result;
        }
    }
}
