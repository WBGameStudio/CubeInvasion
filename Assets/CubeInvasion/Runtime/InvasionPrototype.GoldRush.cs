using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [SerializeField] GoldRushVariantSettings goldRushVariants;
        sealed class GoldCrate {public Transform body,lockMarker;public float hp;public bool locked,jackpot;}
        readonly List<GoldCrate> goldCrates=new List<GoldCrate>();
        readonly List<Foe> goldCarriers=new List<Foe>();
        GoldCrate goldTarget;
        int goldCratesBroken,goldCarriersKilled,goldKeys;
        bool jackpotOpened;
        const string GoldMissionDescription="3 KÜP · HER KÜP 20 sn\nAltın topla, 6 kasayı kır; son kapıda altın sürüsü veya jackpot seç.";
        bool HasGoldRushVariants=>goldRushVariants&&goldRushVariants.enabled&&IsGoldRush;
        GoldRushVariant GoldVariant=>HasGoldRushVariants?(activeOffer!=null&&activeOffer.stage==stage?activeOffer.encounter.goldVariant:GoldRushVariantSettings.ForStage(stage,0)):GoldRushVariant.CoinSprint;
        bool HasAimTarget=>currentTarget!=null||goldTarget!=null||barrelTarget!=null;
        Vector3 AimPoint=>barrelTarget!=null?barrelTarget.body.position:goldTarget!=null?goldTarget.body.position:EnemyTargetPoint(currentTarget);
        void ResetGoldObjectives()
        {
            foreach(var crate in goldCrates)if(crate.body)Destroy(crate.body.gameObject);
            goldCrates.Clear();goldCarriers.Clear();goldTarget=null;goldCratesBroken=goldCarriersKilled=goldKeys=0;jackpotOpened=false;
        }
        void SetupGoldObjectives()
        {
            ResetGoldObjectives();if(!HasGoldRushVariants)return;
            if(GoldVariant==GoldRushVariant.CrateBreaker)
            {
                for(int i=0;i<6;i++)CreateGoldCrate(centre+new Vector3((i%3-1)*2.7f,.45f,i<3?-1:2.8f),false);
            }
            else if(GoldVariant==GoldRushVariant.Jackpot)CreateGoldCrate(centre+new Vector3(0,.45f,2.8f),true);
            RecordValidation("goldrush_variant_started",GoldVariant.ToString());
        }
        void CreateGoldCrate(Vector3 position,bool jackpot)
        {
            // Void Toy crate / jackpot chest when installed (falls back to the cover block), with coin badges.
            string model=jackpot?"Jackpot chest":"Gold crate";if(!artLibrary||!artLibrary.TryGet(model,out _))model="Low cover";
            var body=Shape(model,PrimitiveType.Cube,position,new Vector3(.7f,.7f,.7f),VoidToyPalette.Gold,platform);
            body.position=position;body.name=jackpot?"Locked jackpot chest":"Counted gold crate";
            for(int i=-1;i<=1;i++)Shape("Gold pickup",PrimitiveType.Cylinder,new Vector3(i*.22f,.78f,0),new Vector3(.26f,.1f,.26f),VoidToyPalette.Gold,body);
            var marker=new GameObject("Three key lock").transform;marker.SetParent(body,false);
            for(int i=0;i<3;i++)Shape("Gold pickup",PrimitiveType.Cylinder,new Vector3((i-1)*.28f,1.2f,0),new Vector3(.18f,.1f,.18f),VoidToyPalette.Gold,marker);
            marker.gameObject.SetActive(jackpot);
            goldCrates.Add(new GoldCrate {body=body,hp=jackpot?goldRushVariants.jackpotHealth:goldRushVariants.crateHealth,locked=jackpot,jackpot=jackpot,lockMarker=marker});
        }
        void ConfigureGoldCarrier(Foe foe)
        {
            if(!HasGoldRushVariants||(GoldVariant!=GoldRushVariant.GoldenSwarm&&GoldVariant!=GoldRushVariant.Jackpot))return;
            goldCarriers.Add(foe);if(GoldVariant==GoldRushVariant.GoldenSwarm){foe.hp=goldRushVariants.carrierHealth;foe.maxHp=foe.hp;}
            var badge=Shape("Gold pickup",PrimitiveType.Cylinder,new Vector3(0,1.35f,0),new Vector3(.23f,.09f,.23f),VoidToyPalette.Gold,foe.body);
            badge.name=GoldVariant==GoldRushVariant.Jackpot?"Jackpot key carrier":"Golden carrier badge";
        }
        int EnemyGold(Foe foe)=>HasGoldRushVariants&&GoldVariant==GoldRushVariant.GoldenSwarm&&goldCarriers.Contains(foe)?goldRushVariants.carrierGold:3;
        void GoldEnemyEliminated(Foe foe)
        {
            if(!HasGoldRushVariants||!goldCarriers.Remove(foe))return;
            if(GoldVariant==GoldRushVariant.GoldenSwarm) {goldCarriersKilled++;RecordValidation("gold_carrier_eliminated");}
            else if(GoldVariant==GoldRushVariant.Jackpot)
            {
                goldKeys=Mathf.Min(3,goldKeys+1);RecordValidation("jackpot_key_collected",goldKeys.ToString());
                if(goldCrates.Count>0)
                {
                    var chest=goldCrates[0];if(chest.lockMarker&&chest.lockMarker.childCount>=goldKeys)chest.lockMarker.GetChild(goldKeys-1).gameObject.SetActive(false);
                    if(goldKeys==3) {chest.locked=false;feedback.Play(CombatFeedback.Cue.Gate);feedback.Burst(chest.body.position,VoidToyPalette.Gold,1,.3f,true);}
                }
            }
        }
        void AimAtGoldCrate(ref float nearest)
        {
            goldTarget=null;if(!HasGoldRushVariants)return;
            foreach(var crate in goldCrates)
            {
                if(!crate.body||crate.hp<=0||crate.locked||CoverHit(player.position,crate.body.position,.09f,out _))continue;
                float distance=Vector3.ProjectOnPlane(crate.body.position-player.position,Vector3.up).sqrMagnitude;
                if((int)Weapon.kind>=4&&distance>Mathf.Pow(Weapon.EffectiveRange*(1+MetaRange),2))continue;
                if(distance<nearest) {nearest=distance;goldTarget=crate;currentTarget=null;}
            }
        }
        void HitGoldCrate(GoldCrate crate,float damage)
        {
            if(phase!=Phase.Combat||!HasGoldRushVariants||!goldCrates.Contains(crate)||!crate.body||crate.locked||damage<=0)return;
            crate.hp=Mathf.Max(0,crate.hp-damage);feedback.Play(CombatFeedback.Cue.Hit);if(crate.hp>0)return;
            var at=crate.body.position;goldCrates.Remove(crate);if(goldTarget==crate)goldTarget=null;
            Destroy(crate.body.gameObject);SpawnPickup(at,false,crate.jackpot?goldRushVariants.jackpotGold:goldRushVariants.crateGold);
            feedback.Burst(at,VoidToyPalette.Gold,1,.3f,true);feedback.Play(CombatFeedback.Cue.Gold);
            if(crate.jackpot) {jackpotOpened=true;RecordValidation("jackpot_opened");}
            else {goldCratesBroken++;RecordValidation("gold_crate_broken",goldCratesBroken.ToString());}
        }
        bool GoldObjectiveComplete=>GoldVariant==GoldRushVariant.CrateBreaker?goldCratesBroken>=6:GoldVariant==GoldRushVariant.GoldenSwarm?goldCarriersKilled>=5:GoldVariant==GoldRushVariant.Jackpot?goldKeys==3&&jackpotOpened:quotaGold>=Encounter.goldQuota;
        string GoldObjectiveText=>GoldVariant==GoldRushVariant.CrateBreaker?$"KASA KIR · {goldCratesBroken}/6":GoldVariant==GoldRushVariant.GoldenSwarm?$"ALTIN SÜRÜSÜ {goldCarriersKilled}/5":GoldVariant==GoldRushVariant.Jackpot?(goldKeys<3?$"ANAHTAR {goldKeys}/3":"SANDIĞI KIR"):$"ALTIN {quotaGold}/{Encounter.goldQuota}";
        bool GoldSpawnBlocked(Vector3 at)
        {foreach(var crate in goldCrates)if(crate.body&&FlatDistance(at,crate.body.position)<.95f)return true;return false;}
        void FinishGoldObjectives(bool clear)
        {
            goldTarget=null;if(!clear)return;
            foreach(var crate in goldCrates)if(crate.body)Destroy(crate.body.gameObject);goldCrates.Clear();goldCarriers.Clear();
        }
    }
}
