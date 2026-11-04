using System;
using System.Collections;
using UnityEngine;
#if UNITY_WEBGL
using Playgama;
using Playgama.Modules.Advertisement;
#endif

namespace CubeInvasion.Prototype
{
    public interface IAdService
    {
        bool RewardedSupported { get; }
        bool Busy { get; }
        // Always calls done exactly once, whether or not an ad was shown.
        IEnumerator Interstitial(string placement,Action done);
        // rewarded fires at most once, only after the SDK reports a completed view; done always fires once.
        IEnumerator Rewarded(string placement,Action rewarded,Action done);
    }
    // Mobile builds ship without an ad network; every request completes immediately.
    sealed class NoAdService : IAdService
    {
        public bool RewardedSupported=>false;
        public bool Busy=>false;
        public IEnumerator Interstitial(string placement,Action done) { done?.Invoke();yield break; }
        public IEnumerator Rewarded(string placement,Action rewarded,Action done) { done?.Invoke();yield break; }
    }
#if UNITY_WEBGL
    // Bridge owns frequency (initial delay, minimum interval); the game never counts or times ads itself.
    sealed class BridgeAdService : IAdService
    {
        const float StartGrace=.6f,InterstitialLimit=4,RewardedLimit=8;
        bool busy;
        public bool RewardedSupported=>Bridge.advertisement.isRewardedSupported;
        public bool Busy=>busy;
        public IEnumerator Interstitial(string placement,Action done)
        {
            if(busy||!Bridge.advertisement.isInterstitialSupported) { done?.Invoke();yield break; }
            busy=true;
            bool started=false,opened=false,finished=false;
            void Changed(InterstitialState state)
            {
                started=true;
                if(state==InterstitialState.Opened) { opened=true;PlaygamaService.SetAdOpen(true); }
                else if(state==InterstitialState.Closed||state==InterstitialState.Failed)finished=true;
            }
            Bridge.advertisement.interstitialStateChanged+=Changed;
            Bridge.advertisement.ShowInterstitial(placement);
            float begun=Time.realtimeSinceStartup;
            // No state at all means Bridge skipped this request (frequency rules); a stuck load is abandoned.
            while(!finished)
            {
                float waited=Time.realtimeSinceStartup-begun;
                if(!started&&waited>StartGrace)break;
                if(!opened&&waited>InterstitialLimit)break;
                yield return null;
            }
            Bridge.advertisement.interstitialStateChanged-=Changed;
            PlaygamaService.SetAdOpen(false);busy=false;done?.Invoke();
        }
        public IEnumerator Rewarded(string placement,Action rewarded,Action done)
        {
            if(busy||!RewardedSupported) { done?.Invoke();yield break; }
            busy=true;
            bool opened=false,finished=false,granted=false;
            void Changed(RewardedState state)
            {
                if(state==RewardedState.Opened) { opened=true;PlaygamaService.SetAdOpen(true); }
                else if(state==RewardedState.Rewarded&&!granted) { granted=true;rewarded?.Invoke(); }
                else if(state==RewardedState.Closed||state==RewardedState.Failed)finished=true;
            }
            Bridge.advertisement.rewardedStateChanged+=Changed;
            Bridge.advertisement.ShowRewarded(placement);
            float begun=Time.realtimeSinceStartup;
            while(!finished&&(opened||Time.realtimeSinceStartup-begun<RewardedLimit))yield return null;
            Bridge.advertisement.rewardedStateChanged-=Changed;
            PlaygamaService.SetAdOpen(false);busy=false;done?.Invoke();
        }
    }
#endif
    // Menu-only banner. Reports the reserved height so the layout never sits under the ad.
    public static class PlatformBanner
    {
        public const float ReservedCssHeight=110;
        public static float Inset { get; private set; }
#if UNITY_WEBGL && !UNITY_EDITOR
        static bool wanted,subscribed;
        public static void Set(bool show)
        {
            if(wanted==show||!Bridge.advertisement.isBannerSupported)return;
            wanted=show;
            if(!subscribed) { subscribed=true;Bridge.advertisement.bannerStateChanged+=state=>Inset=state==BannerState.Shown?ReservedCssHeight:0; }
            if(show)Bridge.advertisement.ShowBanner(BannerPosition.Bottom,AdManager.MenuBanner);
            else { Bridge.advertisement.HideBanner();Inset=0; }
        }
#else
        public static void Set(bool show) { }
#endif
    }
    public static class AdManager
    {
        public const string LevelCompleted="level_completed",LevelFailed="level_failed",MainMenu="main_menu",DoubleGold="double_gold",Revive="revive",WeaponTrial="weapon_trial",MenuBanner="menu_banner";
        static IAdService service;
        public static IAdService Service
        {
            get
            {
                if(service!=null)return service;
#if UNITY_EDITOR
                // Automated play-mode checks can't click the Bridge editor ad window; their ads complete at once.
                if(!PlaygamaService.UsesBridgeStorage) { service=new NoAdService();return service; }
#endif
#if UNITY_WEBGL
                service=new BridgeAdService();
#else
                service=new NoAdService();
#endif
                return service;
            }
            set=>service=value; // Editor checks inject scripted ad outcomes.
        }
    }
}
