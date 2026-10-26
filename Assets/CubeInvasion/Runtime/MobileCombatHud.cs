using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    public sealed partial class MobileCombatHud : MonoBehaviour
    {
        public Vector2 InputValue => joystick?new Vector2(joystick.Value.x,-joystick.Value.y):Vector2.zero;
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public RectTransform SafeRoot { get; private set; }
        [SerializeField] MobileJoystick joystick;
        public void SetRouteStatus(string text,float shield)
        {
            if(!buffText)return;
            buffText.text=shield>0?(string.IsNullOrEmpty(text)?"":text+" · ")+$"KALKAN {shield:0}":text;
            buffText.resizeTextForBestFit=true;buffText.resizeTextMinSize=12;buffText.resizeTextMaxSize=17;
        }
        [SerializeField] Text timer, status, healthLabel, gold, hint, resultTitle, resultDetail, pauseText, soundText, buffText;
        [SerializeField] Image timerFill, healthFill;
        [SerializeField] Text fireRhythm;
        public void ConfigureFireRhythm(Text label) { fireRhythm=label; }
        public void SetFireRhythm(float movement,bool hasTarget,bool active)
        {
            if(!fireRhythm)return;
            fireRhythm.gameObject.SetActive(active);
            fireRhythm.text=!hasTarget?"ATEŞ HATTI BUL":movement<.2f?"ODAK ATEŞİ":movement>.8f?"KOŞU ATIŞI":"YAVAŞLA · HIZLI ATEŞ";
            fireRhythm.color=!hasTarget?VoidToyPalette.Gold:Color.Lerp(VoidToyPalette.HealthText,VoidToyPalette.Gold,movement);
        }
        [SerializeField] GameObject result, pauseOverlay;
        Vector3 timerRestScale;bool hasTimerRestScale;
        public void SetCountdownPressure(float strength,float pulse)
        {
            if(!timer)return;
            if(!hasTimerRestScale) {timerRestScale=timer.rectTransform.localScale;hasTimerRestScale=true;}
            float beat=Mathf.Clamp01(strength*pulse);
            timer.rectTransform.localScale=timerRestScale*(1+beat*.065f);
            // Only a halo around the timer pulses; the arena never gets a full-screen alarm overlay.
            if(timerHalo&&strength>0)timerHalo.color=VoidToyPalette.WithAlpha(timerHalo.color,Mathf.Lerp(.35f,.75f,beat));
        }
        float maxHealth=100;
        public void SetMaxHealth(float value) { maxHealth=value; }
        bool campaignMode;
        public void SetCampaignMode(bool value) { campaignMode=value; }
        public void SetMissionInfo(int stage,int total,string objective,bool suppressHint=false)
        {
            status.text=$"KÜP {stage}/{total}";
            if(objectiveText)objectiveText.text=objective;else status.text+="\n"+objective;
            if(suppressHint)hint.text="";
        }
        Font font;
        Texture2D joystickTexture;
        Sprite joystickSprite;
        Rect lastSafe;
        int lastWidth,lastHeight;
        readonly Color navy=VoidToyPalette.Panel, cyan=VoidToyPalette.Cyan, danger=VoidToyPalette.Danger;
        public void Initialize(Camera camera,Action pause,Action retry,Action mute)
        {
            pauseAction=pause;muteAction=mute;
            if(Canvas && SafeRoot)
            {
                Canvas.worldCamera=camera;
                BindButton(pauseText,pause);BindButton(soundText,mute);
                BindButton(result.transform.Find("Retry").GetComponentInChildren<Text>(true),retry);
                ApplyVoidToyStyle(camera);
                return;
            }
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Canvas=gameObject.AddComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceCamera;Canvas.worldCamera=camera;Canvas.planeDistance=1;Canvas.sortingOrder=20;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(!FindAnyObjectByType<EventSystem>())
            { var events=new GameObject("Mobile UI EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(transform.parent,false); }
            SafeRoot=Rect("Safe area",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var top=Panel("Combat header",SafeRoot,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-144),new Vector2(-16,-12),navy);
            timer=Label("Timer",top,"20.0",42,TextAnchor.MiddleLeft,new Vector2(18,-70),new Vector2(190,-12));
            status=Label("Objectives",top,"KÜP 1/3",17,TextAnchor.UpperRight,new Vector2(210,-65),new Vector2(-18,-18));
            gold=Label("Gold",top,"0",16,TextAnchor.UpperRight,new Vector2(210,-100),new Vector2(-18,-68));
            var bar=Panel("Timer track",top,new Vector2(0,1),new Vector2(1,1),new Vector2(18,-119),new Vector2(-18,-109),VoidToyPalette.Raised);
            timerFill=Panel("Time remaining",bar,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,cyan).GetComponent<Image>();
            var healthTrack=Panel("Health track",SafeRoot,new Vector2(0,1),new Vector2(1,1),new Vector2(28,-170),new Vector2(-140,-156),navy);
            healthFill=Panel("Health",healthTrack,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,VoidToyPalette.Health).GetComponent<Image>();
            healthLabel=Label("Health number",SafeRoot,"CAN 100",14,TextAnchor.MiddleLeft,new Vector2(28,-200),new Vector2(-180,-172));
            pauseText=Button("Pause",SafeRoot,"II",new Vector2(1,1),new Vector2(-104,-200),new Vector2(-20,-154),pause);
            soundText=Button("Sound",SafeRoot,"SES",new Vector2(1,0),new Vector2(-100,30),new Vector2(-20,74),mute);
            buffText=Label("Buff",SafeRoot,"",14,TextAnchor.MiddleLeft,new Vector2(28,-228),new Vector2(-20,-202));
            hint=Label("Status hint",SafeRoot,"",18,TextAnchor.MiddleCenter,new Vector2(20,-290),new Vector2(-20,-232));
            var joystickRoot=Panel("Movement joystick",SafeRoot,Vector2.zero,Vector2.zero,new Vector2(26,44),new Vector2(186,204),VoidToyPalette.Glass);
            joystickSprite=VoidToyStyle.Disc();
            joystickRoot.GetComponent<Image>().sprite=joystickSprite;
            joystick=joystickRoot.gameObject.AddComponent<MobileJoystick>();
            var knob=Panel("Thumb",joystickRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-25,-25),new Vector2(25,25),VoidToyPalette.Text);
            knob.GetComponent<Image>().sprite=joystickSprite;knob.GetComponent<Image>().raycastTarget=false;joystick.knob=knob;
            pauseOverlay=Panel("Paused",SafeRoot,new Vector2(0,.45f),new Vector2(1,.55f),new Vector2(24,0),new Vector2(-24,0),navy).gameObject;
            var pauseLabel=Label("Pause label",pauseOverlay.transform,"DURAKLATILDI",24,TextAnchor.MiddleCenter,Vector2.zero,Vector2.zero);
            pauseLabel.rectTransform.anchorMin=Vector2.zero;pauseLabel.rectTransform.anchorMax=Vector2.one;
            pauseOverlay.SetActive(false);
            result=Panel("Result",SafeRoot,new Vector2(0,.34f),new Vector2(1,.67f),new Vector2(24,0),new Vector2(-24,0),navy).gameObject;
            resultTitle=Label("Result title",result.transform,"",28,TextAnchor.MiddleCenter,new Vector2(12,-75),new Vector2(-12,-12));
            resultDetail=Label("Result details",result.transform,"",17,TextAnchor.MiddleCenter,new Vector2(18,-146),new Vector2(-18,-78));
            Button("Retry",result.transform,"TEKRAR OYNA",new Vector2(.5f,0),new Vector2(-155,24),new Vector2(155,82),retry);
            result.SetActive(false);ApplyVoidToyStyle(camera);ApplySafeArea();
        }
        static void BindButton(Text label,Action action)
        {
            var button=label.GetComponentInParent<Button>(true);
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action());
        }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high;return rect;
        }
        RectTransform Panel(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high,Color color)
        { var rect=Rect(name,parent,min,max,low,high);var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return rect; }
        Text Label(string name,Transform parent,string value,int size,TextAnchor alignment,Vector2 low,Vector2 high)
        { var rect=Rect(name,parent,new Vector2(0,1),new Vector2(1,1),low,high);var label=rect.gameObject.AddComponent<Text>();label.font=font;label.text=value;label.fontSize=size;label.alignment=alignment;label.color=Color.white;label.raycastTarget=false;return label; }
        Text Button(string name,Transform parent,string value,Vector2 anchor,Vector2 low,Vector2 high,Action action)
        {
            var root=Panel(name,parent,anchor,anchor,low,high,cyan);root.GetComponent<Image>().raycastTarget=true;
            var button=root.gameObject.AddComponent<Button>();button.targetGraphic=root.GetComponent<Image>();button.onClick.AddListener(()=>action());
            var label=Label(name+" label",root,value,17,TextAnchor.MiddleCenter,Vector2.zero,Vector2.zero);label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;return label;
        }
        void ApplySafeArea()
        {
            var safe=Screen.safeArea;if(Screen.width==0||Screen.height==0)return;
            lastSafe=safe;lastWidth=Screen.width;lastHeight=Screen.height;
            SafeRoot.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);SafeRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
            SafeRoot.offsetMin=SafeRoot.offsetMax=Vector2.zero;
        }
        public void Refresh(float seconds,float hp,int stage,int enemies,int wallet,int pending,int buffCubes,float buff,bool paused,bool muted,string phase,string reason)
        {
            if(Screen.safeArea!=lastSafe||Screen.width!=lastWidth||Screen.height!=lastHeight)ApplySafeArea();
            bool combat=phase=="Combat",final=combat&&seconds<=3,warning=combat&&seconds<=5;
            timer.text=seconds.ToString("0.0");
            timer.color=final?VoidToyPalette.Text:warning?VoidToyPalette.Warm:VoidToyPalette.Text;
            timerFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(seconds/20),1);
            timerFill.color=final?VoidToyPalette.DangerText:warning?VoidToyPalette.Warm:phase=="Choice"?VoidToyPalette.HealthText:cyan;
            if(timerTile)timerTile.color=final?VoidToyPalette.DangerDeep:phase=="Choice"?ChoiceTile:VoidToyPalette.Glass;
            if(timerHalo&&!warning)timerHalo.color=Color.clear;
            else if(timerHalo&&timerHalo.color.a<.01f)timerHalo.color=VoidToyPalette.WithAlpha(final?danger:VoidToyPalette.Warm,.45f);
            else if(timerHalo)timerHalo.color=VoidToyPalette.WithAlpha(final?danger:VoidToyPalette.Warm,timerHalo.color.a);
            if(objectivePill) {objectivePill.color=final?danger:VoidToyPalette.Glass;objectivePill.gameObject.SetActive(phase!="Choice"&&phase!="Transfer");}
            float share=Mathf.Clamp01(hp/maxHealth);
            healthFill.rectTransform.anchorMax=new Vector2(share,1);healthLabel.text=$"CAN {hp:0}/{maxHealth:0}";
            healthFill.color=share<=.25f?danger:share<=.5f?VoidToyPalette.Warm:VoidToyPalette.Health;
            status.text=$"KÜP {stage}/3";if(objectiveText)objectiveText.text=$"{enemies} HEDEF";else status.text+=$"\n{enemies} HEDEF";
            gold.text=pending>0?$"{VoidToyPalette.FormatGold(wallet)} +{pending}":VoidToyPalette.FormatGold(wallet);
            buffText.text=buffCubes>0?$"HASAR +%{buff*100:0} · {buffCubes} KÜP":"";
            hint.text=phase=="Choice"?"TEMİZLENDİ!\nSağ ya da ileri kapıdan geç":phase=="Transfer"?"SONRAKİ KÜPE GEÇİLİYOR":final?"SON SANİYELER!":"";
            hint.color=phase=="Choice"?VoidToyPalette.HealthText:final?VoidToyPalette.DangerText:VoidToyPalette.Text;
            pauseText.text=paused?"DEVAM":"II";pauseText.fontSize=paused?15:26;if(soundText)soundText.text=muted?"SESSİZ":"SES";pauseOverlay.SetActive(paused&&phase!="Loadout"&&phase!="SaveBlocked"&&phase!="Won"&&phase!="Failed");
            if(paused&&pauseInfo) {pauseInfo.text=$"KÜP {stage}  ·  {seconds:0.0} sn kaldı  ·  süre durdu";pauseSound.text=muted?"SES  ·  KAPALI":"SES  ·  AÇIK";}
            bool ended=phase=="Won"||phase=="Failed";result.SetActive(ended&&!campaignMode);
            if(ended)
            {
                resultTitle.text=phase=="Won"?"TEMİZLENDİ!":"KÜP ÇÖKTÜ";resultTitle.color=phase=="Won"?VoidToyPalette.Gold:VoidToyPalette.DangerText;
                resultDetail.text=phase=="Won"?$"3 küp temizlendi\nBankalanan altın: {wallet}":reason;
            }
            if(paused||ended||(phase=="Loadout"||phase=="SaveBlocked")||phase=="Transfer"||(phase=="Collapse"||phase=="Falling"||phase=="Dying"))joystick.ResetInput();
            joystick.GetComponent<Image>().raycastTarget=!paused&&!ended&&phase!="Loadout"&&phase!="SaveBlocked"&&phase!="Transfer"&&phase!="Collapse"&&phase!="Falling"&&phase!="Dying";
            PlaceHealthOverPlayer(combat||phase=="Choice"||phase=="Transfer");
        }
        void OnDestroy() { if(joystickSprite&&joystickSprite!=VoidToyStyle.Disc())Destroy(joystickSprite);if(joystickTexture)Destroy(joystickTexture); }
        public void ResetJoystick() { if(joystick)joystick.ResetInput(); }
    }
}
