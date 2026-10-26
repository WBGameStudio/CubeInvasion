using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class FullCampaignCheck
{
    const string Key="CI.FullCampaignCheck";static InvasionPrototype game;static int mission,step,checks;static double deadline;static Transform player;static Camera camera;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static FullCampaignCheck() {EditorApplication.update+=Tick;}
    public static void RunUnits() {SessionState.SetBool(Key+".units",true);Run();}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        string path="/tmp/cube-full-campaign-profile/profile.json";Directory.CreateDirectory(Path.GetDirectoryName(path));foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
        var settings=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");var legacy=new MetaProfileStore(path,settings,7,50000);legacy.Data.highestMission=6;for(int i=0;i<7;i++)legacy.Data.completed[i]=true;if(!legacy.Select(6,0))throw new Exception("Could not prepare completed legacy profile");
        Environment.SetEnvironmentVariable("CUBE_INVASION_META_TEST_PATH",path);EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string name)=>typeof(InvasionPrototype).GetField(name,Flags).GetValue(game);
    static void Set(string name,object value)=>typeof(InvasionPrototype).GetField(name,Flags).SetValue(game,value);
    static object Prop(string name)=>typeof(InvasionPrototype).GetProperty(name,Flags).GetValue(game);
    static object Call(string name,params object[] args)=>typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static object Item(object value,string name)=>value.GetType().GetField(name).GetValue(value);
    static void Put(object value,string name,object field)=>value.GetType().GetField(name).SetValue(value,field);
    static void Assert(bool condition,string message) {if(!condition)throw new Exception(message);checks++;Debug.Log("CAMPAIGN_CHECK_OK: "+message);}
    static string State=>Get("phase").ToString();
    static IList Foes=>(IList)Get("foes");
    static void RemoveAll() {foreach(var foe in new ArrayList(Foes)) {var body=(Transform)Item(foe,"body");var warning=(Transform)Item(foe,"warning");if(body)Object.Destroy(body.gameObject);if(warning)Object.Destroy(warning.gameObject);}Foes.Clear();if(Get("cover") is Transform cover)Object.Destroy(cover.gameObject);Set("cover",null);foreach(var barrel in new ArrayList((IList)Get("barrels"))) {Object.Destroy(((Transform)Item(barrel,"body")).gameObject);Object.Destroy(((Transform)Item(barrel,"warning")).gameObject);}((IList)Get("barrels")).Clear();Call("ResetExpansionCombat");Call("ClearSupportEffects");}
    static void Fresh(int index=26) {Call("StartMission",index);Call("SetPaused",true);Set("invulnerability",1000f);Set("shot",1000f);}
    static void UnitChecks()
    {
        var catalog=(P2ContentCatalog)Get("content");var store=(MetaProfileStore)Get("profile");Assert(catalog.missions.Length==60&&catalog.weapons.Length==12,"Campaign has 60 levels and 12 weapons");Assert(store.CanWrite&&store.Data.gold==50000&&store.Data.weapons.Length==12&&store.Data.completed.Length==60,"Legacy 4/7 profile expands without losing gold");Assert(store.Data.highestMission==7&&store.Data.completed[6],"Completed legacy campaign unlocks level eight without replay");
        var menu=(MenuFrontEnd)Get("frontEnd");var type=typeof(MenuFrontEnd);Assert(((Array)type.GetField("weaponCards",Flags).GetValue(menu)).Length==12,"All twelve weapon cards are selectable");Assert(((Array)type.GetField("levelNodes",Flags).GetValue(menu)).Length==60,"Road contains all sixty levels");Assert(((Array)type.GetField("weaponUpgrades",Flags).GetValue(menu)).Length==5,"Special and cooldown upgrades have controls");
        for(int i=1;i<12;i++)Assert(store.Unlock(i),"Unlock weapon "+i);
        var cards=(UnityEngine.UI.Button[])type.GetField("weaponCards",Flags).GetValue(menu);cards[0].onClick.Invoke();Assert((int)Get("weaponIndex")==0,"First weapon card equips");cards[11].onClick.Invoke();Assert((int)Get("weaponIndex")==11,"Twelfth weapon card equips");Set("inspectedWeapon",4);Call("RefreshContentHud");var upgrades=(UnityEngine.UI.Button[])type.GetField("weaponUpgrades",Flags).GetValue(menu);upgrades[3].onClick.Invoke();upgrades[4].onClick.Invoke();Assert(store.Data.weapons[4].special==1&&store.Data.weapons[4].cooldown==1,"Special and cooldown buttons purchase persistently");menu.ShowPage(MenuPage.Arsenal,false);Set("inspectedWeapon",4);Call("RefreshContentHud");
        var preview=menu.GetComponentInChildren<WeaponModelPreview>();preview.RenderNow();var renderTarget=(RenderTexture)typeof(WeaponModelPreview).GetField("target",Flags).GetValue(preview);var oldTarget=RenderTexture.active;RenderTexture.active=renderTarget;var pixels=new Texture2D(256,256,TextureFormat.RGBA32,false);pixels.ReadPixels(new Rect(0,0,256,256),0,0);pixels.Apply();int visible=0;foreach(var pixel in pixels.GetPixels32())if(pixel.a>64&&pixel.r+pixel.g+pixel.b>60)visible++;RenderTexture.active=oldTarget;Object.DestroyImmediate(pixels);Assert(visible>32,"Interactive market model actually renders");Assert(Object.FindObjectsByType<Camera>().Length==1,"Menu preview does not duplicate gameplay camera");Capture("Market");
        // Home strip with 60 levels: only the active biome's ten tiles are shown and laid out.
        Set("missionIndex",23);menu.ShowPage(MenuPage.Home,false);Call("RefreshContentHud");Capture("Home60");
        var shownHome=0;foreach(UnityEngine.UI.Button node in (UnityEngine.UI.Button[])type.GetField("homeNodes",Flags).GetValue(menu))if(node.gameObject.activeSelf)shownHome++;Assert(shownHome==10,"Home strip shows one biome of ten levels");
        var rules=(P3MetaSettings)Get("metaSettings");var reload=new MetaProfileStore(store.PathName,rules,60,0);Assert(reload.CanWrite&&reload.Data.weapons[4].special==1&&reload.Data.weapons[4].cooldown==1,"Expanded save reloads special upgrades");Set("metaSettings",null);menu.gameObject.SetActive(false);
        Fresh();RemoveAll();var summoner=Call("SpawnContentEnemy",ContentEnemy.Summoner,new Vector3(0,.6f,2));Call("TickAdvancedEnemy",summoner,6.1f,Vector3.forward,4f);Assert((int)Item(((IDictionary)Get("advancedBrains"))[summoner],"summoned")==0,"Summon waits for telegraph");Call("TickAdvancedEnemy",summoner,.81f,Vector3.forward,4f);Call("FlushSummons");Assert(Foes.Count==3,"Summoner creates at most two required minions");Call("TickAdvancedEnemy",summoner,20f,Vector3.forward,4f);Call("FlushSummons");Assert(Foes.Count==3,"Summon lifetime cap prevents endless mandatory waves");
        RemoveAll();var phaser=Call("SpawnContentEnemy",ContentEnemy.Phaser,new Vector3(0,.6f,2));var body=(Transform)Item(phaser,"body");var before=body.position;Call("TickAdvancedEnemy",phaser,2.6f,Vector3.forward,4f);Assert(body.position==before,"Phaser warns before teleporting");Call("TickAdvancedEnemy",phaser,.71f,Vector3.forward,4f);Assert(body.position!=before,"Phaser relocates after warning");Assert((float)Call("AdvancedDamage",phaser,10f)>10,"Phaser exposes a brief damage window");
        RemoveAll();var elite=Call("SpawnContentEnemy",ContentEnemy.Elite,new Vector3(0,.6f,1));Put(elite,"attackPower",1.15f);player.position=new Vector3(0,.65f,2);Call("TickAdvancedEnemy",elite,2.6f,Vector3.forward,1f);Assert((float)Item(((IDictionary)Get("advancedBrains"))[elite],"windup")>0,"Elite prepares its combined attack");int shotCount=((IList)Get("enemyShots")).Count;Call("TickAdvancedEnemy",elite,1.1f,Vector3.forward,1f);Assert(((IList)Get("enemyShots")).Count==shotCount+3,"Elite executes the ranged half of its combination");Assert(Mathf.Abs((float)Item(((IList)Get("enemyShots"))[shotCount],"damage")-6*1.15f)<.001f,"Route elite multiplier reaches the advanced ranged attack");Set("invulnerability",0f);Set("health",100f);Call("TickAdvancedEnemy",elite,2.1f,Vector3.forward,1f);Call("TickAdvancedEnemy",elite,1.1f,Vector3.forward,1f);Assert((int)Item(((IDictionary)Get("advancedBrains"))[elite],"sequence")==2&&((IList)Get("enemyShots")).Count==shotCount+3,"Elite follows the burst with a push attack");Assert(Mathf.Abs((float)Get("health")-(100-7*(float)Prop("EnemyDamageMultiplier")*1.15f))<.001f,"Route elite multiplier reaches actual push damage once");
        for(int weapon=4;weapon<12;weapon++)
        {
            Set("weaponIndex",weapon);Fresh(7);RemoveAll();player.position=new Vector3(0,.65f,-1);var foe=Call("SpawnContentEnemy",ContentEnemy.Bruiser,new Vector3(0,.6f,.5f));Call("FireContentWeapon");for(int i=0;i<30;i++) {Call("TickPulses",.025f);Call("TickAdvancedWeapons",.025f);}Assert((float)Item(foe,"hp")<45,"Actual advanced weapon damages target "+weapon);
        }
        for(int weapon=0;weapon<12;weapon++)
        {
            Set("weaponIndex",weapon);Fresh(5);RemoveAll();player.position=new Vector3(0,.65f,-1);Call("CreateGoldCrate",new Vector3(0,.45f,.5f),false);var crate=((IList)Get("goldCrates"))[0];Call("FireContentWeapon");for(int i=0;i<30;i++) {Call("TickPulses",.025f);Call("TickAdvancedWeapons",.025f);}Assert((float)Item(crate,"hp")<6,"Actual weapon damages counted gold crate "+weapon);
            Fresh(19);RemoveAll();player.position=new Vector3(0,.65f,-1);Call("CreateBarrel",new Vector3(0,.5f,.5f));var barrel=((IList)Get("barrels"))[0];Call("FireContentWeapon");for(int i=0;i<30;i++) {Call("TickPulses",.025f);Call("TickAdvancedWeapons",.025f);}Assert((float)Item(barrel,"hp")<12,"Actual weapon damages mandatory Furnace barrel "+weapon);
        }
        Set("weaponIndex",11);Fresh(7);RemoveAll();player.position=new Vector3(0,.65f,-1);var wideA=Call("SpawnContentEnemy",ContentEnemy.Bruiser,new Vector3(0,.6f,1));var wideB=Call("SpawnContentEnemy",ContentEnemy.Bruiser,new Vector3(.8f,.6f,1));Call("FireContentWeapon");for(int i=0;i<30;i++)Call("TickPulses",.025f);Assert((float)Item(wideA,"hp")<45&&(float)Item(wideB,"hp")<45,"Nova wave has a wider multi-target collision");
        RemoveAll();player.position=new Vector3(1.8f,.65f,0);var pushed=Call("SpawnContentEnemy",ContentEnemy.Bruiser,new Vector3(4,.6f,0));Call("FireContentWeapon");for(int i=0;i<30;i++)Call("TickPulses",.025f);Call("TickEnemies",.1f);Assert(!Foes.Contains(pushed),"Actual Nova knockback can eliminate an enemy over the edge");
        RemoveAll();var pooled=Call("SpawnContentEnemy",ContentEnemy.Grunt,new Vector3(0,.6f,1));var pooledBody=Item(pooled,"body");Call("HitEnemyArea",pooled,10000f,Vector3.forward);var reused=Call("SpawnContentEnemy",ContentEnemy.Grunt,new Vector3(1,.6f,1));Assert(ReferenceEquals(pooledBody,Item(reused,"body")),"Enemy body and rig are reused from pool");
        RemoveAll();var fallen=Call("SpawnContentEnemy",ContentEnemy.Grunt,new Vector3(5,.6f,0));Call("TickEnemies",.1f);Assert(!Foes.Contains(fallen),"Enemy falling off arena is eliminated");int bank=(int)Get("pendingGold");Call("CollectPickups",0f,false);Assert((int)Get("pendingGold")>bank,"Fallen enemy loot is pulled to player");
        Set("weaponIndex",0);Fresh(22);RemoveAll();Call("SpawnSpecialPickup",player.position,true);Call("TickExpandedArena",.01f);Call("SpawnContentEnemy",ContentEnemy.Grunt,player.position+Vector3.forward*1.5f);Call("FireContentWeapon");Assert(Mathf.Abs((float)Item(((IList)Get("pulses"))[0],"damage")-7.8f)<.001f,"Damage Amp changes actual projectile damage by thirty percent");Call("TickExpandedArena",4.1f);Assert((float)Prop("temporaryAmp")==0,"Damage Amp expires after four seconds");Call("SpawnSpecialPickup",player.position,false);Call("TickExpandedArena",.01f);int gold=(int)Get("pendingGold");Call("SpawnPickup",player.position+Vector3.right*2.5f,false,5);Call("CollectPickups",0f,false);Assert((int)Get("pendingGold")==gold+5,"Magnet Core extends actual pickup reach");Call("TickExpandedArena",3.1f);gold=(int)Get("pendingGold");Call("SpawnPickup",player.position+Vector3.right*2.5f,false,5);Call("CollectPickups",0f,false);Assert((int)Get("pendingGold")==gold,"Magnet reach returns to normal after expiry");
        Fresh(12);Assert(Mathf.Abs((float)Prop("ArenaHalfX")-3)<.001f,"Narrow arena uses real six-unit floor");var gate=(Transform)Get("rightGate");Assert(Mathf.Abs(gate.position.x-((Vector3)Get("centre")).x-2.65f)<.001f,"Narrow first cube gate matches physical edge");var origin=(Vector3)Get("centre");var exit=origin+new Vector3(2.8f,.65f,0);player.position=exit;Set("phase",Enum.Parse(typeof(InvasionPrototype).GetNestedType("Phase",BindingFlags.NonPublic),"Choice"));Set("routesReady",true);Call("CheckGateCrossing",origin+new Vector3(2.5f,.65f,0),exit);Assert(State=="Transfer","Walking through narrow gate commits the route");Fresh(11);Assert(Mathf.Abs(((Transform)Get("rightPreview")).Find("Arena surface").localScale.x-6)<.001f,"Narrow candidate rises with its final geometry");Call("OpenExits");Assert(Mathf.Abs(((Transform)Get("platform")).Find("Right bridge").localScale.x-4.6f)<.001f,"Bridge spans the real destination edge");
        Fresh(22);RemoveAll();Call("SetupExpandedArena");if(((IList)Get("additionalCover")).Count>0) {var block=(Transform)((IList)Get("additionalCover"))[0];var a=block.position-Vector3.right*2;var b=block.position+Vector3.right*2;var args=new object[]{a,b,.09f,0f};Assert((bool)Call("CoverHit",args),"Additional cover blocks lasers");Call("DamageExpandedCover",a,b,100f);Assert(!block||!((IList)Get("additionalCover")).Contains(block),"Breakable cover can be destroyed");}
    }
    static void ResolveCube()
    {
        // Resolve the actual phase sources instead of bypassing protected bosses with arbitrary damage.
        var current=(MissionSpec)Prop("Mission");int stage=(int)Get("stage");
        if(current.mode==MissionMode.GoldRush)
        {var crates=(IList)Get("goldCrates");foreach(var foe in new ArrayList(Foes))Call("HitEnemyArea",foe,10000f,Vector3.forward);foreach(var crate in new ArrayList(crates))Call("HitGoldCrate",crate,10000f);Call("CollectPickups",0f,true);}
        else if((bool)Prop("HasExpandedBoss"))
        {
            var boss=Get("expandedBoss");if(current.boss==BossIdentity.FurnaceKing&&stage==1)foreach(var barrel in new ArrayList((IList)Get("barrels")))Call("Explode",barrel);
            else if(current.boss==BossIdentity.MagmaTitan&&stage==2) {player.position=(Vector3)Get("centre")+new Vector3(0,.65f,-1.8f);for(int i=0;i<22;i++)Call("TickExpandedBossWorld",.1f);}
            else if(current.boss==BossIdentity.GlacierWarden&&stage==2) {Call("TickExpandedBossWorld",3.1f);foreach(var foe in new ArrayList(Foes))if(foe!=boss)Call("HitEnemyArea",foe,10000f,Vector3.forward);}
            else if((bool)Prop("NodePhase")) {foreach(var foe in new ArrayList(Foes))if(foe!=boss)Call("HitEnemyArea",foe,10000f,Vector3.forward);}
            else {Call("TickExpandedBossWorld",1.2f);if(Foes.Contains(boss))Call("HitEnemyArea",boss,10000f,Vector3.forward);}
        }
        else if((bool)Prop("HasCrusherObjectives"))
        {foreach(var foe in new ArrayList(Foes)) {if(Item(foe,"kind").ToString()=="Crusher"&&(int)Get("stage")==1) {Call("HitEnemyAt",foe,10000f,Vector3.forward,0);Call("HitEnemyAt",foe,10000f,Vector3.forward,1);}else Call("HitEnemyArea",foe,10000f,Vector3.forward);}}
        else
        {Call("TickExpandedArena",4.1f);foreach(var foe in new ArrayList(Foes))Call("HitEnemyArea",foe,10000f,Vector3.forward);}
        Assert((bool)Prop("ObjectiveComplete"),"Actual objective resolves L"+(mission+1)+" C"+stage);Call("Clear");
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+900;if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Campaign timeout "+mission+"/"+step);
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(!player) {player=(Transform)Get("player");camera=(Camera)Get("view");UnitChecks();if(SessionState.GetBool(Key+".units",false)) {step=100;mission=19;return;}}
            if(step==100) {Fresh(mission);step=101;return;}
            if(step==101) {RemoveAll();Set("stage",2);Call("BeginCombat",(object)null);Call("SetPaused",true);step=102;return;}
            if(step==102) {Capture("Boss"+mission);mission+=10;if(mission>=60) {Finish(0);return;}step=100;return;}
            if(step==0) {Set("weaponIndex",mission%12);Fresh(mission);ResolveCube();step=1;return;}
            Call("SetPaused",false);Call("TickAnimations",.2f);
            if(State=="Won") {Assert((Transform)Get("player")==player&&(Camera)Get("view")==camera,"Continuous player/camera L"+(mission+1));mission++;if(mission==60) {Finish(0);return;}step=0;return;}
            if(step==1) {if(State!="Choice")throw new Exception("Clear did not open choice: "+State);if(!(bool)Get("routesReady"))return;Call("Select",mission%2==0);step=2;return;}
            if(State=="Transfer")return;Assert(State=="Combat","Gate arrives in combat");Call("SetPaused",true);Set("invulnerability",1000f);Set("shot",1000f);if((bool)Prop("HasExpandedBoss")&&(int)Get("stage")==2)Capture("Boss"+mission);ResolveCube();step=1;
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture(string name)
    {
        bool paused=(bool)Get("paused");Call("SetPaused",false);Call("TickAnimations",.5f);((CombatFeedback)Get("feedback")).Tick(.5f);Call("LateUpdate");var target=new RenderTexture(720,1280,24);var old=RenderTexture.active;camera.targetTexture=target;
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;var texture=new Texture2D(720,1280,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,720,1280),0,0);texture.Apply();Directory.CreateDirectory("/tmp/cube-full-previews");File.WriteAllBytes("/tmp/cube-full-previews/"+name+".png",texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);Call("SetPaused",paused);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();if(code==0)Debug.Log((SessionState.GetBool(Key+".units",false)?"CUBE_FULL_UNITS_OK: ":"CUBE_FULL_CAMPAIGN_OK: ")+(SessionState.GetBool(Key+".units",false)?" advanced weapon/AI/pooling/UI/profile checks, ":" 60 full missions, twelve weapons, six boss identities, phase sources, profile expansion, UI collections, ")+checks+" assertions");SessionState.SetBool(Key+".units",false);EditorApplication.Exit(code);}
}
