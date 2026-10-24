using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class FrontEndSmokeCheck
{
    const string Key="CI.FrontSmoke",Env="CUBE_INVASION_META_TEST_PATH";
    static readonly BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
    static InvasionPrototype game;
    static MenuFrontEnd menu;
    static int step,gold;
    static double next,deadline;
    static string[] preferenceKeys={"CI.MusicVolume","CI.SfxVolume","CI.Muted","CI.Haptics","CI.HighFrames"};
    static FrontEndSmokeCheck() { EditorApplication.update+=Tick; }
    static object Get(object obj,string field)=>obj.GetType().GetField(field,Flags).GetValue(obj);
    static object Call(string method,params object[] args)=>typeof(InvasionPrototype).GetMethod(method,Flags).Invoke(game,args);
    static string State=>Get(game,"phase").ToString();
    static MetaProfileStore Store=>(MetaProfileStore)Get(game,"profile");
    static void Check(bool condition,string message) { if(!condition)throw new Exception(message);Debug.Log("UI_CHECK_OK: "+message); }
    public static void Run()
    {
        foreach(string key in preferenceKeys)
        { SessionState.SetBool(Key+key+"exists",PlayerPrefs.HasKey(key));if(key.Contains("Volume"))SessionState.SetFloat(Key+key,PlayerPrefs.GetFloat(key,1));else SessionState.SetInt(Key+key,PlayerPrefs.GetInt(key,0)); }
        SessionState.SetString(Key+"env",Environment.GetEnvironmentVariable(Env)??"");
        string directory=Path.Combine(Path.GetTempPath(),"cube-ui-check-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);SessionState.SetString(Key+"dir",directory);
        string file=Path.Combine(directory,"profile.json");Environment.SetEnvironmentVariable(Env,file);
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");new MetaProfileStore(file,rules,7,1000);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static void Capture(string name)
    {
        var camera=(Camera)Get(game,"view");var target=new RenderTexture(540,960,24);camera.targetTexture=target;var old=RenderTexture.active;
        camera.GetComponent<ArcadeFollowCamera>().SendMessage("Update");var brain=camera.GetComponent<Cinemachine.CinemachineBrain>();
        if(brain.ActiveVirtualCamera is Cinemachine.CinemachineVirtualCamera vcam) { vcam.PreviousStateIsValid=false;vcam.UpdateCameraState(Vector3.up,-1);brain.ManualUpdate(); }
        Call("RefreshContentHud");Canvas.ForceUpdateCanvases();if(name=="Road")menu.ReframeRoad();camera.Render();RenderTexture.active=target;
        var texture=new Texture2D(540,960,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("/tmp/cube-ui-previews");
        File.WriteAllBytes("/tmp/cube-ui-previews/"+name+".png",texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        try
        {
            double now=EditorApplication.timeSinceStartup;if(deadline==0)deadline=now+65;if(now>deadline)throw new Exception("UI check timed out at "+step);
            if(now<next)return;if(!game)game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game||Get(game,"profile")==null)return;
            if(!menu)menu=(MenuFrontEnd)Get(game,"frontEnd");
            switch(step)
            {
                case 0:
                    Check(State=="Loadout"&&menu&&menu.gameObject.activeSelf&&menu.CurrentPage==MenuPage.Home,"Authored front end opens on home page");
                    Check(((Image)Get(menu,"background")).raycastTarget,"Menu backdrop blocks touches from reaching combat HUD");
                    Capture("Home");var tabs=(Button[])Get(menu,"navigation");tabs[1].onClick.Invoke();next=now+.3;step=1;break;
                case 1:
                    Check(menu.CurrentPage==MenuPage.Road,"Serialized tab buttons receive runtime callbacks");
                    var levels=(Button[])Get(menu,"levelNodes");Check(levels.Length==7,"Road path contains seven interactive missions");
                    levels[3].onClick.Invoke();Check((int)Get(game,"missionIndex")==3,"Locked road node can be inspected");
                    var launch=(Button)Get(menu,"roadPlay");Check(!launch.interactable,"Locked mission launch is disabled");launch.onClick.Invoke();Check(State=="Loadout","Direct locked callback cannot bypass progression");
                    levels[0].onClick.Invoke();Check(launch.interactable,"Unlocked road node enables launch");Capture("Road");
                    var scroll=(ScrollRect)Get(menu,"roadScroll");Check(scroll.content.rect.height>scroll.viewport.rect.height,"Campaign path is vertically scrollable");scroll.verticalNormalizedPosition=1;Capture("RoadTop");
                    ((Button[])Get(menu,"navigation"))[2].onClick.Invoke();next=now+.3;step=2;break;
                case 2:
                    ((Button[])Get(menu,"weaponCards"))[1].onClick.Invoke();Check((int)Get(game,"inspectedWeapon")==1&&!Store.IsOwned(1),"Market card inspects locked weapon without equipping");Capture("MarketLocked");
                    ((Button)Get(menu,"buyWeapon")).onClick.Invoke();Check(Store.IsOwned(1)&&Store.Data.gold==850,"Market purchase charges exact persistent price");
                    ((Button[])Get(menu,"weaponUpgrades"))[0].onClick.Invoke();Check(Store.Data.weapons[1].damage==1&&Store.Data.gold==815,"Market upgrade uses persistent transaction");Capture("Market");
                    ((Button[])Get(menu,"navigation"))[3].onClick.Invoke();next=now+.3;step=3;break;
                case 3:
                    var upgrades=(Button[])Get(menu,"characterUpgrades");foreach(var upgrade in upgrades)upgrade.onClick.Invoke();
                    Check(Store.Data.vitality==1&&Store.Data.agility==1&&Store.Data.magnet==1&&Store.Data.gold==640,"Three pilot upgrades persist with exact prices");Capture("Pilot");
                    ((Button[])Get(menu,"navigation"))[4].onClick.Invoke();next=now+.3;step=4;break;
                case 4:
                    var music=(Slider)Get(menu,"musicSlider");var sfx=(Slider)Get(menu,"sfxSlider");music.value=.35f;sfx.value=.6f;
                    ((Toggle)Get(menu,"muteToggle")).isOn=false;((Toggle)Get(menu,"hapticsToggle")).isOn=true;((Toggle)Get(menu,"frameToggle")).isOn=false;
                    var feedback=(CombatFeedback)Get(game,"feedback");Check(Mathf.Abs(feedback.MusicLevel-.35f)<.001f&&Mathf.Abs(feedback.SfxLevel-.6f)<.001f,"Independent music/effects sliders affect mixer");
                    Check(Application.targetFrameRate==30&&PlayerPrefs.GetInt("CI.Haptics")==1,"Performance and haptics controls update saved settings");
                    Check(music.GetComponent<Image>().raycastTarget&&((Toggle)Get(menu,"muteToggle")).GetComponent<Image>().raycastTarget,"Sliders and toggles have pointer hit surfaces");Capture("Settings");
                    ((Button[])Get(menu,"navigation"))[1].onClick.Invoke();((Button[])Get(menu,"levelNodes"))[0].onClick.Invoke();((Button)Get(menu,"roadPlay")).onClick.Invoke();
                    Check(State=="Combat"&&!menu.gameObject.activeSelf,"Road launch enters combat and hides front end");
                    Check((float)Get(game,"health")==110,"Vitality affects actual starting health");
                    float speed=(float)typeof(InvasionPrototype).GetProperty("MoveSpeed",Flags).GetValue(game),radius=(float)typeof(InvasionPrototype).GetProperty("PickupRadius",Flags).GetValue(game);
                    Check(Mathf.Abs(speed-4.635f)<.001f&&Mathf.Abs(radius-1.2f)<.001f,"Agility and magnet affect gameplay stats");
                    gold=Store.Data.gold;UnityEngine.SceneManagement.SceneManager.LoadScene(game.gameObject.scene.name);game=null;menu=null;next=now+.5;step=5;break;
                case 5:
                    Check(State=="Loadout"&&menu.CurrentPage==MenuPage.Home&&Store.Data.gold==gold&&Store.Data.vitality==1&&Store.Data.agility==1&&Store.Data.magnet==1,"Profile reload preserves pilot upgrades and currency");
                    Check(Mathf.Abs(((CombatFeedback)Get(game,"feedback")).MusicLevel-.35f)<.001f&&Application.targetFrameRate==30,"Settings restore after scene initialization");
                    Check(Object.FindObjectsByType<MenuFrontEnd>().Length==1&&Object.FindObjectsByType<Camera>().Length==1,"Scene reuses one front end and camera");
                    Debug.Log("CUBE_UI_SMOKE_OK");Finish(0);break;
            }
        }
        catch(Exception e) { Debug.LogException(e);Finish(1); }
    }
    static void Finish(int code)
    {
        SessionState.SetBool(Key,false);Environment.SetEnvironmentVariable(Env,SessionState.GetString(Key+"env",""));
        foreach(string key in preferenceKeys)
        {
            if(!SessionState.GetBool(Key+key+"exists",false))PlayerPrefs.DeleteKey(key);
            else if(key.Contains("Volume"))PlayerPrefs.SetFloat(key,SessionState.GetFloat(Key+key,1));else PlayerPrefs.SetInt(key,SessionState.GetInt(Key+key,0));
        }
        PlayerPrefs.Save();string directory=SessionState.GetString(Key+"dir","");if(Directory.Exists(directory))Directory.Delete(directory,true);
        EditorApplication.isPlaying=false;EditorApplication.Exit(code);
    }
}
