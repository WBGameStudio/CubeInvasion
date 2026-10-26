using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class CombatFeedback
    {
        sealed class Flash
        {
            public Transform body,anchor;public LineRenderer jet,flare,glow;
            public Vector3 localPoint,direction;public Color color;public float age,strength;
        }
        readonly List<Flash> flashes=new List<Flash>();
        readonly Stack<Flash> flashPool=new Stack<Flash>();
        public int ActiveMuzzleFlashes=>flashes.Count;
        public void Muzzle(Transform anchor,Vector3 localPoint,Vector3 direction,Color color,float strength=1)
        {
            if(frozen||!anchor)return;
            MakeEffectRoom();
            Flash flash;
            if(flashPool.Count>0)flash=flashPool.Pop();
            else
            {
                var body=new GameObject("Pooled energy muzzle flash").transform;body.SetParent(transform,false);
                flash=new Flash {body=body,jet=LaserBoltVisual.Line(body.gameObject,LaserBoltVisual.Material,.08f)};
                var cross=new GameObject("Energy petals");cross.transform.SetParent(body,false);flash.flare=LaserBoltVisual.Line(cross,LaserBoltVisual.Material,.065f);
                var halo=new GameObject("Muzzle glow");halo.transform.SetParent(body,false);flash.glow=LaserBoltVisual.Line(halo,LaserBoltVisual.Material,.5f);flash.glow.sortingOrder=0;
            }
            flash.anchor=anchor;flash.localPoint=localPoint;flash.direction=direction.normalized;flash.color=color;flash.age=0;flash.strength=strength;
            flash.body.gameObject.SetActive(true);flashes.Add(flash);PoseFlash(flash,0);
        }
        void MakeEffectRoom()
        {
            while(ActiveEffects>=48)
            {if(flashes.Count>0)RecycleFlash(0);else Recycle(0);}
        }
        void PoseFlash(Flash f,float t)
        {
            f.body.position=f.anchor.TransformPoint(f.localPoint);f.body.rotation=Quaternion.LookRotation(f.direction);
            float fade=1-t,scale=f.strength*(.45f+.55f*fade);
            f.jet.SetPosition(0,Vector3.back*.04f);f.jet.SetPosition(1,Vector3.forward*.46f*scale);
            f.flare.SetPosition(0,new Vector3(-.19f*scale,0,.06f));f.flare.SetPosition(1,new Vector3(.19f*scale,0,.06f));
            f.glow.SetPosition(0,Vector3.back*.07f);f.glow.SetPosition(1,Vector3.forward*.13f);
            f.jet.widthMultiplier=.11f*scale;f.flare.widthMultiplier=.085f*scale;f.glow.widthMultiplier=.65f*scale;
            var core=Color.Lerp(f.color,Color.white,.85f);core.a=fade;
            var glow=f.color;glow.a=.4f*fade;
            f.jet.startColor=f.jet.endColor=core;f.flare.startColor=f.flare.endColor=core;f.glow.startColor=f.glow.endColor=glow;
        }
        void TickMuzzles(float dt)
        {
            for(int i=flashes.Count-1;i>=0;i--)
            {
                var f=flashes[i];f.age+=dt;
                if(!f.anchor||!f.anchor.gameObject.activeInHierarchy||f.age>=.095f) {RecycleFlash(i);continue;}
                PoseFlash(f,f.age/.095f);
            }
        }
        void RecycleFlash(int i)
        {var f=flashes[i];f.body.gameObject.SetActive(false);f.anchor=null;flashPool.Push(f);flashes.RemoveAt(i);}
        public void ClearMuzzles() {while(flashes.Count>0)RecycleFlash(flashes.Count-1);}
    }
}
