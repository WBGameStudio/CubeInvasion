using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
using Cue=CubeInvasion.Prototype.CombatFeedback.Cue;

// Recorded CC0 audio for every CombatFeedback cue and music slot (sources: AUDIO_LICENSES.md at the repository root).
// Replaces the old code-synthesized WAVs; the content builders call Install() instead of generating sound.
public static class CubeAudioLibrary
{
    const string Root="Assets/CubeInvasion/Audio/";
    const string Sci=Root+"Kenney_SciFi/",Impact=Root+"Kenney_Impact/",Ui=Root+"Kenney_Interface/",Music=Root+"OGA_DarkSciFi/";
    static readonly (Cue cue,string[] files)[] Cues=
    {
        (Cue.Shot,new[]{Sci+"laserSmall_000",Sci+"laserSmall_001"}),
        (Cue.Hit,new[]{Impact+"impactMetal_light_001",Impact+"impactMetal_light_002"}),
        (Cue.Hurt,new[]{Impact+"impactPunch_medium_000",Impact+"impactPunch_medium_001"}),
        (Cue.Explosion,new[]{Sci+"explosionCrunch_000",Sci+"explosionCrunch_002"}),
        (Cue.Gold,new[]{Ui+"glass_002",Ui+"glass_003"}),
        (Cue.Heal,new[]{Sci+"forceField_000",Sci+"forceField_001"}),
        (Cue.Clear,new[]{Ui+"confirmation_001",Ui+"confirmation_003"}),
        (Cue.Alarm,new[]{Impact+"impactBell_heavy_004",Impact+"impactBell_heavy_003"}),
        (Cue.Fail,new[]{Sci+"lowFrequency_explosion_001",Sci+"lowFrequency_explosion_000"}),
        (Cue.Gate,new[]{Sci+"doorOpen_000",Sci+"doorOpen_001"}),
        (Cue.Twin,new[]{Sci+"laserRetro_000",Sci+"laserRetro_001"}),
        (Cue.Scatter,new[]{Sci+"laserLarge_000",Sci+"laserLarge_001"}),
        (Cue.Rapid,new[]{Sci+"laserSmall_002",Sci+"laserSmall_003"}),
        (Cue.Telegraph,new[]{Ui+"error_007",Ui+"error_008"}),
        (Cue.BossPhase,new[]{Sci+"explosionCrunch_004",Impact+"impactBell_heavy_001"}),
        (Cue.BossSlam,new[]{Impact+"impactPlate_heavy_000",Impact+"impactPlate_heavy_002"}),
        (Cue.Unlock,new[]{Ui+"bong_001",Ui+"maximize_005"}),
        (Cue.Upgrade,new[]{Ui+"maximize_006",Ui+"maximize_004"}),
        (Cue.Denied,new[]{Ui+"error_001",Ui+"error_002"}),
        (Cue.Pierce,new[]{Sci+"laserLarge_002",Sci+"laserLarge_003"}),
        (Cue.Ricochet,new[]{Sci+"impactMetal_002",Sci+"impactMetal_004"}),
        (Cue.Arc,new[]{Impact+"impactGlass_heavy_000",Impact+"impactGlass_heavy_003"}),
        (Cue.Rocket,new[]{Sci+"laserLarge_004",Sci+"laserRetro_004"}),
        (Cue.Flame,new[]{Sci+"slime_000",Impact+"impactSoft_heavy_001"}),
        (Cue.Cryo,new[]{Impact+"impactGlass_light_000",Impact+"impactGlass_light_001"}),
        (Cue.Drone,new[]{Ui+"drop_002",Ui+"drop_004"}),
        (Cue.Nova,new[]{Sci+"explosionCrunch_003",Sci+"explosionCrunch_001"}),
        (Cue.Summon,new[]{Sci+"forceField_002",Sci+"doorClose_000"}),
        (Cue.Teleport,new[]{Sci+"forceField_003",Sci+"forceField_004"}),
        (Cue.ShieldBlock,new[]{Impact+"impactMetal_medium_000",Impact+"impactMetal_medium_003"}),
        (Cue.Repair,new[]{Ui+"pluck_002",Ui+"maximize_003"}),
        (Cue.Victory,new[]{Music+"victory"}),
        (Cue.UiClick,new[]{Ui+"click_001",Ui+"select_007"}),
    };
    // Menu/legacy combat, boss, gold quota; biomes 0-5 and the low-time pressure layer (index 6).
    const string MenuLoop=Music+"airy",BossLoop=Music+"urgent",GoldLoop=Music+"pulse";
    static readonly string[] Biomes={Music+"sector",Music+"pulse",Music+"transmission",Music+"sector",Music+"pulse",Music+"transmission",Music+"urgent"};

    // Overall loudness after play-testing feedback ("too loud"): effects about -5 dB, music about -4.4 dB.
    // Levels only; every clip keeps its full original length.
    const float EffectsTrim=.55f,MusicTrim=.6f;
    [MenuItem("Cube Invasion/Audio/Install Recorded Audio")]
    public static void Install()
    {
        var bank=AssetDatabase.LoadAssetAtPath<P2AudioBank>("Assets/CubeInvasion/Art/P2/Audio/P2AudioBank.asset");
        var campaign=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        if(!bank||!campaign)throw new InvalidOperationException("Audio bank or campaign presentation asset is missing.");
        var entries=new List<P2AudioBank.Entry>();var levels=new Dictionary<Cue,float>();
        foreach(var (cue,files) in Cues)
        {
            var clips=files.Select(f=>Sfx(f,cue==Cue.Victory)).ToArray();
            levels[cue]=clips.Average(Rms);entries.Add(new P2AudioBank.Entry {cue=cue,variants=clips});
        }
        // Loudness match: every cue lands near the median level; the game's per-cue mix (quiet shots, louder events) stays on top.
        float median=levels.Values.OrderBy(v=>v).ElementAt(levels.Count/2);
        foreach(var entry in entries)entry.gain=(float)Math.Round(Mathf.Clamp(median/Mathf.Max(.0001f,levels[entry.cue]),.35f,1.6f)*EffectsTrim,2);
        bank.sounds=entries.ToArray();
        float musicRms=new[]{MenuLoop,BossLoop,GoldLoop}.Average(TrackRms);
        bank.combatLoop=Track(MenuLoop);bank.bossLoop=Track(BossLoop);bank.goldLoop=Track(GoldLoop);
        // Music sits a little under the effects: about a third of the effects' median level.
        Debug.Log($"CUBE_MUSIC_RMS {musicRms:0.0000}");
        bank.musicVolume=(float)Math.Round(Mathf.Clamp(median*.6f/Mathf.Max(.0001f,musicRms)*.35f*MusicTrim,.05f,.6f),2);
        campaign.biomeMusic=Biomes.Select(Track).ToArray();
        int first=(int)Cue.Pierce,last=(int)Cue.Repair;
        campaign.sounds=entries.Where(e=>(int)e.cue>=first&&(int)e.cue<=last).ToArray();
        EditorUtility.SetDirty(bank);EditorUtility.SetDirty(campaign);AssetDatabase.SaveAssets();
        Debug.Log($"CUBE_RECORDED_AUDIO_OK cues={entries.Count} music={bank.musicVolume} median={median:0.000} "+string.Join(" ",entries.Select(e=>$"{e.cue}:{e.gain}")));
    }
    public static AudioClip[] BiomeMusic()=>Biomes.Select(Track).ToArray();
    static AudioClip Sfx(string path,bool jingle)
    {
        var importer=Importer(path);var settings=importer.defaultSampleSettings;
        settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=jingle?.5f:.6f;
        settings.sampleRateSetting=AudioSampleRateSetting.OptimizeSampleRate;
        importer.forceToMono=!jingle;importer.loadInBackground=false;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
        return Load(path);
    }
    static AudioClip Track(string path)
    {
        var importer=Importer(path);var settings=importer.defaultSampleSettings;
        // Long loops stay compressed in memory. Quality applies to device builds; WebGL rejects audio overrides and
        // ships the source encoding (about 5.3 MB for the five tracks).
        settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.35f;
        settings.sampleRateSetting=AudioSampleRateSetting.OptimizeSampleRate;
        importer.forceToMono=false;importer.loadInBackground=true;importer.defaultSampleSettings=settings;
        importer.SaveAndReimport();
        return Load(path);
    }
    // Compressed-in-memory clips can't be read back in the editor; measure a decoded import first.
    static float TrackRms(string path)
    {
        var importer=Importer(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;
        importer.loadInBackground=false;importer.defaultSampleSettings=settings;importer.SaveAndReimport();return Rms(Load(path));
    }
    static AudioImporter Importer(string path)=>AssetImporter.GetAtPath(path+".ogg") as AudioImporter??throw new InvalidOperationException("Missing audio "+path+".ogg");
    static AudioClip Load(string path)=>AssetDatabase.LoadAssetAtPath<AudioClip>(path+".ogg");
    static float Rms(AudioClip clip)
    {
        if(!clip||clip.samples==0)return 0;
        if(clip.loadState!=AudioDataLoadState.Loaded)clip.LoadAudioData();
        int count=Mathf.Min(clip.samples,clip.frequency*20)*clip.channels;var data=new float[count];
        if(!clip.GetData(data,0))return 0;
        double sum=0;foreach(float v in data)sum+=v*v;return (float)Math.Sqrt(sum/count);
    }
}
