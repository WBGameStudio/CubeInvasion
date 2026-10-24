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
        [SerializeField] ScrollRect roadScroll;
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
        bool quiet;
        readonly Color ink=new Color(.025f,.045f,.09f,1),panel=new Color(.055f,.09f,.16f,.96f),line=new Color(.13f,.22f,.34f),cyan=new Color(.1f,.82f,.91f),gold=new Color(1,.73f,.29f),white=new Color(.92f,.96f,1),muted=new Color(.52f,.65f,.78f);
        public MenuPage CurrentPage=>page;
        public void Initialize(RectTransform parent,FrontEndArt graphics,P2ContentCatalog catalog)
        {
            art=graphics;if(pages!=null&&pages.Length==5) { background.raycastTarget=true;return; }
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            transform.SetParent(parent,false);Stretch((RectTransform)transform);
            background=Image("Generated background",transform,ink);background.raycastTarget=true;Stretch(background.rectTransform);
            var fitter=background.gameObject.AddComponent<AspectRatioFitter>();fitter.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            shade=Image("Readability layer",transform,new Color(.01f,.025f,.06f,.55f));Stretch(shade.rectTransform);
            pages=new GameObject[5];for(int i=0;i<5;i++) { pages[i]=Rect(((MenuPage)i)+" page",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject; }
            BuildHome();BuildRoad(catalog);BuildArsenal(catalog);BuildPilot();BuildSettings();
            var header=Panel("Top bar",transform,new Vector2(18,-78),new Vector2(-18,-14),new Color(.025f,.045f,.09f,.92f));
            Label("Brand",header,"C / I",19,new Vector2(16,-48),new Vector2(100,-10),cyan,FontStyle.Bold);
            wallet=Label("Wallet",header,"",18,new Vector2(260,-48),new Vector2(-16,-10),gold,FontStyle.Bold);wallet.alignment=TextAnchor.MiddleRight;
            section=Label("Section",header,"",12,new Vector2(100,-48),new Vector2(260,-10),muted);
            var dock=Panel("Navigation dock",transform,new Vector2(16,-944),new Vector2(-16,-858),new Color(.035f,.06f,.105f,.98f));
            string[] navNames={"ANA MENÜ","BÖLÜMLER","MARKET","PİLOT","AYARLAR"};navigation=new Button[5];
            for(int i=0;i<5;i++)
            {
                int index=i;navigation[i]=Button("Tab "+i,dock,navNames[i],new Vector2(5+i*100,-75),new Vector2(101+i*100,-8),11);
                navigation[i].onClick.AddListener(()=>ShowPage((MenuPage)index));
            }
            toast=Label("Feedback",transform,"",12,new Vector2(28,-849),new Vector2(-28,-818),gold);
            ShowPage(MenuPage.Home,false);
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
            Wire(pages[0].transform.Find("Play campaign").GetComponent<Button>(),()=>ShowPage(MenuPage.Road));
            Wire(pages[0].transform.Find("Open arsenal").GetComponent<Button>(),()=>ShowPage(MenuPage.Arsenal));
            Wire(pages[0].transform.Find("Open pilot").GetComponent<Button>(),()=>ShowPage(MenuPage.Pilot));
            for(int i=0;i<weaponCards.Length;i++) { int index=i;Wire(weaponCards[i],()=>selectWeapon?.Invoke(index)); }
            for(int i=0;i<levelNodes.Length;i++) { int index=i;Wire(levelNodes[i],()=>selectMission?.Invoke(index)); }
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
            if(next==MenuPage.Road)roadLayoutDirty=true;
            background.sprite=page==MenuPage.Home?art.menuHero:art.campaignIslands;
            background.color=Color.white;var aspect=background.GetComponent<AspectRatioFitter>();aspect.aspectRatio=background.sprite.rect.width/background.sprite.rect.height;
            shade.color=new Color(.01f,.025f,.06f,page==MenuPage.Home?.07f:page==MenuPage.Road?.38f:.83f);
            string[] names={"KOMUTA MERKEZİ","SEFER HARİTASI","CEPHANELİK","SCOUT PROGRAMI","TERCİHLER"};section.text=names[(int)page];
            for(int i=0;i<navigation.Length;i++) { navigation[i].GetComponent<Image>().color=i==(int)page?cyan:Color.clear;navigation[i].GetComponentInChildren<Text>().color=i==(int)page?ink:muted; }
            transition?.Kill();pages[(int)page].transform.localScale=Vector3.one;
            if(animate&&Application.isPlaying)transition=pages[(int)page].transform.DOScale(Vector3.one,.22f).From(Vector3.one*.975f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
        }
        public void Refresh(MetaProfile profile,P3MetaSettings rules,P2ContentCatalog content,int mission,int inspected,int equipped,string phase,string message)
        {
            bool visible=phase=="Loadout";
            if(gameObject.activeSelf!=visible)gameObject.SetActive(visible);
            if(!visible) { lastPhase=phase;return; }
            if(lastPhase!=phase)ShowPage(MenuPage.Home,false);lastPhase=phase;
            activeMission=mission;
            wallet.text=$"{profile.gold:N0}  ALTIN";
            if(message!=lastMessage) { lastMessage=message;toastUntil=Time.unscaledTime+3; }
            toast.text=Time.unscaledTime<toastUntil?message??"":"";
            int completed=0;foreach(bool done in profile.completed)if(done)completed++;
            homeProgress.text=$"{completed}/{content.missions.Length} GÖREV TAMAMLANDI  ·  SCOUT HAZIR";
            for(int i=0;i<levelNodes.Length;i++)
            {
                bool open=i<=profile.highestMission,done=profile.completed[i],selected=i==mission;
                nodeRings[i].color=selected?gold:done?cyan:open?new Color(.3f,.55f,.85f):line;
                nodeNumbers[i].text=done?"✓":open?(i+1).ToString("00"):"KİLİT";nodeNumbers[i].fontSize=open?22:14;nodeNumbers[i].color=selected||open?ink:muted;
            }
            var current=content.missions[mission];roadTitle.text=$"{mission+1:00}  {current.title}";
            roadDescription.text=mission>profile.highestMission?"Önce önceki görevi tamamla.":$"{current.cubes.Length} KÜP · HER KÜP 20 sn\n"+current.description;
            roadPlay.interactable=mission<=profile.highestMission&&profile.weapons[equipped].owned;
            roadPlay.GetComponentInChildren<Text>().text=mission>profile.highestMission?"GÖREV KİLİTLİ":"GÖREVE BAŞLA  ›";
            for(int i=0;i<weaponCards.Length;i++)
            {
                bool owned=profile.weapons[i].owned;weaponCards[i].GetComponent<Image>().color=i==inspected?new Color(.08f,.22f,.32f,.98f):panel;
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
            pilotStats.text=$"{100+profile.vitality*10} CAN   ·   {4.5f*(1+profile.agility*.03f):0.00} HIZ   ·   {1+profile.magnet*.2f:0.0} TOPLAMA";
        }
        public void SyncSettings(float music,float sfx,bool mute,bool haptic,bool highFrames)
        {
            quiet=true;musicSlider.value=music;sfxSlider.value=sfx;muteToggle.isOn=mute;hapticsToggle.isOn=haptic;frameToggle.isOn=highFrames;quiet=false;
            musicValue.text=$"%{music*100:0}";sfxValue.text=$"%{sfx*100:0}";
        }
        void SettingsEvent()
        {
            musicValue.text=$"%{musicSlider.value*100:0}";sfxValue.text=$"%{sfxSlider.value*100:0}";
            if(!quiet)settingsChanged?.Invoke(musicSlider.value,sfxSlider.value,muteToggle.isOn,hapticsToggle.isOn,frameToggle.isOn);
        }
        void OnRectTransformDimensionsChange() { if(roadScroll)roadLayoutDirty=true; }
        void LateUpdate() { if(roadLayoutDirty&&page==MenuPage.Road)ReframeRoad(); }
        public void ReframeRoad()
        {
            if(!roadScroll)return;Canvas.ForceUpdateCanvases();roadScroll.StopMovement();
            roadScroll.verticalNormalizedPosition=Mathf.Clamp01((95+activeMission*180-roadScroll.viewport.rect.height*.5f)/Mathf.Max(1,roadScroll.content.rect.height-roadScroll.viewport.rect.height));
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
