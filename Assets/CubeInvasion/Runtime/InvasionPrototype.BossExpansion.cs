using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        Foe expandedBoss;
        float bossWorldClock,bossSafeTime,bossNextAttack,bossLockedWindup;
        int furnaceBarrels,bossWave;
        Vector3 bossAttackAim;
        readonly List<Foe> bossDecoys=new List<Foe>();
        bool HasExpandedBoss=>Expanded&&IsBoss&&Mission.boss!=BossIdentity.Crusher;
        bool ExpandedBossComplete=>!HasExpandedBoss||expandedBoss==null||!foes.Contains(expandedBoss);
        bool NodePhase=>HasExpandedBoss&&(Mission.boss==BossIdentity.FurnaceKing?stage==1:Mission.boss==BossIdentity.MotherboardHydra?stage<CubeCount:Mission.boss==BossIdentity.GlacierWarden?stage<3:Mission.boss==BossIdentity.MagmaTitan?stage<4:stage<4);
        string ExpandedBossObjective
        {
            get
            {
                if(Mission.boss==BossIdentity.FurnaceKing)return stage==1?$"BOSS YAKININDA VARİL · {furnaceBarrels}/3":stage==2?"ATEŞ HALKASI · AÇILAN ÇEKİRDEĞİ VUR":"GÖVDEYİ YOK ET";
                if(Mission.boss==BossIdentity.MotherboardHydra)return stage<3?"BAŞ DÜĞÜMLERİ · "+objectiveNodes.Count:stage==3?"ELEKTRİK SÜTUNLARI · "+objectiveNodes.Count:"ANA ÇEKİRDEĞİ YOK ET";
                if(Mission.boss==BossIdentity.GlacierWarden)return stage==1?"BUZ KALKANINI KIR · "+objectiveNodes.Count:stage==2?"İKİ MİNYON DALGASINI BİTİR":"BUZ ÇEKİRDEĞİNİ YOK ET";
                if(Mission.boss==BossIdentity.MagmaTitan)return stage==1?"TAŞ KOLU PARÇALA":stage==2?$"GÜVENLİ ALANDA KAL · {bossSafeTime:0.0}/2":stage==3?"ZAYIF NOKTALAR · "+objectiveNodes.Count:"TİTANI BİTİR";
                return stage==1?"PORTALLARI KAPAT · "+objectiveNodes.Count:stage==2?"KLONLARI AYRIŞTIR · "+objectiveNodes.Count:stage==3?"ENERJİ KÜPLERİ · "+objectiveNodes.Count:"PARLAK ÇEKİRDEKLİ GERÇEK BOSSU VUR";
            }
        }
        void ClearExpandedBoss()
        {expandedBoss=null;bossDecoys.Clear();bossWorldClock=bossSafeTime=bossLockedWindup=0;furnaceBarrels=bossWave=0;bossNextAttack=1.5f;}
        void SetupExpandedBoss()
        {
            ClearExpandedBoss();if(!HasExpandedBoss)return;
            expandedBoss=foes.Find(f=>f.kind==EnemyKind.Crusher);if(expandedBoss==null)return;
            expandedBoss.hp=expandedBoss.maxHp=stage==CubeCount?96:stage==1?72:84;
            if(stage==1) {bossTotal=72+(CubeCount-2)*84+96;bossHealth=bossTotal;}
            var color=CampaignExpansionSettings.Colors[Mission.biome];var root=expandedBoss.animation?expandedBoss.animation.VisualRoot:expandedBoss.body;
            // Distinct silhouettes made from the authored kit; original Crusher meshes remain intact.
            int ornaments=Mission.boss==BossIdentity.MotherboardHydra?3:Mission.boss==BossIdentity.VoidArchitect?4:2;
            for(int i=0;i<ornaments;i++)Shape("Reward beacon vertical",PrimitiveType.Cube,new Vector3((i-(ornaments-1)*.5f)*.45f,1.3f,.1f),new Vector3(.22f,.65f,.22f),color,root);
            if(Mission.boss==BossIdentity.FurnaceKing&&stage==1)
            {
                foreach(var barrel in barrels) {Destroy(barrel.body.gameObject);Destroy(barrel.warning.gameObject);}barrels.Clear();
                for(int i=0;i<3;i++)CreateBarrel(expandedBoss.body.position+new Vector3((i-1)*1.2f,-.45f,i==1?1:-.5f));
            }
            else if(NodePhase)
            {
                if(Mission.boss==BossIdentity.MagmaTitan&&stage==2)
                {var safe=WarningRing(centre+new Vector3(0,.1f,-1.8f),1.1f,VoidToyPalette.Cyan);hazardZones.Add(new HazardZone {marker=safe,at=centre+new Vector3(0,.1f,-1.8f),kind=ArenaHazard.None});}
                else if(Mission.boss==BossIdentity.GlacierWarden&&stage==2)SpawnBossWave();
                else
                {
                    int count=Mission.boss==BossIdentity.MagmaTitan?(stage==3?4:1):Mission.boss==BossIdentity.VoidArchitect&&stage==3?3:2;
                    for(int i=0;i<count;i++) {float a=(i+.5f)*Mathf.PI*2/count;SpawnObjectiveNode(i,centre+new Vector3(Mathf.Sin(a)*2.4f,.6f,1+Mathf.Cos(a)*1.7f),expandedBoss.maxHp/count,stage==1?"Faz kaynağı":"Enerji hedefi");}
                }
            }
            else if(Mission.boss==BossIdentity.VoidArchitect&&stage==4)
            {for(int i=0;i<2;i++) {var decoy=SpawnContentEnemy(ContentEnemy.Phaser,centre+new Vector3(i==0?-2.6f:2.6f,.6f,2.5f));decoy.hp=decoy.maxHp=8;bossDecoys.Add(decoy);}}
            if(Mission.boss==BossIdentity.FurnaceKing&&stage==2||Mission.boss==BossIdentity.GlacierWarden&&stage==2)
            {for(int i=0;i<3;i++) {var at=centre+new Vector3((i-1)*2,.1f,1);var kind=Mission.boss==BossIdentity.FurnaceKing?ArenaHazard.Lava:ArenaHazard.Ice;hazardZones.Add(new HazardZone {at=at,marker=WarningRing(at,.8f,kind==ArenaHazard.Ice?VoidToyPalette.Cyan:danger),kind=kind,clock=i*.5f});}}
            RecordValidation("expanded_boss_phase",Mission.boss+":"+stage);
        }
        bool ExpandedBossTargetable(Foe foe)
        {
            if(Expanded&&Mission.special==SpecialRule.ChainReactor&&objectiveNodes.TryGetValue(foe,out int order)&&order!=nextObjectiveNode)return false;
            if(!HasExpandedBoss||foe!=expandedBoss)return true;
            if(NodePhase)return false;
            if(Mission.boss==BossIdentity.FurnaceKing&&stage==2)return bossWorldClock%4>=1;
            return true;
        }
        float ResolveExpandedBossHit(Foe foe,float damage)=>ExpandedBossTargetable(foe)?damage:0;
        bool TickExpandedBoss(Foe foe,float dt,Vector3 toward,float distance)
        {
            if(objectiveNodes.ContainsKey(foe))return true;
            if(!HasExpandedBoss||foe!=expandedBoss)return false;
            if(bossLockedWindup>0)
            {
                bossLockedWindup-=dt;
                if(bossLockedWindup<=0)
                {
                    foe.warning.gameObject.SetActive(false);int count=Mission.boss==BossIdentity.MotherboardHydra?9:Mission.boss==BossIdentity.VoidArchitect?12:5;
                    for(int i=0;i<count;i++)FireEnemyShot(foe.body.position,Quaternion.Euler(0,Mission.boss==BossIdentity.VoidArchitect?i*360f/count:(i-(count-1)*.5f)*14,0)*bossAttackAim,7*foe.attackPower,5);
                    foe.animation?.Slam();feedback.Play(CombatFeedback.Cue.BossSlam);bossNextAttack=2.2f;
                }
                return true;
            }
            bossNextAttack-=dt;
            if(bossNextAttack<=0)
            {bossAttackAim=toward.normalized;bossLockedWindup=1.1f;foe.warning.gameObject.SetActive(true);foe.warning.position=new Vector3(foe.body.position.x,.1f,foe.body.position.z);foe.animation?.Telegraph(1.1f);feedback.Play(CombatFeedback.Cue.Telegraph);}
            return true;
        }
        void TickExpandedBossWorld(float dt)
        {
            if(!HasExpandedBoss||expandedBoss==null)return;bossWorldClock+=dt;
            if(Mission.boss==BossIdentity.GlacierWarden&&stage==2&&bossWave==1&&bossWorldClock>=3)SpawnBossWave();
            if(Mission.boss==BossIdentity.MagmaTitan&&stage==2)
            {
                bool safe=FlatDistance(player.position,centre+new Vector3(0,0,-1.8f))<1.1f;
                if(safe)bossSafeTime+=dt;else if(bossWorldClock>1)DamagePlayer(4,Vector3.zero);
            }
            TryCompleteExpandedBoss();
        }
        void SpawnBossWave()
        {bossWave++;for(int i=0;i<2;i++) {var foe=SpawnContentEnemy(ContentEnemy.Sprinter,SafeBranchSpawn(centre+new Vector3(i==0?-2.7f:2.7f,.6f,2.5f),ContentEnemy.Sprinter));foe.hp=foe.maxHp=21;}}
        void TryCompleteExpandedBoss()
        {
            if(!HasExpandedBoss||expandedBoss==null||!foes.Contains(expandedBoss)||!NodePhase)return;
            bool done=Mission.boss==BossIdentity.FurnaceKing?furnaceBarrels>=3:Mission.boss==BossIdentity.MagmaTitan&&stage==2?bossSafeTime>=2:Mission.boss==BossIdentity.GlacierWarden&&stage==2?bossWave>=2&&foes.Count==1:objectiveNodes.Count==0;
            if(done) {var boss=expandedBoss;bossHealth=Mathf.Max(0,bossHealth-boss.hp);EliminateEnemy(boss);}
        }
        void ExpandedBarrelExploded(Vector3 at)
        {if(HasExpandedBoss&&Mission.boss==BossIdentity.FurnaceKing&&stage==1&&expandedBoss!=null&&FlatDistance(at,expandedBoss.body.position)<3) {furnaceBarrels++;TryCompleteExpandedBoss();}}
    }
}
