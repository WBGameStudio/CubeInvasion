using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Combat Balance")]
    public sealed class CombatBalanceSettings : ScriptableObject
    {
        [Min(.1f)] public float enemyMoveMultiplier=1.2f;
        [Min(.1f)] public float enemyRecoveryMultiplier=.8f;
        [Min(.1f)] public float enemyDamageMultiplier=1.3f;
        [Header("One-thumb fire rhythm / joystick distance from center")]
        [Range(0,.6f)] public float focusRadius=.25f;
        [Range(.6f,1)] public float runningRadius=.85f;
        [Min(1)] public float runningShotIntervalMultiplier=1.4f;
    }
}
