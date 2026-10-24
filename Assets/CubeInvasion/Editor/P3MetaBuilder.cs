using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;

public static class P3MetaBuilder
{
    [MenuItem("Cube Invasion/P3/Install Progression In Current Scene")]
    public static void Install()
    {
        const string path="Assets/CubeInvasion/Resources/P3MetaSettings.asset";
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>(path);
        if(!rules) { rules=ScriptableObject.CreateInstance<P3MetaSettings>();AssetDatabase.CreateAsset(rules,path); }
        InstallSounds();
        var runtime=UnityEngine.Object.FindAnyObjectByType<InvasionPrototype>();
        if(!runtime)throw new System.InvalidOperationException("Open the Cube Invasion gameplay scene first.");
        var mobile=UnityEngine.Object.FindAnyObjectByType<MobileCombatHud>();
        var presentation=UnityEngine.Object.FindAnyObjectByType<P3MetaHud>(FindObjectsInactive.Include);
        if(!presentation)presentation=new GameObject("P3 Meta HUD",typeof(RectTransform)).AddComponent<P3MetaHud>();
        presentation.Initialize(mobile.SafeRoot);
        runtime.ConfigureMeta(rules,presentation);
        var missionHud=UnityEngine.Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);missionHud.SetMetaMode(true);
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");
        var preview=new MetaProfile {weapons=new WeaponProgress[4]};for(int i=0;i<4;i++)preview.weapons[i]=new WeaponProgress {owned=i==0};
        missionHud.Refresh(catalog.missions[0],catalog.weapons[0],0,catalog.missions.Length,0,"Loadout","",0);
        missionHud.SetProgression(preview,rules,0,0);presentation.Refresh(preview,rules,catalog.weapons[0],0,0,"Loadout",null);
        EditorUtility.SetDirty(runtime);EditorUtility.SetDirty(presentation);EditorUtility.SetDirty(missionHud);
        EditorSceneManager.MarkSceneDirty(runtime.gameObject.scene);EditorSceneManager.SaveScene(runtime.gameObject.scene);AssetDatabase.SaveAssets();
        Debug.Log("CUBE_P3_INSTALL_OK: persistent wallet, arsenal upgrades and progression UI saved without resetting scene.");
    }
    static void InstallSounds()
    {
        const string folder="Assets/CubeInvasion/Art/P2/Audio/";
        var bank=AssetDatabase.LoadAssetAtPath<P2AudioBank>(folder+"P2AudioBank.asset");
        var entries=new List<P2AudioBank.Entry>(bank.sounds);
        foreach(var cue in new[]{CombatFeedback.Cue.Unlock,CombatFeedback.Cue.Upgrade,CombatFeedback.Cue.Denied})
        {
            if(entries.Exists(entry=>entry.cue==cue))continue;
            var clips=new AudioClip[2];
            for(int variation=0;variation<2;variation++)
            {
                float duration=cue==CombatFeedback.Cue.Unlock?.48f:cue==CombatFeedback.Cue.Upgrade?.24f:.16f;
                const int rate=22050;int samples=(int)(duration*rate);var data=new float[samples];float phase=0;
                for(int i=0;i<samples;i++)
                {
                    float t=i/(float)rate,u=t/duration;
                    float hz=(cue==CombatFeedback.Cue.Denied?190:cue==CombatFeedback.Cue.Unlock?660:880)*(1+variation*.035f);
                    hz*=cue==CombatFeedback.Cue.Denied?1-u*.3f:Mathf.Pow(2,Mathf.Floor(u*3)*4/12f);
                    phase+=2*Mathf.PI*hz/rate;
                    data[i]=(Mathf.Sin(phase)+.15f*Mathf.Sin(phase*2))*Mathf.Min(1,u*25)*Mathf.Pow(1-u,1.7f)*.35f;
                }
                string path=folder+"Meta_"+cue+"_"+variation+".wav";
                using(var writer=new BinaryWriter(File.Create(path)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                    writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples*2);foreach(float sample in data)writer.Write((short)(sample*32767));
                }
                AssetDatabase.ImportAsset(path);clips[variation]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            entries.Add(new P2AudioBank.Entry {cue=cue,variants=clips});
        }
        bank.sounds=entries.ToArray();EditorUtility.SetDirty(bank);
    }
    public static void InstallGameplayScene() { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
}
