using System;
using System.IO;
using System.Linq;
using System.Text;
using CubeInvasion.Prototype;
using UnityEditor;
using UnityEngine;

// Playgama 3.3.2: every clip that can reach the player is a recorded, licensed OGG under Assets/CubeInvasion/Audio,
// every cue and music slot has one, and no code path synthesizes sound.
public static class AudioComplianceCheck
{
    const string Licensed="Assets/CubeInvasion/Audio/";
    [MenuItem("Cube Invasion/Audio/Run Compliance Check")]
    public static string Run()
    {
        var log=new StringBuilder();int failures=0;
        void Check(bool ok,string name) { log.AppendLine((ok?"PASS ":"FAIL ")+name);if(!ok)failures++; }
        var bank=AssetDatabase.LoadAssetAtPath<P2AudioBank>("Assets/CubeInvasion/Art/P2/Audio/P2AudioBank.asset");
        var campaign=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        foreach(CombatFeedback.Cue cue in Enum.GetValues(typeof(CombatFeedback.Cue)))
            Check(bank.Find(cue,0)&&bank.Find(cue,1),$"{cue} has recorded variants");
        Check(bank.combatLoop&&bank.bossLoop&&bank.goldLoop,"Menu, boss and gold loops assigned");
        Check(campaign.biomeMusic!=null&&campaign.biomeMusic.Length==7&&campaign.biomeMusic.All(c=>c),"Six biome tracks + pressure layer assigned");
        Check(bank.musicVolume>0&&bank.musicVolume<=.6f,"Music bed level set from measurement");
        // Everything the release build can load: the build scene and all Resources assets.
        var roots=new[]{"Assets/CubeInvasion/CubeInvasionPrototype.unity"}.Concat(AssetDatabase.GetAllAssetPaths().Where(p=>p.Contains("/Resources/")&&p.StartsWith("Assets/"))).ToArray();
        var clips=AssetDatabase.GetDependencies(roots,true).Where(p=>AssetDatabase.GetMainAssetTypeAtPath(p)==typeof(AudioClip)).Distinct().OrderBy(p=>p).ToArray();
        Check(clips.Length>0,$"Build references {clips.Length} audio clips");
        foreach(var path in clips)Check(path.StartsWith(Licensed)&&path.EndsWith(".ogg",StringComparison.OrdinalIgnoreCase),"Licensed OGG: "+path);
        Check(File.Exists("AUDIO_LICENSES.md"),"AUDIO_LICENSES.md present at the repository root");
        string license=File.Exists("AUDIO_LICENSES.md")?File.ReadAllText("AUDIO_LICENSES.md"):"";
        foreach(var path in clips)Check(license.Contains(Path.GetFileName(path)),"Documented: "+Path.GetFileName(path));
        // No synthesis left in game or builder code.
        var code=Directory.GetFiles("Assets/CubeInvasion","*.cs",SearchOption.AllDirectories).Where(f=>!f.EndsWith("AudioComplianceCheck.cs"));
        var synth=code.Where(f=> { var t=File.ReadAllText(f);return t.Contains("AudioClip.Create")||t.Contains("\"RIFF\"")||t.Contains("OnAudioFilterRead"); }).ToArray();
        Check(synth.Length==0,"No AudioClip.Create / WAV writer / filter synthesis in game code"+(synth.Length>0?": "+string.Join(", ",synth):""));
        string result=(failures==0?"CUBE_AUDIO_COMPLIANCE_OK":"CUBE_AUDIO_COMPLIANCE_FAILED "+failures)+"\n"+log;
        Debug.Log(result);return result;
    }
}
