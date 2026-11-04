using System;
using System.IO;
using System.Reflection;
using CubeInvasion.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Store covers rendered from the real game: plays a mission in play mode, then renders the gameplay camera at the
// exact Playgama cover sizes (square 800×800, portrait 1080×1920, landscape 1920×1080) with the in-game title font.
// Run with graphics (not -nographics); it exits the editor when done.
[InitializeOnLoad]
public static class PlaygamaCoverCapture
{
    const string Key="CI.CoverCapture",Env="CUBE_INVASION_META_TEST_PATH";
    const string Output="Documentation/Playgama/Covers/";
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static readonly (string name,int width,int height)[] Slots={("square",800,800),("portrait",1080,1920),("landscape",1920,1080)};
    static InvasionPrototype game;
    static int step;
    static bool unlocked;
    static double next;
    static PlaygamaCoverCapture() { EditorApplication.update+=Tick; }

    // Candidates: CUBE_COVER_MISSIONS="4,15,19" (0-based); CUBE_COVER_WEAPON picks the hero's weapon.
    public static void Run()
    {
        string directory=Path.Combine(Path.GetTempPath(),"cube-cover-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string file=Path.Combine(directory,"profile.json");SessionState.SetString(Key+"env",Environment.GetEnvironmentVariable(Env)??"");Environment.SetEnvironmentVariable(Env,file);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        SessionState.SetString(Key+"missions",Environment.GetEnvironmentVariable("CUBE_COVER_MISSIONS")??"4,15,26,19");
        SessionState.SetInt(Key+"weapon",int.TryParse(Environment.GetEnvironmentVariable("CUBE_COVER_WEAPON"),out int w)?w:0);
        SessionState.SetInt(Key+"index",0);
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string field)=>typeof(InvasionPrototype).GetField(field,Flags).GetValue(game);
    static void Set(string field,object value)=>typeof(InvasionPrototype).GetField(field,Flags).SetValue(game,value);
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(now<next)return;
            if(!game)game=UnityEngine.Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("profile")==null) {next=now+.2;return;}
            var missions=Array.ConvertAll(SessionState.GetString(Key+"missions","4").Split(','),int.Parse);int index=SessionState.GetInt(Key+"index",0);
            if(step==0&&!unlocked)
            {
                // Open the whole campaign through the game's own store, so its runtime campaign rules validate the save.
                var store=(MetaProfileStore)Get("profile");store.Data.highestMission=59;foreach(var owned in store.Data.weapons)owned.owned=true;
                if(!store.Select(1,0))throw new InvalidOperationException("Cover profile could not be prepared: "+store.Message);
                unlocked=true;
            }
            if(step==0)
            {
                // Back to the loadout state so StartMission accepts the next candidate.
                var phase=typeof(InvasionPrototype).GetField("phase",Flags);phase.SetValue(game,Enum.Parse(phase.FieldType,"Loadout"));
                int weapon=SessionState.GetInt(Key+"weapon",0);Set("weaponIndex",weapon);Set("inspectedWeapon",weapon);
                typeof(InvasionPrototype).GetMethod("StartMission",Flags).Invoke(game,new object[]{missions[index]});
                if(Get("phase").ToString()!="Combat")throw new InvalidOperationException($"Mission {missions[index]} did not start: phase={Get("phase")} notice={Get("metaNotice")} highest={((MetaProfileStore)Get("profile")).Data.highestMission} path={((MetaProfileStore)Get("profile")).PathName} env={Environment.GetEnvironmentVariable(Env)}");
                step=1;next=now+2.6;return; // Enemies spawn in and the hero starts firing.
            }
            Capture(missions[index]);
            index++;SessionState.SetInt(Key+"index",index);step=0;next=now+.3;
            if(index>=missions.Length) {Debug.Log("CUBE_COVERS_OK "+Path.GetFullPath(Output));Finish(0);}
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture(int mission)
    {
        var view=(Camera)Get("view");var hud=(MobileCombatHud)Get("hud");
        bool hudOn=hud.Canvas.enabled;hud.Canvas.enabled=false;
        var frontEndArt=Resources.Load<FrontEndArt>("FrontEndArt");
        var title=TitleCanvas(view,frontEndArt?frontEndArt.uiFont:null);
        Directory.CreateDirectory(Output);
        foreach(var (name,width,height) in Slots)
        {
            // Rendered at 2× and filtered down for smooth edges (MSAA targets read back black in batch mode).
            var target=new RenderTexture(width*2,height*2,24);var final=new RenderTexture(width,height,0);view.targetTexture=target;
            var follow=view.GetComponent<ArcadeFollowCamera>();follow.SendMessage("Update");
            var brain=view.GetComponent<Cinemachine.CinemachineBrain>();
            if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
            // Cover framing: a little closer than gameplay, with the arena lowered so the title sits over the backdrop.
            float zoom=name=="landscape"?.8f:name=="square"?.86f:.96f;view.orthographicSize*=zoom;
            view.transform.position+=view.transform.up*view.orthographicSize*2*(name=="portrait"?.1f:.13f);
            Canvas.ForceUpdateCanvases();view.Render();
            target.filterMode=FilterMode.Bilinear;Graphics.Blit(target,final);
            var old=RenderTexture.active;RenderTexture.active=final;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
            File.WriteAllBytes($"{Output}m{mission+1:00}_{name}_{width}x{height}.png",texture.EncodeToPNG());
            RenderTexture.active=old;view.targetTexture=null;UnityEngine.Object.DestroyImmediate(texture);target.Release();final.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(final);
        }
        UnityEngine.Object.DestroyImmediate(title);hud.Canvas.enabled=hudOn;
    }
    // Title band in the game's own UI font, drawn by the gameplay camera like the HUD.
    static GameObject TitleCanvas(Camera view,Font font)
    {
        var root=new GameObject("Cover title",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=view;canvas.planeDistance=1;canvas.sortingOrder=60;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1080);scaler.matchWidthOrHeight=.5f;
        var band=new GameObject("Title area",typeof(RectTransform));band.transform.SetParent(root.transform,false);
        var bandRect=(RectTransform)band.transform;bandRect.anchorMin=new Vector2(0,1);bandRect.anchorMax=new Vector2(1,1);bandRect.pivot=new Vector2(.5f,1);bandRect.sizeDelta=new Vector2(0,300);
        var label=new GameObject("Title",typeof(RectTransform),typeof(Text));label.transform.SetParent(band.transform,false);
        var rect=(RectTransform)label.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=new Vector2(70,10);rect.offsetMax=new Vector2(-70,-10);
        var text=label.GetComponent<Text>();text.font=font?font:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.text="CUBE INVASION";
        text.fontSize=150;text.resizeTextForBestFit=true;text.resizeTextMinSize=40;text.resizeTextMaxSize=150;text.alignment=TextAnchor.MiddleCenter;text.color=VoidToyPalette.Text;
        var outline=label.AddComponent<Outline>();outline.effectColor=VoidToyPalette.Cyan;outline.effectDistance=new Vector2(4,-4);
        var shadow=label.AddComponent<Shadow>();shadow.effectColor=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.9f);shadow.effectDistance=new Vector2(8,-10);
        return root;
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);Environment.SetEnvironmentVariable(Env,SessionState.GetString(Key+"env",""));
        EditorApplication.isPlaying=false;EditorApplication.Exit(code);
    }
}
