using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Crusher Phase Objectives")]
    public sealed class CrusherPhaseSettings : ScriptableObject
    {
        public bool enabled=true;
        [Min(.6f)] public float pushWindup=1.2f;
        [Min(.5f)] public float pushRecovery=1.8f;
        [Min(0)] public float pushDamage=5;
        [Range(0,9)] public float pushStrength=6;
        [Range(1.5f,3)] public float pushRange=2.5f;
    }
}
