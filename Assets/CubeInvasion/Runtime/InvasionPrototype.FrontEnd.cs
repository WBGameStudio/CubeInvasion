using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("Generated art / mobile front end")]
        [SerializeField] MenuFrontEnd frontEnd;
        [SerializeField] FrontEndArt frontEndArt;
        float settingsSaveAt=-1;
        void FlushMenuSettings(bool force=false)
        { if(settingsSaveAt>=0&&(force||Time.unscaledTime>=settingsSaveAt)) { GamePrefs.Save();settingsSaveAt=-1; } }
        void OnApplicationPause(bool value) { if(value) { FlushMenuSettings(true);validationLog?.Flush();if(!startHold)SetPaused(true); } }
        public void ConfigureFrontEnd(MenuFrontEnd menu,FrontEndArt graphics) { frontEnd=menu;frontEndArt=graphics; }
        void InitializeFrontEnd()
        {
            if(!frontEnd)return;
            if(!HasMeta) { frontEnd.gameObject.SetActive(false);return; }
            frontEnd.Initialize(hud.SafeRoot,frontEndArt,content);
            frontEnd.Bind(ChangeWeapon,SelectFrontMission,()=>StartMission(missionIndex),BuyWeapon,UpgradeWeapon,UpgradePilot,ApplyMenuSettings);
            frontEnd.BindWeaponTrial(WatchWeaponTrial,()=>WeaponTrialOffered);
            missionHud.UseFrontEnd(true);metaHud.UseFrontEnd(true);
            frontEnd.SyncSettings(feedback.MusicLevel,feedback.SfxLevel,feedback.Muted,GamePrefs.GetInt("CI.Haptics",0)==1,GamePrefs.GetInt("CI.HighFrames",1)==1);
            Application.targetFrameRate=GamePrefs.GetInt("CI.HighFrames",1)==1?60:30;
            RefreshFrontEnd();
        }
        void RefreshFrontEnd()
        {
            if(!frontEnd||!HasMeta||profile==null)return;
            if(phase==Phase.Loadout)frontEnd.SyncSettings(feedback.MusicLevel,feedback.SfxLevel,feedback.Muted,GamePrefs.GetInt("CI.Haptics",0)==1,GamePrefs.GetInt("CI.HighFrames",1)==1);
            frontEnd.Refresh(profile.Data,metaSettings,content,missionIndex,inspectedWeapon,weaponIndex,phase.ToString(),metaNotice);
            if(HasGoldRushVariants&&phase==Phase.Loadout&&missionIndex<=profile.Data.highestMission)frontEnd.SetObjectiveDescription(GoldMissionDescription);
        }
        void SelectFrontMission(int index)
        {
            if(phase!=Phase.Loadout||index<0||index>=content.missions.Length)return;
            if(index<=profile.Data.highestMission&&!profile.Select(index,weaponIndex))metaNotice=profile.Message;
            else { missionIndex=index;metaNotice=index>profile.Data.highestMission?"Clear the previous chapter first.":null; }
            RefreshContentHud();
        }
        void UpgradePilot(CharacterUpgrade kind)
        {
            if(!HasMeta||phase!=Phase.Loadout)return;
            if(profile.UpgradeCharacter(kind)) { wallet=profile.Data.gold;metaNotice="Scout upgrade saved.";PlayMetaSuccess(false); }
            else { metaNotice=profile.Message;feedback.Play(CombatFeedback.Cue.Denied); }
            RefreshContentHud();
        }
        void ApplyMenuSettings(float music,float sfx,bool mute,bool haptic,bool highFrames)
        {
            feedback.SetLevels(music,sfx,mute,false);
            GamePrefs.SetInt("CI.Haptics",haptic?1:0);GamePrefs.SetInt("CI.HighFrames",highFrames?1:0);settingsSaveAt=Time.unscaledTime+.35f;
            Application.targetFrameRate=highFrames?60:30;
        }
    }
}
