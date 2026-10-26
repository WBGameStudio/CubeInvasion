using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class CombatPressureBuilder
{
    public static void BuildAndroid() {P4ValidationCheck.Run();P4AndroidBuild.Run();}
    [MenuItem("Cube Invasion/Combat/Install Pressure And Fire Rhythm")]
    public static void Install()
    {
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);
        string path="Assets/CubeInvasion/Resources/CombatBalance.asset";
        var settings=AssetDatabase.LoadAssetAtPath<CombatBalanceSettings>(path);
        if(!settings) {settings=ScriptableObject.CreateInstance<CombatBalanceSettings>();AssetDatabase.CreateAsset(settings,path);}
        data.FindProperty("combatBalance").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        if(catalog.pressureRevision<2)
        {
            if(catalog.missions.Length!=7)throw new InvalidOperationException("Review custom campaign before applying pressure encounters.");
            var g=ContentEnemy.Grunt;var r=ContentEnemy.Sprinter;var a=ContentEnemy.Gunner;var p=ContentEnemy.Pusher;var s=ContentEnemy.Sniper;var b=ContentEnemy.Bruiser;var boss=ContentEnemy.Crusher;
            ContentEnemy[][][] waves={
                new[]{new[]{g,g,a,r,a,r},new[]{g,r,a,g,r,a},new[]{g,r,a,g,r,a}},
                new[]{new[]{g,g,a,r,a,r},new[]{r,r,g,a,g,a},new[]{g,g,a,a,r,r},new[]{r,r,a,g,a,g}},
                new[]{new[]{p,g,g,r,a},new[]{p,r,r,a,g},new[]{p,p,g,a,r},new[]{p,a,r,g,a}},
                new[]{new[]{s,g,g,r,a},new[]{b,r,g,a},new[]{s,s,a,r,g},new[]{b,p,r,a},new[]{s,b,g,r,a}},
                new[]{new[]{a,a,r,r,g,a},new[]{b,p,g,r,a},new[]{s,a,g,g,r,a},new[]{b,r,r,p,a},new[]{b,s,p,r,a}},
                new[]{new[]{g,r,a,a},new[]{r,r,g,a,a},new[]{r,a,p,a}},
                new[]{new[]{boss,g,r},new[]{boss,r,r,a},new[]{boss,a,g,r}}
            };
            for(int m=0;m<7;m++)
            {
                var mission=catalog.missions[m];if(mission.cubes.Length!=waves[m].Length)throw new InvalidOperationException("Custom cube count: "+mission.title);
                for(int c=0;c<mission.cubes.Length;c++)
                {
                    var cube=mission.cubes[c];cube.enemies=waves[m][c];
                    // The opening cube teaches the fire rhythm before introducing cover.
                    cube.cover=m==0?c>0:m==6?c==1:(c%2==0||c==mission.cubes.Length-1);
                    cube.coverOffset=new Vector2(c%2==0?-.65f:.65f,.55f+(c%3)*.2f);
                    cube.healing=mission.mode==MissionMode.Combat&&c==mission.cubes.Length-2&&m>0;
                    cube.healOffset=new Vector2(c%2==0?2.7f:-2.7f,1.7f);
                }
            }
            catalog.missions[0].description="Kaçarken seyrek, yavaşlarken hızlı ateş et. Siperin etrafından açı bul.";
            catalog.missions[1].description="Siper iki tarafın mermisini de durdurur. Yakın ve uzak tehditleri birlikte yönet.";
            catalog.missions[4].description="Çapraz ateş ve yakın dövüş baskısı. Güvenli atış aralıkları yarat.";
            catalog.pressureRevision=2;EditorUtility.SetDirty(catalog);
        }
        var hud=(MobileCombatHud)data.FindProperty("hud").objectReferenceValue;
        var existing=hud.SafeRoot.Find("Fire rhythm");Text label;
        if(existing)label=existing.GetComponent<Text>();
        else
        {
            var obj=new GameObject("Fire rhythm",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));obj.transform.SetParent(hud.SafeRoot,false);
            var rect=obj.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(110,214);rect.sizeDelta=new Vector2(208,42);
            label=obj.GetComponent<Text>();label.font=hud.SafeRoot.GetComponentInChildren<Text>(true).font;label.fontSize=15;label.fontStyle=FontStyle.Bold;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;label.color=new Color(.35f,1,.8f);label.text="ODAK ATEŞİ";
        }
        var outline=label.GetComponent<Outline>();if(!outline)outline=label.gameObject.AddComponent<Outline>();
        outline.effectColor=new Color(.015f,.055f,.09f,.95f);outline.effectDistance=new Vector2(1.2f,-1.2f);outline.useGraphicAlpha=true;
        hud.ConfigureFireRhythm(label);EditorUtility.SetDirty(hud);EditorUtility.SetDirty(game);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log("CUBE_PRESSURE_INSTALL_OK: mixed encounters, limited healing, tactical cover, serialized balance and fire-rhythm HUD; scene anchors preserved.");
    }
}
