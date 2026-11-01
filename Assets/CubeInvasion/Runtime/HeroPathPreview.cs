using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace CubeInvasion.Prototype
{
    // The level path's marker: KIP's real rig rendered off-screen. Idles (breathing, blink, antenna) on its node,
    // hops now and then, and walks along the path when the current level moves on.
    public sealed class HeroPathPreview : MonoBehaviour
    {
        const string HeroKey="Rogue cube";const int Layer=30;const float RestYaw=20,WalkTime=.75f;
        Camera previewCamera;GameObject stage;Transform body;ToyActorAnimator rig;RawImage image;RenderTexture target;
        float nextFrame,nextHop,yaw=RestYaw;int index=-1;Vector2 lastPosition;Tween walk,hop;
        RectTransform Rect=>(RectTransform)transform;
        public bool Ready=>rig;
        public void Place(int level,Vector2 position)
        {
            Build();
            bool moved=rig&&index>=0&&level!=index&&gameObject.activeInHierarchy;index=level;
            walk?.Kill();
            if(!moved) {Rect.anchoredPosition=position;lastPosition=position;return;}
            walk=Rect.DOAnchorPos(position,WalkTime).SetEase(Ease.InOutSine).SetUpdate(true).SetLink(gameObject);
        }
        void Build()
        {
            if(stage||!Application.isPlaying)return;
            var library=Resources.Load<ToyArtLibrary>("ToyArtLibrary");
            if(!library||!library.TryGet(HeroKey,out var model)||model.rig==null||model.rig.Length==0)return;
            stage=new GameObject("Path hero stage") {hideFlags=HideFlags.HideAndDontSave};stage.transform.position=new Vector3(-10000,10000,10000);
            var cameraObject=new GameObject("Path hero camera") {hideFlags=HideFlags.HideAndDontSave};cameraObject.transform.SetParent(stage.transform,false);previewCamera=cameraObject.AddComponent<Camera>();previewCamera.enabled=false;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=Color.clear;previewCamera.orthographic=true;previewCamera.nearClipPlane=.01f;previewCamera.farClipPlane=30;previewCamera.allowHDR=false;previewCamera.cullingMask=1<<Layer;
            target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);previewCamera.targetTexture=target;
            body=new GameObject("Path hero",typeof(MeshFilter),typeof(MeshRenderer)).transform;body.SetParent(stage.transform,false);
            body.GetComponent<MeshFilter>().sharedMesh=model.mesh;body.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(null);
            rig=body.gameObject.AddComponent<ToyActorAnimator>();rig.Bind(model,body);
            var faces=Resources.Load<ToyArtLibrary>("RogueCubeFaces");
            if(faces&&faces.TryGet(HeroKey+"/Happy",out var face))
            {
                Mesh Part(string name) {foreach(var part in face.rig)if(part.name==name)return part.mesh;return null;}
                rig.SetFace(Part("LeftEye"),Part("RightEye"),Part("Mouth"));
            }
            foreach(var child in body.GetComponentsInChildren<Transform>(true))child.gameObject.layer=Layer;
            var graphic=new GameObject("Path hero view",typeof(RectTransform),typeof(RawImage));graphic.transform.SetParent(transform,false);image=graphic.GetComponent<RawImage>();image.texture=target;image.raycastTarget=false;
            var rect=image.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            GetComponent<Image>().enabled=false;
            // Frame once on the rest pose, with headroom for the hop.
            body.localRotation=Quaternion.Euler(0,RestYaw,0);rig.TickPose(Vector3.zero,.02f,false);
            var bounds=new Bounds(body.position,Vector3.zero);foreach(var renderer in body.GetComponentsInChildren<Renderer>())if(renderer.enabled)bounds.Encapsulate(renderer.bounds);
            previewCamera.transform.rotation=Quaternion.Euler(14,180,0);previewCamera.transform.position=bounds.center+Vector3.up*bounds.size.y*.08f-previewCamera.transform.forward*15;
            previewCamera.orthographicSize=Mathf.Max(bounds.size.y,bounds.size.x)*.62f;
            nextHop=Time.unscaledTime+2.5f;
        }
        void LateUpdate()
        {
            if(!rig)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);if(dt<=0)return;
            // Screen up is away from the camera (world -Z), screen right is the camera's right (world -X).
            var step=(Rect.anchoredPosition-lastPosition)/dt;lastPosition=Rect.anchoredPosition;
            bool walking=walk!=null&&walk.IsActive()&&step.sqrMagnitude>1;var direction=walking?new Vector3(-step.x,0,-step.y).normalized:Vector3.zero;
            float goal=walking?Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg:RestYaw;
            yaw=Mathf.LerpAngle(yaw,goal,1-Mathf.Exp(-dt*(walking?14:6)));body.localRotation=Quaternion.Euler(0,yaw,0);
            rig.TickPose(direction*(walking?2.4f:0),dt,false);
            if(!walking&&Time.unscaledTime>nextHop&&(hop==null||!hop.IsActive()))
            {
                nextHop=Time.unscaledTime+Random.Range(3.5f,6f);rig.Hit();
                hop=body.DOLocalJump(Vector3.zero,.35f,1,.42f).SetUpdate(true).SetLink(gameObject);
            }
            if(Time.unscaledTime<nextFrame)return;nextFrame=Time.unscaledTime+1/30f;previewCamera.Render();
        }
        void OnDisable() {if(stage)stage.SetActive(false);}
        void OnEnable() {if(stage) {stage.SetActive(true);nextFrame=0;lastPosition=Rect.anchoredPosition;}}
        void OnDestroy() {walk?.Kill();hop?.Kill();if(stage)Destroy(stage);if(target) {target.Release();Destroy(target);}}
    }
}
