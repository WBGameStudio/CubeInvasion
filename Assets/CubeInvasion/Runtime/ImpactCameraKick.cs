using Cinemachine;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    // Short, decaying camera kick for impacts (kills, hurt, explosions). Separate from the countdown shake
    // so that effect keeps its own zero-offset guarantees; runs on unscaled time so a hit-stop holds the pose.
    [DisallowMultipleComponent]
    [AddComponentMenu("Cube Invasion/Impact Camera Kick")]
    public sealed class ImpactCameraKick : CinemachineExtension
    {
        [SerializeField,Range(0,.6f)] float maximumDisplacement=.32f;
        [SerializeField,Range(4,40)] float frequency=26;
        [SerializeField,Range(2,30)] float damping=13;
        Vector3 direction;float amplitude,started;
        public Vector3 CurrentOffset {get;private set;}
        // Strength 0..1; direction is a world-space push, flattened onto the screen plane.
        public void Kick(float strength,Vector3 push)
        {
            if(!enabled)return;
            float now=Time.unscaledTime,left=Amplitude(now);
            if(strength<left)return;
            amplitude=Mathf.Clamp01(strength);started=now;
            direction=push.sqrMagnitude>.0001f?push.normalized:Random.insideUnitSphere.normalized;
        }
        public void Clear() {amplitude=0;CurrentOffset=Vector3.zero;}
        float Amplitude(float now)=>amplitude*Mathf.Exp(-(now-started)*damping);
        void OnDisable() {Clear();}
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,CinemachineCore.Stage stage,ref CameraState state,float deltaTime)
        {
            if(stage!=CinemachineCore.Stage.Noise||amplitude<=0)return;
            float now=Time.unscaledTime,a=Amplitude(now);
            if(a<.01f) {Clear();return;}
            var local=Quaternion.Inverse(state.RawOrientation)*direction;local.z=0;
            if(local.sqrMagnitude<.0001f)local=Vector3.up;
            CurrentOffset=local.normalized*Mathf.Cos((now-started)*frequency)*a*maximumDisplacement;
            state.PositionCorrection+=state.RawOrientation*CurrentOffset;
        }
    }
}
