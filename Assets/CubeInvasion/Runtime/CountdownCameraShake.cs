using Cinemachine;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Cube Invasion/Countdown Camera Shake")]
    public sealed class CountdownCameraShake : CinemachineExtension
    {
        [SerializeField,Range(0,.15f)] float maximumDisplacement=.075f;
        [SerializeField,Range(1,20)] float startingFrequency=7;
        [SerializeField,Range(1,25)] float endingFrequency=15;
        float noisePhase,beatPhase,engage;
        bool running;
        public Vector3 CurrentOffset {get;private set;}
        public float Strength {get;private set;}
        public float Pulse {get;private set;}
        public float MaximumDisplacement=>maximumDisplacement;
        public bool Drive(float seconds,bool active,float dt)
        {
            if(!enabled||!active||seconds<=0||seconds>=5) {ResetUrgency();return false;}
            bool first=!running;running=true;
            float pressure=Mathf.Clamp01((5-seconds)/5);
            dt=Mathf.Max(0,dt);engage=Mathf.MoveTowards(engage,1,dt*4);
            Strength=Mathf.Lerp(.12f,1,pressure*pressure)*engage;
            noisePhase+=dt*Mathf.Lerp(startingFrequency,endingFrequency,pressure);
            float oldBeat=beatPhase;beatPhase+=dt*Mathf.Lerp(1.25f,2.8f,pressure);
            Pulse=Mathf.Pow((1+Mathf.Cos(beatPhase*Mathf.PI*2))*.5f,5);
            float phase=noisePhase*Mathf.PI*2;
            var noise=new Vector3(Mathf.Sin(phase*1.13f)*.6f+Mathf.Sin(phase*1.71f+.8f)*.4f,
                (Mathf.Sin(phase*.91f+1.6f)*.65f+Mathf.Sin(phase*1.43f)*.35f)*.65f,0);
            CurrentOffset=Vector3.ClampMagnitude(noise,1)*maximumDisplacement*Strength*(.7f+.3f*Pulse);
            return dt>0&&(first||Mathf.FloorToInt(beatPhase)>Mathf.FloorToInt(oldBeat));
        }
        public void ResetUrgency()
        {running=false;engage=noisePhase=beatPhase=Strength=Pulse=0;CurrentOffset=Vector3.zero;}
        void OnDisable() {ResetUrgency();}
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,CinemachineCore.Stage stage,ref CameraState state,float deltaTime)
        {
            if(stage!=CinemachineCore.Stage.Noise||!running||deltaTime<0)return;
            // Only the final camera position changes. Heading, follow anchor and gameplay input stay stable.
            state.PositionCorrection+=state.RawOrientation*CurrentOffset;
        }
    }
}
