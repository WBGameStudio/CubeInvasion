using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CubeInvasion.Prototype
{
    public enum MenuPage { Home, Road, Arsenal, Pilot, Settings }
    public sealed partial class MenuFrontEnd : MonoBehaviour
    {
        [SerializeField] FrontEndArt art;
        [SerializeField] GameObject[] pages;
        [SerializeField] Button[] navigation,weaponCards,weaponUpgrades,characterUpgrades,levelNodes;
        [SerializeField] Image background,shade;
        [SerializeField] Text wallet,section,toast,homeProgress,roadTitle,roadDescription,weaponTitle,weaponDescription,weaponStats,pilotStats;
        [SerializeField] Text[] weaponNames,weaponTags,characterNames,characterLevels,nodeNumbers;
        [SerializeField] Image[] weaponPictures,nodeRings;
        [SerializeField] Button roadPlay,buyWeapon;
        [SerializeField] Image weaponHero;
        [SerializeField] ScrollRect roadScroll,homeScroll;
        [SerializeField] Button[] homeNodes;
        [SerializeField] Image[] homeRings;
        [SerializeField] Text[] homeNumbers;
        [SerializeField] Text homeLaunch,homeMission;
        [SerializeField] Image progressFill;
        [SerializeField] Slider musicSlider,sfxSlider;
        [SerializeField] Toggle muteToggle,hapticsToggle,frameToggle;
        [SerializeField] Text musicValue,sfxValue;
        Action<int> selectWeapon,selectMission;
        Action launch,buy;
        Action<WeaponUpgrade> upgradeWeapon;
        Action<CharacterUpgrade> upgradeCharacter;
        Action<float,float,bool,bool,bool> settingsChanged;
        MenuPage page;
        Font font;
        Tween transition,walletTween;
        string lastMessage;
        float toastUntil;
        string lastPhase;
        int activeMission;
        bool roadLayoutDirty;
        bool quiet,settingsPresented;
        MetaProfile presentedProfile;
        int presentedMission=-1,presentedInspected=-1,presentedEquipped=-1;
        string presentedPhase;
        readonly Color ink=VoidToyPalette.Void,panel=VoidToyPalette.Panel,line=VoidToyPalette.Raised,cyan=VoidToyPalette.Cyan,gold=VoidToyPalette.Gold,white=VoidToyPalette.Text,muted=VoidToyPalette.Muted;
        public MenuPage CurrentPage=>page;
        public void Initialize(RectTransform parent,FrontEndArt graphics,P2ContentCatalog catalog)
        {
            art=graphics;if(pages!=null&&pages.Length==5) { background.raycastTarget=true;VoidToyStyle.Restyle(transform,art.uiFont);return; }
            font=art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            transform.SetParent(parent,false);Stretch((RectTransform)transform);
            background=Image("Generated background",transform,new Color(.025f,.32f,.45f));background.raycastTarget=true;Stretch(background.rectTransform);
            var fitter=background.gameObject.AddComponent<AspectRatioFitter>();fitter.enabled=false;
            shade=Image("Readability layer",transform,Color.clear);Stretch(shade.rectTransform);
            pages=new GameObject[5];for(int i=0;i<5;i++)pages[i]=Rect(((MenuPage)i)+" page",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            BuildHome(catalog);BuildRoad(catalog);BuildArsenal(catalog);BuildPilot();BuildSettings();
            var header=Panel("Top bar",transform,new Vector2(12,-88),new Vector2(-12,-8),new Color(.02f,.24f,.36f));
            var avatar=Icon("Scout badge",header,art.scout,new Vector2(8,-72),new Vector2(72,-4));
            Label("Brand",header,"CUBE INVASION",22,new Vector2(82,-43),new Vector2(305,-8),white);
            section=Label("Section",header,"",12,new Vector2(83,-65),new Vector2(305,-40),muted);
            var purse=Panel("Currency capsule",header,new Vector2(322,-63),new Vector2(-10,-13),new Color(.01f,.16f,.25f));
            Icon("Gold icon",purse,art.coin,new Vector2(-5,-53),new Vector2(53,3));
            wallet=Label("Wallet",purse,"",22,new Vector2(55,-43),new Vector2(-9,-6),white);wallet.alignment=TextAnchor.MiddleRight;
            var dock=Panel("Navigation dock",transform,new Vector2(0,-960),new Vector2(0,-847),new Color(.015f,.24f,.36f));
            var dockRect=(RectTransform)dock;dockRect.anchorMax=new Vector2(1,1);Bottom(dockRect,0,113);
            string[] navNames={"ANA ÜS","BÖLÜMLER","SİLAHLAR","PİLOT","AYARLAR"};
            Sprite[] navArt={art.journey,art.journey,art.arsenal,art.pilot,art.settings};int[] slots={2,1,0,3,4};navigation=new Button[5];
            for(int i=0;i<5;i++)
            {
                int index=i,slot=slots[i];var tab=Button("Tab "+i,dock,navNames[i],new Vector2(3+slot*107,-108),new Vector2(105+slot*107,-4),12);
                tab.GetComponent<Image>().sprite=art.panel;tab.GetComponent<Image>().color=Color.clear;
                var label=tab.GetComponentInChildren<Text>();Top(label.rectTransform,new Vector2(0,-103),new Vector2(0,-77));
                Icon("Navigation icon",tab.transform,navArt[i],new Vector2(10,-78),new Vector2(-10,2));navigation[i]=tab;
                tab.onClick.AddListener(()=>ShowPage((MenuPage)index));
            }
            toast=Label("Feedback",transform,"",16,new Vector2(18,-844),new Vector2(-18,-817),gold);toast.alignment=TextAnchor.MiddleCenter;Bottom(toast.rectTransform,116,143);
            VoidToyStyle.Restyle(transform,font);ShowPage(MenuPage.Home,false);
        }
        public void Bind(Action<int> weapon,Action<int> mission,Action start,Action purchase,Action<WeaponUpgrade> upgrade,Action<CharacterUpgrade> character,Action<float,float,bool,bool,bool> settings)
        {
            selectWeapon=weapon;selectMission=mission;launch=start;buy=purchase;upgradeWeapon=upgrade;upgradeCharacter=character;settingsChanged=settings;
            WireControls();
        }
        static void Wire(Button button,Action action) { button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action()); }
        void WireControls()
        {
            for(int i=0;i<navigation.Length;i++) { var destination=(MenuPage)i;Wire(navigation[i],()=>ShowPage(destination)); }
            Wire(pages[0].transform.Find("Play campaign").GetComponent<Button>(),()=>launch?.Invoke());
            Wire(pages[0].transform.Find("Open arsenal").GetComponent<Button>(),()=>ShowPage(MenuPage.Arsenal));
            Wire(pages[0].transform.Find("Open pilot").GetComponent<Button>(),()=>ShowPage(MenuPage.Pilot));
            for(int i=0;i<weaponCards.Length;i++) { int index=i;Wire(weaponCards[i],()=>selectWeapon?.Invoke(index)); }
            for(int i=0;i<levelNodes.Length;i++) { int index=i;Wire(levelNodes[i],()=>selectMission?.Invoke(index));Wire(homeNodes[i],()=>selectMission?.Invoke(index)); }
            for(int i=0;i<3;i++)
            { var stat=(WeaponUpgrade)i;var pilot=(CharacterUpgrade)i;Wire(weaponUpgrades[i],()=>upgradeWeapon?.Invoke(stat));Wire(characterUpgrades[i],()=>upgradeCharacter?.Invoke(pilot)); }
            Wire(roadPlay,()=>launch?.Invoke());Wire(buyWeapon,()=>buy?.Invoke());
            musicSlider.onValueChanged.RemoveAllListeners();musicSlider.onValueChanged.AddListener(_=>SettingsEvent());
            sfxSlider.onValueChanged.RemoveAllListeners();sfxSlider.onValueChanged.AddListener(_=>SettingsEvent());
            muteToggle.onValueChanged.RemoveAllListeners();muteToggle.onValueChanged.AddListener(_=>SettingsEvent());
            hapticsToggle.onValueChanged.RemoveAllListeners();hapticsToggle.onValueChanged.AddListener(_=>SettingsEvent());
            frameToggle.onValueChanged.RemoveAllListeners();frameToggle.onValueChanged.AddListener(_=>SettingsEvent());
        }
        public void ShowPage(MenuPage next,bool animate=true)
        {
            page=next;toastUntil=0;if(toast)toast.text="";
            for(int i=0;i<pages.Length;i++)pages[i].SetActive(i==(int)page);
            if(next==MenuPage.Road||next==MenuPage.Home)roadLayoutDirty=true;
            background.sprite=null;background.color=VoidToyPalette.Void;
            var aspect=background.GetComponent<AspectRatioFitter>();if(aspect)aspect.enabled=false;
            shade.color=Color.clear;
            string[] names={"MACERAYA HAZIR","YOLCULUK","SİLAHLAR","GELİŞTİR","AYARLAR"};section.text=names[(int)page];
            for(int i=0;i<navigation.Length;i++)
            {
                bool selected=i==(int)page;navigation[i].GetComponent<Image>().color=selected?VoidToyPalette.Raised:Color.clear;navigation[i].GetComponentInChildren<Text>().color=selected?Color.white:muted;
                var icon=navigation[i].transform.Find("Navigation icon");icon.DOKill();icon.localScale=Vector3.one*(selected?1.12f:1);
                if(selected&&animate&&Application.isPlaying)icon.DOScale(1.12f,.24f).From(1).SetEase(Ease.OutBack).SetUpdate(true).SetLink(icon.gameObject);
            }
            transition?.Kill();pages[(int)page].transform.localScale=Vector3.one;
            if(animate&&Application.isPlaying)
            {
                var group=pages[(int)page].GetComponent<CanvasGroup>();if(!group)group=pages[(int)page].AddComponent<CanvasGroup>();
                group.alpha=0;
                transition=DOTween.Sequence().Append(group.DOFade(1,.18f)).Join(pages[(int)page].transform.DOScale(Vector3.one,.24f).From(Vector3.one*.97f).SetEase(Ease.OutCubic)).SetUpdate(true).SetLink(gameObject);
            }
            else { var group=pages[(int)page].GetComponent<CanvasGroup>();if(group)group.alpha=1; }
        }
        public void Refresh(MetaProfile profile,P3MetaSettings rules,P2ContentCatalog content,int mission,int inspected,int equipped,string phase,string message)
        {
            bool visible=phase=="Loadout";
            if(gameObject.activeSelf!=visible)gameObject.SetActive(visible);
            if(!visible) { lastPhase=phase;presentedPhase=null;return; }
            if(lastPhase!=phase)ShowPage(MenuPage.Home,false);lastPhase=phase;
            if(message!=lastMessage) { lastMessage=message;toastUntil=Time.unscaledTime+3; }
            toast.text=Time.unscaledTime<toastUntil?message??"":"";
            if(ReferenceEquals(presentedProfile,profile)&&presentedMission==mission&&presentedInspected==inspected&&presentedEquipped==equipped&&presentedPhase==phase)return;
            if(activeMission!=mission)roadLayoutDirty=true;
            presentedProfile=profile;presentedMission=mission;presentedInspected=inspected;presentedEquipped=equipped;presentedPhase=phase;
            activeMission=mission;
            wallet.text=$"{profile.gold:N0}";
            int completed=0;foreach(bool done in profile.completed)if(done)completed++;
            homeProgress.text=$"{completed} / {content.missions.Length} GÖREV";
            progressFill.rectTransform.anchorMax=new Vector2((float)completed/content.missions.Length,1);
            homeLaunch.text=mission>profile.highestMission?"KİLİTLİ":$"BÖLÜM {mission+1}  OYNA";
            pages[0].transform.Find("Play campaign").GetComponent<Button>().interactable=mission<=profile.highestMission&&profile.weapons[equipped].owned;
            homeMission.text=content.missions[mission].title;
            for(int i=0;i<levelNodes.Length;i++)
            {
                bool open=i<=profile.highestMission,done=profile.completed[i],selected=i==mission;
                var color=selected?VoidToyPalette.Cyan:done?VoidToyPalette.Health:open?VoidToyPalette.Cobalt:VoidToyPalette.Track;
                nodeRings[i].color=homeRings[i].color=color;
                nodeNumbers[i].text=homeNumbers[i].text=open||done?(i+1).ToString():"KİLİT";
                nodeNumbers[i].fontSize=homeNumbers[i].fontSize=open?34:15;
                nodeNumbers[i].color=homeNumbers[i].color=selected?VoidToyPalette.Ink:Color.white;
            }
            var current=content.missions[mission];roadTitle.text=$"{mission+1:00}  {current.title}";
            roadDescription.text=mission>profile.highestMission?"Önce önceki görevi tamamla.":$"{current.cubes.Length} KÜP · HER KÜP 20 sn\n"+current.description;
            roadPlay.interactable=mission<=profile.highestMission&&profile.weapons[equipped].owned;
            roadPlay.GetComponentInChildren<Text>().text=mission>profile.highestMission?"GÖREV KİLİTLİ":$"BÖLÜM {mission+1} OYNA";
            for(int i=0;i<weaponCards.Length;i++)
            {
                bool owned=profile.weapons[i].owned;weaponCards[i].GetComponent<Image>().color=i==inspected?VoidToyPalette.Cobalt:panel;
                weaponTags[i].text=owned?(i==equipped?"KUŞANILDI":"AÇIK"):$"{rules.Price(i)} ALTIN";
                weaponTags[i].color=owned?cyan:gold;
            }
            var spec=content.weapons[inspected];var state=profile.weapons[inspected];
            weaponHero.sprite=art.weapons[inspected];weaponTitle.text=spec.title;weaponDescription.text=spec.description;
            weaponStats.text=$"DPS {spec.NominalDps*(1+state.damage*rules.damagePerLevel)*(1+state.fireRate*rules.fireRatePerLevel):0.0}   /   MENZİL {18*spec.projectileLife*(1+state.range*rules.rangePerLevel):0.0} m";
            buyWeapon.gameObject.SetActive(!state.owned);buyWeapon.interactable=profile.gold>=rules.Price(inspected);
            buyWeapon.GetComponentInChildren<Text>().text=$"SİLAHI AÇ  ·  {rules.Price(inspected)} ALTIN";
            for(int i=0;i<3;i++)
            {
                var kind=(WeaponUpgrade)i;int level=state.Level(kind),cost=rules.UpgradeCost(kind,level);bool max=level>=rules.Cap(kind);
                weaponUpgrades[i].gameObject.SetActive(state.owned);weaponUpgrades[i].interactable=!max&&profile.gold>=cost;
                string title=i==0?"HASAR":i==1?"ATIŞ HIZI":"MENZİL";
                weaponUpgrades[i].GetComponentInChildren<Text>().text=$"{title}  {level}/{rules.Cap(kind)}\n"+(max?"MAKSİMUM":$"+%{(i==0?rules.damagePerLevel:i==1?rules.fireRatePerLevel:rules.rangePerLevel)*100:0} · {cost} ALTIN");
                var character=(CharacterUpgrade)i;int rank=profile.CharacterLevel(character),price=rules.CharacterCost(character,rank);bool capped=rank>=rules.characterCap;
                characterUpgrades[i].interactable=!capped&&profile.gold>=price;
                characterUpgrades[i].GetComponentInChildren<Text>().text=capped?"MAKS.":$"{price}\nALTIN";
                characterLevels[i].text=$"SEVİYE {rank}/{rules.characterCap}\n"+(i==0?$"{100+rank*10} → {100+(rank+1)*10} CAN":i==1?$"{4.5f*(1+rank*.03f):0.00} → {4.5f*(1+(rank+1)*.03f):0.00} m/sn":$"{1+rank*.2f:0.0} → {1+(rank+1)*.2f:0.0} m");
                if(capped)characterLevels[i].text=$"SEVİYE {rank}/{rules.characterCap} · MAKSİMUM";
            }
            pilotStats.text=$"CAN  {100+profile.vitality*10}\nHIZ  {4.5f*(1+profile.agility*.03f):0.00}\nALAN  {1+profile.magnet*.2f:0.0} m";
        }
        public void SyncSettings(float music,float sfx,bool mute,bool haptic,bool highFrames)
        {
            if(settingsPresented&&Mathf.Approximately(musicSlider.value,music)&&Mathf.Approximately(sfxSlider.value,sfx)&&muteToggle.isOn==mute&&hapticsToggle.isOn==haptic&&frameToggle.isOn==highFrames)return;
            settingsPresented=true;quiet=true;musicSlider.value=music;sfxSlider.value=sfx;muteToggle.isOn=mute;hapticsToggle.isOn=haptic;frameToggle.isOn=highFrames;quiet=false;
            musicValue.text=$"%{music*100:0}";sfxValue.text=$"%{sfx*100:0}";
        }
        void SettingsEvent()
        {
            musicValue.text=$"%{musicSlider.value*100:0}";sfxValue.text=$"%{sfxSlider.value*100:0}";
            if(!quiet)settingsChanged?.Invoke(musicSlider.value,sfxSlider.value,muteToggle.isOn,hapticsToggle.isOn,frameToggle.isOn);
        }
        void OnRectTransformDimensionsChange() { if(roadScroll)roadLayoutDirty=true; }
        void LateUpdate() { if(roadLayoutDirty&&(page==MenuPage.Road||page==MenuPage.Home))ReframeRoad(); }
        public void ReframeRoad()
        {
            var scroll=page==MenuPage.Home?homeScroll:roadScroll;if(!scroll)return;Canvas.ForceUpdateCanvases();scroll.StopMovement();
            scroll.verticalNormalizedPosition=Mathf.Clamp01((100+activeMission*164-scroll.viewport.rect.height*.35f)/Mathf.Max(1,scroll.content.rect.height-scroll.viewport.rect.height));
            roadLayoutDirty=false;
        }
        public void PulseWallet()
        {
            walletTween?.Kill();wallet.transform.localScale=Vector3.one;
            walletTween=wallet.transform.DOPunchScale(Vector3.one*.1f,.28f,3,.4f).SetUpdate(true).SetLink(gameObject);
        }
        void OnDestroy() { transition?.Kill();walletTween?.Kill(); }
    }
}
