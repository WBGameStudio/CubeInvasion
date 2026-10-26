using System.Linq;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;

// Campaign expansion round: dedicated support enemies, hazard tiles, Gold Rush crates and the
// Void Toy restyle of the expansion weapons / Summoner / Phaser / Elite (keys owned by ExpansionArt).
public static partial class VoidToyArtGeometry
{
    public static readonly string[] ExpansionActors={"Summoner","Phaser","Elite"};
    public static readonly string[] ExpansionWeapons=Enumerable.Range(4,8).Select(i=>"Expansion "+(ContentWeapon)i).ToArray();
    // Keys that live in Art/Expansion/ExpansionArt.asset; it is merged after the base library, so it wins on duplicates.
    public static bool ExpansionOwned(string key)=>ExpansionWeapons.Contains(key)||ExpansionActors.Contains(key)||key=="Frozen patch";
    public static bool HazardTile(string key)=>key=="Spike tile"||key=="Spike tile spikes"||key=="Electric coil"||key=="Lava tile"||key=="Frozen patch";
    // Hazard tiles show idle / warning / active through the runtime tint on their light strips.
    static bool HazardTinted(string key)=>key=="Spike tile"||key=="Electric coil"||key=="Lava tile";
    static Color ExpansionFamily(string key)=>CampaignExpansionSettings.Colors[(System.Array.IndexOf(ExpansionWeapons,key))%6];
    static VoidToyMeshBuilder Expansion(string key)
    {
        switch(key)
        {
            case "Shielder":return Shielder();
            case "Healer":return Healer();
            case "Bomber":return Bomber();
            case "Summoner":return Summoner();
            case "Phaser":return Phaser();
            case "Elite":return Elite();
            case "Gold crate":return GoldCrate();
            case "Jackpot chest":return JackpotChest();
            case "Spike tile":case "Spike tile spikes":case "Electric coil":case "Lava tile":case "Frozen patch":return Hazard(key);
            default:return ExpansionWeapons.Contains(key)?ExpansionBlaster(key):null;
        }
    }
    static void Ring(VoidToyMeshBuilder b,Vector3 centre,float radius,int count,Vector3 size,Color color)
    {
        for(int i=0;i<count;i++) {float a=i*360f/count;b.Box(centre+Quaternion.Euler(0,a,0)*Vector3.forward*radius,size,color,.25f,Quaternion.Euler(0,a,0));}
    }
    static VoidToyMeshBuilder Shielder()
    {
        var b=Fitted("Shielder");var pink=VoidToyPalette.Danger;var arc=VoidToyPalette.CobaltText;
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(new Vector3(0,.45f,-.08f),new Vector3(.5f,.5f,.44f),Body,.1f);
        b.Box(new Vector3(0,.72f,-.12f),new Vector3(.3f,.06f,.26f),Dark,.2f);
        Legs(b,.15f,.22f,new Vector3(.16f,.22f,.26f),-.08f);
        // Tall frontal slab; the blue arc matches the runtime guard arc that blocks frontal shots.
        b.PartAt("ShieldHand",new Vector3(0,.46f,.16f));
        b.Box(new Vector3(0,.5f,.24f),new Vector3(.86f,.8f,.1f),Plate,.1f);
        b.Box(new Vector3(0,.12f,.25f),new Vector3(.9f,.08f,.12f),Dark,.2f);
        for(int i=0;i<7;i++)
        {float a=-75+i*25;var at=new Vector3(Mathf.Sin(a*Mathf.Deg2Rad)*.3f,.38f+Mathf.Cos(a*Mathf.Deg2Rad)*.3f,.3f);b.Box(at,new Vector3(.11f,.04f,.02f),Glow(arc),.3f,Quaternion.Euler(0,0,-a));}
        b.Box(new Vector3(0,.38f,.3f),new Vector3(.14f,.14f,.02f),Glow(arc),.3f,Quaternion.Euler(0,0,45));
        b.PartAt("VisorEye",new Vector3(0,.86f,.3f),"ShieldHand");
        b.Box(new Vector3(0,.86f,.3f),new Vector3(.26f,.035f,.02f),Glow(pink),.3f);
        return b;
    }
    static VoidToyMeshBuilder Healer()
    {
        var b=Fitted("Healer");var violet=VoidToyPalette.Violet;
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(new Vector3(0,.45f,0),new Vector3(.5f,.46f,.44f),Body,.18f);
        b.Box(new Vector3(0,.24f,0),new Vector3(.52f,.05f,.46f),Plate,.3f);
        // Violet back light: the repair source reads from behind as well as from the beam.
        b.Box(new Vector3(0,.48f,-.225f),new Vector3(.3f,.24f,.03f),Glow(violet),.2f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.255f,.48f,-.05f),new Vector3(.03f,.16f,.2f),Plate,.3f);
        Legs(b,.14f,.2f,new Vector3(.18f,.2f,.26f));
        b.PartAt("VisorEye",new Vector3(0,.52f,.226f));
        b.Box(new Vector3(0,.52f,.226f),new Vector3(.3f,.06f,.03f),Glow(violet),.3f);
        // Antenna with a violet halo ring and tip.
        b.PartAt("Head",new Vector3(0,.68f,0));
        b.Box(new Vector3(0,.71f,0),new Vector3(.14f,.06f,.14f),Dark,.2f);
        b.Cylinder(new Vector3(0,.85f,0),.02f,.26f,Dark,Quaternion.Euler(90,0,0),6);
        Ring(b,new Vector3(0,.93f,0),.13f,8,new Vector3(.07f,.03f,.03f),Glow(violet));
        b.Sphere(new Vector3(0,1.01f,0),Vector3.one*.08f,Glow(violet),10,6);
        for(int s=-1;s<=1;s+=2)
        {b.PartAt(s<0?"LeftArm":"RightArm",new Vector3(s*.3f,.58f,0));b.Box(new Vector3(s*.3f,.46f,0),new Vector3(.09f,.22f,.11f),Dark,.2f);b.Box(new Vector3(s*.3f,.33f,.02f),new Vector3(.1f,.08f,.1f),Glow(violet),.25f);}
        return b;
    }
    static VoidToyMeshBuilder Bomber()
    {
        var b=Fitted("Bomber");var orange=VoidToyPalette.Warm;var up=Quaternion.Euler(90,0,0);
        // The cast's only sphere body; the waist band and fuse ball pre-announce the blast area.
        b.PartAt("Core",new Vector3(0,.42f,0));
        b.Sphere(new Vector3(0,.42f,0),Vector3.one*.56f,Body,16,10);
        b.Cylinder(new Vector3(0,.42f,0),.292f,.08f,Glow(orange),up,16);
        b.Cylinder(new Vector3(0,.42f,0),.3f,.02f,Dark,up,16);
        Legs(b,.13f,.16f,new Vector3(.14f,.15f,.22f));
        b.PartAt("VisorEye",new Vector3(0,.54f,.25f));
        b.Box(new Vector3(0,.54f,.25f),new Vector3(.2f,.05f,.04f),Glow(orange),.3f,Quaternion.Euler(-20,0,0));
        b.PartAt("Head",new Vector3(0,.68f,0));
        b.Box(new Vector3(0,.7f,0),new Vector3(.12f,.05f,.12f),Dark,.25f);
        b.Cylinder(new Vector3(0,.77f,0),.022f,.12f,Dark,up,6);
        b.Sphere(new Vector3(0,.87f,0),Vector3.one*.12f,Glow(orange),10,6);
        return b;
    }
    static VoidToyMeshBuilder Summoner()
    {
        var b=Fitted("Summoner");var cyan=VoidToyPalette.Cyan;
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(new Vector3(0,.45f,0),new Vector3(.56f,.5f,.5f),Body,.1f);
        b.Box(new Vector3(0,.23f,0),new Vector3(.58f,.05f,.52f),Plate,.3f);
        Legs(b,.16f,.2f,new Vector3(.18f,.2f,.28f));
        Chevron(b,new Vector3(0,.52f,.262f),.4f,cyan,Quaternion.identity);
        // Signature: a tilted summoning dish with three emitter pillars on the back.
        b.PartAt("Head",new Vector3(0,.7f,-.1f));
        b.Box(new Vector3(0,.78f,-.16f),new Vector3(.46f,.06f,.32f),Plate,.2f,Quaternion.Euler(-25,0,0));
        for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.14f,.9f,-.2f),new Vector3(.06f,.2f,.06f),Glow(cyan),.25f,Quaternion.Euler(-25,0,0));
        for(int s=-1;s<=1;s+=2)
        {b.PartAt(s<0?"LeftArm":"RightArm",new Vector3(s*.33f,.6f,0));b.Box(new Vector3(s*.33f,.48f,0),new Vector3(.1f,.24f,.12f),Dark,.2f);b.Box(new Vector3(s*.33f,.33f,.02f),new Vector3(.11f,.09f,.11f),Glow(cyan),.25f);}
        return b;
    }
    static VoidToyMeshBuilder Phaser()
    {
        var b=Fitted("Phaser");var violet=new Color(.7f,.3f,1);var orange=VoidToyPalette.Warm;
        b.PartAt("Core",new Vector3(0,.5f,0));
        b.Box(new Vector3(0,.5f,0),new Vector3(.4f,.5f,.36f),Body,.14f);
        // Phase rings: the teleport identity; the orange slit and barrel carry the ranged intent.
        Ring(b,new Vector3(0,.33f,0),.31f,8,new Vector3(.12f,.03f,.03f),Glow(violet));
        Ring(b,new Vector3(0,.66f,0),.31f,8,new Vector3(.12f,.03f,.03f),Glow(violet));
        Legs(b,.12f,.25f,new Vector3(.1f,.25f,.2f));
        b.PartAt("VisorEye",new Vector3(0,.6f,.185f));
        b.Box(new Vector3(0,.6f,.185f),new Vector3(.24f,.045f,.02f),Glow(orange),.3f);
        b.PartAt("RightHand",new Vector3(.2f,.48f,.12f));
        b.Box(new Vector3(.2f,.46f,.32f),new Vector3(.07f,.07f,.36f),Dark,.2f);
        b.Box(new Vector3(.2f,.46f,.52f),new Vector3(.09f,.09f,.05f),Glow(orange),.2f);
        return b;
    }
    static VoidToyMeshBuilder Elite()
    {
        var b=Fitted("Elite");var pink=VoidToyPalette.Danger;
        b.PartAt("Core",new Vector3(0,.47f,0));
        b.Box(new Vector3(0,.47f,0),new Vector3(.6f,.54f,.52f),Body,.1f);
        b.Box(new Vector3(0,.47f,.27f),new Vector3(.42f,.3f,.03f),Plate,.2f);
        b.Box(new Vector3(0,.42f,.29f),new Vector3(.3f,.035f,.02f),Glow(pink),.3f);
        Legs(b,.17f,.22f,new Vector3(.2f,.22f,.28f));
        b.PartAt("VisorEye",new Vector3(0,.6f,.29f));
        b.Box(new Vector3(0,.6f,.29f),new Vector3(.3f,.05f,.02f),Glow(pink),.3f);
        // Gold crown: the same family as the boss crown and the elite reward.
        b.PartAt("Head",new Vector3(0,.74f,0));
        b.Box(new Vector3(0,.77f,0),new Vector3(.36f,.06f,.3f),Plate,.2f);
        for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.13f,.86f+(i==0?.04f:0),0),new Vector3(.07f,.16f+(i==0?.08f:0),.07f),Gold,.2f);
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftArm":"RightArm",new Vector3(s*.38f,.64f,0));
            b.Box(new Vector3(s*.4f,.66f,0),new Vector3(.22f,.18f,.38f),Plate,.12f);
            b.Box(new Vector3(s*.4f,.72f,.195f),new Vector3(.2f,.03f,.02f),Glow(pink),.3f);
            b.Box(new Vector3(s*.39f,.44f,.04f),new Vector3(.13f,.24f,.15f),Dark,.2f);
        }
        return b;
    }
    // Gold Rush containers live in the unit box Shape() scales by 0.7.
    static VoidToyMeshBuilder GoldCrate()
    {
        var b=new VoidToyMeshBuilder();var gold=Gold;var deep=VoidToyPalette.GoldShadow;
        b.Box(Vector3.zero,Vector3.one*.9f,deep,.1f);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)b.Box(new Vector3(x*.42f,0,z*.42f),new Vector3(.12f,.96f,.12f),gold,.25f);
        for(int y=-1;y<=1;y+=2)b.Box(new Vector3(0,y*.42f,0),new Vector3(.96f,.1f,.96f),gold,.25f);
        foreach(var face in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
        {var q=Quaternion.LookRotation(face);b.Cylinder(face*.455f,.16f,.03f,Glow(gold),q,10);}
        return b;
    }
    static VoidToyMeshBuilder JackpotChest()
    {
        var b=new VoidToyMeshBuilder();var gold=Gold;
        b.Box(new Vector3(0,-.18f,0),new Vector3(.96f,.62f,.72f),Body,.1f);
        b.Box(new Vector3(0,.25f,0),new Vector3(1,.3f,.76f),gold,.18f);
        b.Box(new Vector3(0,.42f,0),new Vector3(.9f,.06f,.66f),VoidToyPalette.GoldShadow,.3f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.36f,-.06f,0),new Vector3(.09f,.86f,.78f),gold,.25f);
        // Three-key lock plate; the runtime lock marker above it counts collected keys.
        b.Box(new Vector3(0,.04f,.38f),new Vector3(.32f,.34f,.05f),Ink,.2f);
        for(int i=-1;i<=1;i++)b.Box(new Vector3(i*.08f,.04f,.41f),new Vector3(.04f,.14f,.02f),Glow(gold),.3f);
        return b;
    }
    // Tiles are authored in metres (1.9 m for a 1 m hazard radius) with the base at y=0.
    static VoidToyMeshBuilder Hazard(string key)
    {
        var b=new VoidToyMeshBuilder();var frame=Color.Lerp(VoidToyPalette.Side,Ink,.25f);var up=Quaternion.Euler(90,0,0);
        if(key=="Frozen patch")
        {
            // Unit box (biome decor scales it to 1.6 x 0.025 x 1.6); ice stays light so graphite enemies keep contrast.
            b.Box(Vector3.zero,Vector3.one,Color.Lerp(VoidToyPalette.CyanText,White,.45f),.2f);
            foreach(var (x,z,a) in new[]{(-.15f,.1f,30f),(.2f,-.15f,-40f),(.05f,.3f,75f)})b.Box(new Vector3(x,.02f,z),new Vector3(.55f,1,.025f),White,.3f,Quaternion.Euler(0,a,0));
            return b;
        }
        if(key=="Spike tile spikes")
        {
            for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)
            {var at=new Vector3(x*.55f,0,z*.55f);b.Box(at+Vector3.up*.13f,new Vector3(.12f,.26f,.12f),Plate,.25f,Quaternion.Euler(0,45,0));b.Box(at+Vector3.up*.3f,new Vector3(.06f,.1f,.06f),Glow(VoidToyPalette.Danger),.3f,Quaternion.Euler(0,45,0));}
            return b;
        }
        b.Box(new Vector3(0,.02f,0),new Vector3(1.9f,.04f,1.9f),frame,.2f);
        if(key=="Spike tile")
        {
            for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)b.Box(new Vector3(x*.55f,.042f,z*.55f),new Vector3(.24f,.01f,.24f),Ink,.3f);
            b.Tint=1;for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(0,.045f,s*.88f),new Vector3(1.76f,.012f,.06f),Glow(White),.3f);b.Box(new Vector3(s*.88f,.045f,0),new Vector3(.06f,.012f,1.64f),Glow(White),.3f);}
        }
        else if(key=="Electric coil")
        {
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {
                var at=new Vector3(x*.72f,0,z*.72f);b.Tint=0;b.Cylinder(at+Vector3.up*.18f,.08f,.3f,VoidToyPalette.Side,up,8);
                b.Tint=1;for(int r=0;r<3;r++)b.Cylinder(at+Vector3.up*(.1f+r*.08f),.11f,.03f,Glow(White),up,8);
            }
            b.Tint=1;b.Cylinder(new Vector3(0,.05f,0),.36f,.02f,Glow(White),up,12);
            for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(0,.045f,s*.72f),new Vector3(1.3f,.012f,.04f),Glow(White),.3f);b.Box(new Vector3(s*.72f,.045f,0),new Vector3(.04f,.012f,1.3f),Glow(White),.3f);}
        }
        else
        {
            // Lava: glowing pool inside a dark rim, with a few cooled crust plates.
            for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(0,.05f,s*.88f),new Vector3(1.9f,.06f,.14f),frame,.2f);b.Box(new Vector3(s*.88f,.05f,0),new Vector3(.14f,.06f,1.62f),frame,.2f);}
            foreach(var (x,z) in new[]{(-.35f,.25f),(.4f,-.3f),(.1f,.5f)})b.Box(new Vector3(x,.06f,z),new Vector3(.32f,.03f,.26f),Color.Lerp(Ink,VoidToyPalette.Side,.4f),.3f,Quaternion.Euler(0,x*90,0));
            b.Tint=1;b.Box(new Vector3(0,.045f,0),new Vector3(1.64f,.02f,1.64f),Glow(White),.2f);
        }
        return b;
    }
    // White body, cobalt top plate, family-coloured stripes and muzzle; same unit box as the base four.
    static VoidToyMeshBuilder ExpansionBlaster(string key)
    {
        var b=new VoidToyMeshBuilder();var family=Family(key);var up=Quaternion.Euler(90,0,0);
        b.Box(new Vector3(0,.12f,-.08f),new Vector3(.78f,.5f,.72f),White,.12f);
        b.Box(new Vector3(0,.43f,-.16f),new Vector3(.5f,.14f,.4f),Cobalt,.2f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.395f,.12f,-.08f),new Vector3(.02f,.1f,.5f),Glow(family),.3f);
        b.Box(new Vector3(0,-.3f,-.3f),new Vector3(.34f,.48f,.16f),Navy,.2f,Quaternion.Euler(-8,0,0));
        switch(key.Substring("Expansion ".Length))
        {
            case "PierceRifle":
                b.Box(new Vector3(0,.12f,.36f),new Vector3(.14f,.14f,.28f),White,.2f);b.Box(new Vector3(0,.12f,.47f),new Vector3(.18f,.18f,.06f),Glow(family),.2f);
                b.Box(new Vector3(0,.58f,-.06f),new Vector3(.16f,.14f,.36f),Navy,.2f);b.Box(new Vector3(0,.58f,.125f),new Vector3(.1f,.1f,.02f),Glow(family),.3f);break;
            case "RicochetGun":
                b.Box(new Vector3(0,.12f,.3f),new Vector3(.5f,.4f,.06f),White,.2f);b.Box(new Vector3(0,.12f,.4f),new Vector3(.3f,.3f,.18f),Glow(family),.2f,Quaternion.Euler(0,0,45));break;
            case "ArcLauncher":
                b.Box(new Vector3(0,.12f,.32f),new Vector3(.22f,.22f,.3f),White,.2f);
                for(int i=0;i<3;i++)b.Box(new Vector3(0,.12f,.22f+i*.09f),new Vector3(.46f,.46f,.035f),Glow(family),.3f);
                b.Sphere(new Vector3(0,.12f,.45f),new Vector3(.22f,.22f,.12f),Glow(family),10,6);break;
            case "RocketPod":
                b.Box(new Vector3(0,.12f,.37f),new Vector3(.6f,.5f,.24f),White,.12f);
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)b.Box(new Vector3(x*.14f,.12f+y*.11f,.495f),new Vector3(.14f,.14f,.02f),Glow(family),.3f);break;
            case "FlameSprayer":
                b.Cylinder(new Vector3(0,-.16f,.02f),.14f,.5f,family,null,10);
                b.Box(new Vector3(0,.12f,.34f),new Vector3(.3f,.3f,.14f),White,.2f);b.Box(new Vector3(0,.12f,.45f),new Vector3(.44f,.4f,.08f),Glow(family),.2f);break;
            case "CryoBeam":
                b.Box(new Vector3(0,.12f,.4f),new Vector3(.22f,.22f,.22f),Glow(family),.15f,Quaternion.Euler(45,45,0));
                for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.3f,.12f,.3f),new Vector3(.04f,.36f,.2f),White,.3f);break;
            case "OrbitDrones":
                for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(s*.5f,.2f,.08f),new Vector3(.16f,.16f,.22f),White,.25f);b.Box(new Vector3(s*.5f,.2f,.2f),new Vector3(.11f,.11f,.02f),Glow(family),.3f);}
                b.Box(new Vector3(0,.12f,.4f),new Vector3(.24f,.24f,.16f),Glow(family),.2f);break;
            default: // NovaCannon
                b.Cylinder(new Vector3(0,.12f,.32f),.26f,.36f,White,null,10);b.Cylinder(new Vector3(0,.12f,.47f),.28f,.05f,Glow(family),null,10);
                b.Cylinder(new Vector3(0,.12f,.5f),.14f,.02f,Glow(White),null,10);break;
        }
        return b;
    }
}
