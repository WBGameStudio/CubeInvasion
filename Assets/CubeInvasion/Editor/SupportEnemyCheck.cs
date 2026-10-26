using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class SupportEnemyCheck
{
    const string Key="CI.SupportCheck";static InvasionPrototype game;static int run,step;static double deadline;static Transform player;static Camera view;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static SupportEnemyCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"exists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Prop(string n)=>typeof(InvasionPrototype).GetProperty(n,F).GetValue(game);
    static object Call(string n,params object[] a)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,a);
    static object Item(object o,string n)=>o.GetType().GetField(n).GetValue(o);
    static void Put(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
    static void Check(bool b,string m) {if(!b)throw new Exception(m);Debug.Log("SUPPORT_CHECK_OK: "+m);}
    static void Near(float a,float b,string m)=>Check(Mathf.Abs(a-b)<.005f,m+" ["+a+"]");
    static string State=>Get("phase").ToString();
    static object Spawn(ContentEnemy kind,Vector3 at)=>Call("SpawnContentEnemy",kind,at);
    static void RemoveAll()
    {foreach(var foe in new ArrayList((IList)Get("foes"))) {Object.Destroy(((Transform)Item(foe,"body")).gameObject);Object.Destroy(((Transform)Item(foe,"warning")).gameObject);}((IList)Get("foes")).Clear();Call("ClearSupportEffects");Call("ClearDeathCharges",false);if(Get("cover") is Transform cover)Object.Destroy(cover.gameObject);Set("cover",null);}
    static void Fresh() {Call("StartMission",4);Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);}
    static void KillAll() {foreach(var foe in new ArrayList((IList)Get("foes")))Call("HitEnemy",foe,10000f,Vector3.forward);}
    static void HealTick(object healer,float dt)=>Call("TickSupportEnemy",healer,dt,Vector3.zero,10f);
    static void UnitChecks()
    {
        Fresh();RemoveAll();var shield=Spawn(ContentEnemy.Shielder,new Vector3(0,.6f,0));var body=(Transform)Item(shield,"body");body.rotation=Quaternion.identity;
        Call("HitEnemy",shield,10f,Vector3.back);Near((float)Item(shield,"hp"),26.5f,"Front sector reduces projectile damage by 65 percent");
        Call("HitEnemy",shield,10f,Vector3.forward);Near((float)Item(shield,"hp"),16.5f,"Rear shot applies full damage");Call("HitEnemyArea",shield,10f,Vector3.back);Near((float)Item(shield,"hp"),6.5f,"Area damage bypasses directional guard");
        body.rotation=Quaternion.identity;Put(shield,"attack",100f);Call("TickSupportEnemy",shield,.1f,Vector3.back,3f);Check(Quaternion.Angle(body.rotation,Quaternion.identity)<=4.501f,"Shield turn speed is bounded, allowing a flank");
        for(int weapon=0;weapon<4;weapon++)
        {
            Set("weaponIndex",weapon);Fresh();RemoveAll();shield=Spawn(ContentEnemy.Shielder,new Vector3(0,.6f,0));player.position=new Vector3(0,.65f,2.5f);Call("FireContentWeapon");for(int i=0;i<24;i++)Call("TickPulses",.02f);
            Check((float)Item(shield,"hp")<30&&(float)Item(shield,"hp")>24,"Actual weapon "+weapon+" laser respects frontal guard");
        }
        Fresh();RemoveAll();shield=Spawn(ContentEnemy.Shielder,new Vector3(0,.6f,0));var healer=Spawn(ContentEnemy.Healer,new Vector3(0,.6f,2));Put(shield,"hp",10f);player.position=new Vector3(-3,.65f,-3);
        HealTick(healer,1.24f);Near((float)Item(shield,"hp"),10,"Healer cannot pulse before cooldown");HealTick(healer,.02f);Check((float)Item(healer,"windup")>0,"Heal has a visible windup");HealTick(healer,.54f);Near((float)Item(shield,"hp"),10,"No heal before telegraph completes");HealTick(healer,.02f);Near((float)Item(shield,"hp"),15,"Healer restores five HP after telegraph");
        HealTick(healer,.5f);Near((float)Item(shield,"hp"),15,"Heal cannot repeat before next 1.8s cycle");Put(shield,"hp",29f);for(int i=0;i<40;i++)HealTick(healer,.05f);Near((float)Item(shield,"hp"),30,"Healing never exceeds enemy maximum HP");
        Put(shield,"hp",10f);var state=((IDictionary)Get("supportStates"))[healer];Put(state,"clock",0f);HealTick(healer,.01f);float windup=(float)Item(healer,"windup");Call("Update");Near((float)Item(healer,"windup"),windup,"Pause freezes active healing cast");
        Call("HitEnemy",healer,10000f,Vector3.forward);Near((float)Item(shield,"hp"),10,"Killing healer interrupts pending heal");Check(!((IDictionary)Get("supportStates")).Contains(healer),"Dead healer releases its support state");
        RemoveAll();shield=Spawn(ContentEnemy.Shielder,new Vector3(0,.6f,0));healer=Spawn(ContentEnemy.Healer,new Vector3(0,.6f,2));Put(shield,"hp",10f);state=((IDictionary)Get("supportStates"))[healer];Put(state,"clock",0f);
        var wall=(Transform)Call("Shape","Low cover",PrimitiveType.Cube,new Vector3(0,.45f,1),new Vector3(1.35f,.9f,.8f),Color.gray,Get("platform"));Set("cover",wall);HealTick(healer,.1f);Check(Item(state,"target")==null,"Cover blocks healing link");Object.Destroy(wall.gameObject);Set("cover",null);HealTick(healer,.1f);Check(Item(state,"target")==shield,"Exposed injured ally can be selected");
        Call("HitEnemy",shield,10000f,Vector3.forward);Check(Item(state,"target")==null&&(float)Item(healer,"windup")==0,"Dead target cannot receive delayed healing");
        Call("StartMission",6);Call("SetPaused",true);healer=Spawn(ContentEnemy.Healer,new Vector3(0,.6f,2));Put(Get("crusherBoss"),"hp",60f);Check(!(bool)Call("CanHeal",healer,Get("crusherBoss")),"Support healing cannot restore mandatory boss phase HP");
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+240;if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Support check timeout "+run+"/"+step+" state="+State+" paused="+Get("paused")+" stage="+Get("stage")+" settled="+Get("settledPreviews"));
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;if(!player) {player=(Transform)Get("player");view=(Camera)Get("view");UnitChecks();}
            if(step==0)
            {
                Set("weaponIndex",run/2);Fresh();var mission=(MissionSpec)Prop("Mission");Check(mission.cubes[1].enemies[0]==ContentEnemy.Bomber,"Support expansion preserves Bomber cubes");KillAll();Call("Clear");Call("SetPaused",false);step=1;return;
            }
            if(step==1) {CheckChoice();if(!(bool)Get("routesReady"))return;Call("Select",run%2==0);Set("invulnerability",100f);Set("shot",100f);step=2;return;}
            if(step==2)
            {
                if(State=="Transfer")return;Call("SetPaused",true);int stage=(int)Get("stage");if(stage==4) {bool shield=false;foreach(var foe in (IList)Get("foes"))shield|=Item(foe,"kind").ToString()=="Shielder";Check(shield,"Fourth cube introduces Shielder on both routes");}
                if(stage==5&&run==0)Capture();Check((Transform)Get("player")==player&&(Camera)Get("view")==view,"Same player and camera survive mixed encounters");KillAll();Call("Clear");Check(((IDictionary)Get("supportStates")).Count==0,"Clear leaves no support beams or states");
                if(State=="Won") {Check(stage==5,"Expanded mission keeps five cubes");run++;if(run<8) {step=0;return;}Fresh();RemoveAll();Spawn(ContentEnemy.Healer,new Vector3(0,.6f,0));Call("Fail","TEST TIMEOUT",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Timeout"));Check(((IDictionary)Get("supportStates")).Count==0,"Failure cleans support state");Finish(0);return;}
                Call("SetPaused",false);step=1;
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void CheckChoice() {if(State!="Choice")throw new Exception("Expected choice run="+run+" stage="+Get("stage")+" state="+State+" foes="+((IList)Get("foes")).Count);}
    static void Capture()
    {
        Call("SetPaused",false);for(int i=0;i<12;i++) {Call("TickAnimations",.08f);((CombatFeedback)Get("feedback")).Tick(.08f);}Call("LateUpdate");var target=new RenderTexture(720,1280,24);var previous=RenderTexture.active;view.targetTexture=target;view.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=view.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=target;var img=new Texture2D(720,1280,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,720,1280),0,0);img.Apply();System.IO.File.WriteAllBytes("/tmp/cube-support-gameplay.png",img.EncodeToPNG());view.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(img);Object.DestroyImmediate(target);Call("SetPaused",true);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"exists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();if(code==0)Debug.Log("CUBE_SUPPORT_OK: frontal/rear/area damage, four real weapons, bounded turn, heal cadence/cap, interrupt/LOS/pause, boss protection, eight mixed-mission runs and cleanup.");EditorApplication.Exit(code);}
}
