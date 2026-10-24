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
        Font font;
        readonly Color ink=new Color(.035f,.06f,.105f,1), cyan=new Color(.09f,.74f,.82f), tile=new Color(.1f,.16f,.25f);
        public void Initialize(RectTransform parent)
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(loadout) { BuildBossPanel();return; }
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
            loadout.SetActive(true);result.SetActive(false);BuildBossPanel();
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
        public void Bind(Action<int> mission,Action<int> weapon,Action start,Action restart,Action following,Action select)
        {
            Bind(previous,()=>mission(-1));Bind(next,()=>mission(1));Bind(deploy,start);Bind(retry,restart);Bind(advance,following);Bind(menu,select);
            for(int i=0;i<weapons.Length;i++) { int index=i;Bind(weapons[i],()=>weapon(index)); }
        }
        static void Bind(Button button,Action action) { button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action()); }
        public void Refresh(MissionSpec mission,WeaponSpec weapon,int missionIndex,int count,int weaponIndex,string phase,string reason,int wallet)
        {
            bool choosing=phase=="Loadout",won=phase=="Won",ended=won||phase=="Failed";
            loadout.SetActive(choosing);result.SetActive(ended);
            if(choosing)
            {
                missionTitle.text=$"{missionIndex+1:00} / {mission.title}";
                string mode=mission.mode==MissionMode.Boss?"3 FAZLI BOSS":mission.mode==MissionMode.GoldRush?"ALTIN KOTASI":"SAVAŞ";
                missionDescription.text=$"{mode} · {mission.cubes.Length} KÜP · HER KÜP 20 sn\n{mission.description}";
                weaponDescription.text=weapon.title+"\n"+weapon.description;
                for(int i=0;i<weapons.Length;i++) { weapons[i].GetComponent<Image>().color=i==weaponIndex?cyan:tile;weaponLabels[i].color=i==weaponIndex?ink:Color.white; }
            }
            if(ended)
            {
                resultTitle.text=won?"GÖREV TAMAMLANDI":"GÖREV BAŞARISIZ";
                resultDescription.text=won?mission.title+$"\nBankalanan altın: {wallet}":reason+"\nTamamlanan küplerin altını korundu.";
                advance.gameObject.SetActive(won&&missionIndex<count-1);
                retry.GetComponentInChildren<Text>().text=won?"YENİDEN OYNA":"TEKRAR DENE";
                if(won&&missionIndex==count-1)resultDescription.text+="\nP2 görevlerinin sonuna ulaştın.";
            }
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
