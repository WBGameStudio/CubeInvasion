using UnityEngine;
using CubeInvasion.ArtTools;

// Prismatic ceramic shells, articulated graphite chassis, visible energy technology.
public static class PrismArtGeometry
{
    static readonly Color Suit=CuteMeshBuilder.Suit,Dark=new Color(.035f,.055f,.09f),White=new Color(.89f,.95f,1),Steel=new Color(.22f,.3f,.4f),Cyan=new Color(.06f,.88f,1),Gold=new Color(1,.62f,.08f),Rose=new Color(1,.12f,.32f);
    public static bool Actor(string key)=>CuteArtGeometry.Actor(key);
    public static bool Weapon(string key)=>CuteArtGeometry.Weapon(key);
    public static Color Tint(string key)
    {
        switch(key)
        {
            case "Armored body":return new Color(.08f,.35f,.88f);
            case "Grunt":return new Color(.86f,.15f,.22f);
            case "Sprinter":return new Color(1,.61f,.06f);
            case "Gunner":return new Color(.46f,.25f,.8f);
            case "Pusher":return new Color(.94f,.34f,.07f);
            case "Sniper":return new Color(.08f,.64f,.5f);
            case "Bruiser":return new Color(.78f,.14f,.4f);
            case "Crusher":return new Color(.21f,.19f,.52f);
            default:return CuteArtGeometry.Tint(key);
        }
    }
    public static CuteMeshBuilder Make(string key,float detail=1)
    {
        var b=new CuteMeshBuilder {DetailScale=detail};if(Actor(key))ActorMesh(b,key);else if(Weapon(key))Blaster(b,key);else return Props(key);return b;
    }
    static void Box(CuteMeshBuilder b,Vector3 p,Vector3 s,Color c,float r=.22f,Quaternion? q=null)=>b.RoundBox(p,s,c,r,10,q);
    static void Orb(CuteMeshBuilder b,Vector3 p,Vector3 s,Color c)=>b.Sphere(p,s,c,32,20);
    static void Bolt(CuteMeshBuilder b,Vector3 p,float size=.045f)=>b.Barrel(p,size,.026f,Steel,null,16);
    static void Lens(CuteMeshBuilder b,Vector3 p,float r,Color color)
    {b.Barrel(p,r,.08f,Steel,null,32);b.Torus(p+Vector3.forward*.05f,r*.77f,r*.15f,White,null,40,12);Orb(b,p+Vector3.forward*.067f,new Vector3(r*1.4f,r*1.4f,.055f),color);}
    static void ActorMesh(CuteMeshBuilder b,string key)
    {
        bool hero=key=="Armored body",boss=key=="Crusher",heavy=key=="Bruiser",runner=key=="Sprinter",gunner=key=="Gunner",shield=key=="Pusher",sniper=key=="Sniper";
        float w=boss?1.75f:heavy?1.35f:runner?.78f:1;
        float headY=hero?.57f:boss?.73f:gunner?.26f:.48f;
        b.PartAt("Core",Vector3.zero);
        Box(b,new Vector3(0,-.15f,0),new Vector3(w*.67f,.64f,.56f),Dark,.3f);
        Box(b,new Vector3(0,.02f,-.07f),new Vector3(w*.92f,.65f,.65f),Suit,.25f);
        // Layered shoulder yoke and an inset diamond-shaped reactor.
        Box(b,new Vector3(0,-.03f,.305f),new Vector3(w*.66f,.49f,.12f),White,.17f);
        Box(b,new Vector3(0,-.02f,.386f),new Vector3(.25f,.25f,.07f),Dark,.15f,Quaternion.Euler(0,0,45));
        Box(b,new Vector3(0,-.02f,.429f),new Vector3(.15f,.15f,.04f),hero?Cyan:Gold,.14f,Quaternion.Euler(0,0,45));
        for(int s=-1;s<=1;s+=2)
        {
            Box(b,new Vector3(s*w*.25f,-.3f,.32f),new Vector3(w*.16f,.06f,.075f),Steel);
            Bolt(b,new Vector3(s*w*.25f,.18f,.37f));
            Box(b,new Vector3(s*w*.25f,-.02f,-.43f),new Vector3(w*.27f,.55f,.24f),White,.3f);
            b.Barrel(new Vector3(s*w*.25f,-.05f,-.57f),.105f,.22f,Dark);
            b.Torus(new Vector3(s*w*.25f,-.05f,-.7f),.08f,.025f,Cyan);
        }
        b.Torus(new Vector3(0,.29f,0),w*.25f,.06f,Dark,Quaternion.Euler(90,0,0),40,12);
        b.PartAt("Head",new Vector3(0,headY,0));
        var headSize=new Vector3(w*(boss?.79f:1.04f),boss?.79f:hero?.87f:.75f,runner?.73f:.86f);
        Box(b,new Vector3(0,headY,0),headSize,Suit,.38f);
        Box(b,new Vector3(0,headY+.025f,.421f),new Vector3(headSize.x*.91f,headSize.y*.64f,.16f),White,.33f);
        Box(b,new Vector3(0,headY+.03f,.516f),new Vector3(headSize.x*.8f,headSize.y*.48f,.09f),Dark,.32f);
        // Segmented ceramic brow, vents and amber fasteners; not a recolored shared face.
        Box(b,new Vector3(0,headY+headSize.y*.37f,.35f),new Vector3(headSize.x*.7f,.1f,.23f),White,.2f);
        for(int s=-1;s<=1;s+=2)
        {
            b.Barrel(new Vector3(s*headSize.x*.49f,headY,0),headSize.y*.24f,.12f,Steel,Quaternion.Euler(0,90,0),32);
            b.Barrel(new Vector3(s*headSize.x*.55f,headY,0),headSize.y*.17f,.05f,White,Quaternion.Euler(0,90,0),32);
            for(int n=0;n<3;n++)Box(b,new Vector3(s*headSize.x*.35f,headY-.23f,.41f-n*.006f),new Vector3(.035f,.025f,.07f),Dark);
            var eye=new Vector3(s*headSize.x*.19f,headY+.04f,.575f);b.PartAt(s<0?"LeftEye":"RightEye",eye,"Head");
            Box(b,eye,new Vector3(sniper?.17f:runner?.2f:.18f,hero?.14f:heavy?.08f:.115f,.045f),hero?Cyan:sniper?Rose:Gold,.3f,Quaternion.Euler(0,0,s*(hero?0:runner?17:12)));
            Box(b,eye+new Vector3(-.03f,.03f,.027f),new Vector3(.035f,.025f,.015f),Color.white,.2f);
            b.PartAt("Head",new Vector3(0,headY,0));
        }
        if(hero)
        {
            Box(b,new Vector3(-.28f,headY+.48f,-.14f),new Vector3(.075f,.32f,.08f),Dark,.3f);
            Orb(b,new Vector3(-.28f,headY+.65f,-.14f),Vector3.one*.11f,Cyan);
            Box(b,new Vector3(.24f,headY+.36f,-.03f),new Vector3(.14f,.1f,.45f),White);
        }
        if(runner)
        {
            for(int s=-1;s<=1;s+=2)
            {b.PartAt(s<0?"LeftEar":"RightEar",new Vector3(s*.28f,headY+.25f,-.14f),"Head");Box(b,new Vector3(s*.3f,headY+.47f,-.22f),new Vector3(.12f,.54f,.27f),Suit,.22f,Quaternion.Euler(-22,0,-s*16));Box(b,new Vector3(s*.3f,headY+.54f,-.06f),new Vector3(.045f,.28f,.04f),Cyan);}
        }
        if(boss)
        {
            b.PartAt("Head",new Vector3(0,headY,0));
            for(int i=-1;i<=1;i++)Box(b,new Vector3(i*.31f,headY+.48f+(i==0?.15f:0),-.02f),new Vector3(.21f,.43f,.26f),Gold,.25f,Quaternion.Euler(0,0,-i*17));
        }
        int legs=gunner?4:2;
        for(int i=0;i<legs;i++)
        {
            int s=i%2==0?-1:1;bool back=i>=2;float z=gunner?(back?-.3f:.3f):0;
            string prefix=s<0?"Left":"Right",name=gunner?prefix+(back?"BackLeg":"FrontLeg"):prefix+"Leg";
            var hip=new Vector3(s*w*(gunner?.43f:.24f),-.37f,z);b.PartAt(name,hip);
            Orb(b,hip,new Vector3(.22f,.23f,.23f),Steel);
            Box(b,hip+new Vector3(0,-.15f,0),new Vector3(w*.24f,.34f,.31f),Suit,.3f);
            var knee=hip+new Vector3(0,-.3f,0);string shin=gunner?prefix+(back?"BackShin":"FrontShin"):prefix+"Shin";b.PartAt(shin,knee,name);
            Orb(b,knee,new Vector3(.2f,.2f,.21f),Dark);
            Box(b,knee+new Vector3(0,-.12f,.02f),new Vector3(w*.25f,.29f,.32f),White,.25f);
            Box(b,knee+new Vector3(0,-.27f,.13f),new Vector3(w*(runner?.4f:.34f),.22f,runner?.72f:.53f),Suit,.3f);
            Box(b,knee+new Vector3(0,-.3f,.16f),new Vector3(w*(runner?.42f:.36f),.085f,runner?.74f:.56f),Dark,.2f);
            Box(b,knee+new Vector3(0,-.21f,.35f),new Vector3(w*.19f,.07f,.045f),hero?Cyan:Gold);
        }
        for(int s=-1;s<=1;s+=2)
        {
            var shoulder=new Vector3(s*w*.55f,.08f,0);string name=s<0?"LeftArm":"RightArm";b.PartAt(name,shoulder);
            Orb(b,shoulder,Vector3.one*(heavy||boss?.4f:.3f),Dark);
            Box(b,shoulder+new Vector3(s*.035f,.07f,-.03f),new Vector3(w*.37f,.36f,.43f),White,.3f);
            Box(b,shoulder+new Vector3(0,-.18f,0),new Vector3(w*.23f,.37f,.29f),Suit,.28f);
            var hand=shoulder+new Vector3(0,-.38f,.07f);b.PartAt(s<0?"LeftHand":"RightHand",hand,name);
            Box(b,hand,new Vector3(w*(heavy||boss?.5f:.27f),heavy||boss?.45f:.3f,heavy||boss?.5f:.3f),heavy||boss?White:Steel,.28f);
            if(heavy||boss)
            {
                for(int n=-1;n<=1;n++)Box(b,hand+new Vector3(n*w*.11f,0,.28f),new Vector3(w*.095f,.24f,.08f),Suit);
                Lens(b,hand+Vector3.forward*.31f,boss?.17f:.12f,hero?Cyan:Rose);
            }
            if(shield)
            {
                Box(b,hand+new Vector3(0,.16f,.28f),new Vector3(.57f,.95f,.22f),Steel,.2f);
                Box(b,hand+new Vector3(0,.16f,.42f),new Vector3(.47f,.81f,.12f),Suit,.27f);
                Box(b,hand+new Vector3(0,.16f,.5f),new Vector3(.07f,.58f,.04f),Cyan);
                for(int n=-1;n<=1;n+=2)Bolt(b,hand+new Vector3(n*.15f,.45f,.5f),.05f);
            }
            if((sniper||gunner)&&s==1)
            {
                b.Barrel(hand+new Vector3(0,.04f,.43f),sniper?.1f:.17f,sniper?1.1f:.74f,Steel,null,40);
                b.Torus(hand+new Vector3(0,.04f,sniper?.98f:.8f),sniper?.085f:.15f,.03f,sniper?Rose:Gold);
                if(sniper)Lens(b,hand+new Vector3(0,.28f,.28f),.095f,Rose);
            }
        }
        if(gunner)
        {
            b.PartAt("Head",new Vector3(0,headY,0));Box(b,new Vector3(0,headY+.52f,-.14f),new Vector3(.66f,.35f,.5f),Steel,.3f);
            for(int s=-1;s<=1;s+=2)b.Barrel(new Vector3(s*.18f,headY+.52f,.4f),.12f,.7f,White,null,40);
        }
        if(sniper)
        {
            b.PartAt("Tail",new Vector3(0,-.2f,-.34f));Box(b,new Vector3(0,-.2f,-.64f),new Vector3(.26f,.23f,.75f),Suit,.3f,Quaternion.Euler(-12,0,0));
            Box(b,new Vector3(0,-.08f,-.82f),new Vector3(.08f,.12f,.25f),Cyan);
        }
        if(boss)
        {
            b.PartAt("Core",Vector3.zero);
            for(int s=-1;s<=1;s+=2)
            {Box(b,new Vector3(s*.9f,.28f,-.3f),new Vector3(.45f,.6f,.8f),Steel,.2f);for(int n=0;n<3;n++)Box(b,new Vector3(s*.9f,.35f,-.53f+n*.2f),new Vector3(.36f,.25f,.08f),Gold,.15f);}
        }
    }
    static void Blaster(CuteMeshBuilder b,string key)
    {
        bool twin=key=="Twin blasters",scatter=key=="Scatter shot",rapid=key=="Rapid smg";
        Box(b,new Vector3(0,.02f,-.08f),new Vector3(.82f,.5f,.79f),Suit,.23f);
        Box(b,new Vector3(0,-.43f,-.29f),new Vector3(.33f,.57f,.33f),Dark,.22f,Quaternion.Euler(-14,0,0));
        Box(b,new Vector3(0,.28f,-.11f),new Vector3(.6f,.12f,.64f),White,.2f);
        Box(b,new Vector3(0,.34f,-.15f),new Vector3(.23f,.07f,.39f),Cyan,.2f);
        for(int s=-1;s<=1;s+=2)
        {Box(b,new Vector3(s*.43f,0,-.08f),new Vector3(.07f,.3f,.55f),White);for(int n=0;n<3;n++)Bolt(b,new Vector3(s*.28f,-.06f,.32f+n*.12f),.04f);}
        int count=twin?2:1;
        for(int i=0;i<count;i++)
        {
            float x=twin?(i==0?-.25f:.25f):0,r=scatter?.35f:twin?.16f:.23f;
            b.Barrel(new Vector3(x,.015f,.43f),r,rapid?.95f:.8f,Steel,null,48);
            for(int n=0;n<3;n++)b.Torus(new Vector3(x,.015f,.23f+n*.19f),r*.95f,.035f,White,null,40,10);
            b.Torus(new Vector3(x,.015f,rapid?.93f:.85f),r*.84f,.03f,scatter?Gold:Cyan,null,40,10);
            b.Barrel(new Vector3(x,.015f,rapid?.94f:.86f),r*.65f,.035f,Dark,null,32);
            Lens(b,new Vector3(x,.015f,rapid?.97f:.89f),r*.37f,Cyan);
        }
        if(rapid)b.Barrel(new Vector3(0,-.2f,.03f),.27f,.44f,Suit,Quaternion.Euler(0,90,0),40);
        if(scatter)Box(b,new Vector3(0,-.16f,.43f),new Vector3(.6f,.19f,.41f),Gold);
    }
    static CuteMeshBuilder Props(string key)
    {
        var b=new CuteMeshBuilder();
        if(key=="Low cover")
        {Box(b,Vector3.zero,Vector3.one,Suit,.12f);Box(b,Vector3.up*.47f,new Vector3(1.04f,.15f,1.04f),White,.2f);for(int s=-1;s<=1;s+=2)Box(b,new Vector3(s*.36f,0,.51f),new Vector3(.08f,.75f,.04f),Cyan);return b;}
        if(key=="Explosive barrel")
        {b.Barrel(Vector3.zero,.46f,.95f,Steel,Quaternion.Euler(90,0,0),48);for(int n=-1;n<=1;n+=2)b.Torus(Vector3.up*n*.37f,.45f,.05f,Gold,Quaternion.Euler(90,0,0),48,12);Box(b,new Vector3(0,0,.47f),new Vector3(.35f,.48f,.09f),Suit);Box(b,new Vector3(0,.08f,.53f),new Vector3(.07f,.2f,.03f),White);Orb(b,new Vector3(0,-.1f,.53f),Vector3.one*.07f,White);return b;}
        if(key=="Health +20")
        {Box(b,Vector3.zero,new Vector3(.94f,.72f,.77f),White,.19f);Box(b,Vector3.up*.4f,new Vector3(.45f,.16f,.4f),Steel);Box(b,new Vector3(0,0,.4f),new Vector3(.5f,.13f,.06f),Cyan);Box(b,new Vector3(0,0,.4f),new Vector3(.13f,.5f,.06f),Cyan);return b;}
        return CuteArtGeometry.Make(key);
    }
}
