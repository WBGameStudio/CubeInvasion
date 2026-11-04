using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using CubeInvasion.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Plays the first-session and rewarded flows on a throwaway profile with scripted ads.
// Run with graphics (not -nographics): -executeMethod PlaygamaFlowCheck.Run. It exits the editor when done.
[InitializeOnLoad]
public static class PlaygamaFlowCheck
{
    const string Key="CI.FlowCheck",MetaEnv="CUBE_INVASION_META_TEST_PATH",LiveEnv="CUBE_INVASION_LIVE_FLOW";
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    sealed class ScriptedAds : IAdService
    {
        public int interstitials,rewarded;public bool grant=true;
        public bool RewardedSupported=>true;
        public bool Busy=>false;
        public IEnumerator Interstitial(string placement,Action done) { interstitials++;done();yield break; }
        public IEnumerator Rewarded(string placement,Action reward,Action done) { rewarded++;if(grant)reward();done();yield break; }
    }
    static InvasionPrototype game;static ScriptedAds ads;static int step,failures;static double next;
    static readonly StringBuilder log=new StringBuilder();
    static PlaygamaFlowCheck() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        string directory=Path.Combine(Path.GetTempPath(),"cube-flow-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        Environment.SetEnvironmentVariable(MetaEnv,Path.Combine(directory,"profile.json"));Environment.SetEnvironmentVariable(LiveEnv,"1");
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string field)=>typeof(InvasionPrototype).GetField(field,Flags).GetValue(game);
    static void Set(string field,object value)=>typeof(InvasionPrototype).GetField(field,Flags).SetValue(game,value);
    static void Call(string method,params object[] args)=>typeof(InvasionPrototype).GetMethod(method,Flags).Invoke(game,args);
    static string State=>Get("phase").ToString();
    static void SetPhase(string name) { var field=typeof(InvasionPrototype).GetField("phase",Flags);field.SetValue(game,Enum.Parse(field.FieldType,name)); }
    static void Check(bool ok,string name) { log.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)failures++; }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        double now=EditorApplication.timeSinceStartup;if(now<next)return;
        try
        {
            if(!game)game=UnityEngine.Object.FindAnyObjectByType<InvasionPrototype>();
            if(!game||Get("profile")==null||Get("overlays")==null) {next=now+.2;return;}
            var store=(MetaProfileStore)Get("profile");
            switch(step)
            {
                case 0:
                    ads=new ScriptedAds();AdManager.Service=ads;
                    Check(State=="Combat"&&store.Data.run!=null&&store.Data.run.mission==0,"New player lands in mission 1 without the menu");
                    Check((bool)Get("startHold")&&((IList)Get("foes")).Count==2,"First cube holds for the first move and has two enemies");
                    step=1;next=now+.5;return;
                case 1:
                    Check(PlaygamaService.GameReadySent,"game_ready is sent although the session opens straight into a mission");
                    Check(Mathf.Approximately((float)Get("remaining"),20),"Clock stays at 20 s until the player moves");
                    Call("OnApplicationFocus",false);Check(!(bool)Get("paused"),"Losing focus while waiting for the first move keeps the start prompt");Call("OnApplicationFocus",true);
                    Call("ReleaseStartHold");Set("remaining",.01f);step=2;next=now+.3;return;
                case 2:
                    Check(State=="Combat"&&(float)Get("remaining")>15,"First mission grants extra time instead of a collapse");
                    Call("Fail","Out of health",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Health"));
                    Check(State=="Revive"&&((IList)Get("foes")).Count>0,"Failure offers a continue with the cube frozen as it was");
                    Call("AcceptRevive");step=3;next=now+.2;return;
                case 3:
                    Check(State=="Combat"&&ads.rewarded==1&&(float)Get("health")>=60&&(bool)Get("startHold")&&(bool)Get("revivedThisRun"),"Rewarded continue restores health and waits for a move");
                    Call("ReleaseStartHold");
                    Call("Fail","Out of health",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Health"));
                    Check(State!="Revive","A run gets one continue");
                    step=4;next=now+1;return;
                case 4:
                    Check(State=="Failed","Second failure reaches the result screen");
                    Call("ResultRetry");
                    Check(State=="Combat"&&ads.interstitials==0,"Retry in onboarding shows no interstitial");
                    // Mission 1 done from here on.
                    GamePrefs.SetInt((string)Get("tutorialPreferenceKey"),1);store.Data.completed[0]=true;store.Data.highestMission=1;
                    for(int i=0;i<3;i++) {SetPhase("Failed");Call("ResultRetry");}
                    int free=ads.interstitials;SetPhase("Failed");Call("ResultRetry");
                    Check(free==0&&ads.interstitials==1,"First three retries are ad-free, the fourth asks for an interstitial");
                    SetPhase("Failed");Call("OpenMissionMenu");Call("ChangeWeapon",1);
                    var menu=(MenuFrontEnd)Get("frontEnd");menu.ShowPage(MenuPage.Arsenal,false);
                    step=5;next=now+.6;return;
                case 5:
                    menu=(MenuFrontEnd)Get("frontEnd");
                    var trial=menu.transform.GetComponentsInChildren<UnityEngine.UI.Button>(true);bool found=false;foreach(var b in trial)if(b.name=="Weapon trial"&&b.gameObject.activeInHierarchy)found=true;
                    Check(found,"Locked weapon offers a rewarded trial");
                    Capture("flow_weapon_trial.png");
                    int rewardedBefore=ads.rewarded;Call("WatchWeaponTrial");
                    Check(ads.rewarded==rewardedBefore+1&&State=="Combat"&&(int)Get("weaponIndex")==1&&store.Data.selectedWeapon==0&&!store.Data.weapons[1].owned,"Trial runs the locked weapon without owning or saving it");
                    SetPhase("Failed");Call("OpenMissionMenu");
                    Check((int)Get("weaponIndex")==0,"Leaving the run returns to the owned weapon");
                    Finish();return;
            }
        }
        catch(Exception e) {Debug.LogException(e);failures++;Finish();}
    }
    static void Capture(string file)
    {
        var view=(Camera)Get("view");var target=new RenderTexture(1280,720,24);view.targetTexture=target;Canvas.ForceUpdateCanvases();view.Render();
        var old=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
        File.WriteAllBytes(Path.Combine(Path.GetTempPath(),file),image.EncodeToPNG());RenderTexture.active=old;view.targetTexture=null;target.Release();
        Debug.Log("CUBE_FLOW_CAPTURE "+Path.Combine(Path.GetTempPath(),file));
    }
    static void Finish()
    {
        Debug.Log(log.ToString());Debug.Log(failures==0?"CUBE_FLOW_CHECK_OK":"CUBE_FLOW_CHECK_FAILED "+failures);
        SessionState.SetBool(Key,false);if(game)GamePrefs.SetInt((string)Get("tutorialPreferenceKey"),0);
        Environment.SetEnvironmentVariable(MetaEnv,null);Environment.SetEnvironmentVariable(LiveEnv,null);
        AdManager.Service=null;EditorApplication.isPlaying=false;EditorApplication.Exit(failures==0?0:1);
    }
}
