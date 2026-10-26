using UnityEngine;
namespace CubeInvasion.Prototype
{
    // "Void Toy Arcade" tokens. GDD §14 palette plus contrast-safe text variants.
    public static class VoidToyPalette
    {
        public static readonly Color Void=Hex(0x101A2D), Glass=new Color(.063f,.102f,.176f,.82f), Panel=Hex(0x17233D), Raised=Hex(0x22315A), RaisedShadow=Hex(0x121C36), Track=Hex(0x2A3A5C);
        public static readonly Color Floor=Hex(0xC9D6EA), Side=Hex(0x22315A), Graphite=Hex(0x3A4361);
        public static readonly Color Cobalt=Hex(0x395DE6), CobaltText=Hex(0x8EA6FF), Cyan=Hex(0x18BDD0), CyanText=Hex(0x7FE3EE), CyanShadow=Hex(0x0D8494);
        public static readonly Color Gold=Hex(0xFFBE4D), GoldShadow=Hex(0xC98A1E), Danger=Hex(0xE64B68), DangerText=Hex(0xFF6B85), DangerDeep=Hex(0x3A1426);
        public static readonly Color Health=Hex(0x16A989), HealthText=Hex(0x3FD9B4), Warm=Hex(0xFF9C32), Violet=Hex(0x8B7CF6);
        public static readonly Color Text=Hex(0xEEF3FB), Muted=Hex(0xA9B8D6), Ink=Hex(0x0B1426), Disabled=Hex(0x6F7EA0);
        public static Color Hex(int rgb) => new Color(((rgb>>16)&255)/255f,((rgb>>8)&255)/255f,(rgb&255)/255f,1);
        public static Color WithAlpha(Color c,float a) { c.a=a;return c; }
        // Melee threats read pink, ranged orange; silhouettes carry the rest.
        public static Color Intent(ContentEnemy kind) => kind==ContentEnemy.Sniper||kind==ContentEnemy.Gunner?Warm:Danger;
        public static Color Reward(RouteReward reward) => reward==RouteReward.Health?Health:reward==RouteReward.Gold||reward==RouteReward.Elite?Gold:
            reward==RouteReward.Shield||reward==RouteReward.Magnet?CyanText:reward==RouteReward.Blast?Warm:Cobalt;
        public static Color Risk(int risk) => risk>=3?Danger:risk==2?Warm:Gold;
    }
}
