using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;

public static class ToyArtBuilder
{
    const string Root="Assets/CubeInvasion/Art";
    static readonly string[] Keys={"Armored body","Visor","Pulse pistol","Grunt","Sprinter","Gunner","Threat crest","Gunner barrel","Deep sides","Arena surface","Low cover","Explosive barrel","Explosive band","Health +20","Gold pickup","Gate left post","Gate right post","Gate header","Locked until clear","Right bridge","Forward bridge","Edge light","Reward beacon","Reward beacon vertical","Health cross H","Health cross V"};
    [MenuItem("Cube Invasion/Art/Build Toy Sci-Fi Assets")]
    public static void BuildAssets()
    {
        Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory("Assets/CubeInvasion/Resources");AssetDatabase.Refresh();
        var trim=Material("Graphite",new Color(.065f,.095f,.15f),false);
        var entries=new List<ToyArtLibrary.Model>();
        var metrics=new System.Text.StringBuilder("Model,Vertices,Triangles\n");
        foreach(var key in Keys)
        {
            var mesh=Make(key,out var rigParts);var path=Root+"/Meshes/"+Slug(key)+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing) {EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}
            else AssetDatabase.CreateAsset(mesh,path);
            var rig=new List<ToyArtLibrary.RigPart>();
            foreach(var part in rigParts)
            {
                var partPath=Root+"/Meshes/"+Slug(key)+"_"+part.name+".asset";
                var partMesh=AssetDatabase.LoadAssetAtPath<Mesh>(partPath);
                if(partMesh) { EditorUtility.CopySerialized(part.mesh,partMesh);Object.DestroyImmediate(part.mesh);EditorUtility.SetDirty(partMesh); }
                else {partMesh=part.mesh;AssetDatabase.CreateAsset(partMesh,partPath);}
                rig.Add(new ToyArtLibrary.RigPart {name=part.name,pivot=part.pivot,mesh=partMesh});
                metrics.AppendLine($"{key} / {part.name},{partMesh.vertexCount},{partMesh.triangles.Length/3}");
            }
            var model=new ToyArtLibrary.Model {key=key,mesh=mesh,rig=rig.ToArray(),trim=trim,accent=Material("Light_"+AccentName(key),Accent(key),true)};
            entries.Add(model);metrics.AppendLine($"{key},{mesh.vertexCount},{mesh.triangles.Length/3}");
        }
        const string libraryPath="Assets/CubeInvasion/Resources/ToyArtLibrary.asset";
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(libraryPath);
        if(!library){library=ScriptableObject.CreateInstance<ToyArtLibrary>();AssetDatabase.CreateAsset(library,libraryPath);}
        // Keep extension libraries and hand-added models when rebuilding the base toy set.
        if(library.models!=null)foreach(var model in library.models)
            if(model!=null&&!entries.Exists(item=>item.key==model.key))entries.Add(model);
        library.models=entries.ToArray();EditorUtility.SetDirty(library);
        foreach(var key in new[]{"Armored body","Grunt","Sprinter","Gunner","Explosive barrel","Low cover","Health +20","Gold pickup","Deep sides"})
        {
            var model=entries.Find(item=>item.key==key);
            var obj=Model(key,model,Material("Paint_"+Slug(key),Tint(key),false));
            if(key=="Armored body")
            {
                obj.name="Scout Robot";obj.transform.localScale=new Vector3(.65f,.55f,.65f);
                Child(obj,entries.Find(item=>item.key=="Visor"),"Visor",new Vector3(0,.3f/.55f,.29f/.65f),new Vector3(.5f/.65f,.19f/.55f,.14f/.65f),new Color(.09f,.74f,.82f));
                Child(obj,entries.Find(item=>item.key=="Pulse pistol"),"Pulse Pistol",new Vector3(.35f/.65f,0,.4f/.65f),new Vector3(.2f/.65f,.2f/.55f,.65f/.65f),new Color(.09f,.13f,.23f));
            }
            else
            {
                if(key=="Gold pickup")obj.transform.localScale=new Vector3(.28f,.07f,.28f);
                else if(key=="Health +20")obj.transform.localScale=Vector3.one*.35f;
                else if(key=="Explosive barrel")obj.transform.localScale=Vector3.one*.55f;
                else if(key=="Low cover")obj.transform.localScale=new Vector3(1.35f,.9f,.8f);
                else if(key=="Sprinter")obj.transform.localScale=new Vector3(.45f,.75f,.8f);
                else if(key=="Grunt"||key=="Gunner")obj.transform.localScale=new Vector3(.8f,.9f,.8f);
                if(key=="Gunner")Child(obj,entries.Find(item=>item.key=="Gunner barrel"),"Twin cannon",new Vector3(0,0,.7f),new Vector3(.25f,.25f,.9f),Color.white);
                if(key=="Gunner"||key=="Grunt"||key=="Sprinter")Child(obj,entries.Find(item=>item.key=="Threat crest"),"Threat crest",new Vector3(0,.65f,0),new Vector3(.18f,.4f,.18f),Color.white);
                if(key=="Explosive barrel")Child(obj,entries.Find(item=>item.key=="Explosive band"),"Hazard band",new Vector3(0,.2f,0),new Vector3(1.08f,.2f,1.08f),new Color(.9f,.29f,.41f));
                if(key=="Health +20")
                {
                    Child(obj,entries.Find(item=>item.key=="Health cross H"),"Cross H",new Vector3(0,.6f,0),new Vector3(1,.16f,.3f),Color.white);
                    Child(obj,entries.Find(item=>item.key=="Health cross V"),"Cross V",new Vector3(0,.6f,0),new Vector3(.3f,.16f,1),Color.white);
                }
                if(key=="Deep sides")
                {
                    obj.transform.localScale=new Vector3(9,2.4f,9);
                    Child(obj,entries.Find(item=>item.key=="Arena surface"),"Tiled surface",new Vector3(0,1.16f/2.4f,0),new Vector3(1,.16f/2.4f,1),new Color(.64f,.73f,.82f));
                }
            }
            if(model.rig.Length>0)obj.AddComponent<ToyActorAnimator>().ConfigurePrefab(key);
            PrefabUtility.SaveAsPrefabAsset(obj,Root+"/Prefabs/"+Slug(obj.name)+".prefab");Object.DestroyImmediate(obj);
        }
        File.WriteAllText(Root+"/MeshBudget.csv",metrics.ToString());AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Debug.Log("CUBE_TOY_ART_OK: "+entries.Count+" model entries with articulated parts and 9 reusable prefabs; no gameplay scenes rewritten.");
    }
    public static void BuildAndRender() { BuildAssets();RenderCatalog(); }
    [MenuItem("Cube Invasion/Art/Render Model Catalog")]
    public static void RenderCatalog()
    {
        // Isolated preview scene: no saved gameplay scene is opened or rewritten.
        var preview=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var cameraObject=new GameObject("Art review camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,preview);
        var camera=cameraObject.AddComponent<Camera>();camera.scene=preview;camera.orthographic=true;camera.orthographicSize=7.5f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.085f,.14f);
        camera.transform.rotation=Quaternion.Euler(38,155,0);camera.transform.position=new Vector3(0,.8f,0)-camera.transform.forward*25;
        var lightObject=new GameObject("Art key light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,preview);
        var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(45,150,0);
        int index=0;
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{Root+"/Prefabs"}))
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(asset,preview);
            var renderers=obj.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            obj.transform.localScale*=1.6f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
            bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            var cell=new Vector3((index%3-1)*4,0,(1-index/3)*4);
            obj.transform.position+=cell-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            var labelObject=new GameObject("Model label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(labelObject,preview);
            var label=labelObject.AddComponent<TextMesh>();label.text=asset.name.Replace("_"," ");label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
            label.fontSize=64;label.characterSize=.035f;label.color=Color.white;
            label.transform.position=cell+Vector3.up*2.35f;label.transform.rotation=camera.transform.rotation;index++;
        }
        var target=new RenderTexture(1800,1350,24);camera.targetTexture=target;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1800,1350,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1800,1350),0,0);image.Apply();File.WriteAllBytes("/tmp/cube-toy-catalog.png",image.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);Debug.Log("CUBE_TOY_CATALOG_OK");
    }
    static string Slug(string value)=>value.Replace(" ","_").Replace("+","Plus");
    static string AccentName(string key)=>key=="Grunt"||key=="Locked until clear"?"Rose":key=="Sprinter"||key.StartsWith("Explosive")||key=="Gold pickup"?"Amber":key=="Gunner"||key=="Gunner barrel"?"Violet":key.StartsWith("Health")?"Mint":"Cyan";
    static Color Accent(string key)
    {
        switch(AccentName(key)){case "Rose":return new Color(1,.12f,.3f);case "Amber":return new Color(1,.57f,.08f);case "Violet":return new Color(.65f,.3f,1);case "Mint":return new Color(.1f,1,.62f);default:return new Color(.1f,.85f,1);}
    }
    static Color Tint(string key)
    {
        switch(key){case "Armored body":return new Color(.22f,.36f,.9f);case "Grunt":return new Color(.9f,.29f,.41f);case "Sprinter":return new Color(1,.62f,.2f);case "Gunner":return new Color(.64f,.3f,.95f);case "Explosive barrel":return new Color(1,.38f,.16f);case "Health +20":return new Color(.09f,.8f,.57f);case "Gold pickup":return new Color(1,.72f,.2f);case "Deep sides":return new Color(.1f,.16f,.28f);default:return new Color(.25f,.34f,.46f);}
    }
    static Material Material(string name,Color color,bool glow)
    {
        string path=Root+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
        material.name=name;material.color=color;material.SetFloat("_Metallic",glow?.05f:.25f);material.SetFloat("_Glossiness",glow?.5f:.38f);
        if(glow){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*.7f);}
        EditorUtility.SetDirty(material);return material;
    }
    static GameObject Model(string name,ToyArtLibrary.Model model,Material tint)
    {
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.GetComponent<MeshFilter>().sharedMesh=model.mesh;
        obj.GetComponent<MeshRenderer>().sharedMaterials=new[]{tint,model.trim,model.accent};return obj;
    }
    static void Child(GameObject parent,ToyArtLibrary.Model model,string name,Vector3 position,Vector3 scale,Color tint)
    {var obj=Model(name,model,Material("Paint_"+Slug(model.key),tint,false));obj.transform.SetParent(parent.transform,false);obj.transform.localPosition=position;obj.transform.localScale=scale;}
    static Mesh Make(string key,out List<ToyMeshBuilder.PartMesh> rig)
    {
        var b=new ToyMeshBuilder();
        bool articulated=key=="Armored body"||key=="Grunt"||key=="Sprinter"||key=="Gunner";
        if(articulated)b.Part("Core",Vector3.zero);
        switch(key)
        {
            case "Armored body":
                b.Box(new Vector3(0,-.13f,0),new Vector3(.8f,.85f,.66f),.12f);
                b.Box(new Vector3(0,.58f,.015f),new Vector3(.97f,.68f,.82f),.13f);
                b.Box(new Vector3(0,-.08f,.34f),new Vector3(.5f,.46f,.08f),.1f,1);
                b.Box(new Vector3(0,-.05f,.395f),new Vector3(.18f,.2f,.05f),.1f,2);
                b.Box(new Vector3(0,.1f,-.39f),new Vector3(.54f,.6f,.25f),.12f,1);
                for(int side=-1;side<=1;side+=2)
                {
                    b.Part(side<0?"LeftArm":"RightArm",new Vector3(side*.47f,.2f,0));
                    b.Box(new Vector3(side*.47f,-.1f,0),new Vector3(.24f,.65f,.4f),.14f);
                    b.Box(new Vector3(side*.47f,-.43f,.04f),new Vector3(.25f,.22f,.42f),.1f,1);
                    b.Part(side<0?"LeftLeg":"RightLeg",new Vector3(side*.23f,-.53f,0));
                    b.Box(new Vector3(side*.23f,-.74f,.05f),new Vector3(.3f,.42f,.46f),.1f,1);
                    b.Box(new Vector3(side*.23f,-.91f,.12f),new Vector3(.32f,.15f,.55f),.1f);
                    b.Part(side<0?"LeftArm":"RightArm",new Vector3(side*.47f,.2f,0));
                    b.Box(new Vector3(side*.5f,.04f,.21f),new Vector3(.12f,.15f,.05f),.08f,2);
                }
                b.Part("Core",Vector3.zero);
                b.Box(new Vector3(.3f,1,0),new Vector3(.08f,.24f,.08f),.08f,1);b.Box(new Vector3(.3f,1.13f,0),Vector3.one*.12f,.1f,2);break;
            case "Grunt":
                b.Box(new Vector3(0,.08f,0),new Vector3(.94f,.73f,.85f),.14f);
                b.Box(new Vector3(0,.1f,.43f),new Vector3(.66f,.28f,.13f),.1f,1);
                for(int s=-1;s<=1;s+=2){b.Part("Core",Vector3.zero);b.Box(new Vector3(s*.19f,.11f,.51f),new Vector3(.12f,.1f,.06f),.08f,2);
                    b.Part(s<0?"LeftLeg":"RightLeg",new Vector3(s*.36f,-.23f,0));b.Box(new Vector3(s*.36f,-.37f,.05f),new Vector3(.27f,.26f,.65f),.08f,1);
                    b.Part("Core",Vector3.zero);b.Box(new Vector3(s*.48f,.02f,-.05f),new Vector3(.13f,.46f,.5f),.1f);}
                b.Box(new Vector3(0,.48f,-.1f),new Vector3(.55f,.13f,.4f),.1f,1);break;
            case "Sprinter":
                b.Wedge(new Vector3(0,.15f,.05f),new Vector3(.95f,.55f,1.25f));
                b.Box(new Vector3(0,.02f,-.05f),new Vector3(.64f,.4f,.74f),.1f,1);
                b.Box(new Vector3(0,.35f,.25f),new Vector3(.18f,.12f,.4f),.1f,2);
                for(int s=-1;s<=1;s+=2){b.Part(s<0?"LeftLeg":"RightLeg",new Vector3(s*.4f,-.14f,-.05f));b.Box(new Vector3(s*.4f,-.3f,-.05f),new Vector3(.2f,.36f,.9f),.1f,1);
                    b.Box(new Vector3(s*.4f,-.39f,.4f),new Vector3(.16f,.1f,.22f),.1f,2);b.Part("Core",Vector3.zero);b.Wedge(new Vector3(s*.35f,.5f,-.3f),new Vector3(.18f,.45f,.55f));}break;
            case "Gunner":
                b.Box(new Vector3(0,-.27f,0),new Vector3(.95f,.36f,.95f),.14f,1);b.Drum(new Vector3(0,.06f,0),.44f,.63f);b.Box(new Vector3(0,.41f,-.03f),new Vector3(.76f,.17f,.7f),.1f);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                {b.Part((x<0?"Left":"Right")+(z<0?"BackLeg":"FrontLeg"),new Vector3(x*.39f,-.28f,z*.34f));b.Box(new Vector3(x*.39f,-.4f,z*.34f),new Vector3(.22f,.18f,.3f),.08f);}
                b.Part("Core",Vector3.zero);b.Box(new Vector3(0,.21f,.43f),new Vector3(.5f,.11f,.07f),.08f,2);break;
            case "Pulse pistol":case "Gunner barrel":
                b.Box(new Vector3(0,.06f,0),new Vector3(.9f,.75f,.88f),.1f);
                b.Box(new Vector3(0,0,.47f),new Vector3(.78f,.65f,.16f),.08f,1);
                b.Box(new Vector3(0,0,.56f),new Vector3(.44f,.3f,.04f),.08f,2);
                b.Box(new Vector3(0,-.53f,-.2f),new Vector3(.58f,.55f,.35f),.1f,1);
                b.Box(new Vector3(0,.48f,-.1f),new Vector3(.2f,.13f,.22f),.08f,2);break;
            case "Visor":
                b.Box(Vector3.zero,Vector3.one,.16f,1);b.Box(new Vector3(0,0,.48f),new Vector3(.8f,.7f,.08f),.12f,2);break;
            case "Deep sides":
                b.Box(Vector3.zero,Vector3.one,.035f);b.Box(new Vector3(0,-.43f,0),new Vector3(1.02f,.14f,1.02f),.04f,1);
                for(int s=-1;s<=1;s+=2)
                {
                    b.Box(new Vector3(s*.465f,0,0),new Vector3(.07f,.82f,1.01f),.04f,1);
                    for(int i=-1;i<=1;i++) {b.Box(new Vector3(i*.21f,-.09f,s*.5f),new Vector3(.12f,.28f,.013f),.04f,1);b.Box(new Vector3(s*.5f,-.09f,i*.21f),new Vector3(.013f,.28f,.12f),.04f,1);}
                    b.Box(new Vector3(0,.26f,s*.505f),new Vector3(.48f,.025f,.012f),.03f,2);
                }break;
            case "Arena surface":
                b.Box(new Vector3(0,-.08f,0),new Vector3(1,.84f,1),.015f,1);
                for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)b.Box(new Vector3(x*.331f,.45f,z*.331f),new Vector3(.326f,.1f,.326f),.02f);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)b.Box(new Vector3(x*.43f,.52f,z*.43f),new Vector3(.045f,.025f,.008f),.01f,2);
                break;
            case "Low cover":
                b.Box(Vector3.zero,Vector3.one,.09f);b.Box(new Vector3(0,.39f,0),new Vector3(1.04f,.19f,1.04f),.08f,1);
                for(int s=-1;s<=1;s+=2){b.Box(new Vector3(s*.42f,0,0),new Vector3(.14f,.86f,1.04f),.05f,1);b.Box(new Vector3(0,.05f,s*.505f),new Vector3(.4f,.09f,.025f),.05f,2);b.Box(new Vector3(0,-.28f,s*.51f),new Vector3(.6f,.1f,.025f),.05f,1);}break;
            case "Explosive barrel":
                b.Drum(Vector3.zero,.5f,2);b.Drum(new Vector3(0,.89f,0),.53f,.16f,1);b.Drum(new Vector3(0,-.89f,0),.53f,.16f,1);b.Drum(new Vector3(0,1,0),.23f,.1f,1);
                for(int s=-1;s<=1;s+=2)b.Box(new Vector3(0,.28f,s*.49f),new Vector3(.26f,.45f,.06f),.05f,2);break;
            case "Explosive band":b.Drum(Vector3.zero,.51f,1,1);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;b.Box(new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f),new Vector3(.16f,.8f,.025f),.02f,2,Quaternion.Euler(0,-a*Mathf.Rad2Deg+90,0));}break;
            case "Health +20":
                b.Box(Vector3.zero,new Vector3(1.2f,.85f,1),.12f);b.Box(new Vector3(0,.34f,0),new Vector3(1.25f,.18f,1.05f),.1f,1);
                b.Box(new Vector3(0,.65f,0),new Vector3(.65f,.18f,.25f),.12f,1);b.Box(new Vector3(0,.16f,.51f),new Vector3(.5f,.1f,.04f),.04f,2);break;
            case "Gold pickup":b.Drum(Vector3.zero,.5f,2,0,12);b.Drum(new Vector3(0,1.02f,0),.36f,.08f,1,12);b.Box(new Vector3(0,1.08f,0),new Vector3(.15f,.06f,.46f),.08f,2);break;
            case "Gate left post":case "Gate right post":
                b.Box(Vector3.zero,Vector3.one,.07f,1);b.Box(new Vector3(0,0,.48f),new Vector3(.6f,.8f,.08f),.05f,2);
                b.Box(new Vector3(0,-.43f,0),new Vector3(1.25f,.14f,1.4f),.08f);b.Box(new Vector3(0,.43f,0),new Vector3(1.2f,.14f,1.2f),.08f);break;
            case "Gate header":
                b.Box(Vector3.zero,Vector3.one,.08f);b.Box(new Vector3(0,0,.48f),new Vector3(.7f,.3f,.08f),.04f,2);b.Box(new Vector3(0,-.45f,0),new Vector3(.9f,.13f,.75f),.05f,1);break;
            case "Locked until clear":
                b.Box(Vector3.zero,Vector3.one,.025f,1);for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.2f,0,.51f),new Vector3(.055f,.75f,.04f),.03f,2,Quaternion.Euler(0,0,s*25));break;
            case "Right bridge":case "Forward bridge":
                b.Box(Vector3.zero,Vector3.one,.025f,1);for(int i=-2;i<=2;i++)b.Box(new Vector3(0,.5f,i*.19f),new Vector3(.96f,.08f,.15f),.025f);
                for(int s=-1;s<=1;s+=2)b.Box(new Vector3(s*.46f,.55f,0),new Vector3(.02f,.03f,.96f),.02f,2);break;
            case "Threat crest":b.Wedge(Vector3.zero,Vector3.one,1);b.Box(new Vector3(0,.1f,0),new Vector3(.5f,.5f,.1f),.08f,2);break;
            default:b.Box(Vector3.zero,Vector3.one,.09f,key=="Edge light"||key.StartsWith("Reward")?2:0);break;
        }
        rig=b.BuildParts("CI_"+Slug(key));
        return b.Build("CI_"+Slug(key));
    }
}
