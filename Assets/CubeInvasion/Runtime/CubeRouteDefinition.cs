using UnityEngine;

namespace CubeInvasion.Prototype
{
    public enum RouteReward { Health, Damage }

    [CreateAssetMenu(menuName = "Cube Invasion/Route")]
    public sealed class CubeRouteDefinition : ScriptableObject
    {
        public string title;
        public RouteReward reward;
        public float value = 30;
        public int duration = 2;
        [Range(1, 3)] public int risk = 1;
        public int grunts = 3;
        public int sprinters;
    }
}
