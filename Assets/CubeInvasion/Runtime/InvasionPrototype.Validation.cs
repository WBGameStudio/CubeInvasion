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
#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            if(!GetComponent<P1PerformanceProbe>())
                gameObject.AddComponent<P1PerformanceProbe>().Capture(300,System.IO.Path.Combine(Application.persistentDataPath,"ci_p4_device_profile.json"),()=>phase==Phase.Combat&&!paused);
#endif
            tutorialMoved=false;tutorialOrigin=new Vector3(0,.65f,-2.5f);
        }
        void BeginValidationCube()
        {
            cubeDamageTaken=0;RecordValidation("cube_started");
        }
        void RecordValidation(string name,string detail=null,int coins=0)
        {
            if(validationLog==null)return;
            validationLog.Record(new P4ValidationLog.Event {name=name,run=validationRun,mission=missionIndex,cube=stage,weapon=weaponIndex,
                detail=detail,coins=coins,enemiesLeft=foes.Count,timeRemaining=remaining,hp=health,damageTaken=cubeDamageTaken,seconds=runSeconds});
            if(name=="level_completed"&&missionIndex==0&&PlayerPrefs.GetInt(tutorialPreferenceKey,0)==0)
            { PlayerPrefs.SetInt(tutorialPreferenceKey,1);PlayerPrefs.Save(); }
        }
        void TickOnboarding()
        {
            if(!onboardingHint)return;
            string message="";
            if(HasMeta&&!paused&&PlayerPrefs.GetInt(tutorialPreferenceKey,0)==0&&missionIndex==0)
            {
                if(phase==Phase.Combat)
                {
                    if(!tutorialMoved&&Vector3.Distance(player.position,tutorialOrigin)>.6f)
                    { tutorialMoved=true;RecordValidation("tutorial_moved"); }
                    float elapsed=20-remaining;
                    if(stage==1&&!tutorialMoved)message="SOL ALTTAKİ JOYSTICK İLE HAREKET ET";
                    else if(stage==1&&elapsed<9)message="PARMAĞINI KALDIRMADAN YAVAŞLA · DAHA HIZLI ATEŞ ET";
                    else if(stage==2&&elapsed<6)message="HEPSİNİ 20 SANİYE İÇİNDE İNDİR · KENARDAN UZAK DUR";
                }
                else if(phase==Phase.Choice)message="KAPIDAKİ GÜÇLENDİRMEYİ SEÇMEK İÇİN İÇİNDEN GEÇ";
                else if(phase==Phase.Transfer)message="YENİ KÜP, AYNI MACERA · HAZIR OL";
            }
            if(HasMeta&&!paused&&phase==Phase.Combat&&stage==1&&remaining>14&&missionIndex>0)
            {
                switch(missionIndex)
                {
                    case 1:message="SİPER İKİ TARAFIN MERMİSİNİ DE KESER · AÇI DEĞİŞTİR";break;
                    case 2:message="KÜP ALTINLARI BANKALANIR · MARKETTE SİLAHINI GELİŞTİR";break;
                    case 3:message="VARİLLERİ DÜŞMANLARIN YAKININDA PATLAT";break;
                    case 4:message="KIRMIZI KONİDEN ÇIK · İTİCİ SALDIRI HAZIRLIYOR";break;
                    case 5:message="ALTINLARI TOPLA · SÜRE BİTMEDEN KOTAYI DOLDUR";break;
                    case 6:message="KIRMIZI İŞARETLERDEN KAÇ · BOSS ÜÇ FAZLI";break;
                }
            }
            if(onboardingHint.text!=message)onboardingHint.text=message;
            if(onboardingHint.gameObject.activeSelf!=(message.Length>0))onboardingHint.gameObject.SetActive(message.Length>0);
        }
    }
}
