using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Support Enemies")]
    public sealed class SupportEnemySettings : ScriptableObject
    {
        public bool enabled=true;
        [Min(0)] public int missionIndex=4;
        [Range(10,80)] public float shieldTurnSpeed=45;
        [Min(.5f)] public float healInterval=1.8f;
        [Range(.3f,.8f)] public float healWindup=.55f;
        [Range(1,5)] public float healRadius=3.2f;
        [Min(0)] public float healing=5;
        public static MissionSpec Expand(MissionSpec source)
        {
            var cubes=(EncounterSpec[])source.cubes.Clone();
            for(int i=3;i<=4&&i<cubes.Length;i++)
            {var old=cubes[i];cubes[i]=new EncounterSpec {enemies=i==3?new[]{ContentEnemy.Shielder,ContentEnemy.Grunt,ContentEnemy.Gunner}:new[]{ContentEnemy.Shielder,ContentEnemy.Healer,ContentEnemy.Grunt},
                cover=old.cover,barrels=old.barrels,healing=old.healing,coverOffset=old.coverOffset,healOffset=old.healOffset,goldQuota=old.goldQuota,goldVariant=old.goldVariant};}
            return new MissionSpec {title=source.title,description=source.description,mode=source.mode,accent=source.accent,cubes=cubes};
        }
    }
}
