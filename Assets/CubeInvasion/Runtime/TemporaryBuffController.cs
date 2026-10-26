using System;
namespace CubeInvasion.Prototype
{
    public sealed class TemporaryBuffController
    {
        public float DamageRemaining,MagnetRemaining;
        public void Tick(float seconds) {DamageRemaining=Math.Max(0,DamageRemaining-seconds);MagnetRemaining=Math.Max(0,MagnetRemaining-seconds);}
        public void Clear() {DamageRemaining=MagnetRemaining=0;}
    }
}
