using UnityEngine;
using UnityEngine.UI;
namespace CubeInvasion.Prototype
{
    public sealed partial class MenuFrontEnd
    {
        void BuildHome()
        {
            var root=pages[0].transform;
            Label("Game title",root,"CUBE\nINVASION",48,new Vector2(30,-228),new Vector2(-30,-100),white,FontStyle.Bold);
            var eyebrow=Panel("Title caption",root,new Vector2(28,-264),new Vector2(306,-232),new Color(.025f,.045f,.09f,.9f));
            Label("Eyebrow",eyebrow,"TEK PİLOT. SONSUZ BOŞLUK.",11,new Vector2(10,-29),new Vector2(-8,-3),cyan,FontStyle.Bold);
            homeProgress=Label("Campaign progress",root,"",12,new Vector2(30,-623),new Vector2(-30,-595),white);
            var play=Button("Play campaign",root,"MACERAYA DEVAM ET    ›",new Vector2(26,-709),new Vector2(-26,-637),19,true);play.onClick.AddListener(()=>ShowPage(MenuPage.Road));
            var arsenal=Button("Open arsenal",root,"SİLAH MARKETİ",new Vector2(26,-796),new Vector2(264,-724),14);arsenal.onClick.AddListener(()=>ShowPage(MenuPage.Arsenal));
            var pilot=Button("Open pilot",root,"SCOUT GELİŞTİR",new Vector2(280,-796),new Vector2(-26,-724),14);pilot.onClick.AddListener(()=>ShowPage(MenuPage.Pilot));
        }
        void BuildRoad(P2ContentCatalog catalog)
        {
            var root=pages[1].transform;Heading(root,"BÖLÜMLER","Yolu yukarı kaydır, bir göreve dokun.");
            var view=Rect("Road viewport",root,Vector2.zero,Vector2.one,new Vector2(18,390),new Vector2(-18,-175));view.gameObject.AddComponent<RectMask2D>();
            roadScroll=view.gameObject.AddComponent<ScrollRect>();roadScroll.viewport=view;roadScroll.horizontal=false;roadScroll.movementType=ScrollRect.MovementType.Clamped;roadScroll.scrollSensitivity=40;
            // Transparent image receives touch drags in the empty spaces between nodes.
            var touch=view.gameObject.AddComponent<Image>();touch.color=Color.clear;touch.raycastTarget=true;
            var content=Rect("Road nodes",view,new Vector2(0,0),new Vector2(1,0),Vector2.zero,Vector2.zero);content.pivot=new Vector2(.5f,0);content.sizeDelta=new Vector2(0,180*catalog.missions.Length+100);
            roadScroll.content=content;
            levelNodes=new Button[catalog.missions.Length];nodeRings=new Image[levelNodes.Length];nodeNumbers=new Text[levelNodes.Length];
            Vector2 previous=Vector2.zero;
            for(int i=0;i<levelNodes.Length;i++)
            {
                Vector2 point=new Vector2(252+Mathf.Sin(i*1.55f)*95,95+i*180);
                if(i>0)
                {
                    for(int dot=1;dot<12;dot++)
                    {
                        float t=dot/12f;var p=Vector2.Lerp(previous,point,t);p.x+=Mathf.Sin(t*Mathf.PI)*18*(i%2==0?1:-1);
                        if(Vector2.Distance(p,previous)<53||Vector2.Distance(p,point)<53)continue;
                        var marker=Image("Path dot",content,new Color(.22f,.5f,.65f,.8f));marker.sprite=art.disc;
                        marker.rectTransform.anchorMin=marker.rectTransform.anchorMax=Vector2.zero;marker.rectTransform.sizeDelta=Vector2.one*6;marker.rectTransform.anchoredPosition=p;
                    }
                }
                int index=i;var button=Button("Level "+i,content,(i+1).ToString("00"),Vector2.zero,Vector2.one,22);
                var rect=(RectTransform)button.transform;rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=new Vector2(.5f,.5f);rect.sizeDelta=Vector2.one*78;rect.anchoredPosition=point;
                var ring=button.GetComponent<Image>();ring.sprite=art.disc;ring.type=UnityEngine.UI.Image.Type.Simple;
                button.onClick.AddListener(()=>selectMission?.Invoke(index));levelNodes[i]=button;nodeRings[i]=ring;nodeNumbers[i]=button.GetComponentInChildren<Text>();nodeNumbers[i].color=ink;
                var label=Label("Level name",content,catalog.missions[i].title,14,Vector2.zero,Vector2.zero,white,FontStyle.Bold);
                label.rectTransform.anchorMin=label.rectTransform.anchorMax=Vector2.zero;label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(240,30);label.rectTransform.anchoredPosition=point+Vector2.up*62;label.alignment=TextAnchor.MiddleCenter;
                previous=point;
            }
            roadScroll.verticalNormalizedPosition=0;
            var info=Panel("Selected mission",root,new Vector2(24,-725),new Vector2(-24,-586),panel);
            roadTitle=Label("Selected mission title",info,"",21,new Vector2(18,-43),new Vector2(-18,-8),white,FontStyle.Bold);
            roadDescription=Label("Selected mission description",info,"",13,new Vector2(18,-130),new Vector2(-18,-48),muted);
            roadPlay=Button("Launch selected mission",root,"GÖREVE BAŞLA  ›",new Vector2(24,-804),new Vector2(-24,-744),17,true);roadPlay.onClick.AddListener(()=>launch?.Invoke());
        }
        void BuildArsenal(P2ContentCatalog catalog)
        {
            var root=pages[2].transform;Heading(root,"CEPHANELİK","Silahını seç. Gücünü kalıcı olarak artır.");
            weaponCards=new Button[4];weaponNames=new Text[4];weaponTags=new Text[4];weaponPictures=new Image[4];weaponUpgrades=new Button[3];
            for(int i=0;i<4;i++)
            {
                float x=24+i%2*254,y=-180-i/2*151;int index=i;
                var card=Button("Weapon card "+i,root,"",new Vector2(x,y-137),new Vector2(x+238,y),14);card.onClick.AddListener(()=>selectWeapon?.Invoke(index));weaponCards[i]=card;
                var image=Image("Weapon model",card.transform,Color.white);image.sprite=art.weapons[i];image.preserveAspect=true;
                Top(image.rectTransform,new Vector2(10,-88),new Vector2(-10,-8));weaponPictures[i]=image;
                weaponNames[i]=Label("Weapon name",card.transform,catalog.weapons[i].title,14,new Vector2(12,-111),new Vector2(-12,-85),white,FontStyle.Bold);
                weaponTags[i]=Label("Weapon status",card.transform,"",10,new Vector2(12,-132),new Vector2(-12,-110),gold,FontStyle.Bold);
            }
            var detail=Panel("Weapon detail",root,new Vector2(24,-662),new Vector2(-24,-493),panel);
            weaponHero=Image("Weapon hero",detail,Color.white);weaponHero.preserveAspect=true;Top(weaponHero.rectTransform,new Vector2(8,-155),new Vector2(150,-12));
            weaponTitle=Label("Weapon title",detail,"",21,new Vector2(157,-45),new Vector2(-12,-10),white,FontStyle.Bold);
            weaponDescription=Label("Weapon description",detail,"",13,new Vector2(157,-121),new Vector2(-12,-50),muted);
            weaponStats=Label("Weapon stats",detail,"",12,new Vector2(157,-156),new Vector2(-12,-124),cyan,FontStyle.Bold);
            buyWeapon=Button("Buy weapon",root,"",new Vector2(24,-778),new Vector2(-24,-681),17,true);buyWeapon.onClick.AddListener(()=>buy?.Invoke());
            for(int i=0;i<3;i++)
            {
                var kind=(WeaponUpgrade)i;weaponUpgrades[i]=Button("Weapon upgrade "+i,root,"",new Vector2(24+i*169,-778),new Vector2(181+i*169,-681),12);
                weaponUpgrades[i].onClick.AddListener(()=>upgradeWeapon?.Invoke(kind));
            }
        }
        void BuildPilot()
        {
            var root=pages[3].transform;Heading(root,"SCOUT GELİŞTİR","Bir sonraki göreve daha hazır gir.");
            var hero=Panel("Scout card",root,new Vector2(24,-357),new Vector2(-24,-182),panel);
            var portrait=Image("Scout portrait",hero,Color.white);portrait.sprite=art.scout;portrait.preserveAspect=true;Top(portrait.rectTransform,new Vector2(6,-171),new Vector2(180,-3));
            Label("Scout name",hero,"SCOUT / 01",23,new Vector2(190,-63),new Vector2(-18,-18),white,FontStyle.Bold);
            Label("Scout role",hero,"ÇEVİK KEŞİF PİLOTU",10,new Vector2(190,-91),new Vector2(-18,-67),cyan,FontStyle.Bold);
            pilotStats=Label("Pilot stats",hero,"",12,new Vector2(190,-155),new Vector2(-18,-99),muted);
            characterNames=new Text[3];characterLevels=new Text[3];characterUpgrades=new Button[3];
            string[] names={"YAŞAM ÇEKİRDEĞİ","HAREKET SİSTEMİ","TOPLAMA ALANI"},symbols={"+","››","◎"};
            for(int i=0;i<3;i++)
            {
                float y=-378-i*139;var card=Panel("Character upgrade card "+i,root,new Vector2(24,y-122),new Vector2(-24,y),panel);
                var badge=Panel("Upgrade badge",card,new Vector2(12,-82),new Vector2(66,-28),new Color(.07f,.22f,.3f));
                var symbol=Label("Upgrade symbol",badge,symbols[i],25,Vector2.zero,Vector2.zero,cyan,FontStyle.Bold);Stretch(symbol.rectTransform);symbol.alignment=TextAnchor.MiddleCenter;
                characterNames[i]=Label("Upgrade name",card,names[i],13,new Vector2(80,-42),new Vector2(355,-14),white,FontStyle.Bold);
                characterLevels[i]=Label("Upgrade levels",card,"",13,new Vector2(80,-103),new Vector2(350,-47),muted);
                var kind=(CharacterUpgrade)i;characterUpgrades[i]=Button("Buy character upgrade "+i,card,"",new Vector2(365,-100),new Vector2(-12,-22),13,true);
                characterUpgrades[i].onClick.AddListener(()=>upgradeCharacter?.Invoke(kind));
            }
        }
        void BuildSettings()
        {
            var root=pages[4].transform;Heading(root,"AYARLAR","Oyun deneyimini kendine göre düzenle.");
            var sound=Panel("Audio settings",root,new Vector2(24,-454),new Vector2(-24,-182),panel);
            Label("Sound section",sound,"SES VE MÜZİK",12,new Vector2(20,-42),new Vector2(-20,-14),cyan,FontStyle.Bold);
            musicSlider=Slider("Music",sound,"Müzik",-64,out musicValue);sfxSlider=Slider("SFX",sound,"Efektler",-153,out sfxValue);
            muteToggle=Toggle("Mute",root,"Tüm sesleri kapat","Hızlı sessiz mod",-475);
            hapticsToggle=Toggle("Haptics",root,"Titreşim","Darbe ve geliştirme geri bildirimi",-583);
            frameToggle=Toggle("Frame rate",root,"Akıcı görüntü · 60 FPS","Kapalı: pil dostu 30 FPS",-691);
            musicSlider.onValueChanged.AddListener(_=>SettingsEvent());sfxSlider.onValueChanged.AddListener(_=>SettingsEvent());
            muteToggle.onValueChanged.AddListener(_=>SettingsEvent());hapticsToggle.onValueChanged.AddListener(_=>SettingsEvent());frameToggle.onValueChanged.AddListener(_=>SettingsEvent());
        }
        void Heading(Transform parent,string title,string subtitle)
        { Label("Page title",parent,title,30,new Vector2(26,-133),new Vector2(-26,-93),white,FontStyle.Bold);Label("Page subtitle",parent,subtitle,13,new Vector2(28,-167),new Vector2(-26,-139),muted); }
        Slider Slider(string name,Transform parent,string title,float y,out Text value)
        {
            Label(name+" title",parent,title,17,new Vector2(20,y-30),new Vector2(250,y),white);
            value=Label(name+" value",parent,"",15,new Vector2(320,y-30),new Vector2(-20,y),cyan);value.alignment=TextAnchor.MiddleRight;
            var rect=Rect(name+" slider",parent,new Vector2(0,1),new Vector2(1,1),new Vector2(20,y-78),new Vector2(-20,y-33));
            var hit=rect.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
            var track=Image("Track",rect,line);Stretch(track.rectTransform);track.rectTransform.offsetMin=new Vector2(0,18);track.rectTransform.offsetMax=new Vector2(0,-18);
            var fill=Image("Fill",rect,cyan);Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(0,18);fill.rectTransform.offsetMax=new Vector2(0,-18);
            var handleArea=Rect("Handle area",rect,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(0,-14),new Vector2(0,14));
            var handle=Image("Handle",handleArea,white);handle.sprite=art.disc;handle.rectTransform.sizeDelta=new Vector2(28,0);
            var slider=rect.gameObject.AddComponent<Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.value=1;
            return slider;
        }
        Toggle Toggle(string name,Transform parent,string title,string description,float y)
        {
            var card=Panel(name+" setting",parent,new Vector2(24,y-91),new Vector2(-24,y),panel);
            Label(name+" title",card,title,17,new Vector2(18,-42),new Vector2(370,-12),white,FontStyle.Bold);
            Label(name+" description",card,description,12,new Vector2(18,-76),new Vector2(370,-46),muted);
            card.GetComponent<Image>().raycastTarget=true;
            var toggle=card.gameObject.AddComponent<Toggle>();toggle.toggleTransition=UnityEngine.UI.Toggle.ToggleTransition.None;var box=Image("Toggle background",card,line);box.sprite=art.panel;box.type=UnityEngine.UI.Image.Type.Sliced;Top(box.rectTransform,new Vector2(410,-61),new Vector2(-20,-26));
            var check=Image("Enabled",box.transform,cyan);check.sprite=art.panel;check.type=UnityEngine.UI.Image.Type.Sliced;Stretch(check.rectTransform);check.rectTransform.offsetMin=Vector2.one*5;check.rectTransform.offsetMax=-Vector2.one*5;
            toggle.targetGraphic=box;toggle.graphic=check;return toggle;
        }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        { var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high;return rect; }
        static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
        static void Top(RectTransform rect,Vector2 low,Vector2 high) { rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(high.x>0?0:1,1);rect.offsetMin=low;rect.offsetMax=high; }
        Image Image(string name,Transform parent,Color color)
        { var rect=Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image; }
        RectTransform Panel(string name,Transform parent,Vector2 low,Vector2 high,Color color)
        { var image=Image(name,parent,color);image.sprite=art.panel;image.type=UnityEngine.UI.Image.Type.Sliced;Top(image.rectTransform,low,high);return image.rectTransform; }
        Text Label(string name,Transform parent,string value,int size,Vector2 low,Vector2 high,Color color,FontStyle style=FontStyle.Normal)
        { var rect=Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);Top(rect,low,high);var text=rect.gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.fontStyle=style;text.color=color;text.text=value;text.raycastTarget=false;text.alignment=TextAnchor.MiddleLeft;return text; }
        Button Button(string name,Transform parent,string value,Vector2 low,Vector2 high,int size,bool primary=false)
        { var rect=Panel(name,parent,low,high,primary?cyan:panel);var image=rect.GetComponent<Image>();image.raycastTarget=true;var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;var text=Label("Label",rect,value,size,Vector2.zero,Vector2.zero,primary?ink:white,FontStyle.Bold);Stretch(text.rectTransform);text.alignment=TextAnchor.MiddleCenter;return button; }
    }
}
