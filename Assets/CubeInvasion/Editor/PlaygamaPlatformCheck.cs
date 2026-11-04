using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CubeInvasion.Prototype;
using UnityEditor;
using UnityEngine;

// Edit-mode checks for the Playgama platform layer: key/value save backend, rewarded x2 idempotency,
// corrupt/unreadable storage and the scripted ad service contract.
public static class PlaygamaPlatformCheck
{
    sealed class MemoryBackend : IProfileBackend
    {
        public string primary,backup;public bool fail;public int writes;
        public string Name=>"memory";
        public bool PrimaryExists=>primary!=null;
        public bool BackupExists=>backup!=null;
        public string Read(bool fromBackup)=>fromBackup?backup:primary;
        public void RestoreBackup() { primary=backup; }
        public void Write(string envelope) { if(fail)throw new System.IO.IOException("offline");backup=primary??envelope;primary=envelope;writes++; }
    }
    sealed class ScriptedAds : IAdService
    {
        public int rewardEvents;public bool supported=true;
        public bool RewardedSupported=>supported;
        public bool Busy=>false;
        public IEnumerator Interstitial(string placement,Action done) { done();yield break; }
        public IEnumerator Rewarded(string placement,Action rewarded,Action done) { for(int i=0;i<rewardEvents;i++)rewarded();done();yield break; }
    }
    static readonly StringBuilder log=new StringBuilder();
    static int failures;
    static void Check(bool ok,string name) { log.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)failures++; }

    [MenuItem("Cube Invasion/Playgama/Run Platform Checks")]
    public static string Run()
    {
        log.Clear();failures=0;
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");
        int missions=Resources.Load<P2ContentCatalog>("CampaignContent")?.missions.Length??60;
        var disk=new MemoryBackend();
        var store=new MetaProfileStore(disk,rules,missions,0);
        Check(store.CanWrite&&disk.writes==1,"New profile is written once through the backend");
        Check(store.BeginRun(0,0),"Run begins");string run=store.Data.run.id;
        Check(store.BankCube(run,1,30,4,15,false)&&store.BankCube(run,1,30,4,15,false)&&store.Data.gold==30,"Same cube reward cannot bank twice");
        Check(store.BankCube(run,2,40,8,30,false)&&store.BankCube(run,3,50,12,45,true)&&store.Data.gold==120,"Final cube completes the run");
        Check(store.CanDoubleRun(run),"Won run offers rewarded x2");
        Check(store.DoubleRun(run)&&store.Data.gold==240,"Rewarded x2 adds the run gold once");
        Check(!store.DoubleRun(run)&&store.Data.gold==240&&!store.CanDoubleRun(run),"Second rewarded callback pays nothing");
        var reload=new MetaProfileStore(disk,rules,missions,0);
        Check(reload.Data.gold==240&&reload.Data.lastResult.doubled&&!reload.CanDoubleRun(run),"Reload keeps gold and the claimed flag");
        Check(reload.BeginRun(0,0)&&reload.FailRun(reload.Data.run.id,"Time's up (20 s)",0,20)&&!reload.CanDoubleRun(reload.Data.run.id),"Failed run without banked gold offers no x2");
        disk.primary="{broken";
        var recovered=new MetaProfileStore(disk,rules,missions,0);
        Check(recovered.CanWrite&&recovered.Data.gold==240,"Corrupt primary restores the backup");
        var extra=new MetaProfileStore(new MemoryBackend(),rules,missions,0);
        int goldBefore=extra.Data.gold;
        Check(extra.BeginRun(0,0)&&extra.BankCube(extra.Data.run.id,1,50,0,1,false)&&extra.FailRun(extra.Data.run.id,"Out of health",0,5)&&extra.CanDoubleRun(extra.Data.run.id)&&extra.DoubleRun(extra.Data.run.id)&&extra.Data.gold==goldBefore+100,"Failed run with banked gold can double it once");
        goldBefore=extra.Data.gold;
        Check(extra.ClaimDaily(1000)==0&&extra.ClaimDaily(1000)==0&&extra.Data.gold==goldBefore,"First visit starts the daily count without paying");
        Check(extra.ClaimDaily(1001)==50&&extra.Data.dailyStreak==2&&extra.ClaimDaily(1001)==0,"Next day pays by streak, once");
        Check(extra.ClaimDaily(1005)==25&&extra.Data.dailyStreak==1&&extra.ClaimDaily(1004)==0,"A missed day resets the streak; a rolled-back clock pays nothing");
        var broken=new MemoryBackend {primary="x",backup="y"};
        var blocked=new MetaProfileStore(broken,rules,missions,9999);
        Check(!blocked.CanWrite&&blocked.Data.gold==0,"Unreadable save blocks writes instead of overwriting progress");
        var offline=new MemoryBackend();var first=new MetaProfileStore(offline,rules,missions,0);offline.fail=true;
        Check(first.BeginRun(0,0)==false&&first.Data.run==null,"A failed write applies nothing");
        // Ad contract: the game's grant closure guards duplicate SDK rewarded events.
        var ads=new ScriptedAds {rewardEvents=2};int grants=0;bool paid=false;bool done=false;
        var routine=ads.Rewarded("double_gold",()=> { if(paid)return;paid=true;grants++; },()=>done=true);while(routine.MoveNext()) { }
        Check(grants==1&&done,"Duplicate rewarded events grant once and finish");
        ads.rewardEvents=0;grants=0;paid=false;done=false;routine=ads.Rewarded("double_gold",()=>grants++,()=>done=true);while(routine.MoveNext()) { }
        Check(grants==0&&done,"Closed/failed rewarded grants nothing and continues");
        bool continued=false;routine=new ScriptedAds().Interstitial("level_failed",()=>continued=true);while(routine.MoveNext()) { }
        Check(continued,"Interstitial always continues the flow");
        Check(!PlaygamaService.IsBridge||EditorUserBuildSettings.activeBuildTarget==BuildTarget.WebGL,"Bridge path compiled only for WebGL");
        string result=(failures==0?"CUBE_PLAYGAMA_CHECK_OK":"CUBE_PLAYGAMA_CHECK_FAILED "+failures)+"\n"+log;
        Debug.Log(result);return result;
    }
}
