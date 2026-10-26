using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class ReferenceUiBuilder
{
    const string Root="Assets/CubeInvasion/Art/UI/Reference/";
    [MenuItem("Cube Invasion/UI/Rebuild Reference Layout")]
    public static void Install()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode before rebuilding the authored UI.");
        AssetDatabase.Refresh();
        var art=AssetDatabase.LoadAssetAtPath<FrontEndArt>("Assets/CubeInvasion/Resources/FrontEndArt.asset");
        art.journey=Sprite("Journey");art.arsenal=Sprite("Arsenal");art.pilot=Sprite("Pilot");art.settings=Sprite("Settings");art.coin=Sprite("Coin");
        art.uiFont=AssetDatabase.LoadAssetAtPath<Font>(Root+"CubeRounded-Bold.ttf");
        if(!art.uiFont||!art.casualButton)throw new InvalidOperationException("Reference font or nine-slice button asset is missing.");
        foreach(char glyph in "ÇĞİÖŞÜçğıöşü")if(!art.uiFont.HasCharacter(glyph))throw new InvalidOperationException("Missing Turkish glyph: "+glyph);
        art.menuHero=null;art.campaignIslands=null;EditorUtility.SetDirty(art);
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var hud=Object.FindAnyObjectByType<MobileCombatHud>();
        if(!game||!hud)throw new InvalidOperationException("Open the gameplay scene first.");
        var old=Object.FindAnyObjectByType<MenuFrontEnd>(FindObjectsInactive.Include);
        if(old)Object.DestroyImmediate(old.gameObject);
        var menu=new GameObject("Reference Casual Front End",typeof(RectTransform)).AddComponent<MenuFrontEnd>();
        hud.Canvas.GetComponent<CanvasScaler>().matchWidthOrHeight=0;EditorUtility.SetDirty(hud.Canvas.GetComponent<CanvasScaler>());
        var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        menu.Initialize(hud.SafeRoot,art,content);game.ConfigureFrontEnd(menu,art);
        foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(text.GetComponentInParent<MenuFrontEnd>(true))continue;
            text.font=art.uiFont;text.fontStyle=FontStyle.Normal;text.color=Color.white;
            var outline=text.GetComponent<Outline>();if(!outline)outline=text.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.01f,.07f,.15f,.9f);outline.effectDistance=new Vector2(1,-1);EditorUtility.SetDirty(text);
        }
        foreach(var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(image.GetComponentInParent<MenuFrontEnd>(true))continue;
            if(image.sprite==art.panel) { image.color=new Color(.035f,.37f,.53f,.97f);EditorUtility.SetDirty(image); }
        }
        foreach(var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(button.GetComponentInParent<MenuFrontEnd>(true))continue;
            var image=button.GetComponent<Image>();if(!image)continue;
            image.sprite=art.casualButton;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=8;
            image.color=button.name.Contains("Retry")?new Color(.2f,.82f,.1f):new Color(1,.45f,.08f);
            if(!button.GetComponent<CasualButtonMotion>())button.gameObject.AddComponent<CasualButtonMotion>();EditorUtility.SetDirty(image);EditorUtility.SetDirty(button);
        }
        ConfigureCombatAndResults(hud,art);
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");
        var profile=new MetaProfile {weapons=new WeaponProgress[4],completed=new bool[content.missions.Length]};
        for(int i=0;i<4;i++)profile.weapons[i]=new WeaponProgress {owned=i==0};
        menu.Refresh(profile,rules,content,0,0,0,"Loadout",null);menu.SyncSettings(1,1,false,false,true);menu.ShowPage(MenuPage.Home,false);Canvas.ForceUpdateCanvases();menu.ReframeRoad();
        EditorUtility.SetDirty(menu);EditorUtility.SetDirty(game);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log("CUBE_REFERENCE_UI_INSTALL_OK: rebuilt five screens, home journey, icon dock, loadout carousel, upgrade rows and settings.");
    }
    static void ConfigureCombatAndResults(MobileCombatHud hud,FrontEndArt art)
    {
        var header=hud.SafeRoot.Find("Combat header");Place((RectTransform)header,new Vector2(16,-164),new Vector2(-16,-16));header.GetComponent<Image>().color=new Color(.02f,.33f,.47f,.98f);
        var timerFrame=Frame("Timer tile",header,art,new Vector2(370,-133),new Vector2(-10,-14));
        var targetFrame=Frame("Objective tile",header,art,new Vector2(10,-133),new Vector2(358,-14));
        var time=Find(hud.transform,"Timer");time.SetParent(timerFrame,false);Place((RectTransform)time,new Vector2(4,-91),new Vector2(-4,-40));time.GetComponent<Text>().fontSize=34;time.GetComponent<Text>().alignment=TextAnchor.MiddleCenter;
        var caption=FindOrText("Time caption",timerFrame,art);Place(caption.rectTransform,new Vector2(4,-39),new Vector2(-4,-5));caption.text="SANİYE";caption.fontSize=17;caption.alignment=TextAnchor.MiddleCenter;
        var objective=Find(hud.transform,"Objectives");objective.SetParent(targetFrame,false);Place((RectTransform)objective,new Vector2(14,-75),new Vector2(-12,-9));objective.GetComponent<Text>().fontSize=23;objective.GetComponent<Text>().alignment=TextAnchor.UpperLeft;
        var gold=Find(hud.transform,"Gold");gold.SetParent(targetFrame,false);Place((RectTransform)gold,new Vector2(14,-113),new Vector2(-12,-77));gold.GetComponent<Text>().fontSize=18;gold.GetComponent<Text>().alignment=TextAnchor.MiddleLeft;gold.GetComponent<Text>().color=new Color(1,.8f,.15f);
        var track=Find(hud.transform,"Timer track");track.SetParent(timerFrame,false);var rect=(RectTransform)track;rect.anchorMin=Vector2.zero;rect.anchorMax=new Vector2(1,0);rect.offsetMin=new Vector2(9,8);rect.offsetMax=new Vector2(-9,17);track.GetComponent<Image>().color=new Color(.01f,.13f,.23f);
        Place((RectTransform)hud.SafeRoot.Find("Health track"),new Vector2(24,-187),new Vector2(-104,-174));
        Place((RectTransform)hud.SafeRoot.Find("Health number"),new Vector2(24,-217),new Vector2(300,-188));
        var pause=(RectTransform)hud.SafeRoot.Find("Pause");pause.anchorMin=pause.anchorMax=Vector2.one;pause.offsetMin=new Vector2(-82,-229);pause.offsetMax=new Vector2(-24,-180);
        Place((RectTransform)hud.SafeRoot.Find("Buff"),new Vector2(24,-249),new Vector2(-24,-219));
        Place((RectTransform)hud.SafeRoot.Find("Status hint"),new Vector2(20,-336),new Vector2(-20,-297));
        var guide=hud.SafeRoot.Find("First run guidance");if(guide)Place((RectTransform)guide,new Vector2(25,-403),new Vector2(-25,-343));
        var mission=Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);
        if(mission)
        {
            var boss=mission.transform.Find("Crusher health");if(boss)Place((RectTransform)boss,new Vector2(24,-289),new Vector2(-24,-264));
            var result=mission.transform.Find("Mission result");var modal=(RectTransform)result;modal.anchorMin=modal.anchorMax=new Vector2(.5f,.5f);modal.pivot=new Vector2(.5f,.5f);modal.sizeDelta=new Vector2(492,570);modal.anchoredPosition=new Vector2(0,15);
            result.GetComponent<Image>().sprite=art.panel;result.GetComponent<Image>().type=Image.Type.Sliced;result.GetComponent<Image>().pixelsPerUnitMultiplier=1.5f;result.GetComponent<Image>().color=new Color(.025f,.33f,.49f);
            if(!result.GetComponent<CasualModalMotion>())result.gameObject.AddComponent<CasualModalMotion>();
            var dim=mission.transform.Find("Result dim");if(!dim)dim=new GameObject("Result dim",typeof(RectTransform),typeof(Image)).transform;
            dim.SetParent(mission.transform,false);dim.SetAsLastSibling();dim.SetSiblingIndex(result.GetSiblingIndex());var dimRect=(RectTransform)dim;dimRect.anchorMin=Vector2.zero;dimRect.anchorMax=Vector2.one;dimRect.offsetMin=dimRect.offsetMax=Vector2.zero;dim.GetComponent<Image>().color=new Color(.005f,.025f,.06f,.72f);dim.GetComponent<Image>().raycastTarget=true;dim.gameObject.SetActive(false);result.GetComponent<CasualModalMotion>().ConfigureBackdrop(dim.gameObject);
            var emblem=result.Find("Result emblem");if(!emblem)emblem=new GameObject("Result emblem",typeof(RectTransform),typeof(Image)).transform;emblem.SetParent(result,false);Place((RectTransform)emblem,new Vector2(175,-123),new Vector2(317,10));emblem.GetComponent<Image>().sprite=art.journey;emblem.GetComponent<Image>().preserveAspect=true;emblem.GetComponent<Image>().raycastTarget=false;
            mission.ConfigureResultEmblem(emblem.GetComponent<Image>(),art.journey,art.pilot);EditorUtility.SetDirty(mission);
            var title=result.Find("Result title");Place((RectTransform)title,new Vector2(16,-187),new Vector2(-16,-128));title.GetComponent<Text>().fontSize=30;title.GetComponent<Text>().alignment=TextAnchor.MiddleCenter;title.GetComponent<Text>().color=new Color(1,.8f,.15f);
            var detail=result.Find("Result detail");Place((RectTransform)detail,new Vector2(22,-275),new Vector2(-22,-194));detail.GetComponent<Text>().fontSize=18;detail.GetComponent<Text>().alignment=TextAnchor.MiddleCenter;
            var actions=result.Find("Result actions");if(!actions)actions=new GameObject("Result actions",typeof(RectTransform),typeof(VerticalLayoutGroup)).transform;
            actions.SetParent(result,false);var body=(RectTransform)actions;body.anchorMin=Vector2.zero;body.anchorMax=new Vector2(1,0);body.offsetMin=new Vector2(24,24);body.offsetMax=new Vector2(-24,270);
            var layout=actions.GetComponent<VerticalLayoutGroup>();layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;layout.spacing=12;layout.childAlignment=TextAnchor.LowerCenter;
            foreach(string name in new[]{"Advance","Retry","Menu"})
            {
                var button=Find(result,name);button.SetParent(actions,false);var element=button.GetComponent<LayoutElement>();if(!element)element=button.gameObject.AddComponent<LayoutElement>();element.preferredHeight=70;element.minHeight=70;
                var label=button.GetComponentInChildren<Text>(true);label.fontSize=22;label.color=Color.white;label.alignment=TextAnchor.MiddleCenter;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Overflow;
                label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
                button.GetComponent<Image>().color=name=="Menu"?new Color(.08f,.6f,.92f):new Color(.22f,.85f,.1f);
            }
        }
    }
    static Transform Find(Transform parent,string name)=>parent.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);
    static Text FindOrText(string name,Transform parent,FrontEndArt art)
    {
        var child=parent.Find(name);if(!child)child=new GameObject(name,typeof(RectTransform),typeof(Text)).transform;
        child.SetParent(parent,false);var text=child.GetComponent<Text>();text.font=art.uiFont;text.color=Color.white;text.raycastTarget=false;
        return text;
    }
    static RectTransform Frame(string name,Transform parent,FrontEndArt art,Vector2 low,Vector2 high)
    {
        var child=parent.Find(name);if(!child)child=new GameObject(name,typeof(RectTransform),typeof(Image)).transform;
        child.SetParent(parent,false);var image=child.GetComponent<Image>();image.sprite=art.casualButton;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=10;image.color=new Color(.015f,.2f,.32f);image.raycastTarget=false;Place((RectTransform)child,low,high);return (RectTransform)child;
    }
    static void Place(RectTransform rect,Vector2 low,Vector2 high)
    { rect.anchorMin=Vector2.up;rect.anchorMax=new Vector2(high.x>0?0:1,1);rect.offsetMin=low;rect.offsetMax=high; }
    static Sprite Sprite(string name)
    {
        string path=Root+name+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
}
