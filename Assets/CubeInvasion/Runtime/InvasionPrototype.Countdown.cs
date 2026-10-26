using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        ArcadeFollowCamera countdownCamera;
        void TickCountdownPresentation(float dt)
        {
            if(!countdownCamera&&view)countdownCamera=view.GetComponent<ArcadeFollowCamera>();
            if(!countdownCamera)return;
            bool active=!paused&&phase==Phase.Combat&&remaining>0&&remaining<5;
            bool beat=countdownCamera.DriveCountdown(remaining,active,dt);
            TintArenaEdges(active?remaining<=3?VoidToyPalette.Danger:VoidToyPalette.Warm:VoidToyPalette.BiomeEdge(ArenaBiome));
            if(beat&&feedback)feedback.Play(CombatFeedback.Cue.Alarm);
        }
        Transform tintedArena;Color arenaEdge;MaterialPropertyBlock edgeTint;
        // The cube's front edge light carries the timer state: cyan, orange at 5 s, pink at 3 s.
        void TintArenaEdges(Color color)
        {
            if(!platform)return;
            if(tintedArena==platform&&arenaEdge==color)return;
            tintedArena=platform;arenaEdge=color;edgeTint??=new MaterialPropertyBlock();
            foreach(var renderer in platform.GetComponentsInChildren<Renderer>())
            {
                if(renderer.gameObject.name!="Edge light")continue;
                renderer.GetPropertyBlock(edgeTint);edgeTint.SetColor("_Color",color);edgeTint.SetColor("_EmissionColor",color*.6f);renderer.SetPropertyBlock(edgeTint);
            }
        }
        void ResetCountdownPresentation()
        {
            if(!countdownCamera&&view)countdownCamera=view.GetComponent<ArcadeFollowCamera>();
            if(countdownCamera)countdownCamera.DriveCountdown(20,false,0);
            if(hud)hud.SetCountdownPressure(0,0);
        }
        void RefreshCountdownHud()
        {
            if(paused||phase!=Phase.Combat||remaining>=5||remaining<=0)ResetCountdownPresentation();
            if(hud&&countdownCamera)hud.SetCountdownPressure(countdownCamera.CountdownStrength,countdownCamera.CountdownPulse);
        }
    }
}
