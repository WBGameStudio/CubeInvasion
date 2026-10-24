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
        new WeaponSpec {title="Pulse Pistol",description="Dengeli tek atış. Uzak hedeflerde güvenilir.",kind=ContentWeapon.PulsePistol,model="Pulse pistol",damage=6,interval=.5f,pellets=1,color=new Color(.1f,.8f,1)},
        new WeaponSpec {title="Twin Blasters",description="İki namludan birlikte ateş. Orta mesafede yoğun hasar.",kind=ContentWeapon.TwinBlasters,model="Twin blasters",damage=3.5f,interval=.48f,pellets=2,spread=3,color=new Color(.45f,.5f,1)},
        new WeaponSpec {title="Scatter Shot",description="Beş saçma, geniş açı. Yaklaşınca tek hedefte de etkili.",kind=ContentWeapon.ScatterShot,model="Scatter shot",damage=3,interval=.9f,pellets=5,spread=28,projectileLife=.32f,color=new Color(1,.63f,.18f)},
        new WeaponSpec {title="Rapid SMG",description="Küçük mermiler, hızlı atış. Hareket ederek baskıyı sürdür.",kind=ContentWeapon.RapidSmg,model="Rapid smg",damage=2,interval=.14f,pellets=1,color=new Color(.25f,1,.62f)}
    };
    static EncounterSpec Wave(bool cover,bool barrels,params ContentEnemy[] enemies)=>new EncounterSpec {cover=cover,barrels=barrels,enemies=enemies};
    static MissionSpec[] Missions()
    {
        var g=ContentEnemy.Grunt;var r=ContentEnemy.Sprinter;var a=ContentEnemy.Gunner;var p=ContentEnemy.Pusher;var s=ContentEnemy.Sniper;var b=ContentEnemy.Bruiser;var boss=ContentEnemy.Crusher;
        return new[] {
            new MissionSpec {title="İlk Temas",description="İstilacıları temizle. Kapılar arasında canını ve gücünü yönet.",accent=new Color(.1f,.75f,.85f),cubes=new[]{Wave(false,false,g,g,g),Wave(false,false,g,g,r),Wave(false,false,g,r,r)}},
            new MissionSpec {title="Neon Yard",description="Atıcıların serilerini oku. Siperler düşman mermilerini keser.",accent=new Color(.3f,.55f,1),cubes=new[]{Wave(true,false,g,g,a),Wave(false,true,g,r,r),Wave(true,true,g,g,a,a),Wave(true,false,r,r,a)}},
            new MissionSpec {title="İtki Tesisi",description="İticinin turuncu konisinden çık; kenarlarda oyalanma.",accent=new Color(1,.56f,.2f),cubes=new[]{Wave(false,false,p,g,g),Wave(true,false,p,r,r),Wave(false,true,p,p,g),Wave(true,true,p,a,r)}},
            new MissionSpec {title="Uzun Menzil",description="Lazer çizgisi sabitlenince yana kaç. Ezicilerin darbesine yaklaşma.",accent=new Color(.7f,.35f,1),cubes=new[]{Wave(true,false,s,g,g),Wave(false,true,b,r),Wave(true,false,s,s,a),Wave(false,true,b,p,r),Wave(true,true,s,b,g)}},
            new MissionSpec {title="Son Hat",description="Üç tehdide kadar karışık karşılaşmalar. Her kapı aynı savaş aşamasına çıkar.",accent=new Color(.95f,.28f,.45f),cubes=new[]{Wave(true,true,a,a,r,r),Wave(false,true,b,p,g),Wave(true,false,s,a,g,g),Wave(false,true,b,r,r,p),Wave(true,true,b,s,p)}},
            new MissionSpec {title="Gold Rush",description="Hedefleri öldürmek yetmez. Her küpün altın kotasını süre dolmadan topla.",mode=MissionMode.GoldRush,accent=new Color(1,.72f,.2f),cubes=new[]{new EncounterSpec {goldQuota=40,enemies=new[]{g,g}},new EncounterSpec {goldQuota=50,enemies=new[]{r,r,g}},new EncounterSpec {goldQuota=60,enemies=new[]{r,a},cover=true}}},
            new MissionSpec {title="The Crusher",description="Zırh, silah sistemi, çekirdek: üç ayrı 20 saniyelik faz. Toplam boss canı kapılarda sıfırlanmaz.",mode=MissionMode.Boss,accent=new Color(1,.32f,.16f),cubes=new[]{Wave(false,false,boss,g),Wave(true,false,boss,r,r),Wave(false,true,boss,a,g)}}
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
        var entries=new List<P2AudioBank.Entry>();
        foreach(CombatFeedback.Cue cue in Enum.GetValues(typeof(CombatFeedback.Cue)))
        {
            var variants=new AudioClip[2];
            for(int v=0;v<2;v++)
            {
                string clipPath=Root+"/Audio/"+cue+"_"+v+".wav";
                WriteWave(clipPath,Sound(cue,v));AssetDatabase.ImportAsset(clipPath);
                var importer=(AudioImporter)AssetImporter.GetAtPath(clipPath);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
                variants[v]=AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            }
            entries.Add(new P2AudioBank.Entry {cue=cue,variants=variants});
        }
        bank=ScriptableObject.CreateInstance<P2AudioBank>();bank.sounds=entries.ToArray();
        for(int mode=0;mode<3;mode++)
        {
            string clipPath=Root+"/Audio/Loop_"+mode+".wav";WriteWave(clipPath,Music(mode));AssetDatabase.ImportAsset(clipPath);
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);if(mode==0)bank.combatLoop=clip;else if(mode==1)bank.bossLoop=clip;else bank.goldLoop=clip;
        }
        AssetDatabase.CreateAsset(bank,path);return bank;
    }
    static float[] Sound(CombatFeedback.Cue cue,int variant)
    {
        const int rate=22050;float duration=cue==CombatFeedback.Cue.BossPhase?.7f:cue==CombatFeedback.Cue.Telegraph?.35f:cue==CombatFeedback.Cue.Clear?.42f:cue==CombatFeedback.Cue.Fail?.6f:cue==CombatFeedback.Cue.BossSlam||cue==CombatFeedback.Cue.Explosion?.35f:.14f;
        var data=new float[(int)(rate*duration)];uint state=(uint)(123+(int)cue*97+variant);float phase=0;
        float basePitch=cue==CombatFeedback.Cue.Shot?760:cue==CombatFeedback.Cue.Twin?580:cue==CombatFeedback.Cue.Scatter?140:cue==CombatFeedback.Cue.Rapid?1000:cue==CombatFeedback.Cue.Gold?1150:cue==CombatFeedback.Cue.Heal?660:cue==CombatFeedback.Cue.Alarm?360:cue==CombatFeedback.Cue.Telegraph?520:cue==CombatFeedback.Cue.BossSlam?90:cue==CombatFeedback.Cue.BossPhase?165:cue==CombatFeedback.Cue.Fail?180:440;
        for(int i=0;i<data.Length;i++)
        {
            float t=i/(float)rate,u=t/duration;state=state*1664525+1013904223;float noise=((state>>16)/65535f)*2-1;
            bool notes=cue==CombatFeedback.Cue.Clear||cue==CombatFeedback.Cue.Heal||cue==CombatFeedback.Cue.Gate||cue==CombatFeedback.Cue.Gold;
            float pitch=notes?Mathf.Pow(2,Mathf.Floor(u*3)*4/12):cue==CombatFeedback.Cue.Telegraph?1+u*.8f:1-u*.65f;
            phase+=2*Mathf.PI*basePitch*(variant==0?1:1.055f)*pitch/rate;
            float wave=Mathf.Sin(phase)+Mathf.Sin(phase*2.01f)*.18f;
            if(cue==CombatFeedback.Cue.Explosion||cue==CombatFeedback.Cue.Scatter||cue==CombatFeedback.Cue.Hit||cue==CombatFeedback.Cue.BossSlam)wave=wave*.5f+noise*.55f;
            data[i]=Mathf.Clamp(wave*Mathf.Min(1,u*40)*Mathf.Pow(1-u,2)*.42f,-.9f,.9f);
        }
        return data;
    }
    static float[] Music(int mode)
    {
        const int rate=22050;float beat=60f/112;int length=Mathf.RoundToInt(beat*16*rate);var data=new float[length];
        int[] notes=mode==1?new[]{0,0,3,1,0,0,6,3}:mode==2?new[]{0,7,12,7,3,10,15,10}:new[]{0,0,7,10,3,3,7,5};
        uint noise=71;
        for(int i=0;i<length;i++)
        {
            float time=i/(float)rate,step=time/(beat*.5f),local=(step-Mathf.Floor(step))*beat*.5f;
            int index=(int)step%notes.Length;float hz=82.4069f*Mathf.Pow(2,notes[index]/12f);
            float bass=(Mathf.Sin(2*Mathf.PI*hz*local)+.18f*Mathf.Sin(4*Mathf.PI*hz*local))*Mathf.Exp(-local*15)*.23f;
            float kickTime=time%beat;float kick=Mathf.Sin(2*Mathf.PI*(55*kickTime+3*(1-Mathf.Exp(-kickTime*30))))*Mathf.Exp(-kickTime*20)*.28f;
            noise=noise*1664525+1013904223;float hat=(((noise>>16)/65535f)*2-1)*Mathf.Exp(-local*95)*.06f;
            float melody=mode==2?Mathf.Sin(2*Mathf.PI*hz*8*local)*Mathf.Exp(-local*19)*.1f:0;
            data[i]=(bass+kick+hat+melody)*Mathf.Min(1,i/100f)*Mathf.Min(1,(length-i)/100f);
        }
        return data;
    }
    static void WriteWave(string path,float[] samples)
    {
        using(var writer=new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Length*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(22050);writer.Write(44100);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(samples.Length*2);foreach(float sample in samples)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
        }
    }
}
