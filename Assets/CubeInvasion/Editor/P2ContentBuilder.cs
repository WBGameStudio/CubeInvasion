using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;
using Object=UnityEngine.Object;

public static class P2ContentBuilder
{
    const string Root="Assets/CubeInvasion/Art/P2";
    const string CatalogPath="Assets/CubeInvasion/Resources/P2Content.asset";
    [MenuItem("Cube Invasion/P2/Install Content In Current Scene")]
    public static void Install()
    {
        Directory.CreateDirectory(Root);Directory.CreateDirectory(Root+"/Models");Directory.CreateDirectory(Root+"/Audio");AssetDatabase.Refresh();
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");
        BuildModels(library);
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>(CatalogPath);
        if(!catalog)
        {
            catalog=ScriptableObject.CreateInstance<P2ContentCatalog>();catalog.weapons=Weapons();catalog.missions=Missions();
            AssetDatabase.CreateAsset(catalog,CatalogPath);
        }
        var bank=BuildAudio();
        var runtime=Object.FindAnyObjectByType<InvasionPrototype>();
        if(!runtime)throw new InvalidOperationException("Open the gameplay scene first.");
        runtime.EnsureSceneSetup();
        var hud=Object.FindAnyObjectByType<MobileCombatHud>();
        var presentation=Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);
        if(!presentation)presentation=new GameObject("P2 Mission HUD",typeof(RectTransform)).AddComponent<P2MissionHud>();
        presentation.Initialize(hud.SafeRoot);
        runtime.ConfigureContent(catalog,presentation);
        var feedback=Object.FindAnyObjectByType<CombatFeedback>();feedback.ConfigureAudio(bank);
        EditorUtility.SetDirty(runtime);EditorUtility.SetDirty(feedback);EditorUtility.SetDirty(presentation);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(runtime.gameObject.scene);EditorSceneManager.SaveScene(runtime.gameObject.scene);
        Debug.Log("CUBE_P2_INSTALL_OK: 6 enemy archetypes + Crusher, 4 weapons, 5 combat missions + Gold Rush + 3-phase boss.");
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
    [MenuItem("Cube Invasion/P2/Render Content Catalog")]
    public static void RenderCatalog()
    {
        var scene=EditorSceneManager.NewPreviewScene();
        var cameraObject=new GameObject("P2 art camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);
        var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.orthographicSize=5.4f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.06f,.105f);
        camera.transform.rotation=Quaternion.Euler(38,155,0);camera.transform.position=-camera.transform.forward*25;
        var lightObject=new GameObject("P2 art light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,scene);
        var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(45,150,0);
        int index=0;
        foreach(string key in new[]{"Pusher","Sniper","Bruiser","Crusher","Twin_blasters","Scatter_shot","Rapid_smg"})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+key+".prefab");
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            var bounds=obj.GetComponent<Renderer>().bounds;obj.transform.localScale*=1.7f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
            bounds=obj.GetComponent<Renderer>().bounds;var cell=camera.transform.right*((index%3-1)*4.3f)+camera.transform.up*((1-index/3)*3.4f);
            obj.transform.position+=cell-bounds.center;
            var labelObject=new GameObject("P2 model label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(labelObject,scene);
            var label=labelObject.AddComponent<TextMesh>();label.text=key.Replace('_',' ');label.fontSize=64;label.characterSize=.035f;label.anchor=TextAnchor.MiddleCenter;label.color=Color.white;
            label.transform.position=cell+camera.transform.up*1.3f;label.transform.rotation=camera.transform.rotation;index++;
        }
        var target=new RenderTexture(1800,1350,24);camera.targetTexture=target;camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(1800,1350,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1800,1350),0,0);texture.Apply();File.WriteAllBytes("/tmp/cube-p2-catalog.png",texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);EditorSceneManager.ClosePreviewScene(scene);
        Debug.Log("CUBE_P2_CATALOG_OK");
    }
    static WeaponSpec[] Weapons()=>new[] {
        new WeaponSpec {title="Pulse Pistol",description="Balanced single shot. Reliable at long range.",kind=ContentWeapon.PulsePistol,model="Pulse pistol",damage=6,interval=.5f,pellets=1,color=new Color(.1f,.8f,1)},
        new WeaponSpec {title="Twin Blasters",description="Twin barrels fire together. Heavy damage at mid range.",kind=ContentWeapon.TwinBlasters,model="Twin blasters",damage=3.5f,interval=.48f,pellets=2,spread=3,color=new Color(.45f,.5f,1)},
        new WeaponSpec {title="Scatter Shot",description="Five pellets, wide spread. Deadly up close on one target.",kind=ContentWeapon.ScatterShot,model="Scatter shot",damage=3,interval=.9f,pellets=5,spread=28,projectileLife=.32f,color=new Color(1,.63f,.18f)},
        new WeaponSpec {title="Rapid SMG",description="Small rounds, high fire rate. Keep moving, keep the pressure.",kind=ContentWeapon.RapidSmg,model="Rapid smg",damage=2,interval=.14f,pellets=1,color=new Color(.25f,1,.62f)}
    };
    static EncounterSpec Wave(bool cover,bool barrels,params ContentEnemy[] enemies)=>new EncounterSpec {cover=cover,barrels=barrels,enemies=enemies};
    static MissionSpec[] Missions()
    {
        var g=ContentEnemy.Grunt;var r=ContentEnemy.Sprinter;var a=ContentEnemy.Gunner;var p=ContentEnemy.Pusher;var s=ContentEnemy.Sniper;var b=ContentEnemy.Bruiser;var boss=ContentEnemy.Crusher;
        return new[] {
            new MissionSpec {title="First Contact",description="Clear the invaders. Manage your health and power between gates.",accent=new Color(.1f,.75f,.85f),cubes=new[]{Wave(false,false,g,g,g),Wave(false,false,g,g,r),Wave(false,false,g,r,r)}},
            new MissionSpec {title="Neon Yard",description="Read the gunners' bursts. Cover blocks enemy shots.",accent=new Color(.3f,.55f,1),cubes=new[]{Wave(true,false,g,g,a),Wave(false,true,g,r,r),Wave(true,true,g,g,a,a),Wave(true,false,r,r,a)}},
            new MissionSpec {title="Push Works",description="Get out of the pusher's orange cone; don't linger at the edges.",accent=new Color(1,.56f,.2f),cubes=new[]{Wave(false,false,p,g,g),Wave(true,false,p,r,r),Wave(false,true,p,p,g),Wave(true,true,p,a,r)}},
            new MissionSpec {title="Long Range",description="Dodge sideways when the laser line locks. Keep clear of crusher slams.",accent=new Color(.7f,.35f,1),cubes=new[]{Wave(true,false,s,g,g),Wave(false,true,b,r),Wave(true,false,s,s,a),Wave(false,true,b,p,r),Wave(true,true,s,b,g)}},
            new MissionSpec {title="Last Line",description="Mixed fights with up to three threats. Every gate leads to the same battle stage.",accent=new Color(.95f,.28f,.45f),cubes=new[]{Wave(true,true,a,a,r,r),Wave(false,true,b,p,g),Wave(true,false,s,a,g,g),Wave(false,true,b,r,r,p),Wave(true,true,b,s,p)}},
            new MissionSpec {title="Gold Rush",description="Killing targets isn't enough. Hit each cube's gold quota before time runs out.",mode=MissionMode.GoldRush,accent=new Color(1,.72f,.2f),cubes=new[]{new EncounterSpec {goldQuota=40,enemies=new[]{g,g}},new EncounterSpec {goldQuota=50,enemies=new[]{r,r,g}},new EncounterSpec {goldQuota=60,enemies=new[]{r,a},cover=true}}},
            new MissionSpec {title="The Crusher",description="Armor, weapon system, core: three separate 20s phases. Total boss health never resets at gates.",mode=MissionMode.Boss,accent=new Color(1,.32f,.16f),cubes=new[]{Wave(false,false,boss,g),Wave(true,false,boss,r,r),Wave(false,true,boss,a,g)}}
        };
    }
    static Material Paint(string name,Color color,bool glow=false)
    {
        string path=Root+"/Models/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material)return material;
        material=new Material(Shader.Find("Standard"));material.color=color;material.SetFloat("_Metallic",.2f);material.SetFloat("_Glossiness",.35f);
        if(glow) { material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*.7f); }
        AssetDatabase.CreateAsset(material,path);return material;
    }
    static void BuildModels(ToyArtLibrary library)
    {
        var entries=new List<ToyArtLibrary.Model>(library.models);
        var budget=new System.Text.StringBuilder("Model,Vertices,Triangles,RigParts\n");
        foreach(string key in new[]{"Pusher","Sniper","Bruiser","Crusher","Twin blasters","Scatter shot","Rapid smg"})
        {
            if(entries.Exists(item=>item.key==key))continue;
            var builder=Geometry(key);var mesh=builder.Build(key);var parts=builder.BuildParts(key);var rig=new List<ToyArtLibrary.RigPart>();
            string name=key.Replace(' ','_');AssetDatabase.CreateAsset(mesh,Root+"/Models/"+name+".asset");
            foreach(var part in parts)
            { AssetDatabase.CreateAsset(part.mesh,Root+"/Models/"+name+"_"+part.name+".asset");rig.Add(new ToyArtLibrary.RigPart {name=part.name,mesh=part.mesh,pivot=part.pivot}); }
            Color tint=key=="Pusher"?new Color(1,.5f,.15f):key=="Sniper"?new Color(.7f,.3f,1):key=="Bruiser"?new Color(.85f,.22f,.35f):key=="Crusher"?new Color(.95f,.35f,.12f):new Color(.2f,.4f,.85f);
            var model=new ToyArtLibrary.Model {key=key,mesh=mesh,rig=rig.ToArray(),trim=Paint("Graphite",new Color(.06f,.09f,.14f)),accent=Paint("Light_"+name,tint,true)};
            entries.Add(model);
            var prefab=new GameObject(key,typeof(MeshFilter),typeof(MeshRenderer));prefab.GetComponent<MeshFilter>().sharedMesh=mesh;prefab.GetComponent<MeshRenderer>().sharedMaterials=new[]{Paint("Paint_"+name,tint),model.trim,model.accent};
            if(rig.Count>0)prefab.AddComponent<ToyActorAnimator>().ConfigurePrefab(key);
            PrefabUtility.SaveAsPrefabAsset(prefab,Root+"/Models/"+name+".prefab");Object.DestroyImmediate(prefab);
            budget.AppendLine($"{key},{mesh.vertexCount},{mesh.triangles.Length/3},{rig.Count}");
        }
        library.models=entries.ToArray();EditorUtility.SetDirty(library);
        if(budget.ToString().Split('\n').Length>2)File.WriteAllText(Root+"/MeshBudget.csv",budget.ToString());
    }
    static ToyMeshBuilder Geometry(string key)
    {
        var b=new ToyMeshBuilder();bool actor=key=="Pusher"||key=="Sniper"||key=="Bruiser"||key=="Crusher";
        if(actor)
        {
            bool boss=key=="Crusher",heavy=key=="Bruiser";float width=boss?2.1f:heavy?1.25f:.9f;
            b.Part("Core",Vector3.zero);
            b.Box(new Vector3(0,.1f,0),new Vector3(width,boss?1.25f:.8f,boss?1.5f:.8f),.13f);
            b.Box(new Vector3(0,.58f,.08f),new Vector3(width*.7f,.35f,.65f),.14f,1);
            b.Box(new Vector3(0,.59f,.42f),new Vector3(width*.45f,.1f,.07f),.08f,2);
            if(key=="Sniper")
            {
                b.Box(new Vector3(.28f,.14f,.9f),new Vector3(.18f,.2f,1.4f),.08f,1);
                b.Box(new Vector3(.28f,.3f,.4f),new Vector3(.15f,.15f,.3f),.08f,2);
                b.Box(new Vector3(-.28f,.95f,-.18f),new Vector3(.07f,.65f,.07f),.06f,1);
                b.Drum(new Vector3(-.28f,1.28f,-.18f),.13f,.1f,2);
            }
            if(boss)
            {
                b.Drum(new Vector3(0,.24f,.79f),.42f,.25f,2);
                for(int x=-1;x<=1;x+=2)b.Box(new Vector3(x*1.1f,.32f,-.3f),new Vector3(.45f,.8f,1.2f),.12f,1);
            }
            for(int side=-1;side<=1;side+=2)
            {
                b.Part(side<0?"LeftLeg":"RightLeg",new Vector3(side*width*.32f,-.3f,0));
                b.Box(new Vector3(side*width*.32f,-.53f,.08f),new Vector3(width*.29f,.45f,boss?1.35f:.65f),.12f,1);
                b.Box(new Vector3(side*width*.32f,-.7f,.18f),new Vector3(width*.32f,.15f,boss?1.45f:.8f),.12f);
                b.Part(side<0?"LeftArm":"RightArm",new Vector3(side*width*.58f,.25f,0));
                b.Box(new Vector3(side*width*.58f,-.06f,.08f),new Vector3(width*.25f,.65f,.55f),.12f);
                if(key=="Pusher")
                {
                    b.Box(new Vector3(side*.44f,.02f,.52f),new Vector3(.67f,1.05f,.27f),.12f,1);
                    b.Box(new Vector3(side*.44f,.02f,.67f),new Vector3(.43f,.13f,.07f),.08f,2);
                }
                if(heavy||boss)
                {
                    b.Box(new Vector3(side*width*.68f,-.28f,.33f),new Vector3(width*.5f,.53f,.65f),.12f,1);
                    b.Box(new Vector3(side*width*.68f,-.28f,.67f),new Vector3(width*.35f,.27f,.08f),.08f,2);
                }
            }
        }
        else
        {
            b.Box(new Vector3(0,0,-.08f),new Vector3(1.1f,.8f,.7f),.1f);
            b.Box(new Vector3(0,-.55f,-.2f),new Vector3(.5f,.6f,.3f),.1f,1);
            if(key=="Twin blasters")for(int side=-1;side<=1;side+=2)
            { b.Box(new Vector3(side*.31f,.03f,.5f),new Vector3(.4f,.5f,.9f),.1f,1);b.Box(new Vector3(side*.31f,.03f,.97f),new Vector3(.23f,.28f,.08f),.08f,2); }
            if(key=="Scatter shot")
            { b.Box(new Vector3(0,.04f,.45f),new Vector3(1.3f,.55f,.85f),.1f,1);b.Box(new Vector3(0,.04f,.9f),new Vector3(1,.32f,.08f),.08f,2);b.Drum(new Vector3(0,-.45f,.3f),.45f,.5f,1); }
            if(key=="Rapid smg")
            { b.Box(new Vector3(0,.03f,.45f),new Vector3(.5f,.45f,.8f),.1f,1);b.Box(new Vector3(0,.03f,.87f),new Vector3(.3f,.25f,.06f),.08f,2);b.Box(new Vector3(0,-.55f,.12f),new Vector3(.38f,.8f,.3f),.1f,1); }
        }
        return b;
    }
    static P2AudioBank BuildAudio()
    {
        string path=Root+"/Audio/P2AudioBank.asset";var bank=AssetDatabase.LoadAssetAtPath<P2AudioBank>(path);if(bank)return bank;
        // Recorded CC0 clips only (Playgama 3.3.2); CubeAudioLibrary fills the bank.
        bank=ScriptableObject.CreateInstance<P2AudioBank>();AssetDatabase.CreateAsset(bank,path);
        if(AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset"))CubeAudioLibrary.Install();
        return bank;
    }
}
