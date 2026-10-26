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
public static class BranchEconomyCheck
{
    const string Key="CI.BranchEconomy",Env="CUBE_INVASION_META_TEST_PATH";static InvasionPrototype game;static int step;static double deadline;static int previous;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static BranchEconomyCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        string dir=Path.Combine(Path.GetTempPath(),"cube-branch-economy-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        SessionState.SetString(Key+"env",Environment.GetEnvironmentVariable(Env)??"");SessionState.SetString(Key+"dir",dir);Environment.SetEnvironmentVariable(Env,Path.Combine(dir,"profile.json"));
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");new MetaProfileStore(Path.Combine(dir,"profile.json"),rules,7,0);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Call(string n,params object[] a)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,a);
    static void Check(bool b,string m) {if(!b)throw new Exception(m);Debug.Log("BRANCH_ECONOMY_OK: "+m);}
    static void Kill() {var copy=new ArrayList((IList)Get("foes"));foreach(var foe in copy)Call("HitEnemy",foe,10000f,Vector3.forward);Call("ClearPickups");Set("pendingGold",0);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(deadline==0)deadline=now+90;if(now>deadline)throw new Exception("Branch economy timeout");
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            var store=(MetaProfileStore)Get("profile");
            if(step==0) {Call("StartMission",0);Set("invulnerability",100f);Set("shot",100f);Kill();Call("Clear");step=1;return;}
            if(step==1)
            {
                if(!(bool)Get("routesReady"))return;
                var settings=(BranchRouteSettings)Get("branchSettings");var catalog=(P2ContentCatalog)Get("content");
                var offer=RouteDirector.Build(settings,catalog.missions[0],2,settings.rewards[(int)RouteReward.Gold],17,12);Set("rightOffer",offer);
                previous=store.Data.gold;Call("Select",true);Check(store.Data.gold==previous,"Entering gold gate never pays early");step=2;return;
            }
            if(step==2)
            {
                if(Get("phase").ToString()=="Transfer")return;Call("SetPaused",true);Kill();
                string dir=SessionState.GetString(Key+"dir","");Directory.Move(dir,dir+"-saved");File.WriteAllText(dir,"blocked-storage");
                Call("Clear");Check(Get("phase").ToString()=="SaveBlocked"&&store.Data.gold==previous,"Failed disk write retains conditional bonus without crediting it");
                File.Delete(dir);Directory.Move(dir+"-saved",dir);Call("RetrySave");
                Check(store.Data.gold==previous+50&&store.Data.run.cubes==2,"Selected gold clear durably banks base 10 plus bonus 40");
                Call("Clear");Call("RetrySave");Check(store.Data.gold==previous+50,"Repeat clear and save retry cannot double bank conditional gold");
                var loaded=new MetaProfileStore(Path.Combine(SessionState.GetString(Key+"dir",""),"profile.json"),(P3MetaSettings)Get("metaSettings"),7,0);
                Check(loaded.Data.gold==store.Data.gold,"Conditional earnings survive profile reload");
                string log=File.ReadAllText(Path.Combine(SessionState.GetString(Key+"dir",""),"ci_playtest_v1.json"));
                Check(log.Contains("branch_offered")&&log.Contains("branch_selected")&&log.Contains("branch_result")&&log.Contains("selectedDirection"),"Local branch telemetry records offers, selection, result and direction");
                Check(log.Contains("Gold / R3"),"Result retains chosen offers after future previews replace them");Finish(0);
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);Environment.SetEnvironmentVariable(Env,SessionState.GetString(Key+"env",""));if(code==0)Debug.Log("CUBE_BRANCH_ECONOMY_OK");EditorApplication.Exit(code);}
}
