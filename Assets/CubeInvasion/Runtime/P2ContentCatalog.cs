using System;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    public enum ContentEnemy { Grunt, Sprinter, Gunner, Pusher, Sniper, Bruiser, Crusher, Bomber, Shielder, Healer, Summoner, Phaser, Elite, ObjectiveNode }
    public enum ContentWeapon { PulsePistol, TwinBlasters, ScatterShot, RapidSmg, PierceRifle, RicochetGun, ArcLauncher, RocketPod, FlameSprayer, CryoBeam, OrbitDrones, NovaCannon }
    public enum MissionMode { Combat, GoldRush, Boss }
    [Serializable] public sealed class WeaponSpec
    {
        public string title, description, model;
        public ContentWeapon kind;
        public float damage=6, interval=.5f, projectileLife=1;
        public int pellets=1;
        public float spread;
        public Color color=Color.cyan;
        public float EffectiveRange=>kind==ContentWeapon.FlameSprayer?2.3f:kind==ContentWeapon.CryoBeam||kind==ContentWeapon.ArcLauncher?8:18*projectileLife;
        public float NominalDps => damage*pellets/interval;
    }
    [Serializable] public sealed class EncounterSpec
    {
        public ContentEnemy[] enemies;
        public bool cover, barrels, healing;
        public Vector2 coverOffset=new Vector2(0,.6f),healOffset=new Vector2(2.6f,1.4f);
        public int goldQuota=30;
        public GoldRushVariant goldVariant;
        public ArenaLayout layout;
        public ArenaHazard hazard;
    }
    [Serializable] public sealed class MissionSpec
    {
        public string title, description;
        public MissionMode mode;
        public Color accent=Color.cyan;
        public EncounterSpec[] cubes;
        public int biome;
        public SpecialRule special;
        public BossIdentity boss;
    }
    [CreateAssetMenu(menuName="Cube Invasion/P2 Content Catalog")]
    public sealed class P2ContentCatalog : ScriptableObject
    {
        public WeaponSpec[] weapons;
        public MissionSpec[] missions;
        public int pressureRevision;
    }
}
