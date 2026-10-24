using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Front End Art")]
    public sealed class FrontEndArt : ScriptableObject
    {
        public Sprite menuHero, campaignIslands, scout, panel, disc;
        public Sprite[] weapons;
    }
}
