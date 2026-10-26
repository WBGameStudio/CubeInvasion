using System;
namespace CubeInvasion.Prototype
{
    public enum CubeOutcome { Active, Cleared, Timeout }
    // The clock owns frame clipping and terminal ordering; presentation never changes its result.
    public readonly struct CubeCombatClock
    {
        public readonly float Delta,Remaining;
        public CubeCombatClock(float remaining,float frameSeconds)
        {Delta=Math.Max(0,Math.Min(frameSeconds,remaining));Remaining=Math.Max(0,remaining-Delta);}
        public CubeOutcome Resolve(bool objectiveComplete)=>objectiveComplete?CubeOutcome.Cleared:Remaining<=0?CubeOutcome.Timeout:CubeOutcome.Active;
    }
}
