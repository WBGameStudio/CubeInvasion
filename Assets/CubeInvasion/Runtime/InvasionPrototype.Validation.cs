using System;
using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("P4 / first-run guidance")]
        [SerializeField] Text onboardingHint;
        P4ValidationLog validationLog;
        string validationRun;
        float cubeDamageTaken;
        Vector3 tutorialOrigin;
        bool tutorialMoved;
        const string TutorialKey="CI.Onboarding.v1";
        string tutorialPreferenceKey=TutorialKey;
        public void ConfigureOnboarding(Text label) { onboardingHint=label; }
        void EnsureValidationLog()
        {
            if(HasMeta&&validationLog==null)validationLog=new P4ValidationLog(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ProfilePath),"ci_playtest_v1.json"));
        }
        void RecordEconomy(string name,string detail,int cost)
        {
            if(cost<=0)return;
            EnsureValidationLog();validationLog?.Record(new P4ValidationLog.Event {name=name,weapon=inspectedWeapon,detail=detail,coins=-cost});
        }
        void BeginValidationRun(int index)
        {
            if(!HasMeta)return;
#if UNITY_EDITOR
            tutorialPreferenceKey=string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUBE_INVASION_META_TEST_PATH"))?TutorialKey:TutorialKey+".test";
#endif
            // Test profiles keep diagnostics beside their isolated save, never in the player's folder.
            EnsureValidationLog();
            validationRun=runId??Guid.NewGuid().ToString("N");
            validationLog.Record(new P4ValidationLog.Event {name="level_started",run=validationRun,mission=index,weapon=weaponIndex});
            PlaygamaService.Level("level_started",index);
#if DEVELOPMENT_BUILD && !UNITY_EDITOR && !UNITY_WEBGL
            if(!GetComponent<P1PerformanceProbe>())
                gameObject.AddComponent<P1PerformanceProbe>().Capture(300,System.IO.Path.Combine(Application.persistentDataPath,"ci_p4_device_profile.json"),()=>phase==Phase.Combat&&!paused);
#endif
            tutorialMoved=false;tutorialOrigin=new Vector3(0,.65f,-2.5f);
        }
        void BeginValidationCube()
        {
            cubeDamageTaken=0;RecordValidation("cube_started");
        }
        // Gameplay milestones forwarded to Bridge analytics (no-op off the web).
        static readonly System.Collections.Generic.HashSet<string> AnalyticsEvents=new System.Collections.Generic.HashSet<string>
            {"cube_cleared","cube_failed","route_selected","goldrush_completed","boss_phase_failed","tutorial_moved"};
        void RecordValidation(string name,string detail=null,int coins=0)
        {
            if(name=="level_completed")PlaygamaService.Level(name,missionIndex);
            else if(AnalyticsEvents.Contains(name))
                PlaygamaService.Analytics(name,new System.Collections.Generic.Dictionary<string,object> {{"mission",missionIndex+1},{"cube",stage},{"detail",detail??""},{"gold",coins}});
            if(validationLog==null)return;
            validationLog.Record(new P4ValidationLog.Event {name=name,run=validationRun,mission=missionIndex,cube=stage,weapon=weaponIndex,
                detail=detail,coins=coins,enemiesLeft=foes.Count,timeRemaining=remaining,hp=health,damageTaken=cubeDamageTaken,seconds=runSeconds});
            if(name=="level_completed"&&missionIndex==0&&GamePrefs.GetInt(tutorialPreferenceKey,0)==0)
            { GamePrefs.SetInt(tutorialPreferenceKey,1);GamePrefs.Save(); }
        }
        void TickOnboarding()
        {
            if(!onboardingHint)return;
            string message="";
            if(HasMeta&&!paused&&GamePrefs.GetInt(tutorialPreferenceKey,0)==0&&missionIndex==0)
            {
                if(phase==Phase.Combat)
                {
                    if(!tutorialMoved&&Vector3.Distance(player.position,tutorialOrigin)>.6f)
                    { tutorialMoved=true;RecordValidation("tutorial_moved"); }
                    float elapsed=20-remaining;
                    // Web players may be on a keyboard; the joystick also works with a mouse drag.
                    if(startHold)message=""; // The large start prompt says it.
                    else if(stage==1&&!tutorialMoved)message=Application.isMobilePlatform?"DRAG ANYWHERE TO MOVE":"MOVE: WASD / ARROWS OR DRAG";
                    else if(stage==1&&elapsed<9)message="YOUR GUN FIRES BY ITSELF · KEEP MOVING";
                    else if(stage==2&&elapsed<6)message="DOWN THEM ALL IN 20 S · STAY OFF THE EDGE";
                }
                // The cleared header already says this on the web layout, where the hint would sit on the route cards.
                else if(phase==Phase.Choice&&!PlaygamaService.IsBridge)message="WALK THROUGH A GATE TO PICK ITS POWER-UP";
                else if(phase==Phase.Transfer)message="NEW CUBE, SAME ADVENTURE · GET READY";
            }
            if(HasMeta&&!paused&&phase==Phase.Combat&&stage==1&&remaining>14&&missionIndex>0)
            {
                switch(missionIndex)
                {
                    case 1:message="COVER BLOCKS SHOTS BOTH WAYS · CHANGE ANGLE";break;
                    case 2:message="CUBE GOLD IS BANKED · UPGRADE YOUR WEAPON IN THE SHOP";break;
                    case 3:message="BLOW UP BARRELS NEAR ENEMIES";break;
                    case 4:message="LEAVE THE RED CONE · PUSHER IS WINDING UP";break;
                    case 5:message="COLLECT GOLD · FILL THE QUOTA BEFORE TIME RUNS OUT";break;
                    case 6:message="DODGE THE RED MARKERS · BOSS HAS 3 PHASES";break;
                }
            }
            if(phase==Phase.Combat&&Time.unscaledTime<extraTimeUntil)message="EXTRA TIME · FINISH THEM";
            if((HasCrusherObjectives||(HasGoldRushVariants&&GoldVariant!=GoldRushVariant.CoinSprint))&&phase==Phase.Combat)message=""; // The existing objective pill carries the phase instruction.
            if(onboardingHint.text!=message)onboardingHint.text=message;
            if(onboardingHint.gameObject.activeSelf!=(message.Length>0))onboardingHint.gameObject.SetActive(message.Length>0);
        }
    }
}
