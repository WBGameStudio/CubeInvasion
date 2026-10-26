using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class BomberCheck
{
    const string Key="CI.BomberCheck";static InvasionPrototype game;static int step;static double deadline;
    static readonly BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
    static BomberCheck() {EditorApplication.update+=Tick;}
    public static void Run()
    {
        SessionState.SetBool(Key+"exists",PlayerPrefs.HasKey("CI.Wallet"));SessionState.SetInt(Key+"wallet",PlayerPrefs.GetInt("CI.Wallet",0));EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();
        var data=new SerializedObject(Object.FindAnyObjectByType<InvasionPrototype>());data.FindProperty("metaSettings").objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static object Get(string n)=>typeof(InvasionPrototype).GetField(n,F).GetValue(game);
    static void Set(string n,object v)=>typeof(InvasionPrototype).GetField(n,F).SetValue(game,v);
    static object Prop(string n)=>typeof(InvasionPrototype).GetProperty(n,F).GetValue(game);
    static object Call(string n,params object[] args)=>typeof(InvasionPrototype).GetMethod(n,F).Invoke(game,args);
    static object Item(object o,string n)=>o.GetType().GetField(n).GetValue(o);
    static void Put(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);
    static void Check(bool b,string m) {if(!b)throw new Exception(m);Debug.Log("BOMBER_CHECK_OK: "+m);}
    static void RemoveAll() {var list=(IList)Get("foes");foreach(var foe in new ArrayList(list)) {Object.Destroy(((Transform)Item(foe,"body")).gameObject);Object.Destroy(((Transform)Item(foe,"warning")).gameObject);}list.Clear();Call("ClearDeathCharges",false);}
    static object Spawn(ContentEnemy kind,Vector3 at)=>Call("SpawnContentEnemy",kind,at);
    static void Hit(object foe,float damage)=>Call("HitEnemy",foe,damage,Vector3.forward);
    static void Fresh() {Call("StartMission",4);Call("SetPaused",true);Set("invulnerability",100f);Set("shot",100f);}
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            if(deadline==0)deadline=EditorApplication.timeSinceStartup+90;if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Bomber timeout");game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get("player")==null)return;
            if(step==0)
            {
                for(int weapon=0;weapon<4;weapon++)
                {
                    Set("weaponIndex",weapon);Fresh();RemoveAll();var at=(Transform)Get("player");at.position=new Vector3(0,.65f,-2.5f);var target=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,0));Spawn(ContentEnemy.Bruiser,new Vector3(3,.6f,3));
                    Call("FireContentWeapon");for(int i=0;i<24;i++)Call("TickPulses",.02f);Check((float)Item(target,"hp")<16,"Actual weapon "+weapon+" laser hits Bomber");
                }
                Fresh();RemoveAll();var player=(Transform)Get("player");player.position=new Vector3(0,.65f,0);var bomber=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,1));var survivor=Spawn(ContentEnemy.Bruiser,new Vector3(3,.6f,3));
                Check((float)Item(bomber,"hp")==16,"Bomber has GDD 16 HP");Check(((Transform)Item(bomber,"body")).GetComponent<ToyActorAnimator>(),"Bomber reuses authored animated rig");
                Hit(bomber,1000);Check(((IList)Get("deathCharges")).Count==1&&((IList)Get("foes")).Count==1,"Death removes objective target once and drops a telegraphed charge");Call("SetPaused",false);Set("invulnerability",0f);float hp=(float)Get("health");
                Call("TickDeathCharges",.4f);Check((float)Get("health")==hp,"Charge cannot damage before its fuse");Call("SetPaused",true);Call("TickDeathCharges",2f);Check(((IList)Get("deathCharges")).Count==1,"Pause freezes charge fuse");Call("SetPaused",false);
                player.position=new Vector3(-3,.65f,-3);Call("TickDeathCharges",.5f);Check((float)Get("health")==hp&&((IList)Get("deathCharges")).Count==0,"Escaping warning radius avoids damage");
                bomber=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,0));player.position=new Vector3(.5f,.65f,0);Set("invulnerability",0f);Hit(bomber,1000);Call("TickDeathCharges",.81f);Check(Mathf.Abs((float)Get("health")-(hp-13))<.01f,"Standing inside blast receives scaled 10-base damage");
                // A wounded second bomber starts its own complete fuse, not an instant recursive blast.
                Call("SetPaused",true);Call("ClearDeathCharges",false);player.position=new Vector3(-3,.65f,-3);var a=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,0));var b=Spawn(ContentEnemy.Bomber,new Vector3(.5f,.6f,0));Put(b,"hp",9f);Hit(a,1000);Call("SetPaused",false);Call("TickDeathCharges",.81f);
                Check(!((IList)Get("foes")).Contains(b)&&((IList)Get("deathCharges")).Count==1,"Explosion chains to wounded bomber with a separate visible fuse");Call("TickDeathCharges",.4f);Check(((IList)Get("deathCharges")).Count==1,"Chained charge retains its full dodge window");Call("TickDeathCharges",.5f);Check(((IList)Get("deathCharges")).Count==0,"Chained charge resolves once");
                // Completion must not add mandatory corpse-fuse time to the 20s budget.
                RemoveAll();bomber=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,0));Set("remaining",.1f);Hit(bomber,1000);Check((bool)Prop("ObjectiveComplete"),"Last bomber death immediately satisfies elimination goal");Call("TickDeathCharges",.01f);Check(((IList)Get("deathCharges")).Count==0,"Successful objective cancels harmful post-clear charge");Call("Clear");Check(Get("phase").ToString()=="Choice","Final kill at deadline clears without waiting for corpse");
                Fresh();RemoveAll();bomber=Spawn(ContentEnemy.Bomber,new Vector3(0,.6f,0));Spawn(ContentEnemy.Bruiser,new Vector3(3,.6f,3));Hit(bomber,1000);int bank=(int)Get("wallet");Call("Fail","TEST TIMEOUT",Enum.Parse(typeof(InvasionPrototype).GetNestedType("FailureKind",BindingFlags.NonPublic),"Timeout"));Check(((IList)Get("deathCharges")).Count==0&&(int)Get("wallet")==bank,"Failure cancels charges and does not bank unfinished drops");
                Fresh();var mission=(MissionSpec)Prop("Mission");Check(mission.cubes[1].enemies[0]==ContentEnemy.Bomber&&mission.cubes[1].enemies[2]==ContentEnemy.Bruiser,"Son Hat includes two Bombers plus Bruiser at second cube");
                foreach(var foe in new ArrayList((IList)Get("foes")))Hit(foe,1000);Call("Clear");Call("SetPaused",false);step=1;return;
            }
            if(step==1)
            {if(!(bool)Get("routesReady"))return;Call("Select",true);Set("invulnerability",100f);step=2;return;}
            if(step==2)
            {
                if(Get("phase").ToString()=="Transfer")return;Call("SetPaused",true);bool found=false;foreach(var foe in (IList)Get("foes"))if(Item(foe,"kind").ToString()=="Bomber")found=true;
                Check(found&&(int)Get("stage")==2,"Real gate transition spawns expanded bomber encounter");
                Capture();foreach(var foe in new ArrayList((IList)Get("foes")))Hit(foe,1000);Call("Clear");Call("SetPaused",false);step=3;return;
            }
            if(step==3) {if(!(bool)Get("routesReady"))return;Call("Select",false);Set("invulnerability",100f);step=4;return;}
            if(step==4)
            {
                if(Get("phase").ToString()=="Transfer")return;Call("SetPaused",true);
                if((int)Get("stage")==3) {bool found=false;foreach(var foe in (IList)Get("foes"))if(Item(foe,"kind").ToString()=="Bomber")found=true;Check(found,"Second bomber encounter survives the other gate direction");}
                foreach(var foe in new ArrayList((IList)Get("foes")))Hit(foe,1000);Call("Clear");Check(((IList)Get("deathCharges")).Count==0,"Cleared cubes leave no armed charges");
                if(Get("phase").ToString()=="Won") {Check((int)Get("stage")==5,"Expanded Son Hat completes with original five-cube length");Finish(0);return;}
                Call("SetPaused",false);step=3;
            }
        }
        catch(Exception e) {Debug.LogException(e);Finish(1);}
    }
    static void Capture()
    {
        Call("SetPaused",false);for(int i=0;i<12;i++) {Call("TickAnimations",.08f);((CombatFeedback)Get("feedback")).Tick(.08f);}Call("LateUpdate");var camera=(Camera)Get("view");var target=new RenderTexture(720,1280,24);var previous=RenderTexture.active;camera.targetTexture=target;camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) {vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate();}
        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;var texture=new Texture2D(720,1280,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,720,1280),0,0);texture.Apply();System.IO.File.WriteAllBytes("/tmp/cube-bomber-gameplay.png",texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
    }
    static void Finish(int code)
    {SessionState.SetBool(Key,false);if(SessionState.GetBool(Key+"exists",false))PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt(Key+"wallet",0));else PlayerPrefs.DeleteKey("CI.Wallet");PlayerPrefs.Save();if(code==0)Debug.Log("CUBE_BOMBER_OK: HP, authored rig, windup, dodge, damage, pause, chain fuse, deadline clear, failure cleanup and real gate encounter.");EditorApplication.Exit(code);}
}
