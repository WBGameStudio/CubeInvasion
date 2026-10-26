using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class P3SmokeCheck
{
    const string Key="CI.P3Smoke",Env="CUBE_INVASION_META_TEST_PATH";
    static InvasionPrototype game;
    static int step,runs;
    static double deadline,next;
    static Transform player;
    static Camera camera;
    static int beforeFailure,expectedRunGold;
    static string runBeforeRestart,blockedDirectory;
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static P3SmokeCheck() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        string directory=Path.Combine(Path.GetTempPath(),"cube-p3-integration-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        string file=Path.Combine(directory,"profile.json");SessionState.SetString(Key+".PreviousEnv",Environment.GetEnvironmentVariable(Env)??"");
        SessionState.SetString(Key+".Directory",directory);Environment.SetEnvironmentVariable(Env,file);
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");
        new MetaProfileStore(file,rules,7,0);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var legacyRoutes=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());legacyRoutes.FindProperty("branchSettings").objectReferenceValue=null;legacyRoutes.ApplyModifiedPropertiesWithoutUndo();SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string name)=>typeof(InvasionPrototype).GetField(name,Flags).GetValue(game);
    static void Set(string name,object value)=>typeof(InvasionPrototype).GetField(name,Flags).SetValue(game,value);
    static object Call(string name,params object[] args)=>typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static string State=>Get("phase").ToString();
    static MetaProfileStore Store=>(MetaProfileStore)Get("profile");
    static void Check(bool condition,string message) { if(!condition)throw new Exception(message);Debug.Log("P3_CHECK_OK: "+message); }
    static void KillAll()
    {
        var copy=new List<object>();foreach(var foe in (IList)Get("foes"))copy.Add(foe);
        foreach(var foe in copy)Call("HitEnemy",foe,1000f,Vector3.forward);
    }
    static void Capture(string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var target=new RenderTexture(540,960,24);var previous=RenderTexture.active;camera.targetTexture=target;
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();
        if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) { vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate(); }
        Call("RefreshContentHud");Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();
        Directory.CreateDirectory("/tmp/cube-p3-previews");File.WriteAllBytes("/tmp/cube-p3-previews/"+name+".png",image.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(deadline==0)deadline=now+120;if(now>deadline)throw new Exception("P3 timeout at "+step);
            if(now<next)return;if(!game)game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("profile")==null)return;
            switch(step)
            {
                case 0:
                    player=(Transform)Get("player");camera=(Camera)Get("view");
                    expectedRunGold=0;foreach(var encounter in ((P2ContentCatalog)Get("content")).missions[0].cubes)expectedRunGold+=10+encounter.enemies.Length*3;
                    Check(State=="Loadout"&&Store.Data.gold==0&&Store.Data.highestMission==0,"New player has zero gold and first mission only");
                    Capture("Arsenal");Call("ChangeWeapon",1);Call("BuyWeapon");
                    Check(!Store.IsOwned(1)&&Store.Data.gold==0&&(int)Get("weaponIndex")==0,"Unaffordable locked weapon cannot be equipped or granted");Capture("LockedWeapon");
                    Call("ChangeWeapon",0);Call("ChangeMission",1);Call("StartMission",1);
                    Check(State=="Loadout"&&Store.Data.run==null,"Locked mission cannot start even via direct callback");
                    Call("ChangeMission",-1);Call("StartMission",0);runBeforeRestart=Store.Data.run.id;Call("StartMission",0);
                    Check(Store.Data.run.id==runBeforeRestart&&State=="Combat","Double deploy cannot restart run or duplicate its transaction");
                    Set("invulnerability",100f);step=1;next=now+.1;break;
                case 1:
                    if(State=="Transfer")return;
                    if(State=="Combat")
                    {
                        Check((Transform)Get("player")==player&&(Camera)Get("view")==camera,"P3 retains authored player/camera through gates");
                        KillAll();Set("invulnerability",100f);Call("Update");int bank=Store.Data.gold;Call("Clear");
                        Check(Store.Data.gold==bank,"Repeated clear callback cannot award twice");
                    }
                    if(State=="Choice")
                    {
                        if(!(bool)Get("routesReady"))return;
                        Call("Select",runs%2==0);next=now+1.05;return;
                    }
                    if(State!="Won")return;
                    Check(Store.Data.run.won&&Store.Data.run.gold==expectedRunGold&&Store.Data.run.cubes==3,"Win result contains this attempt's earnings and completed cubes");
                    Check(Store.Data.highestMission==1&&Store.Data.completed[0],"First win opens second mission and persists completion");
                    runs++;
                    if(runs<5) { Call("StartMission",0);Set("invulnerability",100f);next=now+.1;return; }
                    Check(Store.Data.gold==expectedRunGold*5,"Five legitimate replays bank the exact catalog reward");Capture("Win");Call("OpenMissionMenu");Call("ChangeWeapon",1);
                    int gold=Store.Data.gold;Call("BuyWeapon");Check(Store.IsOwned(1)&&Store.Data.gold==gold-150&&(int)Get("weaponIndex")==1,"Unlock persists ownership, charges price and equips selected weapon");
                    gold=Store.Data.gold;Call("BuyWeapon");Check(Store.Data.gold==gold,"Repeated unlock press cannot double-charge");
                    Call("UpgradeWeapon",WeaponUpgrade.Damage);Call("UpgradeWeapon",WeaponUpgrade.FireRate);Call("UpgradeWeapon",WeaponUpgrade.Range);
                    Check(Store.Data.gold==expectedRunGold*5-265&&Store.Data.weapons[1].damage==1&&Store.Data.weapons[1].fireRate==1&&Store.Data.weapons[1].range==1,"Three upgrade types save exact prices and levels");
                    Capture("UpgradedWeapon");Call("StartMission",1);Set("invulnerability",100f);Set("paused",true);Call("FirePulse");
                    var pulse=((IList)Get("pulses"))[0];var type=pulse.GetType();
                    Check(Mathf.Abs((float)type.GetField("damage").GetValue(pulse)-3.675f)<.001f,"Permanent damage upgrade affects actual projectiles");
                    Check(Mathf.Abs((float)type.GetField("life").GetValue(pulse)-1.04f)<.001f,"Range upgrade affects actual projectile lifetime");
                    float interval=(float)typeof(InvasionPrototype).GetProperty("FireInterval",Flags).GetValue(game);
                    Check(Mathf.Abs(interval-.48f/1.04f)<.001f,"Fire-rate upgrade affects actual firing interval");
                    Set("paused",false);KillAll();Call("Update");step=2;next=now+.1;break;
                case 2:
                    if(State!="Choice"||!(bool)Get("routesReady"))return;
                    beforeFailure=Store.Data.gold;Call("Select",true);next=now+1.05;step=3;break;
                case 3:
                    if(State!="Combat")return;
                    Set("pendingGold",99);player.position=(Vector3)Get("centre")+new Vector3(0,.65f,-4.7f);Call("Update");
                    Check(State=="Falling","P3 keeps player-only edge fall");next=now+1.3;step=4;break;
                case 4:
                    Check(State=="Failed"&&Store.Data.gold==beforeFailure,"Fail discards active cube's unbanked gold");
                    Check(Store.Data.lastResult.cubes==1&&Store.Data.lastResult.gold==beforeFailure-(expectedRunGold*5-265),"Fail result reports only completed-cube gold");Capture("Fail");
                    Call("Retry");Check(State=="Combat"&&(int)Get("weaponIndex")==1&&Store.Data.weapons[1].damage==1,"Retry keeps selected owned weapon and permanent upgrades");
                    runBeforeRestart=Store.Data.run.id;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(game.gameObject.scene.name);game=null;next=now+.4;step=5;break;
                case 5:
                    player=(Transform)Get("player");camera=(Camera)Get("view");
                    Check(State=="Loadout"&&Store.Data.gold==beforeFailure&&Store.Data.selectedWeapon==1,"Scene/app initialization restores wallet, owned loadout and selected mission");
                    Check(Store.Data.run.id==runBeforeRestart&&Store.Data.run.ended&&!Store.Data.run.won,"Interrupted run closes safely without granting unfinished cube rewards");
                    Capture("RestoredProfile");Call("StartMission",1);Set("invulnerability",100f);
                    blockedDirectory=SessionState.GetString(Key+".Directory","");Directory.Move(blockedDirectory,blockedDirectory+"-saved");File.WriteAllText(blockedDirectory,"blocked-storage");
                    beforeFailure=Store.Data.gold;KillAll();Call("Update");
                    Check(State=="SaveBlocked"&&Store.Data.gold==beforeFailure&&(bool)Get("paused"),"Bank write failure freezes gameplay without losing or granting pending reward");Capture("SaveRecovery");
                    File.Delete(blockedDirectory);Directory.Move(blockedDirectory+"-saved",blockedDirectory);Call("RetrySave");
                    Check(State=="Choice"&&Store.Data.gold>beforeFailure&&!(bool)Get("paused"),"Storage recovery commits pending reward once and resumes clear flow");
                    gold=Store.Data.gold;Call("RetrySave");Check(Store.Data.gold==gold,"Repeated save-retry cannot duplicate committed reward");
                    Check(Object.FindObjectsByType<Camera>().Length==1,"Reload does not duplicate scene camera");
                    Debug.Log("CUBE_P3_SMOKE_OK");Finish(0);break;
            }
        }
        catch(Exception exception) { Debug.LogException(exception);Finish(1); }
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);Environment.SetEnvironmentVariable(Env,SessionState.GetString(Key+".PreviousEnv",""));
        string directory=SessionState.GetString(Key+".Directory","");
        if(!string.IsNullOrEmpty(directory))
        {
            if(File.Exists(directory))File.Delete(directory);
            if(Directory.Exists(directory+"-saved"))Directory.Move(directory+"-saved",directory);
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        EditorApplication.isPlaying=false;EditorApplication.Exit(code);
    }
}
