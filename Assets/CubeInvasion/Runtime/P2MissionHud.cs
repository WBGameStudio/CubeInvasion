using System;
using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    // Authored in the scene by P2ContentBuilder; callbacks and text are runtime data.
    public sealed class P2MissionHud : MonoBehaviour
    {
        [SerializeField] GameObject loadout, result;
        [SerializeField] Text missionTitle, missionDescription, weaponDescription, resultTitle, resultDescription;
        [SerializeField] Button previous, next, deploy, retry, advance, menu;
        [SerializeField] Button[] weapons;
        [SerializeField] Text[] weaponLabels;
        [SerializeField] GameObject bossPanel;
        [SerializeField] Image bossFill;
        [SerializeField] Text bossLabel;
        bool metaMode,frontEndMode;
        public void UseFrontEnd(bool value) { frontEndMode=value; }
        Font font;
        readonly Color ink=new Color(.035f,.06f,.105f,1), cyan=new Color(.09f,.74f,.82f), tile=new Color(.1f,.16f,.25f);
        public void Initialize(RectTransform parent)
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(loadout) { BuildBossPanel();ApplyVoidToyStyle();return; }
            transform.SetParent(parent,false);
            var root=(RectTransform)transform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            loadout=Panel("Mission loadout",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,ink).gameObject;
            Label("Brand",loadout.transform,"CUBE / INVASION",20,new Vector2(26,-66),new Vector2(-26,-25),cyan);
            Label("Chapter",loadout.transform,"GÖREV SEÇİMİ",13,new Vector2(26,-109),new Vector2(-26,-82),Color.white);
            missionTitle=Label("Mission title",loadout.transform,"",29,new Vector2(26,-166),new Vector2(-26,-113),Color.white);
            missionDescription=Label("Mission description",loadout.transform,"",17,new Vector2(26,-270),new Vector2(-26,-175),new Color(.72f,.8f,.9f));
            previous=MakeButton("Previous mission",loadout.transform,"‹  ÖNCEKİ",new Vector2(26,-325),new Vector2(258,-279));
            next=MakeButton("Next mission",loadout.transform,"SONRAKİ  ›",new Vector2(282,-325),new Vector2(-26,-279));
            Label("Arsenal",loadout.transform,"SİLAHINI SEÇ",14,new Vector2(26,-376),new Vector2(-26,-345),Color.white);
            string[] names={"PULSE PISTOL","TWIN BLASTERS","SCATTER SHOT","RAPID SMG"};
            weapons=new Button[4];weaponLabels=new Text[4];
            for(int i=0;i<4;i++)
            {
                float left=i%2==0?26:282, right=i%2==0?258:-26, y=-390-i/2*65;
                weapons[i]=MakeButton("Weapon "+i,loadout.transform,names[i],new Vector2(left,y-54),new Vector2(right,y));
                weaponLabels[i]=weapons[i].GetComponentInChildren<Text>();
            }
            weaponDescription=Label("Weapon description",loadout.transform,"",17,new Vector2(26,-632),new Vector2(-26,-530),new Color(.72f,.8f,.9f));
            Label("Controls",loadout.transform,"Joystick ile hareket et. En yakın hedefe otomatik ateş.\nKüp temizlenince güçlendirme kapısından geç.",16,new Vector2(26,-737),new Vector2(-26,-647),Color.white);
            deploy=MakeButton("Deploy",loadout.transform,"GÖREVE BAŞLA",new Vector2(26,-833),new Vector2(-26,-759));
            result=Panel("Mission result",transform,new Vector2(0,.22f),new Vector2(1,.78f),new Vector2(22,0),new Vector2(-22,0),ink).gameObject;
            resultTitle=Label("Result title",result.transform,"",27,new Vector2(20,-80),new Vector2(-20,-20),Color.white);
            resultDescription=Label("Result detail",result.transform,"",17,new Vector2(20,-160),new Vector2(-20,-86),new Color(.72f,.8f,.9f));
            advance=MakeButton("Advance",result.transform,"SONRAKİ GÖREV",new Vector2(24,-228),new Vector2(-24,-172));
            retry=MakeButton("Retry",result.transform,"TEKRAR DENE",new Vector2(24,-296),new Vector2(-24,-240));
            menu=MakeButton("Menu",result.transform,"GÖREV / SİLAH SEÇ",new Vector2(24,-364),new Vector2(-24,-308));
            loadout.SetActive(true);result.SetActive(false);BuildBossPanel();ApplyVoidToyStyle();
        }
        [SerializeField] Image resultEmblem;
        Image reasonChip,resultIcon;Text reasonLabel;GameObject resultCheck,resultShards;bool shownEnded;
        [SerializeField] Sprite victoryEmblem,failureEmblem;
        public void ConfigureResultEmblem(Image image,Sprite won,Sprite failed) { resultEmblem=image;victoryEmblem=won;failureEmblem=failed; }
        public void SetMetaMode(bool enabled)
        {
            metaMode=enabled;
            var controls=loadout.transform.Find("Controls");if(controls)controls.gameObject.SetActive(!enabled);
        }
        public void SetProgression(MetaProfile profile,P3MetaSettings settings,int inspected,int mission)
        {
            bool lockedMission=profile.highestMission<mission;
            if(lockedMission)missionDescription.text="KİLİTLİ GÖREV · Önce önceki görevi tamamla.\n"+missionDescription.text;
            deploy.interactable=!lockedMission&&profile.weapons[inspected].owned;
            string[] names={"PULSE PISTOL","TWIN BLASTERS","SCATTER SHOT","RAPID SMG"};
            for(int i=0;i<weapons.Length;i++)
            {
                weaponLabels[i].text=names[i]+(profile.weapons[i].owned?i==profile.selectedWeapon?"\nSEÇİLİ":"\nAÇIK":$"\n{settings.Price(i)} ALTIN");
                weapons[i].GetComponent<Image>().color=i==inspected?cyan:tile;
                weaponLabels[i].color=i==inspected?ink:Color.white;
            }
            if(profile.run!=null&&profile.run.ended)
            {
                var run=profile.run;
                // Calm summary: what this run earned, in one line. The reason already sits in red above the title.
                resultDescription.text=run.won?$"+{VoidToyPalette.FormatGold(run.gold)} altın · {run.cubes} küp · {run.kills} hedef":$"{run.cubes} küp temizlendi · {VoidToyPalette.FormatGold(run.gold)} altın korundu";
            }
        }
        void BuildBossPanel()
        {
            if(bossPanel)return;
            var track=Panel("Crusher health",transform,new Vector2(0,1),new Vector2(1,1),new Vector2(28,-270),new Vector2(-28,-238),ink);
            bossPanel=track.gameObject;
            bossFill=Panel("Boss remaining health",track,Vector2.zero,Vector2.one,new Vector2(3,3),new Vector2(-3,-3),new Color(.9f,.25f,.36f)).GetComponent<Image>();
            bossLabel=Label("Boss phase",track,"THE CRUSHER",13,Vector2.zero,Vector2.zero,Color.white);
            bossLabel.alignment=TextAnchor.MiddleCenter;bossLabel.rectTransform.anchorMin=Vector2.zero;bossLabel.rectTransform.anchorMax=Vector2.one;
            bossPanel.SetActive(false);
        }
        public void SetBoss(float health,float total,bool visible,int phase)
        {
            if(!bossPanel)return;bossPanel.SetActive(visible);
            if(!visible)return;
            bossFill.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(health/Mathf.Max(1,total)),1);
            bossLabel.text=$"THE CRUSHER · {Mathf.CeilToInt(health)}/{total:0} · "+(phase==1?"ZIRH":phase==2?"SİLAH SİSTEMİ":"ÇEKİRDEK");
        }
        public void SetBossCaption(string caption) {if(bossLabel)bossLabel.text=caption;}
        public void Bind(Action<int> mission,Action<int> weapon,Action start,Action restart,Action following,Action select)
        {
            Bind(previous,()=>mission(-1));Bind(next,()=>mission(1));Bind(deploy,start);Bind(retry,restart);Bind(advance,following);Bind(menu,select);
            for(int i=0;i<weapons.Length;i++) { int index=i;Bind(weapons[i],()=>weapon(index)); }
        }
        static void Bind(Button button,Action action) { button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action()); }
        public void SetObjectiveDescription(string text) {if(missionDescription&&missionDescription.text!=text)missionDescription.text=text;}
        public void Refresh(MissionSpec mission,WeaponSpec weapon,int missionIndex,int count,int weaponIndex,string phase,string reason,int wallet)
        {
            bool choosing=phase=="Loadout",won=phase=="Won",ended=won||phase=="Failed";
            loadout.SetActive(choosing&&!frontEndMode);result.SetActive(ended);
            if(choosing&&!frontEndMode)
            {
                missionTitle.text=$"{missionIndex+1:00} / {mission.title}";
                string mode=mission.mode==MissionMode.Boss?"3 FAZLI BOSS":mission.mode==MissionMode.GoldRush?"ALTIN KOTASI":"SAVAŞ";
                missionDescription.text=$"{mode} · {mission.cubes.Length} KÜP · HER KÜP 20 sn\n{mission.description}";
                weaponDescription.text=weapon.title+"\n"+weapon.description;
                for(int i=0;i<weapons.Length;i++) { weapons[i].GetComponent<Image>().color=i==weaponIndex?cyan:tile;weaponLabels[i].color=i==weaponIndex?ink:Color.white; }
            }
            if(!ended)shownEnded=false;
            if(ended)
            {
                bool timeout=!won&&reason!=null&&reason.Contains("saniye"),fell=!won&&reason!=null&&reason.Contains("düştün");
                resultTitle.text=won?"Temizlendi":timeout?"Küp çöktü":fell?"Düştün":"Can bitti";resultTitle.color=VoidToyPalette.Text;
                if(reasonLabel) {reasonLabel.text=won?$"Bölüm {missionIndex+1}":timeout?"Süre doldu":fell?"Kenardan düştün":"Canın tükendi";reasonLabel.color=won?VoidToyPalette.Muted:VoidToyPalette.DangerText;}
                if(resultIcon) {resultIcon.gameObject.SetActive(won);resultCheck.SetActive(won);}
                if(resultShards)resultShards.SetActive(!won);
                // One cyan primary per state: advance after a win, retry after a fail.
                bool canAdvance=won&&missionIndex<count-1;
                Paint(advance,true);Paint(retry,!canAdvance);Paint(menu,false);
                if(!shownEnded&&Application.isPlaying)
                {
                    if(advance)VoidToyStyle.AnimateShine((RectTransform)advance.transform,gameObject);
                    if(retry&&!canAdvance)VoidToyStyle.AnimateShine((RectTransform)retry.transform,gameObject);
                }
                if(retry) {var shine=retry.transform.Find("Shine");if(shine)shine.gameObject.SetActive(!canAdvance);}
                if(resultEmblem)resultEmblem.sprite=won?victoryEmblem:failureEmblem;
                resultDescription.text=won?mission.title+$"\nBankalanan altın: {wallet}":reason+"\nTamamlanan küplerin altını korundu.";
                advance.gameObject.SetActive(won&&missionIndex<count-1);
                retry.GetComponentInChildren<Text>().text=won?"Yeniden oyna":"TEKRAR";shownEnded=true;
                if(menu)menu.GetComponentInChildren<Text>().text="Yola dön";
                if(won&&missionIndex==count-1&&!metaMode)resultDescription.text+="\nTüm görevler tamamlandı.";
            }
        }
        public void ApplyVoidToyStyle()
        {
            var art=Resources.Load<FrontEndArt>("FrontEndArt");Sprite tile=art?art.casualButton:null;
            var modal=result?result.GetComponent<Image>():null;
            if(modal)
            {
                // Calm Void result (Sade UI 06–07): a full screen, not a card. One symbol, a title, one muted line, one shining action.
                var page=(RectTransform)result.transform;page.anchorMin=Vector2.zero;page.anchorMax=Vector2.one;page.pivot=new Vector2(.5f,.5f);page.offsetMin=page.offsetMax=Vector2.zero;
                modal.sprite=null;modal.type=Image.Type.Simple;modal.color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.97f);
                var font=resultTitle?resultTitle.font:null;
                if(resultEmblem)resultEmblem.gameObject.SetActive(false);
                resultIcon=VoidToyStyle.Tile("Result icon",result.transform,tile,VoidToyPalette.Health,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-64,-300),new Vector2(64,-172));resultIcon.pixelsPerUnitMultiplier=3;
                // Check mark from two strokes; collapse as two tilted halves of a cube.
                resultCheck=(VoidToyStyle.Tile("Result check",resultIcon.transform,null,Color.clear,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero)).gameObject;
                var shortStroke=VoidToyStyle.Tile("Check short",resultCheck.transform,tile,VoidToyPalette.Ink,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-34,-8),new Vector2(-2,4));shortStroke.pixelsPerUnitMultiplier=40;shortStroke.rectTransform.localRotation=Quaternion.Euler(0,0,-45);shortStroke.rectTransform.anchoredPosition=new Vector2(-16,-6);
                var longStroke=VoidToyStyle.Tile("Check long",resultCheck.transform,tile,VoidToyPalette.Ink,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-30,-6),new Vector2(30,6));longStroke.pixelsPerUnitMultiplier=40;longStroke.rectTransform.localRotation=Quaternion.Euler(0,0,45);longStroke.rectTransform.anchoredPosition=new Vector2(10,4);
                resultShards=(VoidToyStyle.Tile("Result shards",result.transform,null,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-80,-320),new Vector2(80,-160))).gameObject;
                foreach(var side in new[]{-1,1})
                {
                    var half=VoidToyStyle.Tile(side<0?"Shard left":"Shard right",resultShards.transform,tile,VoidToyPalette.DangerDeep,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-34,-70),new Vector2(34,70));
                    half.pixelsPerUnitMultiplier=12;half.rectTransform.anchoredPosition=new Vector2(side*40,side<0?-6:-18);half.rectTransform.localRotation=Quaternion.Euler(0,0,side<0?8:-6);
                    var edge=half.GetComponent<Outline>()??half.gameObject.AddComponent<Outline>();edge.effectColor=VoidToyPalette.Danger;edge.effectDistance=new Vector2(2,-2);
                }
                reasonChip=VoidToyStyle.Tile("Reason chip",result.transform,tile,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-220,-360),new Vector2(220,-326));
                reasonLabel=VoidToyStyle.Label("Reason",reasonChip.transform,font,19,TextAnchor.MiddleCenter,VoidToyPalette.Muted);
                var summary=VoidToyStyle.Tile("Result summary",result.transform,tile,Color.clear,new Vector2(0,1),new Vector2(1,1),new Vector2(24,-292),new Vector2(-24,-170));
                summary.transform.SetAsFirstSibling();
                if(resultTitle) {VoidToyStyle.Place(resultTitle.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-420),new Vector2(-16,-360));resultTitle.fontSize=48;resultTitle.alignment=TextAnchor.MiddleCenter;}
                if(resultDescription) {VoidToyStyle.Place(resultDescription.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(48,-560),new Vector2(-48,-436));resultDescription.fontSize=19;resultDescription.alignment=TextAnchor.UpperCenter;}
                var actions=result.transform.Find("Result actions") as RectTransform;
                if(actions) {actions.anchorMin=Vector2.zero;actions.anchorMax=new Vector2(1,0);actions.offsetMin=new Vector2(32,40);actions.offsetMax=new Vector2(-32,260);}
                if(advance)VoidToyStyle.AddShine(advance);if(retry)VoidToyStyle.AddShine(retry);
            }
            var dim=transform.Find("Result dim");if(dim)dim.GetComponent<Image>().color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.8f);
            if(resultDescription)resultDescription.color=VoidToyPalette.Muted;
            if(bossPanel)
            {
                // Boss bar hangs right under the objective pill, off the arena and the gates.
                var bar=(RectTransform)bossPanel.transform;bar.anchorMin=bar.anchorMax=bar.pivot=new Vector2(.5f,1);bar.sizeDelta=new Vector2(312,24);bar.anchoredPosition=new Vector2(0,-186);
                VoidToyStyle.Surface(bossPanel.GetComponent<Image>(),tile,VoidToyPalette.WithAlpha(VoidToyPalette.Void,.92f),20);
                VoidToyStyle.Surface(bossFill,tile,VoidToyPalette.Danger,20);
                if(bossLabel) {bossLabel.fontSize=14;bossLabel.resizeTextForBestFit=true;bossLabel.resizeTextMinSize=10;bossLabel.resizeTextMaxSize=14;bossLabel.color=VoidToyPalette.Text;}
            }
            foreach(var text in GetComponentsInChildren<Text>(true))VoidToyStyle.Outline(text);
        }
        static void Paint(Button button,bool primary)
        {
            if(!button)return;
            // Primary: cyan with shine. Secondary: no surface, a quiet text link.
            var image=button.GetComponent<Image>();if(image)image.color=primary?VoidToyPalette.Cyan:Color.clear;
            var label=button.GetComponentInChildren<Text>(true);
            if(label) {label.color=primary?VoidToyPalette.Ink:VoidToyPalette.Muted;VoidToyStyle.Outline(label);}
        }
        RectTransform Panel(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high,Color color)
        {
            var rect=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high;
            var image=rect.GetComponent<Image>();image.color=color;image.raycastTarget=true;return rect;
        }
        Text Label(string name,Transform parent,string text,int size,Vector2 low,Vector2 high,Color color)
        {
            var rect=new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);rect.offsetMin=low;rect.offsetMax=high;
            var label=rect.GetComponent<Text>();label.font=font;label.text=text;label.fontSize=size;label.color=color;label.raycastTarget=false;label.alignment=TextAnchor.MiddleLeft;
            return label;
        }
        Button MakeButton(string name,Transform parent,string text,Vector2 low,Vector2 high)
        {
            // Positive right coordinates are measured from the left, negative ones from the right.
            Vector2 min=new Vector2(0,1),max=new Vector2(high.x>0?0:1,1);
            var rect=Panel(name,parent,min,max,low,high,tile);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=rect.GetComponent<Image>();
            var label=Label("Label",rect,text,16,Vector2.zero,Vector2.zero,Color.white);label.alignment=TextAnchor.MiddleCenter;label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
            return button;
        }
    }
}
