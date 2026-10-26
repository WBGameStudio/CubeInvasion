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
public static class GoldRushVariantCheck
{
    const string Key="CI.GoldVariants";static InvasionPrototype game;static int run,step,wallet;static double deadline;static Transform player;static Camera view;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static GoldRushVariantCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Prop(string n)=>typeof(InvasionPrototype).GetProperty(n,F).GetValue(game);
    static object Call(string n,params object[] args)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,args);
    static object Item(object o,string n)=>o.GetType().GetField(n).GetValue(o);
    static void Check(bool ok,string m) {if(!ok)throw new Exception(m);Debug.Log("GOLD_VARIANT_OK: "+m);}
    static string State=>Get("phase").ToString();
    static void KillAdds() {var copy=new ArrayList((IList)Get("foes"));foreach(var foe in copy)Call("HitEnemy",foe,10000f,Vector3.forward);}
    static void FireAtCrate(object crate)
    {
        var at=((Transform)Item(crate,"body")).position;player.position=new Vector3(at.x,.65f,at.z-1.1f);Call("FireContentWeapon");for(int i=0;i<24;i++)Call("TickPulses",.02f);Call("ClearPulses");
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+240;if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Gold variant timeout "+run+"/"+step);
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(step==0)
            {
                if(!player) {player=(Transform)Get("player");view=(Camera)Get("view");}
                Set("weaponIndex",run/2);Call("StartMission",5);Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);
                Check((GoldRushVariant)Prop("GoldVariant")==GoldRushVariant.CoinSprint,"Gold Rush starts with Coin Sprint");KillAdds();Check(!(bool)Prop("ObjectiveComplete"),"Killing ordinary enemies cannot substitute for coin quota");
                Call("CollectPickups",0f,true);Call("Clear");Check(State=="Choice","Quota opens physical gate choices");Call("SetPaused",false);step=1;return;
            }
            if(step==1) {if(!(bool)Get("routesReady"))return;Call("Select",run%2==0);step=2;return;}
            if(step==2)
            {
                if(State=="Transfer")return;Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);
                Check((GoldRushVariant)Prop("GoldVariant")==GoldRushVariant.CrateBreaker&&((IList)Get("goldCrates")).Count==6,"Selected cube contains exactly six counted crates");KillAdds();Call("ClearPickups");Set("pendingGold",0);
                Check(!(bool)Prop("ObjectiveComplete"),"Enemy kills do not complete crate objective");
                if(run==0)Capture("GoldRush_Crates");var first=((IList)Get("goldCrates"))[0];FireAtCrate(first);
                Check((float)Item(first,"hp")<6,"Actual weapon "+run/2+" fires automatically at a nearby crate");
                var crates=new ArrayList((IList)Get("goldCrates"));for(int i=0;i<crates.Count;i++)Call("HitGoldCrate",crates[i],10000f);
                Check((int)Get("goldCratesBroken")==6&&(bool)Prop("ObjectiveComplete"),"Six broken crates satisfy objective exactly once");
                wallet=(int)Get("wallet");int bonus=(int)Prop("RouteClearBonus");Call("Clear");Check((int)Get("wallet")==wallet+58+bonus,"Six crate rewards bank only on clear");Call("Clear");Check((int)Get("wallet")==wallet+58+bonus,"Repeated clear cannot duplicate crate earnings");Call("SetPaused",false);step=3;return;
            }
            if(step==3)
            {
                if(!(bool)Get("routesReady"))return;var right=(RouteOffer)Get("rightOffer");var forward=(RouteOffer)Get("forwardOffer");
                Check(right.encounter.goldVariant!=forward.encounter.goldVariant,"Final gates expose Swarm and Jackpot separately");
                if(run==0)Capture("GoldRush_Gates");var desired=run%2==0?GoldRushVariant.GoldenSwarm:GoldRushVariant.Jackpot;Call("Select",right.encounter.goldVariant==desired);step=4;return;
            }
            if(step==4)
            {
                if(State=="Transfer")return;Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);
                Check((Transform)Get("player")==player&&(Camera)Get("view")==view,"Same player and camera continue through all Gold Rush objectives");
                var variant=(GoldRushVariant)Prop("GoldVariant");int expected;
                if(variant==GoldRushVariant.GoldenSwarm)
                {
                    var carriers=new ArrayList((IList)Get("foes"));Check(carriers.Count==5,"Swarm has exactly five required carriers");if(run==0)Capture("GoldRush_Swarm");
                    for(int i=0;i<4;i++)Call("HitEnemy",carriers[i],10000f,Vector3.forward);Set("quotaGold",60);Check(!(bool)Prop("ObjectiveComplete"),"Coin count cannot skip fifth golden carrier");Call("HitEnemy",carriers[4],10000f,Vector3.forward);expected=70;
                }
                else
                {
                    Check(variant==GoldRushVariant.Jackpot,"Other final branch is Jackpot");var chest=((IList)Get("goldCrates"))[0];Call("HitGoldCrate",chest,10000f);Check((float)Item(chest,"hp")==24&&(bool)Item(chest,"locked"),"Locked jackpot ignores damage before keys");
                    Set("quotaGold",60);Check(!(bool)Prop("ObjectiveComplete"),"Coins alone cannot unlock jackpot");var guards=new ArrayList((IList)Get("foes"));Check(guards.Count==3,"Jackpot has three key guards");
                    if(run==1)Capture("GoldRush_Jackpot");Call("HitEnemy",guards[0],10000f,Vector3.forward);Call("HitEnemy",guards[1],10000f,Vector3.forward);Check((bool)Item(chest,"locked"),"Two keys cannot unlock chest");
                    Call("HitEnemy",guards[2],10000f,Vector3.forward);Check(!(bool)Item(chest,"locked")&&!(bool)Prop("ObjectiveComplete"),"Third key unlocks chest without completing it");FireAtCrate(chest);Check((float)Item(chest,"hp")<24,"Unlocked chest is automatically targeted by actual weapon");Call("HitGoldCrate",chest,10000f);expected=79;
                }
                Check((bool)Prop("ObjectiveComplete"),"Final variant completes only its own objective");wallet=(int)Get("wallet");int bonus=(int)Prop("RouteClearBonus");Call("Clear");Check(State=="Won"&&(int)Get("wallet")==wallet+expected+bonus,"Final variant banks exact conditional reward and completes mission");
                run++;if(run<8) {step=0;return;}
                Call("StartMission",5);Call("SetPaused",true);Set("stage",2);Call("BeginCombat",(object)null);Set("invulnerability",100f);
                KillAdds();Call("ClearPickups");Set("pendingGold",0);
                var central=((IList)Get("goldCrates"))[1];var cover=(Transform)Get("cover");
                if(!cover) {cover=(Transform)Call("Shape","Low cover",PrimitiveType.Cube,Vector3.zero,new Vector3(1.35f,.9f,.8f),Color.gray,Get("platform"));Set("cover",cover);}
                var originalCover=cover.position;
                cover.position=(Vector3)Get("centre")+new Vector3(0,.45f,-2);player.position=(Vector3)Get("centre")+new Vector3(0,.65f,-3.5f);
                Call("AimAtNearestEnemy");Check(Get("goldTarget")!=central,"Automatic aim excludes a crate behind cover");
                var pulseType=typeof(InvasionPrototype).GetNestedType("Pulse",BindingFlags.NonPublic);var shot=Activator.CreateInstance(pulseType,true);
                var laser=(Transform)Call("RentProjectile",Get("pulsePool"),"Gold cover test",player.position,Color.cyan,.18f);
                pulseType.GetField("body").SetValue(shot,laser);pulseType.GetField("direction").SetValue(shot,Vector3.forward);pulseType.GetField("damage").SetValue(shot,100f);pulseType.GetField("life").SetValue(shot,1f);((IList)Get("pulses")).Add(shot);Call("TickPulses",.3f);
                Check((float)Item(central,"hp")==6&&((IList)Get("pulses")).Count==0,"Actual swept laser hits cover before the crate");cover.position=originalCover;
                var crate=((IList)Get("goldCrates"))[0];var at=((Transform)Item(crate,"body")).position;Call("CreateBarrel",at);Call("Explode",((IList)Get("barrels"))[0]);Check((int)Get("goldCratesBroken")>0,"Barrel blast can break counted crates");
                wallet=(int)Get("wallet");Call("Fail","TEST TIMEOUT",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Timeout"));Check(State=="Collapse"&&(int)Get("wallet")==wallet,"Unfinished objective times out without banking drops");Call("CompleteFailure");Call("StartMission",5);
                Check((GoldRushVariant)Prop("GoldVariant")==GoldRushVariant.CoinSprint&&(int)Get("goldKeys")==0&&(int)Get("goldCratesBroken")==0,"Retry resets temporary objective state and restarts Coin Sprint");Finish(0);
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture(string name)
    {
        bool paused=(bool)Get("paused");Call("SetPaused",false);for(int i=0;i<12;i++) {Call("TickAnimations",.08f);((CombatFeedback)Get("feedback")).Tick(.08f);}Call("LateUpdate");
        var target=new RenderTexture(720,1280,24);var previous=RenderTexture.active;view.targetTexture=target;view.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=view.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
        Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=target;var img=new Texture2D(720,1280,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,720,1280),0,0);img.Apply();Directory.CreateDirectory("/tmp/cube-goldrush-previews");File.WriteAllBytes("/tmp/cube-goldrush-previews/"+name+".png",img.EncodeToPNG());view.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(img);Object.DestroyImmediate(target);Call("SetPaused",paused);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();if(code==0)Debug.Log("CUBE_GOLDRUSH_VARIANTS_OK: eight runs, four weapons, all objectives, actual crate lasers, locked jackpot, gate continuity, exact rewards, blast, timeout and retry.");EditorApplication.Exit(code);}
}
