using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    /// <summary>DOTween-driven rigid-part rig. All motion is visual, never root motion.</summary>
    public sealed class ToyActorAnimator : MonoBehaviour
    {
        public enum Motion { Idle, Walk, Run }
        [SerializeField] bool selfDriven;
        [SerializeField] string prefabModelKey;
        public void ConfigurePrefab(string key) {prefabModelKey=key;selfDriven=true;}
        void Start()
        {
            if(!selfDriven||ready)return;
            var library=Resources.Load<ToyArtLibrary>("ToyArtLibrary");
            if(!library||!library.TryGet(prefabModelKey,out var model))return;
            var accessories=new List<Transform>();foreach(Transform child in transform)accessories.Add(child);
            Bind(model,transform);foreach(var child in accessories)Attach(child);
        }
        void LateUpdate() {if(selfDriven)TickFromRoot(Time.deltaTime,false,true,false);}
        sealed class Joint { public string name;public Transform pivot;public bool left,arm,back; }
        readonly List<Joint> joints=new List<Joint>();
        readonly List<Tween> reactions=new List<Tween>();
        readonly List<Renderer> renderers=new List<Renderer>();
        Transform motionRoot, gaitRoot, reactionRoot;
        Renderer original;
        Tween gait, idle, blend;
        float gaitPhase,idlePhase,weight;
        Vector3 previous,localDirection;
        Motion mode;
        bool shooting,visible=true,ready;
        public Motion CurrentMotion => mode;
        public Transform VisualRoot => gaitRoot;
        public Renderer CoreRenderer { get; private set; }
        public int JointCount => joints.Count;
        public void Bind(ToyArtLibrary.Model model,Transform root)
        {
            if(model==null||model.rig==null||model.rig.Length==0)return;
            motionRoot=root;previous=root.position;
            original=GetComponent<Renderer>();var materials=original.sharedMaterials;
            gaitRoot=new GameObject("DOTween Gait").transform;gaitRoot.SetParent(transform,false);
            reactionRoot=new GameObject("DOTween Reactions").transform;reactionRoot.SetParent(gaitRoot,false);
            foreach(var part in model.rig)
            {
                var joint=new GameObject(part.name,typeof(MeshFilter),typeof(MeshRenderer));joint.transform.SetParent(reactionRoot,false);joint.transform.localPosition=part.pivot;
                joint.GetComponent<MeshFilter>().sharedMesh=part.mesh;
                var renderer=joint.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderers.Add(renderer);
                if(part.name=="Core")CoreRenderer=renderer;
                else joints.Add(new Joint {name=part.name,pivot=joint.transform,left=part.name.StartsWith("Left"),arm=part.name.EndsWith("Arm"),back=part.name.Contains("Back")});
            }
            original.enabled=false;
            gait=DOTween.To(()=>gaitPhase,v=>gaitPhase=v,1,.65f).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(UpdateType.Manual);
            idle=DOTween.To(()=>idlePhase,v=>idlePhase=v,1,2.4f).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(UpdateType.Manual);
            ready=true;
        }
        public void Attach(Transform accessory)
        {
            if(!ready||!accessory)return;
            accessory.SetParent(reactionRoot,true);
            foreach(var renderer in accessory.GetComponentsInChildren<Renderer>())renderers.Add(renderer);
        }
        public void SetVisible(bool value)
        {
            visible=value;if(original)original.enabled=false;
            foreach(var renderer in renderers)if(renderer)renderer.enabled=value;
        }
        public void TickFromRoot(float dt,bool paused,bool allowMotion,bool firing)
        {
            if(!ready||!motionRoot)return;
            Vector3 delta=motionRoot.position-previous;previous=motionRoot.position;
            if(paused||dt<=0)return;
            Vector3 velocity=allowMotion&&delta.sqrMagnitude<4?Vector3.ProjectOnPlane(delta,Vector3.up)/dt:Vector3.zero;
            TickPose(velocity,dt,firing);
        }
        public void TickPose(Vector3 velocity,float dt,bool firing)
        {
            if(!ready||dt<=0)return;
            shooting=firing;
            float speed=velocity.magnitude;
            var next=speed<.12f?Motion.Idle:speed<2.8f?Motion.Walk:Motion.Run;
            if(next!=mode)
            {
                mode=next;blend?.Kill();
                blend=DOTween.To(()=>weight,v=>weight=v,next==Motion.Idle?0:1,.12f).SetEase(Ease.OutSine).SetUpdate(UpdateType.Manual);
            }
            if(speed>.12f)localDirection=motionRoot.InverseTransformDirection(velocity.normalized);
            idle.ManualUpdate(dt,dt);
            if(speed>.12f)gait.ManualUpdate(dt*(mode==Motion.Run?1.65f:Mathf.Clamp(speed, .65f,1.4f)),dt);
            if(blend!=null&&blend.IsActive())blend.ManualUpdate(dt,dt);
            for(int i=reactions.Count-1;i>=0;i--)
            {
                if(!reactions[i].IsActive()) {reactions.RemoveAt(i);continue;}
                reactions[i].ManualUpdate(dt,dt);
            }
            float wave=Mathf.Sin(gaitPhase*Mathf.PI*2),stride=(mode==Motion.Run?34:23)*weight;
            foreach(var joint in joints)
            {
                float sign=(joint.left?1:-1)*(joint.back?-1:1);
                float swing=wave*stride*sign*(joint.arm?(shooting?.22f:-.6f):1);
                joint.pivot.localRotation=Quaternion.Euler(localDirection.z*swing,0,-localDirection.x*swing);
            }
            gaitRoot.localPosition=Vector3.up*(Mathf.Sin(idlePhase*Mathf.PI*2)*.008f+Mathf.Abs(wave)*weight*(mode==Motion.Run?.075f:.04f));
            gaitRoot.localRotation=Quaternion.Euler(localDirection.z*weight*(mode==Motion.Run?7:3),0,-localDirection.x*weight*5);
            if(original)original.enabled=false;
        }
        public void Shoot()
        {
            if(!ready)return;
            reactions.Add(reactionRoot.DOPunchPosition(Vector3.back*.035f,.12f,3,.4f).SetUpdate(UpdateType.Manual));
        }
        public void Slam()
        {
            if(!ready)return;
            reactions.Add(reactionRoot.DOPunchPosition(Vector3.down*.14f,.26f,3,.35f).SetUpdate(UpdateType.Manual));
            reactions.Add(reactionRoot.DOPunchRotation(Vector3.right*15,.26f,3,.35f).SetUpdate(UpdateType.Manual));
        }
        public void Hit()
        {
            if(!ready)return;
            // Keep scale reactions separate from the gait joints and recoil position.
            reactions.Add(reactionRoot.DOPunchScale(Vector3.one*.065f,.18f,3,.4f).SetUpdate(UpdateType.Manual));
        }
        public void Telegraph(float duration)
        {
            if(!ready)return;
            reactions.Add(reactionRoot.DOPunchRotation(Vector3.right*8,Mathf.Max(.1f,duration),2,.3f).SetUpdate(UpdateType.Manual));
        }
        public void Spawn()
        {
            if(!ready)return;
            reactionRoot.localScale=Vector3.one*.05f;
            reactions.Add(reactionRoot.DOScale(Vector3.one,.35f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Manual));
        }
        public Transform DetachVisual(Transform parent)
        {
            if(!ready)return null;
            SetVisible(true);
            gaitRoot.SetParent(parent,true);var detached=gaitRoot;gaitRoot=null;ready=false;return detached;
        }
        void OnDestroy()
        {
            gait?.Kill();idle?.Kill();blend?.Kill();foreach(var tween in reactions)tween.Kill();
        }
    }
}
