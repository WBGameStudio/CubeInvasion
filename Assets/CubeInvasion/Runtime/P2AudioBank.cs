using System;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/P2 Audio Bank")]
    public sealed class P2AudioBank : ScriptableObject
    {
        [Serializable] public sealed class Entry { public CombatFeedback.Cue cue; public AudioClip[] variants; }
        public Entry[] sounds;
        public AudioClip combatLoop,bossLoop,goldLoop;
        public AudioClip Find(CombatFeedback.Cue cue,int variation)
        {
            if(sounds!=null)foreach(var entry in sounds)
                if(entry.cue==cue && entry.variants!=null && entry.variants.Length>0)return entry.variants[variation%entry.variants.Length];
            var expansion=Resources.Load<CampaignPresentation>("CampaignPresentation");
            if(expansion&&expansion.sounds!=null)foreach(var entry in expansion.sounds)if(entry.cue==cue&&entry.variants!=null&&entry.variants.Length>0)return entry.variants[variation%entry.variants.Length];
            return null;
        }
    }
}
