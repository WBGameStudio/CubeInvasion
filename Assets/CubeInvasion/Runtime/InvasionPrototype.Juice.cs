using UnityEngine;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        // Impact pacing: a few frames of hit-stop plus a camera kick sell kills, hurt and explosions.
        // The freeze slows the whole simulation (combat clock included), so it never costs the player time.
        const float HitStopScale=.05f, KillStop=.045f, HurtStop=.085f, BlastStop=.07f;
        const float MoveAcceleration=55, MoveDeceleration=70, TurnRate=900;
        const string HeroKey="Rogue cube";
        float hitStopUntil;
        Vector3 moveVelocity;
        Quaternion aimFacing=Quaternion.identity;
        ArcadeFollowCamera followCamera;

        void Impact(float stop,float kick,Vector3 push)
        {
            if(!followCamera&&view)followCamera=view.GetComponent<ArcadeFollowCamera>();
            if(followCamera)followCamera.Kick(kick,push);
            // Batch smoke checks step Update directly and assert exact clocks; the freeze is presentation only.
            if(stop<=0||paused||Application.isBatchMode)return;
            hitStopUntil=Mathf.Max(hitStopUntil,Time.unscaledTime+stop);Time.timeScale=HitStopScale;
        }
        void TickHitStop()
        {
            if(Time.timeScale<1&&(paused||phase!=Phase.Combat||Time.unscaledTime>=hitStopUntil))Time.timeScale=1;
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

        // KİP's screen-face: a short event expression wins by priority, then it settles back to the
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
