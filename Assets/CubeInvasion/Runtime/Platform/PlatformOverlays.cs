using System;
using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    // Runtime-built layers over the whole game UI: the start prompt, the ad wait screen and the continue offer.
    public sealed class PlatformOverlays : MonoBehaviour
    {
        GameObject wait,revive,promptRoot;
        Text prompt;
        Font font;
        Sprite tile;
        Action accept,decline;
        public static PlatformOverlays Create(RectTransform parent)
        {
            var root=new GameObject("Platform overlays",typeof(RectTransform)).AddComponent<PlatformOverlays>();
            var rect=(RectTransform)root.transform;rect.SetParent(parent,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            root.Build();return root;
        }
        void Build()
        {
            var art=Resources.Load<FrontEndArt>("FrontEndArt");
            tile=art?art.casualButton:null;font=art&&art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            // Sits low on the screen so the hero and the first enemies stay visible; lets every touch through to the stick.
            var pill=VoidToyStyle.Tile("Start prompt",transform,tile,VoidToyPalette.WithAlpha(VoidToyPalette.Void,.82f),new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-270,150),new Vector2(270,262));
            pill.raycastTarget=false;promptRoot=pill.gameObject;
            prompt=VoidToyStyle.Label("Start prompt text",pill.transform,font,26,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            prompt.raycastTarget=false;promptRoot.SetActive(false);

            var waitDim=Dim("Ad wait",.62f);wait=waitDim.gameObject;
            var waitLabel=VoidToyStyle.Label("Ad wait text",wait.transform,font,28,TextAnchor.MiddleCenter,VoidToyPalette.Text);waitLabel.text="Loading ad…";waitLabel.raycastTarget=false;
            wait.SetActive(false);

            var reviveDim=Dim("Continue offer",.86f);revive=reviveDim.gameObject;
            var title=Line("Continue title","One more try?",52,new Vector2(-260,70),new Vector2(260,150));title.color=VoidToyPalette.Text;
            var detail=Line("Continue detail","Keep this cube with 60% HP and at least 10 s",22,new Vector2(-260,18),new Vector2(260,66));detail.color=VoidToyPalette.Muted;
            Action2("Continue ad","WATCH AD: CONTINUE",true,new Vector2(-230,-76),new Vector2(230,-4),()=>accept?.Invoke());
            Action2("Continue decline","No thanks",false,new Vector2(-150,-146),new Vector2(150,-92),()=>decline?.Invoke());
            revive.SetActive(false);
        }
        Image Dim(string name,float alpha)
        {
            var image=VoidToyStyle.Tile(name,transform,null,VoidToyPalette.WithAlpha(VoidToyPalette.Void,alpha),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            image.raycastTarget=true;return image;
        }
        Text Line(string name,string value,int size,Vector2 low,Vector2 high)
        {
            var holder=VoidToyStyle.Tile(name,revive.transform,null,Color.clear,new Vector2(.5f,.5f),new Vector2(.5f,.5f),low,high);holder.raycastTarget=false;
            var label=VoidToyStyle.Label(name+" text",holder.transform,font,size,TextAnchor.MiddleCenter,VoidToyPalette.Text);label.text=value;label.raycastTarget=false;return label;
        }
        void Action2(string name,string value,bool primary,Vector2 low,Vector2 high,Action action)
        {
            var image=VoidToyStyle.Tile(name,revive.transform,primary?tile:null,primary?VoidToyPalette.Gold:Color.clear,new Vector2(.5f,.5f),new Vector2(.5f,.5f),low,high);image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>action());
            image.gameObject.AddComponent<CasualButtonMotion>();
            var label=VoidToyStyle.Label(name+" text",image.transform,font,primary?26:22,TextAnchor.MiddleCenter,primary?VoidToyPalette.Ink:VoidToyPalette.Muted);label.text=value;label.raycastTarget=false;
        }
        public void SetPrompt(string text)
        {
            bool show=!string.IsNullOrEmpty(text);
            if(promptRoot.activeSelf!=show)promptRoot.SetActive(show);
            if(!show)return;
            if(prompt.text!=text)prompt.text=text;
            var color=prompt.color;color.a=Mathf.Lerp(.72f,1,Mathf.PingPong(Time.unscaledTime*1.6f,1));prompt.color=color;
            transform.SetAsLastSibling();
        }
        public void SetWait(bool show)
        {
            if(wait.activeSelf==show)return;wait.SetActive(show);
            if(show) { transform.SetAsLastSibling();wait.transform.SetAsLastSibling(); }
        }
        public void ShowRevive(Action yes,Action no)
        {
            accept=yes;decline=no;revive.SetActive(true);
            transform.SetAsLastSibling();revive.transform.SetSiblingIndex(Mathf.Max(0,wait.transform.GetSiblingIndex()));wait.transform.SetAsLastSibling();
        }
        public void HideRevive() { revive.SetActive(false); }
    }
}
