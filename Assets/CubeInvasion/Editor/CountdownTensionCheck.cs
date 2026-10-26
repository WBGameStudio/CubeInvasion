using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class CountdownTensionCheck
{
    const string Key="CI.CountdownCheck";static double deadline;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static InvasionPrototype game;
    static CountdownTensionCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();
        var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string name)=>typeof(InvasionPrototype).GetField(name,Flags).GetValue(game);
    static void Set(string name,object value)=>typeof(InvasionPrototype).GetField(name,Flags).SetValue(game,value);
    static object Call(string name,params object[] args)=>typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static void Check(bool value,string message) {if(!value)throw new Exception(message);Debug.Log("COUNTDOWN_CHECK_OK: "+message);}
    static void Phase(string name)=>Set("phase",Enum.Parse(Get("phase").GetType(),name));
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        if(deadline==0)deadline=EditorApplication.timeSinceStartup+60;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Countdown check timeout");
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            Call("StartMission",1);Set("paused",true);Set("invulnerability",100f);
            var view=(Camera)Get("view");var rig=view.GetComponent<ArcadeFollowCamera>();var shake=Object.FindAnyObjectByType<CountdownCameraShake>();
            Check(shake&&shake.GetComponent<CinemachineVirtualCamera>(),"Countdown effect is authored on the existing virtual camera");
            var player=(Transform)Get("player");var start=player.position;var heading=player.rotation;
            foreach(float remaining in new[]{20f,6f,5f,0f})
            {Check(!shake.Drive(remaining,true,.016f)&&shake.CurrentOffset==Vector3.zero,"No shake or beat at "+remaining+" seconds");}
            float early=0,late=0;int earlyBeats=0,lateBeats=0;
            for(int pass=0;pass<2;pass++)
            {
                shake.ResetUrgency();
                for(int i=0;i<240;i++)
                {
                    bool beat=shake.Drive(pass==0?4.5f:.5f,true,1f/60);if(beat) {if(pass==0)earlyBeats++;else lateBeats++;}
                    float amplitude=shake.CurrentOffset.magnitude;
                    CheckBound(amplitude<=shake.MaximumDisplacement+.000001f&&float.IsFinite(amplitude));
                    if(pass==0)early=Mathf.Max(early,amplitude);else late=Mathf.Max(late,amplitude);
                }
            }
            Check(late>early*3&&lateBeats>earlyBeats,"Shake amplitude and heartbeat pace increase toward timeout");
            var state=CameraState.Default;state.RawPosition=new Vector3(3,7,2);state.RawOrientation=Quaternion.Euler(45,45,0);var raw=state.RawPosition;var orientation=state.RawOrientation;
            shake.InvokePostPipelineStageCallback(shake.VirtualCamera,CinemachineCore.Stage.Noise,ref state,.016f);
            Check(state.RawPosition==raw&&state.RawOrientation==orientation&&state.PositionCorrection.sqrMagnitude>0,"Cinemachine adds only bounded positional noise, preserving raw pose and heading");
            var expected=state.PositionCorrection;
            state=CameraState.Default;state.RawPosition=raw;state.RawOrientation=orientation;
            shake.InvokePostPipelineStageCallback(shake.VirtualCamera,CinemachineCore.Stage.Noise,ref state,.016f);
            Check(state.PositionCorrection==expected,"Repeated pipeline evaluations do not advance noise or accumulate drift");
            Call("SetPaused",false);Set("remaining",1f);Call("TickCountdownPresentation",.25f);Call("LateUpdate");
            var hud=(MobileCombatHud)Get("hud");var timer=(Text)typeof(MobileCombatHud).GetField("timer",Flags).GetValue(hud);var normalScale=Vector3.one;
            var rest=(Vector3)typeof(MobileCombatHud).GetField("timerRestScale",Flags).GetValue(hud);normalScale=rest;
            Check(shake.Strength>0&&timer.rectTransform.localScale.x>=normalScale.x,"Live countdown drives camera and timer presentation");
            Call("SetPaused",true);Check(shake.CurrentOffset==Vector3.zero&&shake.Strength==0&&timer.rectTransform.localScale==normalScale,"Pause immediately removes shake and restores timer scale");
            foreach(string phase in new[]{"Choice","Transfer","Collapse","Falling","Dying","Failed","Won","Loadout"})
            {
                Set("paused",false);Phase("Combat");Set("remaining",1f);Call("TickCountdownPresentation",.2f);Phase(phase);Call("RefreshCountdownHud");
                Check(shake.CurrentOffset==Vector3.zero&&shake.Strength==0,"No countdown noise during "+phase);
            }
            Phase("Combat");Set("remaining",20f);Call("TickCountdownPresentation",.016f);Check(shake.Strength==0,"A fresh cube starts without leftover tension");
            Check(player.position==start&&player.rotation==heading,"Countdown presentation never moves or rotates the character");
            // Controlled five-second countdown in the actual gameplay camera, without advancing enemy AI.
            Call("StartMission",1);Set("paused",false);Set("invulnerability",100f);var originalAngle=view.transform.rotation;
            var cameraData=new SerializedObject(rig);var vcam=(CinemachineVirtualCamera)cameraData.FindProperty("virtualCamera").objectReferenceValue;
            var brain=view.GetComponent<CinemachineBrain>();vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();
            Directory.CreateDirectory("/tmp/cube-countdown-frames");
            for(int i=0;i<120;i++)
            {
                float remaining=4.99f-i*(4.89f/119);Set("remaining",remaining);Call("TickCountdownPresentation",1f/24);Call("LateUpdate");
                vcam.UpdateCameraState(Vector3.up,1f/24);brain.ManualUpdate();Save(view,"/tmp/cube-countdown-frames/"+i.ToString("D3")+".png");
                CheckBound(Quaternion.Angle(view.transform.rotation,originalAngle)<.01f);
            }
            Check(player.position==start,"Recorded countdown leaves the player and input orientation unchanged");
            Set("remaining",20f);Call("TickCountdownPresentation",.016f);Call("RefreshCountdownHud");
            Check(shake.CurrentOffset==Vector3.zero&&timer.rectTransform.localScale==normalScale,"Countdown reset has no residual shake or timer scaling");
            Debug.Log("CUBE_COUNTDOWN_TENSION_OK: strict five-second threshold, bounded increasing noise, faster beats, stable heading, no drift, phase/pause/reset cleanup and Unity render review.");Finish(0);
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void CheckBound(bool value) {if(!value)throw new Exception("Non-finite, unbounded or rotating countdown shake");}
    static void Save(Camera view,string path)
    {
        var rt=new RenderTexture(540,960,24);view.targetTexture=rt;var old=RenderTexture.active;Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=rt;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        view.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();EditorApplication.Exit(code);}
}
