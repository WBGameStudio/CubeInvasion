using UnityEngine;
namespace CubeInvasion.Prototype
{
    public enum GoldRushVariant { CoinSprint, CrateBreaker, GoldenSwarm, Jackpot }
    [CreateAssetMenu(menuName="Cube Invasion/Gold Rush Variants")]
    public sealed class GoldRushVariantSettings : ScriptableObject
    {
        public bool enabled=true;
        [Min(1)] public float crateHealth=6,jackpotHealth=24,carrierHealth=12;
        [Min(1)] public int crateGold=8,carrierGold=12,jackpotGold=60;
        public static GoldRushVariant ForStage(int stage,int seed)=>stage<=1?GoldRushVariant.CoinSprint:stage==2?GoldRushVariant.CrateBreaker:(seed&1)==0?GoldRushVariant.GoldenSwarm:GoldRushVariant.Jackpot;
        public static ContentEnemy[] Enemies(GoldRushVariant variant,ContentEnemy[] source)
        {
            if(variant==GoldRushVariant.GoldenSwarm)return new[]{ContentEnemy.Grunt,ContentEnemy.Grunt,ContentEnemy.Grunt,ContentEnemy.Sprinter,ContentEnemy.Sprinter};
            if(variant==GoldRushVariant.Jackpot) {var guards=new ContentEnemy[3];for(int i=0;i<3;i++)guards[i]=i<source.Length?source[i]:ContentEnemy.Grunt;return guards;}
            return source;
        }
        public static string Name(GoldRushVariant variant)=>variant==GoldRushVariant.CrateBreaker?"6 KASA":variant==GoldRushVariant.GoldenSwarm?"ALTIN SÜRÜSÜ":variant==GoldRushVariant.Jackpot?"JACKPOT":"ALTIN TOPLA";
    }
}
