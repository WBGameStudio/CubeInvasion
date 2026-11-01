using UnityEngine;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        // Impact pacing: a few frames of hit-stop plus a camera kick sell kills, hurt and explosions.
        // The freeze slows the whole simulation (combat clock included), so it never costs the player time.
        const float HitStopScale=.05f, KillStop=.045f, HurtStop=.085f, BlastStop=.07f;
        // Exponential easing rates (per second): ~90% of top speed in 0.19 s, stop in 0.14 s, turns settle in ~0.15 s.
        const float MoveEase=12, StopEase=16, TurnEase=15, MinTurnRate=240;
        const string HeroKey="Rogue cube";
        float hitStopUntil;
        Vector3 moveVelocity;
        Quaternion aimFacing=Quaternion.identity;
        ArcadeFollowCamera followCamera;
        Vector3 transferFocus;

        void Impact(float stop,float kick,Vector3 push)
        {
            if(!followCamera&&view)followCamera=view.GetComponent<ArcadeFollowCamera>();
            if(followCamera)followCamera.Kick(kick,push);
            // Batch smoke checks step Update directly and assert exact clocks; the freeze is presentation only.
            if(stop<=0||paused||Application.isBatchMode)return;
            hitStopUntil=Mathf.Max(hitStopUntil,Time.unscaledTime+stop);Time.timeScale=HitStopScale;
        }
        void FrameArena()
        {
            if(!followCamera&&view)followCamera=view.GetComponent<ArcadeFollowCamera>();
            // During a transfer the view already leans toward the cube being walked onto.
            if(followCamera)followCamera.SetArenaFocus(phase==Phase.Transfer?transferFocus:centre,phase==Phase.Combat||phase==Phase.Choice||phase==Phase.Transfer);
        }
        void TickHitStop()
        {
            if(Time.timeScale<1&&(paused||phase!=Phase.Combat||Time.unscaledTime>=hitStopUntil))Time.timeScale=1;
        }
        // Stick response: a small dead zone, the inner quarter kept for slow focus walking, then full speed
        // from ~65% thumb travel instead of only at the rim.
        const float StickDeadZone=.06f, StickFullSpeed=.65f;
        static float MoveResponse(float magnitude)
        {
            if(magnitude<StickDeadZone)return 0;
            const float focus=.25f;
            return magnitude<focus?magnitude:Mathf.Lerp(focus,1,Mathf.InverseLerp(focus,StickFullSpeed,magnitude));
        }
        void KillImpact(Vector3 at,Vector3 direction)
        {
            feedback.Shatter(at+Vector3.up*.35f,VoidToyPalette.Graphite,VoidToyPalette.Danger,direction,7);
            feedback.Burst(at+Vector3.up*.4f,Color.white,.6f,.07f);
            Impact(KillStop,.32f,direction);
        }
        void HurtImpact(Vector3 push)
        {
            if(hud)hud.FlashHurt();
            Impact(HurtStop,.75f,push);
            Face(FaceSurprised,.4f,3);
        }

        // KIP's screen-face: a short event expression wins by priority, then it settles back to the
        // resting face (Pain below a quarter of health, otherwise Neutral).
        const string FaceNeutral="Neutral",FaceDetermined="Determined",FaceSurprised="Surprised",FacePain="Pain",FaceHappy="Happy",FaceDizzy="Dizzy";
        ToyArtLibrary heroFaces;string shownFace;float faceHold;int facePriority;
        void HeroShot()
        {
            if(playerAnimation)playerAnimation.Shoot();
            Face(FaceDetermined,.3f,1);
        }
        void Face(string expression,float hold,int priority)
        {
            if(faceHold>0&&priority<facePriority)return;
            faceHold=hold;facePriority=priority;ShowFace(expression);
        }
        void TickHeroFace(float dt)
        {
            if(!playerAnimation)return;
            faceHold=Mathf.Max(0,faceHold-dt);
            if(faceHold>0)return;
            facePriority=0;ShowFace(phase==Phase.Combat&&health<MaxHealth*.25f?FacePain:FaceNeutral);
        }
        void ShowFace(string expression)
        {
            if(shownFace==expression||!playerAnimation)return;
            if(!heroFaces)heroFaces=Resources.Load<ToyArtLibrary>("RogueCubeFaces");
            if(!heroFaces||!heroFaces.TryGet(HeroKey+"/"+expression,out var face))return;
            Mesh Part(string name) {foreach(var part in face.rig)if(part.name==name)return part.mesh;return null;}
            playerAnimation.SetFace(Part("LeftEye"),Part("RightEye"),Part("Mouth"));shownFace=expression;
        }
    }
}
