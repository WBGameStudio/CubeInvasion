using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using DG.Tweening;
using Object = UnityEngine.Object;
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
    static UnityEngine.SceneManagement.SceneHandle originalScene;
    static Vector3 pausedPlayerPosition, pausedCubePosition;
    static P1PerformanceProbe probe;
    static int savedWallet;
    static bool walletExisted;
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static PrototypeSmokeCheck() { EditorApplication.update+=Tick; }
    public static void Run()
    {
        walletExisted=PlayerPrefs.HasKey("CI.Wallet"); savedWallet=PlayerPrefs.GetInt("CI.Wallet",0);
        SessionState.SetInt("CI.Smoke.Wallet",savedWallet); SessionState.SetBool("CI.Smoke.WalletExisted",walletExisted);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var runtime=Object.FindAnyObjectByType<InvasionPrototype>();
        var setup=new SerializedObject(runtime);setup.FindProperty("content").objectReferenceValue=null;setup.ApplyModifiedPropertiesWithoutUndo();
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
    static object Item(object value,string field) => value.GetType().GetField(field).GetValue(value);
    static void ItemSet(object value,string field,object data) => value.GetType().GetField(field).SetValue(value,data);
    static void AnimationChecks()
    {
        var animator=(ToyActorAnimator)Get("playerAnimation");var player=(Transform)Get("player");
        Check(animator && animator.JointCount>=4,"DOTween rig has independent player arms and legs");
        Vector3 rootBefore=player.position;
        animator.TickPose(player.forward*1.2f,.2f,false);
        var left=animator.VisualRoot.Find("DOTween Reactions/LeftLeg");var right=animator.VisualRoot.Find("DOTween Reactions/RightLeg");
        Check(animator.CurrentMotion==ToyActorAnimator.Motion.Walk && Quaternion.Angle(left.localRotation,right.localRotation)>5,
            "DOTween walk alternates leg joints");
        animator.TickPose(player.forward*4.5f,.15f,true);
        Check(animator.CurrentMotion==ToyActorAnimator.Motion.Run && player.position==rootBefore,"Run pose leaves gameplay root unchanged");
        var pose=left.localRotation;var visual=animator.VisualRoot.localPosition;
        animator.TickFromRoot(.2f,true,true,true);
        Check(left.localRotation==pose && animator.VisualRoot.localPosition==visual,"Pause freezes rig pose and tween clocks");
        animator.TickPose(Vector3.zero,.2f,false);
        Check(animator.CurrentMotion==ToyActorAnimator.Motion.Idle && Quaternion.Angle(left.localRotation,Quaternion.identity)<.1f,
            "Stopping returns limbs to idle stance");
        animator.Shoot();animator.TickPose(Vector3.zero,.025f,true);
        Check(animator.VisualRoot.Find("DOTween Reactions").localPosition.sqrMagnitude>.0000001f,"DOTween shot recoil changes visual pose");
        Check(player.position==rootBefore,"Recoil never moves gameplay root");
        Debug.Log("CUBE_ANIMATION_SMOKE_OK");
    }
    static void FeelChecks()
    {
        Set("paused",true);Set("stage",2);Call("BeginCombat",game.forwardRoute);Set("health",80f);Set("invulnerability",0f);
        var hud=(MobileCombatHud)Get("hud");
        Check(hud && hud.Canvas && hud.SafeRoot.anchorMin.x>=0 && hud.SafeRoot.anchorMax.x<=1,"P1 Canvas HUD fits safe area");
        Canvas.ForceUpdateCanvases();
        var joystick=hud.GetComponentInChildren<MobileJoystick>();
        var view=(Camera)Get("view");var rect=(RectTransform)joystick.transform;
        var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { pointerId=7 };
        pointer.pointerPressRaycast=new UnityEngine.EventSystems.RaycastResult { module=hud.GetComponent<UnityEngine.UI.GraphicRaycaster>() };
        pointer.position=RectTransformUtility.WorldToScreenPoint(view,rect.TransformPoint(Vector3.zero));joystick.OnPointerDown(pointer);
        pointer.position=RectTransformUtility.WorldToScreenPoint(view,rect.TransformPoint(new Vector3(45,35,0)));joystick.OnDrag(pointer);
        Check(hud.InputValue.x>.1f && hud.InputValue.y<-.1f,"P1 joystick converts pointer drag to screen-relative movement");
        var other=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { pointerId=8 };
        joystick.OnPointerUp(other);Check(hud.InputValue.sqrMagnitude>0,"Second finger cannot release movement joystick");
        joystick.OnPointerUp(pointer);Check(hud.InputValue==Vector2.zero,"Joystick release clears movement");
        var enemies=(IList)Get("foes");object gunner=enemies[0], sprinter=enemies[enemies.Count-1];
        var player=(Transform)Get("player");
        foreach(var foe in enemies) { ItemSet(foe,"spawn",0f);ItemSet(foe,"attack",100f); }
        Check(Item(gunner,"kind").ToString()=="Gunner","Risky route includes ranged gunner");
        ((Transform)Item(gunner,"body")).position=player.position+Vector3.forward*3;
        ItemSet(gunner,"attack",0f);Call("TickEnemies",.01f);
        Check((float)Item(gunner,"windup")>0 && ((IList)Get("enemyShots")).Count==0,"Gunner telegraphs before firing");
        Call("TickEnemies",1.05f);Call("TickEnemies",.01f);
        Check(((IList)Get("enemyShots")).Count==1,"Gunner fires after visible windup");Call("ClearEnemyShots");
        ItemSet(gunner,"attack",100f);ItemSet(gunner,"burst",0);
        ((Transform)Item(sprinter,"body")).position=player.position+Vector3.left*2;
        ItemSet(sprinter,"attack",0f);Call("TickEnemies",.01f);
        Check((float)Item(sprinter,"windup")>0,"Sprinter warns before dash");Call("TickEnemies",.7f);
        Check((float)Item(sprinter,"dash")>0,"Sprinter enters committed dash after warning");
        Call("ClearPulses");Call("FirePulse");var first=(Transform)Item(((IList)Get("pulses"))[0],"body");Call("ClearPulses");Call("FirePulse");
        Check((Transform)Item(((IList)Get("pulses"))[0],"body")==first,"Player projectile pool reuses objects");Call("ClearPulses");
        var cover=(Transform)Get("cover");
        var art=(ToyArtLibrary)Get("artLibrary");
        Check(art.TryGet("Low cover",out var coverMesh) && cover.GetComponent<MeshFilter>().sharedMesh==coverMesh.mesh,
            "P1 cover uses authored art mesh");
        foreach(var foe in enemies)((Transform)Item(foe,"body")).position=(Vector3)Get("centre")+new Vector3(-4,.55f,4);
        ((Transform)Item(gunner,"body")).position=cover.position+Vector3.forward*2;
        player.position=cover.position-Vector3.forward*2;Call("MovePlayer",WorldInput(Vector3.forward),0f);
        float targetHp=(float)Item(gunner,"hp");Call("FirePulse");Call("TickPulses",.25f);
        Check(Get("currentTarget")!=gunner && (float)Item(gunner,"hp")==targetHp,
            "Player skips the nearer target blocked by cover");Call("ClearPulses");
        float protectedHealth=(float)Get("health");Set("invulnerability",0f);
        Call("FireEnemyShot",cover.position+Vector3.forward*2,Vector3.back,6f,7f);Call("TickEnemyShots",.6f);
        Check((float)Get("health")==protectedHealth && ((IList)Get("enemyShots")).Count==0,"Cover blocks incoming enemy projectiles");
        var pickupList=(IList)Get("pickups");foreach(var item in pickupList)UnityEngine.Object.Destroy(((Transform)Item(item,"body")).gameObject);pickupList.Clear();
        Call("SpawnPickup",player.position,true,20);Call("CollectPickups",0f,false);
        Check((float)Get("health")==100,"Health pickup heals and clamps to max HP");
        int bank=(int)Get("wallet");Call("SpawnPickup",player.position,false,5);Call("CollectPickups",0f,false);
        Check((int)Get("pendingGold")==5 && (int)Get("wallet")==bank,"Combat gold remains unbanked until clear");
        var barrel=((IList)Get("barrels"))[0];
        var barrelPosition=((Transform)Item(barrel,"body")).position;
        var chained=((IList)Get("barrels"))[1];((Transform)Item(chained,"body")).position=barrelPosition+Vector3.right*1.8f;
        ((Transform)Item(gunner,"body")).position=barrelPosition+Vector3.right*.2f;
        ItemSet(gunner,"hp",18f);ItemSet(gunner,"windup",0f);ItemSet(gunner,"burst",0);
        player.position=barrelPosition+Vector3.right*.7f;Set("health",100f);Set("invulnerability",0f);
        int targets=enemies.Count;Call("HitBarrel",barrel,12f);
        Check((float)Item(barrel,"fuse")>0 && enemies.Count==targets,"Barrel starts visible fuse without immediate explosion");
        Call("TickWorldFeel",.2f);Check(enemies.Count==targets,"Barrel respects fuse delay");
        Call("TickWorldFeel",.3f);
        Check(enemies.Count<targets && (float)Get("health")==90,"Explosion damages enemies and player; eliminated enemies leave objective");
        Check((float)Item(chained,"fuse")>0,"Chain reaction starts another visible fuse instead of instant damage");
        Call("DamagePlayer",10f,Vector3.right);Check((float)Get("health")==90,"Damage grace period prevents immediate repeated hit");
        var beforeHealthFailure=((Transform)Get("platform")).position;
        Set("invulnerability",0f);Set("health",3f);Call("FireEnemyShot",player.position-Vector3.forward,Vector3.forward,6f,7f);Call("TickEnemyShots",.2f);
        Check(State=="Dying" && ((IList)Get("enemyShots")).Count==0 && (int)Get("pendingGold")==0,"Lethal projectile fails safely and discards unbanked gold");
        Check(((Transform)Get("platform")).position==beforeHealthFailure,"Health failure leaves combat cube stationary");
        Check((int)Get("wallet")==bank,"Failed cube preserves previously banked wallet");
        var feedback=(CombatFeedback)Get("feedback");
        for(int i=0;i<100;i++)feedback.Burst(player.position,Color.white);
        Check(feedback.ActiveEffects<=48,"Visual effect budget remains bounded under burst load");
        feedback.SetPaused(false);feedback.Tick(1);Check(feedback.ActiveEffects==0,"Finished visual effects return to pool");
        // Start a fresh P1 encounter for a visual check and a short editor-only profile.
        RemoveEnemies();Set("stage",2);Call("BeginCombat",game.forwardRoute);
        Set("health",75f);Set("remaining",18.5f);Set("invulnerability",100f);Set("paused",true);
        player.position=(Vector3)Get("centre")+new Vector3(0,.65f,-2.5f);
        Debug.Log("CUBE_P1_SMOKE_OK");
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
                    Check(Object.FindObjectsByType<Camera>().Length==1,"Authored scene camera is reused without duplicates");
                    Check(((MobileCombatHud)Get("hud")).SafeRoot && ((Transform)Get("playerBody")),"Authored HUD and player references survive scene serialization");
                    var art=(ToyArtLibrary)Get("artLibrary");
                    Check(art && art.models.Length>=26,"Toy art library replaces gameplay placeholders");
                    foreach(var model in art.models)
                    {
                        if(model.rig==null||model.rig.Length==0)continue;
                        int indices=0;foreach(var part in model.rig)indices+=part.mesh.triangles.Length;
                        Check(indices==model.mesh.triangles.Length,"Articulation preserves model geometry: "+model.key);
                    }
                    Check(art.TryGet("Armored body",out var scoutMesh) &&
                        ((Transform)Get("player")).GetChild(0).GetComponent<MeshFilter>().sharedMesh==scoutMesh.mesh,
                        "Scout uses authored robot mesh instead of capsule");
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
                    AnimationChecks();
                    CaptureArena();
                    Check((float)Get("remaining")==12f,"Pause freezes combat timer");
                    var player=(Transform)Get("player");
                    var initial=player.position;
                    player.position=new Vector3(3.8f,.65f,0);
                    Call("MovePlayer",WorldInput(Vector3.right),.2f);
                    Check(State=="Combat" && player.position.x<4,"Closed combat gates block passage");
                    player.position=initial;
                    var enemies=(IList)Get("foes");var ahead=enemies[0];var behind=enemies[1];
                    ((Transform)Item(ahead,"body")).position=initial+Vector3.forward*2;
                    ((Transform)Item(behind,"body")).position=initial+Vector3.back*3;
                    Call("MovePlayer",WorldInput(Vector3.right),.05f);
                    Check(player.position.x>initial.x && Vector3.Dot(player.forward,(((Transform)Item(ahead,"body")).position-player.position).normalized)>.99f,
                        "Character moves sideways while locking onto nearest enemy");
                    Call("ClearPulses");Call("FirePulse");Call("TickPulses",.2f);
                    Check((float)Item(ahead,"hp")==12 && (float)Item(behind,"hp")==18,"Auto-fire hits nearest target independently of movement");
                    ((Transform)Item(behind,"body")).position=player.position+Vector3.left;
                    Call("MovePlayer",Vector2.zero,0f);
                    Check(Get("currentTarget")==behind,"Nearest lock switches when another enemy becomes closer");
                    for(int i=0;i<3;i++) { Call("FirePulse");Call("TickPulses",.2f); }
                    Check(enemies.Count==1 && Get("currentTarget")==behind,"Closest enemy eliminated without manual aim");
                    for(int i=0;i<2;i++) { Call("FirePulse");Call("TickPulses",.2f); }
                    Check(enemies.Count==0,"Lock reacquires remaining enemy after target death");
                    Call("FirePulse");Check(((IList)Get("pulses")).Count==0,"No enemy means no automatic fire");
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
                    Check(chosenCube && chosenCube.position.y< -8,"Timeout drops the active combat cube");
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
                    Debug.Log("CUBE_P0_SMOKE_OK"); FeelChecks(); next=now+.2;step=10;break;
                case 10:
                    var mobileHud=(MobileCombatHud)Get("hud");var audioFeedback=(CombatFeedback)Get("feedback");
                    mobileHud.Refresh(18.5f,75,2,((IList)Get("foes")).Count,(int)Get("wallet"),0,0,0,false,audioFeedback.Muted,"Combat","");
                    var captureView=(Camera)Get("view");
                    captureView.GetComponent<ArcadeFollowCamera>().SendMessage("Update");
                    var captureVcam=(Cinemachine.CinemachineVirtualCamera)captureView.GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera;
                    captureVcam.PreviousStateIsValid=false;captureVcam.UpdateCameraState(Vector3.up,-1);
                    CaptureArena();
                    Set("paused",false);probe=game.gameObject.AddComponent<P1PerformanceProbe>();probe.Capture(180,"/tmp/cube-p1-profile.json");step=11;break;
                case 11:
                    if(!probe.Finished)return;
                    Check(System.IO.File.Exists(probe.OutputPath),"P1 editor performance sample written");
                    Set("paused",true);CaptureAnimationReview((ToyArtLibrary)Get("artLibrary"));
                    Set("phase",Enum.Parse(Get("phase").GetType(),"Combat"));
                    Set("paused",false);
                    pausedCubePosition=((Transform)Get("platform")).position;
                    originalPlayer=(Transform)Get("player");
                    originalPlayer.position=(Vector3)Get("centre")+new Vector3(0,.65f,-4.7f);
                    Call("Update");
                    Check(State=="Falling","Edge crossing starts player-only falling phase");
                    next=EditorApplication.timeSinceStartup+.2;step=12;break;
                case 12:
                    Check(State=="Falling","Edge fall delays failure until player has fallen; actual="+State);
                    Check(((Transform)Get("platform")).position==pausedCubePosition,"Edge fall leaves combat cube stationary");
                    Check(originalPlayer.position.y<.65f,"Only the player falls off the edge");
                    Set("paused",true);pausedPlayerPosition=originalPlayer.position;
                    next=now+.2;step=13;break;
                case 13:
                    Check(originalPlayer.position==pausedPlayerPosition,"Pause freezes edge fall");
                    Set("paused",false);next=now+1.2;step=14;break;
                case 14:
                    Check(State=="Failed" && originalPlayer.position.y< -8,"Failure appears after the player falls");
                    Check(((Transform)Get("platform")).position==pausedCubePosition,"Failed edge fall never drops the cube");
                    Debug.Log("CUBE_FALL_SMOKE_OK");Finish(0);break;
            }
        }
        catch(Exception exception) { Debug.LogException(exception); Finish(1); }
    }
    public static void CaptureAnimationReview(ToyArtLibrary library)
    {
        const string folder="/tmp/cube-animation-frames";Directory.CreateDirectory(folder);
        library.TryGet("Armored body",out var activeModel);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(activeModel.integratedFace?CuteArtBuilder.Root+"/Prefabs/"+CuteArtBuilder.Slug("Armored body")+".prefab":"Assets/CubeInvasion/Art/Prefabs/Scout_Robot.prefab");
        if(!prefab)prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CubeInvasion/Art/Prefabs/Scout_Robot.prefab");
        var actor=Object.Instantiate(prefab);actor.name="Animation review";actor.transform.position=Vector3.zero;actor.transform.rotation=Quaternion.identity;
        var animator=actor.GetComponent<ToyActorAnimator>();if(!animator)animator=actor.AddComponent<ToyActorAnimator>();animator.enabled=false;
        var accessories=new List<Transform>();foreach(Transform child in actor.transform)accessories.Add(child);
        library.TryGet("Armored body",out var model);animator.Bind(model,actor.transform);foreach(var child in accessories)animator.Attach(child);
        foreach(var node in actor.GetComponentsInChildren<Transform>())node.gameObject.layer=31;
        var cameraObject=new GameObject("Animation review camera");var camera=cameraObject.AddComponent<Camera>();camera.cullingMask=1<<31;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.085f,.14f);camera.orthographic=true;camera.orthographicSize=.9f;
        camera.transform.rotation=Quaternion.Euler(24,155,0);camera.transform.position=Vector3.up*.12f-camera.transform.forward*3;
        var labelObject=new GameObject("Animation state");labelObject.layer=31;var label=labelObject.AddComponent<TextMesh>();label.fontSize=48;label.characterSize=.012f;label.anchor=TextAnchor.MiddleCenter;label.color=Color.white;
        label.transform.rotation=camera.transform.rotation;label.transform.position=Vector3.up*.9f;
        var target=new RenderTexture(480,480,24);camera.targetTexture=target;var previous=RenderTexture.active;
        var image=new Texture2D(480,480,TextureFormat.RGB24,false);
        for(int i=0;i<96;i++)
        {
            bool idle=i<16,run=i>=48;
            label.text=idle?"IDLE":run?"RUN + AUTO FIRE":"WALK";
            if(run && i%10==0)animator.Shoot();
            animator.TickPose(idle?Vector3.zero:Vector3.forward*(run?4.5f:1.2f),1f/24,run);
            camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,480,480),0,0);image.Apply();
            File.WriteAllBytes(folder+"/"+i.ToString("D3")+".png",image.EncodeToPNG());
        }
        camera.targetTexture=null;RenderTexture.active=previous;
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var gait=(Tween)typeof(ToyActorAnimator).GetField("gait",flags).GetValue(animator);
        var idleTween=(Tween)typeof(ToyActorAnimator).GetField("idle",flags).GetValue(animator);
        Object.DestroyImmediate(image);Object.DestroyImmediate(target);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(labelObject);Object.DestroyImmediate(actor);
        if(gait.IsActive()||idleTween.IsActive())throw new System.Exception("Actor destruction left locomotion tweens active");
        Debug.Log("CUBE_ANIMATION_CLEANUP_OK");
        Debug.Log("CUBE_ANIMATION_REVIEW_OK: "+folder);
        if(activeModel.integratedFace)CuteArtReview.Animation();
    }
}
