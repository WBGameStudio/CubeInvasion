using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class CrusherPhaseCheck
{
    const string Key="CI.CrusherCheck";static InvasionPrototype game;static int step,run;static double deadline;static Transform player;static Camera camera;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static CrusherPhaseCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Call(string n,params object[] a)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,a);
    static object Item(object o,string n)=>o.GetType().GetField(n).GetValue(o);
    static void Put(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
    static string State=>Get("phase").ToString();
    static object Boss=>Get("crusherBoss");
    static void Check(bool condition,string m) {if(!condition)throw new Exception(m);Debug.Log("CRUSHER_CHECK_OK: "+m);}
    static void Near(float actual,float expected,string m)=>Check(Mathf.Abs(actual-expected)<.002f,m+" ["+actual+"]");
    static void KillAdds()
    {var copy=new ArrayList((IList)Get("foes"));foreach(var foe in copy)if(foe!=Boss)Call("HitEnemy",foe,10000f,Vector3.forward);Call("ClearPickups");Set("pendingGold",0);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(deadline==0)deadline=now+240;if(now>deadline)throw new Exception("Crusher check timeout "+run+"/"+step);
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(step==0)
            {
                if(!player) {player=(Transform)Get("player");camera=(Camera)Get("view");}
                Set("weaponIndex",run/2);Call("StartMission",6);Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);KillAdds();
                var boss=Boss;var body=(Transform)Item(boss,"body");body.rotation=Quaternion.identity;player.position=body.position+Vector3.back*2.5f;player.position=new Vector3(player.position.x,.65f,player.position.z);
                Near((float)Get("bossHealth"),252,"Shared boss health starts at 252");var panels=(Transform[])Get("crusherArmor");
                Check(panels[0]&&panels[1]&&panels[0].gameObject.activeSelf&&panels[1].gameObject.activeSelf,"Both authored Void Toy armor panels are bound");
                if(run==0)Capture("Crusher_Armor");
                Call("FireContentWeapon");for(int i=0;i<30;i++)Call("TickPulses",.02f);
                Check((float)Item(boss,"hp")<72,"Actual weapon "+run/2+" laser hits the selected armor panel");Call("ClearPulses");
                float otherPanel=((float[])Get("crusherArmorHealth"))[1];Call("HitEnemyAt",boss,10000f,Vector3.forward,0);Near((float)Item(boss,"hp"),otherPanel,"One overkill hit breaks only one armor panel");
                Check(!panels[0].gameObject.activeSelf&&panels[1].gameObject.activeSelf,"Only broken authored panel hides; second stays visible");
                float before=(float)Get("bossHealth");Call("HitEnemyAt",boss,10000f,Vector3.forward,0);Near((float)Get("bossHealth"),before,"Broken armor cannot be damaged twice");
                Call("HitEnemyAt",boss,10000f,Vector3.forward,1);Near((float)Get("bossHealth"),180,"Armor phase spends exactly 72 shared health");
                Check(((IList)Get("foes")).Count==0,"Two panels and support clearance complete phase one");Call("Clear");Check(State=="Choice","Objective waits for walking-gate choice");
                Call("SetPaused",false);step=1;return;
            }
            if(step==1)
            {
                if(!(bool)Get("routesReady"))return;Near((float)Get("bossHealth"),180,"Waiting for gates never resets boss bar");Call("Select",run%2==0);step=2;return;
            }
            if(step==2)
            {
                if(State=="Transfer")return;Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);
                Check(State=="Combat"&&(int)Get("stage")==2,"Gate enters mandatory support phase");
                var boss=Boss;var body=(Transform)Item(boss,"body");var drones=(Array)Get("crusherDrones");float hp=0;
                foreach(var drone in drones) {Check(drone!=null,"Three explicit support targets exist");hp+=(float)Item(drone,"hp");}Near(hp,84,"Drone health shares the original 84 HP phase budget");
                Check(((IList)Get("foes")).Count==4,"Preview count matches three drones and protected boss");
                Call("HitEnemy",boss,10000f,Vector3.forward);Near((float)Get("bossHealth"),180,"Protected body cannot bypass support objective");
                player.position=body.position+Vector3.right*.75f;Call("AimAtNearestEnemy");Check(Get("currentTarget")!=null&&Get("currentTarget")!=boss,"Automatic targeting skips protected boss");
                if(run==0)Capture("Crusher_Support");
                // Locked cone direction gives the player a real escape window.
                var pose=body.position;player.position=pose+Vector3.left*1.9f;Set("routeShield",0f);Set("health",100f);Set("invulnerability",0f);Put(boss,"attack",0f);Put(boss,"spawn",0f);Put(boss,"windup",0f);
                Call("TickSpecialEnemy",boss,.01f,Vector3.left,1.9f);Check((float)Item(boss,"windup")>=1.19f,"Push attack always telegraphs before damage");
                player.position=pose+Vector3.right*1.9f;Call("TickSpecialEnemy",boss,1.3f,Vector3.right,1.9f);Near((float)Get("health"),100,"Stepping out of locked cone avoids push damage");
                player.position=pose+Vector3.left*1.9f;Put(boss,"attack",0f);Call("TickSpecialEnemy",boss,.01f,Vector3.left,1.9f);Call("TickSpecialEnemy",boss,1.3f,Vector3.left,1.9f);
                Check((float)Get("health")<100&&((Vector3)Get("knockback")).magnitude<=6.001f,"Remaining in cone damages and pushes with bounded strength");Set("invulnerability",100f);
                var first=drones.GetValue(0);float firstHp=(float)Item(first,"hp");Call("HitEnemy",first,10000f,Vector3.forward);Near((float)Get("bossHealth"),180-firstHp,"Drone damage advances shared bar once");
                Check(((IList)Get("foes")).Contains(boss),"One drone cannot finish support phase");
                Call("HitEnemy",drones.GetValue(1),10000f,Vector3.forward);Call("HitEnemy",drones.GetValue(2),10000f,Vector3.forward);
                Near((float)Get("bossHealth"),96,"Third drone disables boss and leaves exactly 96 core HP");Check(((IList)Get("foes")).Count==0,"Protected body cannot softlock clear");
                Call("Clear");Call("SetPaused",false);step=3;return;
            }
            if(step==3)
            {if(!(bool)Get("routesReady"))return;Call("Select",run%2==0);step=4;return;}
            if(step==4)
            {
                if(State=="Transfer")return;Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);Check((int)Get("stage")==3,"Both routes preserve core phase");Near((float)Get("bossHealth"),96,"Core bar never resets at gate");
                Check((Transform)Get("player")==player&&(Camera)Get("view")==camera,"Same player and camera survive all three phases");
                var panels=(Transform[])Get("crusherArmor");Check(!panels[0].gameObject.activeSelf&&!panels[1].gameObject.activeSelf,"Broken armor does not regrow in core phase");
                if(run==0)Capture("Crusher_Core");KillAdds();Call("HitEnemy",Boss,10000f,Vector3.forward);Near((float)Get("bossHealth"),0,"Core kill reduces shared health to zero");Call("Clear");
                Check(State=="Won","Boss level ends only after all phase objectives");run++;
                if(run<8) {step=0;return;}
                // One blast may eliminate all drones and retire the boss in the same callback.
                Call("StartMission",6);Call("SetPaused",true);KillAdds();Call("HitEnemyAt",Boss,10000f,Vector3.forward,0);Call("HitEnemyAt",Boss,10000f,Vector3.forward,1);
                Set("stage",2);Call("BeginCombat",(object)null);Set("invulnerability",100f);
                var blastAt=(Vector3)Get("centre")+new Vector3(0,.5f,1.8f);int n=0;
                foreach(var foe in (IList)Get("foes")) {var body=(Transform)Item(foe,"body");body.position=blastAt+Vector3.right*(n++*.2f);}
                Call("CreateBarrel",blastAt);Call("Explode",((IList)Get("barrels"))[0]);
                Check(((IList)Get("foes")).Count==0,"One barrel can safely kill all drones and retire protected boss");Near((float)Get("bossHealth"),96,"Blast chain advances phase budget exactly once");
                // Failure clears the run, and a retry restores authored panels and phase state.
                Call("StartMission",6);Call("SetPaused",true);Call("HitEnemyAt",Boss,10000f,Vector3.forward,0);
                Call("Fail","TEST TIMEOUT",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Timeout"));Check(State=="Collapse","Incomplete armor objective still fails on timeout");
                Call("CompleteFailure");Call("StartMission",6);Near((float)Get("bossHealth"),252,"Retry restores health and starts at first phase");Check(((Transform[])Get("crusherArmor"))[0].gameObject.activeSelf,"Retry restores authored broken panel");Finish(0);
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture(string name)
    {
        Call("SetPaused",false);for(int i=0;i<12;i++) {Call("TickAnimations",.08f);((CombatFeedback)Get("feedback")).Tick(.08f);}
        Call("LateUpdate");var target=new RenderTexture(720,1280,24);var previous=RenderTexture.active;camera.targetTexture=target;
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;var picture=new Texture2D(720,1280,TextureFormat.RGB24,false);picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();Directory.CreateDirectory("/tmp/cube-crusher-previews");File.WriteAllBytes("/tmp/cube-crusher-previews/"+name+".png",picture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(picture);Object.DestroyImmediate(target);Call("SetPaused",true);
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();
        if(code==0)Debug.Log("CUBE_CRUSHER_PHASES_OK: four weapons, both gate directions, eight full boss runs; authored armor, actual lasers, support protection, telegraph dodge, health continuity, timeout and retry.");EditorApplication.Exit(code);
    }
}
