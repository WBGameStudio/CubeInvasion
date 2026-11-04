using System;
using System.IO;
using CubeInvasion.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Transparent character renders for the store covers, from the game's own models. Tools/compose_covers.py builds the art.
// Run with graphics: CUBE_LAYER_OUT=<folder> Unity -batchmode -executeMethod PlaygamaCoverLayers.Run (exits when done).
[InitializeOnLoad]
public static class PlaygamaCoverLayers
{
    const string Key="CI.CoverLayers";
    const int Layer=30,Size=1400;
    static readonly (string key,string file,float yaw,float pitch)[] Shots=
    {
        ("Rogue cube","hero",-28,-7),("Grunt","grunt",24,-4),("Sprinter","sprinter",34,-4),("Gunner","gunner",-34,-5),
        ("Pusher","pusher",28,-4),("Bruiser","bruiser",-26,-4),("Crusher","crusher",22,-7),("Sniper","sniper",30,-4),
    };
    static double start;
    static PlaygamaCoverLayers() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        SessionState.SetString(Key+"out",Environment.GetEnvironmentVariable("CUBE_LAYER_OUT")??"");
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        if(start==0) {start=EditorApplication.timeSinceStartup;return;}
        if(EditorApplication.timeSinceStartup-start<2)return;
        SessionState.SetBool(Key,false);
        int code=0;
        try { foreach(var shot in Shots)Render(shot.key,shot.file,shot.yaw,shot.pitch);Debug.Log("CUBE_LAYERS_OK"); }
        catch(Exception e) {Debug.LogException(e);code=1;}
        EditorApplication.isPlaying=false;EditorApplication.Exit(code);
    }
    static void Render(string key,string file,float yaw,float pitch)
    {
        var library=Resources.Load<ToyArtLibrary>("ToyArtLibrary");var extra=Resources.Load<CampaignPresentation>("CampaignPresentation");
        if(!library.TryGet(key,out var model)&&!(extra&&extra.art&&extra.art.TryGet(key,out model)))throw new Exception("Missing model "+key);
        var stage=new GameObject("Cover stage");stage.transform.position=new Vector3(-20000,20000,20000);
        var body=new GameObject(key,typeof(MeshFilter),typeof(MeshRenderer)).transform;body.SetParent(stage.transform,false);
        body.GetComponent<MeshFilter>().sharedMesh=model.mesh;
        var paint=new Material(Shader.Find("Universal Render Pipeline/Unlit"));paint.color=VoidToyPalette.Graphite;
        body.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(paint);
        ToyActorAnimator rig=null;
        if(model.rig!=null&&model.rig.Length>0) {rig=body.gameObject.AddComponent<ToyActorAnimator>();rig.Bind(model,body);}
        if(key=="Rogue cube")
        {
            var faces=Resources.Load<ToyArtLibrary>("RogueCubeFaces");
            if(faces.TryGet(key+"/Determined",out var face))
            {
                Mesh Part(string name) {foreach(var part in face.rig)if(part.name==name)return part.mesh;return null;}
                rig.SetFace(Part("LeftEye"),Part("RightEye"),Part("Mouth"));
            }
        }
        foreach(var child in body.GetComponentsInChildren<Transform>(true))child.gameObject.layer=Layer;
        body.localRotation=Quaternion.Euler(0,180+yaw,0);if(rig)rig.TickPose(Vector3.zero,.02f,false);
        var cameraObject=new GameObject("Cover camera");cameraObject.transform.SetParent(stage.transform,false);
        var view=cameraObject.AddComponent<Camera>();view.enabled=false;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(0,0,0,0);view.cullingMask=1<<Layer;view.fieldOfView=24;view.nearClipPlane=.05f;view.farClipPlane=100;view.allowHDR=false;view.allowMSAA=false;
        var bounds=new Bounds(body.position,Vector3.zero);foreach(var renderer in body.GetComponentsInChildren<Renderer>())if(renderer.enabled)bounds.Encapsulate(renderer.bounds);
        float distance=bounds.extents.magnitude*1.15f/Mathf.Sin(view.fieldOfView*.5f*Mathf.Deg2Rad);
        view.transform.rotation=Quaternion.Euler(pitch,0,0);view.transform.position=bounds.center-view.transform.forward*distance;
        var target=new RenderTexture(Size*2,Size*2,24,RenderTextureFormat.ARGB32);var final=new RenderTexture(Size,Size,0,RenderTextureFormat.ARGB32);view.targetTexture=target;view.Render();
        target.filterMode=FilterMode.Bilinear;Graphics.Blit(target,final);
        var old=RenderTexture.active;RenderTexture.active=final;var image=new Texture2D(Size,Size,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,Size,Size),0,0);image.Apply();
        File.WriteAllBytes(Path.Combine(SessionState.GetString(Key+"out",""),file+".png"),image.EncodeToPNG());
        RenderTexture.active=old;view.targetTexture=null;target.Release();final.Release();UnityEngine.Object.DestroyImmediate(stage);
    }
}
