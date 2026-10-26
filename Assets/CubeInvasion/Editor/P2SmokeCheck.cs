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
public static class P2SmokeCheck
{
    const string Key="CI.P2Smoke";
    static InvasionPrototype game;
    static P2ContentCatalog catalog;
    static int step,mission,pass;
    static double deadline,next;
    static Transform player,world;
    static Camera camera;
    static Vector3 floor;
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static P2SmokeCheck() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        SessionState.SetBool(Key+".WalletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+".Wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();
        var legacyRoutes=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());legacyRoutes.FindProperty("branchSettings").objectReferenceValue=null;legacyRoutes.FindProperty("crusherPhases").objectReferenceValue=null;legacyRoutes.FindProperty("goldRushVariants").objectReferenceValue=null;legacyRoutes.FindProperty("bomberSettings").objectReferenceValue=null;legacyRoutes.FindProperty("supportSettings").objectReferenceValue=null;legacyRoutes.ApplyModifiedPropertiesWithoutUndo();
        var settings=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());
        settings.FindProperty("metaSettings").objectReferenceValue=null;settings.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    public static void RunVisualReview() { SessionState.SetBool(Key+".VisualOnly",true);Run(); }
    static object Get(string field)=>typeof(InvasionPrototype).GetField(field,Flags).GetValue(game);
    static void Set(string field,object value)=>typeof(InvasionPrototype).GetField(field,Flags).SetValue(game,value);
    static object Call(string method,params object[] args)=>typeof(InvasionPrototype).GetMethod(method,Flags).Invoke(game,args);
    static object Item(object item,string field)=>item.GetType().GetField(field).GetValue(item);
    static void Put(object item,string field,object value)=>item.GetType().GetField(field).SetValue(item,value);
    static string State=>Get("phase").ToString();
    static void Check(bool condition,string message) { if(!condition)throw new Exception(message);Debug.Log("P2_CHECK_OK: "+message); }
    static void KillAll()
    {
        var copy=new List<object>();foreach(var foe in (IList)Get("foes"))copy.Add(foe);
        foreach(var foe in copy)Call("HitEnemy",foe,1000f,Vector3.forward);
    }
    static void Isolate(string kind)
    {
        var foes=(IList)Get("foes");var remove=new List<object>();
        foreach(var foe in foes)if(Item(foe,"kind").ToString()!=kind)remove.Add(foe);
        foreach(var foe in remove) { Object.Destroy(((Transform)Item(foe,"body")).gameObject);Object.Destroy(((Transform)Item(foe,"warning")).gameObject);foes.Remove(foe); }
        foreach(var foe in foes) { Put(foe,"spawn",0f);Put(foe,"attack",0f); }
    }
    static void Capture(string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var target=new RenderTexture(540,960,24);var previous=RenderTexture.active;camera.targetTexture=target;
        var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");
        if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) { vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate(); }
        var legacy=(MobileCombatHud)Get("hud");
        legacy.Refresh((float)Get("remaining"),(float)Get("health"),(int)Get("stage"),((IList)Get("foes")).Count,(int)Get("wallet"),(int)Get("pendingGold"),(int)Get("damageCubes"),(float)Get("damageBonus"),false,((CombatFeedback)Get("feedback")).Muted,State,(string)Get("reason"));
        Call("RefreshContentHud");
        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(540,960,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();
        Directory.CreateDirectory("/tmp/cube-p2-previews");File.WriteAllBytes("/tmp/cube-p2-previews/"+name+".png",texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(deadline==0)deadline=now+180;if(now>deadline)throw new Exception("P2 smoke timeout at "+step);
            if(now<next)return;if(!game)game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(step==0&&SessionState.GetBool(Key+".VisualOnly",false))
            {
                catalog=(P2ContentCatalog)Get("content");player=(Transform)Get("player");world=(Transform)Get("world");camera=(Camera)Get("view");
                Call("StartMission",6);Set("invulnerability",100f);Set("shot",100f);next=now+.7;step=4;return;
            }
            switch(step)
            {
                case 0:
                    catalog=(P2ContentCatalog)Get("content");player=(Transform)Get("player");world=(Transform)Get("world");camera=(Camera)Get("view");
                    Check(catalog&&State=="Loadout","Authored P2 scene boots into loadout");
                    Check(catalog.weapons.Length==4 && catalog.missions.Length==7,"Four weapons, five combat missions and two special missions exist");
                    var kinds=new HashSet<ContentEnemy>();
                    foreach(var level in catalog.missions)foreach(var cube in level.cubes)
                    {
                        float hp=0;foreach(var enemy in cube.enemies) { kinds.Add(enemy);hp+=enemy==ContentEnemy.Grunt?18:enemy==ContentEnemy.Sprinter?12:enemy==ContentEnemy.Gunner?21:enemy==ContentEnemy.Pusher?24:enemy==ContentEnemy.Sniper?14:enemy==ContentEnemy.Bruiser?45:96; }
                        Check(hp/(12*.78f)+3.5f<20,"Base pistol HP budget fits 20s: "+level.title);
                        if(level.mode==MissionMode.GoldRush)Check(cube.goldQuota<=80,"Gold quota has enough reachable pickups");
                    }
                    Check(kinds.Count==7,"Six enemy archetypes plus boss are present in playable encounters");
                    var art=(ToyArtLibrary)Get("artLibrary");
                    foreach(string key in new[]{"Pusher","Sniper","Bruiser","Crusher","Twin blasters","Scatter shot","Rapid smg"})
                    { Check(art.TryGet(key,out var model)&&model.mesh.vertexCount>100,"Authored mesh exists: "+key);if(model.rig.Length>0)Check(model.rig.Length>=5,"Articulated rig exists: "+key); }
                    var bank=AssetDatabase.LoadAssetAtPath<P2AudioBank>("Assets/CubeInvasion/Art/P2/Audio/P2AudioBank.asset");
                    foreach(CombatFeedback.Cue cue in Enum.GetValues(typeof(CombatFeedback.Cue)))Check(bank.Find(cue,0)&&bank.Find(cue,1),"Two imported SFX variants: "+cue);
                    Check(bank.combatLoop&&bank.bossLoop&&bank.goldLoop,"Three music loops are assigned");
                    Capture("Loadout");step=1;break;
                case 1:
                    for(int i=0;i<4;i++)
                    {
                        Set("phase",Enum.Parse(Get("phase").GetType(),"Loadout"));Call("ChangeWeapon",i);Call("StartMission",0);Set("paused",true);
                        int before=((IList)Get("pulses")).Count;Call("FirePulse");var shots=(IList)Get("pulses");
                        Check(shots.Count-before==catalog.weapons[i].pellets,"Weapon emits its own pellet count: "+catalog.weapons[i].title);
                        Check(Mathf.Approximately((float)Item(shots[0],"damage"),catalog.weapons[i].damage),"Weapon damage is data driven: "+i);
                        Check(Mathf.Approximately((float)Item(shots[0],"life"),catalog.weapons[i].projectileLife),"Weapon range/lifetime is data driven: "+i);
                        Check((Transform)Get("player")==player&&(Transform)Get("world")==world&&(Camera)Get("view")==camera,"Mission restart preserves authored player/camera/world");
                    }
                    Call("StartMission",2);Set("paused",true);Set("invulnerability",0f);Isolate("Pusher");
                    var pusher=((IList)Get("foes"))[0];var pusherBody=(Transform)Item(pusher,"body");
                    player.position=pusherBody.position+Vector3.forward*2;Call("TickEnemies",.01f);
                    Check((float)Item(pusher,"windup")>.8f,"Pusher displays its cone before attacking");
                    player.position=pusherBody.position+Vector3.right*2;Call("TickEnemies",.9f);
                    Check((float)Get("health")==100,"Sidestepping locked pusher cone avoids damage");
                    player.position=pusherBody.position+Vector3.forward*2;Put(pusher,"attack",0f);Call("TickEnemies",.01f);Call("TickEnemies",.9f);
                    Check(Mathf.Approximately((float)Get("health"),93.5f)&&((Vector3)Get("knockback")).magnitude>8,"Pusher applies warned directional knockback");
                    Capture("Pusher");
                    Call("StartMission",3);Set("paused",true);Set("invulnerability",0f);Isolate("Sniper");
                    var sniper=((IList)Get("foes"))[0];Call("TickEnemies",.01f);var aim=(Vector3)Item(sniper,"aim");
                    Check((float)Item(sniper,"windup")>1,"Sniper has a full 1.1s warning");
                    player.position+=Vector3.right*2;Call("TickEnemies",1.11f);var enemyShots=(IList)Get("enemyShots");
                    Check(enemyShots.Count==1 && (Vector3)Item(enemyShots[0],"direction")==aim,"Sniper shot preserves the telegraphed aim");
                    Check((float)Item(enemyShots[0],"damage")==9,"Sniper has distinct 9-damage projectile");
                    // Keep this damage test in the open lane; the revised cover blocks attacks through it.
                    KillAll();var bruiser=Call("SpawnContentEnemy",ContentEnemy.Bruiser,new Vector3(2,.6f,0));
                    player.position=new Vector3(2,.65f,1);Put(bruiser,"spawn",0f);Put(bruiser,"attack",0f);Call("TickEnemies",.01f);Call("TickEnemies",.35f);
                    Check((float)Get("health")==100,"Bruiser windup cannot deal early damage");Call("TickEnemies",.5f);
                    Check(Mathf.Approximately((float)Get("health"),83.1f),"Bruiser slam applies the configured enemy damage multiplier");
                    Call("StartMission",5);Set("paused",true);KillAll();Set("invulnerability",100f);Set("paused",false);Call("Update");
                    Check(State=="Combat"&&(int)Get("quotaGold")<catalog.missions[5].cubes[0].goldQuota,"Gold Rush does not clear when enemies die before quota");
                    Set("paused",true);Capture("GoldRush");Call("CollectPickups",0f,true);Set("remaining",.001f);Set("paused",false);Call("Update");
                    Check(State=="Choice","Quota completion wins over timeout in the same frame");
                    Call("StartMission",6);Set("paused",true);Capture("Crusher");
                    float total=(float)Get("bossHealth");var boss=((IList)Get("foes"))[0];Call("HitEnemy",boss,12f,Vector3.forward);
                    Check((float)Get("bossHealth")==total-12,"Boss shared health tracks damage without resetting per phase");
                    Isolate("Crusher");boss=((IList)Get("foes"))[0];Put(boss,"attack",0f);Put(boss,"spawn",0f);Set("invulnerability",100f);
                    player.position=((Transform)Item(boss,"body")).position+Vector3.back*2;
                    Call("TickEnemies",.01f);Call("TickEnemies",1.3f);
                    Check(((IList)Get("enemyShots")).Count==0,"Crusher phase 1 uses a warned close-range slam");
                    Set("stage",2);Put(boss,"attack",0f);Call("TickEnemies",.01f);Call("TickEnemies",1.3f);
                    Check(((IList)Get("enemyShots")).Count==7,"Crusher phase 2 uses seven-shot fan");Call("ClearEnemyShots");
                    Set("stage",3);Put(boss,"pattern",0);Put(boss,"attack",0f);Call("TickEnemies",.01f);Call("TickEnemies",1.3f);
                    Check(((IList)Get("enemyShots")).Count==12,"Crusher core phase starts with twelve radial projectiles");Call("ClearEnemyShots");
                    Put(boss,"attack",0f);Call("TickEnemies",.01f);Call("TickEnemies",1.3f);
                    Check(((IList)Get("enemyShots")).Count==9,"Crusher core phase alternates radial and aimed attacks");
                    Set("weaponIndex",0);mission=0;pass=0;Call("StartMission",mission);Set("invulnerability",100f);step=2;next=now+.1;break;
                case 2:
                    if(State=="Transfer")return;
                    if(State=="Combat")
                    {
                        Check((Transform)Get("player")==player&&(Camera)Get("view")==camera,"Continuous mission traversal retains player and camera");
                        Check((float)Get("remaining")>18,"Every arrival starts a fresh 20s clock");
                        KillAll();if(catalog.missions[mission].mode==MissionMode.GoldRush)Call("CollectPickups",0f,true);
                        Set("invulnerability",100f);Call("Update");
                    }
                    if(State=="Choice")
                    {
                        if(!(bool)Get("routesReady"))return;
                        float previousBoss=(float)Get("bossHealth");Call("Select",pass==0);
                        Check(State=="Transfer","Both door directions can advance campaign encounter");
                        Check((float)Get("bossHealth")==previousBoss,"Gate commit never resets boss health");
                        next=now+1.05;return;
                    }
                    if(State!="Won")return;
                    Check((int)Get("stage")==catalog.missions[mission].cubes.Length,"Correct final cube wins mission "+mission+" / route "+pass);
                    Check(!((Transform)Get("platform")).Find("Right upgrade gate")&&!((Transform)Get("platform")).Find("Forward upgrade gate"),"Final encounter offers no next route");
                    if(catalog.missions[mission].mode==MissionMode.Boss)Check((float)Get("bossHealth")==0,"Crusher total health reaches zero only after phase 3");
                    mission++;
                    if(mission==catalog.missions.Length) { mission=0;pass++; }
                    if(pass==2) { Call("StartMission",0);Set("remaining",.001f);Set("shot",100f);floor=((Transform)Get("platform")).position;Call("Update");next=now+1.4;step=3;break; }
                    Call("StartMission",mission);Set("invulnerability",100f);next=now+.1;break;
                case 3:
                    Check(State=="Failed"&&((Transform)Get("platform")).position.y<floor.y-8,"P2 timeout still collapses only the active cube");
                    var selected=Get("weaponIndex");Call("Retry");Check(State=="Combat"&&Get("weaponIndex").Equals(selected),"P2 retry preserves selected loadout");
                    Set("paused",true);KillAll();Set("paused",false);Call("Update");Call("OpenMissionMenu");
                    Check(State=="Loadout","Result/menu flow returns to loadout");
                    Check(Object.FindObjectsByType<Camera>().Length==1,"Campaign changes do not duplicate authored camera");
                    Call("StartMission",6);Set("invulnerability",100f);Set("shot",100f);next=now+.7;step=4;break;
                case 4:
                    Set("paused",true);Capture("Crusher");Call("StartMission",2);Set("invulnerability",100f);Set("shot",100f);next=now+.7;step=5;break;
                case 5:
                    Set("paused",true);var visualPusher=((IList)Get("foes"))[0];Put(visualPusher,"spawn",0f);Put(visualPusher,"attack",0f);
                    player.position=((Transform)Item(visualPusher,"body")).position+Vector3.back*2;Call("TickEnemies",.01f);Capture("Pusher");
                    Call("StartMission",5);Set("invulnerability",100f);Set("shot",100f);next=now+.7;step=6;break;
                case 6:
                    Set("paused",true);Capture("GoldRush");
                    Set("phase",Enum.Parse(Get("phase").GetType(),"Won"));Call("OpenMissionMenu");Capture("Loadout");
                    Debug.Log(SessionState.GetBool(Key+".VisualOnly",false)?"CUBE_P2_VISUAL_OK":"CUBE_P2_SMOKE_OK: both routes through all seven missions, weapons, telegraphs, quota, boss, retry.");Finish(0);break;
            }
        }
        catch(Exception exception) { Debug.LogException(exception);Finish(1); }
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);SessionState.SetBool(Key+".VisualOnly",false);
        if(SessionState.GetBool(Key+".WalletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+".Wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");
        PlayerPrefs.Save();EditorApplication.isPlaying=false;EditorApplication.Exit(code);
    }
}
