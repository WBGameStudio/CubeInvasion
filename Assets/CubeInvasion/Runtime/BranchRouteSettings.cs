using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Branch Routes")]
    public sealed class BranchRouteSettings : ScriptableObject
    {
        public int seed=48271;
        public CubeRouteDefinition[] rewards;
        [Range(.4f,.9f)] public float effectiveDps=.72f;
        [Range(1,5)] public float movementAllowance=2.5f;
        [Range(1,1.6f)] public float eliteHealth=1.35f;
    }
}
