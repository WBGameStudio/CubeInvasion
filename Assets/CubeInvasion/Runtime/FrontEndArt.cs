using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Front End Art")]
    public sealed class FrontEndArt : ScriptableObject
    {
        public Sprite menuHero, campaignIslands, scout, panel, disc;
        public Sprite casualButton;
        public Sprite journey,arsenal,pilot,settings,coin;
        public Font uiFont;
        public Sprite[] weapons;
    }
}
