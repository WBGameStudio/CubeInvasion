using System;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    public enum ContentEnemy { Grunt, Sprinter, Gunner, Pusher, Sniper, Bruiser, Crusher }
    public enum ContentWeapon { PulsePistol, TwinBlasters, ScatterShot, RapidSmg }
    public enum MissionMode { Combat, GoldRush, Boss }
    [Serializable] public sealed class WeaponSpec
    {
        public string title, description, model;
        public ContentWeapon kind;
        public float damage=6, interval=.5f, projectileLife=1;
        public int pellets=1;
        public float spread;
        public Color color=Color.cyan;
        public float NominalDps => damage*pellets/interval;
    }
    [Serializable] public sealed class EncounterSpec
    {
        public ContentEnemy[] enemies;
        public bool cover, barrels, healing;
        public Vector2 coverOffset=new Vector2(0,.6f),healOffset=new Vector2(2.6f,1.4f);
        public int goldQuota=30;
    }
    [Serializable] public sealed class MissionSpec
    {
        public string title, description;
        public MissionMode mode;
        public Color accent=Color.cyan;
        public EncounterSpec[] cubes;
    }
    [CreateAssetMenu(menuName="Cube Invasion/P2 Content Catalog")]
    public sealed class P2ContentCatalog : ScriptableObject
    {
        public WeaponSpec[] weapons;
        public MissionSpec[] missions;
        public int pressureRevision;
    }
}
