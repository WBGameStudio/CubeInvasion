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
public static class BranchRoutesCheck
{
    const string Key="CI.BranchCheck";static InvasionPrototype game;static int step;static double next,deadline;
    static RouteOffer committed;static Transform player;static Camera view;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static BranchRoutesCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"exists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet"));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Call(string n,params object[] a)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,a);
    static void Check(bool b,string m) {if(!b)throw new Exception(m);Debug.Log("BRANCH_CHECK_OK: "+m);}
    static void Phase(string n)=>Set("phase",Enum.Parse(Get("phase").GetType(),n));
    static void Kill()
    {var copy=new ArrayList((IList)Get("foes"));foreach(var foe in copy)Call("HitEnemy",foe,10000f,Vector3.forward);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        if(deadline==0)deadline=EditorApplication.timeSinceStartup+90;
        try
        {
            var now=EditorApplication.timeSinceStartup;if(now>deadline)throw new Exception("Branch timeout step "+step);if(now<next)return;
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            var settings=(BranchRouteSettings)Get("branchSettings");var catalog=(P2ContentCatalog)Get("content");
            if(step==0)
            {
                player=(Transform)Get("player");view=(Camera)Get("view");
                foreach(var reward in settings.rewards)
                {
                    Call("StartMission",2);Call("SetPaused",true);Kill();Call("FinishEncounterFeel",false);
                    Set("stage",2);var offer=RouteDirector.Build(settings,catalog.missions[2],2,reward,99,12);Set("activeOffer",offer);
                    Set("health",50f);Call("ApplyRouteReward",reward);Call("BeginCombat",reward);Set("invulnerability",0f);
                    if(reward.reward==RouteReward.Health)Check((float)Get("health")==80,"Health applies immediately");
                    if(reward.reward==RouteReward.Damage)Check(Mathf.Abs((float)Get("damageBonus")-.15f)<.001f,"Damage capped and applied");
                    if(reward.reward==RouteReward.FireRate)
                    {
                        float interval=(float)typeof(InvasionPrototype).GetProperty("FireInterval",F).GetValue(game);Check(Mathf.Abs(interval-catalog.weapons[0].interval/1.12f)<.001f,"Fire rate affects actual weapon interval");
                        Call("ApplyRouteReward",reward);Check((int)Get("routeRateCubes")==2&&(float)Get("routeRateBonus")<.121f,"Repeated buff refreshes without stacking");
                        Call("ExpireRouteBuffs");Check((int)Get("routeRateCubes")==1,"First clear retains one cube");Call("ExpireRouteBuffs");Check((float)Get("routeRateBonus")==0,"Second clear expires rate buff");
                    }
                    if(reward.reward==RouteReward.Shield)
                    {Call("DamagePlayer",45f,Vector3.zero);Check((float)Get("health")==45&&(float)Get("routeShield")==0,"Shield absorbs 40; overflow damages HP");}
                    if(reward.reward==RouteReward.Blast)Check(((IList)Get("barrels")).Count==2,"Blast encounter supplies two barrels");
                    if(reward.reward==RouteReward.Magnet)
                    {
                        Call("ClearPickups");Call("SpawnPickup",player.position+Vector3.right*1.2f,false,5);Call("SpawnPickup",player.position+Vector3.left*1.2f,true,20);Call("CollectPickups",0f,false);
                        Check(((IList)Get("pickups")).Count==1&&(float)Get("health")==50,"Magnet extends coin reach only");
                    }
                    foreach(var foe in (IList)Get("foes"))
                    {var body=(Transform)foe.GetType().GetField("body").GetValue(foe);Check(Vector3.ProjectOnPlane(body.position-player.position,Vector3.up).magnitude>=1.799f,"Landing area stays clear");}
                    if(reward.reward==RouteReward.Elite)
                    {var foe=((IList)Get("foes"))[offer.eliteIndex];Check((float)foe.GetType().GetField("attackPower").GetValue(foe)>1,"Elite has stronger attacks and visual crest");}
                    if(reward.reward==RouteReward.Gold||reward.reward==RouteReward.Elite)
                    {
                        int wallet=(int)Get("wallet");Call("ClearPickups");Kill();Call("ClearPickups");Set("pendingGold",0);Call("Clear");
                        Check((int)Get("wallet")==wallet+10+(int)reward.value,"Conditional bonus banks only on clear");Call("Clear");Check((int)Get("wallet")==wallet+10+(int)reward.value,"Repeated clear cannot double pay");
                    }
                }
                Call("StartMission",2);Call("SetPaused",true);Kill();Call("Clear");Call("SetPaused",false);step=1;next=now+1.2;return;
            }
            if(step==1)
            {
                if(!(bool)Get("routesReady"))return;
                Check(true,"Both candidate cubes finish rising");committed=(RouteOffer)Get("forwardOffer");
                Check(((RouteOffer)Get("rightOffer")).reward.reward!=committed.reward.reward,"Live gate offers differ");
                Call("LateUpdate");Capture("BranchGates");
                var centre=(Vector3)Get("centre");player.position=centre+new Vector3(0,.65f,4.1f);
                Call("CheckGateCrossing",player.position,centre+new Vector3(0,.65f,4.2f));Check(Get("phase").ToString()=="Transfer","Walking through gate commits the route");
                Call("SetPaused",false);Set("invulnerability",100f);step=2;next=now+1.05;return;
            }
            if(step==2)
            {
                if(Get("phase").ToString()=="Transfer")return;
                Call("SetPaused",true);Check((Transform)Get("player")==player&&(Camera)Get("view")==view,"Player and camera identity persist across animated transfer");
                Check(Get("activeOffer")==committed&&(int)Get("stage")==2,"Future preview preparation cannot overwrite selected encounter");
                Check(((IList)Get("foes")).Count==committed.encounter.enemies.Length,"Selected branch spawns its own encounter");
                Set("activeOffer",RouteDirector.Build(settings,catalog.missions[2],2,settings.rewards[(int)RouteReward.Gold],45,12));
                var shield=settings.rewards[(int)RouteReward.Shield];Call("ApplyRouteReward",shield);int before=(int)Get("wallet");
                Call("Fail","TEST FALL",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"EdgeFall"));
                Check(Get("phase").ToString()=="Falling"&&((Transform)Get("platform")).position.y==0,"Shield cannot prevent edge fall and floor stays in place");
                Check((int)Get("wallet")==before,"Failed route pays no conditional bonus");Call("CompleteFailure");Check((float)Get("routeShield")==0&&(float)Get("routeRateBonus")==0,"Failure resets temporary buffs");
                Call("StartMission",2);Call("SetPaused",true);Call("ApplyRouteReward",shield);
                Call("Fail","TEST TIMEOUT",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Timeout"));
                Check(Get("phase").ToString()=="Collapse","Shield cannot prevent timer collapse");
                Finish(0);return;
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture(string name)
    {
        var target=new RenderTexture(720,1280,24);var previous=RenderTexture.active;view.targetTexture=target;
        var rig=view.GetComponent<ArcadeFollowCamera>();rig.SendMessage("Update");var brain=view.GetComponent<Cinemachine.CinemachineBrain>();
        if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
        Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=target;var image=new Texture2D(720,1280,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,720,1280),0,0);image.Apply();Directory.CreateDirectory("/tmp/cube-branch-previews");File.WriteAllBytes("/tmp/cube-branch-previews/"+name+".png",image.EncodeToPNG());view.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"exists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();
        if(code==0)Debug.Log("CUBE_BRANCH_ROUTES_OK");EditorApplication.Exit(code);
    }
}
