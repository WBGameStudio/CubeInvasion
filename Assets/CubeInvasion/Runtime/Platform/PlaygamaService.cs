using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL
using Playgama;
using Playgama.Modules.Platform;
#endif

namespace CubeInvasion.Prototype
{
    // The only gameplay-facing entry to Playgama Bridge. Mobile and desktop builds compile the
    // pass-through branch and keep PlayerPrefs / file saves; WebGL routes everything through Bridge.
    public static class PlaygamaService
    {
#if UNITY_WEBGL
        public static readonly bool IsBridge=true;
#else
        public static readonly bool IsBridge=false;
#endif
        // Progress and diagnostics go to Bridge Storage instead of files. Editor play-mode checks that seed a file
        // profile through CUBE_INVASION_META_TEST_PATH keep files on the WebGL target too.
        public static bool UsesBridgeStorage
        {
            get
            {
#if UNITY_EDITOR
                if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUBE_INVASION_META_TEST_PATH")))return false;
#endif
                return IsBridge;
            }
        }
        public const string ProfileKey="ci_profile",ProfileBackupKey="ci_profile_bak",SettingsKey="ci_settings";
        static readonly string[] BootKeys={ProfileKey,ProfileBackupKey,SettingsKey};
        static readonly Dictionary<string,string> cache=new Dictionary<string,string>();
        static readonly Dictionary<string,string> pending=new Dictionary<string,string>();
        static bool writing,gameReadySent,platformPaused,adOpen,focusLost;
        static float retryAt=-1;
        public static bool Ready { get; private set; }=!IsBridge;
        public static bool StorageHealthy { get; private set; }=true;
        public static string Language { get; private set; }="en";
        public static bool PlatformAudioEnabled { get; private set; }=true;
        // Platform pause (tab hidden, platform overlay) or a full-screen ad.
        public static bool ExternalPause=>platformPaused||adOpen;
        public static event Action<bool> ExternalPauseChanged;

#if UNITY_WEBGL
        // The page forwards Bridge events (audio/pause state) as soon as the player starts; the receiving
        // object must exist before the first scene so none of them is dropped.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateBridgeReceiver() { _=Bridge.platform; }
#endif
        // Loads every persistent key in one request before the first scene object reads them.
        public static IEnumerator Boot()
        {
            if(Ready)yield break;
#if UNITY_WEBGL
            Language=string.IsNullOrEmpty(Bridge.platform.language)?"en":Bridge.platform.language;
            PlatformAudioEnabled=Bridge.platform.isAudioEnabled;
            Bridge.platform.audioStateChanged+=enabled=> {PlatformAudioEnabled=enabled;ApplyAudio();};
            Bridge.platform.pauseStateChanged+=paused=>SetPlatformPause(paused);
            bool done=false;
            Fetch(()=>done=true);
            // Bridge resolves or rejects the request; the timeout only guards a broken host page.
            float giveUp=Time.realtimeSinceStartup+10;
            while(!done&&Time.realtimeSinceStartup<giveUp)yield return null;
            if(!done)StorageHealthy=false;
            ApplyAudio();
#endif
            Ready=true;
        }
#if UNITY_WEBGL
        static void Fetch(Action done)
        {
            Bridge.storage.Get(new List<string>(BootKeys),(success,values)=>
            {
                StorageHealthy=success;
                if(success&&values!=null)
                    for(int i=0;i<BootKeys.Length&&i<values.Count;i++)
                        if(!pending.ContainsKey(BootKeys[i])&&!string.IsNullOrEmpty(values[i]))cache[BootKeys[i]]=values[i];
                done?.Invoke();
            });
        }
#endif
        // After a failed boot read: fetch again so the player's Retry sees the stored progress.
        public static void Reload()
        {
#if UNITY_WEBGL
            Fetch(null);
#endif
        }
        public static bool TryGet(string key,out string value)=>cache.TryGetValue(key,out value)&&!string.IsNullOrEmpty(value);
        // Memory is the source of truth for the session; Bridge receives every changed key in one call.
        public static void Save(Dictionary<string,string> values)
        {
            foreach(var pair in values) { cache[pair.Key]=pair.Value;pending[pair.Key]=pair.Value; }
            Flush();
        }
        static void Flush()
        {
#if UNITY_WEBGL
            if(writing||pending.Count==0)return;
            var batch=new Dictionary<string,object>();foreach(var pair in pending)batch[pair.Key]=pair.Value;
            pending.Clear();writing=true;
            Bridge.storage.Set(batch,success=>
            {
                writing=false;StorageHealthy=success;
                if(!success) { foreach(var pair in batch)if(!pending.ContainsKey(pair.Key))pending[pair.Key]=(string)pair.Value;retryAt=Time.realtimeSinceStartup+3; }
                else Flush();
            });
#else
            pending.Clear();
#endif
        }
        // Called from the game loop; only retries a failed write, never writes on its own.
        public static void Tick() { if(retryAt>=0&&Time.realtimeSinceStartup>=retryAt) { retryAt=-1;Flush(); } }
        public static bool GameReadySent=>gameReadySent;
        static readonly List<KeyValuePair<string,int>> deferredLevels=new List<KeyValuePair<string,int>>();
        public static void SendGameReady()
        {
            if(gameReadySent)return;gameReadySent=true;
#if UNITY_WEBGL
            Bridge.platform.SendMessage(PlatformMessage.GameReady);
            foreach(var level in deferredLevels)Level(level.Key,level.Value);
            deferredLevels.Clear();
#endif
        }
        public static void Level(string message,int mission)
        {
#if UNITY_WEBGL
            // The first session starts its mission before the first frame; the platform hears game_ready first.
            if(!gameReadySent) { deferredLevels.Add(new KeyValuePair<string,int>(message,mission));return; }
            var kind=message=="level_completed"?PlatformMessage.LevelCompleted:message=="level_failed"?PlatformMessage.LevelFailed:PlatformMessage.LevelStarted;
            Bridge.platform.SendMessage(kind);
            Analytics(message,new Dictionary<string,object> {{"mission",mission+1}});
#endif
        }
        public static void Analytics(string name,Dictionary<string,object> data=null)
        {
#if UNITY_WEBGL
            Bridge.analytics.Send(name,data);
#endif
        }
        public static bool AdOpen=>adOpen;
        static float lastAdOpenedAt=-1;
        // True when a full-screen ad really appeared after the given realtime moment (skipped requests never open).
        public static bool AdOpenedSince(float moment)=>lastAdOpenedAt>=moment;
        internal static void SetAdOpen(bool open) { if(open)lastAdOpenedAt=Time.realtimeSinceStartup;if(adOpen==open)return;bool before=ExternalPause;adOpen=open;Changed(before); }
        static void SetPlatformPause(bool paused) { if(platformPaused==paused)return;bool before=ExternalPause;platformPaused=paused;Changed(before); }
        public static void SetFocus(bool focused) { focusLost=!focused;ApplyAudio(); }
        static void Changed(bool before) { ApplyAudio();if(before!=ExternalPause)ExternalPauseChanged?.Invoke(ExternalPause); }
        // Platform audio state, ads and hidden tabs silence the listener; the player's own mute lives in CombatFeedback.
        static void ApplyAudio()
        {
            if(!IsBridge)return;
            AudioListener.pause=ExternalPause||focusLost;
            AudioListener.volume=PlatformAudioEnabled?1:0;
        }
    }
}
