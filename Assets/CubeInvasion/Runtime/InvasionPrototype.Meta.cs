using System;
using System.IO;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("P3 progression")]
        [SerializeField] P3MetaSettings metaSettings;
        [SerializeField] P3MetaHud metaHud;
        MetaProfileStore profile;
        Action retrySave;
        string runId, metaNotice;
        int inspectedWeapon, runKills,runVitality,runAgility,runMagnet;
        float MaxHealth=>100+(HasMeta?runVitality*10:0);
        float MoveSpeed=>4.5f*(1+(HasMeta?runAgility*.03f:0));
        float PickupRadius=>1+(HasMeta?runMagnet*.2f:0);
        float runSeconds;
        WeaponProgress runUpgrades;
        bool HasMeta=>HasContent&&metaSettings;
        float MetaDamage=>HasMeta&&runUpgrades!=null?metaSettings.damagePerLevel*runUpgrades.damage:0;
        float MetaRate=>HasMeta&&runUpgrades!=null?metaSettings.fireRatePerLevel*runUpgrades.fireRate:0;
        float MetaRange=>HasMeta&&runUpgrades!=null?metaSettings.rangePerLevel*runUpgrades.range:0;
        public void ConfigureMeta(P3MetaSettings rules,P3MetaHud presentation) { metaSettings=rules;metaHud=presentation; }
        string ProfilePath
        {
            get
            {
#if UNITY_EDITOR
                string testPath=Environment.GetEnvironmentVariable("CUBE_INVASION_META_TEST_PATH");
                if(!string.IsNullOrEmpty(testPath))return testPath;
#endif
                return Path.Combine(Application.persistentDataPath,"ci_profile_v1.json");
            }
        }
        void InitializeMeta()
        {
            if(!HasMeta) { missionHud.SetMetaMode(false);if(metaHud)metaHud.gameObject.SetActive(false);return; }
            profile=new MetaProfileStore(ProfilePath,metaSettings,content.missions.Length,PlayerPrefs.GetInt("CI.Wallet",0));
            wallet=profile.Data.gold;weaponIndex=profile.Data.selectedWeapon;missionIndex=profile.Data.selectedMission;inspectedWeapon=weaponIndex;
            if(!metaHud)metaHud=new GameObject("P3 Meta HUD",typeof(RectTransform)).AddComponent<P3MetaHud>();
            metaHud.gameObject.SetActive(true);metaHud.Initialize(hud.SafeRoot);
            metaHud.Bind(BuyWeapon,UpgradeWeapon,RetrySave);
            missionHud.SetMetaMode(true);
            metaNotice=profile.Message;
            if(!profile.CanWrite)BlockSave(ReloadProfile);
            else if(profile.Data.run!=null&&!profile.Data.run.ended)
            {
                var previous=profile.Data.run;
                if(!profile.FailRun(previous.id,"Önceki oturum sonlandı",previous.kills,previous.seconds))BlockSave(ReloadProfile);
                else metaNotice="Önceki denemenin tamamlanan küp altınları korundu.";
            }
        }
        void ReloadProfile()
        {
            InitializeMeta();
            if(profile.CanWrite&&retrySave==null)phase=Phase.Loadout;
            RefreshContentHud();
        }
        void BlockSave(Action retry)
        { retrySave=retry;phase=Phase.SaveBlocked;SetPaused(true);metaNotice=profile.Message??"Kayıt tamamlanamadı."; }
        void RetrySave()
        {
            var retry=retrySave;if(retry==null)return;
            retrySave=null;SetPaused(false);retry();RefreshContentHud();
        }
        bool BeginMetaRun(int index)
        {
            if(!HasMeta)return true;
            if(index<0||index>=content.missions.Length||index>profile.Data.highestMission||(!profile.IsOwned(weaponIndex)||!profile.IsOwned(inspectedWeapon)))
            { metaNotice="Önce önceki görevi tamamla ve açık bir silah seç.";RefreshContentHud();return false; }
            if(!profile.BeginRun(index,weaponIndex)) { metaNotice=profile.Message;RefreshContentHud();return false; }
            runVitality=profile.Data.vitality;runAgility=profile.Data.agility;runMagnet=profile.Data.magnet;
            runId=profile.Data.run.id;runSeconds=0;runKills=0;metaNotice=null;
            runUpgrades=JsonUtility.FromJson<WeaponProgress>(JsonUtility.ToJson(profile.Data.weapons[weaponIndex]));
            return true;
        }
        bool BankMetaCube(int amount)
        {
            if(!HasMeta)
            { wallet+=amount;PlayerPrefs.SetInt("CI.Wallet",wallet);PlayerPrefs.Save();return true; }
            if(!profile.BankCube(runId,stage,amount,runKills,runSeconds,stage==CubeCount))
            { BlockSave(()=> { phase=Phase.Combat;Clear(); });return false; }
            wallet=profile.Data.gold;return true;
        }
        void CompleteFailure()
        {
            if(HasMeta&&!profile.FailRun(runId,reason,runKills,runSeconds)) { BlockSave(CompleteFailure);return; }
            phase=Phase.Failed;damageCubes=0;
        }
        void BuyWeapon()
        {
            if(!HasMeta||phase!=Phase.Loadout)return;
            if(profile.Unlock(inspectedWeapon))
            { weaponIndex=inspectedWeapon;wallet=profile.Data.gold;metaNotice="Silah açıldı.";PlayMetaSuccess(true); }
            else { metaNotice=profile.Message;feedback.Play(CombatFeedback.Cue.Denied); }
            RefreshContentHud();
        }
        void UpgradeWeapon(WeaponUpgrade kind)
        {
            if(!HasMeta||phase!=Phase.Loadout)return;
            if(profile.Upgrade(inspectedWeapon,kind))
            { wallet=profile.Data.gold;metaNotice="Yükseltme kaydedildi.";PlayMetaSuccess(false); }
            else { metaNotice=profile.Message;feedback.Play(CombatFeedback.Cue.Denied); }
            RefreshContentHud();
        }
        void PlayMetaSuccess(bool unlock)
        {
            feedback.Play(unlock?CombatFeedback.Cue.Unlock:CombatFeedback.Cue.Upgrade);
            if(frontEnd&&frontEnd.gameObject.activeSelf)frontEnd.PulseWallet();else if(metaHud)metaHud.PulsePurchase();
        }
        void RefreshMetaHud()
        {
            if(!HasMeta||profile==null||!metaHud)return;
            missionHud.SetProgression(profile.Data,metaSettings,inspectedWeapon,missionIndex);
            metaHud.Refresh(profile.Data,metaSettings,content.weapons[inspectedWeapon],inspectedWeapon,weaponIndex,phase.ToString(),metaNotice);
        }
    }
}
