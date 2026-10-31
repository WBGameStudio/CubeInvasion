using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        static string ThreatLabel(RouteOffer offer) => offer.eliteIndex>=0?"elit":System.Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Healer)?"tamirci + destek":System.Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Shielder)?"kalkanlı":System.Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Bomber)?"bombacı":System.Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Crusher)?"boss":
            System.Array.Exists(offer.encounter.enemies,e=>e==ContentEnemy.Sniper||e==ContentEnemy.Gunner)?"menzilli":"yakın dövüş";
        BranchCard BranchPreviewCard(RouteOffer offer,bool right) => new BranchCard {
            direction=right?"SAĞ KAPI":"İLERİ KAPI",title=RewardLabel(offer.reward).Replace("\n"," · "),
            detail=HasGoldRushVariants?GoldRushVariantSettings.Name(offer.encounter.goldVariant)+$" · {offer.encounter.enemies.Length} düşman":$"{offer.encounter.enemies.Length} hedef · {ThreatLabel(offer)}",
            accent=VoidToyPalette.Reward(offer.reward.reward),risk=Mathf.Clamp(offer.reward.risk,1,3)};
        Material RouteIconMaterial()
        {
            if(!routeIconMaterial) {routeIconMaterial=new Material(Shader.Find("Sprites/Default"));materials.Add(routeIconMaterial);}
            return routeIconMaterial;
        }
        void UpdateBranchView(Transform gate,RouteOffer offer)
        {
            if(!gate||offer==null)return;
            var label=gate.GetComponentInChildren<TextMesh>();ApplyGateFont(label);
            // The HUD offer cards carry reward and risk; the world sign stays hidden so the two never overlap.
            label.GetComponent<Renderer>().enabled=false;
            label.text=(gate.name.StartsWith("Right")?"SAĞ · ":"İLERİ · ")+RewardLabel(offer.reward)+$"\nRİSK {offer.reward.risk}/3 · {offer.encounter.enemies.Length} HEDEF";
            label.fontSize=64;label.characterSize=.04f;
            // Gate color names the reward family (heal, power, gold, shield); risk stays on the pips and label.
            Color tint=VoidToyPalette.Reward(offer.reward.reward);
            foreach(var renderer in gate.GetComponentsInChildren<MeshRenderer>())
                if(renderer.gameObject.name.StartsWith("Gate "))renderer.sharedMaterial=Mat(tint);
            // A different silhouette accompanies every written reward, independent of color.
            var old=gate.Find("Reward symbol");if(old)Destroy(old.gameObject);
            var icon=new GameObject("Reward symbol").transform;icon.SetParent(gate,false);icon.position=gate.position+Vector3.up*3.35f;icon.rotation=view.transform.rotation;
            Vector2[] points;
            switch(offer.reward.reward)
            {
                case RouteReward.Health:points=new[]{new Vector2(-.3f,.1f),new Vector2(-.15f,.3f),new Vector2(0,.15f),new Vector2(.15f,.3f),new Vector2(.3f,.1f),new Vector2(0,-.3f),new Vector2(-.3f,.1f)};break;
                case RouteReward.Damage:points=new[]{new Vector2(-.25f,-.3f),new Vector2(.25f,.3f),new Vector2(.27f,.1f),new Vector2(.1f,.25f),new Vector2(-.3f,-.25f)};break;
                case RouteReward.FireRate:points=new[]{new Vector2(.15f,.35f),new Vector2(-.2f,0),new Vector2(.1f,0),new Vector2(-.15f,-.35f)};break;
                case RouteReward.Shield:points=new[]{new Vector2(-.28f,.25f),new Vector2(.28f,.25f),new Vector2(.2f,-.15f),new Vector2(0,-.35f),new Vector2(-.2f,-.15f),new Vector2(-.28f,.25f)};break;
                case RouteReward.Gold:points=new[]{new Vector2(0,.3f),new Vector2(.3f,0),new Vector2(0,-.3f),new Vector2(-.3f,0),new Vector2(0,.3f)};break;
                case RouteReward.Blast:points=new[]{new Vector2(0,.35f),new Vector2(.1f,.1f),new Vector2(.35f,0),new Vector2(.1f,-.1f),new Vector2(0,-.35f),new Vector2(-.1f,-.1f),new Vector2(-.35f,0),new Vector2(-.1f,.1f),new Vector2(0,.35f)};break;
                case RouteReward.Magnet:points=new[]{new Vector2(-.25f,.3f),new Vector2(-.25f,-.15f),new Vector2(0,-.3f),new Vector2(.25f,-.15f),new Vector2(.25f,.3f)};break;
                default:points=new[]{new Vector2(-.3f,-.25f),new Vector2(-.3f,.25f),new Vector2(-.1f,0),new Vector2(0,.35f),new Vector2(.1f,0),new Vector2(.3f,.25f),new Vector2(.3f,-.25f),new Vector2(-.3f,-.25f)};break;
            }
            var line=icon.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=RouteIconMaterial();line.widthMultiplier=.065f;line.positionCount=points.Length;line.startColor=line.endColor=tint;
            for(int i=0;i<points.Length;i++)line.SetPosition(i,new Vector3(points[i].x,points[i].y,0));
        }
    }
}
