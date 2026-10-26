using System.IO;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
public static class CampaignSoundBuilder
{
    public static void Build()
    {
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");const string folder="Assets/CubeInvasion/Art/Expansion/Audio";Directory.CreateDirectory(folder);var entries=new P2AudioBank.Entry[12];
        for(int cue=0;cue<12;cue++)
        {
            entries[cue]=new P2AudioBank.Entry {cue=(CombatFeedback.Cue)((int)CombatFeedback.Cue.Pierce+cue),variants=new AudioClip[2]};
            for(int variation=0;variation<2;variation++)
            {
                const int rate=22050;int count=(int)(rate*(cue==3||cue==7?.35f:cue>=8?.3f:.14f));string path=folder+"/"+entries[cue].cue+"_"+variation+".wav";uint noise=17;
                using(var writer=new BinaryWriter(File.Create(path)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                    for(int i=0;i<count;i++) {float t=i/(float)rate,p=i/(float)count;noise=noise*1664525+1013904223;float random=((noise>>16)/65535f)*2-1;float frequency=(cue==3||cue==7?95:350+cue*95)*(variation==0?1:1.06f);float wave=Mathf.Sin(t*frequency*(cue==8||cue==11?1+p:1-p*.65f)*Mathf.PI*2);float value=(wave*.7f+(cue==3||cue==4?random*.5f:random*.08f))*Mathf.Sin(p*Mathf.PI)*Mathf.Pow(1-p,1.5f)*.32f;writer.Write((short)(Mathf.Clamp(value,-1,1)*32767));}
                }
                AssetDatabase.ImportAsset(path);entries[cue].variants[variation]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
        }
        presentation.sounds=entries;EditorUtility.SetDirty(presentation);AssetDatabase.SaveAssets();Debug.Log("CUBE_EXPANSION_SOUNDS_OK: twelve cues, 24 original variations");
    }
}
