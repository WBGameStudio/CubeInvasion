using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class CuteArtReview
{
    static GameObject Load(string key,UnityEngine.SceneManagement.Scene scene)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(CuteArtBuilder.Root+"/Prefabs/"+CuteArtBuilder.Slug(key)+".prefab");
        if(!prefab)throw new InvalidOperationException("Missing cute prefab "+key);
        return (GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
    }
    static Camera Camera(UnityEngine.SceneManagement.Scene scene,Color background)
    {
        var obj=new GameObject("Cute studio camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);var camera=obj.AddComponent<Camera>();camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=background;camera.orthographic=true;camera.allowHDR=false;
        camera.transform.rotation=Quaternion.Euler(22,155,0);
        for(int i=0;i<3;i++)
        {
            var lightObj=new GameObject("Soft studio light "+i);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObj,scene);var light=lightObj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=i==0?1.25f:i==1?.55f:.3f;lightObj.transform.rotation=i==0?Quaternion.Euler(40,145,0):i==1?Quaternion.Euler(20,-35,0):Quaternion.Euler(80,20,0);
        }
        return camera;
    }
    static Bounds Bounds(GameObject obj)
    {
        var renderers=obj.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);return b;
    }
    static void Save(Camera camera,int width,int height,string path,bool alpha=false)
    {
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,alpha?TextureFormat.RGBA32:TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));File.WriteAllBytes(path,texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
    }
    public static Sprite Thumbnail(string key)
    {
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var obj=Load(key,scene);var bounds=Bounds(obj);var camera=Camera(scene,Color.clear);camera.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.6f;camera.transform.position=bounds.center-camera.transform.forward*10;
            string path=CuteArtBuilder.Root+"/Thumbnails/"+CuteArtBuilder.Slug(key)+".png";Save(camera,640,640,path,true);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=512;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
    }
    [MenuItem("Cube Invasion/Art/Render Cute Model Catalog")]
    public static void Catalog()
    {
        string[] keys={"Armored body","Grunt","Sprinter","Gunner","Pusher","Sniper","Bruiser","Crusher","Pulse pistol","Twin blasters","Scatter shot","Rapid smg","Explosive barrel","Low cover","Health +20","Gold pickup"};
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var camera=Camera(scene,new Color(.85f,.91f,.96f));camera.orthographicSize=7.7f;camera.transform.position=-camera.transform.forward*30;
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/CubeInvasion/Art/UI/Reference/CubeRounded-Bold.ttf");
            for(int i=0;i<keys.Length;i++)
            {
                var obj=Load(keys[i],scene);var b=Bounds(obj);obj.transform.localScale*=2.2f/Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z));b=Bounds(obj);
                var point=camera.transform.right*((i%4-1.5f)*3.7f)+camera.transform.up*((1.5f-i/4)*3.7f+.2f);obj.transform.position+=point-b.center;
                var labelObj=new GameObject("Model label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(labelObj,scene);var label=labelObj.AddComponent<TextMesh>();label.font=font;labelObj.GetComponent<MeshRenderer>().sharedMaterial=font.material;label.text=keys[i]=="Armored body"?"SCOUT":keys[i].ToUpperInvariant();label.fontSize=64;label.characterSize=.034f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.04f,.11f,.2f);labelObj.transform.rotation=camera.transform.rotation;labelObj.transform.position=point-camera.transform.up*1.42f;
            }
            Save(camera,2048,2048,"/tmp/cube-cute-catalog.png");
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
        Debug.Log("CUBE_CUTE_CATALOG_OK");
    }
    [MenuItem("Cube Invasion/Art/Render Cute Animation Review")]
    public static void Animation()
    {
        string[] keys={"Armored body","Grunt","Sprinter","Gunner","Pusher","Sniper","Bruiser","Crusher"};
        var scene=EditorSceneManager.NewPreviewScene();var animators=new List<ToyActorAnimator>();
        var roots=new List<Transform>();var positions=new List<Vector3>();var eyes=new List<Transform>();var blinked=new bool[keys.Length];
        var library=Resources.Load<ToyArtLibrary>("ToyArtLibrary");
        try
        {
            var camera=Camera(scene,new Color(.85f,.91f,.96f));camera.orthographicSize=4.6f;camera.aspect=1280f/720;camera.transform.position=-camera.transform.forward*30;
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/CubeInvasion/Art/UI/Reference/CubeRounded-Bold.ttf");
            for(int i=0;i<keys.Length;i++)
            {
                var obj=Load(keys[i],scene);var bounds=Bounds(obj);obj.transform.localScale*=2.3f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));bounds=Bounds(obj);
                var point=camera.transform.right*((i%4-1.5f)*3.8f)+camera.transform.up*((.5f-i/4)*3.65f);
                obj.transform.position+=point-bounds.center;roots.Add(obj.transform);positions.Add(obj.transform.position);
                var animator=obj.GetComponent<ToyActorAnimator>();animator.enabled=false;
                var accessories=new List<Transform>();foreach(Transform child in obj.transform)accessories.Add(child);
                library.TryGet(keys[i],out var model);animator.Bind(model,obj.transform);foreach(var child in accessories)animator.Attach(child);
                animators.Add(animator);eyes.Add(animator.VisualRoot.Find("DOTween Reactions/Head/LeftEye"));
                var labelObj=new GameObject("Model label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(labelObj,scene);
                var label=labelObj.AddComponent<TextMesh>();label.font=font;labelObj.GetComponent<MeshRenderer>().sharedMaterial=font.material;label.text=keys[i]=="Armored body"?"SCOUT":keys[i].ToUpperInvariant();label.fontSize=48;label.characterSize=.032f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.04f,.11f,.2f);labelObj.transform.rotation=camera.transform.rotation;labelObj.transform.position=point-camera.transform.up*1.44f;
            }
            var titleObj=new GameObject("Action label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(titleObj,scene);
            var title=titleObj.AddComponent<TextMesh>();title.font=font;titleObj.GetComponent<MeshRenderer>().sharedMaterial=font.material;title.fontSize=60;title.characterSize=.035f;title.anchor=TextAnchor.MiddleCenter;title.color=new Color(.04f,.11f,.2f);titleObj.transform.rotation=camera.transform.rotation;titleObj.transform.position=camera.transform.up*4.08f;
            for(int frame=0;frame<144;frame++)
            {
                bool walk=frame>=24&&frame<60,run=frame>=60&&frame<96;
                title.text=frame<24?"IDLE / BLINK":walk?"WALK":run?"RUN / AUTO FIRE":frame<110?"ATTACK WINDUP":frame<122?"SLAM":"HIT / RECOVER";
                for(int i=0;i<animators.Count;i++)
                {
                    var animator=animators[i];if(run&&frame%8==0)animator.Shoot();if(frame==96)animator.Telegraph(.6f);if(frame==110)animator.Slam();if(frame==122)animator.Hit();
                    animator.TickPose(walk?Vector3.forward*1.2f:run?Vector3.forward*4.5f:Vector3.zero,1f/24,run);
                    if(roots[i].position!=positions[i]||roots[i].rotation!=Quaternion.identity)throw new Exception("Animation moved gameplay root: "+keys[i]);
                    if(!eyes[i])throw new Exception("Missing eye joint: "+keys[i]);blinked[i]|=eyes[i].localScale.y<.4f;
                    if(!float.IsFinite(animator.VisualRoot.localPosition.y))throw new Exception("Non-finite animation: "+keys[i]);
                }
                Save(camera,1280,720,"/tmp/cube-cute-animation/"+frame.ToString("D3")+".png");
            }
            for(int i=0;i<keys.Length;i++)if(!blinked[i])throw new Exception("Actor never blinked: "+keys[i]);
            Debug.Log("CUBE_CUTE_ANIMATION_OK: eight actors, blink, walk, run, recoil, windup, slam, hit; no gameplay root movement.");
        }
        finally
        {
            foreach(var animator in animators)if(animator)animator.SendMessage("OnDestroy",SendMessageOptions.DontRequireReceiver);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

}
