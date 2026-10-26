using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class CombatPressureSmokeCheck
{
    const string Key="CI.PressureSmoke";
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static InvasionPrototype game;
    static double deadline;
    static bool capturePending;
    static CombatPressureSmokeCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string field)=>typeof(InvasionPrototype).GetField(field,Flags).GetValue(game);
    static void Set(string field,object value)=>typeof(InvasionPrototype).GetField(field,Flags).SetValue(game,value);
    static object Call(string name,params object[] args)=>typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static object Item(object obj,string field)=>obj.GetType().GetField(field).GetValue(obj);
    static void Put(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
    static void Check(bool value,string name) {if(!value)throw new Exception(name);Debug.Log("PRESSURE_CHECK_OK: "+name);}
    static bool Wall(Vector3 a,Vector3 b,float radius=.09f)=>(bool)Call("CoverHit",a,b,radius,0f);
    static object Pulse(Vector3 start,Vector3 direction,float life=1)
    {
        var body=(Transform)Call("RentProjectile",Get("pulsePool"),"Pressure test pulse",start,Color.cyan,.18f);
        var pulse=Activator.CreateInstance(typeof(InvasionPrototype).GetNestedType("Pulse",BindingFlags.NonPublic),true);
        Put(pulse,"body",body);Put(pulse,"direction",direction);Put(pulse,"damage",6f);Put(pulse,"life",life);((IList)Get("pulses")).Add(pulse);return pulse;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        if(deadline==0)deadline=EditorApplication.timeSinceStartup+40;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Pressure smoke timeout");
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(capturePending)
            {
                Set("paused",false);Call("TickAnimations",.5f);Call("LateUpdate");
                var camera=(Camera)Get("view");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();
                if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
                Capture(camera);Debug.Log("CUBE_PRESSURE_SMOKE_OK: swept collisions, target visibility, scaled cover, hit ordering, lifetime, held-thumb rhythm, pause, damage grace, cover navigation and scarce healing.");Finish(0);return;
            }
            Call("StartMission",1);Set("paused",true);Set("weaponIndex",0);Call("EquipWeapon");
            var cover=(Transform)Get("cover");var player=(Transform)Get("player");var foes=(IList)Get("foes");var foe=foes[0];var exposed=foes[1];
            foreach(var f in foes) {((Transform)Item(f,"body")).position=new Vector3(100,.6f,100);Put(f,"spawn",0f);Put(f,"attack",100f);}
            var origin=cover.position;origin.y=player.position.y;
            player.position=origin+Vector3.back*2;((Transform)Item(foe,"body")).position=origin+Vector3.forward*2;Put(foe,"hp",100f);
            ((Transform)Item(exposed,"body")).position=origin+new Vector3(3.5f,0,-1);
            Check((bool)Call("AimAtNearestEnemy")&&Get("currentTarget")==exposed,"Visible farther target wins over nearer blocked target");
            Pulse(player.position,Vector3.forward);Call("TickPulses",.4f);
            Check((float)Item(foe,"hp")==100&&((IList)Get("pulses")).Count==0,"Large-frame player projectile stops at barrier before enemy");
            Call("CreateBarrel",origin+Vector3.forward*1.4f);var barrels=(IList)Get("barrels");var behindBarrel=barrels[barrels.Count-1];
            Pulse(player.position,Vector3.forward);Call("TickPulses",.4f);
            Check((float)Item(behindBarrel,"hp")==12,"Barrier prevents damage to an explosive barrel behind it");
            ((Transform)Item(foe,"body")).position=origin+Vector3.back*1.1f;
            Pulse(player.position,Vector3.forward);Call("TickPulses",.4f);
            Check((float)Item(foe,"hp")==94,"Enemy in front of barrier receives hit before the wall");
            ((Transform)Item(foe,"body")).position=new Vector3(100,.6f,100);
            Call("CreateBarrel",origin+Vector3.back*1.05f);var frontBarrel=barrels[barrels.Count-1];
            Pulse(player.position,Vector3.forward);Call("TickPulses",.4f);
            Check((float)Item(frontBarrel,"hp")==6,"Barrel in front of barrier receives the first hit");
            foreach(var barrel in barrels) {Object.DestroyImmediate(((Transform)Item(barrel,"body")).gameObject);Object.DestroyImmediate(((Transform)Item(barrel,"warning")).gameObject);}barrels.Clear();
            Set("health",50f);var healAt=new Vector3(-3,.5f,-2.7f);Call("SpawnPickup",healAt,true,20);Call("CollectPickups",0f,true);
            Check((float)Get("health")==50,"Cube-clear coin magnet does not grant a remote health pickup for free");
            player.position=healAt;Set("phase",Enum.Parse(Get("phase").GetType(),"Choice"));Set("paused",false);Call("Update");
            Check((float)Get("health")==70,"An uncollected health kit remains reachable on the cleared cube");
            Set("phase",Enum.Parse(Get("phase").GetType(),"Combat"));Set("paused",true);player.position=origin+Vector3.back*2;
            Set("health",100f);Set("invulnerability",0f);Call("FireEnemyShot",origin+Vector3.forward*2,Vector3.back,6f,7f);Call("TickEnemyShots",.6f);
            Check((float)Get("health")==100&&((IList)Get("enemyShots")).Count==0,"Enemy projectile uses the same swept barrier collision");
            Call("FireEnemyShot",player.position,Vector3.back,6f,7f);Call("TickEnemyShots",.01f);
            Check(Mathf.Approximately((float)Get("health"),92.2f),"Enemy damage is raised by thirty percent");
            Call("FireEnemyShot",player.position,Vector3.back,6f,7f);Call("TickEnemyShots",.01f);
            Check(Mathf.Approximately((float)Get("health"),92.2f),"Damage grace still prevents unavoidable stacked hits");
            Set("invulnerability",0f);Call("FireEnemyShot",player.position-Vector3.right*2,Vector3.right,6f,7f);
            var expired=((IList)Get("enemyShots"))[0];Put(expired,"life",.02f);Call("TickEnemyShots",1f);
            Check(Mathf.Approximately((float)Get("health"),92.2f),"Expired enemy shot also respects its lifetime on a slow frame");
            var scale=cover.localScale;cover.localScale=new Vector3(1.7f,.9f,.5f);cover.rotation=Quaternion.Euler(0,37,0);
            Check(Wall(origin-cover.forward*2,origin+cover.forward*2),"Rotated and nonuniformly scaled cover blocks a swept shot");
            Check(!Wall(origin-cover.forward*2+cover.right*2,origin+cover.forward*2+cover.right*2),"Clear shot beside the cover is not falsely blocked");
            player.position=origin-cover.forward*.9f;
            var safe=(Vector3)Call("SafeMuzzle",origin+cover.forward);
            Check(Vector3.Dot(safe-origin,cover.forward)<0,"Long muzzle is clamped before cover instead of spawning through it");
            cover.rotation=Quaternion.identity;cover.localScale=scale;
            player.position=origin+new Vector3(2.5f,0,-2);
            var target=(Transform)Item(exposed,"body");target.position=player.position+Vector3.forward*2;Put(exposed,"hp",1000f);
            Pulse(player.position,Vector3.forward,.02f);Call("TickPulses",1f);
            Check((float)Item(exposed,"hp")==1000,"Expired projectile cannot travel beyond its range on a slow frame");
            // Drive the actual joystick without ever sending pointer-up between running and focus.
            var hud=(MobileCombatHud)Get("hud");var joystick=hud.GetComponentInChildren<MobileJoystick>();var rect=(RectTransform)joystick.transform;var view=(Camera)Get("view");Canvas.ForceUpdateCanvases();
            var pointer=new PointerEventData(EventSystem.current){pointerId=71};pointer.pointerPressRaycast=new RaycastResult{module=hud.GetComponent<GraphicRaycaster>()};
            pointer.position=RectTransformUtility.WorldToScreenPoint(view,rect.TransformPoint(Vector3.zero));joystick.OnPointerDown(pointer);
            pointer.position=RectTransformUtility.WorldToScreenPoint(view,rect.TransformPoint(new Vector3(rect.rect.width*.5f,0,0)));joystick.OnDrag(pointer);
            Call("ReadInput");Check(((Vector2)Get("stick")).magnitude>.9f,"One held thumb reaches running fire mode");
            Set("shot",.5f);for(int i=0;i<110;i++)Call("TickAutoFire",.02f);int running=((IList)Get("pulses")).Count;Call("ClearPulses");
            pointer.position=RectTransformUtility.WorldToScreenPoint(view,rect.TransformPoint(new Vector3(rect.rect.width*.04f,0,0)));joystick.OnDrag(pointer);
            Call("ReadInput");Check(((Vector2)Get("stick")).magnitude>.01f&&((Vector2)Get("stick")).magnitude<.25f,"Same held thumb can slow-walk into focus without lifting");
            Set("shot",.5f);for(int i=0;i<110;i++)Call("TickAutoFire",.02f);int focused=((IList)Get("pulses")).Count;Call("ClearPulses");
            Check(focused>=4&&running<=2&&running>0,"Focused fire is faster while running retains automatic shots");
            Set("stick",Vector2.right);Set("shot",.4f);Call("TickAutoFire",.21f);float recovery=(float)Get("shot");Set("stick",Vector2.zero);Call("TickAutoFire",.2f);
            Check(Mathf.Approximately(recovery,.3f)&&Mathf.Approximately((float)Get("shot"),.1f),"Changing speed changes current recovery without restarting the cooldown");
            float shot=(float)Get("shot"),clock=(float)Get("remaining");var before=player.position;Call("Update");
            Check((float)Get("shot")==shot&&(float)Get("remaining")==clock&&player.position==before,"Pause freezes fire recovery and combat clocks");joystick.OnPointerUp(pointer);
            hud.SetFireRhythm(0,true,true);Check(hud.SafeRoot.Find("Fire rhythm").GetComponent<Text>().text=="ODAK ATEŞİ","Authored HUD explains focus mode beside the joystick");
            // A ranged enemy must walk around cover instead of remaining behind it indefinitely.
            foreach(var f in foes)Put(f,"attack",100f);
            for(int side=0;side<4;side++)
            {
                Vector3 axis=side%2==0?Vector3.forward:Vector3.right;if(side>=2)axis=-axis;
                player.position=origin+axis*2;var body=(Transform)Item(foe,"body");body.position=origin-axis*2;Put(foe,"dash",0f);
                for(int i=0;i<500;i++)
                {
                    var old=body.position;var toward=player.position-body.position;toward.y=0;if(toward.magnitude<.6f)break;
                    Call("MoveEnemy",foe,toward.normalized*(float)Item(foe,"speed")*.02f);
                    CheckNoClip(old,body.position);
                }
                Check(Vector3.Distance(body.position,player.position)<.7f,"Cover detour reaches target from side "+side);
            }
            player.position=origin+Vector3.forward*(cover.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z*cover.lossyScale.z+.305f);
            var hugging=(Transform)Item(foe,"body");hugging.position=origin+Vector3.back*2;
            for(int i=0;i<500;i++) {var toward=player.position-hugging.position;toward.y=0;if(toward.magnitude<.8f)break;Call("MoveEnemy",foe,toward.normalized*(float)Item(foe,"speed")*.02f);}
            Check(Vector3.Distance(hugging.position,player.position)<.85f,"Hugging a cover edge does not make the enemy destination unreachable");
            var catalog=(P2ContentCatalog)Get("content");int healing=0,cubes=0;
            foreach(var mission in catalog.missions)foreach(var cube in mission.cubes)
            {cubes++;if(cube.healing)healing++;Check(cube.enemies.Length>=3,"Every cube contains a mixed pressure group");}
            Check(healing>0&&healing<cubes/3,"Healing is scarce instead of appearing on every cover cube");
            // Deterministic hands-off probe: actual combat methods, base pistol, no meta upgrades.
            var samples=new System.Text.StringBuilder("mission,seconds,hp,enemies_left,outcome\n");
            for(int mission=0;mission<catalog.missions.Length;mission++)
            {
                Call("StartMission",mission);Set("paused",true);Set("stick",Vector2.zero);Set("weaponIndex",0);Call("EquipWeapon");
                float elapsed=0;const float dt=1f/60;
                while(elapsed<20&&Get("phase").ToString()=="Combat"&&((IList)Get("foes")).Count>0)
                {
                    Set("invulnerability",(float)Get("invulnerability")-dt);Call("TickEnemies",dt);
                    if(Get("phase").ToString()!="Combat")break;
                    Call("TickAutoFire",dt);Call("TickPulses",dt);Call("TickWorldFeel",dt);elapsed+=dt;
                    if(Mathf.Abs(player.position.x)>4.5f||Mathf.Abs(player.position.z)>4.5f)break;
                }
                int alive=((IList)Get("foes")).Count;string outcome=alive==0?"all_enemies_dead":Get("phase").ToString()!="Combat"?"health_fail":elapsed>=20?"timeout":"edge_fall";
                samples.AppendLine($"{mission+1},{elapsed:0.00},{(float)Get("health"):0.0},{alive},{outcome}");
            }
            File.WriteAllText("/tmp/cube-pressure-hands-off.csv",samples.ToString());Debug.Log("PRESSURE_HANDS_OFF_PROBE: "+samples);
            Call("StartMission",1);Set("stick",Vector2.zero);Set("shot",0f);Set("paused",true);player.position=new Vector3(0,.65f,-2.5f);
            capturePending=true;return;
        }
        catch(Exception error) {Debug.LogException(error);Finish(1);}
    }
    static void CheckNoClip(Vector3 a,Vector3 b) {if(Wall(a,b,.29f))throw new Exception("Enemy clipped through cover while detouring");}
    static void Capture(Camera view)
    {
        var rt=new RenderTexture(540,960,24);view.targetTexture=rt;var old=RenderTexture.active;view.GetComponent<ArcadeFollowCamera>().SendMessage("Update");view.GetComponent<Cinemachine.CinemachineBrain>().ManualUpdate();Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=rt;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();File.WriteAllBytes("/tmp/cube-pressure-gameplay.png",image.EncodeToPNG());view.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(rt);Object.DestroyImmediate(image);
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();EditorApplication.Exit(code);
    }
}
