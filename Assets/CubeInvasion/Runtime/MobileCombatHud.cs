using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    public sealed class MobileCombatHud : MonoBehaviour
    {
        public Vector2 InputValue => joystick?new Vector2(joystick.Value.x,-joystick.Value.y):Vector2.zero;
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public RectTransform SafeRoot { get; private set; }
        [SerializeField] MobileJoystick joystick;
        [SerializeField] Text timer, status, healthLabel, gold, hint, resultTitle, resultDetail, pauseText, soundText, buffText;
        [SerializeField] Image timerFill, healthFill;
        [SerializeField] GameObject result, pauseOverlay;
        Font font;
        Texture2D joystickTexture;
        Sprite joystickSprite;
        Rect lastSafe;
        int lastWidth,lastHeight;
        readonly Color navy=new Color(.055f,.09f,.16f,.96f), cyan=new Color(.09f,.74f,.82f), danger=new Color(.9f,.29f,.41f);
        public void Initialize(Camera camera,Action pause,Action retry,Action mute)
        {
            if(Canvas && SafeRoot)
            {
                Canvas.worldCamera=camera;
                BindButton(pauseText,pause);BindButton(soundText,mute);
                BindButton(result.transform.Find("Retry").GetComponentInChildren<Text>(true),retry);
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
            gold=Label("Gold",top,"ALTIN 0",16,TextAnchor.UpperRight,new Vector2(210,-100),new Vector2(-18,-68));
            var bar=Panel("Timer track",top,new Vector2(0,1),new Vector2(1,1),new Vector2(18,-119),new Vector2(-18,-109),new Color(.15f,.22f,.33f));
            timerFill=Panel("Time remaining",bar,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,cyan).GetComponent<Image>();
            var healthTrack=Panel("Health track",SafeRoot,new Vector2(0,1),new Vector2(1,1),new Vector2(28,-170),new Vector2(-140,-156),navy);
            healthFill=Panel("Health",healthTrack,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.09f,.66f,.54f)).GetComponent<Image>();
            healthLabel=Label("Health number",SafeRoot,"CAN 100",14,TextAnchor.MiddleLeft,new Vector2(28,-200),new Vector2(-180,-172));
            pauseText=Button("Pause",SafeRoot,"II",new Vector2(1,1),new Vector2(-104,-200),new Vector2(-20,-154),pause);
            soundText=Button("Sound",SafeRoot,"SES",new Vector2(1,0),new Vector2(-100,30),new Vector2(-20,74),mute);
            buffText=Label("Buff",SafeRoot,"",14,TextAnchor.MiddleLeft,new Vector2(28,-228),new Vector2(-20,-202));
            hint=Label("Status hint",SafeRoot,"",18,TextAnchor.MiddleCenter,new Vector2(20,-290),new Vector2(-20,-232));
            var joystickRoot=Panel("Movement joystick",SafeRoot,Vector2.zero,Vector2.zero,new Vector2(26,44),new Vector2(186,204),new Color(.08f,.15f,.25f,.7f));
            joystickTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                joystickTexture.SetPixel(x,y,new Vector2(x-31.5f,y-31.5f).magnitude<31?Color.white:Color.clear);
            joystickTexture.Apply();joystickSprite=Sprite.Create(joystickTexture,new Rect(0,0,64,64),new Vector2(.5f,.5f));
            joystickRoot.GetComponent<Image>().sprite=joystickSprite;
            joystick=joystickRoot.gameObject.AddComponent<MobileJoystick>();
            var knob=Panel("Thumb",joystickRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-25,-25),new Vector2(25,25),cyan);
            knob.GetComponent<Image>().sprite=joystickSprite;knob.GetComponent<Image>().raycastTarget=false;joystick.knob=knob;
            pauseOverlay=Panel("Paused",SafeRoot,new Vector2(0,.45f),new Vector2(1,.55f),new Vector2(24,0),new Vector2(-24,0),navy).gameObject;
            var pauseLabel=Label("Pause label",pauseOverlay.transform,"DURAKLATILDI",24,TextAnchor.MiddleCenter,Vector2.zero,Vector2.zero);
            pauseLabel.rectTransform.anchorMin=Vector2.zero;pauseLabel.rectTransform.anchorMax=Vector2.one;
            pauseOverlay.SetActive(false);
            result=Panel("Result",SafeRoot,new Vector2(0,.34f),new Vector2(1,.67f),new Vector2(24,0),new Vector2(-24,0),navy).gameObject;
            resultTitle=Label("Result title",result.transform,"",28,TextAnchor.MiddleCenter,new Vector2(12,-75),new Vector2(-12,-12));
            resultDetail=Label("Result details",result.transform,"",17,TextAnchor.MiddleCenter,new Vector2(18,-146),new Vector2(-18,-78));
            Button("Retry",result.transform,"TEKRAR OYNA",new Vector2(.5f,0),new Vector2(-155,24),new Vector2(155,82),retry);
            result.SetActive(false);ApplySafeArea();
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
            timer.text=seconds.ToString("00.0")+" sn";timer.color=seconds<=5?danger:Color.white;
            timerFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(seconds/20),1);timerFill.color=seconds<=5?danger:cyan;
            healthFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(hp/100),1);healthLabel.text=$"CAN {hp:0}/100";
            status.text=$"KÜP {stage}/3\n{enemies} HEDEF";gold.text=$"ALTIN {wallet}"+(pending>0?$"  (+{pending})":"");
            buffText.text=buffCubes>0?$"HASAR +%{buff*100:0} · {buffCubes} KÜP":"";
            hint.text=phase=="Choice"?"KÜP TEMİZLENDİ\nBir güçlendirme kapısından geç":phase=="Transfer"?"SONRAKİ KÜPE İLERLENİYOR":seconds<=5&&phase=="Combat"?"SON SANİYELER!":"";
            pauseText.text=paused?"▶":"II";soundText.text=muted?"SESSİZ":"SES";pauseOverlay.SetActive(paused);
            bool ended=phase=="Won"||phase=="Failed";result.SetActive(ended);
            if(ended) { resultTitle.text=phase=="Won"?"BÖLÜM TAMAMLANDI":"LEVEL FAIL";resultDetail.text=phase=="Won"?$"3 küp temizlendi\nBankalanan altın: {wallet}":reason; }
            if(paused||ended||phase=="Transfer"||(phase=="Collapse"||phase=="Falling"||phase=="Dying"))joystick.ResetInput();
            joystick.GetComponent<Image>().raycastTarget=!paused&&!ended&&phase!="Transfer"&&phase!="Collapse"&&phase!="Falling"&&phase!="Dying";
        }
        void OnDestroy() { if(joystickSprite)Destroy(joystickSprite);if(joystickTexture)Destroy(joystickTexture); }
        public void ResetJoystick() { if(joystick)joystick.ResetInput(); }
    }
}
