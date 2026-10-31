namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        // Per-type tempo in one place: move speed (m/s, before EnemyMoveMultiplier), telegraph windup (s)
        // and recovery after an attack (s, before EnemyRecoveryMultiplier). Telegraphs stay readable;
        // difficulty comes from speed, shorter recovery and harder pushes.
        struct EnemyTempo { public float speed, windup, recovery; }
        static EnemyTempo Tempo(EnemyKind kind)
        {
            switch(kind)
            {
                case EnemyKind.Grunt:    return new EnemyTempo {speed=2.3f,windup=.5f,recovery=.6f};
                case EnemyKind.Sprinter: return new EnemyTempo {speed=3.8f,windup=.6f,recovery=.95f};
                case EnemyKind.Gunner:   return new EnemyTempo {speed=1.9f,windup=.9f,recovery=1.05f};
                // Pushers hunt the hero down almost at running speed so their shove lands near edges often.
                case EnemyKind.Pusher:   return new EnemyTempo {speed=3.1f,windup=.85f,recovery=.8f};
                case EnemyKind.Sniper:   return new EnemyTempo {speed=1.6f,windup=1.1f,recovery=1.35f};
                case EnemyKind.Bruiser:  return new EnemyTempo {speed=1.45f,windup=.8f,recovery=.95f};
                case EnemyKind.Crusher:  return new EnemyTempo {speed=.5f,windup=1.2f,recovery=1.8f};
                default:                 return new EnemyTempo {speed=1.05f,windup=.8f,recovery=1.6f};
            }
        }
        // Pusher shove: knockback speed (decays ~10/s, so 16 carries the hero about 1.6 m) and cone reach.
        const float PushForce=16, PushReach=2.8f;
        // Sprinter dash speed (m/s) over its 0.32 s dash.
        const float SprintDashSpeed=10.5f;
    }
}
