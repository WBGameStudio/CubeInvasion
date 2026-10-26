using System.Collections.Generic;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        [Header("GDD v1.1 / branching reward encounters")]
        [SerializeField] BranchRouteSettings branchSettings;
        RouteOffer rightOffer,forwardOffer,activeOffer,committedOffer;
        bool offersRevealed;
        BranchCard rightPreviewCard,forwardPreviewCard;
        string committedDirection,committedRight,committedForward;
        float selectedHpBefore;
        float routeDecisionSeconds,routeShield,routeRateBonus,routeMagnetBonus;
        int routeRateCubes,routeMagnetCubes;
        Transform shieldVisual;
        Material routeIconMaterial;
        bool HasBranches=>HasContent&&branchSettings&&branchSettings.rewards!=null&&branchSettings.rewards.Length>=8;
        float RouteRate=>routeRateCubes>0?routeRateBonus:0;
        float RouteMagnet=>routeMagnetCubes>0?routeMagnetBonus:0;
        float RouteBlast=>HasBranches&&activeOffer!=null&&activeOffer.stage==stage&&activeOffer.reward.reward==RouteReward.Blast?.2f:0;
        int RouteClearBonus=>HasBranches&&activeOffer!=null&&activeOffer.stage==stage&&(activeOffer.reward.reward==RouteReward.Gold||activeOffer.reward.reward==RouteReward.Elite)?Mathf.Max(0,Mathf.RoundToInt(activeOffer.reward.value)):0;
        CubeRouteDefinition OfferedRoute(bool right)=>HasBranches?(right?rightOffer:forwardOffer)?.reward:(right?rightRoute:forwardRoute);
        float RouteBudgetDps=>Weapon.NominalDps*(Weapon.kind==ContentWeapon.ScatterShot?.6f:1)*(1+MetaDamage)*(1+MetaRate);
        void ResetRouteRun()
        {
            rightOffer=forwardOffer=activeOffer=committedOffer=null;offersRevealed=false;routeDecisionSeconds=0;
            routeShield=routeRateBonus=routeMagnetBonus=0;routeRateCubes=routeMagnetCubes=0;
            if(shieldVisual)shieldVisual.gameObject.SetActive(false);
        }
        void PrepareBranchOffers(int target)
        {
            if(!HasBranches)return;
            var pair=RouteDirector.Pair(branchSettings,Mission,missionIndex,target,health/MaxHealth,RouteBudgetDps);
            rightOffer=pair[0];forwardOffer=pair[1];offersRevealed=false;
        }
        void RevealBranchOffers()
        {
            if(!HasBranches||offersRevealed||stage>=CubeCount)return;
            // Candidate geometry rises during combat; the final offers lock once the objective clears.
            var pair=RouteDirector.Pair(branchSettings,Mission,missionIndex,stage+1,health/MaxHealth,RouteBudgetDps);
            rightOffer=pair[0];forwardOffer=pair[1];offersRevealed=true;routeDecisionSeconds=0;
            UpdateBranchView(rightGate,rightOffer);UpdateBranchView(forwardGate,forwardOffer);
            rightPreviewCard=BranchPreviewCard(rightOffer,true);forwardPreviewCard=BranchPreviewCard(forwardOffer,false);
            RecordBranch("branch_offered",null);
        }
        void ApplyRouteReward(CubeRouteDefinition route)
        {
            if(!route)return;
            switch(route.reward)
            {
                case RouteReward.Health:health=Mathf.Min(MaxHealth,health+Mathf.Max(0,route.value));feedback.Play(CombatFeedback.Cue.Heal);break;
                case RouteReward.Damage:
                    damageBonus=Mathf.Clamp(Mathf.Max(damageCubes>0?damageBonus:0,route.value/100f),0,.35f);damageCubes=Mathf.Max(1,route.duration);break;
                case RouteReward.FireRate:
                    routeRateBonus=Mathf.Clamp(Mathf.Max(RouteRate,route.value/100f),0,.25f);routeRateCubes=Mathf.Max(1,route.duration);break;
                case RouteReward.Magnet:
                    routeMagnetBonus=Mathf.Clamp(Mathf.Max(RouteMagnet,route.value/100f),0,.35f);routeMagnetCubes=Mathf.Max(1,route.duration);break;
                case RouteReward.Shield:routeShield=Mathf.Clamp(route.value,0,40);break;
            }
        }
        void ExpireRouteBuffs()
        {
            routeShield=0;routeRateCubes=Mathf.Max(0,routeRateCubes-1);routeMagnetCubes=Mathf.Max(0,routeMagnetCubes-1);
            if(routeRateCubes==0)routeRateBonus=0;if(routeMagnetCubes==0)routeMagnetBonus=0;
        }
        void ClearAllRouteBuffs()
        {ResetRouteRun();damageCubes=0;damageBonus=0;}
        void ConfigureRouteEnemy(Foe foe,int index)
        {
            if(!HasBranches||activeOffer==null||activeOffer.stage!=stage||index!=activeOffer.eliteIndex)return;
            foe.hp*=branchSettings.eliteHealth;foe.speed*=1.1f;foe.attackPower=1.15f;foe.body.localScale*=1.08f;
            var parent=foe.animation?foe.animation.VisualRoot:foe.body;
            float y=foe.body.GetComponent<MeshFilter>().sharedMesh.bounds.max.y+.18f;
            for(int i=-1;i<=1;i++)Shape("Elite crown",PrimitiveType.Cube,new Vector3(i*.16f,y+(i==0?.09f:0),0),new Vector3(.1f,.25f,.12f),new Color(1,.65f,.1f),parent);
        }
        Vector3 SafeBranchSpawn(Vector3 desired,ContentEnemy kind)
        {
            if(!HasBranches)return desired;
            if(SpawnFree(desired,kind))return desired;
            Vector3 best=desired;float score=float.PositiveInfinity;
            for(int z=0;z<4;z++)for(int x=0;x<4;x++)
            {
                var point=centre+new Vector3(-2.7f+x*1.8f,desired.y,-2.7f+z*1.8f);point.y=desired.y;
                if(!SpawnFree(point,kind))continue;float cost=(point-desired).sqrMagnitude;
                if(cost<score) {score=cost;best=point;}
            }
            return best;
        }
        bool SpawnFree(Vector3 point,ContentEnemy kind)
        {
            if(FlatDistance(point,player.position)<1.8f||CoverHit(point,point,.4f,out _))return false;
            foreach(var foe in foes)if(FlatDistance(point,foe.body.position)<(kind==ContentEnemy.Crusher||foe.kind==EnemyKind.Crusher?1.6f:.85f))return false;
            return true;
        }
        void RecordBranch(string name,RouteOffer selected,string detail=null,int coins=0)
        {
            if(!HasMeta||validationLog==null)return;
            validationLog.Record(new P4ValidationLog.Event {name=name,run=validationRun,mission=missionIndex,cube=stage,weapon=weaponIndex,
                hp=health,timeRemaining=remaining,damageTaken=cubeDamageTaken,seconds=runSeconds,enemiesLeft=foes.Count,coins=coins,
                detail=detail,seed=selected?.seed??rightOffer?.seed??branchSettings.seed,offeredRight=selected==null?rightOffer?.Summary:committedRight,offeredForward=selected==null?forwardOffer?.Summary:committedForward,
                hpBefore=selected==null?health:selectedHpBefore,
                selectedDirection=selected==null?null:committedDirection,
                reward=selected?.reward.reward.ToString(),risk=selected?.reward.risk??0,responseMs=Mathf.RoundToInt(routeDecisionSeconds*1000),buffAfter=RouteBuffText()});
        }
        string cachedBuffText;int cachedDamage=-1,cachedRate=-1,cachedMagnet=-1,cachedBonus=-1;
        string RouteBuffText()
        {
            int bonus=phase==Phase.Combat?RouteClearBonus:0;
            if(cachedBuffText!=null&&cachedDamage==damageCubes&&cachedRate==routeRateCubes&&cachedMagnet==routeMagnetCubes&&cachedBonus==bonus)return cachedBuffText;
            cachedDamage=damageCubes;cachedRate=routeRateCubes;cachedMagnet=routeMagnetCubes;cachedBonus=bonus;
            var text=new List<string>(4);
            if(damageCubes>0)text.Add($"HASAR +%{damageBonus*100:0} ({damageCubes})");
            if(routeRateCubes>0)text.Add($"ATIŞ +%{RouteRate*100:0} ({routeRateCubes})");
            if(routeMagnetCubes>0)text.Add($"MIKNATIS +%{RouteMagnet*100:0} ({routeMagnetCubes})");
            if(phase==Phase.Combat&&RouteClearBonus>0)text.Add($"TEMİZLE +{RouteClearBonus} ALTIN");
            return cachedBuffText=string.Join(" · ",text);
        }
        void RefreshRouteHud()
        {
            if(!HasBranches)return;
            hud.SetRouteStatus(RouteBuffText(),routeShield);
            hud.SetBranchPreview(rightPreviewCard,forwardPreviewCard,phase==Phase.Choice&&offersRevealed&&!paused);
            if(routeShield>0&&!shieldVisual)
            {
                shieldVisual=new GameObject("Temporary route shield").transform;shieldVisual.SetParent(player,false);shieldVisual.localPosition=Vector3.down*.52f;
                var line=shieldVisual.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=RouteIconMaterial();line.useWorldSpace=false;line.loop=true;line.widthMultiplier=.035f;line.positionCount=12;
                for(int i=0;i<12;i++) {float a=i*Mathf.PI*2/12;line.SetPosition(i,new Vector3(Mathf.Cos(a)*.65f,0,Mathf.Sin(a)*.65f));}
                line.startColor=line.endColor=new Color(.1f,.85f,1);line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            if(shieldVisual)shieldVisual.gameObject.SetActive(routeShield>0&&(phase==Phase.Combat||phase==Phase.Transfer));
        }
    }
}
