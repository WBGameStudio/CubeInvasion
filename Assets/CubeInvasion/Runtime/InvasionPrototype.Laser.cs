using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        void PlayerMuzzle(Color color,bool twin=false,float strength=1)
        {
            var source=playerAnimation&&playerAnimation.WeaponVisual?playerAnimation.WeaponVisual:gun;
            var filter=source.GetComponent<MeshFilter>();var bounds=filter.sharedMesh.bounds;
            var tip=new Vector3(bounds.center.x,bounds.center.y,bounds.max.z);
            if(twin)
            {
                tip.x=-.25f;feedback.Muzzle(source,tip,player.forward,color,strength);
                tip.x=.25f;feedback.Muzzle(source,tip,player.forward,color,strength);
            }
            else feedback.Muzzle(source,tip,player.forward,color,strength);
        }
        void EnemyMuzzle(Vector3 at,Vector3 direction)
        {
            foreach(var foe in foes)
            {
                if(!foe.body||(foe.body.position-at).sqrMagnitude>.01f)continue;
                // Boss fans/radial attacks share one energy discharge, not one flash per pellet.
                if(foe.muzzleFrame==Time.frameCount)return;foe.muzzleFrame=Time.frameCount;
                Transform source=null;
                if(foe.animation&&foe.animation.VisualRoot)
                    source=foe.animation.VisualRoot.Find("DOTween Reactions/RightArm/RightHand")
                        ??foe.animation.VisualRoot.Find("DOTween Reactions/Gunner barrel")
                        ??foe.animation.VisualRoot.Find("DOTween Reactions/Twin cannon")
                        ??foe.animation.VisualRoot.Find("DOTween Reactions/RightHand")
                        ??foe.animation.VisualRoot.Find("DOTween Reactions/Head/RightHand")
                        ??foe.animation.VisualRoot.Find("DOTween Reactions/Core");
                if(source)
                {
                    var filter=source.GetComponent<MeshFilter>();var b=filter.sharedMesh.bounds;
                    var tip=new Vector3(b.center.x,b.center.y,b.max.z);
                    if(source.name=="Core"&&foe.kind==EnemyKind.Sniper)tip=new Vector3(.28f,.14f,b.max.z);
                    feedback.Muzzle(source,tip,direction,new Color(1,.18f,.055f),foe.kind==EnemyKind.Sniper?1.25f:1);
                }
                else feedback.Muzzle(foe.body,foe.body.InverseTransformPoint(at+direction*.5f),direction,new Color(1,.18f,.055f));
                return;
            }
        }
    }
}
