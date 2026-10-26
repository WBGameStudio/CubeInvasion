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
        [SerializeField] Text[] weaponUpgradeInfo;
        [SerializeField] Text homeBiome;
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
            art=graphics;
            // A front end saved before the Void Toy layout (no upgrade info rows) is rebuilt in place.
            bool current=weaponUpgradeInfo!=null&&weaponUpgradeInfo.Length==(catalog.weapons.Length>4?5:3)&&weaponUpgradeInfo[0]&&weaponCards!=null&&weaponCards.Length==catalog.weapons.Length&&levelNodes!=null&&levelNodes.Length==catalog.missions.Length&&homeBiome;
            if(pages!=null&&pages.Length==5&&current) { background.raycastTarget=true;VoidToyStyle.Restyle(transform,art.uiFont);return; }
            if(pages!=null&&pages.Length==5)
            {
                for(int i=transform.childCount-1;i>=0;i--)DestroyImmediate(transform.GetChild(i).gameObject);
                pages=null;navigation=null;
            }
            font=art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            transform.SetParent(parent,false);Stretch((RectTransform)transform);
            background=Image("Generated background",transform,new Color(.025f,.32f,.45f));background.raycastTarget=true;Stretch(background.rectTransform);
            var fitter=background.gameObject.AddComponent<AspectRatioFitter>();fitter.enabled=false;
            shade=Image("Readability layer",transform,Color.clear);Stretch(shade.rectTransform);
            pages=new GameObject[5];for(int i=0;i<5;i++)pages[i]=Rect(((MenuPage)i)+" page",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            BuildHome(catalog);BuildRoad(catalog);BuildArsenal(catalog);BuildPilot();BuildSettings();
            // Void Toy top bar: profile chip on the left, gold chip on the right, no full-width slab.
            var header=Rect("Top bar",transform,new Vector2(0,1),new Vector2(1,1),new Vector2(14,-92),new Vector2(-14,-12));
            var profile=Panel("Profile chip",header,new Vector2(0,-76),new Vector2(270,-4),panel);
            var avatarTile=Panel("Scout badge tile",profile,new Vector2(8,-64),new Vector2(64,-8),VoidToyPalette.Cobalt);
            Icon("Scout badge",avatarTile,art.scout,new Vector2(2,-54),new Vector2(-2,-2));
            Label("Brand",profile,"CUBE INVASION",21,new Vector2(74,-40),new Vector2(-10,-8),white);
            section=Label("Section",profile,"",14,new Vector2(75,-66),new Vector2(-10,-40),muted);
            var purse=Panel("Currency capsule",header,new Vector2(340,-70),new Vector2(0,-10),panel);
            Icon("Gold icon",purse,art.coin,new Vector2(8,-54),new Vector2(58,-6));
            wallet=Label("Wallet",purse,"",26,new Vector2(60,-52),new Vector2(-14,-8),gold);wallet.alignment=TextAnchor.MiddleRight;
            var dock=Panel("Navigation dock",transform,new Vector2(12,-960),new Vector2(-12,-847),panel);
            var dockRect=(RectTransform)dock;Bottom(dockRect,12,118);
            string[] navNames={"ANA ÜS","BÖLÜMLER","SİLAHLAR","PİLOT","AYARLAR"};
            Sprite[] navArt={art.journey,art.journey,art.arsenal,art.pilot,art.settings};int[] slots={2,1,0,3,4};navigation=new Button[5];
            for(int i=0;i<5;i++)
            {
                int index=i,slot=slots[i];var tab=Button("Tab "+i,dock,navNames[i],new Vector2(6+slot*102,-100),new Vector2(98+slot*102,-6),12);
                tab.GetComponent<Image>().sprite=art.panel;tab.GetComponent<Image>().color=Color.clear;
                var label=tab.GetComponentInChildren<Text>();Top(label.rectTransform,new Vector2(0,-103),new Vector2(0,-77));
                Icon("Navigation icon",tab.transform,navArt[i],new Vector2(10,-78),new Vector2(-10,2));navigation[i]=tab;
                tab.onClick.AddListener(()=>ShowPage((MenuPage)index));
            }
            toast=Label("Feedback",transform,"",16,new Vector2(18,-844),new Vector2(-18,-817),gold);toast.alignment=TextAnchor.MiddleCenter;Bottom(toast.rectTransform,122,150);
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
            for(int i=3;i<weaponUpgrades.Length;i++) {var stat=(WeaponUpgrade)i;Wire(weaponUpgrades[i],()=>upgradeWeapon?.Invoke(stat));}
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
        public void SetObjectiveDescription(string text) {if(roadDescription&&roadDescription.text!=text)roadDescription.text=text;}
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
            wallet.text=VoidToyPalette.FormatGold(profile.gold);
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
                // Home shows the journey as a compact cube strip: numbers only, locked tiles stay blank.
                homeNumbers[i].text=open||done||selected?(i+1).ToString():"";homeNumbers[i].fontSize=selected?22:16;
            }
            LayoutHomeStrip(mission,content.missions.Length);
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
            weaponHero.sprite=art.weapons[inspected];if(content.weapons.Length>4) {var preview=weaponHero.GetComponent<WeaponModelPreview>();if(!preview)preview=weaponHero.gameObject.AddComponent<WeaponModelPreview>();preview.Show(content.weapons[inspected]);}weaponTitle.text=spec.title;weaponDescription.text=spec.description;
            var resolved=new WeaponRuntimeStats(spec,state,rules);weaponStats.text=$"DPS {resolved.Dps:0.0}   /   MENZİL {resolved.Range:0.0} m";
            buyWeapon.gameObject.SetActive(!state.owned);buyWeapon.interactable=profile.gold>=rules.Price(inspected);
            buyWeapon.GetComponentInChildren<Text>().text=$"SİLAHI AÇ  ·  {rules.Price(inspected)} ALTIN";
            for(int i=0;i<weaponUpgrades.Length;i++)
            {
                var kind=(WeaponUpgrade)i;int level=state.Level(kind),cost=rules.UpgradeCost(kind,level);bool max=level>=rules.Cap(kind);if(content.weapons.Length==12&&inspected==0&&i==0&&level==0&&profile.highestMission>=2)cost=0;
                weaponUpgrades[i].gameObject.SetActive(state.owned);if(weaponUpgradeInfo!=null&&weaponUpgradeInfo.Length==weaponUpgrades.Length)weaponUpgradeInfo[i].transform.parent.gameObject.SetActive(state.owned);weaponUpgrades[i].interactable=!max&&profile.gold>=cost;
                string title=i==0?"HASAR":i==1?"ATIŞ HIZI":i==2?"MENZİL":i==3?"ÖZEL EVRİM":"DÖNGÜ";
                float step=(i==0?rules.damagePerLevel:i==1?rules.fireRatePerLevel:rules.rangePerLevel)*100;
                if(weaponUpgradeInfo!=null&&weaponUpgradeInfo.Length==weaponUpgrades.Length)
                {
                    weaponUpgradeInfo[i].text=$"{title}   SV {level}/{rules.Cap(kind)}\n"+(max?"Maksimum seviye":i==3?"Silaha özel etki +1":$"Sonraki seviye +%{(i==4?4:step):0}");
                    weaponUpgrades[i].GetComponentInChildren<Text>().text=max?"MAKS.":cost==0?"ÜCRETSİZ":$"{cost}";
                }
                else weaponUpgrades[i].GetComponentInChildren<Text>().text=$"{title}  {level}/{rules.Cap(kind)}\n"+(max?"MAKSİMUM":$"+%{step:0} · {cost} ALTIN");
                if(i>=3)continue;
                var character=(CharacterUpgrade)i;int rank=profile.CharacterLevel(character),price=rules.CharacterCost(character,rank);bool capped=rank>=rules.characterCap;
                characterUpgrades[i].interactable=!capped&&profile.gold>=price;
                characterUpgrades[i].GetComponentInChildren<Text>().text=capped?"MAKS.":VoidToyPalette.FormatGold(price);
                characterLevels[i].text=$"SEVİYE {rank}/{rules.characterCap}\n"+(i==0?$"{100+rank*10} → {100+(rank+1)*10} CAN":i==1?$"{4.5f*(1+rank*.03f):0.00} → {4.5f*(1+(rank+1)*.03f):0.00} m/sn":$"{1+rank*.2f:0.0} → {1+(rank+1)*.2f:0.0} m");
                if(capped)characterLevels[i].text=$"SEVİYE {rank}/{rules.characterCap} · MAKSİMUM";
            }
            pilotStats.text=$"CAN  {100+profile.vitality*10}\nHIZ  {4.5f*(1+profile.agility*.03f):0.00}\nALAN  {1+profile.magnet*.2f:0.0} m";
        }
        // Home shows only the active biome's ten levels (or every level of a short catalog), boss slot last and larger.
        void LayoutHomeStrip(int mission,int count)
        {
            if(homeNodes==null||homeNodes.Length==0)return;
            int window=count>=10?10:count,first=count>=10?Mathf.Clamp(mission,0,count-1)/10*10:0,last=Mathf.Min(count,first+window)-1;
            float spacing=440f/Mathf.Max(1,window-1),start=258-spacing*(window-1)*.5f;
            for(int i=0;i<homeNodes.Length;i++)
            {
                bool shown=i>=first&&i<=last;homeNodes[i].gameObject.SetActive(shown);if(!shown)continue;
                bool boss=count>=10&&i%10==9,selected=i==mission;
                var body=(RectTransform)homeNodes[i].transform;body.anchoredPosition=new Vector2(start+spacing*(i-first),0);
                body.sizeDelta=Vector2.one*(selected||boss?Mathf.Min(54,spacing+4):Mathf.Min(44,spacing-6));
            }
            var rails=homeNodes[0].transform.parent;int rail=0;
            foreach(Transform child in rails)
            {
                if(child.name!="Route rail")continue;
                bool shown=rail<window-1;child.gameObject.SetActive(shown);
                if(shown) {var rect=(RectTransform)child;rect.sizeDelta=new Vector2(spacing,8);rect.anchoredPosition=new Vector2(start+spacing*(rail+.5f),0);}
                rail++;
            }
            if(homeBiome)
            {
                int biome=count>=10?first/10:0;
                homeBiome.text=$"BİYOM {biome+1} · {VoidToyPalette.BiomeNames[Mathf.Clamp(biome,0,5)]}"+(count>=10?$"   ·   {first+1}–{last+1}":"");
            }
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
