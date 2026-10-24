using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;

// Batch entry point: -executeMethod PrototypeSmokeCheck.Run (without -quit).
[InitializeOnLoad]
public static class PrototypeSmokeCheck
{
    const string Key="CI.Smoke.Active";
    static int step;
    static double deadline, next;
    static InvasionPrototype game;
    static Transform originalPlayer, originalWorld, originalCamera, oldCube, chosenCube, rejectedCube;
    static int originalScene;
    static Vector3 pausedPlayerPosition, pausedCubePosition;
    static int savedWallet;
    static bool walletExisted;
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static PrototypeSmokeCheck() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        walletExisted=PlayerPrefs.HasKey("CI.Wallet"); savedWallet=PlayerPrefs.GetInt("CI.Wallet",0);
        SessionState.SetInt("CI.Smoke.Wallet",savedWallet); SessionState.SetBool("CI.Smoke.WalletExisted",walletExisted);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        SessionState.SetBool(Key,true); EditorApplication.isPlaying=true;
    }
    static object Get(string field) => typeof(InvasionPrototype).GetField(field,Flags).GetValue(game);
    static void Set(string field,object value) => typeof(InvasionPrototype).GetField(field,Flags).SetValue(game,value);
    static void Call(string name,params object[] args) => typeof(InvasionPrototype).GetMethod(name,Flags).Invoke(game,args);
    static string State => Get("phase").ToString();
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); Debug.Log("P0_CHECK_OK: "+message); }
    static void CaptureArena()
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera=(Camera)Get("view");
        var target=new RenderTexture(540,960,24);
        camera.targetTexture=target; camera.aspect=540f/960f;
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");
        camera.GetComponent<Cinemachine.CinemachineBrain>().ManualUpdate();
        camera.Render();
        var previous=RenderTexture.active; RenderTexture.active=target;
        var image=new Texture2D(540,960,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,540,960),0,0); image.Apply();
        System.IO.File.WriteAllBytes("/tmp/cube-p0-preview.png",image.EncodeToPNG());
        camera.targetTexture=null; camera.ResetAspect(); RenderTexture.active=previous;
        UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
    }
    static void Finish(int exit)
    {
        SessionState.SetBool(Key,false);
        if(SessionState.GetBool("CI.Smoke.WalletExisted",false)) PlayerPrefs.SetInt("CI.Wallet",SessionState.GetInt("CI.Smoke.Wallet",0));
        else PlayerPrefs.DeleteKey("CI.Wallet");
        PlayerPrefs.Save(); EditorApplication.Exit(exit);
    }
    static void RemoveEnemies()
    {
        var enemies=(IList)Get("foes");
        foreach(var foe in enemies)
            UnityEngine.Object.Destroy(( (Transform)foe.GetType().GetField("body").GetValue(foe)).gameObject);
        enemies.Clear();
    }
    static Vector2 WorldInput(Vector3 direction)
    {
        var camera=(Camera)Get("view");
        var forward=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;
        return new Vector2(Vector3.Dot(direction,camera.transform.right),-Vector3.Dot(direction,forward));
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;
            if(deadline==0) deadline=now+35;
            if(now>deadline) throw new Exception("Smoke check timed out at step "+step);
            if(now<next)return;
            if(!game) game=UnityEngine.Object.FindAnyObjectByType<InvasionPrototype>();
            if(!game||Get("player")==null)return;
            switch(step)
            {
                case 0:
                    Check(EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android,"Android is the active build target");
                    Check(PlayerSettings.defaultInterfaceOrientation==UIOrientation.Portrait,"Portrait orientation configured");
                    Check(State=="Combat" && ((IList)Get("foes")).Count==2,"Scene boots with 2 mandatory enemies");
                    originalPlayer=(Transform)Get("player"); originalWorld=(Transform)Get("world");
                    originalCamera=((Camera)Get("view")).transform;
                    originalScene=game.gameObject.scene.handle;
                    Check(((Transform)Get("rightPreview")).position.y<0 && ((Transform)Get("forwardPreview")).position.y<0,
                        "Next cubes begin below the arena and rise into place");
                    Set("paused",true); Set("remaining",12f); next=now+.2; step++; break;
                case 1:
                    var followView=(Camera)Get("view");
                    Check(followView.GetComponent<ArcadeFollowCamera>()!=null &&
                        Mathf.Abs(Mathf.DeltaAngle(followView.transform.eulerAngles.x,45))<.1f,
                        "Camera uses 45-degree arcade follow rig");
                    Check(Mathf.Abs(Mathf.DeltaAngle(followView.transform.eulerAngles.y,45))<.1f &&
                        followView.transform.position.x<((Transform)Get("player")).position.x,
                        "Camera looks from the character upper-left diagonal");
                    var brain=followView.GetComponent<Cinemachine.CinemachineBrain>();
                    Check(brain && brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera,
                        "Cinemachine Brain drives the live virtual camera");
                    var liveCamera=(Cinemachine.CinemachineVirtualCamera)brain.ActiveVirtualCamera;
                    var body=liveCamera.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();
                    Check(body && body.m_BindingMode==Cinemachine.CinemachineTransposer.BindingMode.WorldSpace && body.m_XDamping>0,
                        "Cinemachine Transposer provides world-space follow damping");
                    CaptureArena();
                    Check((float)Get("remaining")==12f,"Pause freezes combat timer");
                    var player=(Transform)Get("player");
                    var initial=player.position;
                    player.position=new Vector3(3.8f,.65f,0);
                    Call("MovePlayer",WorldInput(Vector3.right),.2f);
                    Check(State=="Combat" && player.position.x<4,"Closed combat gates block passage");
                    player.position=initial;
                    Call("MovePlayer",Vector2.right,0f);
                    Check(Vector3.Dot(player.forward,followView.transform.right)>.99f,"Joystick turns character toward movement");
                    Call("MovePlayer",Vector2.zero,.1f);
                    Check(Vector3.Dot(player.forward,followView.transform.right)>.99f,"Releasing joystick retains facing direction");
                    var enemies=(IList)Get("foes");
                    var ahead=enemies[0]; var behind=enemies[1];
                    ((Transform)ahead.GetType().GetField("body").GetValue(ahead)).position=player.position+followView.transform.right*2;
                    ((Transform)behind.GetType().GetField("body").GetValue(behind)).position=player.position-followView.transform.right*2;
                    Call("ClearPulses"); Call("FirePulse"); Call("TickPulses",.2f);
                    Check((float)ahead.GetType().GetField("hp").GetValue(ahead)==12 &&
                          (float)behind.GetType().GetField("hp").GetValue(behind)==18,"Forward projectile hits facing target and ignores enemy behind");
                    Call("FirePulse"); Call("TickPulses",.2f); Call("FirePulse"); Call("TickPulses",.2f);
                    Call("MovePlayer",Vector2.left,0f);
                    for(int i=0;i<3;i++) { Call("FirePulse"); Call("TickPulses",.2f); }
                    Check(enemies.Count==0,"Facing shots eliminate both mandatory enemies");
                    Set("paused",false); step++; break;
                case 2:
                    if(State!="Choice" || !(bool)Get("routesReady"))return;
                    Check((float)Get("remaining")>0,"Objective resolves before timeout");
                    Check(!((Transform)Get("rightBarrier")).gameObject.activeSelf &&
                          !((Transform)Get("forwardBarrier")).gameObject.activeSelf,"Clear opens both physical gates");
                    Set("remaining",9f); next=now+.3; step++; break;
                case 3:
                    Check((float)Get("remaining")==9f,"Open-gate traversal freezes timer without pausing gameplay");
                    Set("health",50f);
                    var scout=(Transform)Get("player");
                    scout.position=new Vector3(3.9f,.65f,1.5f);
                    Call("MovePlayer",WorldInput(Vector3.right),.1f);
                    Check(State=="Choice","Walking beside gate does not select route");
                    oldCube=(Transform)Get("platform"); chosenCube=(Transform)Get("rightPreview"); rejectedCube=(Transform)Get("forwardPreview");
                    scout.position=new Vector3(3.9f,.65f,0);
                    Call("MovePlayer",WorldInput(Vector3.right),.1f);
                    Check(State=="Transfer" && (float)Get("health")==80f,"Crossing right gate grants health immediately");
                    Call("Select",false);
                    Check((int)Get("damageCubes")==0,"Duplicate gate commit cannot grant other reward");
                    next=now+.45; step=30; break;
                case 30:
                    Check((Transform)Get("player")==originalPlayer && (Transform)Get("world")==originalWorld &&
                        ((Camera)Get("view")).transform==originalCamera && game.gameObject.scene.handle==originalScene,
                        "Transfer preserves player, world, Cinemachine camera and scene identity");
                    Check(oldCube && Mathf.Abs(oldCube.position.y)<.001f && rejectedCube && rejectedCube.position.y<0,
                        "Old floor remains until safe; rejected cube falls instead of popping away");
                    Check((Transform)Get("rightPreview")!=chosenCube && ((Transform)Get("rightPreview")).position.y<0,
                        "Following route cubes rise during bridge traversal");
                    Set("paused",true); pausedPlayerPosition=originalPlayer.position; pausedCubePosition=rejectedCube.position;
                    next=now+.2; step=31; break;
                case 31:
                    Check(originalPlayer.position==pausedPlayerPosition && rejectedCube.position==pausedCubePosition,
                        "Pause freezes both traversal and falling cube animation");
                    Set("paused",false); next=now+.65; step=4; break;
                case 4:
                    if(State!="Combat")return;
                    Check((int)Get("stage")==2 && (float)Get("health")==80f,"Right gate reward applies only once");
                    Check((Transform)Get("platform")==chosenCube && (Transform)Get("player")==originalPlayer,
                        "Existing candidate is promoted without respawning the character");
                    Check(oldCube && oldCube.position.y<0,"Previous combat cube falls after the character has crossed");
                    var trackingView=(Camera)Get("view");
                    var followed=(Transform)Get("player");
                    var trackedFocus=trackingView.transform.position+trackingView.transform.forward*20;
                    var desiredFocus=new Vector3(followed.position.x,0,followed.position.z)+
                        Vector3.ProjectOnPlane(trackingView.transform.forward,Vector3.up).normalized;
                    Check(Vector3.Distance(trackedFocus,desiredFocus)<1,
                        "Camera follows character across the bridge");
                    Check((float)Get("remaining")>19f,"Bridge arrival starts fresh 20-second timer");
                    Set("remaining",.001f); Set("shot",100f); next=now+1.6; step++; break;
                case 5:
                    Check(State=="Failed","Timeout collapses cube and reaches fail without opening gates");
                    Check(!oldCube && !rejectedCube,"Retired cubes are removed after falling animation");
                    UnityEngine.Object.DestroyImmediate(game.gameObject);
                    UnityEngine.Object.DestroyImmediate(GameObject.Find("P0 generated models"));
                    var fresh=new GameObject("Smoke level").AddComponent<InvasionPrototype>();
                    fresh.rightRoute=AssetDatabase.LoadAssetAtPath<CubeRouteDefinition>("Assets/CubeInvasion/HealthRoute.asset");
                    fresh.forwardRoute=AssetDatabase.LoadAssetAtPath<CubeRouteDefinition>("Assets/CubeInvasion/DamageRoute.asset");
                    game=fresh; next=now+.1; step++; break;
                case 6:
                    // Isolate forward-gate and final-level transitions from the shot checks above.
                    RemoveEnemies(); step++; break;
                case 7:
                    if(State!="Choice" || !(bool)Get("routesReady"))return;
                    ((Transform)Get("player")).position=new Vector3(0,.65f,3.9f);
                    Call("MovePlayer",WorldInput(Vector3.forward),.1f);
                    Check(State=="Transfer" && (int)Get("damageCubes")==2 && (float)Get("damageBonus")==.15f,
                          "Crossing forward gate grants two-cube damage upgrade");
                    next=now+1.05; step++; break;
                case 8:
                    if(State!="Combat")return;
                    Check((int)Get("stage")==2 && (int)Get("damageCubes")==2,"Bridge transfer preserves damage buff duration");
                    Set("stage",3); RemoveEnemies(); step++; break;
                case 9:
                    if(State!="Won")return;
                    Check((int)Get("damageCubes")==0,"Final objective ends level and clears temporary buffs");
                    Debug.Log("CUBE_P0_SMOKE_OK"); Finish(0); break;
            }
        }
        catch(Exception exception) { Debug.LogException(exception); Finish(1); }
    }
}
