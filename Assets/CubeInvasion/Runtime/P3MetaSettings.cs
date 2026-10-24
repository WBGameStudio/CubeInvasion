using UnityEngine;
namespace CubeInvasion.Prototype
{
    public enum CharacterUpgrade { Vitality, Agility, Magnet }
    public enum WeaponUpgrade { Damage, FireRate, Range }
    [CreateAssetMenu(menuName="Cube Invasion/P3 Economy Settings")]
    public sealed class P3MetaSettings : ScriptableObject
    {
        public int characterCap=5;
        public int vitalityCost=60,agilityCost=70,magnetCost=45;
        public int CharacterCost(CharacterUpgrade kind,int level)=>Mathf.Max(1,kind==CharacterUpgrade.Vitality?vitalityCost:kind==CharacterUpgrade.Agility?agilityCost:magnetCost)*(level+1);
        public int[] unlockPrices={0,150,350,600};
        public int damageCap=10, fireRateCap=5, rangeCap=5;
        public int damageBaseCost=35, fireRateBaseCost=50, rangeBaseCost=30;
        public float damagePerLevel=.05f, fireRatePerLevel=.04f, rangePerLevel=.04f;
        public int Price(int weapon)=>weapon>=0&&weapon<unlockPrices.Length?Mathf.Max(0,unlockPrices[weapon]):int.MaxValue;
        public int Cap(WeaponUpgrade kind)=>kind==WeaponUpgrade.Damage?damageCap:kind==WeaponUpgrade.FireRate?fireRateCap:rangeCap;
        public int UpgradeCost(WeaponUpgrade kind,int currentLevel)
        {
            if(currentLevel>=Cap(kind))return 0;
            int basic=kind==WeaponUpgrade.Damage?damageBaseCost:kind==WeaponUpgrade.FireRate?fireRateBaseCost:rangeBaseCost;
            return Mathf.Max(1,basic)*(currentLevel+1);
        }
    }
}
