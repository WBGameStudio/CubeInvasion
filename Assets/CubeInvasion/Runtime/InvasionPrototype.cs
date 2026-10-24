using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CubeInvasion.Prototype
{
    // A self-contained P0 playground; legacy scenes and components remain available.
    public sealed partial class InvasionPrototype : MonoBehaviour
    {
        public CubeRouteDefinition rightRoute, forwardRoute;
        [SerializeField] ToyArtLibrary artLibrary;
        enum Phase { Loadout, Combat, Choice, Transfer, Collapse, Falling, Dying, Failed, Won }
        enum FailureKind { Health, EdgeFall, Timeout }
        sealed class Foe
        {
            public Transform body;
            public float hp, speed, attack;
            public EnemyKind kind;
            public Transform warning;
            public Color color;
            public Renderer visual;
            public ToyActorAnimator animation;
            public Vector3 aim;
            public float spawn=.65f, windup, dash, burstClock, flash;
            public int burst, pattern;
            public bool dashHit;
        }
        sealed class Pulse
        {
            public Transform body;
            public Vector3 direction;
            public float life = 1f, damage;
        }
        readonly List<Pulse> pulses = new List<Pulse>();
        Transform rightGate, forwardGate, rightBarrier, forwardBarrier;
        Texture2D joystickDisc;
        int joystickFinger = -1;
        float damageBonus;
        readonly List<Foe> foes = new List<Foe>();
        readonly List<Material> materials = new List<Material>();
        [Header("Persistent scene setup")]
        [SerializeField] Transform world, player, gun, playerBody, playerVisor, dynamicRoot;
        [SerializeField] Camera view;
        Transform platform, rightPreview, forwardPreview;
        Foe currentTarget;
        Phase phase;
        Vector3 centre;
        Vector2 stick, stickOrigin;
        bool dragging, paused, routesReady;
        int settledPreviews;
        int stage, wallet, damageCubes;
        float health, remaining, shot, invulnerability;
        string reason;
        GUIStyle titleStyle, labelStyle, smallStyle, buttonStyle;
        readonly Color cyan = new Color(.09f,.74f,.82f);
        readonly Color danger = new Color(.9f,.29f,.41f);
        const float Half = 4.5f;

        void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Input.multiTouchEnabled = true;
            if(!artLibrary)artLibrary=Resources.Load<ToyArtLibrary>("ToyArtLibrary");
            BuildWorld();
        }
        Material Mat(Color color)
        {
            if(materialCache.TryGetValue(color,out var shared))return shared;
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            materials.Add(material);
            materialCache[color]=material;
            return material;
        }
        Transform Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent)
        {
            GameObject obj;
            if(artLibrary && artLibrary.TryGet(name,out var model))
            {
                obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
                obj.GetComponent<MeshFilter>().sharedMesh=model.mesh;
                obj.GetComponent<MeshRenderer>().sharedMaterials=new[]{Mat(color),model.trim,model.accent};
            }
            else
            {
                obj=GameObject.CreatePrimitive(type);obj.name=name;
                obj.GetComponent<Renderer>().sharedMaterial=Mat(color);
            }
            obj.transform.SetParent(Application.isPlaying && parent==world && dynamicRoot?dynamicRoot:parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            // Movement and hits are resolved by this prototype's bounded simulation.
            if(obj.TryGetComponent<Collider>(out var collider))Destroy(collider);
            return obj.transform;
        }
        Transform Arena(Vector3 at, bool preview, Color accent)
        {
            var root = new GameObject(preview ? "Route preview" : "Combat cube").transform;
            root.SetParent(dynamicRoot?dynamicRoot:world); root.position = at;
            Shape("Deep sides", PrimitiveType.Cube, new Vector3(0,-1.2f,0), new Vector3(9,2.4f,9), new Color(.1f,.16f,.28f),root);
            Shape("Arena surface", PrimitiveType.Cube, new Vector3(0,-.04f,0), new Vector3(9,.16f,9),new Color(.64f,.73f,.82f),root);
            for(int i=0;i<4;i++)
            {
                bool alongX = i < 2;
                Shape("Edge light",PrimitiveType.Cube,new Vector3(alongX?0:(i==2?-4.45f:4.45f),.03f,alongX?(i==0?-4.45f:4.45f):0),
                    alongX?new Vector3(9,.06f,.07f):new Vector3(.07f,.06f,9),accent,root);
            }
            if(preview)
            {
                Shape("Reward beacon", PrimitiveType.Cube,new Vector3(0,1,0),new Vector3(1.2f,.3f,.3f),accent,root);
                Shape("Reward beacon vertical", PrimitiveType.Cube,new Vector3(0,1,0),new Vector3(.3f,1.2f,.3f),accent,root);
            }
            return root;
        }
        // Also called by the scene installer. Existing authored objects are preserved.
        public void EnsureSceneSetup()
        {
            if(world)
            {
                if(!dynamicRoot) { dynamicRoot=new GameObject("Dynamic gameplay").transform; dynamicRoot.SetParent(world,false); }
                if(view && !view.GetComponent<AudioListener>())view.gameObject.AddComponent<AudioListener>();
                return;
            }
            if(!artLibrary)artLibrary=Resources.Load<ToyArtLibrary>("ToyArtLibrary");
            world = new GameObject("P0 generated models").transform;
            dynamicRoot=new GameObject("Dynamic gameplay").transform;dynamicRoot.SetParent(world,false);
            var cameraObject = new GameObject("Prototype Camera");
            cameraObject.transform.SetParent(world);
            view = cameraObject.AddComponent<Camera>(); view.tag="MainCamera";
            cameraObject.AddComponent<AudioListener>();
            view.orthographic=true;
            view.backgroundColor=new Color(.063f,.102f,.176f); view.clearFlags=CameraClearFlags.SolidColor;
            view.transform.rotation=Quaternion.Euler(45,45,0);
            var lightObject=new GameObject("Key light"); lightObject.transform.SetParent(world);
            var light=lightObject.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.3f;
            light.transform.rotation=Quaternion.Euler(45,-25,0);
            RenderSettings.ambientLight=new Color(.55f,.63f,.78f);
            player=new GameObject("Scout robot").transform; player.SetParent(world);
            player.position=new Vector3(0,.65f,-2.5f);
            playerBody=Shape("Armored body",PrimitiveType.Capsule,Vector3.zero,new Vector3(.65f,.55f,.65f),new Color(.22f,.36f,.9f),player);
            playerVisor=Shape("Visor",PrimitiveType.Cube,new Vector3(0,.3f,.29f),new Vector3(.5f,.19f,.14f),cyan,player);
            gun=Shape("Pulse pistol",PrimitiveType.Cube,new Vector3(.35f,0,.4f),new Vector3(.2f,.2f,.65f),new Color(.09f,.13f,.23f),player);
            cameraObject.AddComponent<ArcadeFollowCamera>().Follow(player);
            feedback=new GameObject("Combat feedback").AddComponent<CombatFeedback>();
            feedback.transform.SetParent(world,false);
            hud=new GameObject("Mobile Combat HUD",typeof(RectTransform)).AddComponent<MobileCombatHud>();
            hud.transform.SetParent(world,false);
            hud.Initialize(view,()=>SetPaused(!paused),Retry,()=>feedback.ToggleMute());
        }
        void BuildWorld()
        {
            EnsureSceneSetup();
            centre=Vector3.zero; stage=1; health=100; remaining=20;
            wallet=PlayerPrefs.GetInt("CI.Wallet",0);
            platform=Arena(centre,false,cyan);
            playerAnimation=AnimateActor(playerBody,player,"Armored body",playerVisor,gun);
            view.GetComponent<ArcadeFollowCamera>().Follow(player);
            InitializeFeel();
            if(HasContent) { InitializeContent();return; }
            if(missionHud)missionHud.gameObject.SetActive(false);
            BeginCombat(null);
            PrepareNextCubes(centre,platform);
        }
        void BeginCombat(CubeRouteDefinition route)
        {
            remaining=20; shot=0; invulnerability=1.3f; phase=Phase.Combat;
            SetupEncounterFeel();
            if(HasContent) { SpawnContentEncounter();return; }
            int grunts=route?route.grunts:2, runners=route?route.sprinters:0;
            for(int i=0;i<grunts+runners;i++)
            {
                bool runner=i>=grunts;
                bool gunner=route && route.risk>=2 && i==0 && !runner;
                var color=gunner?new Color(.64f,.3f,.95f):runner?new Color(1,.62f,.2f):danger;
                float angle=(i+1)*Mathf.PI*2/(grunts+runners);
                var body=Shape(gunner?"Gunner":runner?"Sprinter":"Grunt",PrimitiveType.Cube,centre+new Vector3(Mathf.Cos(angle)*3,.55f,1+Mathf.Sin(angle)*1.8f),
                    runner?new Vector3(.45f,.75f,.8f):new Vector3(.8f,.9f,.8f),color,world);
                var crest=Shape("Threat crest",PrimitiveType.Cube,new Vector3(0,.65f,0),new Vector3(.18f,.4f,.18f),Color.white,body);
                Transform cannon=gunner?Shape("Gunner barrel",PrimitiveType.Cube,new Vector3(0,0,.7f),new Vector3(.25f,.25f,.9f),Color.white,body):null;
                var animation=AnimateActor(body,body,gunner?"Gunner":runner?"Sprinter":"Grunt",crest,cannon);
                if(animation)animation.Spawn();
                var warning=WarningRing(body.position,runner?1:1.5f,color);warning.gameObject.SetActive(false);
                foes.Add(new Foe {body=body,hp=gunner?21:runner?12:18,speed=runner?2.2f:1.05f,attack=0,
                    color=color,visual=animation?animation.CoreRenderer:body.GetComponent<Renderer>(),animation=animation,kind=gunner?EnemyKind.Gunner:runner?EnemyKind.Sprinter:EnemyKind.Grunt,warning=warning});
                feedback.Burst(new Vector3(body.position.x,.1f,body.position.z),color,1,.65f,true);
            }
        }
        void PrepareNextCubes(Vector3 origin, Transform owner)
        {
            routesReady=false; settledPreviews=0;
            rightPreview=Arena(origin+Vector3.right*12+Vector3.down*14,true,new Color(.09f,.66f,.54f));
            forwardPreview=Arena(origin+Vector3.forward*12+Vector3.down*14,true,cyan);
            rightGate=CreateGate(owner,true,rightRoute,out rightBarrier);
            forwardGate=CreateGate(owner,false,forwardRoute,out forwardBarrier);
            StartCoroutine(RaiseCube(rightPreview,origin+Vector3.right*12,0));
            StartCoroutine(RaiseCube(forwardPreview,origin+Vector3.forward*12,.15f));
        }
        IEnumerator RaiseCube(Transform cube, Vector3 destination, float delay)
        {
            var tween=WorldTween(cube.DOMove(destination,.85f).SetDelay(delay).SetEase(Ease.OutCubic)).SetLink(cube.gameObject);
            yield return tween.WaitForCompletion();
            if(!cube)yield break;
            cube.position=destination;
            settledPreviews++;
            if(settledPreviews==2)
            {
                routesReady=true;
                if(phase==Phase.Choice)OpenExits();
            }
        }
        IEnumerator RetireCube(Transform cube)
        {
            if(!cube)yield break;
            var sequence=DOTween.Sequence().Join(cube.DOMoveY(cube.position.y-22,1.1f).SetEase(Ease.InQuad))
                .Join(cube.DORotate(cube.eulerAngles+new Vector3(7,0,-5),1.1f).SetEase(Ease.Linear));
            WorldTween(sequence).SetLink(cube.gameObject);
            yield return sequence.WaitForCompletion();
            if(cube)Destroy(cube.gameObject);
        }
        Transform CreateGate(Transform owner, bool right, CubeRouteDefinition route, out Transform barrier)
        {
            var root=new GameObject(right?"Right upgrade gate":"Forward upgrade gate").transform;
            root.SetParent(owner,false);
            root.localPosition=right?new Vector3(4.15f,0,0):new Vector3(0,0,4.15f);
            root.localRotation=Quaternion.Euler(0,right?90:0,0);
            Color color=right?new Color(.09f,.66f,.54f):cyan;
            Shape("Gate left post",PrimitiveType.Cube,new Vector3(-1,1,0),new Vector3(.18f,2,.2f),color,root);
            Shape("Gate right post",PrimitiveType.Cube,new Vector3(1,1,0),new Vector3(.18f,2,.2f),color,root);
            Shape("Gate header",PrimitiveType.Cube,new Vector3(0,2,0),new Vector3(2.2f,.2f,.25f),color,root);
            barrier=Shape("Locked until clear",PrimitiveType.Cube,new Vector3(0,1,0),new Vector3(1.8f,1.8f,.08f),danger,root);
            var label=new GameObject("Upgrade label").AddComponent<TextMesh>();
            label.transform.SetParent(root,false); label.transform.position=root.position+Vector3.up*2.6f;
            label.transform.rotation=view.transform.rotation;
            label.text=RewardLabel(route); label.anchor=TextAnchor.MiddleCenter; label.alignment=TextAlignment.Center;
            label.fontSize=64; label.characterSize=.035f; label.color=Color.white;
            return root;
        }
        string RewardLabel(CubeRouteDefinition route)
        {
            if(!route)return "ROTA";
            return route.reward==RouteReward.Health?$"+{route.value:0} CAN":$"+%{route.value:0} HASAR\n{route.duration} KÜP";
        }
        bool InJoystickArea(Vector2 p) => p.y>Screen.height*.58f && p.x<Screen.width*.55f;
        void ReadInput()
        {
            if(hud)stick=hud.InputValue;
            else if(Input.touchCount>0 || joystickFinger>=0)
            {
                bool found=false;
                for(int i=0;i<Input.touchCount;i++)
                {
                    var touch=Input.GetTouch(i);
                    var point=new Vector2(touch.position.x,Screen.height-touch.position.y);
                    if(joystickFinger<0 && touch.phase==TouchPhase.Began && InJoystickArea(point))
                    { joystickFinger=touch.fingerId; stickOrigin=point; dragging=true; }
                    if(touch.fingerId!=joystickFinger)continue;
                    found=true;
                    if(touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled)
                    { dragging=false; joystickFinger=-1; }
                    stick=dragging?Vector2.ClampMagnitude((point-stickOrigin)/(Screen.width*.12f),1):Vector2.zero;
                }
                if(!found) { dragging=false; joystickFinger=-1; stick=Vector2.zero; }
            }
            else
            {
                var point=new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y);
                if(Input.GetMouseButtonDown(0)&&InJoystickArea(point)) { dragging=true; stickOrigin=point; }
                if(Input.GetMouseButtonUp(0))dragging=false;
                stick=dragging?Vector2.ClampMagnitude((point-stickOrigin)/(Screen.width*.12f),1):Vector2.zero;
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            Vector2 keys=new Vector2(Input.GetAxisRaw("Horizontal"),-Input.GetAxisRaw("Vertical"));
            if(keys.sqrMagnitude>0)stick=Vector2.ClampMagnitude(keys,1);
#endif
            if(Input.GetKeyDown(KeyCode.Escape))SetPaused(!paused);
        }
        void MovePlayer(Vector2 input, float dt)
        {
            var forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
            var right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;
            var direction=right*input.x-forward*input.y;
            if(direction.sqrMagnitude>.01f)player.rotation=Quaternion.LookRotation(direction);
            Vector3 before=player.position;
            player.position+=direction*4.5f*dt;
            if(phase==Phase.Combat && cover && FlatDistance(player.position,cover.position)<1.05f)player.position=before;
            if(phase==Phase.Choice)
            {
                if(!routesReady)
                {
                    var blocked=player.position-centre;
                    if(Mathf.Abs(blocked.z)<1.15f)blocked.x=Mathf.Min(blocked.x,3.85f);
                    if(Mathf.Abs(blocked.x)<1.15f)blocked.z=Mathf.Min(blocked.z,3.85f);
                    player.position=centre+blocked;
                }
                CheckGateCrossing(before,player.position);
                if(phase==Phase.Transfer)return;
                // Cleared cubes are safe while walking to an exit.
                var relative=player.position-centre;
                relative.x=Mathf.Clamp(relative.x,-4.3f,4.3f); relative.z=Mathf.Clamp(relative.z,-4.3f,4.3f);
                player.position=centre+relative;
            }
            else if(stage<CubeCount)
            {
                var relative=player.position-centre;
                if(Mathf.Abs(relative.z)<1.15f && relative.x>3.85f)relative.x=3.85f;
                if(Mathf.Abs(relative.x)<1.15f && relative.z>3.85f)relative.z=3.85f;
                player.position=centre+relative;
            }
            if(phase==Phase.Combat)AimAtNearestEnemy();
        }
        bool AimAtNearestEnemy()
        {
            currentTarget=null;
            float nearest=float.PositiveInfinity;
            foreach(var foe in foes)
            {
                if(!foe.body || foe.hp<=0)continue;
                var offset=foe.body.position-player.position;offset.y=0;
                if(offset.sqrMagnitude<nearest) { nearest=offset.sqrMagnitude;currentTarget=foe; }
            }
            if(currentTarget==null)return false;
            var aim=currentTarget.body.position-player.position;aim.y=0;
            if(aim.sqrMagnitude>.0001f)player.rotation=Quaternion.LookRotation(aim);
            return true;
        }
        void CheckGateCrossing(Vector3 before, Vector3 after)
        {
            if(phase!=Phase.Choice)return;
            Vector3 a=before-centre,b=after-centre;
            if(a.x<4.15f && b.x>=4.15f)
            {
                float z=Mathf.Lerp(a.z,b.z,(4.15f-a.x)/(b.x-a.x));
                if(Mathf.Abs(z)<.8f) { Select(true); return; }
            }
            if(a.z<4.15f && b.z>=4.15f)
            {
                float x=Mathf.Lerp(a.x,b.x,(4.15f-a.z)/(b.z-a.z));
                if(Mathf.Abs(x)<.8f)Select(false);
            }
        }
        void Update()
        {
            ReadInput();
            if(paused) return;
            float dt=Time.deltaTime;
            if(phase==Phase.Combat)
            {
                dt=Mathf.Min(dt,remaining);
                invulnerability-=dt;
                MovePlayer(stick,dt);
                if(Mathf.Abs(player.position.x-centre.x)>Half || Mathf.Abs(player.position.z-centre.z)>Half) { Fail("Kenardan düştün",FailureKind.EdgeFall); return; }
                TickEnemies(dt);
                if(phase!=Phase.Combat)return;
                shot-=dt;
                if(AimAtNearestEnemy() && shot<=0) { shot=FireInterval; FirePulse(); }
                TickPulses(dt);
                TickWorldFeel(dt);
                if(phase!=Phase.Combat)return;
                if(Mathf.Abs(player.position.x-centre.x)>Half || Mathf.Abs(player.position.z-centre.z)>Half) { Fail("Kenardan düştün",FailureKind.EdgeFall); return; }
                // Resolve valid damage before timeout, using only the frame's remaining combat time.
                remaining=Mathf.Max(0,remaining-dt);
                if(ObjectiveComplete) Clear();
                else if(remaining<=0) Fail("20 saniye doldu",FailureKind.Timeout);
            }
            else if(phase==Phase.Choice)
            {
                MovePlayer(stick,dt);
            }

        }
        void FirePulse()
        {
            if(phase!=Phase.Combat || !AimAtNearestEnemy())return;
            if(HasContent) { FireContentWeapon();return; }
            var body=RentProjectile(pulsePool,"Forward pulse",gun.position+player.forward*.35f,cyan,.18f);
            if(playerAnimation)playerAnimation.Shoot();
            feedback.Play(CombatFeedback.Cue.Shot);feedback.Burst(body.position,cyan,.23f,.07f);
            var aim=currentTarget.body.position-body.position;aim.y=0;
            pulses.Add(new Pulse {body=body,direction=aim.sqrMagnitude>.0001f?aim.normalized:player.forward,damage=6*(1+(damageCubes>0?damageBonus:0))});
        }
        void TickPulses(float dt)
        {
            for(int i=pulses.Count-1;i>=0;i--)
            {
                var pulse=pulses[i];
                Vector3 start=pulse.body.position, end=start+pulse.direction*18*dt;
                Foe hit=null; float nearest=float.MaxValue;
                foreach(var foe in foes)
                {
                    Vector3 offset=foe.body.position-start; offset.y=0;
                    Vector3 segment=end-start; segment.y=0;
                    float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector3.Dot(offset,segment)/segment.sqrMagnitude):0;
                    if((offset-segment*t).sqrMagnitude<.55f*.55f && t<nearest) { nearest=t; hit=foe; }
                }
                Barrel hitBarrel=null;
                foreach(var barrel in barrels)
                {
                    Vector3 offset=barrel.body.position-start;offset.y=0;Vector3 segment=end-start;segment.y=0;
                    float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector3.Dot(offset,segment)/segment.sqrMagnitude):0;
                    if((offset-segment*t).sqrMagnitude<.45f*.45f && t<nearest) { nearest=t;hitBarrel=barrel;hit=null; }
                }
                // Player shots ignore walls/cover, just like nearest-enemy target acquisition.
                pulse.life-=dt; pulse.body.position=end;
                if(hitBarrel!=null)HitBarrel(hitBarrel,pulse.damage);
                else if(hit!=null)HitEnemy(hit,pulse.damage,pulse.direction);
                if(hit!=null||hitBarrel!=null||pulse.life<=0)
                { pulse.body.gameObject.SetActive(false);pulsePool.Push(pulse.body);pulses.RemoveAt(i); }
            }
        }
        void ClearPulses()
        {
            foreach(var pulse in pulses)if(pulse.body) { pulse.body.gameObject.SetActive(false);pulsePool.Push(pulse.body); }
            pulses.Clear();
        }
        void Clear()
        {
            ClearPulses();
            FinishEncounterFeel(true);
            wallet+=10+pendingGold;pendingGold=0; PlayerPrefs.SetInt("CI.Wallet",wallet); PlayerPrefs.Save();
            damageCubes=Mathf.Max(0,damageCubes-1);
            if(stage==CubeCount) { phase=Phase.Won; damageCubes=0; return; }
            phase=Phase.Choice;
            if(routesReady)OpenExits();
        }
        void OpenExits()
        {
            if(!rightBarrier || !rightBarrier.gameObject.activeSelf)return;
            rightBarrier.gameObject.SetActive(false); forwardBarrier.gameObject.SetActive(false);
            WorldTween(rightGate.DOPunchScale(Vector3.one*.06f,.3f,3,.4f)).SetLink(rightGate.gameObject);
            WorldTween(forwardGate.DOPunchScale(Vector3.one*.06f,.3f,3,.4f)).SetLink(forwardGate.gameObject);
            Shape("Right bridge",PrimitiveType.Cube,new Vector3(6,-.05f,0),new Vector3(3.1f,.18f,1.8f),cyan,platform);
            Shape("Forward bridge",PrimitiveType.Cube,new Vector3(0,-.05f,6),new Vector3(1.8f,.18f,3.1f),cyan,platform);
        }
        void Select(bool right)
        {
            if(phase!=Phase.Choice || !routesReady) return;
            phase=Phase.Transfer;
            feedback.Play(CombatFeedback.Cue.Gate);
            var route=right?rightRoute:forwardRoute;
            if(route && route.reward==RouteReward.Health)health=Mathf.Min(100,health+route.value);
            if(route && route.reward==RouteReward.Damage)
            { damageCubes=route.duration; damageBonus=Mathf.Clamp(route.value/100,0,.35f); }
            StartCoroutine(Transfer(right));
        }
        IEnumerator Transfer(bool right)
        {
            // Promote the existing candidate. Never rebuild the world, player or camera.
            var chosen=right?rightPreview:forwardPreview;
            var rejected=right?forwardPreview:rightPreview;
            var old=platform;
            StartCoroutine(RetireCube(rejected));
            for(int i=chosen.childCount-1;i>=0;i--)
                if(chosen.GetChild(i).name.StartsWith("Reward"))Destroy(chosen.GetChild(i).gameObject);
            Vector3 destination=chosen.position;
            var start=player.position;
            var end=destination+(right?new Vector3(-3.6f,.65f,0):new Vector3(0,.65f,-3.6f));
            player.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(end-start,Vector3.up));
            float elapsed=0;
            bool prepared=false, retired=false;
            while(elapsed<.9f)
            {
                if(!paused)elapsed+=Time.deltaTime;
                float t=Mathf.Clamp01(elapsed/.9f);
                player.position=Vector3.Lerp(start,end,Mathf.SmoothStep(0,1,t));
                if(t>=.35f && !prepared)
                {
                    prepared=true;
                    if(stage+1<CubeCount)PrepareNextCubes(destination,chosen);
                }
                // Drop the old floor only after the player is safely on the new cube.
                if(t>=.8f && !retired)
                {
                    retired=true;
                    StartCoroutine(RetireCube(old));
                }
                yield return null;
            }
            platform=chosen; centre=destination;
            var route=right?rightRoute:forwardRoute;
            stage++; BeginCombat(route);
        }
        void Fail(string why, FailureKind kind=FailureKind.Health)
        {
            if(phase!=Phase.Combat)return;
            ClearPulses();
            FinishEncounterFeel(false);
            reason=why;
            bool timeout=kind==FailureKind.Timeout;
            bool falling=kind==FailureKind.EdgeFall;
            phase=timeout?Phase.Collapse:falling?Phase.Falling:Phase.Dying;
            StartCoroutine(Collapse(timeout,falling));
        }
        IEnumerator Collapse(bool collapseCube, bool fallPlayer)
        {
            float t=0;
            while(t<(collapseCube||fallPlayer?1.15f:.35f))
            {
                if(!paused)
                {
                    t+=Time.deltaTime;
                    var fall=Vector3.down*Time.deltaTime*(2+t*12);
                    if(collapseCube)platform.position+=fall;
                    if(collapseCube||fallPlayer)player.position+=fall;
                }
                yield return null;
            }
            phase=Phase.Failed; damageCubes=0;
        }
        void OnGUI()
        {
            if(hud)return;
            float scale=Screen.width/480f;
            GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(scale,scale,1));
            float h=Screen.height/scale;
            if(titleStyle==null)
            {
                titleStyle=new GUIStyle(GUI.skin.label){fontSize=30,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
                labelStyle=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter};
                smallStyle=new GUIStyle(labelStyle){fontSize=13};
                buttonStyle=new GUIStyle(GUI.skin.button){fontSize=17,wordWrap=true};
            }
            GUI.Box(new Rect(16,20,448,110),GUIContent.none);
            GUI.Label(new Rect(20,25,440,24),"CUBE INVASION   /   NEON YARD",smallStyle);
            GUI.contentColor=remaining<=5?danger:Color.white;
            GUI.Label(new Rect(20,48,440,42),remaining.ToString("00.0")+" sn",titleStyle); GUI.contentColor=Color.white;
            GUI.Label(new Rect(20,93,440,28),$"KÜP {stage}/3   •   {foes.Count} HEDEF   •   CAN {health:0}   •   ALTIN {wallet}",smallStyle);
            if(GUI.Button(new Rect(380,138,80,35),paused?"DEVAM":"DURAKLAT",smallStyle)) paused=!paused;
            if(damageCubes>0) GUI.Label(new Rect(10,140,280,30),$"HASAR +%{damageBonus*100:0}  /  {damageCubes} küp",smallStyle);
            if(phase==Phase.Choice)
            {
                GUI.Label(new Rect(20,180,440,55),"KÜP TEMİZLENDİ\nGüçlendirme almak için bir kapıdan geç",labelStyle);
            }
            if(phase==Phase.Failed||phase==Phase.Won)
            {
                GUI.Box(new Rect(30,h*.36f,420,185),GUIContent.none);
                GUI.Label(new Rect(30,h*.36f+15,420,45),phase==Phase.Won?"BÖLÜM TAMAMLANDI":"LEVEL FAIL",titleStyle);
                GUI.Label(new Rect(30,h*.36f+65,420,30),phase==Phase.Won?"3 küp temizlendi. Altın bankalandı.":reason,labelStyle);
                if(GUI.Button(new Rect(85,h*.36f+115,310,50),"TEKRAR OYNA",buttonStyle))
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            if(paused) GUI.Label(new Rect(20,h*.25f,440,45),"DURAKLATILDI",titleStyle);
            GUI.Label(new Rect(15,h-60,450,45),"WASD / Ok tuşları veya sol alanda sürükle\nBaktığın yöne otomatik ateş · Kapıdan geç, güçlen",smallStyle);
            DrawJoystick(scale,h);
        }
        void DrawJoystick(float scale,float h)
        {
            if(!joystickDisc)
            {
                joystickDisc=new Texture2D(64,64,TextureFormat.RGBA32,false);
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                    joystickDisc.SetPixel(x,y,new Vector2(x-31.5f,y-31.5f).magnitude<31?Color.white:Color.clear);
                joystickDisc.Apply();
            }
            Vector2 origin=dragging?stickOrigin/scale:new Vector2(100,h-150);
            GUI.color=new Color(.05f,.12f,.22f,.7f);
            GUI.DrawTexture(new Rect(origin.x-58,origin.y-58,116,116),joystickDisc);
            GUI.color=cyan;
            Vector2 knob=origin+stick*44;
            GUI.DrawTexture(new Rect(knob.x-23,knob.y-23,46,46),joystickDisc);
            GUI.color=Color.white;
        }
        void OnApplicationFocus(bool focused) { if(!focused)SetPaused(true); }
        void OnDestroy() { ClearAnimationTweens();foreach(var mat in materials) if(mat) Destroy(mat); if(joystickDisc)Destroy(joystickDisc); }
    }
}
