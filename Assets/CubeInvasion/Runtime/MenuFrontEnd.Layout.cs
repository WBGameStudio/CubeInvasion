using UnityEngine;
using UnityEngine.UI;
namespace CubeInvasion.Prototype
{
    public sealed partial class MenuFrontEnd
    {
        void BuildHome(P2ContentCatalog catalog)
        {
            // Calm Void home: the level path is the screen. Title lives in the top bar; one shining Play at the bottom.
            var root=pages[0].transform;
            homeBiome=Label("Biome",root,"",15,new Vector2(22,-110),new Vector2(-22,-88),muted);
            BuildPath(root,catalog);
            var progress=Panel("Campaign progress",root,new Vector2(150,-226),new Vector2(-150,-216),line);
            var track=Panel("Progress track",progress,new Vector2(0,-10),new Vector2(0,0),ink);
            progressFill=Image("Progress fill",track,VoidToyPalette.Health);Stretch(progressFill.rectTransform);
            homeProgress=Label("Campaign progress label",root,"",15,new Vector2(20,-258),new Vector2(-20,-232),muted);
            progress.gameObject.SetActive(false);homeProgress.gameObject.SetActive(false);
            var arsenal=Button("Open arsenal",root,"SİLAH",new Vector2(16,-500),new Vector2(102,-406),14);arsenal.gameObject.SetActive(false);
            var pilot=Button("Open pilot",root,"PİLOT",new Vector2(438,-500),new Vector2(-16,-406),14);pilot.gameObject.SetActive(false);
            homeMission=Label("Current mission",root,"",17,new Vector2(24,-0),new Vector2(-24,-0),muted);homeMission.alignment=TextAnchor.MiddleCenter;Bottom(homeMission.rectTransform,214,240);
            var launch=Button("Play campaign",root,"",new Vector2(32,-807),new Vector2(-32,-718),28,true);homeLaunch=launch.GetComponentInChildren<Text>();Bottom((RectTransform)launch.transform,124,208);
            VoidToyStyle.AddShine(launch);
            // Level detail (Sade UI 02): tapping a tile slides up a sheet with three lines and its own Play.
            levelSheet=Rect("Level sheet",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            var dim=Image("Sheet dim",levelSheet.transform,VoidToyPalette.WithAlpha(VoidToyPalette.Ink,.55f));Stretch(dim.rectTransform);dim.raycastTarget=true;
            var dimButton=dim.gameObject.AddComponent<UnityEngine.UI.Button>();dimButton.targetGraphic=dim;dimButton.onClick.AddListener(CloseLevelSheet);
            var sheet=Image("Sheet panel",levelSheet.transform,panel);sheet.sprite=art.casualButton;sheet.type=UnityEngine.UI.Image.Type.Sliced;sheet.pixelsPerUnitMultiplier=4;sheet.raycastTarget=true;
            sheet.rectTransform.anchorMin=Vector2.zero;sheet.rectTransform.anchorMax=new Vector2(1,0);sheet.rectTransform.pivot=new Vector2(.5f,0);sheet.rectTransform.offsetMin=new Vector2(0,-20);sheet.rectTransform.offsetMax=new Vector2(0,500);
            // The rounded sprite is slightly translucent; a flat backing below the corners keeps the path from ghosting through.
            var backing=Image("Sheet backing",sheet.transform,panel);Stretch(backing.rectTransform);backing.rectTransform.offsetMax=new Vector2(0,-40);backing.raycastTarget=false;
            var handle=Image("Sheet handle",sheet.transform,line);Top(handle.rectTransform,new Vector2(242,-22),new Vector2(-242,-15));
            sheetNumber=Label("Sheet number",sheet.transform,"",16,new Vector2(28,-66),new Vector2(-28,-42),muted);
            sheetTitle=Label("Sheet title",sheet.transform,"",36,new Vector2(28,-116),new Vector2(-28,-68),white);
            sheetInfo=Label("Sheet info",sheet.transform,"",19,new Vector2(28,-226),new Vector2(-28,-132),white);sheetInfo.lineSpacing=1.3f;sheetInfo.alignment=TextAnchor.UpperLeft;
            sheetWeapon=Label("Sheet weapon",sheet.transform,"",19,new Vector2(28,-282),new Vector2(-150,-246),white);
            sheetWeaponLink=Button("Change weapon",sheet.transform,"Değiştir",new Vector2(380,-284),new Vector2(-16,-244),17);sheetWeaponLink.GetComponent<Image>().color=Color.clear;sheetWeaponLink.GetComponentInChildren<Text>().color=muted;
            sheetPlay=Button("Sheet play",sheet.transform,"OYNA",new Vector2(28,-0),new Vector2(-28,-0),26,true);Bottom((RectTransform)sheetPlay.transform,52,136);VoidToyStyle.AddShine(sheetPlay);
            levelSheet.SetActive(false);
        }
        // Level path: one biome's ten tiles on a winding line, bottom to top, boss last. Rails sit behind tiles.
        void BuildPath(Transform root,P2ContentCatalog catalog)
        {
            var view=Rect("Road viewport",root,Vector2.zero,Vector2.one,new Vector2(0,250),new Vector2(0,-120));
            var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.horizontal=false;scroll.vertical=false;
            var touch=view.gameObject.AddComponent<Image>();touch.color=Color.clear;touch.raycastTarget=false;
            var content=Rect("Road nodes",view,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);scroll.content=content;
            int count=catalog.missions.Length;var nodes=new Button[count];var rings=new Image[count];var numbers=new Text[count];
            for(int i=1;i<count;i++)
            {
                var rail=Image("Route rail",content,line);var rect=rail.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(.5f,.5f);
            }
            var pulse=Image("Path pulse",content,VoidToyPalette.Cyan);pulse.sprite=art.casualButton;pulse.type=UnityEngine.UI.Image.Type.Sliced;pulse.pixelsPerUnitMultiplier=6;
            pulse.rectTransform.anchorMin=pulse.rectTransform.anchorMax=new Vector2(0,1);
            for(int i=0;i<count;i++)
            {
                var button=Button("Level "+i,content,"",Vector2.zero,Vector2.one,22);var body=(RectTransform)button.transform;
                body.anchorMin=body.anchorMax=new Vector2(0,1);body.pivot=new Vector2(.5f,.5f);body.sizeDelta=Vector2.one*66;
                button.GetComponent<Image>().pixelsPerUnitMultiplier=6;
                nodes[i]=button;rings[i]=button.GetComponent<Image>();numbers[i]=button.GetComponentInChildren<Text>();
            }
            var scout=Image("Path scout",content,Color.white);scout.sprite=art.scout;scout.preserveAspect=true;
            scout.rectTransform.anchorMin=scout.rectTransform.anchorMax=new Vector2(0,1);scout.rectTransform.sizeDelta=Vector2.one*92;
            homeNodes=nodes;homeRings=rings;homeNumbers=numbers;homeScroll=scroll;
        }
        void QuickIcon(Button button,Sprite sprite)
        {
            var label=button.GetComponentInChildren<Text>();Top(label.rectTransform,new Vector2(0,-93),new Vector2(0,-65));
            Icon("Shortcut icon",button.transform,sprite,new Vector2(6,-69),new Vector2(-6,6));
        }
        void BuildRoad(P2ContentCatalog catalog)
        {
            var root=pages[1].transform;Heading(root,"Bölüm yolu","Bir düğüme dokun, görevini seç.");BuildMap(root,catalog,false);
            var info=Panel("Selected mission",root,new Vector2(26,-721),new Vector2(-26,-586),panel);
            roadTitle=Label("Selected mission title",info,"",26,new Vector2(18,-46),new Vector2(-18,-8),gold);
            roadDescription=Label("Selected mission description",info,"",16,new Vector2(18,-126),new Vector2(-18,-49),white);
            roadPlay=Button("Launch selected mission",root,"GÖREVE BAŞLA",new Vector2(75,-810),new Vector2(-75,-737),25,true);Bottom((RectTransform)roadPlay.transform,150,223);Bottom(info,239,374);
        }
        void BuildMap(Transform root,P2ContentCatalog catalog,bool home)
        {
            var view=Rect("Road viewport",root,Vector2.zero,Vector2.one,new Vector2(12,home?279:384),new Vector2(-12,home?-157:-184));view.gameObject.AddComponent<RectMask2D>();
            var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.viewport=view;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=42;
            var touch=view.gameObject.AddComponent<Image>();touch.color=Color.clear;touch.raycastTarget=true;
            var content=Rect("Road nodes",view,new Vector2(0,0),new Vector2(1,0),Vector2.zero,Vector2.zero);content.pivot=new Vector2(.5f,0);content.sizeDelta=new Vector2(0,164*catalog.missions.Length+170);scroll.content=content;
            var nodes=new Button[catalog.missions.Length];var rings=new Image[nodes.Length];var numbers=new Text[nodes.Length];
            Vector2 previous=Vector2.zero;
            for(int i=0;i<nodes.Length;i++)
            {
                var point=new Vector2(258+Mathf.Sin(i*1.25f)*46,100+i*164);
                if(i>0)
                {
                    var delta=point-previous;
                    var edge=Image("Route rail",content,new Color(.03f,.17f,.25f));edge.transform.SetAsFirstSibling();var rect=edge.rectTransform;
                    rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=new Vector2(.5f,0);rect.sizeDelta=new Vector2(15,delta.magnitude);rect.anchoredPosition=previous;
                    rect.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg);
                    var rail=Image("Route highlight",edge.transform,new Color(.52f,.85f,.9f));Stretch(rail.rectTransform);rail.rectTransform.offsetMin=new Vector2(5,0);rail.rectTransform.offsetMax=new Vector2(-5,0);
                }
                var button=Button("Level "+i,content,"",Vector2.zero,Vector2.one,34);var body=(RectTransform)button.transform;
                body.anchorMin=body.anchorMax=Vector2.zero;body.pivot=new Vector2(.5f,.5f);body.sizeDelta=new Vector2(106,104);body.anchoredPosition=point;
                nodes[i]=button;rings[i]=button.GetComponent<Image>();numbers[i]=button.GetComponentInChildren<Text>();
                var label=Label("Level name",content,catalog.missions[i].title,17,Vector2.zero,Vector2.zero,white);
                label.rectTransform.anchorMin=label.rectTransform.anchorMax=Vector2.zero;label.rectTransform.sizeDelta=new Vector2(240,34);label.rectTransform.anchoredPosition=point+Vector2.up*66;label.alignment=TextAnchor.MiddleCenter;
                previous=point;
            }
            if(home) { homeNodes=nodes;homeRings=rings;homeNumbers=numbers;homeScroll=scroll; }
            else { levelNodes=nodes;nodeRings=rings;nodeNumbers=numbers;roadScroll=scroll; }
        }
        void BuildArsenal(P2ContentCatalog catalog)
        {
            // Calm Void weapons (Sade UI 03): one weapon at a time on a soft glow, arrows and dots to move,
            // three thin stats, one shining upgrade. Every upgrade row lives in a sheet behind "Tüm yükseltmeler".
            var root=pages[2].transform;
            bool expanded=catalog.weapons.Length>4;int upgradeCount=expanded?5:3,total=catalog.weapons.Length;
            var glow=Image("Weapon glow",root,panel);Top(glow.rectTransform,new Vector2(120,-432),new Vector2(-120,-132));
            weaponHero=Icon("Weapon hero",root,art.weapons[0],new Vector2(150,-402),new Vector2(-150,-162));
            prevWeapon=Button("Previous weapon",root,"<",new Vector2(12,-320),new Vector2(84,-248),40);prevWeapon.GetComponent<Image>().color=Color.clear;
            nextWeapon=Button("Next weapon",root,">",new Vector2(456,-320),new Vector2(-12,-248),40);nextWeapon.GetComponent<Image>().color=Color.clear;
            weaponTitle=Label("Weapon title",root,"",34,new Vector2(20,-494),new Vector2(-20,-446),white);weaponTitle.alignment=TextAnchor.MiddleCenter;
            weaponDescription=Label("Weapon description",root,"",16,new Vector2(48,-546),new Vector2(-48,-498),muted);weaponDescription.alignment=TextAnchor.UpperCenter;
            weaponDots=Rect("Weapon dots",root,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-568),new Vector2(0,-560));
            for(int i=0;i<total;i++) {var dot=Image("Dot "+i,weaponDots,line);dot.rectTransform.anchorMin=dot.rectTransform.anchorMax=new Vector2(.5f,.5f);dot.rectTransform.sizeDelta=new Vector2(8,8);}
            string[] statNames={"Hasar","Atış hızı","Menzil"};statFills=new Image[3];statValues=new Text[3];
            for(int i=0;i<3;i++)
            {
                float y=-592-i*30;
                Label("Stat name "+i,root,statNames[i],16,new Vector2(56,y-22),new Vector2(170,y),muted);
                var bar=Image("Stat track "+i,root,panel);Top(bar.rectTransform,new Vector2(174,y-14),new Vector2(-104,y-8));
                statFills[i]=Image("Stat fill",bar.transform,white);statFills[i].rectTransform.anchorMin=Vector2.zero;statFills[i].rectTransform.anchorMax=new Vector2(.5f,1);statFills[i].rectTransform.offsetMin=statFills[i].rectTransform.offsetMax=Vector2.zero;
                statValues[i]=Label("Stat value "+i,root,"",16,new Vector2(440,y-22),new Vector2(-56,y),white);statValues[i].alignment=TextAnchor.MiddleRight;
            }
            weaponStats=Label("Weapon stats",root,"",14,new Vector2(20,-702),new Vector2(-20,-682),muted);weaponStats.alignment=TextAnchor.MiddleCenter;
            primaryUpgrade=Button("Primary upgrade",root,"",new Vector2(32,-790),new Vector2(-32,-714),24,true);VoidToyStyle.AddShine(primaryUpgrade);
            buyWeapon=Button("Buy weapon",root,"",new Vector2(32,-790),new Vector2(-32,-714),24,true);VoidToyStyle.AddShine(buyWeapon);
            allUpgrades=Button("All upgrades",root,"Tüm yükseltmeler",new Vector2(140,-842),new Vector2(-140,-800),17);allUpgrades.GetComponent<Image>().color=Color.clear;allUpgrades.GetComponentInChildren<Text>().color=muted;
            upgradeSheet=Rect("Upgrade sheet",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            var dim=Image("Sheet dim",upgradeSheet.transform,VoidToyPalette.WithAlpha(VoidToyPalette.Ink,.6f));Stretch(dim.rectTransform);dim.raycastTarget=true;
            var dimButton=dim.gameObject.AddComponent<UnityEngine.UI.Button>();dimButton.targetGraphic=dim;dimButton.onClick.AddListener(()=>upgradeSheet.SetActive(false));
            float sheetHeight=110+upgradeCount*66;
            var sheet=Image("Sheet panel",upgradeSheet.transform,panel);sheet.sprite=art.casualButton;sheet.type=UnityEngine.UI.Image.Type.Sliced;sheet.pixelsPerUnitMultiplier=4;sheet.raycastTarget=true;
            sheet.rectTransform.anchorMin=Vector2.zero;sheet.rectTransform.anchorMax=new Vector2(1,0);sheet.rectTransform.offsetMin=new Vector2(0,80);sheet.rectTransform.offsetMax=new Vector2(0,80+sheetHeight);
            Label("Sheet title",sheet.transform,"Yükseltmeler",24,new Vector2(24,-56),new Vector2(-140,-16),white);
            var close=Button("Close upgrades",sheet.transform,"Kapat",new Vector2(400,-56),new Vector2(-12,-14),17);close.GetComponent<Image>().color=Color.clear;close.GetComponentInChildren<Text>().color=muted;
            close.onClick.AddListener(()=>upgradeSheet.SetActive(false));
            weaponUpgrades=new Button[upgradeCount];weaponUpgradeInfo=new Text[upgradeCount];
            for(int i=0;i<upgradeCount;i++)
            {
                float y=-70-i*66;var row=Panel("Upgrade row "+i,sheet.transform,new Vector2(16,y-58),new Vector2(-16,y),line);
                weaponUpgradeInfo[i]=Label("Upgrade info",row,"",17,new Vector2(18,-56),new Vector2(-140,-2),white);
                var price=Button("Weapon upgrade "+i,row,"",new Vector2(380,-50),new Vector2(-8,-8),22,true);
                price.GetComponent<Image>().color=gold;weaponUpgrades[i]=price;
            }
            upgradeSheet.SetActive(false);
            // Cards stay as selection hooks (and for tests) but the carousel replaces the visible collection.
            var collection=Rect(expanded?"Weapon collection viewport":"Weapon collection",root,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-812),new Vector2(-16,-678));
            weaponCards=new Button[total];weaponNames=new Text[total];weaponTags=new Text[total];weaponPictures=new Image[total];
            for(int i=0;i<total;i++)
            {
                float x=i*128;var card=Button("Weapon card "+i,collection,"",new Vector2(x,-126),new Vector2(x+120,0),14);weaponCards[i]=card;
                weaponPictures[i]=Icon("Weapon model",card.transform,art.weapons[i],new Vector2(8,-72),new Vector2(-8,-6));
                weaponNames[i]=Label("Weapon name",card.transform,catalog.weapons[i].title,14,new Vector2(3,-98),new Vector2(-3,-74),white);weaponNames[i].alignment=TextAnchor.MiddleCenter;
                weaponTags[i]=Label("Weapon status",card.transform,"",13,new Vector2(2,-122),new Vector2(-2,-98),gold);weaponTags[i].alignment=TextAnchor.MiddleCenter;
            }
            collection.gameObject.SetActive(false);
        }
        void BuildPilot()
        {
            var root=pages[3].transform;Heading(root,"Pilot","Her geliştirme oyunda gerçek bir avantaj.");
            var hero=Panel("Scout card",root,new Vector2(20,-442),new Vector2(-20,-177),panel);
            Icon("Scout portrait",hero,art.scout,new Vector2(30,-246),new Vector2(255,-5));
            Label("Scout name",hero,"Scout Mk-II",30,new Vector2(270,-68),new Vector2(-8,-20),white);
            Label("Scout role",hero,"Küp avcısı",17,new Vector2(270,-100),new Vector2(-8,-70),muted);
            pilotStats=Label("Pilot stats",hero,"",19,new Vector2(270,-236),new Vector2(-12,-122),VoidToyPalette.CyanText);
            characterNames=new Text[3];characterLevels=new Text[3];characterUpgrades=new Button[3];string[] names={"YAŞAM ÇEKİRDEĞİ","HAREKET SİSTEMİ","TOPLAMA ALANI"};string[] badges={"CAN","HIZ","ALAN"};
            for(int i=0;i<3;i++)
            {
                float y=-466-i*117;var card=Panel("Character upgrade card "+i,root,new Vector2(20,y-105),new Vector2(-20,y),panel);
                var badge=Panel("Upgrade badge",card,new Vector2(10,-88),new Vector2(77,-16),i==0?VoidToyPalette.Health:i==1?VoidToyPalette.Cobalt:gold);
                var symbol=Label("Upgrade symbol",badge,badges[i],18,Vector2.zero,Vector2.zero,i==2?VoidToyPalette.Ink:white);Stretch(symbol.rectTransform);symbol.alignment=TextAnchor.MiddleCenter;
                characterNames[i]=Label("Upgrade name",card,names[i],18,new Vector2(87,-38),new Vector2(360,-8),white);
                characterLevels[i]=Label("Upgrade levels",card,"",16,new Vector2(87,-96),new Vector2(360,-40),muted);
                characterUpgrades[i]=Button("Buy character upgrade "+i,card,"",new Vector2(380,-80),new Vector2(-10,-24),24,true);characterUpgrades[i].GetComponent<Image>().color=gold;
            }
        }
        void BuildSettings()
        {
            var root=pages[4].transform;Heading(root,"Ayarlar","Ses, titreşim ve kare hızı.");
            var sound=Panel("Audio settings",root,new Vector2(24,-447),new Vector2(-24,-183),panel);
            Icon("Audio icon",sound,art.settings,new Vector2(12,-53),new Vector2(63,0));
            Label("Sound section",sound,"SES VE MÜZİK",23,new Vector2(75,-46),new Vector2(-20,-6),gold);
            musicSlider=Slider("Music",sound,"Müzik",-62,out musicValue);sfxSlider=Slider("SFX",sound,"Efektler",-155,out sfxValue);
            muteToggle=Toggle("Mute",root,"Sessiz mod","Tüm sesleri kapat",-470);
            hapticsToggle=Toggle("Haptics",root,"Titreşim","Darbe ve ödül geri bildirimi",-581);
            frameToggle=Toggle("Frame rate",root,"60 FPS","Kapalı: pil dostu 30 FPS",-692);
        }
        void Heading(Transform parent,string title,string subtitle)
        {
            // Calm Void: the page name is the top-bar title; only the short hint stays under it.
            var sub=Label("Page subtitle",parent,subtitle,15,new Vector2(22,-118),new Vector2(-20,-94),muted);sub.alignment=TextAnchor.MiddleLeft;
        }
        Slider Slider(string name,Transform parent,string title,float y,out Text value)
        {
            Label(name+" title",parent,title,22,new Vector2(22,y-32),new Vector2(260,y),white);
            value=Label(name+" value",parent,"",22,new Vector2(320,y-32),new Vector2(-22,y),gold);value.alignment=TextAnchor.MiddleRight;
            var rect=Rect(name+" slider",parent,new Vector2(0,1),new Vector2(1,1),new Vector2(22,y-80),new Vector2(-22,y-34));
            var hit=rect.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var track=Image("Track",rect,line);track.sprite=art.panel;track.type=UnityEngine.UI.Image.Type.Sliced;Stretch(track.rectTransform);track.rectTransform.offsetMin=new Vector2(0,15);track.rectTransform.offsetMax=new Vector2(0,-15);
            var fill=Image("Fill",rect,cyan);fill.sprite=art.panel;fill.type=UnityEngine.UI.Image.Type.Sliced;Stretch(fill.rectTransform);fill.rectTransform.offsetMin=new Vector2(0,15);fill.rectTransform.offsetMax=new Vector2(0,-15);
            var area=Rect("Handle area",rect,new Vector2(0,.5f),new Vector2(1,.5f),new Vector2(0,-19),new Vector2(0,19));
            var handle=Image("Handle",area,Color.white);handle.sprite=art.disc;handle.rectTransform.sizeDelta=new Vector2(38,0);
            var slider=rect.gameObject.AddComponent<Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.maxValue=1;slider.value=1;return slider;
        }
        Toggle Toggle(string name,Transform parent,string title,string description,float y)
        {
            var card=Panel(name+" setting",parent,new Vector2(24,y-94),new Vector2(-24,y),panel);card.GetComponent<Image>().raycastTarget=true;
            Label(name+" title",card,title,25,new Vector2(18,-43),new Vector2(360,-10),white);
            Label(name+" description",card,description,16,new Vector2(18,-80),new Vector2(365,-46),muted);
            var box=Image("Toggle background",card,line);box.sprite=art.casualButton;box.type=UnityEngine.UI.Image.Type.Sliced;box.pixelsPerUnitMultiplier=9;Top(box.rectTransform,new Vector2(370,-73),new Vector2(-14,-20));
            var check=Image("Enabled",box.transform,cyan);check.sprite=art.casualButton;check.type=UnityEngine.UI.Image.Type.Sliced;check.pixelsPerUnitMultiplier=9;Stretch(check.rectTransform);check.rectTransform.offsetMin=Vector2.one*4;check.rectTransform.offsetMax=-Vector2.one*4;
            var toggle=card.gameObject.AddComponent<Toggle>();toggle.toggleTransition=UnityEngine.UI.Toggle.ToggleTransition.None;toggle.targetGraphic=box;toggle.graphic=check;return toggle;
        }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        { var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high;return rect; }
        static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero; }
        static void Bottom(RectTransform rect,float low,float high) { var a=rect.anchorMin;var b=rect.anchorMax;a.y=b.y=0;rect.anchorMin=a;rect.anchorMax=b;var min=rect.offsetMin;var max=rect.offsetMax;min.y=low;max.y=high;rect.offsetMin=min;rect.offsetMax=max; }
        static void Top(RectTransform rect,Vector2 low,Vector2 high) { rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(high.x>0?0:1,1);rect.offsetMin=low;rect.offsetMax=high; }
        Image Image(string name,Transform parent,Color color)
        { var rect=Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image; }
        Image Icon(string name,Transform parent,Sprite sprite,Vector2 low,Vector2 high)
        { var image=Image(name,parent,Color.white);image.sprite=sprite;image.preserveAspect=true;Top(image.rectTransform,low,high);return image; }
        RectTransform Panel(string name,Transform parent,Vector2 low,Vector2 high,Color color)
        {
            var image=Image(name,parent,color);image.sprite=art.casualButton;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=12;Top(image.rectTransform,low,high);
            var shadow=image.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.005f,.06f,.12f,.65f);shadow.effectDistance=new Vector2(0,-4);
            return image.rectTransform;
        }
        Text Label(string name,Transform parent,string value,int size,Vector2 low,Vector2 high,Color color,FontStyle style=FontStyle.Normal)
        {
            var rect=Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);Top(rect,low,high);var text=rect.gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.fontStyle=FontStyle.Normal;text.color=color;text.text=value;text.raycastTarget=false;text.alignment=TextAnchor.MiddleLeft;
            var outline=rect.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.015f,.07f,.13f,.85f);outline.effectDistance=new Vector2(1.3f,-1.3f);return text;
        }
        Button Button(string name,Transform parent,string value,Vector2 low,Vector2 high,int size,bool primary=false)
        {
            var rect=Panel(name,parent,low,high,primary?cyan:panel);var image=rect.GetComponent<Image>();image.sprite=art.casualButton;image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=8;image.raycastTarget=true;
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;var colors=button.colors;colors.highlightedColor=Color.white;colors.selectedColor=Color.white;colors.pressedColor=new Color(.8f,.9f,1);colors.disabledColor=new Color(.55f,.61f,.68f);colors.fadeDuration=.08f;button.colors=colors;
            rect.gameObject.AddComponent<CasualButtonMotion>();var text=Label("Label",rect,value,size,Vector2.zero,Vector2.zero,white);Stretch(text.rectTransform);text.alignment=TextAnchor.MiddleCenter;return button;
        }
    }
}
