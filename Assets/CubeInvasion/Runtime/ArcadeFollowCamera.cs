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
            anchor.position=new Vector3(target.position.x,0,target.position.z)+forward*forwardFraming;
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
