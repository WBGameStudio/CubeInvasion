using UnityEngine;
// Expansion weapon/enemy cues now come from recorded CC0 audio (see CubeAudioLibrary); nothing is synthesized.
public static class CampaignSoundBuilder
{
    public static void Build() { CubeAudioLibrary.Install();Debug.Log("CUBE_EXPANSION_SOUNDS_OK: recorded clips installed"); }
}
