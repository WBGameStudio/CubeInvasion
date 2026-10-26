using System.Linq;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;

// "Void Toy Arcade": bright plastic Scout, one graphite invader family with a single signature extension
// and an emissive intent light each (pink melee, orange ranged). Chunky chamfered blocks, low poly.
public static partial class VoidToyArtGeometry
{
    static readonly Color Cobalt=VoidToyPalette.Cobalt,Cyan=VoidToyPalette.Cyan,White=VoidToyPalette.Text,Ink=VoidToyPalette.Ink,Navy=VoidToyPalette.Side,Gold=VoidToyPalette.Gold;
    // Graphite family: the token is the body; a deeper tone for feet/joints and a lifted tone for plates.
    static readonly Color Body=Color.Lerp(VoidToyPalette.Graphite,VoidToyPalette.Text,.08f),Dark=Color.Lerp(VoidToyPalette.Graphite,VoidToyPalette.Ink,.45f),Plate=Color.Lerp(VoidToyPalette.Graphite,VoidToyPalette.Text,.22f);
    static Color Glow(Color c)=>VoidToyMeshBuilder.Glow(c);
    public static readonly string[] Actors={"Armored body","Grunt","Sprinter","Gunner","Pusher","Sniper","Bruiser","Crusher","Shielder","Healer","Bomber"};
    public static readonly string[] Weapons={"Pulse pistol","Twin blasters","Scatter shot","Rapid smg"};
    // Arena kit: the library keys InvasionPrototype already spawns (Arena, gates, bridges, pickups, cover).
    public static readonly string[] Props={"Deep sides","Arena surface","Edge light","Low cover","Explosive barrel","Explosive band","Health +20","Health cross H","Health cross V","Gold pickup","Elite crown",
        "Gate left post","Gate right post","Gate header","Locked until clear","Right bridge","Forward bridge","Reward beacon","Reward beacon vertical",
        "Gold crate","Jackpot chest","Spike tile","Spike tile spikes","Electric coil","Lava tile"};
    // These take their runtime colour (timer state, reward, risk) from the Shape() property block on masked vertices.
    public static bool Tinted(string key)=>HazardTinted(key)||key=="Edge light"||key.StartsWith("Reward beacon")||key.StartsWith("Gate ")||key=="Locked until clear"||key.EndsWith("bridge");
    public static bool Actor(string key)=>System.Array.IndexOf(Actors,key)>=0||System.Array.IndexOf(ExpansionActors,key)>=0;
    public static bool Weapon(string key)=>System.Array.IndexOf(Weapons,key)>=0||System.Array.IndexOf(ExpansionWeapons,key)>=0;
    // Gameplay transforms (InvasionPrototype.SpawnContentEnemy / scene player) the meshes are fitted to.
    public static Vector3 GameplayScale(string key)=>Props.Contains(key)?PropGameplayScale(key):key=="Armored body"?new Vector3(.65f,.55f,.65f):key=="Crusher"?Vector3.one*1.05f:key=="Bruiser"?Vector3.one*.85f:Weapon(key)?new Vector3(.2f,.2f,.65f):Vector3.one*.7f;
    static float RootHeight(string key)=>key=="Armored body"?.65f:key=="Crusher"?.95f:.6f;
    public static Color Family(string key)=>key.StartsWith("Expansion ")?ExpansionFamily(key):key=="Scatter shot"?VoidToyPalette.Warm:key=="Twin blasters"?Cobalt:key=="Rapid smg"?VoidToyPalette.Violet:Cyan;
    public static int Budget(string key)=>key=="Crusher"?6000:Weapon(key)?600:key=="Deep sides"||key=="Arena surface"||HazardTile(key)?1200:Actor(key)?3000:600;
    public static VoidToyMeshBuilder Make(string key)
    {
        switch(key)
        {
            case "Armored body":return Scout();
            case "Grunt":return Grunt();
            case "Sprinter":return Sprinter();
            case "Pusher":return Pusher();
            case "Bruiser":return Bruiser();
            case "Sniper":return Sniper();
            case "Gunner":return Gunner();
            case "Crusher":return Crusher();
            default:return Expansion(key)??(Weapon(key)?Blaster(key):Prop(key));
        }
    }
    // Ground sits at y=0. Mechanical enemy rigs are lifted 0.22 mesh units by ToyActorAnimator; the offset cancels it.
    static VoidToyMeshBuilder Fitted(string key)
    {
        var scale=GameplayScale(key);float lift=key=="Armored body"?0:.22f*scale.y;
        return new VoidToyMeshBuilder {Scale=scale,Offset=new Vector3(0,.04f-RootHeight(key)-lift,0)};
    }
    static VoidToyMeshBuilder Scout()
    {
        var b=Fitted("Armored body");
        b.PartAt("Core",new Vector3(0,.4f,0));
        b.Box(new Vector3(0,.44f,0),new Vector3(.56f,.44f,.46f),Cobalt,.12f);
        b.Box(new Vector3(0,.672f,0),new Vector3(.5f,.045f,.4f),White,.3f);
        b.Box(new Vector3(0,.6f,.236f),new Vector3(.48f,.045f,.02f),White,.3f);
        b.Box(new Vector3(0,.24f,0),new Vector3(.5f,.06f,.42f),Navy,.25f);
        // Chest core: flickers with the hit flash.
        b.Box(new Vector3(-.11f,.45f,.236f),new Vector3(.15f,.15f,.03f),Glow(Cyan),.2f);
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftLeg":"RightLeg",new Vector3(s*.14f,.24f,0));
            b.Box(new Vector3(s*.14f,.13f,0),new Vector3(.18f,.22f,.24f),Navy,.16f);
            b.Box(new Vector3(s*.14f,.03f,.04f),new Vector3(.2f,.06f,.3f),Ink,.3f);
        }
        b.PartAt("Head",new Vector3(0,.66f,0));
        b.Box(new Vector3(0,.7f,0),new Vector3(.16f,.08f,.16f),Navy,.2f);
        b.Sphere(new Vector3(0,.88f,0),Vector3.one*.46f,White,18,12);
        b.Box(new Vector3(0,.875f,.17f),new Vector3(.34f,.14f,.14f),Ink,.4f);
        // Antenna ball keeps the Scout findable from the top-down camera.
        b.Cylinder(new Vector3(0,1.15f,0),.014f,.12f,Navy,Quaternion.Euler(90,0,0),6);
        b.Sphere(new Vector3(0,1.235f,0),Vector3.one*.075f,Glow(Cyan),10,6);
        b.PartAt("VisorEye",new Vector3(0,.875f,.245f),"Head");
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.07f,.875f,.244f),new Vector3(.055f,.06f,.02f),Glow(Cyan),.3f);
        b.PartAt("LeftArm",new Vector3(-.33f,.6f,0));
        b.Box(new Vector3(-.34f,.585f,0),new Vector3(.11f,.11f,.15f),White,.2f);
        b.Box(new Vector3(-.34f,.45f,0),new Vector3(.1f,.2f,.12f),Cobalt,.2f);
        b.Box(new Vector3(-.34f,.31f,0),new Vector3(.12f,.1f,.13f),White,.25f);
        // Right arm carries the weapon; it recoils, the fire anchor stays on the gameplay root.
        b.PartAt("RightHand",new Vector3(.33f,.6f,0));
        b.Box(new Vector3(.34f,.585f,0),new Vector3(.11f,.11f,.15f),White,.2f);
        b.Box(new Vector3(.345f,.56f,.11f),new Vector3(.1f,.1f,.24f),Cobalt,.2f);
        b.Box(new Vector3(.35f,.545f,.23f),new Vector3(.12f,.12f,.1f),White,.25f);
        return b;
    }
    static void Legs(VoidToyMeshBuilder b,float x,float hip,Vector3 foot,float z=0)
    {
        for(int s=-1;s<=1;s+=2)
        {b.PartAt(s<0?"LeftLeg":"RightLeg",new Vector3(s*x,hip,z));b.Box(new Vector3(s*x,foot.y*.5f,z),foot,Dark,.15f);}
    }
    static void Chevron(VoidToyMeshBuilder b,Vector3 at,float width,Color light,Quaternion frame)
    {
        b.PartAt("VisorEye",at);
        for(int s=-1;s<=1;s+=2)b.Box(at+frame*new Vector3(s*width*.23f,0,0),new Vector3(width*.52f,.085f,.035f),Glow(light),.2f,frame*Quaternion.Euler(0,0,s*14));
    }
    static VoidToyMeshBuilder Grunt()
    {
        var b=Fitted("Grunt");var pink=VoidToyPalette.Danger;
        b.PartAt("Core",new Vector3(0,.4f,0));
        b.Box(new Vector3(0,.45f,0),new Vector3(.62f,.52f,.56f),Body,.1f);
        b.Box(new Vector3(0,.23f,0),new Vector3(.64f,.05f,.58f),Plate,.3f);
        Legs(b,.16f,.2f,new Vector3(.2f,.2f,.3f));
        b.PartAt("Head",new Vector3(0,.71f,0));
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.21f,.77f,-.06f),new Vector3(.12f,.12f,.12f),Dark,.15f);
        b.Box(new Vector3(0,.77f,0),new Vector3(.1f,.12f,.1f),Dark,.15f);
        b.Box(new Vector3(0,.88f,0),new Vector3(.13f,.11f,.13f),Glow(pink),.2f);
        // Wrap-around visor: chevron on the face, returns onto both sides.
        Chevron(b,new Vector3(0,.5f,.288f),.46f,pink,Quaternion.identity);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.31f,.5f,.2f),new Vector3(.035f,.08f,.16f),Glow(pink),.2f);
        return b;
    }
    static VoidToyMeshBuilder Sprinter()
    {
        var b=Fitted("Sprinter");var pink=VoidToyPalette.Danger;var lean=Quaternion.Euler(14,0,0);var centre=new Vector3(0,.47f,.02f);
        Vector3 On(float x,float y,float z)=>centre+lean*new Vector3(x,y,z);
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(centre,new Vector3(.36f,.46f,.32f),Body,.12f,lean);
        b.Box(new Vector3(0,.27f,0),new Vector3(.3f,.08f,.26f),Dark,.2f);
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftLeg":"RightLeg",new Vector3(s*.12f,.26f,0));
            b.Box(new Vector3(s*.12f,.15f,0),new Vector3(.07f,.2f,.07f),Dark,.2f);
            b.Box(new Vector3(s*.12f,.03f,.06f),new Vector3(.09f,.05f,.52f),Dark,.3f);
            b.Box(new Vector3(s*.12f,.06f,.3f),new Vector3(.09f,.035f,.06f),Plate,.3f);
        }
        Chevron(b,On(0,.06f,.17f),.3f,pink,lean);
        for(int s=-1;s<=1;s+=2)
        {b.PartAt(s<0?"LeftEar":"RightEar",On(s*.1f,.23f,-.04f));b.Box(On(s*.12f,.34f,-.06f),new Vector3(.05f,.24f,.14f),Glow(pink),.2f,lean*Quaternion.Euler(-12,0,-s*16));}
        // Signature back fin; swings with the stride as the Tail joint.
        b.PartAt("Tail",On(0,.05f,-.16f));
        b.Box(On(0,.08f,-.26f),new Vector3(.04f,.22f,.22f),Dark,.2f,lean*Quaternion.Euler(-30,0,0));
        b.Box(On(0,.2f,-.31f),new Vector3(.045f,.03f,.12f),Glow(pink),.3f,lean*Quaternion.Euler(-30,0,0));
        return b;
    }
    static VoidToyMeshBuilder Pusher()
    {
        var b=Fitted("Pusher");var pink=VoidToyPalette.Danger;
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(new Vector3(0,.46f,-.04f),new Vector3(.5f,.48f,.5f),Body,.1f);
        Legs(b,.15f,.22f,new Vector3(.16f,.22f,.26f),-.06f);
        for(int s=-1;s<=1;s+=2)
        {b.PartAt(s<0?"LeftEye":"RightEye",new Vector3(s*.255f,.58f,-.04f));b.Box(new Vector3(s*.255f,.58f,-.04f),new Vector3(.03f,.07f,.2f),Glow(pink),.2f);}
        // Big front shield slab; the shock cone reads from its light lines. Shoves on windup via the Hand joint.
        b.PartAt("ShieldHand",new Vector3(0,.46f,.21f));
        b.Box(new Vector3(0,.46f,.26f),new Vector3(.74f,.66f,.08f),Plate,.1f);
        b.Box(new Vector3(0,.72f,.305f),new Vector3(.56f,.04f,.02f),Glow(pink),.3f);
        b.Box(new Vector3(0,.2f,.305f),new Vector3(.56f,.04f,.02f),Glow(pink),.3f);
        b.Box(new Vector3(0,.46f,.305f),new Vector3(.34f,.05f,.02f),Glow(pink),.3f);
        b.Box(new Vector3(0,.46f,.305f),new Vector3(.05f,.26f,.02f),Glow(pink),.3f);
        return b;
    }
    static VoidToyMeshBuilder Bruiser()
    {
        var b=Fitted("Bruiser");var pink=VoidToyPalette.Danger;
        b.PartAt("Core",new Vector3(0,.45f,0));
        b.Box(new Vector3(0,.46f,0),new Vector3(.66f,.5f,.5f),Body,.1f);
        Legs(b,.17f,.22f,new Vector3(.2f,.22f,.26f));
        b.PartAt("Head",new Vector3(0,.7f,.06f));
        b.Box(new Vector3(0,.78f,.06f),new Vector3(.24f,.16f,.22f),Plate,.15f);
        b.PartAt("VisorEye",new Vector3(0,.78f,.176f),"Head");
        b.Box(new Vector3(0,.78f,.176f),new Vector3(.14f,.05f,.02f),Glow(pink),.3f);
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftArm":"RightArm",new Vector3(s*.4f,.66f,0));
            b.Box(new Vector3(s*.43f,.68f,0),new Vector3(.28f,.22f,.44f),Plate,.12f);
            b.Box(new Vector3(s*.43f,.75f,.225f),new Vector3(.26f,.035f,.02f),Glow(pink),.3f);
            b.Box(new Vector3(s*.42f,.47f,s>0?.06f:0),new Vector3(.14f,.22f,.16f),Dark,.2f);
        }
        // One oversized fist; the slam reads from it.
        b.PartAt("RightHand",new Vector3(.43f,.4f,.12f),"RightArm");
        b.Box(new Vector3(.45f,.3f,.2f),new Vector3(.3f,.3f,.3f),Plate,.12f);
        b.Box(new Vector3(.45f,.3f,.355f),new Vector3(.15f,.13f,.02f),Glow(pink),.2f);
        return b;
    }
    static void FourLegs(VoidToyMeshBuilder b,float x,float z,float hip,Vector3 size)
    {
        for(int i=0;i<4;i++)
        {
            int s=i%2==0?-1:1;bool back=i>=2;var at=new Vector3(s*x,hip,back?-z:z);
            b.PartAt((s<0?"Left":"Right")+(back?"BackLeg":"FrontLeg"),at);b.Box(new Vector3(at.x,size.y*.5f,at.z),size,Dark,.18f);
        }
    }
    static VoidToyMeshBuilder Sniper()
    {
        var b=Fitted("Sniper");var orange=VoidToyPalette.Warm;
        b.PartAt("Core",new Vector3(0,.6f,0));
        b.Box(new Vector3(0,.6f,0),new Vector3(.42f,.34f,.4f),Body,.1f);
        b.Box(new Vector3(0,.79f,-.04f),new Vector3(.2f,.05f,.14f),Dark,.2f);
        FourLegs(b,.17f,.14f,.44f,new Vector3(.06f,.46f,.06f));
        // Long barrel is the muzzle source; the lens glows through the 1.1 s laser telegraph.
        b.PartAt("RightHand",new Vector3(0,.62f,.2f));
        b.Box(new Vector3(0,.62f,.52f),new Vector3(.07f,.07f,.64f),Dark,.2f);
        b.Box(new Vector3(0,.62f,.86f),new Vector3(.1f,.1f,.08f),Glow(orange),.2f);
        b.PartAt("LensEye",new Vector3(0,.62f,.215f));
        b.Box(new Vector3(0,.62f,.205f),new Vector3(.2f,.2f,.03f),Dark,.2f);
        b.Box(new Vector3(0,.62f,.225f),new Vector3(.13f,.13f,.025f),Glow(orange),.3f);
        return b;
    }
    static VoidToyMeshBuilder Gunner()
    {
        var b=Fitted("Gunner");var orange=VoidToyPalette.Warm;
        b.PartAt("Core",new Vector3(0,.34f,0));
        b.Box(new Vector3(0,.34f,0),new Vector3(.64f,.28f,.56f),Body,.1f);
        b.Box(new Vector3(0,.27f,.285f),new Vector3(.46f,.035f,.02f),Glow(orange),.3f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.325f,.27f,0),new Vector3(.02f,.035f,.4f),Glow(orange),.3f);
        FourLegs(b,.25f,.19f,.22f,new Vector3(.15f,.2f,.15f));
        b.PartAt("Head",new Vector3(0,.48f,-.02f));
        b.Box(new Vector3(0,.59f,-.04f),new Vector3(.38f,.22f,.34f),Plate,.12f);
        b.PartAt("VisorEye",new Vector3(0,.645f,.136f),"Head");
        b.Box(new Vector3(0,.645f,.136f),new Vector3(.16f,.035f,.02f),Glow(orange),.3f);
        // Twin barrels light before the burst; their bounds give the muzzle tip.
        b.PartAt("RightHand",new Vector3(0,.58f,.13f),"Head");
        for(int s=-1;s<=1;s+=2)
        {b.Box(new Vector3(s*.085f,.58f,.33f),new Vector3(.07f,.07f,.4f),Dark,.2f);b.Box(new Vector3(s*.085f,.58f,.55f),new Vector3(.085f,.085f,.05f),Glow(orange),.2f);}
        return b;
    }
    static VoidToyMeshBuilder Crusher()
    {
        var b=Fitted("Crusher");var pink=VoidToyPalette.Danger;
        b.PartAt("Core",new Vector3(0,.68f,0));
        b.Box(new Vector3(0,.68f,0),new Vector3(1,.8f,.8f),Body,.07f);
        b.Box(new Vector3(0,.31f,0),new Vector3(.9f,.08f,.72f),Dark,.2f);
        Legs(b,.3f,.28f,new Vector3(.3f,.28f,.42f));
        // Phase 1 armour: two separate pink-trimmed panels that can drop away and reveal the grey hull.
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftArmor":"RightArmor",new Vector3(s*.245f,.84f,.43f));
            b.Box(new Vector3(s*.245f,.84f,.43f),new Vector3(.44f,.4f,.06f),Plate,.12f);
            b.Box(new Vector3(s*.245f,1.025f,.465f),new Vector3(.4f,.035f,.02f),Glow(pink),.3f);
            b.Box(new Vector3(s*.245f,.655f,.465f),new Vector3(.4f,.035f,.02f),Glow(pink),.3f);
        }
        b.PartAt("LeftHand",new Vector3(-.3f,.42f,.4f));
        b.Box(new Vector3(-.3f,.32f,.5f),new Vector3(.24f,.24f,.24f),Dark,.12f);
        b.PartAt("RightHand",new Vector3(.3f,.42f,.4f));
        b.Box(new Vector3(.3f,.33f,.54f),new Vector3(.32f,.32f,.32f),Plate,.1f);
        b.Box(new Vector3(.3f,.33f,.705f),new Vector3(.17f,.15f,.02f),Glow(pink),.3f);
        // Core sphere inside the shared boss / elite gold crown.
        b.PartAt("Head",new Vector3(0,1.08f,0));
        b.Box(new Vector3(0,1.16f,-.02f),new Vector3(.62f,.18f,.5f),Plate,.12f);
        b.Sphere(new Vector3(0,1.36f,0),Vector3.one*.32f,Glow(pink),16,10);
        for(int i=0;i<5;i++)
        {
            float a=(90+i*72)*Mathf.Deg2Rad;
            b.Box(new Vector3(Mathf.Cos(a)*.19f,1.37f,Mathf.Sin(a)*.19f),new Vector3(.08f,.2f,.08f),Gold,.2f,Quaternion.Euler(0,90-i*72,0));
        }
        b.PartAt("VisorEye",new Vector3(0,1.15f,.235f),"Head");
        b.Box(new Vector3(0,1.15f,.235f),new Vector3(.3f,.045f,.02f),Glow(pink),.3f);
        return b;
    }
    // Weapons live in the unit box the authored fire anchor scales (0.2 x 0.2 x 0.65); muzzle at +Z.
    static VoidToyMeshBuilder Blaster(string key)
    {
        var b=new VoidToyMeshBuilder();var family=Family(key);
        b.Box(new Vector3(0,.12f,-.08f),new Vector3(.78f,.5f,.72f),White,.12f);
        b.Box(new Vector3(0,.43f,-.16f),new Vector3(.5f,.14f,.4f),Cobalt,.2f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.395f,.12f,-.08f),new Vector3(.02f,.1f,.5f),Glow(family),.3f);
        b.Box(new Vector3(0,-.3f,-.3f),new Vector3(.34f,.48f,.16f),Navy,.2f,Quaternion.Euler(-8,0,0));
        if(key=="Pulse pistol")b.Box(new Vector3(0,.12f,.4f),new Vector3(.5f,.42f,.2f),Glow(family),.15f);
        else if(key=="Scatter shot")
        {b.Box(new Vector3(0,.12f,.38f),new Vector3(.9f,.72f,.24f),family,.12f);b.Box(new Vector3(0,.12f,.495f),new Vector3(.6f,.44f,.02f),Glow(family),.3f);}
        else if(key=="Twin blasters")
            for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(s*.2f,.12f,.34f),new Vector3(.2f,.2f,.28f),White,.2f);b.Box(new Vector3(s*.2f,.12f,.46f),new Vector3(.22f,.22f,.08f),Glow(family),.2f);}
        else
        {
            b.Box(new Vector3(0,.12f,.33f),new Vector3(.24f,.24f,.34f),White,.2f);
            for(int i=0;i<3;i++)b.Box(new Vector3(0,.12f,.2f+i*.1f),new Vector3(.34f,.34f,.04f),Glow(family),.25f);
            b.Box(new Vector3(0,.12f,.48f),new Vector3(.28f,.28f,.04f),Glow(family),.25f);
        }
        return b;
    }
    // Props are authored in metres around the unit primitive they replace, so bevels stay even under the Shape() scale.
    static Vector3 PropScale(string key)
    {
        switch(key)
        {
            case "Deep sides":return new Vector3(9,2.4f,9);
            case "Arena surface":return new Vector3(9,.16f,9);
            case "Low cover":return new Vector3(1.35f,.9f,.8f);
            case "Gate left post":case "Gate right post":return new Vector3(.18f,2,.2f);
            case "Gate header":return new Vector3(2.2f,.2f,.25f);
            case "Locked until clear":return new Vector3(1.8f,1.8f,.08f);
            case "Right bridge":return new Vector3(3.1f,.18f,1.8f);
            case "Forward bridge":return new Vector3(1.8f,.18f,3.1f);
            case "Reward beacon":return new Vector3(1.2f,.3f,.3f);
            case "Reward beacon vertical":return new Vector3(.3f,1.2f,.3f);
            default:return Vector3.one; // Uniform or multi-use shapes stay in the unit box.
        }
    }
    static Vector3 PropGameplayScale(string key)=>key=="Explosive barrel"?Vector3.one*.55f:key=="Health +20"?Vector3.one*.35f:key=="Gold pickup"?new Vector3(.28f,.07f,.28f):
        key=="Edge light"?new Vector3(9,.06f,.07f):key=="Explosive band"?new Vector3(1.08f,.2f,1.08f):key=="Health cross H"?new Vector3(1,.16f,.3f):key=="Health cross V"?new Vector3(.3f,.16f,1):
        key=="Elite crown"?new Vector3(.1f,.25f,.12f):PropScale(key);
    static VoidToyMeshBuilder Prop(string key)
    {
        var size=PropScale(key);var b=new VoidToyMeshBuilder {Scale=size};
        Color floor=VoidToyPalette.Floor,side=VoidToyPalette.Side,seam=Color.Lerp(floor,side,.32f),panel=Color.Lerp(side,floor,.09f),deep=Color.Lerp(side,Ink,.45f);
        var up=Quaternion.Euler(90,0,0);
        switch(key)
        {
            case "Deep sides":
                // Deep dark cube sides with inset panels; the light top belongs to the arena surface.
                b.Box(Vector3.zero,size,side,.02f);
                b.Box(new Vector3(0,-1.04f,0),new Vector3(9.04f,.3f,9.04f),deep,.2f);
                for(int f=0;f<4;f++)for(int p=0;p<4;p++)
                {
                    float along=-3.3f+p*2.2f,out_=4.52f*(f%2==0?-1:1);bool x=f<2;
                    b.Box(x?new Vector3(along,-.12f,out_):new Vector3(out_,-.12f,along),x?new Vector3(1.9f,1.5f,.05f):new Vector3(.05f,1.5f,1.9f),panel,.3f);
                }
                break;
            case "Arena surface":
                // 4 x 4 tile grid (2.25 m tiles) over a darker seam slab.
                b.Box(new Vector3(0,-.02f,0),new Vector3(9,.12f,9),seam,.1f);
                for(int i=0;i<16;i++)b.Box(new Vector3(-3.375f+i%4*2.25f,0,-3.375f+i/4*2.25f),new Vector3(2.205f,.16f,2.205f),floor,.12f);
                break;
            case "Edge light":b.Tint=1;b.Box(Vector3.zero,Vector3.one,Glow(White),.2f);break;
            case "Low cover":
                b.Box(new Vector3(0,-.06f,0),new Vector3(1.35f,.78f,.8f),side,.1f);
                b.Box(new Vector3(0,.38f,0),new Vector3(1.39f,.14f,.84f),Color.Lerp(floor,side,.15f),.2f);
                for(int s=-1;s<=1;s+=2)for(int z=-1;z<=1;z+=2)b.Box(new Vector3(s*.45f,-.05f,z*.405f),new Vector3(.06f,.4f,.02f),Glow(Cyan),.3f);
                break;
            case "Explosive barrel":
                b.Cylinder(Vector3.zero,.44f,.9f,Color.Lerp(VoidToyPalette.Warm,VoidToyPalette.Danger,.3f),up,12);
                for(int s=-1;s<=1;s+=2)b.Cylinder(new Vector3(0,s*.36f,0),.465f,.07f,deep,up,12);
                b.Cylinder(new Vector3(0,.46f,0),.3f,.04f,side,up,12);
                break;
            case "Explosive band":b.Cylinder(Vector3.zero,.5f,1,Glow(VoidToyPalette.Danger),up,12);break;
            case "Health +20":
                b.Box(Vector3.zero,new Vector3(.9f,.7f,.8f),White,.15f);
                b.Box(new Vector3(0,-.12f,0),new Vector3(.92f,.12f,.82f),Glow(VoidToyPalette.Health),.3f);
                b.Box(new Vector3(0,.37f,0),new Vector3(.5f,.06f,.4f),Navy,.3f);
                break;
            case "Health cross H":case "Health cross V":b.Box(Vector3.zero,Vector3.one,Glow(VoidToyPalette.HealthText),.2f);break;
            case "Gold pickup":
                b.Cylinder(Vector3.zero,.5f,.8f,Gold,up,14);
                b.Cylinder(Vector3.zero,.34f,.9f,Glow(Gold),up,14);
                break;
            case "Elite crown":b.Box(Vector3.zero,Vector3.one,Gold,.2f);break;
            case "Gate left post":case "Gate right post":
            {
                float inner=key=="Gate left post"?1:-1;
                b.Box(new Vector3(0,.06f,0),new Vector3(.18f,1.88f,.2f),White,.2f);
                b.Box(new Vector3(0,-.93f,0),new Vector3(.26f,.14f,.28f),side,.2f);
                b.Tint=1;b.Box(new Vector3(inner*.09f,.06f,0),new Vector3(.03f,1.6f,.08f),Glow(White),.3f);
                break;
            }
            case "Gate header":
                b.Box(Vector3.zero,size,White,.2f);
                b.Tint=1;b.Box(new Vector3(0,-.1f,0),new Vector3(1.9f,.03f,.1f),Glow(White),.3f);
                for(int s=-1;s<=1;s+=2)b.Box(new Vector3(0,0,s*.125f),new Vector3(1.9f,.05f,.02f),Glow(White),.3f);
                break;
            case "Locked until clear":
                for(int s=-1;s<=1;s+=2) {b.Box(new Vector3(s*.85f,0,0),new Vector3(.1f,1.8f,.08f),side,.2f);b.Box(new Vector3(0,s*.85f,0),new Vector3(1.6f,.1f,.08f),side,.2f);}
                b.Tint=1;b.Box(Vector3.zero,new Vector3(1.6f,1.6f,.04f),White,.1f);
                for(int i=-1;i<=1;i++)b.Box(new Vector3(0,i*.45f,0),new Vector3(1.4f,.07f,.07f),Glow(White),.3f);
                break;
            case "Right bridge":case "Forward bridge":
            {
                bool x=key=="Right bridge";float length=x?size.x:size.z,width=x?size.z:size.x;
                Vector3 Along(float l,float y,float w)=>x?new Vector3(l,y,w):new Vector3(w,y,l);
                b.Box(Along(0,.03f,0),Along(length,.12f,width),floor,.15f);
                b.Box(Along(0,-.05f,0),Along(length-.1f,.08f,width-.2f),side,.2f);
                b.Tint=1;for(int s=-1;s<=1;s+=2)b.Box(Along(0,.1f,s*(width*.5f-.04f)),Along(length,.04f,.06f),Glow(White),.3f);
                break;
            }
            default:
            {
                // Reward beacons: two halves of a tinted plus sign with white end caps.
                var axis=size.x>size.y?Vector3.right:Vector3.up;float length=Vector3.Dot(size,axis);
                for(int s=-1;s<=1;s+=2)b.Box(axis*s*(length*.5f-.06f),size-axis*(length-.12f),White,.25f);
                b.Tint=1;b.Box(Vector3.zero,size-axis*.24f+Vector3.Scale(Vector3.one-axis,Vector3.one*.02f),Glow(White),.25f);
            }
                break;
        }
        return b;
    }
}
