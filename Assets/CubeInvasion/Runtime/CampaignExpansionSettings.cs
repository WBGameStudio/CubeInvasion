using System;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public enum ArenaLayout { Classic, Narrow, Pillars, SplitLane, HazardGrid, ExplosiveYard, BossArena }
    public enum ArenaHazard { None, Electric, Ice, Spike, Lava }
    public enum SpecialRule { None, SurvivalFlip, BarrelParty, EliteHunt, ChainReactor, LowGravity, HealingStop, Gauntlet }
    public enum BossIdentity { Crusher, FurnaceKing, MotherboardHydra, GlacierWarden, MagmaTitan, VoidArchitect }
    [CreateAssetMenu(menuName="Cube Invasion/Full Campaign")]
    public sealed class CampaignExpansionSettings : ScriptableObject
    {
        public bool enabled=true;
        public int seed=2601026;
        public P2ContentCatalog catalog;
        public int[] unlockPrices=(int[])Prices.Clone();
        public static readonly int[] Prices={0,150,350,600,850,1150,1500,1900,2300,2800,3400,4200};
        public static readonly string[] Biomes={"Neon Yard","Factory Rift","Circuit Abyss","Frozen Blocks","Lava Foundry","Void Core"};
        public static readonly Color[] Colors={new Color(.1f,.8f,.85f),new Color(1,.58f,.2f),new Color(.5f,.4f,1),new Color(.4f,.85f,1),new Color(1,.3f,.12f),new Color(.85f,.25f,.85f)};
        public static P2ContentCatalog Expand(P2ContentCatalog source,int seed)
        {
            var catalog=Instantiate(source);catalog.name=source.name+" Runtime 60";
            var weapons=new WeaponSpec[12];Array.Copy(source.weapons,weapons,Math.Min(4,source.weapons.Length));
            string[] names={"Pierce Rifle","Ricochet Gun","Arc Launcher","Rocket Pod","Flame Sprayer","Cryo Beam","Orbit Drones","Nova Cannon"};
            string[] roles={"Sıralı hedefleri deler; ön kalkanı aşar.","İki ek hedefe seker; siper bağlantıyı keser.","Yakın üç hedefe zincir yıldırım.","Uyarılı alan patlaması; yakında durma.","Kısa menzilli alev konisi.","Hedefi yavaşlatan kesintisiz ışın.","Çevrende dönen iki savunma dronu.","Yavaş, geniş ve itici enerji dalgası."};
            float[] dmg={12,8,7,24,2,3,5,25},rate={1,.8f,.65f,2,.16f,.25f,.45f,2.2f};
            for(int i=4;i<12;i++)weapons[i]=new WeaponSpec {title=names[i-4],description=roles[i-4],kind=(ContentWeapon)i,model="Expansion "+(ContentWeapon)i,damage=dmg[i-4],interval=rate[i-4],projectileLife=i==8?.3f:1,pellets=1,color=Colors[(i-4)%6]};
            catalog.weapons=weapons;catalog.missions=new MissionSpec[60];
            // Keep the seven legacy slots and every saved index stable; L6/L7 remain bonus introductions.
            Array.Copy(source.missions,catalog.missions,Math.Min(7,source.missions.Length));
            for(int level=8;level<=60;level++)
            {
                int biome=(level-1)/10;bool boss=level%10==0,gold=!boss&&(level==15||level==35||level==55);
                var special=!boss&&!gold?Rule(level):SpecialRule.None;int count=special==SpecialRule.Gauntlet?5:boss?(level==30||level>=50?4:3):gold?3:level<11?4:4+(level%2);
                var mission=new MissionSpec {title=boss?BossName((BossIdentity)(level/10-1)):gold?"Gold Rush · "+Biomes[biome]:Biomes[biome]+" · "+level,
                    description=boss?"Her fazın hedefini 20 saniyede tamamla.":gold?"Altın, kasalar ve kilitli jackpot.":special!=SpecialRule.None?special+" · Tehlikeyi oku, kapını seç.":"Yeni mekanikler, açık kaçış yolları ve iki ödül rotası.",
                    mode=boss?MissionMode.Boss:gold?MissionMode.GoldRush:MissionMode.Combat,accent=Colors[biome],biome=biome,boss=(BossIdentity)Mathf.Max(0,level/10-1),special=special,cubes=new EncounterSpec[count]};
                for(int c=0;c<count;c++)
                {
                    var random=new System.Random(seed+level*193+c*7919);var types=Pool(level);var primary=types[random.Next(types.Length)];
                    var layout=boss?ArenaLayout.BossArena:(ArenaLayout)((level+c)%6);var hazard=biome==2?ArenaHazard.Electric:biome==3?ArenaHazard.Ice:biome==4?ArenaHazard.Lava:level>15&&(level+c)%4==0?ArenaHazard.Spike:ArenaHazard.None;
                    mission.cubes[c]=new EncounterSpec {enemies=boss?new[]{ContentEnemy.Crusher}:special==SpecialRule.EliteHunt?new[]{ContentEnemy.Elite}:special==SpecialRule.Gauntlet?new[]{ContentEnemy.Sprinter,ContentEnemy.Gunner,ContentEnemy.Sprinter}:gold?new[]{ContentEnemy.Grunt,ContentEnemy.Sprinter}:primary==ContentEnemy.Elite||primary==ContentEnemy.Shielder?new[]{primary,ContentEnemy.Grunt}:new[]{primary,ContentEnemy.Grunt,level>12?primary:ContentEnemy.Sprinter},cover=layout==ArenaLayout.Pillars||layout==ArenaLayout.SplitLane,barrels=layout==ArenaLayout.ExplosiveYard||special==SpecialRule.BarrelParty,healing=special==SpecialRule.HealingStop||(level+c)%4==0,goldQuota=35+c*10,layout=layout,hazard=hazard};
                }
                catalog.missions[level-1]=mission;
            }
            return catalog;
        }
        public static string BossName(BossIdentity id)=>new[]{"The Crusher","Furnace King","Motherboard Hydra","Glacier Warden","Magma Titan","Void Architect"}[(int)id];
        static SpecialRule Rule(int level)=>level%10==5?SpecialRule.Gauntlet:level%10==8?SpecialRule.EliteHunt:level%10==9?SpecialRule.ChainReactor:level%10==2?SpecialRule.LowGravity:level%10==3?SpecialRule.SurvivalFlip:level%10==4?SpecialRule.HealingStop:level%10==6?SpecialRule.BarrelParty:SpecialRule.None;
        static ContentEnemy[] Pool(int level)=>level<11?new[]{ContentEnemy.Gunner,ContentEnemy.Pusher,ContentEnemy.Sniper}:level<21?new[]{ContentEnemy.Bomber,ContentEnemy.Bruiser,ContentEnemy.Shielder}:level<26?new[]{ContentEnemy.Shielder,ContentEnemy.Sniper,ContentEnemy.Healer}:level<41?new[]{ContentEnemy.Summoner,ContentEnemy.Healer,ContentEnemy.Phaser}:new[]{ContentEnemy.Summoner,ContentEnemy.Phaser,ContentEnemy.Elite};
    }
}
