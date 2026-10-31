using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    // Display data for one walking-gate route. Cards inform only; the gate itself is the choice.
    public struct BranchCard { public string direction,title,detail; public Color accent; public int risk; }

    // Calm Void HUD (Sade UI canvas 04–05, 08): a boxless timer with a thin time line and one objective line,
    // cube progress as dots, gold only as a floating +N, health over the player and small gate labels.
    // Applied on every Initialize, so it is idempotent.
    public sealed partial class MobileCombatHud
    {
        [SerializeField] Text objectiveText;
        [SerializeField] Image timerTile,timerHalo,objectivePill;
        RectTransform healthTrack;
        Action pauseAction,muteAction;
        Text pauseInfo,pauseSound,pauseTime,goldFloater;
        Image joystickZone;
        Image[] cubeDots;
        int cubeTotal=3,lastGoldTotal=-1;
        Camera sceneCamera;
        Vector3 playerAnchor;bool hasPlayerAnchor;
        static readonly Color ChoiceTile=VoidToyPalette.Hex(0x0E3B39);

        sealed class CardView { public RectTransform root; public Image border,icon; public Text direction,title,detail; public Image[] pips; }
        RectTransform branchCards;
        CardView rightCard,forwardCard;

        public void ApplyVoidToyStyle(Camera camera)
        {
            sceneCamera=camera;
            var art=Resources.Load<FrontEndArt>("FrontEndArt");
            var uiFont=art&&art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            font=uiFont;
            Sprite tile=art?art.casualButton:null;
            var header=SafeRoot.Find("Combat header") as RectTransform;
            if(header)
            {
                VoidToyStyle.Place(header,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-200),Vector2.zero);
                var image=header.GetComponent<Image>();if(image) {image.color=Color.clear;image.raycastTarget=false;}
            }
            Transform band=header?header:(Transform)SafeRoot;

            var stageChip=VoidToyStyle.Tile("Objective tile",band,tile,Color.clear,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-78),new Vector2(170,-16));
            Fit(status,(RectTransform)stageChip.transform,new Vector2(14,4),new Vector2(-8,-4),22,TextAnchor.MiddleLeft,VoidToyPalette.Text);
            status.enabled=false;
            // Cube progress reads as dots: done green, current a wider white bar, upcoming faint.
            cubeDots=new Image[6];
            for(int i=0;i<6;i++)cubeDots[i]=VoidToyStyle.Tile("Cube dot "+i,stageChip.transform,tile,Color.clear,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(8+i*16,-6),new Vector2(20+i*16,6));
            foreach(var dot in cubeDots)dot.pixelsPerUnitMultiplier=40;

            timerHalo=VoidToyStyle.Tile("Timer halo",band,tile,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-92,-132),new Vector2(92,-8));
            timerTile=VoidToyStyle.Tile("Timer tile",band,tile,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-96,-124),new Vector2(96,-12));
            Fit(timer,(RectTransform)timerTile.transform,new Vector2(0,20),new Vector2(0,0),76,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            var caption=timerTile.transform.Find("Time caption");if(caption)caption.gameObject.SetActive(false);
            var track=(RectTransform)timerFill.transform.parent;track.SetParent(timerTile.transform,false);
            VoidToyStyle.Place(track,Vector2.zero,new Vector2(1,0),new Vector2(36,8),new Vector2(-36,13));
            VoidToyStyle.Surface(track.GetComponent<Image>(),tile,VoidToyPalette.WithAlpha(VoidToyPalette.Text,.2f),60);VoidToyStyle.Surface(timerFill,tile,VoidToyPalette.Text,60);

            objectivePill=VoidToyStyle.Tile("Objective pill",band,tile,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-170,-166),new Vector2(170,-128));
            if(!objectiveText)objectiveText=VoidToyStyle.Label("Objective",objectivePill.transform,uiFont,22,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            Fit(objectiveText,(RectTransform)objectivePill.transform,new Vector2(12,0),new Vector2(-12,0),22,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            // Long boss/Gold Rush objectives shrink onto one line instead of spilling out of the pill.
            objectiveText.verticalOverflow=VerticalWrapMode.Truncate;objectiveText.resizeTextForBestFit=true;objectiveText.resizeTextMinSize=12;objectiveText.resizeTextMaxSize=21;

            var goldChip=VoidToyStyle.Tile("Gold chip",band,tile,VoidToyPalette.Glass,new Vector2(1,1),new Vector2(1,1),new Vector2(-180,-76),new Vector2(-86,-16));
            var coin=goldChip.transform.Find("Coin") as RectTransform;
            if(!coin) {coin=new GameObject("Coin",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();coin.SetParent(goldChip.transform,false);}
            VoidToyStyle.Place(coin,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(8,-18),new Vector2(44,18));
            var coinImage=coin.GetComponent<Image>();coinImage.sprite=art?art.coin:null;coinImage.preserveAspect=true;coinImage.raycastTarget=false;coinImage.color=art&&art.coin?Color.white:VoidToyPalette.Gold;
            Fit(gold,(RectTransform)goldChip.transform,new Vector2(42,0),new Vector2(-10,0),22,TextAnchor.MiddleRight,VoidToyPalette.Gold);
            // Wallet plus pending gold stays on one line; best fit shrinks it inside the chip.
            gold.horizontalOverflow=HorizontalWrapMode.Wrap;gold.resizeTextForBestFit=true;gold.resizeTextMinSize=12;gold.resizeTextMaxSize=22;gold.verticalOverflow=VerticalWrapMode.Truncate;
            // Gold stays out of combat; it only appears as a floating +N when something is picked up.
            goldChip.gameObject.SetActive(false);
            goldFloater=VoidToyStyle.Label("Gold floater",SafeRoot,uiFont,26,TextAnchor.MiddleRight,VoidToyPalette.Gold);
            VoidToyStyle.Place(goldFloater.rectTransform,new Vector2(1,1),new Vector2(1,1),new Vector2(-220,-124),new Vector2(-22,-88));goldFloater.text="";

            var pause=pauseText.GetComponentInParent<Button>(true).GetComponent<RectTransform>();pause.SetParent(SafeRoot,false);
            VoidToyStyle.Place(pause,Vector2.one,Vector2.one,new Vector2(-76,-76),new Vector2(-16,-16));
            VoidToyStyle.Surface(pause.GetComponent<Image>(),tile,VoidToyPalette.WithAlpha(VoidToyPalette.Void,.5f));
            Fit(pauseText,pause,Vector2.zero,Vector2.zero,26,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            // Sound lives in Settings; the combat screen keeps only pause in the corner.
            if(soundText)soundText.GetComponentInParent<Button>(true).gameObject.SetActive(false);

            Fit(buffText,SafeRoot,new Vector2(16,-112),new Vector2(-16,-86),17,TextAnchor.MiddleLeft,VoidToyPalette.CyanText,true);
            Fit(hint,SafeRoot,new Vector2(20,-250),new Vector2(-20,-176),30,TextAnchor.MiddleCenter,VoidToyPalette.Text,true);
            var guide=SafeRoot.Find("First run guidance") as RectTransform;
            if(guide)VoidToyStyle.Place(guide,new Vector2(0,1),new Vector2(1,1),new Vector2(25,-322),new Vector2(-25,-258));

            healthTrack=(RectTransform)healthFill.transform.parent;healthTrack.SetParent(SafeRoot,false);
            healthTrack.anchorMin=healthTrack.anchorMax=healthTrack.pivot=new Vector2(.5f,.5f);healthTrack.sizeDelta=new Vector2(84,12);
            VoidToyStyle.Surface(healthTrack.GetComponent<Image>(),tile,VoidToyPalette.WithAlpha(VoidToyPalette.Void,.9f),20);
            var fill=healthFill.rectTransform;fill.offsetMin=new Vector2(2,2);fill.offsetMax=new Vector2(-2,-2);
            VoidToyStyle.Surface(healthFill,tile,VoidToyPalette.Health,20);
            if(healthLabel)healthLabel.gameObject.SetActive(false);

            var stick=joystick?joystick.GetComponent<Image>():null;
            if(stick)
            {
                stick.sprite=VoidToyStyle.Disc();stick.type=Image.Type.Simple;stick.color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.18f);
                var ring=stick.transform.Find("Joystick ring");
                if(!ring) {ring=new GameObject("Joystick ring",typeof(RectTransform),typeof(Image)).transform;ring.SetParent(stick.transform,false);ring.SetAsFirstSibling();}
                VoidToyStyle.Place((RectTransform)ring,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                var ringImage=ring.GetComponent<Image>();ringImage.sprite=VoidToyStyle.Ring();ringImage.color=VoidToyPalette.WithAlpha(VoidToyPalette.Text,.18f);ringImage.raycastTarget=false;
                if(joystick.knob) {var knob=joystick.knob.GetComponent<Image>();knob.sprite=VoidToyStyle.Disc();knob.color=VoidToyPalette.Text;}
                // Free-hand control: the whole open screen accepts the thumb; the stick only shows while held.
                var zone=SafeRoot.Find("Joystick zone");
                if(!zone) {zone=new GameObject("Joystick zone",typeof(RectTransform),typeof(Image)).transform;zone.SetParent(SafeRoot,false);}
                zone.SetAsFirstSibling();VoidToyStyle.Place((RectTransform)zone,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                joystickZone=zone.GetComponent<Image>();joystickZone.color=Color.clear;joystickZone.raycastTarget=true;
                var touch=zone.GetComponent<JoystickTouchZone>();if(!touch)touch=zone.gameObject.AddComponent<JoystickTouchZone>();touch.stick=joystick;joystick.ResetInput();
            }
            if(pauseOverlay)
            {
                // Pause: no card. A deep dim, the frozen time as the headline, one shining resume and the sound toggle.
                var overlay=(RectTransform)pauseOverlay.transform;VoidToyStyle.Place(overlay,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);overlay.SetAsLastSibling();
                var dim=pauseOverlay.GetComponent<Image>();dim.sprite=null;dim.type=Image.Type.Simple;dim.color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.86f);dim.raycastTarget=true;
                var card=VoidToyStyle.Tile("Pause card",overlay,tile,Color.clear,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-226,-200),new Vector2(226,220));
                pauseTime=pauseOverlay.transform.Find("Pause label")?.GetComponent<Text>()??card.transform.Find("Pause label")?.GetComponent<Text>();
                if(pauseTime) {pauseTime.transform.SetParent(card.transform,false);VoidToyStyle.Place(pauseTime.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(10,-120),new Vector2(-10,-10));pauseTime.fontSize=88;pauseTime.alignment=TextAnchor.MiddleCenter;VoidToyStyle.Ink(pauseTime,uiFont,VoidToyPalette.Text);}
                pauseInfo=VoidToyStyle.Label("Pause info",card.transform,uiFont,20,TextAnchor.MiddleCenter,VoidToyPalette.Muted);
                VoidToyStyle.Place(pauseInfo.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(10,-160),new Vector2(-10,-122));VoidToyStyle.Outline(pauseInfo);
                var resume=VoidToyStyle.Button("Resume",card.transform,tile,VoidToyPalette.Cyan,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-300),new Vector2(-16,-214),uiFont,30,VoidToyPalette.Ink,out var resumeLabel);
                resumeLabel.text="DEVAM";resume.onClick.RemoveAllListeners();resume.onClick.AddListener(()=>pauseAction?.Invoke());VoidToyStyle.AddShine(resume);
                var sound=VoidToyStyle.Button("Sound toggle",card.transform,tile,VoidToyPalette.Panel,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-388),new Vector2(-16,-320),uiFont,21,VoidToyPalette.Text,out pauseSound);
                pauseSound.text="Ses açık";sound.onClick.RemoveAllListeners();sound.onClick.AddListener(()=>muteAction?.Invoke());
                if(Application.isPlaying)VoidToyStyle.AnimateShine((RectTransform)resume.transform,gameObject);
            }
            if(result)VoidToyStyle.Surface(result.GetComponent<Image>(),tile,VoidToyPalette.Panel);
            foreach(var text in GetComponentsInChildren<Text>(true))
            {
                if(text.font!=uiFont)text.font=uiFont;
                VoidToyStyle.Outline(text);
            }
        }

        void Fit(Text text,RectTransform parent,Vector2 low,Vector2 high,int size,TextAnchor anchor,Color color,bool topBand=false)
        {
            if(!text)return;
            text.transform.SetParent(parent,false);
            if(topBand)VoidToyStyle.Place(text.rectTransform,new Vector2(0,1),new Vector2(1,1),low,high);
            else VoidToyStyle.Place(text.rectTransform,Vector2.zero,Vector2.one,low,high);
            text.fontSize=size;text.alignment=anchor;text.color=color;text.resizeTextForBestFit=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Overflow;
        }

        public void SetPlayerAnchor(Vector3 world) { playerAnchor=world;hasPlayerAnchor=true; }
        void PlaceHealthOverPlayer(bool visible)
        {
            if(!healthTrack)return;
            bool show=visible&&hasPlayerAnchor&&sceneCamera;
            if(healthTrack.gameObject.activeSelf!=show)healthTrack.gameObject.SetActive(show);
            if(!show)return;
            var screen=sceneCamera.WorldToScreenPoint(playerAnchor);
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(SafeRoot,screen,Canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:Canvas.worldCamera,out var local))
                healthTrack.localPosition=new Vector3(local.x,local.y,0);
        }

        // Damage reads at the screen edges only: a short pink vignette, never a full-screen wash over the arena.
        Image hurtEdge;static Sprite hurtVignette;
        public void FlashHurt()
        {
            if(!Canvas)return;
            if(!hurtEdge)
            {
                if(!hurtVignette)hurtVignette=Vignette();
                hurtEdge=new GameObject("Hurt edge",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
                hurtEdge.transform.SetParent(Canvas.transform,false);hurtEdge.transform.SetSiblingIndex(0);
                VoidToyStyle.Place(hurtEdge.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                hurtEdge.sprite=hurtVignette;hurtEdge.raycastTarget=false;
            }
            hurtEdge.DOKill();hurtEdge.gameObject.SetActive(true);hurtEdge.color=VoidToyPalette.WithAlpha(VoidToyPalette.Danger,.75f);
            hurtEdge.DOFade(0,.38f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(hurtEdge.gameObject).OnComplete(()=>hurtEdge.gameObject.SetActive(false));
        }
        static Sprite Vignette()
        {
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false) {name="Hurt vignette",wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Abs(x+.5f-size*.5f)/(size*.5f),dy=Mathf.Abs(y+.5f-size*.5f)/(size*.5f);
                float edge=Mathf.Max(dx,dy);float a=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.62f,1,edge));
                pixels[y*size+x]=new Color32(255,255,255,(byte)(255*a));
            }
            texture.SetPixels32(pixels);texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100);sprite.hideFlags=HideFlags.DontSave;return sprite;
        }

        public void SetBranchPreview(BranchCard right,BranchCard forward,bool visible)
        {
            if(!branchCards&&visible)
            {
                var art=Resources.Load<FrontEndArt>("FrontEndArt");
                var uiFont=art&&art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                branchCards=new GameObject("Walking gate previews",typeof(RectTransform)).GetComponent<RectTransform>();branchCards.SetParent(SafeRoot,false);
                VoidToyStyle.Place(branchCards,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                // Forward sits toward the top of the screen, right toward the right edge, matching the gates.
                forwardCard=Card("Forward gate preview",art,uiFont,new Vector2(.5f,1),new Vector2(-118,-340),new Vector2(118,-272));
                rightCard=Card("Right gate preview",art,uiFont,new Vector2(1,0),new Vector2(-256,250),new Vector2(-18,318));
                if(Application.isPlaying)
                    foreach(var card in new[]{forwardCard,rightCard})
                        card.root.DOAnchorPosY(card.root.anchoredPosition.y+6,1.3f).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Yoyo).SetUpdate(true).SetLink(card.root.gameObject);
            }
            if(!branchCards)return;branchCards.gameObject.SetActive(visible);
            if(!visible)return;
            Show(rightCard,right);Show(forwardCard,forward);
        }

        CardView Card(string name,FrontEndArt art,Font uiFont,Vector2 anchor,Vector2 low,Vector2 high)
        {
            Sprite tile=art?art.casualButton:null;
            var view=new CardView();
            view.border=VoidToyStyle.Tile(name,branchCards,tile,VoidToyPalette.Cyan,anchor,anchor,low,high);
            view.root=(RectTransform)view.border.transform;
            var face=VoidToyStyle.Tile("Card face",view.root,tile,VoidToyPalette.WithAlpha(VoidToyPalette.Panel,.97f),Vector2.zero,Vector2.one,new Vector2(3,3),new Vector2(-3,-3));
            // Calm label: reward icon, reward line and risk dots. Direction and encounter detail stay off screen.
            view.direction=VoidToyStyle.Label("Direction",face.transform,uiFont,17,TextAnchor.MiddleLeft,VoidToyPalette.Muted);view.direction.gameObject.SetActive(false);
            view.pips=new Image[3];
            for(int i=0;i<3;i++)
            {
                var pip=VoidToyStyle.Tile("Risk "+(i+1),face.transform,tile,VoidToyPalette.Track,Vector2.zero,Vector2.zero,new Vector2(70+i*15,12),new Vector2(81+i*15,23));
                pip.pixelsPerUnitMultiplier=40;view.pips[i]=pip;
            }
            view.icon=VoidToyStyle.Tile("Reward icon",face.transform,tile,VoidToyPalette.Health,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(10,-24),new Vector2(58,24));
            view.title=VoidToyStyle.Label("Reward",face.transform,uiFont,21,TextAnchor.MiddleLeft,VoidToyPalette.Text);
            VoidToyStyle.Place(view.title.rectTransform,new Vector2(0,0),new Vector2(1,1),new Vector2(70,28),new Vector2(-10,-4));
            view.detail=VoidToyStyle.Label("Encounter",face.transform,uiFont,16,TextAnchor.MiddleLeft,VoidToyPalette.Muted);view.detail.gameObject.SetActive(false);
            view.title.verticalOverflow=view.detail.verticalOverflow=VerticalWrapMode.Truncate;view.title.horizontalOverflow=HorizontalWrapMode.Wrap;
            view.title.resizeTextForBestFit=view.detail.resizeTextForBestFit=true;view.title.resizeTextMinSize=13;view.title.resizeTextMaxSize=24;view.detail.resizeTextMinSize=12;view.detail.resizeTextMaxSize=16;
            foreach(var text in view.root.GetComponentsInChildren<Text>(true))VoidToyStyle.Outline(text);
            return view;
        }

        static void Show(CardView view,BranchCard card)
        {
            if(view.direction.text!=card.direction)view.direction.text=card.direction;
            if(view.title.text!=card.title)view.title.text=card.title;
            if(view.detail.text!=card.detail)view.detail.text=card.detail;
            view.border.color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.9f);view.icon.color=card.accent;
            for(int i=0;i<3;i++)view.pips[i].color=i<card.risk?VoidToyPalette.Risk(card.risk):VoidToyPalette.Track;
        }
    }
}
