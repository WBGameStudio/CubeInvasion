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
        [SerializeField] int layoutVersion;
        const int CalmLayout=7;
        [SerializeField] Image[] statFills;
        [SerializeField] Text[] statValues;
        [SerializeField] Button primaryUpgrade,prevWeapon,nextWeapon,allUpgrades,sheetPlay,sheetWeaponLink;
        [SerializeField] GameObject upgradeSheet,levelSheet;
        [SerializeField] RectTransform weaponDots;
        [SerializeField] Text sheetNumber,sheetTitle,sheetInfo,sheetWeapon;
        WeaponUpgrade primaryKind;
        string homeTitle="";
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
            bool current=weaponUpgradeInfo!=null&&weaponUpgradeInfo.Length==(catalog.weapons.Length>4?5:3)&&weaponUpgradeInfo[0]&&weaponCards!=null&&weaponCards.Length==catalog.weapons.Length&&levelNodes!=null&&levelNodes.Length==catalog.missions.Length&&homeBiome&&layoutVersion==CalmLayout;
            if(pages!=null&&pages.Length==5&&current) { background.raycastTarget=true;VoidToyStyle.Restyle(transform,art.uiFont);StartAmbient();return; }
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
            // Calm Void top bar: page title on the left, gold as a plain number, settings as one quiet icon.
            var header=Rect("Top bar",transform,new Vector2(0,1),new Vector2(1,1),new Vector2(22,-84),new Vector2(-10,-14));
            section=Label("Section",header,"",30,new Vector2(0,-62),new Vector2(-200,-8),white);section.alignment=TextAnchor.MiddleLeft;
            var purse=Rect("Currency capsule",header,new Vector2(1,1),new Vector2(1,1),new Vector2(-196,-58),new Vector2(-66,-12));
            var coin=Image("Gold ring",purse,gold);Top(coin.rectTransform,new Vector2(4,-38),new Vector2(32,-10));
            var coinDot=Image("Gold dot",coin.transform,gold);coinDot.rectTransform.anchorMin=coinDot.rectTransform.anchorMax=new Vector2(.5f,.5f);coinDot.rectTransform.sizeDelta=Vector2.one*10;
            wallet=Label("Wallet",purse,"",24,new Vector2(38,-46),new Vector2(0,0),gold);wallet.alignment=TextAnchor.MiddleRight;
            var gear=Rect("Open settings",header,new Vector2(1,1),new Vector2(1,1),new Vector2(-58,-60),new Vector2(-2,-8));
            var gearHit=gear.gameObject.AddComponent<Image>();gearHit.color=Color.clear;var gearButton=gear.gameObject.AddComponent<UnityEngine.UI.Button>();gearButton.targetGraphic=gearHit;
            // Settings as three slider lines with knobs: a line icon built from UI images, no illustration.
            for(int line=0;line<3;line++)
            {
                var bar=Image("Settings line "+line,gear,muted);Top(bar.rectTransform,new Vector2(14,-18-line*9),new Vector2(-14,-16-line*9));
                var knob=Image("Settings knob "+line,bar.transform,muted);knob.rectTransform.anchorMin=knob.rectTransform.anchorMax=new Vector2(line==1?.7f:.3f,.5f);knob.rectTransform.sizeDelta=Vector2.one*7;
            }
            gearButton.onClick.AddListener(()=>ShowPage(MenuPage.Settings));
            // Three destinations as quiet text tabs; Road and Settings stay reachable from the path and the gear.
            var dock=Rect("Navigation dock",transform,new Vector2(0,0),new Vector2(1,0),new Vector2(0,0),new Vector2(0,96));
            string[] navNames={"Yol","Bölümler","Silahlar","Pilot","Ayarlar"};int[] slots={0,-1,1,2,-1};navigation=new Button[5];
            for(int i=0;i<5;i++)
            {
                int index=i,slot=Mathf.Max(0,slots[i]);var tab=Button("Tab "+i,dock,navNames[i],new Vector2(slot*180,-96),new Vector2(slot*180+180,0),18);
                tab.GetComponent<Image>().sprite=null;tab.GetComponent<Image>().color=Color.clear;
                var label=tab.GetComponentInChildren<Text>();Top(label.rectTransform,new Vector2(0,-66),new Vector2(0,-30));
                var icon=Icon("Navigation icon",tab.transform,null,new Vector2(0,-4),new Vector2(0,0));icon.gameObject.SetActive(false);
                var dot=Image("Tab dot",tab.transform,VoidToyPalette.Cyan);Top(dot.rectTransform,new Vector2(80,-78),new Vector2(-80,-74));
                navigation[i]=tab;if(slots[i]<0)tab.gameObject.SetActive(false);
                tab.onClick.AddListener(()=>ShowPage((MenuPage)index));
            }
            toast=Label("Feedback",transform,"",16,new Vector2(18,-844),new Vector2(-18,-817),gold);toast.alignment=TextAnchor.MiddleCenter;Bottom(toast.rectTransform,122,150);
            VoidToyStyle.Restyle(transform,font);layoutVersion=CalmLayout;ShowPage(MenuPage.Home,false);StartAmbient();
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
            for(int i=0;i<levelNodes.Length;i++) { int index=i;Wire(levelNodes[i],()=>selectMission?.Invoke(index));Wire(homeNodes[i],()=>{selectMission?.Invoke(index);OpenLevelSheet();}); }
            if(prevWeapon)Wire(prevWeapon,()=>selectWeapon?.Invoke((Mathf.Max(0,presentedInspected)-1+weaponCards.Length)%weaponCards.Length));
            if(nextWeapon)Wire(nextWeapon,()=>selectWeapon?.Invoke((Mathf.Max(0,presentedInspected)+1)%weaponCards.Length));
            if(primaryUpgrade)Wire(primaryUpgrade,()=>upgradeWeapon?.Invoke(primaryKind));
            if(allUpgrades)Wire(allUpgrades,()=>upgradeSheet.SetActive(true));
            if(sheetPlay)Wire(sheetPlay,()=>{CloseLevelSheet();launch?.Invoke();});
            if(sheetWeaponLink)Wire(sheetWeaponLink,()=>{CloseLevelSheet();ShowPage(MenuPage.Arsenal);});
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
            if(levelSheet&&levelSheet.activeSelf)CloseLevelSheet();if(upgradeSheet)upgradeSheet.SetActive(false);
            for(int i=0;i<pages.Length;i++)pages[i].SetActive(i==(int)page);
            if(next==MenuPage.Road||next==MenuPage.Home)roadLayoutDirty=true;
            background.sprite=null;background.color=VoidToyPalette.Void;
            var aspect=background.GetComponent<AspectRatioFitter>();if(aspect)aspect.enabled=false;
            shade.color=Color.clear;
            string[] names={homeTitle,"Bölüm yolu","Silahlar","Pilot","Ayarlar"};section.text=names[(int)page];
            for(int i=0;i<navigation.Length;i++)
            {
                bool selected=i==(int)page;navigation[i].GetComponent<Image>().color=Color.clear;navigation[i].GetComponentInChildren<Text>().color=selected?white:muted;
                var dot=navigation[i].transform.Find("Tab dot");if(dot)dot.gameObject.SetActive(selected);
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
            // The toast and the quiet "all upgrades" link share the same line; the toast wins while it shows.
            if(allUpgrades)allUpgrades.GetComponentInChildren<Text>().enabled=toast.text.Length==0;
            if(ReferenceEquals(presentedProfile,profile)&&presentedMission==mission&&presentedInspected==inspected&&presentedEquipped==equipped&&presentedPhase==phase)return;
            if(activeMission!=mission)roadLayoutDirty=true;
            presentedProfile=profile;presentedMission=mission;presentedInspected=inspected;presentedEquipped=equipped;presentedPhase=phase;
            activeMission=mission;
            wallet.text=VoidToyPalette.FormatGold(profile.gold);
            int completed=0;foreach(bool done in profile.completed)if(done)completed++;
            homeProgress.text=$"{completed} / {content.missions.Length} GÖREV";
            progressFill.rectTransform.anchorMax=new Vector2((float)completed/content.missions.Length,1);
            homeLaunch.text=mission>profile.highestMission?$"KİLİTLİ · {mission+1}":$"OYNA · {mission+1}";
            var play=pages[0].transform.Find("Play campaign");bool playable=mission<=profile.highestMission&&profile.weapons[equipped].owned;
            play.GetComponent<Button>().interactable=playable;
            // A locked level gets a quiet surface; only a playable level gets the shining cyan action.
            play.GetComponent<Image>().color=playable?VoidToyPalette.Cyan:VoidToyPalette.Panel;homeLaunch.color=playable?VoidToyPalette.Ink:VoidToyPalette.Disabled;
            var shine=play.Find("Shine");if(shine)shine.gameObject.SetActive(playable);
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
            LayoutHomePath(mission,content.missions.Length,profile);
            if(sheetTitle)
            {
                var level=content.missions[mission];string sentence=level.description;int stop=sentence.IndexOf('.');if(stop>0)sentence=sentence.Substring(0,stop);
                string kind=level.mode==MissionMode.Boss?$"Boss · {level.cubes.Length} faz":level.mode==MissionMode.GoldRush?"Altın kotası":sentence;
                sheetNumber.text=$"Bölüm {mission+1}";sheetTitle.text=level.title;sheetInfo.text=$"{level.cubes.Length} küp · her küp 20 saniye\n{kind}";
                sheetWeapon.text=content.weapons[equipped].title;
                bool open=mission<=profile.highestMission&&profile.weapons[equipped].owned;sheetPlay.interactable=open;
                sheetPlay.GetComponent<Image>().color=open?VoidToyPalette.Cyan:VoidToyPalette.Raised;var playText=sheetPlay.GetComponentInChildren<Text>();playText.text=open?"OYNA":"KİLİTLİ";playText.color=open?VoidToyPalette.Ink:VoidToyPalette.Disabled;
                var sheetShine=sheetPlay.transform.Find("Shine");if(sheetShine)sheetShine.gameObject.SetActive(open);
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
            LayoutArsenal(profile,rules,content,inspected,equipped);
            pilotStats.text=$"CAN  {100+profile.vitality*10}\nHIZ  {4.5f*(1+profile.agility*.03f):0.00}\nALAN  {1+profile.magnet*.2f:0.0} m";
        }
        static readonly Vector2[] pathPoints={new Vector2(195,640),new Vector2(270,584),new Vector2(300,528),new Vector2(250,472),new Vector2(170,416),new Vector2(116,364),new Vector2(78,302),new Vector2(160,248),new Vector2(250,192),new Vector2(195,124)};
        static readonly string[] biomeTitles={"Neon Yard","Factory Rift","Circuit Abyss","Frozen Blocks","Lava Foundry","Void Core"};
        // Sade UI canvas 01: the active biome's ten levels (or a short catalog's levels) climb a winding path,
        // boss last and largest. Done levels are green, the next one cyan with a pulse and the Scout on it.
        void LayoutHomePath(int mission,int count,MetaProfile profile)
        {
            if(homeNodes==null||homeNodes.Length==0)return;
            int window=count>=10?10:count,first=count>=10?Mathf.Clamp(mission,0,count-1)/10*10:0,last=Mathf.Min(count,first+window)-1;
            int current=Mathf.Clamp(profile.highestMission,0,count-1);
            Vector2 Point(int i)
            {
                int slot=window>=10?i-first:Mathf.RoundToInt((i-first)*9f/Mathf.Max(1,window-1));
                var p=pathPoints[Mathf.Clamp(slot,0,9)];return new Vector2(p.x*540f/390f,-(26+(p.y-124)*.98f));
            }
            for(int i=0;i<homeNodes.Length;i++)
            {
                bool shown=i>=first&&i<=last;homeNodes[i].gameObject.SetActive(shown);if(!shown)continue;
                bool boss=count>=10&&i%10==9,done=profile.completed[i],now=i==current&&!done,locked=i>current,selected=i==mission;
                var body=(RectTransform)homeNodes[i].transform;body.anchoredPosition=Point(i);
                body.sizeDelta=Vector2.one*(boss?84:now?70:56);
                homeRings[i].color=boss?VoidToyPalette.DangerDeep:done?VoidToyPalette.Health:now?VoidToyPalette.Cyan:selected?VoidToyPalette.Raised:VoidToyPalette.Panel;
                homeNumbers[i].text=(i+1).ToString();homeNumbers[i].fontSize=now?28:boss?24:20;
                homeNumbers[i].color=boss?VoidToyPalette.DangerText:done||now?VoidToyPalette.Ink:locked&&!selected?VoidToyPalette.Disabled:white;
                var outline=homeNumbers[i].GetComponent<Outline>();if(outline)outline.enabled=false;
            }
            var content=homeNodes[0].transform.parent;int rail=0;
            foreach(Transform child in content)
            {
                if(child.name!="Route rail")continue;
                int from=first+rail;bool shown=rail<window-1;child.gameObject.SetActive(shown);
                if(shown)
                {
                    var a=Point(from);var b=Point(from+1);var rect=(RectTransform)child;var delta=b-a;bool walked=from+1<=current;
                    rect.anchoredPosition=(a+b)*.5f;rect.sizeDelta=new Vector2(delta.magnitude,walked?8:5);rect.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
                    child.GetComponent<Image>().color=walked?VoidToyPalette.WithAlpha(VoidToyPalette.Cyan,.55f):VoidToyPalette.Track;
                }
                rail++;
            }
            bool currentShown=current>=first&&current<=last&&!profile.completed[current];
            var pulse=content.Find("Path pulse") as RectTransform;
            if(pulse) {pulse.gameObject.SetActive(currentShown);if(currentShown) {pulse.anchoredPosition=Point(current);pulse.sizeDelta=Vector2.one*70;}}
            var scout=content.Find("Path scout") as RectTransform;
            if(scout) {scout.gameObject.SetActive(currentShown);if(currentShown)scout.anchoredPosition=Point(current)+new Vector2(0,70);scout.SetAsLastSibling();}
            int biome=count>=10?first/10:0;
            homeTitle=count>=10?biomeTitles[Mathf.Clamp(biome,0,5)]:"Bölüm yolu";
            if(homeBiome)homeBiome.text=count>=10?$"Biyom {biome+1} · bölüm {first+1}–{last+1}":$"Bölüm 1–{count}";
            if(page==MenuPage.Home&&section)section.text=homeTitle;
        }
        void OpenLevelSheet()
        {
            if(!levelSheet)return;levelSheet.SetActive(true);var dock=transform.Find("Navigation dock");if(dock)dock.gameObject.SetActive(false);
            var panelRect=levelSheet.transform.Find("Sheet panel") as RectTransform;
            if(panelRect&&Application.isPlaying) {panelRect.DOKill();panelRect.anchoredPosition=new Vector2(0,-520);panelRect.DOAnchorPosY(-20,.28f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(panelRect.gameObject);}
            if(Application.isPlaying)VoidToyStyle.AnimateShine((RectTransform)sheetPlay.transform,gameObject);
        }
        void CloseLevelSheet()
        {
            if(levelSheet)levelSheet.SetActive(false);var dock=transform.Find("Navigation dock");if(dock)dock.gameObject.SetActive(true);
        }
        // Weapons carousel: dots, three stat bars, and the single upgrade worth buying next as the shining action.
        void LayoutArsenal(MetaProfile profile,P3MetaSettings rules,P2ContentCatalog content,int inspected,int equipped)
        {
            if(!weaponDots||statFills==null)return;
            int count=weaponDots.childCount;float width=0;for(int i=0;i<count;i++)width+=(i==inspected?22:8)+6;float x=-width*.5f;
            for(int i=0;i<count;i++)
            {
                var dot=(RectTransform)weaponDots.GetChild(i);float w=i==inspected?22:8;dot.sizeDelta=new Vector2(w,8);dot.anchoredPosition=new Vector2(x+w*.5f,0);x+=w+6;
                dot.GetComponent<Image>().color=i==inspected?VoidToyPalette.Cyan:line;
            }
            var state=profile.weapons[inspected];
            for(int i=0;i<3;i++)
            {
                var kind=(WeaponUpgrade)i;int level=state.Level(kind),cap=Mathf.Max(1,rules.Cap(kind));
                statFills[i].rectTransform.anchorMax=new Vector2(Mathf.Lerp(.18f,1,(float)level/cap),1);statValues[i].text=$"{level}/{cap}";
            }
            primaryUpgrade.gameObject.SetActive(state.owned);if(allUpgrades)allUpgrades.gameObject.SetActive(state.owned);
            buyWeapon.GetComponent<Image>().color=VoidToyPalette.Cyan;buyWeapon.GetComponentInChildren<Text>().color=VoidToyPalette.Ink;
            if(state.owned)
            {
                int best=-1,bestCost=int.MaxValue;
                for(int i=0;i<weaponUpgrades.Length;i++)
                {
                    var kind=(WeaponUpgrade)i;int level=state.Level(kind);if(level>=rules.Cap(kind))continue;
                    int cost=rules.UpgradeCost(kind,level);if(content.weapons.Length==12&&inspected==0&&i==0&&level==0&&profile.highestMission>=2)cost=0;
                    if(cost<bestCost) {best=i;bestCost=cost;}
                }
                var label=primaryUpgrade.GetComponentInChildren<Text>();
                if(best<0) {label.text="MAKSİMUM";primaryUpgrade.interactable=false;}
                else
                {
                    primaryKind=(WeaponUpgrade)best;string[] names={"HASAR","ATIŞ HIZI","MENZİL","ÖZEL EVRİM","DÖNGÜ"};
                    float step=(best==0?rules.damagePerLevel:best==1?rules.fireRatePerLevel:rules.rangePerLevel)*100;
                    string gain=best==3?"+1":$"+%{(best==4?4:step):0}";
                    label.text=bestCost==0?$"{names[best]} {gain} · ÜCRETSİZ":$"{names[best]} {gain} · {VoidToyPalette.FormatGold(bestCost)}";
                    primaryUpgrade.interactable=profile.gold>=bestCost;
                }
                bool can=primaryUpgrade.interactable;primaryUpgrade.GetComponent<Image>().color=can?VoidToyPalette.Cyan:VoidToyPalette.Raised;label.color=can?VoidToyPalette.Ink:VoidToyPalette.Disabled;
                foreach(var effect in label.GetComponents<Shadow>())effect.enabled=false;
                var shine=primaryUpgrade.transform.Find("Shine");if(shine)shine.gameObject.SetActive(can);
            }
            weaponDescription.text=content.weapons[inspected].description;
            // Ownership rides on the stats line so the description never wraps an orphaned status word.
            weaponStats.text+=state.owned?(inspected==equipped?"   ·   KUŞANILDI":""):"   ·   KİLİTLİ";
        }
        Tween pulseTween,scoutTween;
        // Ambient motion only in Play: the Play button shine, the next-level pulse and the Scout's bob.
        void StartAmbient()
        {
            if(pages==null)return;
            // Round shapes are imported sprites, so the builder saves them and the edit-mode Game view matches Play.
            foreach(var image in GetComponentsInChildren<Image>(true))
            {
                if(image.name=="Gold ring")image.sprite=VoidToyStyle.Ring();
                else if(image.name=="Gold dot"||image.name.StartsWith("Settings knob"))image.sprite=VoidToyStyle.Disc();
            }
            var glow=pages[2].transform.Find("Weapon glow");if(glow)glow.GetComponent<Image>().sprite=VoidToyStyle.Disc();
            if(!Application.isPlaying)return;
            var launch=pages[0].transform.Find("Play campaign");if(launch)VoidToyStyle.AnimateShine((RectTransform)launch,gameObject);
            if(primaryUpgrade)VoidToyStyle.AnimateShine((RectTransform)primaryUpgrade.transform,gameObject);if(buyWeapon)VoidToyStyle.AnimateShine((RectTransform)buyWeapon.transform,gameObject);
            var content=homeNodes!=null&&homeNodes.Length>0?homeNodes[0].transform.parent:null;
            var pulse=content?content.Find("Path pulse") as RectTransform:null;
            if(pulse)
            {
                pulseTween?.Kill();var image=pulse.GetComponent<Image>();
                pulseTween=DOTween.Sequence().Append(pulse.DOScale(1.45f,1.6f).From(1f).SetEase(Ease.OutCubic)).Join(image.DOFade(0,1.6f).From(.55f)).SetLoops(-1).SetUpdate(true).SetLink(pulse.gameObject);
            }
            var scout=content?content.Find("Path scout") as RectTransform:null;
            if(scout) {scoutTween?.Kill();scoutTween=scout.DOScale(1.04f,1.2f).From(1f).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Yoyo).SetUpdate(true).SetLink(scout.gameObject);}
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
