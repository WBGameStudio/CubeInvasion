using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed class CampaignPresentation : ScriptableObject
    {
        public ToyArtLibrary art;
        public Sprite[] weapons;
        public AudioClip[] biomeMusic;
        public P2AudioBank.Entry[] sounds;
    }
}
