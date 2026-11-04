using UnityEngine;
// Biome music now uses the recorded OpenGameArt loops (see CubeAudioLibrary); nothing is synthesized.
public static class CampaignAudioBuilder
{
    public static AudioClip[] Build(string folder)=>CubeAudioLibrary.BiomeMusic();
}
