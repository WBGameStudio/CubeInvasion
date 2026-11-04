using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    // Web-platform glue: lifecycle, external pause, the first-session flow and the chosen ad placements.
    // Interstitials sit on result-screen buttons, never inside a 20 s cube; device builds run the same flow with NoAdService.
    public sealed partial class InvasionPrototype
    {
        const int FreeRetries=3;
        const string StartPromptNew="PRESS WASD / ARROWS OR DRAG TO MOVE\nYOUR GUN FIRES BY ITSELF",StartPromptTouch="DRAG ANYWHERE TO MOVE\nYOUR GUN FIRES BY ITSELF",StartPromptBack="MOVE TO START";
        bool adFlow,holdNextStart,startHold,revivedThisRun,trialActive,firstKillSent,firstInputSent,keyboardMoving;
        int retriesWithoutAd,pendingTrial=-1,gameReadyFrames;
        float adRequestedAt,extraTimeUntil,hiddenSentAt=-100;
        string pendingFailWhy;FailureKind pendingFailKind;
        Phase trackedPhase=Phase.SaveBlocked;
        PlatformOverlays overlays;

        // Automated editor checks drive the authored flow directly; CUBE_INVASION_LIVE_FLOW=1 turns this flow on there.
        static bool LiveFlow
        {
            get
            {
#if UNITY_EDITOR
                return Environment.GetEnvironmentVariable("CUBE_INVASION_LIVE_FLOW")=="1";
#else
                return true;
#endif
            }
        }
        // First mission, not finished yet: forgiving timer, a small hand-picked wave and no failure ads.
        bool OnboardingRun=>LiveFlow&&HasMeta&&missionIndex==0&&GamePrefs.GetInt(tutorialPreferenceKey,0)==0;
        static ContentEnemy[] OnboardingWave(int cube)
        {
            var g=ContentEnemy.Grunt;var s=ContentEnemy.Sprinter;var a=ContentEnemy.Gunner;
            return cube<=1?new[]{g,g}:cube==2?new[]{g,g,s}:new[]{g,s,a};
        }

        void BeginPlatformSession()
        {
            PlaygamaService.ExternalPauseChanged+=OnExternalPause;
            gameReadyFrames=2;
        }
        void EndPlatformSession() { PlaygamaService.ExternalPauseChanged-=OnExternalPause; }
        // The first frame must be on screen and accepting input, not just built. Counted in Update rather than a
        // coroutine: StartMission stops every coroutine, and the first-session flow starts a mission right away.
        void TickGameReady()
        {
            if(gameReadyFrames<=0||--gameReadyFrames>0)return;
            PlaygamaService.SendGameReady();
        }
        // Players who have not finished mission 1 skip the menu and land in it, waiting for their first move.
        void BeginPlayerSession()
        {
            if(!HasMeta||profile==null)return;
            overlays=PlatformOverlays.Create(hud.SafeRoot);
            bool unfinished=!profile.Data.completed[0];
            Track("session_start",new Dictionary<string,object> {{"new_player",unfinished?1:0},{"highest_mission",profile.Data.highestMission+1}});
            if(!LiveFlow)return;
            ClaimDailyReward();
            if(!unfinished||!profile.CanWrite||retrySave!=null||phase!=Phase.Loadout)return;
            holdNextStart=true;
            StartMission(0);
        }
        void ClaimDailyReward()
        {
            int reward=profile.ClaimDaily((int)(DateTime.UtcNow-new DateTime(2020,1,1,0,0,0,DateTimeKind.Utc)).TotalDays);
            if(reward<=0)return;
            wallet=profile.Data.gold;metaNotice=$"Daily reward: +{VoidToyPalette.FormatGold(reward)} gold · day {profile.Data.dailyStreak}";
            Track("daily_reward",new Dictionary<string,object> {{"gold",reward},{"streak",profile.Data.dailyStreak}});
            RefreshContentHud();
        }
        void TogglePauseKey()=>SetPaused(!paused);
        void OnExternalPause(bool external)
        {
            // Tab hidden or a full-screen ad: freeze any live cube; the player resumes from the pause menu.
            // A cube still waiting for the first move is already frozen, and a pause screen would hide its prompt.
            if(external&&!startHold&&(phase==Phase.Combat||phase==Phase.Choice||phase==Phase.Transfer||phase==Phase.Collapse||phase==Phase.Falling||phase==Phase.Dying))SetPaused(true);
            if(external&&!PlaygamaService.AdOpen)TrackHidden();
        }
        void TickPlatformFlow()
        {
            if(!overlays)return;
            overlays.SetWait(adFlow&&!PlaygamaService.AdOpen&&Time.realtimeSinceStartup-adRequestedAt>.25f);
            overlays.SetPrompt(startHold&&phase==Phase.Combat&&!paused?StartPromptText:null);
            if(phase!=trackedPhase)
            {
                trackedPhase=phase;
                if(phase==Phase.Won||phase==Phase.Failed)
                {
                    Track("result_shown",new Dictionary<string,object> {{"won",phase==Phase.Won?1:0},{"kills",runKills},{"seconds",Mathf.RoundToInt(runSeconds)}});
                    if(DoubleGoldOffered)Track("rewarded_offer_shown",new Dictionary<string,object> {{"placement",AdManager.DoubleGold}});
                }
            }
            if(LiveFlow)
            {
                PlatformBanner.Set(phase==Phase.Loadout&&!adFlow&&!paused&&profile!=null&&profile.Data.completed[0]);
                hud.SetBottomInset(phase==Phase.Loadout?PlatformBanner.Inset:0);
            }
        }
        string StartPromptText=>OnboardingRun?(Application.isMobilePlatform?StartPromptTouch:StartPromptNew):StartPromptBack;
        void ReleaseStartHold()
        {
            startHold=false;
            if(firstInputSent)return;firstInputSent=true;
            Track("first_input",new Dictionary<string,object> {{"seconds_since_start",Mathf.RoundToInt(Time.realtimeSinceStartup)}});
        }
        void NoteFirstKill()
        {
            if(firstKillSent)return;firstKillSent=true;
            Track("first_kill",new Dictionary<string,object> {{"seconds_since_start",Mathf.RoundToInt(Time.realtimeSinceStartup)}});
        }
        // Best effort: the browser may close the page before a hidden-tab event can be sent.
        void TrackHidden()
        {
            if(Time.realtimeSinceStartup-hiddenSentAt<5)return;hiddenSentAt=Time.realtimeSinceStartup;
            Track("session_hidden",new Dictionary<string,object> {{"cube",stage},{"ad_flow",adFlow?1:0},{"seconds_since_start",Mathf.RoundToInt(Time.realtimeSinceStartup)}});
        }
        void Track(string name,Dictionary<string,object> data=null)
        {
            data=data??new Dictionary<string,object>();
            data["mission"]=missionIndex+1;data["phase"]=phase.ToString();
            PlaygamaService.Analytics(name,data);
        }

        // Result-screen actions: ask Bridge for an interstitial, then continue whether or not one showed.
        void AfterBreakAd(string placement,Action next)
        {
            if(adFlow)return;
            adFlow=true;adRequestedAt=Time.realtimeSinceStartup;float asked=adRequestedAt;
            Track("ad_requested",new Dictionary<string,object> {{"placement",placement}});
            StartCoroutine(AdManager.Service.Interstitial(placement,()=>
            {
                adFlow=false;bool shown=PlaygamaService.AdOpenedSince(asked);
                Track("ad_result",new Dictionary<string,object> {{"placement",placement},{"shown",shown?1:0}});
                // Coming back from an ad, the next cube waits for the player instead of already counting down.
                if(shown)holdNextStart=true;
                next();
            }));
        }
        void ResultRetry()
        {
            // "One more try" is the moment players are most willing to stay: the first few in a session stay ad-free.
            if(phase==Phase.Failed&&LiveFlow&&!adFlow&&(OnboardingRun||retriesWithoutAd<FreeRetries))
            {
                if(!OnboardingRun)retriesWithoutAd++;
                StartMission(missionIndex);return;
            }
            AfterBreakAd(phase==Phase.Won?AdManager.LevelCompleted:AdManager.LevelFailed,()=>StartMission(missionIndex));
        }
        void ResultAdvance()=>AfterBreakAd(AdManager.LevelCompleted,()=>StartMission(missionIndex+1));
        void ResultMenu()=>AfterBreakAd(AdManager.MainMenu,OpenMissionMenu);

        // Rewarded: double the run's banked gold, on wins and on fails that banked something.
        bool DoubleGoldOffered=>HasMeta&&profile!=null&&(phase==Phase.Won||phase==Phase.Failed)&&!adFlow&&AdManager.Service.RewardedSupported&&profile.CanDoubleRun(runId);
        void WatchDoubleGold()
        {
            if(!DoubleGoldOffered)return;
            string run=runId;bool paid=false;
            BeginRewarded(AdManager.DoubleGold);
            StartCoroutine(AdManager.Service.Rewarded(AdManager.DoubleGold,()=>
            {
                if(paid)return;paid=true;
                GrantDoubleGold(run);
            },()=> { adFlow=false;RefreshContentHud(); }));
        }
        void BeginRewarded(string placement)
        {
            adFlow=true;adRequestedAt=Time.realtimeSinceStartup;
            Track("rewarded_clicked",new Dictionary<string,object> {{"placement",placement}});
        }
        void GrantDoubleGold(string run)
        {
            Phase resultPhase=phase;
            int before=profile.Data.gold;
            if(!profile.DoubleRun(run))
            {
                // Only a failed write lands here; the save screen retries the same idempotent grant.
                if(profile.CanDoubleRun(run))BlockSave(()=> { phase=resultPhase;GrantDoubleGold(run); });
                return;
            }
            wallet=profile.Data.gold;metaNotice=$"+{VoidToyPalette.FormatGold(wallet-before)} bonus gold";
            feedback.Play(CombatFeedback.Cue.Gold);
            PlaygamaService.Analytics("rewarded_double_gold",new Dictionary<string,object> {{"mission",missionIndex+1},{"gold",wallet-before}});
            RefreshContentHud();
        }

        // Rewarded: one continue per run. The cube freezes exactly as it was and resumes after the ad;
        // nothing is torn down until the player declines.
        bool OfferRevive(string why,FailureKind kind)
        {
            if(!LiveFlow||!HasMeta||revivedThisRun||adFlow||!overlays||!AdManager.Service.RewardedSupported)return false;
            pendingFailWhy=why;pendingFailKind=kind;phase=Phase.Revive;
            overlays.ShowRevive(AcceptRevive,DeclineRevive);
            Track("rewarded_offer_shown",new Dictionary<string,object> {{"placement",AdManager.Revive},{"kind",kind.ToString()}});
            return true;
        }
        void AcceptRevive()
        {
            if(phase!=Phase.Revive||adFlow)return;
            bool granted=false;
            BeginRewarded(AdManager.Revive);
            StartCoroutine(AdManager.Service.Rewarded(AdManager.Revive,()=>granted=true,()=>
            {
                adFlow=false;
                if(phase!=Phase.Revive)return;
                if(granted)ApplyRevive();else DeclineRevive();
            }));
        }
        void DeclineRevive()
        {
            if(phase!=Phase.Revive||adFlow)return;
            overlays.HideRevive();
            FailNow(pendingFailWhy,pendingFailKind);
        }
        void ApplyRevive()
        {
            overlays.HideRevive();
            revivedThisRun=true;
            health=Mathf.Max(health,MaxHealth*.6f);remaining=Mathf.Max(remaining,10);invulnerability=2.5f;knockback=Vector3.zero;moveVelocity=Vector3.zero;
            // A fall is caught at the edge: step back onto the floor.
            var relative=player.position-centre;
            relative.x=Mathf.Clamp(relative.x,-ArenaHalfX+1.2f,ArenaHalfX-1.2f);relative.z=Mathf.Clamp(relative.z,-Half+1.2f,Half-1.2f);
            player.position=centre+relative;
            phase=Phase.Combat;startHold=true;
            feedback.Play(CombatFeedback.Cue.Heal);
            Track("rewarded_revive");
            RefreshContentHud();
        }

        // Rewarded: try a locked weapon, with no upgrades, for one run. Ownership and the saved loadout never change.
        bool WeaponTrialOffered=>LiveFlow&&HasMeta&&profile!=null&&phase==Phase.Loadout&&!adFlow&&profile.Data.completed[0]&&AdManager.Service.RewardedSupported;
        void WatchWeaponTrial()
        {
            if(!WeaponTrialOffered||profile.IsOwned(inspectedWeapon))return;
            int weapon=inspectedWeapon;bool granted=false;
            BeginRewarded(AdManager.WeaponTrial);
            StartCoroutine(AdManager.Service.Rewarded(AdManager.WeaponTrial,()=>granted=true,()=>
            {
                adFlow=false;
                if(!granted) { metaNotice="Ad not available right now.";RefreshContentHud();return; }
                Track("rewarded_weapon_trial",new Dictionary<string,object> {{"weapon",weapon}});
                pendingTrial=weapon;inspectedWeapon=weaponIndex;holdNextStart=true;
                StartMission(Mathf.Min(missionIndex,profile.Data.highestMission));
            }));
        }
        void ApplyWeaponTrial()
        {
            if(pendingTrial<0)return;
            weaponIndex=pendingTrial;pendingTrial=-1;trialActive=true;runUpgrades=new WeaponProgress {owned=true};
        }
        void EndWeaponTrial()
        {
            if(!trialActive)return;
            trialActive=false;weaponIndex=profile.Data.selectedWeapon;inspectedWeapon=weaponIndex;
        }
    }
}
