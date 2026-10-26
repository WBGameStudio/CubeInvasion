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
        sealed class Joint { public string name;public Transform pivot;public Vector3 rest;public bool left,arm,back; }
        readonly List<Joint> joints=new List<Joint>();
        readonly List<Tween> reactions=new List<Tween>();
        readonly List<Renderer> renderers=new List<Renderer>();
        Transform motionRoot, gaitRoot, reactionRoot;
        Renderer original;
        Tween gait, idle, blend,windup,recoil;
        float anticipation,kick;
        bool integratedFace,mechanical;
        float restHeight;
        Transform weaponVisual;
        MeshFilter weaponSource,weaponCopy;
        Renderer weaponSourceRenderer,weaponCopyRenderer;
        public bool IsMechanical=>mechanical;
        public Transform WeaponVisual=>weaponVisual;
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
            if(ready||model==null||model.rig==null||model.rig.Length==0)return;
            integratedFace=model.integratedFace;mechanical=model.mechanicalRig;restHeight=mechanical&&model.key!="Armored body"?.22f:0;
            motionRoot=root;previous=root.position;
            original=GetComponent<Renderer>();var materials=model.surface?model.Materials(null):original.sharedMaterials;
            gaitRoot=new GameObject("DOTween Gait").transform;gaitRoot.SetParent(transform,false);
            reactionRoot=new GameObject("DOTween Reactions").transform;reactionRoot.SetParent(gaitRoot,false);
            var hierarchy=new Dictionary<string,Transform>();
            var high=new List<Renderer>();var low=new List<Renderer>();
            foreach(var part in model.rig)
            {
                var joint=new GameObject(part.name,typeof(MeshFilter),typeof(MeshRenderer));joint.transform.SetParent(reactionRoot,false);joint.transform.localPosition=part.pivot;
                joint.GetComponent<MeshFilter>().sharedMesh=part.mesh;hierarchy[part.name]=joint.transform;
                var renderer=joint.GetComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderers.Add(renderer);high.Add(renderer);
                if(part.lodMesh)
                {
                    var reduced=new GameObject("LOD1",typeof(MeshFilter),typeof(MeshRenderer));reduced.transform.SetParent(joint.transform,false);
                    reduced.GetComponent<MeshFilter>().sharedMesh=part.lodMesh;var reducedRenderer=reduced.GetComponent<MeshRenderer>();reducedRenderer.sharedMaterials=materials;renderers.Add(reducedRenderer);low.Add(reducedRenderer);
                }
                if(part.name=="Core")CoreRenderer=renderer;
                else joints.Add(new Joint {name=part.name,pivot=joint.transform,left=part.name.StartsWith("Left"),arm=part.name.EndsWith("Arm"),back=part.name.Contains("Back")});
            }
            foreach(var part in model.rig)if(!string.IsNullOrEmpty(part.parent)&&hierarchy.TryGetValue(part.parent,out var parent))hierarchy[part.name].SetParent(parent,true);
            foreach(var joint in joints)joint.rest=joint.pivot.localPosition;
            if(low.Count==high.Count&&low.Count>0)
            {
                var levels=gaitRoot.gameObject.AddComponent<LODGroup>();levels.SetLODs(new[]{new LOD(.08f,high.ToArray()),new LOD(.012f,low.ToArray())});levels.RecalculateBounds();
            }
            original.enabled=false;
            gait=DOTween.To(()=>gaitPhase,v=>gaitPhase=v,1,.65f).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(UpdateType.Manual);
            idle=DOTween.To(()=>idlePhase,v=>idlePhase=v,1,2.4f).SetEase(Ease.Linear).SetLoops(-1).SetUpdate(UpdateType.Manual);
            idle.ManualUpdate(Mathf.Repeat(Mathf.Abs(GetInstanceID())*.173f,2.4f),0);
            ready=true;
        }
        public void Attach(Transform accessory)
        {
            if(!ready||!accessory)return;
            if(integratedFace&&(accessory.name=="Visor"||accessory.name=="Threat crest"||accessory.name=="Gunner barrel"||accessory.name=="Twin cannon")) {accessory.gameObject.SetActive(false);return;}
            if(mechanical&&(accessory.name=="Pulse pistol"||accessory.name=="Pulse Pistol"))
            {
                // Keep the simulation's fire anchor fixed; only animate its visible mesh.
                weaponSource=accessory.GetComponent<MeshFilter>();weaponSourceRenderer=accessory.GetComponent<Renderer>();
                if(weaponSource&&weaponSourceRenderer)
                {
                    weaponVisual=new GameObject("Articulated weapon",typeof(MeshFilter),typeof(MeshRenderer)).transform;
                    weaponVisual.position=accessory.position;weaponVisual.rotation=accessory.rotation;weaponVisual.localScale=accessory.lossyScale;
                    Transform hand=null;foreach(var joint in joints)if(joint.name=="RightHand") {hand=joint.pivot;break;}weaponVisual.SetParent(hand?hand:reactionRoot,true);
                    weaponCopy=weaponVisual.GetComponent<MeshFilter>();weaponCopyRenderer=weaponVisual.GetComponent<Renderer>();renderers.Add(weaponCopyRenderer);SyncWeapon();return;
                }
            }
            accessory.SetParent(reactionRoot,true);
            foreach(var renderer in accessory.GetComponentsInChildren<Renderer>())renderers.Add(renderer);
        }
        void SyncWeapon()
        {
            if(!weaponSource||!weaponCopy)return;
            if(weaponCopy.sharedMesh!=weaponSource.sharedMesh)weaponCopy.sharedMesh=weaponSource.sharedMesh;
            if(weaponCopyRenderer.sharedMaterial!=weaponSourceRenderer.sharedMaterial)weaponCopyRenderer.sharedMaterial=weaponSourceRenderer.sharedMaterial;
            weaponSourceRenderer.enabled=false;
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
            SyncWeapon();shooting=firing;
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
            float wave=Mathf.Sin(gaitPhase*Mathf.PI*2),stride=(mode==Motion.Run?37:24)*weight;
            float breathe=Mathf.Sin(idlePhase*Mathf.PI*2);
            float blinkT=(idlePhase-.78f)/.085f,blink=blinkT>0&&blinkT<1?1-Mathf.Sin(blinkT*Mathf.PI)*.92f:1;
            foreach(var joint in joints)
            {
                joint.pivot.localPosition=joint.rest;
                // Bolted-on armour (boss panels) rides the reaction root but never swings.
                if(joint.name.EndsWith("Armor"))continue;
                if(joint.name.EndsWith("Eye")) {joint.pivot.localScale=new Vector3(1,blink,1);continue;}
                if(joint.name=="Head") { joint.pivot.localRotation=Quaternion.Euler(-weight*2+breathe*1.2f,Mathf.Sin(idlePhase*Mathf.PI*2+.8f)*3,breathe*2-weight*localDirection.x*3);continue; }
                if(joint.name.EndsWith("Ear")) {float lag=Mathf.Sin(gaitPhase*Mathf.PI*2-.55f);joint.pivot.localRotation=Quaternion.Euler(lag*weight*17+breathe*3,0,(joint.left?-1:1)*(weight*7+lag*weight*6));continue;}
                if(joint.name=="Tail") {joint.pivot.localRotation=Quaternion.Euler(wave*weight*10,breathe*12+wave*weight*15,0);continue;}
                float sign=(joint.left?1:-1)*(joint.back?-1:1);
                if(mechanical&&joint.name.EndsWith("Shin"))
                {
                    joint.pivot.localRotation=Quaternion.Euler(Mathf.Max(0,wave*sign)*weight*(mode==Motion.Run?62:36)+anticipation*16,0,0);continue;
                }
                if(mechanical&&joint.name.EndsWith("Hand"))
                { joint.pivot.localRotation=Quaternion.Euler(-anticipation*18+kick*14,0,(joint.left?-1:1)*anticipation*8);continue; }
                float swing=wave*stride*sign*(joint.arm?(shooting?.16f:-.65f):1);
                joint.pivot.localRotation=Quaternion.Euler(localDirection.z*swing*(mechanical&&!joint.arm?-1:1)-(joint.arm?anticipation*42+kick*13:0),0,-localDirection.x*swing+(joint.arm?(joint.left?-1:1)*anticipation*12:0));
                if(!joint.arm)joint.pivot.localPosition+=Vector3.up*Mathf.Max(0,wave*sign)*weight*(mode==Motion.Run?.11f:.065f);
            }
            gaitRoot.localPosition=Vector3.up*(restHeight-anticipation*(mechanical?.07f:0)+breathe*.012f+Mathf.Abs(wave)*weight*(mode==Motion.Run?.08f:.045f));
            gaitRoot.localScale=new Vector3(1+breathe*.006f,1-breathe*.009f,1+breathe*.006f);
            gaitRoot.localRotation=Quaternion.Euler(localDirection.z*weight*(mode==Motion.Run?8:3),0,-localDirection.x*weight*6);
            if(original)original.enabled=false;
        }
        public void Shoot()
        {
            if(!ready)return;
            anticipation=0;windup?.Kill();kick=1;recoil?.Kill();recoil=DOTween.To(()=>kick,v=>kick=v,0,.14f).SetUpdate(UpdateType.Manual);reactions.Add(recoil);
            reactions.Add(reactionRoot.DOPunchPosition(Vector3.back*.035f,.12f,3,.4f).SetUpdate(UpdateType.Manual));
        }
        public void Slam()
        {
            if(!ready)return;
            anticipation=0;windup?.Kill();kick=1;recoil?.Kill();recoil=DOTween.To(()=>kick,v=>kick=v,0,.3f).SetUpdate(UpdateType.Manual);reactions.Add(recoil);
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
            windup?.Kill();anticipation=0;
            windup=DOTween.Sequence().Append(DOTween.To(()=>anticipation,v=>anticipation=v,1,Mathf.Max(.08f,duration*.65f)).SetEase(Ease.InQuad))
                .AppendInterval(duration*.35f).Append(DOTween.To(()=>anticipation,v=>anticipation=v,0,.18f)).SetUpdate(UpdateType.Manual);
            reactions.Add(windup);
        }
        public void Spawn()
        {
            if(!ready)return;
            reactionRoot.localScale=Vector3.one*.05f;
            reactions.Add(reactionRoot.DOScale(Vector3.one,.35f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Manual));
        }
        public void ResetForReuse(Transform root)
        {
            foreach(var tween in reactions)tween.Kill();reactions.Clear();windup?.Kill();recoil?.Kill();anticipation=kick=weight=0;previous=root.position;visible=true;
            if(reactionRoot) {reactionRoot.localPosition=Vector3.zero;reactionRoot.localRotation=Quaternion.identity;reactionRoot.localScale=Vector3.one;}SetVisible(true);if(CoreRenderer)CoreRenderer.SetPropertyBlock(null);
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
