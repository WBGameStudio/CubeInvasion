using UnityEngine;
using CubeInvasion.ArtTools;

public static class CuteArtGeometry
{
    static readonly Color Suit=CuteMeshBuilder.Suit,Ink=CuteMeshBuilder.Ink,Cream=CuteMeshBuilder.Cream,Cyan=CuteMeshBuilder.Glow,Pink=CuteMeshBuilder.Cheek;
    static readonly Color Gold=new Color(1,.73f,.13f),Mint=new Color(.36f,.92f,.71f);
    public static bool Actor(string key)=>key=="Armored body"||key=="Grunt"||key=="Sprinter"||key=="Gunner"||key=="Pusher"||key=="Sniper"||key=="Bruiser"||key=="Crusher";
    public static bool Weapon(string key)=>key=="Pulse pistol"||key=="Twin blasters"||key=="Scatter shot"||key=="Rapid smg"||key=="Gunner barrel";
    public static Color Tint(string key)
    {
        switch(key)
        {
            case "Armored body":return new Color(.24f,.59f,.95f);
            case "Grunt":return new Color(1,.43f,.52f);
            case "Sprinter":return new Color(1,.76f,.19f);
            case "Gunner":return new Color(.67f,.51f,.94f);
            case "Pusher":return new Color(1,.57f,.24f);
            case "Sniper":return new Color(.29f,.81f,.65f);
            case "Bruiser":return new Color(.93f,.41f,.58f);
            case "Crusher":return new Color(.39f,.44f,.85f);
            case "Gold pickup":return Gold;
            case "Health +20":return Mint;
            case "Arena surface":return new Color(.86f,.91f,.96f);
            case "Deep sides":return new Color(.25f,.36f,.55f);
            case "Explosive barrel":return new Color(1,.53f,.35f);
            case "Scatter shot":return new Color(1,.69f,.3f);
            case "Rapid smg":return Mint;
            default:return new Color(.37f,.64f,.91f);
        }
    }
    public static CuteMeshBuilder Make(string key)
    {
        var b=new CuteMeshBuilder();if(Actor(key))Creature(b,key);else if(Weapon(key))Blaster(b,key);else Prop(b,key);return b;
    }
    static void Creature(CuteMeshBuilder b,string key)
    {
        bool hero=key=="Armored body",boss=key=="Crusher",bear=key=="Bruiser",bunny=key=="Sprinter",crab=key=="Pusher",squid=key=="Gunner",gecko=key=="Sniper";
        float width=boss?1.96f:bear?1.25f:hero?1:.94f;
        float headY=boss?.62f:hero?.53f:.34f,headW=width*(hero?1.1f:1),headH=boss?1.04f:hero?.83f:.74f,headD=boss?1.15f:.83f;
        float footY=hero?-.88f:boss?-.76f:bear?-.74f:-.64f;
        b.PartAt("Core",Vector3.zero);
        b.Sphere(new Vector3(0,hero?-.23f:-.18f,-.025f),new Vector3(width*.79f,boss?1.2f:.83f,headD*.85f),Suit,32,20);
        b.Sphere(new Vector3(0,-.2f,headD*.37f),new Vector3(width*.57f,.55f,.095f),Cream,28,16);
        b.Sphere(new Vector3(0,-.18f,headD*.43f),new Vector3(.17f,.18f,.06f),Cyan,24,14);
        // A padded backpack makes the silhouette readable when seen from behind.
        b.RoundBox(new Vector3(0,-.04f,-headD*.48f),new Vector3(width*.6f,.55f,.25f),Cream,.36f,12);
        b.Sphere(new Vector3(0,-.015f,-headD*.64f),new Vector3(width*.36f,.31f,.09f),Cyan,24,14);
        b.Torus(new Vector3(0,headY-headH*.43f,0),width*.28f,.065f,Ink,Quaternion.Euler(90,0,0));
        b.PartAt("Head",new Vector3(0,headY,0));
        b.Sphere(new Vector3(0,headY,.01f),new Vector3(headW,headH,headD),Suit,boss?40:36,boss?24:22);
        float faceZ=headD*.46f;
        b.Sphere(new Vector3(0,headY,faceZ),new Vector3(headW*.82f,headH*.68f,.12f),Cream,36,20);
        b.Sphere(new Vector3(0,headY+.015f,faceZ+.055f),new Vector3(headW*.74f,headH*.59f,.12f),Ink,36,20);
        float eyeX=headW*.19f,eyeY=headY+.045f,eyeZ=faceZ+.115f;
        for(int side=-1;side<=1;side+=2)
        {
            var at=new Vector3(side*eyeX,eyeY,eyeZ);
            b.PartAt(side<0?"LeftEye":"RightEye",at,"Head");
            b.Sphere(at,new Vector3(headW*.235f,headH*.34f,.073f),Cream,28,18);
            b.Sphere(at+new Vector3(-side*.015f,0,.045f),new Vector3(headW*.112f,headH*.22f,.047f),Ink,24,16);
            b.Sphere(at+new Vector3(-.026f,.045f,.071f),new Vector3(headW*.047f,headH*.075f,.025f),Color.white,16,12);
            b.PartAt("Head",new Vector3(0,headY,0));
            b.Sphere(new Vector3(side*headW*.285f,headY-headH*.15f,faceZ+.16f),new Vector3(headW*.14f,headH*.085f,.025f),Pink,20,12);
        }
        b.Sphere(new Vector3(0,headY-headH*.17f,faceZ+.12f),new Vector3(headW*.105f,headH*.05f,.025f),Cream,20,12);
        // Articulated soft antennae / ears, with archetype-specific outlines.
        for(int side=-1;side<=1;side+=2)
        {
            var pivot=new Vector3(side*headW*.32f,headY+headH*.4f,-.06f);
            b.PartAt(side<0?"LeftEar":"RightEar",pivot,"Head");
            float length=bunny?.63f:hero?(side<0?.37f:.24f):bear?.22f:boss?.27f:gecko?.37f:.2f;
            var q=Quaternion.Euler(0,0,-side*(bunny?18:hero?12:25));
            var size=new Vector3(bear?.29f:boss?.31f:.16f,length,bear?.24f:.19f);
            b.Sphere(pivot+q*(Vector3.up*length*.28f),size,bear?Suit:hero?Cream:Suit,26,16,q);
            b.Sphere(pivot+q*(Vector3.up*length*.55f),new Vector3(size.x*.65f,length*.23f,size.z*.8f),hero?Cyan:Pink,20,12,q);
        }
        if(boss)
        {
            b.PartAt("Head",new Vector3(0,headY,0));
            b.RoundBox(new Vector3(0,headY+headH*.45f,.04f),new Vector3(.84f,.17f,.52f),Gold,.3f,14);
            for(int i=-1;i<=1;i++)b.Sphere(new Vector3(i*.27f,headY+headH*.59f+(i==0?.08f:0),.03f),new Vector3(.2f,.32f,.25f),Gold,24,16);
        }
        int feet=squid?4:2;
        for(int i=0;i<feet;i++)
        {
            int side=i%2==0?-1:1;bool back=squid&&i>=2;float z=squid?(back?-.27f:.28f):.03f;
            string name=squid?(side<0?"Left":"Right")+(back?"BackLeg":"FrontLeg"):(side<0?"LeftLeg":"RightLeg");
            b.PartAt(name,new Vector3(side*width*.235f,footY+.25f,z));
            b.Sphere(new Vector3(side*width*.235f,footY+.12f,z),new Vector3(width*.24f,.42f,.3f),Suit,24,16);
            b.RoundBox(new Vector3(side*width*.235f,footY,.12f+z),new Vector3(width*(bunny?.38f:.31f),.25f,boss?.76f:bunny?.69f:.5f),Cream,.4f,12);
            b.Sphere(new Vector3(side*width*.235f,footY+.07f,.33f+z),new Vector3(width*.19f,.13f,.1f),Suit,22,14);
        }
        for(int side=-1;side<=1;side+=2)
        {
            var pivot=new Vector3(side*width*.46f,.03f,0);b.PartAt(side<0?"LeftArm":"RightArm",pivot);
            b.Sphere(pivot+new Vector3(side*.025f,-.17f,0),new Vector3(width*.27f,.53f,.35f),Suit,28,18);
            var hand=new Vector3(side*width*.52f,-.34f,.12f);
            float glove=boss?.8f:bear?.6f:.34f;
            b.Sphere(hand,new Vector3(glove,.37f*(boss?1.6f:1),boss?.72f:.4f),bear||boss?Cream:Suit,30,18);
            if(crab)
            {
                b.Sphere(hand+new Vector3(side*.02f,.12f,.2f),new Vector3(.55f,.74f,.28f),Cream,32,20);
                b.Sphere(hand+new Vector3(side*.02f,.12f,.35f),new Vector3(.39f,.52f,.11f),Cyan,28,18);
            }
            if(bear||boss)
            {
                b.Torus(hand+new Vector3(0,0,.13f),glove*.32f,.065f,hero?Cyan:Pink);
                for(int n=-1;n<=1;n++)b.Sphere(hand+new Vector3(n*glove*.18f,.02f,.22f),new Vector3(glove*.19f,.19f,.12f),boss?Cyan:Pink,20,12);
            }
            if((squid||gecko)&&side==1)
            {
                float length=gecko?.95f:.57f;var cannon=hand+new Vector3(0,.07f,length*.34f);
                b.Barrel(cannon,gecko?.12f:.18f,length,Cream);
                b.Torus(cannon+Vector3.forward*length*.5f,gecko?.09f:.14f,.035f,Cyan);
                b.Sphere(cannon+Vector3.forward*(length*.5f+.01f),new Vector3(.13f,.13f,.025f),Ink,24,14);
            }
        }
        if(bunny||gecko)
        {
            b.PartAt("Tail",new Vector3(0,-.28f,-.37f));
            b.Sphere(new Vector3(0,-.25f,-.57f),gecko?new Vector3(.26f,.24f,.85f):Vector3.one*.3f,gecko?Suit:Cream,28,16,Quaternion.Euler(-20,0,0));
        }
    }
    static void Blaster(CuteMeshBuilder b,string key)
    {
        bool twin=key=="Twin blasters",scatter=key=="Scatter shot",rapid=key=="Rapid smg";
        b.RoundBox(new Vector3(0,scatter?-.06f:.07f,scatter?-.3f:-.12f),new Vector3(scatter?.8f:.94f,scatter?.48f:.65f,scatter?.5f:.79f),Suit,.4f,16);
        b.RoundBox(new Vector3(0,-.47f,-.26f),new Vector3(.38f,.61f,.39f),Cream,.35f,12,Quaternion.Euler(-12,0,0));
        b.Sphere(new Vector3(0,scatter?.3f:.42f,scatter?-.38f:-.13f),new Vector3(.48f,.29f,scatter?.27f:.54f),Cyan,28,18);
        int barrels=twin?2:1;
        for(int i=0;i<barrels;i++)
        {
            float x=twin?(i==0?-.26f:.26f):0,radius=scatter?.48f:twin?.2f:.26f;
            b.Barrel(new Vector3(x,.05f,.24f),radius,.65f,Cream);
            b.Torus(new Vector3(x,.05f,.565f),radius*.78f,.045f,scatter?Gold:Cyan);
            b.Sphere(new Vector3(x,.05f,.586f),new Vector3(radius*1.25f,radius*1.25f,.035f),Ink,28,16);
            if(scatter)for(int n=-1;n<=1;n++)b.Sphere(new Vector3(n*.16f,.05f,.61f),new Vector3(.1f,.16f,.035f),Cyan,20,12);
            else b.Sphere(new Vector3(x,.05f,.613f),new Vector3(radius*.65f,radius*.65f,.035f),Cyan,24,14);
        }
        if(rapid)b.Barrel(new Vector3(0,-.25f,.02f),.28f,.36f,Suit,Quaternion.Euler(0,90,0));
        b.Sphere(new Vector3(-.48f,.05f,-.2f),new Vector3(.08f,.25f,.25f),Cyan,24,14);
    }
    static void Prop(CuteMeshBuilder b,string key)
    {
        switch(key)
        {
            case "Gold pickup":
                b.Barrel(Vector3.zero,.49f,.85f,Suit,Quaternion.Euler(90,0,0),40);
                b.Torus(Vector3.up*.45f,.33f,.055f,Cream,Quaternion.Euler(90,0,0));
                b.RoundBox(Vector3.up*.48f,new Vector3(.33f,.08f,.33f),Cream,.18f,8,Quaternion.Euler(0,45,0));break;
            case "Explosive barrel":
                b.Barrel(Vector3.zero,.46f,.95f,Suit,Quaternion.Euler(90,0,0),36);
                b.Sphere(new Vector3(0,.12f,.46f),new Vector3(.44f,.45f,.08f),Cream,28,18);
                b.RoundBox(new Vector3(0,.12f,.51f),new Vector3(.1f,.26f,.04f),Pink,.3f,8);
                b.Sphere(new Vector3(0,-.08f,.51f),new Vector3(.1f,.1f,.04f),Pink,18,12);break;
            case "Explosive band":b.Torus(Vector3.zero,.48f,.08f,Suit,Quaternion.Euler(90,0,0));break;
            case "Health +20":
                b.RoundBox(Vector3.zero,new Vector3(.94f,.8f,.84f),Cream,.38f,16);
                b.RoundBox(new Vector3(0,.5f,0),new Vector3(.5f,.26f,.5f),Suit,.35f,12);
                b.RoundBox(new Vector3(0,0,.44f),new Vector3(.55f,.14f,.07f),Mint,.3f,8);
                b.RoundBox(new Vector3(0,0,.44f),new Vector3(.14f,.55f,.07f),Mint,.3f,8);break;
            case "Deep sides":b.RoundBox(Vector3.zero,Vector3.one,Suit,.065f,18);b.RoundBox(Vector3.down*.4f,new Vector3(1.005f,.12f,1.005f),Ink,.1f,12);break;
            case "Arena surface":b.RoundBox(Vector3.zero,Vector3.one,Suit,.035f,18);break;
            case "Low cover":
                b.RoundBox(Vector3.zero,Vector3.one,Suit,.2f,18);
                b.RoundBox(new Vector3(0,.47f,0),new Vector3(1.02f,.22f,1.02f),Cream,.3f,12);break;
            case "Locked until clear":
                b.RoundBox(Vector3.zero,Vector3.one,Suit,.16f,12);
                b.RoundBox(new Vector3(0,0,.5f),new Vector3(.4f,.08f,.04f),Cream,.3f,8,Quaternion.Euler(0,0,45));
                b.RoundBox(new Vector3(0,0,.5f),new Vector3(.4f,.08f,.04f),Cream,.3f,8,Quaternion.Euler(0,0,-45));break;
            case "Gate left post":case "Gate right post":case "Gate header":b.RoundBox(Vector3.zero,Vector3.one,Suit,.4f,18);break;
            case "Visor":b.Sphere(Vector3.zero,Vector3.one,Ink,28,18);break;
            case "Threat crest":b.Sphere(Vector3.zero,Vector3.one,Gold,28,18);break;
            default:b.RoundBox(Vector3.zero,Vector3.one,Suit,.2f,12);break;
        }
    }
}
