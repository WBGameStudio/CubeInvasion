using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
namespace CubeInvasion.Prototype
{
    public sealed class P3MetaHud : MonoBehaviour
    {
        [SerializeField] GameObject loadout, blocked;
        [SerializeField] Text gold, detail, notice, saveMessage;
        [SerializeField] Button buy, retrySave;
        [SerializeField] Button[] upgrades;
        bool frontEndMode;
        public void UseFrontEnd(bool value) { frontEndMode=value; }
        Font font;
        Tween pulse;
        readonly Color ink=new Color(.035f,.06f,.105f,1),tile=new Color(.1f,.16f,.25f),cyan=new Color(.09f,.74f,.82f);
        public void Initialize(RectTransform safeRoot)
        {
            if(loadout)return;
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            transform.SetParent(safeRoot,false);var root=(RectTransform)transform;
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            loadout=Rect("Arsenal progression",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            gold=Label("Persistent wallet",loadout.transform,"ALTIN 0",18,new Vector2(265,-66),new Vector2(-26,-25));gold.alignment=TextAnchor.MiddleRight;gold.color=new Color(1,.72f,.2f);
            detail=Label("Arsenal stats",loadout.transform,"",15,new Vector2(26,-625),new Vector2(-26,-591));
            buy=Button("Unlock weapon",loadout.transform,"SİLAHI AÇ",new Vector2(26,-719),new Vector2(-26,-637));
            upgrades=new Button[3];
            for(int i=0;i<3;i++)upgrades[i]=Button("Upgrade "+i,loadout.transform,"",new Vector2(26+i*166,-719),new Vector2(181+i*166,-637));
            notice=Label("Transaction feedback",loadout.transform,"",13,new Vector2(26,-753),new Vector2(-26,-724));notice.color=new Color(1,.72f,.2f);
            blocked=Rect("Save recovery",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero).gameObject;
            var overlay=blocked.AddComponent<Image>();overlay.color=ink;overlay.raycastTarget=true;
            Label("Save title",blocked.transform,"KAYIT BEKLİYOR",27,new Vector2(26,-340),new Vector2(-26,-280));
            saveMessage=Label("Save error",blocked.transform,"",18,new Vector2(26,-470),new Vector2(-26,-350));
            retrySave=Button("Retry save",blocked.transform,"KAYDI YENİDEN DENE",new Vector2(26,-560),new Vector2(-26,-490));
            blocked.SetActive(false);
        }
        public void Bind(Action unlock,Action<WeaponUpgrade> upgrade,Action retry)
        {
            Bind(buy,unlock);Bind(retrySave,retry);
            for(int i=0;i<3;i++) { var kind=(WeaponUpgrade)i;Bind(upgrades[i],()=>upgrade(kind)); }
        }
        static void Bind(Button button,Action action) { button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action()); }
        public void Refresh(MetaProfile profile,P3MetaSettings rules,WeaponSpec weapon,int inspected,int equipped,string phase,string message)
        {
            loadout.SetActive(phase=="Loadout"&&!frontEndMode);blocked.SetActive(phase=="SaveBlocked");saveMessage.text=message;
            if(phase!="Loadout"||frontEndMode)return;
            gold.text=$"ALTIN {profile.gold}";var state=profile.weapons[inspected];
            float dps=weapon.NominalDps*(1+state.damage*rules.damagePerLevel)*(1+state.fireRate*rules.fireRatePerLevel);
            detail.text=$"DPS {dps:0.0} · Menzil {18*weapon.projectileLife*(1+state.range*rules.rangePerLevel):0.0} m";
            notice.text=message??(profile.lastResult!=null?$"Son deneme: +{profile.lastResult.gold} altın · {profile.lastResult.cubes} küp":"Kazanılan altınla silah açabilir ve yükseltebilirsin.");
            buy.gameObject.SetActive(!state.owned);buy.interactable=profile.gold>=rules.Price(inspected);
            buy.GetComponentInChildren<Text>().text=$"{weapon.title.ToUpperInvariant()} AÇ · {rules.Price(inspected)} ALTIN";
            for(int i=0;i<3;i++)
            {
                var kind=(WeaponUpgrade)i;int level=state.Level(kind),cap=rules.Cap(kind),cost=rules.UpgradeCost(kind,level);
                upgrades[i].gameObject.SetActive(state.owned);upgrades[i].interactable=state.owned&&level<cap&&profile.gold>=cost;
                string title=i==0?"HASAR":i==1?"ATIŞ HIZI":"MENZİL";
                float from=i==0?weapon.damage*(1+level*rules.damagePerLevel):i==1?1/weapon.interval*(1+level*rules.fireRatePerLevel):18*weapon.projectileLife*(1+level*rules.rangePerLevel);
                float to=i==0?weapon.damage*(1+(level+1)*rules.damagePerLevel):i==1?1/weapon.interval*(1+(level+1)*rules.fireRatePerLevel):18*weapon.projectileLife*(1+(level+1)*rules.rangePerLevel);
                upgrades[i].GetComponentInChildren<Text>().text=$"{title} {level}/{cap}\n"+(level==cap?"MAKSİMUM":$"{from:0.0} → {to:0.0}\n{cost} ALTIN");
            }
        }
        public void PulsePurchase()
        {
            pulse?.Kill();gold.transform.localScale=Vector3.one;
            pulse=gold.transform.DOPunchScale(Vector3.one*.12f,.3f,3,.4f).SetUpdate(true).SetLink(gameObject);
        }
        void OnDestroy() { pulse?.Kill(); }
        RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high;return rect;
        }
        Text Label(string name,Transform parent,string text,int size,Vector2 low,Vector2 high)
        {
            var rect=Rect(name,parent,new Vector2(0,1),new Vector2(1,1),low,high);var label=rect.gameObject.AddComponent<Text>();label.font=font;label.fontSize=size;label.color=Color.white;label.text=text;label.alignment=TextAnchor.MiddleLeft;label.raycastTarget=false;return label;
        }
        Button Button(string name,Transform parent,string text,Vector2 low,Vector2 high)
        {
            var rect=Rect(name,parent,new Vector2(0,1),new Vector2(high.x>0?0:1,1),low,high);var image=rect.gameObject.AddComponent<Image>();image.color=tile;
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var label=Label("Label",rect,text,13,Vector2.zero,Vector2.zero);label.alignment=TextAnchor.MiddleCenter;label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;
            return button;
        }
    }
}
