using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;

// KİP, the "Rogue cube" player hero (Documentation/RogueCube_Hero_Brief_TR.md): a cream factory cube that
// changed sides. The body is the head; screen-face, cyan scarf, antenna and two stubby legs carry the read.
public static partial class VoidToyArtGeometry
{
    public const string Hero="Rogue cube";
    public static readonly string[] HeroFaces={"Neutral","Determined","Surprised","Pain","Happy","Dizzy"};
    public static readonly string[] HeroFaceParts={"LeftEye","RightEye","Mouth"};
    static readonly Color HeroCyan=VoidToyPalette.Hex(0x22C7D6),HeroCyanInner=VoidToyPalette.CyanShadow,Screen=VoidToyPalette.Hex(0x07111F),
        Emboss=Color.Lerp(VoidToyPalette.Text,VoidToyPalette.Muted,.35f),FootBlue=Color.Lerp(VoidToyPalette.Cobalt,VoidToyPalette.Ink,.25f);
    // Face pivots (ground space); the expression variants are built around the same points.
    static readonly Vector3 LeftEyeAt=new Vector3(-.1f,.72f,.33f),RightEyeAt=new Vector3(.1f,.72f,.33f),MouthAt=new Vector3(0,.6f,.33f);
    static Color Emissive(Color c,float amount) {c.a=1-amount;return c;}
    static VoidToyMeshBuilder RogueCube(bool low)
    {
        var b=Fitted(Hero);b.Low=low;
        // Lower body: Hips carries both legs so the legs can follow the move direction while Core aims.
        b.PartAt("Hips",new Vector3(0,.31f,0));
        b.Box(new Vector3(0,.32f,0),new Vector3(.46f,.06f,.36f),Cobalt,.25f);
        for(int s=-1;s<=1;s+=2)
        {
            b.PartAt(s<0?"LeftLeg":"RightLeg",new Vector3(s*.19f,.31f,0),"Hips");
            b.Box(new Vector3(s*.19f,.29f,0),new Vector3(.17f,.04f,.21f),Dark,.3f);
            b.Box(new Vector3(s*.19f,.18f,0),new Vector3(.16f,.22f,.2f),Cobalt,.18f);
            b.Box(new Vector3(s*.19f,.035f,.03f),new Vector3(.2f,.07f,.26f),FootBlue,.45f);
        }
        // Core is one piece so the hit flash covers the whole body.
        b.PartAt("Core",new Vector3(0,.61f,0));
        b.Box(new Vector3(0,.66f,0),new Vector3(.72f,.66f,.64f),White,.125f);
        b.Box(new Vector3(0,.355f,0),new Vector3(.732f,.05f,.652f),Cobalt,.3f);
        // Top face carries identity from the high camera: cyan stripe, antenna base, raised K-1P serial.
        b.Box(new Vector3(0,.993f,-.02f),new Vector3(.12f,.014f,.5f),Emissive(HeroCyan,.3f),.3f);
        Serial(b,new Vector3(.1f,.997f,.1f));
        // Recessed screen-face: dark glass inside a raised white bezel.
        b.Box(new Vector3(0,.69f,.316f),new Vector3(.46f,.34f,.02f),Screen,.2f);
        for(int s=-1;s<=1;s+=2)
        {b.Box(new Vector3(s*.245f,.69f,.333f),new Vector3(.03f,.4f,.03f),White,.3f);b.Box(new Vector3(0,.69f+s*.185f,.333f),new Vector3(.52f,.03f,.03f),White,.3f);}
        // Scarf wrapped round the neck (top edge); the trailing tails are the ScarfA-C chain.
        // Sits a little below the top edge so a white lip shows above it and it reads as wrapped cloth, not a rim.
        b.Box(new Vector3(0,.9f,.336f),new Vector3(.75f,.07f,.04f),HeroCyan,.25f);
        b.Box(new Vector3(0,.9f,-.336f),new Vector3(.75f,.07f,.04f),HeroCyan,.25f);
        for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.376f,.9f,0),new Vector3(.04f,.07f,.712f),HeroCyan,.25f);
        foreach(var part in HeroFaceParts) {var at=FaceAt(part);b.PartAt(part,at,"Core");Face(b,"Neutral",part,at);}
        b.PartAt("LeftArm",new Vector3(-.36f,.7f,0),"Core");
        b.Box(new Vector3(-.375f,.7f,0),new Vector3(.05f,.13f,.13f),Dark,.3f);
        b.Box(new Vector3(-.405f,.58f,.02f),new Vector3(.11f,.28f,.12f),Cobalt,.25f,Quaternion.Euler(0,0,-6));
        b.Box(new Vector3(-.41f,.43f,.03f),new Vector3(.12f,.07f,.13f),White,.3f);
        // Weapon arm: reaches the authored fire anchor (0.35 right, 0.40 forward); the gun attaches here.
        b.PartAt("RightHand",new Vector3(.36f,.7f,0),"Core");
        b.Box(new Vector3(.375f,.7f,0),new Vector3(.05f,.13f,.13f),Dark,.3f);
        b.Box(new Vector3(.405f,.64f,.13f),new Vector3(.11f,.11f,.28f),Cobalt,.25f,Quaternion.Euler(12,0,0));
        b.Box(new Vector3(.4f,.6f,.27f),new Vector3(.12f,.12f,.08f),White,.3f);
        // Antenna: short stalk, signal ball tint-masked (UV0.x) for the gold pick-up / fire flash.
        b.PartAt("Antenna",new Vector3(0,.99f,-.1f),"Core");
        b.Box(new Vector3(0,1.005f,-.1f),new Vector3(.08f,.03f,.08f),Dark,.3f);
        b.Cylinder(new Vector3(0,1.095f,-.1f),.016f,.16f,Dark,Quaternion.Euler(90,0,0),6);
        b.Tint=1;b.Sphere(new Vector3(0,1.21f,-.1f),Vector3.one*.07f,Glow(HeroCyan),10,6);b.Tint=0;
        // Two scarf tails hang from the back of the neck as a 3-link chain (pivot at each link's front end).
        Vector3[] chain={new Vector3(0,.9f,-.356f),new Vector3(0,.85f,-.49f),new Vector3(0,.77f,-.6f),new Vector3(0,.64f,-.68f)};
        string[] links={"ScarfA","ScarfB","ScarfC"};
        for(int k=0;k<3;k++)
        {
            b.PartAt(links[k],chain[k],k==0?"Core":links[k-1]);var along=chain[k+1]-chain[k];var q=Quaternion.LookRotation(along);
            for(int s=-1;s<=1;s+=2)
            {
                var offset=Vector3.right*s*(.072f+.022f*k);
                b.Plain((chain[k]+chain[k+1])*.5f+offset,new Vector3(.13f,.03f,along.magnitude+.02f),HeroCyan,q*Quaternion.Euler(0,0,-s*6),HeroCyanInner);
                if(k==2)b.Plain(chain[3]+offset+along.normalized*.03f,new Vector3(.08f,.05f,.05f),Cobalt,q);
            }
        }
        return b;
    }
    static Vector3 FaceAt(string part)=>part=="LeftEye"?LeftEyeAt:part=="RightEye"?RightEyeAt:MouthAt;
    // Raised "K-1P" serial on the top face, read with +Z as up.
    static void Serial(VoidToyMeshBuilder b,Vector3 o)
    {
        void Bar(float x,float z,float w,float d,float yaw=0)=>b.Plain(o+new Vector3(x,0,z),new Vector3(w,.008f,d),Emboss,Quaternion.Euler(0,yaw,0));
        Bar(0,0,.016f,.09f);Bar(.025f,.02f,.05f,.014f,-45);Bar(.025f,-.02f,.05f,.014f,45);
        Bar(.068f,0,.026f,.012f);
        Bar(.104f,0,.016f,.09f);
        Bar(.14f,0,.016f,.09f);Bar(.158f,.039f,.036f,.012f);Bar(.158f,.004f,.036f,.012f);Bar(.176f,.022f,.012f,.044f);
    }
    // Emissive cyan screen glyphs; round shapes use chamfered blocks, line strokes cheap plain bars.
    static void Face(VoidToyMeshBuilder b,string expression,string part,Vector3 at)
    {
        var light=Glow(HeroCyan);bool eye=part!="Mouth";float side=part=="LeftEye"?1:-1;var z=Vector3.forward*.004f;
        void Stroke(float x,float y,float w,float h,float roll=0)=>b.Plain(at+z+new Vector3(x,y,0),new Vector3(w,h,.02f),light,Quaternion.Euler(0,0,roll));
        switch(expression)
        {
            case "Neutral":
                if(eye)b.Box(at+z,new Vector3(.09f,.09f,.02f),light,.35f);else Stroke(0,0,.12f,.025f);break;
            case "Determined":
                if(eye) {b.Box(at+z+Vector3.down*.012f,new Vector3(.1f,.055f,.02f),light,.2f);Stroke(0,.03f,.1f,.014f,side*-10);}
                else {Stroke(0,.006f,.04f,.018f);Stroke(-.032f,-.006f,.032f,.016f,30);Stroke(.032f,-.006f,.032f,.016f,-30);}
                break;
            case "Surprised":
                if(eye)b.Box(at+z,new Vector3(.12f,.12f,.02f),light,.45f);
                else {Stroke(0,.022f,.05f,.012f);Stroke(0,-.022f,.05f,.012f);Stroke(.022f,0,.012f,.05f);Stroke(-.022f,0,.012f,.05f);}
                break;
            case "Pain":
                if(eye) {Stroke(-side*.005f,.015f,.065f,.016f,-side*30);Stroke(-side*.005f,-.015f,.065f,.016f,side*30);}
                else for(int i=0;i<4;i++)Stroke(-.045f+i*.03f,0,.036f,.014f,i%2==0?35:-35);
                break;
            case "Happy":
                if(eye) {Stroke(-.03f,-.008f,.016f,.04f);Stroke(.03f,-.008f,.016f,.04f);Stroke(0,.02f,.076f,.016f);}
                else {Stroke(0,-.016f,.08f,.018f);Stroke(-.05f,0,.018f,.032f,20);Stroke(.05f,0,.018f,.032f,-20);}
                break;
            default: // Dizzy: spiral eyes (the runtime spins them in opposite directions), skewed mouth.
                if(eye)for(int i=0;i<5;i++) {float a=(i*80+(side>0?0:180))*Mathf.Deg2Rad,r=.045f-.008f*i;Stroke(Mathf.Cos(a)*r,Mathf.Sin(a)*r,.032f,.014f,i*80+90);}
                else {Stroke(0,0,.1f,.02f,12);Stroke(.055f,.018f,.025f,.016f,-30);}
                break;
        }
    }
    // Expression variants share the default face pivots, so a runtime can swap the meshes on LeftEye / RightEye / Mouth.
    public static VoidToyMeshBuilder HeroFace(string expression)
    {
        var b=Fitted(Hero);foreach(var part in HeroFaceParts) {var at=FaceAt(part);b.PartAt(part,at,"Core");Face(b,expression,part,at);}return b;
    }
}
