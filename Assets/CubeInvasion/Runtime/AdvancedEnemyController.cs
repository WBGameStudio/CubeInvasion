using System;
namespace CubeInvasion.Prototype
{
    public enum AdvancedEnemyOrder { None, PrepareSummon, Summon, PrepareTeleport, Teleport, PrepareShot, Shoot, PreparePush, Push }
    // Pure decision state, separate from scene objects, collision, VFX and audio.
    public sealed class AdvancedEnemyController
    {
        public float clock,windup,weak;
        public int summoned,sequence;
        readonly ContentEnemy kind;
        public AdvancedEnemyController(ContentEnemy type) {kind=type;clock=type==ContentEnemy.Summoner?6:2.5f;}
        public AdvancedEnemyOrder Tick(float seconds,float remaining)
        {
            clock-=seconds;weak=Math.Max(0,weak-seconds);
            if(windup>0)
            {
                windup-=seconds;if(windup>0)return AdvancedEnemyOrder.None;
                if(kind==ContentEnemy.Summoner) {clock=6;if(remaining<=3)return AdvancedEnemyOrder.None;summoned=2;return AdvancedEnemyOrder.Summon;}
                if(kind==ContentEnemy.Phaser) {clock=2.5f;weak=.6f;return AdvancedEnemyOrder.Teleport;}
                clock=2;return sequence++%2==0?AdvancedEnemyOrder.Shoot:AdvancedEnemyOrder.Push;
            }
            if(clock>0)return AdvancedEnemyOrder.None;
            if(kind==ContentEnemy.Summoner)
            {if(summoned>=2||remaining<=4)return AdvancedEnemyOrder.None;windup=.8f;return AdvancedEnemyOrder.PrepareSummon;}
            if(kind==ContentEnemy.Phaser) {windup=.7f;return AdvancedEnemyOrder.PrepareTeleport;}
            windup=1;return sequence%2==0?AdvancedEnemyOrder.PrepareShot:AdvancedEnemyOrder.PreparePush;
        }
    }
}
