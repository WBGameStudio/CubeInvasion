using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    // Shared uGUI helpers for the Void Toy Arcade look. Every helper finds before it creates,
    // so styling can be re-applied to an authored scene without duplicating objects.
    public static class VoidToyStyle
    {
        static Sprite disc,ring;

        public static void Place(RectTransform rect,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        { rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=low;rect.offsetMax=high; }

        public static void Surface(Image image,Sprite tile,Color color,float cornerScale=10)
        {
            if(!image)return;
            if(tile) {image.sprite=tile;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=cornerScale;}
            image.color=color;
        }

        public static Image Tile(string name,Transform parent,Sprite tile,Color color,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        {
            var child=parent.Find(name);
            if(!child) {child=new GameObject(name,typeof(RectTransform),typeof(Image)).transform;child.SetParent(parent,false);}
            var image=child.GetComponent<Image>();if(!image)image=child.gameObject.AddComponent<Image>();
            Surface(image,tile,color);image.raycastTarget=false;
            Place((RectTransform)child,min,max,low,high);return image;
        }

        public static Text Label(string name,Transform parent,Font font,int size,TextAnchor anchor,Color color)
        {
            var child=parent.Find(name);
            if(!child) {child=new GameObject(name,typeof(RectTransform),typeof(Text)).transform;child.SetParent(parent,false);}
            var text=child.GetComponent<Text>();if(!text)text=child.gameObject.AddComponent<Text>();
            Place(text.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            text.font=font;text.fontSize=size;text.alignment=anchor;text.color=color;text.raycastTarget=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
        }

        public static Button Button(string name,Transform parent,Sprite tile,Color color,Vector2 min,Vector2 max,Vector2 low,Vector2 high,Font font,int size,Color ink,out Text label)
        {
            var image=Tile(name,parent,tile,color,min,max,low,high);image.raycastTarget=true;image.pixelsPerUnitMultiplier=8;
            var button=image.GetComponent<UnityEngine.UI.Button>();if(!button) {button=image.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;}
            if(!image.GetComponent<CasualButtonMotion>())image.gameObject.AddComponent<CasualButtonMotion>();
            label=Label("Label",image.transform,font,size,TextAnchor.MiddleCenter,ink);Outline(label);return button;
        }
        // A soft white band that sweeps across the one primary button on a screen.
        public static void AddShine(UnityEngine.UI.Button button)
        {
            var rect=(RectTransform)button.transform;
            if(!rect.Find("Shine")) {var band=new GameObject("Shine",typeof(RectTransform),typeof(Image)).transform;band.SetParent(rect,false);band.SetSiblingIndex(0);}
            var shine=ClipToFace(rect);
            shine.anchorMin=new Vector2(0,0);shine.anchorMax=new Vector2(0,1);shine.pivot=new Vector2(.5f,.5f);shine.sizeDelta=new Vector2(70,40);shine.localRotation=Quaternion.Euler(0,0,-20);
            var image=shine.GetComponent<Image>();image.color=new Color(1,1,1,.28f);image.raycastTarget=false;shine.anchoredPosition=new Vector2(-80,0);
        }
        public static void AnimateShine(RectTransform button,GameObject owner)
        {
            if(!button.Find("Shine"))return;
            var shine=ClipToFace(button);DOTween.Kill(shine);float width=Mathf.Max(200,button.rect.width);shine.anchoredPosition=new Vector2(-80,0);
            DOTween.Sequence().Append(shine.DOAnchorPosX(width+80,1.1f).SetEase(Ease.InOutSine)).AppendInterval(1.7f)
                .SetLoops(-1,LoopType.Restart).SetUpdate(true).SetTarget(shine).SetLink(owner);
        }
        // "Shine" is a rounded clip matching the button face; the moving band is its "Band" child.
        // The button art carries a soft shadow and stray alpha around the face, so neither a RectMask2D
        // nor a stencil on the button sprite keeps the band inside. Older scenes kept the band as "Shine"
        // under a RectMask2D; they are converted in place, and callers still toggle "Shine" as before.
        static RectTransform ClipToFace(RectTransform button)
        {
            var square=button.GetComponent<RectMask2D>();if(square)Object.DestroyImmediate(square);
            var clip=(RectTransform)button.Find("Shine");var band=clip.Find("Band") as RectTransform;
            if(!band)
            {
                band=clip;band.name="Band";
                clip=new GameObject("Shine",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
                clip.SetParent(button,false);clip.SetSiblingIndex(band.GetSiblingIndex());clip.gameObject.SetActive(band.gameObject.activeSelf);
                band.SetParent(clip,false);band.gameObject.SetActive(true);
            }
            Place(clip,Vector2.zero,Vector2.one,new Vector2(8.5f,8.5f),new Vector2(-8.5f,-8.5f));
            var face=clip.GetComponent<Image>();face.sprite=RoundRect();face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=1.75f;face.color=Color.white;face.raycastTarget=false;
            var mask=clip.GetComponent<Mask>();if(!mask)mask=clip.gameObject.AddComponent<Mask>();mask.showMaskGraphic=false;
            return band;
        }
        static Sprite roundRect;
        static Sprite RoundRect() { if(!roundRect)roundRect=Resources.Load<Sprite>("VoidToyRoundRect");return roundRect; }
        public static void Ink(Text text,Font font,Color color) { if(!text)return;if(font)text.font=font;text.color=color;Outline(text); }

        // Thin dark contour keeps labels readable over the bright arena floor.
        public static void Outline(Text text)
        {
            var outline=text.GetComponent<Outline>();if(!outline)outline=text.gameObject.AddComponent<Outline>();
            bool onLight=text.color.grayscale<.35f;
            outline.enabled=!onLight;outline.effectColor=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.85f);outline.effectDistance=new Vector2(1.2f,-1.2f);
        }

        // Imported sprites first, so scene references survive a save; the generated circle is only a fallback.
        public static Sprite Disc() { if(!disc)disc=Resources.Load<Sprite>("VoidToyDisc");if(!disc)disc=Circle("Void Toy disc",0);return disc; }
        public static Sprite Ring() { if(!ring)ring=Resources.Load<Sprite>("VoidToyRing");if(!ring)ring=Circle("Void Toy ring",.06f);return ring; }

        static Sprite Circle(string name,float thickness)
        {
            const int size=256;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false) {name=name,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            var pixels=new Color32[size*size];float radius=size*.5f-2;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float d=new Vector2(x-size*.5f+.5f,y-size*.5f+.5f).magnitude;
                float outer=Mathf.Clamp01(radius-d+.5f);
                float inner=thickness>0?Mathf.Clamp01(d-(radius-thickness*size)+.5f):1;
                pixels[y*size+x]=new Color32(255,255,255,(byte)(255*outer*inner));
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100);sprite.name=name;sprite.hideFlags=HideFlags.DontSave;return sprite;
        }

        // Maps colors authored by the earlier teal/green casual pass onto the Void Toy palette.
        // Whites, icons and gold accents are left alone; primary green turns into the cyan action color.
        public static Color Remap(Color c)
        {
            if(c.a<.05f||IsPalette(c))return c;
            Color.RGBToHSV(c,out float h,out float s,out float v);h*=360;
            Color mapped=c;
            if(s<.18f)return c;
            if(h>=80&&h<=150&&s>.5f&&v>.5f)mapped=VoidToyPalette.Cyan;
            else if(h>=175&&h<=230)mapped=v<.3f?VoidToyPalette.Void:v<.62f?VoidToyPalette.Panel:VoidToyPalette.Raised;
            else if(h>=15&&h<=40&&s>.75f&&v>.85f&&c.g<.6f)mapped=VoidToyPalette.Raised;  // orange secondary buttons
            mapped.a=c.a;return mapped;
        }

        public static void Restyle(Transform root,Font font)
        {
            foreach(var image in root.GetComponentsInChildren<Image>(true))
            {
                if(image.sprite&&image.color==Color.white)continue;  // icons and pictures keep their art
                image.color=Remap(image.color);
            }
            foreach(var text in root.GetComponentsInChildren<Text>(true))
            {
                if(font&&text.font!=font)text.font=font;
                var surface=text.GetComponentInParent<Button>(true)?text.GetComponentInParent<Button>(true).GetComponent<Image>():null;
                bool onCyan=surface&&(Approximately(surface.color,VoidToyPalette.Cyan)||Approximately(surface.color,VoidToyPalette.Gold));
                if(onCyan)text.color=VoidToyPalette.Ink;
                else if(Approximately(text.color,new Color(.75f,.91f,1)))text.color=VoidToyPalette.Muted;
                else if(text.color.r>.95f&&text.color.g>.7f&&text.color.b<.3f)text.color=VoidToyPalette.Gold;
                Outline(text);
            }
        }

        // Already-mapped colors stay put, so Restyle can run on an authored scene and again at runtime.
        static readonly Color[] palette={VoidToyPalette.Void,VoidToyPalette.Panel,VoidToyPalette.Raised,VoidToyPalette.Track,VoidToyPalette.Cyan,VoidToyPalette.CyanText,
            VoidToyPalette.Cobalt,VoidToyPalette.CobaltText,VoidToyPalette.Gold,VoidToyPalette.Danger,VoidToyPalette.DangerText,VoidToyPalette.Health,VoidToyPalette.HealthText,
            VoidToyPalette.Warm,VoidToyPalette.Muted,VoidToyPalette.Ink,VoidToyPalette.Text};
        static bool IsPalette(Color c) { foreach(var p in palette)if(Approximately(c,p))return true;return false; }
        public static bool Approximately(Color a,Color b) => Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)<.06f;
    }
}
