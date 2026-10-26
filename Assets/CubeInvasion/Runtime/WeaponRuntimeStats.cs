namespace CubeInvasion.Prototype
{
    // Resolves permanent progression and temporary buffs without reading scene or UI state.
    public readonly struct WeaponRuntimeStats
    {
        public readonly float Damage,Interval,Range;
        public readonly int Pellets,SpecialRank;
        public float Dps=>Damage*Pellets/Interval;
        public WeaponRuntimeStats(WeaponSpec weapon,WeaponProgress progress,P3MetaSettings rules,float routeDamage=0,float routeRate=0,bool amplifier=false)
        {
            bool upgraded=rules&&progress!=null;SpecialRank=upgraded?progress.special:0;
            float damage=upgraded?rules.damagePerLevel*progress.damage:0,rate=upgraded?rules.fireRatePerLevel*progress.fireRate:0,range=upgraded?rules.rangePerLevel*progress.range:0;
            Damage=weapon.damage*(1+damage)*(1+routeDamage)*(amplifier?1.3f:1);
            if((int)weapon.kind<4&&weapon.kind!=ContentWeapon.ScatterShot)Damage*=1+SpecialRank*.08f;
            Pellets=weapon.pellets+(weapon.kind==ContentWeapon.ScatterShot?SpecialRank:0);
            Interval=weapon.interval/((1+rate)*(1+routeRate)*(1+(upgraded?progress.cooldown*.04f:0)));
            Range=weapon.EffectiveRange*(1+range);
        }
    }
}
