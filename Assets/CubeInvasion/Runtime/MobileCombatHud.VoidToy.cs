using UnityEngine;
using UnityEngine.UI;

namespace CubeInvasion.Prototype
{
    // Display data for one walking-gate route. Cards inform only; the gate itself is the choice.
    public struct BranchCard { public string direction,title,detail; public Color accent; public int risk; }

    // Void Toy Arcade layout: top band (cube chip · timer · gold/pause), objective pill,
    // health bar over the player and gate cards. Applied on every Initialize, so it is idempotent.
    public sealed partial class MobileCombatHud
    {
        [SerializeField] Text objectiveText;
        [SerializeField] Image timerTile,timerHalo,objectivePill;
        RectTransform healthTrack;
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

            var stageChip=VoidToyStyle.Tile("Objective tile",band,tile,VoidToyPalette.Glass,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-78),new Vector2(170,-16));
            Fit(status,(RectTransform)stageChip.transform,new Vector2(14,4),new Vector2(-8,-4),22,TextAnchor.MiddleLeft,VoidToyPalette.Text);

            timerHalo=VoidToyStyle.Tile("Timer halo",band,tile,Color.clear,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-92,-132),new Vector2(92,-8));
            timerTile=VoidToyStyle.Tile("Timer tile",band,tile,VoidToyPalette.Glass,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-84,-124),new Vector2(84,-16));
            Fit(timer,(RectTransform)timerTile.transform,new Vector2(0,22),new Vector2(0,-2),62,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            var caption=timerTile.transform.Find("Time caption");if(caption)caption.gameObject.SetActive(false);
            var track=(RectTransform)timerFill.transform.parent;track.SetParent(timerTile.transform,false);
            VoidToyStyle.Place(track,Vector2.zero,new Vector2(1,0),new Vector2(28,12),new Vector2(-28,20));
            track.GetComponent<Image>().color=VoidToyPalette.Raised;

            objectivePill=VoidToyStyle.Tile("Objective pill",band,tile,VoidToyPalette.Glass,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(-140,-180),new Vector2(140,-134));
            if(!objectiveText)objectiveText=VoidToyStyle.Label("Objective",objectivePill.transform,uiFont,22,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            Fit(objectiveText,(RectTransform)objectivePill.transform,new Vector2(10,0),new Vector2(-10,0),22,TextAnchor.MiddleCenter,VoidToyPalette.Text);

            var goldChip=VoidToyStyle.Tile("Gold chip",band,tile,VoidToyPalette.Glass,new Vector2(1,1),new Vector2(1,1),new Vector2(-180,-76),new Vector2(-86,-16));
            var coin=goldChip.transform.Find("Coin") as RectTransform;
            if(!coin) {coin=new GameObject("Coin",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();coin.SetParent(goldChip.transform,false);}
            VoidToyStyle.Place(coin,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(8,-18),new Vector2(44,18));
            var coinImage=coin.GetComponent<Image>();coinImage.sprite=art?art.coin:null;coinImage.preserveAspect=true;coinImage.raycastTarget=false;coinImage.color=art&&art.coin?Color.white:VoidToyPalette.Gold;
            Fit(gold,(RectTransform)goldChip.transform,new Vector2(42,0),new Vector2(-10,0),22,TextAnchor.MiddleRight,VoidToyPalette.Gold);
            // Wallet plus pending gold stays on one line; best fit shrinks it inside the chip.
            gold.horizontalOverflow=HorizontalWrapMode.Wrap;gold.resizeTextForBestFit=true;gold.resizeTextMinSize=12;gold.resizeTextMaxSize=22;gold.verticalOverflow=VerticalWrapMode.Truncate;

            var pause=pauseText.GetComponentInParent<Button>(true).GetComponent<RectTransform>();pause.SetParent(SafeRoot,false);
            VoidToyStyle.Place(pause,Vector2.one,Vector2.one,new Vector2(-76,-76),new Vector2(-16,-16));
            VoidToyStyle.Surface(pause.GetComponent<Image>(),tile,VoidToyPalette.Glass);
            Fit(pauseText,pause,Vector2.zero,Vector2.zero,26,TextAnchor.MiddleCenter,VoidToyPalette.Text);
            // Sound lives in Settings; the combat screen keeps only pause in the corner.
            if(soundText)soundText.GetComponentInParent<Button>(true).gameObject.SetActive(false);

            Fit(buffText,SafeRoot,new Vector2(16,-112),new Vector2(-16,-86),17,TextAnchor.MiddleLeft,VoidToyPalette.CyanText,true);
            Fit(hint,SafeRoot,new Vector2(20,-250),new Vector2(-20,-190),28,TextAnchor.MiddleCenter,VoidToyPalette.Text,true);
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
                stick.sprite=VoidToyStyle.Disc();stick.type=Image.Type.Simple;stick.color=VoidToyPalette.WithAlpha(VoidToyPalette.Void,.38f);
                var ring=stick.transform.Find("Joystick ring");
                if(!ring) {ring=new GameObject("Joystick ring",typeof(RectTransform),typeof(Image)).transform;ring.SetParent(stick.transform,false);ring.SetAsFirstSibling();}
                VoidToyStyle.Place((RectTransform)ring,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                var ringImage=ring.GetComponent<Image>();ringImage.sprite=VoidToyStyle.Ring();ringImage.color=VoidToyPalette.WithAlpha(VoidToyPalette.Text,.28f);ringImage.raycastTarget=false;
                if(joystick.knob) {var knob=joystick.knob.GetComponent<Image>();knob.sprite=VoidToyStyle.Disc();knob.color=VoidToyPalette.Text;}
            }
            if(pauseOverlay)
            {
                VoidToyStyle.Surface(pauseOverlay.GetComponent<Image>(),tile,VoidToyPalette.Panel);
                foreach(var text in pauseOverlay.GetComponentsInChildren<Text>(true))VoidToyStyle.Ink(text,uiFont,VoidToyPalette.Text);
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

        public void SetBranchPreview(BranchCard right,BranchCard forward,bool visible)
        {
            if(!branchCards&&visible)
            {
                var art=Resources.Load<FrontEndArt>("FrontEndArt");
                var uiFont=art&&art.uiFont?art.uiFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                branchCards=new GameObject("Walking gate previews",typeof(RectTransform)).GetComponent<RectTransform>();branchCards.SetParent(SafeRoot,false);
                VoidToyStyle.Place(branchCards,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
                // Forward sits toward the top of the screen, right toward the right edge, matching the gates.
                forwardCard=Card("Forward gate preview",art,uiFont,new Vector2(.5f,1),new Vector2(-180,-382),new Vector2(180,-272));
                rightCard=Card("Right gate preview",art,uiFont,new Vector2(1,0),new Vector2(-318,232),new Vector2(-16,342));
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
            view.direction=VoidToyStyle.Label("Direction",face.transform,uiFont,17,TextAnchor.MiddleLeft,VoidToyPalette.Muted);
            VoidToyStyle.Place(view.direction.rectTransform,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-34),new Vector2(-80,-8));
            view.pips=new Image[3];
            for(int i=0;i<3;i++)
            {
                var pip=VoidToyStyle.Tile("Risk "+(i+1),face.transform,tile,VoidToyPalette.Track,Vector2.one,Vector2.one,new Vector2(-70+i*19,-28),new Vector2(-56+i*19,-14));
                pip.pixelsPerUnitMultiplier=40;view.pips[i]=pip;
            }
            view.icon=VoidToyStyle.Tile("Reward icon",face.transform,tile,VoidToyPalette.Health,new Vector2(0,0),new Vector2(0,0),new Vector2(14,12),new Vector2(64,62));
            view.title=VoidToyStyle.Label("Reward",face.transform,uiFont,24,TextAnchor.MiddleLeft,VoidToyPalette.Text);
            VoidToyStyle.Place(view.title.rectTransform,new Vector2(0,0),new Vector2(1,0),new Vector2(76,36),new Vector2(-12,66));
            view.detail=VoidToyStyle.Label("Encounter",face.transform,uiFont,16,TextAnchor.MiddleLeft,VoidToyPalette.Muted);
            VoidToyStyle.Place(view.detail.rectTransform,new Vector2(0,0),new Vector2(1,0),new Vector2(76,8),new Vector2(-12,36));
            view.title.verticalOverflow=view.detail.verticalOverflow=VerticalWrapMode.Truncate;view.title.horizontalOverflow=HorizontalWrapMode.Wrap;
            view.title.resizeTextForBestFit=view.detail.resizeTextForBestFit=true;view.title.resizeTextMinSize=16;view.title.resizeTextMaxSize=24;view.detail.resizeTextMinSize=12;view.detail.resizeTextMaxSize=16;
            foreach(var text in view.root.GetComponentsInChildren<Text>(true))VoidToyStyle.Outline(text);
            return view;
        }

        static void Show(CardView view,BranchCard card)
        {
            if(view.direction.text!=card.direction)view.direction.text=card.direction;
            if(view.title.text!=card.title)view.title.text=card.title;
            if(view.detail.text!=card.detail)view.detail.text=card.detail;
            view.border.color=card.accent;view.icon.color=card.accent;
            for(int i=0;i<3;i++)view.pips[i].color=i<card.risk?VoidToyPalette.Risk(card.risk):VoidToyPalette.Track;
        }
    }
}
