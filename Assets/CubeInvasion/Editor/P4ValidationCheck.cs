using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class P4ValidationCheck
{
    [MenuItem("Cube Invasion/P4/Install Scene Guidance")]
    public static void Install()
    {
        var game=Object.FindAnyObjectByType<InvasionPrototype>();
        var hud=Object.FindAnyObjectByType<MobileCombatHud>();
        if(!game||!hud)throw new InvalidOperationException("Open gameplay scene first.");
        var child=hud.SafeRoot.Find("First run guidance");
        var label=child?child.GetComponent<Text>():new GameObject("First run guidance",typeof(RectTransform),typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(hud.SafeRoot,false);
        var rect=label.rectTransform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);
        rect.offsetMin=new Vector2(28,-360);rect.offsetMax=new Vector2(-28,-294);
        label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=18;label.fontStyle=FontStyle.Bold;
        label.alignment=TextAnchor.MiddleCenter;label.color=new Color(.3f,.95f,1);label.raycastTarget=false;
        if(!label.GetComponent<Outline>()) { var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.015f,.035f,.07f,.95f);outline.effectDistance=new Vector2(1,-1); }
        label.text="";label.gameObject.SetActive(false);game.ConfigureOnboarding(label);
        EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log("CUBE_P4_INSTALL_OK");
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");ExpansionTestFixture.Legacy();Install(); }
    [MenuItem("Cube Invasion/P4/Replay First Run Guidance")]
    public static void ResetGuidance() { PlayerPrefs.DeleteKey("CI.Onboarding.v1");PlayerPrefs.Save();Debug.Log("First-run guidance will appear on mission 1."); }
    [MenuItem("Cube Invasion/P4/Check Diagnostics and Balance")]
    public static void Run()
    {
        string root=Path.Combine(Path.GetTempPath(),"ci-p4-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        try
        {
            string path=Path.Combine(root,"events.json");var log=new P4ValidationLog(path);
            for(int i=0;i<270;i++)log.Record(new P4ValidationLog.Event {name="cube_started",cube=i});
            var report=JsonUtility.FromJson<P4ValidationLog.Report>(File.ReadAllText(path));
            Require(report.events.Count==256&&report.events[0].cube==14&&report.events[255].cube==269,"bounded FIFO");
            Require(new P4ValidationLog(path).Count==256,"reload");
            File.WriteAllText(path,"broken");Require(new P4ValidationLog(path).Count==0,"corrupt diagnostics must not block game");
            string blocked=Path.Combine(root,"blocked");File.WriteAllText(blocked,"file");
            var bad=new P4ValidationLog(Path.Combine(blocked,"events.json"));bad.Record(new P4ValidationLog.Event {name="test"});
            Require(bad.LastError!=null,"failed diagnostic IO remains nonfatal");
            var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
            var csv=new StringBuilder("mission,cube,mode,enemies,hp,pulse_ideal_seconds,quota\n");
            for(int m=0;m<content.missions.Length;m++)
            {
                var mission=content.missions[m];Require(mission.cubes.Length>0,"empty mission");
                for(int c=0;c<mission.cubes.Length;c++)
                {
                    var cube=mission.cubes[c];float hp=0;
                    foreach(var enemy in cube.enemies)hp+=Hp(enemy,c);
                    Require(cube.enemies.Length>0,"empty encounter");
                    if(mission.mode==MissionMode.GoldRush)Require(cube.goldQuota>0,"quota");
                    csv.AppendLine($"{m+1},{c+1},{mission.mode},{cube.enemies.Length},{hp},{(hp/content.weapons[0].NominalDps).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)},{cube.goldQuota}");
                }
            }
            foreach(var weapon in content.weapons)Require(weapon.damage>0&&weapon.interval>0&&weapon.pellets>0&&weapon.projectileLife>0,"invalid weapon");
            File.WriteAllText("/tmp/cube-p4-balance.csv",csv.ToString());
            Debug.Log("CUBE_P4_CHECK_OK: bounded diagnostics, corrupt/unwritable recovery, content integrity, ideal DPS balance report /tmp/cube-p4-balance.csv");
        }
        finally { Directory.Delete(root,true); }
    }
    static float Hp(ContentEnemy e,int cube)
    { switch(e) { case ContentEnemy.Grunt:return 18;case ContentEnemy.Sprinter:return 12;case ContentEnemy.Gunner:return 21;case ContentEnemy.Pusher:return 24;case ContentEnemy.Sniper:return 14;case ContentEnemy.Bruiser:return 45;default:return 72+cube*12; } }
    static void Require(bool value,string what) { if(!value)throw new Exception("P4 validation failed: "+what); }
}
