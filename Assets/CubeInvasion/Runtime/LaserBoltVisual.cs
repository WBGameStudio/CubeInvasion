using UnityEngine;
using UnityEngine.Rendering;
namespace CubeInvasion.Prototype
{
    // The root remains the simulated projectile tip. Its luminous tail never leads the collision point.
    public sealed class LaserBoltVisual : MonoBehaviour
    {
        LineRenderer core,halo,rim;
        float travelled,length;
        static Material material,rimMaterial;
        static Material RimMaterial=>rimMaterial?rimMaterial:(rimMaterial=Resources.Load<Material>("LaserRim"));
        public static Material Material=>material?material:(material=Resources.Load<Material>("LaserGlow"));
        public static LineRenderer Line(GameObject owner,Material paint,float width)
        {
            var line=owner.GetComponent<LineRenderer>();if(!line)line=owner.AddComponent<LineRenderer>();
            line.sharedMaterial=paint;line.useWorldSpace=false;line.positionCount=2;line.widthMultiplier=width;
            line.numCapVertices=4;line.numCornerVertices=0;line.textureMode=LineTextureMode.Stretch;line.alignment=LineAlignment.View;
            line.sortingOrder=2;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.lightProbeUsage=LightProbeUsage.Off;line.reflectionProbeUsage=ReflectionProbeUsage.Off;
            return line;
        }
        public void ResetVisual(Color color,float size)
        {
            if(!core)
            {
                core=Line(gameObject,Material,size*.34f);
                var glow=new GameObject("Soft laser halo");glow.transform.SetParent(transform,false);halo=Line(glow,Material,size*1.9f);halo.sortingOrder=0;
                var edge=new GameObject("Colored laser edge");edge.transform.SetParent(transform,false);rim=Line(edge,RimMaterial,size*.67f);rim.sortingOrder=1;
            }
            core.SetPropertyBlock(null);core.widthMultiplier=size*.34f;halo.widthMultiplier=size*1.9f;
            core.startColor=core.endColor=Color.Lerp(color,Color.white,.85f);
            color.a=.95f;rim.startColor=rim.endColor=color;rim.widthMultiplier=size*.67f;
            color.a=.42f;halo.startColor=halo.endColor=color;
            transform.localScale=Vector3.one;travelled=.08f;length=size*4.2f;
            Fly(Vector3.forward,0);
        }
        public void Fly(Vector3 direction,float distance)
        {
            if(direction.sqrMagnitude>.00001f)transform.rotation=Quaternion.LookRotation(direction);
            travelled+=distance;float tail=Mathf.Min(length,travelled);
            core.SetPosition(0,Vector3.back*tail);core.SetPosition(1,Vector3.zero);
            rim.SetPosition(0,Vector3.back*tail);rim.SetPosition(1,Vector3.zero);
            halo.SetPosition(0,Vector3.back*(tail+.02f));halo.SetPosition(1,Vector3.zero);
        }
    }
}
