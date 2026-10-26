using UnityEngine;
namespace CubeInvasion.Prototype
{
    [CreateAssetMenu(menuName="Cube Invasion/Bomber Encounters")]
    public sealed class BomberSettings : ScriptableObject
    {
        public bool enabled=true;
        [Min(0)] public int missionIndex=4;
        [Range(.65f,1.5f)] public float fuse=.8f;
        [Range(1,2.5f)] public float radius=2.2f;
        [Min(0)] public float blastDamage=10;
    }
    public static class BomberEncounters
    {
        public static MissionSpec Expand(MissionSpec source)
        {
            var cubes=(EncounterSpec[])source.cubes.Clone();
            for(int i=1;i<=2&&i<cubes.Length;i++)
            {
                var old=cubes[i];cubes[i]=new EncounterSpec {enemies=i==1?new[]{ContentEnemy.Bomber,ContentEnemy.Bomber,ContentEnemy.Bruiser}:new[]{ContentEnemy.Bomber,ContentEnemy.Bomber,ContentEnemy.Gunner,ContentEnemy.Grunt},
                    cover=old.cover,barrels=true,healing=old.healing,coverOffset=old.coverOffset,healOffset=old.healOffset,goldQuota=old.goldQuota,goldVariant=old.goldVariant};
            }
            return new MissionSpec {title=source.title,description=source.description,mode=source.mode,accent=source.accent,cubes=cubes};
        }
    }
}
