using Cinemachine;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    // The target proxy updates after player movement, before the Brain's LateUpdate.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Camera), typeof(CinemachineBrain))]
    public sealed class ArcadeFollowCamera : MonoBehaviour
    {
        [SerializeField, Range(30, 60)] float tilt = 45;
        [SerializeField, Range(-180, 180)] float heading = 45;
        [SerializeField, Min(0)] float followDamping = .18f;
        [SerializeField, Min(1)] float minimumSize = 6.8f;
        [SerializeField, Min(1)] float horizontalHalfView = 5.2f;
        [SerializeField] float forwardFraming = 1;
        [Header("Arena framing and lookahead")]
        [Tooltip("How far the view leans from the hero toward the active cube's centre, so edges stay in frame.")]
        [SerializeField, Range(0, 1)] float arenaPull = .45f;
        [Tooltip("Metres the view leads in the direction the hero is running.")]
        [SerializeField, Min(0)] float lookAhead = 1.3f;
        [SerializeField, Min(.01f)] float lookAheadTime = .35f;
        [Tooltip("Seconds the arena lean takes to settle when the focus moves (e.g. walking onto a new cube).")]
        [SerializeField, Min(.01f)] float arenaShiftTime = .55f;
        Vector3 arenaFocus,lookOffset,lookVelocity,lastTarget,easedFocus,arenaVelocity;bool hasArena,hasLast,easedArena;
        // Called every frame by the game with the active cube's centre; off during menus and falls.
        public void SetArenaFocus(Vector3 centre,bool active) {arenaFocus=centre;hasArena=active;}
        [SerializeField] Transform target, anchor;
        bool ownsAnchor,ownsCamera;
        Camera view;
        [SerializeField] CinemachineVirtualCamera virtualCamera;
        CinemachineTransposer transposer;
        [SerializeField] CountdownCameraShake countdownShake;
        public float CountdownStrength=>countdownShake?countdownShake.Strength:0;
        public float CountdownPulse=>countdownShake?countdownShake.Pulse:0;
        public void ConfigureCountdown(CountdownCameraShake effect) {countdownShake=effect;}
        public bool DriveCountdown(float seconds,bool active,float dt)=>countdownShake&&countdownShake.Drive(seconds,active,dt);
        ImpactCameraKick impactKick;
        public void Kick(float strength,Vector3 push) {if(impactKick)impactKick.Kick(strength,push);}
        public void ClearKick() {if(impactKick)impactKick.Clear();}

        public void Follow(Transform followTarget)
        {
            target=followTarget;
            view=GetComponent<Camera>();
            view.orthographic=true;
            var brain=GetComponent<CinemachineBrain>();
            brain.m_UpdateMethod=CinemachineBrain.UpdateMethod.LateUpdate;
            brain.m_BlendUpdateMethod=CinemachineBrain.BrainUpdateMethod.LateUpdate;
            brain.m_DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut,0);
            if(!anchor)
            {
                ownsAnchor=Application.isPlaying;
                anchor=new GameObject("Camera ground follow anchor").transform;
                anchor.SetParent(transform.parent,false);
            }
            if(!virtualCamera)
            {
                ownsCamera=Application.isPlaying;
                var cameraObject=new GameObject("Arcade Cinemachine Virtual Camera");
                cameraObject.transform.SetParent(transform.parent,false);
                virtualCamera=cameraObject.AddComponent<CinemachineVirtualCamera>();
                virtualCamera.Priority=20;
                transposer=virtualCamera.AddCinemachineComponent<CinemachineTransposer>();
                transposer.m_BindingMode=CinemachineTransposer.BindingMode.WorldSpace;
            }
            transposer=virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if(!countdownShake)countdownShake=virtualCamera.GetComponent<CountdownCameraShake>();
            if(!countdownShake)countdownShake=virtualCamera.gameObject.AddComponent<CountdownCameraShake>();
            countdownShake.ResetUrgency();
            impactKick=virtualCamera.GetComponent<ImpactCameraKick>();
            if(!impactKick)impactKick=virtualCamera.gameObject.AddComponent<ImpactCameraKick>();
            impactKick.Clear();
            virtualCamera.Follow=anchor;
            virtualCamera.LookAt=null; // Preserve the diagonal heading when the robot turns.
            UpdateRig();
            virtualCamera.PreviousStateIsValid=false;
            virtualCamera.UpdateCameraState(Vector3.up,-1);
            brain.ManualUpdate(); // Start in the correct pose without a first-frame fly-in.
        }
        void Update() { if(target && virtualCamera) UpdateRig(); }
        void UpdateRig()
        {
            var rotation=Quaternion.Euler(tilt,heading,0);
            var forward=Vector3.ProjectOnPlane(rotation*Vector3.forward,Vector3.up).normalized;
            // Ground anchor keeps a fall visible instead of following into the void.
            var ground=new Vector3(target.position.x,0,target.position.z);
            // Lead toward where the hero is heading, eased so starts and stops never jerk the view.
            float dt=Time.deltaTime;
            if(Application.isPlaying&&hasLast&&dt>0)
            {
                // Only true teleports (a restart, a scripted reset) are ignored; cube transfers peak near 20 m/s.
                var velocity=(ground-lastTarget)/dt;if(velocity.sqrMagnitude>3600)velocity=Vector3.zero;
                var lead=Vector3.ClampMagnitude(velocity/5f,1)*lookAhead;
                lookOffset=Vector3.SmoothDamp(lookOffset,lead,ref lookVelocity,lookAheadTime,Mathf.Infinity,dt);
            }
            else if(!Application.isPlaying) {lookOffset=lookVelocity=Vector3.zero;}
            lastTarget=ground;hasLast=true;
            // The cube centre the view leans toward is eased on its own, so a new focus (next cube) glides in
            // instead of shifting the whole view in one frame; ordinary movement still follows immediately.
            var focus=new Vector3(arenaFocus.x,0,arenaFocus.z);
            if(!hasArena||!Application.isPlaying||!easedArena) {easedFocus=focus;arenaVelocity=Vector3.zero;easedArena=hasArena;}
            else if(dt>0)easedFocus=Vector3.SmoothDamp(easedFocus,focus,ref arenaVelocity,arenaShiftTime,Mathf.Infinity,dt);
            var lean=hasArena?(easedFocus-ground)*arenaPull:Vector3.zero;
            anchor.position=ground+lean+lookOffset+forward*forwardFraming;
            virtualCamera.transform.rotation=rotation;
            transposer.m_FollowOffset=-(rotation*Vector3.forward)*20;
            transposer.m_XDamping=followDamping;
            transposer.m_YDamping=followDamping;
            transposer.m_ZDamping=followDamping;
            virtualCamera.m_Lens.ModeOverride=LensSettings.OverrideModes.Orthographic;
            virtualCamera.m_Lens.OrthographicSize=Mathf.Max(minimumSize,horizontalHalfView/Mathf.Max(.4f,view.aspect));
        }
        void OnDestroy()
        {
            if(ownsCamera && virtualCamera)Destroy(virtualCamera.gameObject);
            if(ownsAnchor && anchor)Destroy(anchor.gameObject);
        }
    }
}
