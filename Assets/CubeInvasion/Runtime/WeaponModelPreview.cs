using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace CubeInvasion.Prototype
{
    // A small, on-demand model turntable. Inactive during combat; never changes the gameplay camera.
    public sealed class WeaponModelPreview : MonoBehaviour,IDragHandler
    {
        Camera previewCamera;GameObject stage,model;RawImage image;RenderTexture target;float nextFrame,yaw=25;string key;
        public void Show(WeaponSpec weapon)
        {
            if(key==weapon.model&&stage)return;key=weapon.model;
            if(!stage)
            {
                stage=new GameObject("Menu weapon turntable") {hideFlags=HideFlags.HideAndDontSave};stage.transform.position=new Vector3(10000,10000,10000);
                var cameraObject=new GameObject("Menu preview camera") {hideFlags=HideFlags.HideAndDontSave};cameraObject.transform.SetParent(stage.transform,false);previewCamera=cameraObject.AddComponent<Camera>();previewCamera.enabled=false;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=Color.clear;previewCamera.orthographic=true;previewCamera.nearClipPlane=.01f;previewCamera.farClipPlane=30;previewCamera.allowHDR=false;previewCamera.cullingMask=1<<30;
                target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);previewCamera.targetTexture=target;
                model=new GameObject("Displayed weapon",typeof(MeshFilter),typeof(MeshRenderer)) {hideFlags=HideFlags.HideAndDontSave,layer=30};model.transform.SetParent(stage.transform,false);model.transform.localScale=new Vector3(1,1,3.25f);
                var graphic=new GameObject("Interactive 3D preview",typeof(RectTransform),typeof(RawImage));graphic.transform.SetParent(transform,false);image=graphic.GetComponent<RawImage>();image.texture=target;var rect=image.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;image.raycastTarget=true;GetComponent<Image>().enabled=true;
            }
            var baseArt=Resources.Load<ToyArtLibrary>("ToyArtLibrary");var expansion=Resources.Load<CampaignPresentation>("CampaignPresentation");ToyArtLibrary.Model entry=null;
            if(!(expansion&&expansion.art&&expansion.art.TryGet(weapon.model,out entry))&&!baseArt.TryGet(weapon.model,out entry))return;
            model.GetComponent<MeshFilter>().sharedMesh=entry.mesh;model.GetComponent<MeshRenderer>().sharedMaterials=entry.Materials(entry.surface?null:new Material(Shader.Find("Universal Render Pipeline/Unlit")) {color=weapon.color});stage.SetActive(gameObject.activeInHierarchy);nextFrame=0;if(gameObject.activeInHierarchy)RenderNow();
        }
        public void OnDrag(PointerEventData pointer) {yaw+=pointer.delta.x*.5f;nextFrame=0;}
        void LateUpdate()
        {
            if(!stage||Time.unscaledTime<nextFrame)return;RenderNow();
        }
        public void RenderNow()
        {
            if(!stage||!model.GetComponent<MeshFilter>().sharedMesh)return;nextFrame=Time.unscaledTime+.1f;yaw+=2;
            model.transform.localRotation=Quaternion.Euler(0,yaw,0);var bounds=model.GetComponent<Renderer>().bounds;previewCamera.transform.rotation=Quaternion.Euler(25,215,0);previewCamera.transform.position=bounds.center-previewCamera.transform.forward*15;previewCamera.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.65f;previewCamera.Render();
        }
        void OnDisable() {if(stage)stage.SetActive(false);}
        void OnEnable() {if(stage) {stage.SetActive(true);nextFrame=0;}}
        void OnDestroy() {if(stage)Destroy(stage);if(target) {target.Release();Destroy(target);}}
    }
}
