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
public static class LaserVfxReview
{
    const string Key="CI.LaserReview";
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static InvasionPrototype game;static double deadline;
    static LaserVfxReview() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"walletExists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string name)=>typeof(InvasionPrototype).GetField(name,Flags).GetValue(game);
    static void Set(string name,object value)=>typeof(InvasionPrototype).GetField(name,Flags).SetValue(game,value);
    static object Call(string name,params object[] args)=>typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static object Item(object obj,string name)=>obj.GetType().GetField(name).GetValue(obj);
    static void Put(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
    static void Check(bool value,string message) {if(!value)throw new Exception(message);Debug.Log("LASER_CHECK_OK: "+message);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        if(deadline==0)deadline=EditorApplication.timeSinceStartup+60;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Laser review timeout");
            game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            Call("StartMission",1);Set("paused",true);Set("invulnerability",100f);Call("ClearPulses");Call("ClearEnemyShots");
            var foes=(IList)Get("foes");var player=(Transform)Get("player");player.position=new Vector3(0,.65f,-2.5f);
            object gunner=null;foreach(var f in foes)
            {Put(f,"hp",1000f);Put(f,"attack",100f);Put(f,"spawn",0f);((Transform)Item(f,"body")).position=new Vector3(100,.6f,100);if(Item(f,"kind").ToString()=="Gunner"&&gunner==null)gunner=f;}
            var target=(Transform)Item(foes[0],"body");target.position=new Vector3(-2.4f,.6f,.5f);
            var enemy=(Transform)Item(gunner,"body");enemy.position=new Vector3(2.8f,.6f,2.5f);var direction=player.position-enemy.position;direction.y=0;direction.Normalize();enemy.rotation=Quaternion.LookRotation(direction);
            foreach(var f in foes) {var anim=(ToyActorAnimator)Item(f,"animation");if(anim)anim.TickPose(Vector3.zero,.5f,false);}
            var fx=(CombatFeedback)Get("feedback");fx.SetPaused(false);
            Call("FirePulse");var shots=(IList)Get("pulses");var body=(Transform)Item(shots[0],"body");var laser=body.GetComponent<LaserBoltVisual>();
            Check(laser&&!body.GetComponent<MeshFilter>()&&!body.GetComponent<Collider>(),"Projectile is a luminous line, with no sphere mesh or physics collider");
            Check(body.GetComponentsInChildren<LineRenderer>().Length==3&&LaserBoltVisual.Material.shader.name=="CubeInvasion/Laser Glow","Core, colored edge and halo use the packaged laser shader");
            Call("TickPulses",.03f);var line=body.GetComponent<LineRenderer>();Check(line.GetPosition(0).z<-.5f&&line.GetPosition(1)==Vector3.zero,"Tail extends behind the collision tip along flight direction");
            Check(fx.ActiveMuzzleFlashes==1,"Player shot emits one muzzle flash on the animated weapon");
            var pos=body.position;var tail=line.GetPosition(0);Set("paused",true);Call("Update");fx.SetPaused(true);int count=fx.ActiveMuzzleFlashes;fx.Tick(.5f);
            Check(body.position==pos&&line.GetPosition(0)==tail&&fx.ActiveMuzzleFlashes==count,"Pause freezes laser flight and muzzle-flash lifetime");fx.SetPaused(false);fx.Tick(1);Call("ClearPulses");
            var old=body;Call("FirePulse");body=(Transform)Item(((IList)Get("pulses"))[0],"body");Check(body==old&&body.GetComponent<LineRenderer>().GetPosition(0).z>-.1f,"Pooled laser resets old tail length");Call("ClearPulses");fx.ClearMuzzles();
            Call("FireEnemyShot",enemy.position,direction,6f,7f);var enemyBody=(Transform)Item(((IList)Get("enemyShots"))[0],"body");
            Check(enemyBody.GetComponent<LineRenderer>().startColor.r>enemyBody.GetComponent<LineRenderer>().startColor.b&&fx.ActiveMuzzleFlashes==1,"Enemy laser uses warm color and its own muzzle discharge");Call("ClearEnemyShots");fx.ClearMuzzles();
            Set("weaponIndex",1);Call("EquipWeapon");Call("FirePulse");Check(((IList)Get("pulses")).Count==2&&fx.ActiveMuzzleFlashes==2,"Twin blasters fire two laser bolts and two muzzle flashes");Call("ClearPulses");fx.ClearMuzzles();
            Set("weaponIndex",2);Call("EquipWeapon");Call("FirePulse");Check(((IList)Get("pulses")).Count==5&&fx.ActiveMuzzleFlashes==1,"Scatter keeps five projectiles with one wide muzzle flash");Call("ClearPulses");fx.ClearMuzzles();
            Set("weaponIndex",0);Call("EquipWeapon");
            // The genuine game camera and HUD render a short live projectile/flash sequence.
            var view=(Camera)Get("view");Set("paused",false);Call("LateUpdate");Set("paused",true);fx.SetPaused(false);
            var brain=view.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera cam) {cam.PreviousStateIsValid=false;cam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
            var rootBefore=player.position;Directory.CreateDirectory("/tmp/cube-laser-frames");
            for(int frame=0;frame<72;frame++)
            {
                if(frame%8==0)Call("FirePulse");if(frame%15==0) {Put(gunner,"muzzleFrame",-1);Call("FireEnemyShot",enemy.position,direction,6f,7f);}
                Call("TickPulses",1f/30);Call("TickEnemyShots",1f/30);fx.Tick(1f/30);
                var animator=(ToyActorAnimator)Get("playerAnimation");animator.TickPose(Vector3.zero,1f/30,true);
                Save(view,"/tmp/cube-laser-frames/"+frame.ToString("D3")+".png");
            }
            Check(player.position==rootBefore,"VFX never move the gameplay character");
            for(int i=0;i<100;i++)fx.Muzzle(player,Vector3.zero,Vector3.forward,Color.cyan);
            Check(fx.ActiveEffects<=48,"Combined muzzle and impact effects stay within the shared budget");fx.Tick(1);
            Check(fx.ActiveMuzzleFlashes==0&&fx.ActiveEffects==0,"Expired flashes and impact effects return to their pools");
            Debug.Log("CUBE_LASER_VFX_OK: line projectiles, glow, muzzle flashes, twin/scatter, pool reset, pause, shared effect budget and Unity render review.");Finish(0);
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Save(Camera view,string path)
    {
        var rt=new RenderTexture(540,960,24);view.targetTexture=rt;var old=RenderTexture.active;Canvas.ForceUpdateCanvases();view.Render();RenderTexture.active=rt;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        view.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"walletExists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();EditorApplication.Exit(code);}
}
