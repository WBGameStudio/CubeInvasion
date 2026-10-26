using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class CampaignAudioBuilder
{
    public static AudioClip[] Build(string folder)
    {
        Directory.CreateDirectory(folder);var clips=new AudioClip[7];int[] roots={48,43,45,50,41,42};
        for(int biome=0;biome<7;biome++)
        {
            const int rate=22050;float bpm=112+Math.Min(biome,5)*4;float beat=60/bpm;int count=(int)(rate*beat*16);var data=new float[count];
            for(int i=0;i<count;i++)
            {
                float t=i/(float)rate;int step=(int)(t/(beat*.5f));float phase=t%(beat*.5f),env=Mathf.Exp(-phase*18);int[] scale={0,3,7,10,7,3,5,7};float hz=440*Mathf.Pow(2,(roots[Math.Min(biome,5)]+scale[step%8]-69)/12f);
                float lead=Mathf.Sin(t*hz*Mathf.PI*2)*env*.13f;float kick=Mathf.Sin((t%beat)*Mathf.PI*2*(55+60*Mathf.Exp(-(t%beat)*35)))*Mathf.Exp(-(t%beat)*22)*.22f;
                float bass=Mathf.Sin(t*(hz*.5f)*Mathf.PI*2)*.05f;
                data[i]=biome==6?Mathf.Sin(t*180*Mathf.PI*2)*env*.08f:lead+kick+bass;
            }
            string path=folder+(biome==6?"/Pressure.wav":"/Biome_"+biome+".wav");
            using(var stream=new BinaryWriter(File.Create(path)))
            {stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));stream.Write(36+count*2);stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));stream.Write(16);stream.Write((short)1);stream.Write((short)1);stream.Write(rate);stream.Write(rate*2);stream.Write((short)2);stream.Write((short)16);stream.Write(System.Text.Encoding.ASCII.GetBytes("data"));stream.Write(count*2);foreach(float sample in data)stream.Write((short)(Mathf.Clamp(sample,-1,1)*32767));}
            AssetDatabase.ImportAsset(path);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();clips[biome]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return clips;
    }
}
