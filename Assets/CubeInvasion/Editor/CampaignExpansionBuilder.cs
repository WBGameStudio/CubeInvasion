using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;
using Object=UnityEngine.Object;
public static class CampaignExpansionBuilder
{
    const string Root="Assets/CubeInvasion/Art/Expansion";
    [MenuItem("Cube Invasion/Full Campaign/Install")]
    public static void Install()
    {
        Directory.CreateDirectory(Root);AssetDatabase.Refresh();
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        if(!presentation)
        {
            presentation=ScriptableObject.CreateInstance<CampaignPresentation>();var library=ScriptableObject.CreateInstance<ToyArtLibrary>();var models=new List<ToyArtLibrary.Model>();
            var baseArt=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");baseArt.TryGet("Grunt",out var sample);var material=sample.surface;
            for(int i=4;i<12;i++)
            {
                var b=Weapon(i);var mesh=Save(((ContentWeapon)i).ToString(),b.Build(((ContentWeapon)i).ToString()));models.Add(new ToyArtLibrary.Model {key="Expansion "+(ContentWeapon)i,mesh=mesh,surface=material,rig=Array.Empty<ToyArtLibrary.RigPart>()});
            }
            for(int i=0;i<8;i++)
            {
                string name=i<3?new[]{"Summoner","Phaser","Elite"}[i]:((BossIdentity)(i-2)).ToString();var b=Actor(i);var rig=b.BuildRig(name,Save);var mesh=Save(name,b.Build(name));models.Add(new ToyArtLibrary.Model {key=name,mesh=mesh,rig=rig,surface=material,mechanicalRig=true,integratedFace=true});
            }
            library.models=models.ToArray();AssetDatabase.CreateAsset(library,Root+"/ExpansionArt.asset");presentation.art=library;
            presentation.weapons=new Sprite[8];for(int i=0;i<8;i++)presentation.weapons[i]=Thumbnail(models[i].mesh,material,i);
            presentation.biomeMusic=CampaignAudioBuilder.Build(Root+"/Audio");
            AssetDatabase.CreateAsset(presentation,"Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        }
        if(!presentation.art.TryGet("Energy node",out _))
        {
            var node=new VoidToyMeshBuilder();node.Box(new Vector3(0,-.2f,0),new Vector3(.7f,.15f,.7f),VoidToyPalette.Graphite);node.Box(new Vector3(0,.2f,0),new Vector3(.4f,.5f,.4f),VoidToyMeshBuilder.Glow(VoidToyPalette.Cyan),.2f,Quaternion.Euler(0,45,0));
            var entries=new List<ToyArtLibrary.Model>(presentation.art.models);entries.Add(new ToyArtLibrary.Model {key="Energy node",mesh=Save("Energy node",node.Build("Energy node")),surface=presentation.art.models[0].surface,rig=Array.Empty<ToyArtLibrary.RigPart>()});presentation.art.models=entries.ToArray();presentation.art.InvalidateLookup();EditorUtility.SetDirty(presentation.art);
        }
        foreach(string key in new[]{"Orbital drone","Damage amplifier","Magnet core","Frozen patch"})
        {
            if(presentation.art.TryGet(key,out _))continue;var prop=new VoidToyMeshBuilder();var blue=VoidToyPalette.Cobalt;var glow=VoidToyMeshBuilder.Glow(VoidToyPalette.Cyan);
            if(key=="Orbital drone") {prop.Box(Vector3.zero,new Vector3(.5f,.24f,.6f),blue);for(int side=-1;side<=1;side+=2)prop.Box(new Vector3(side*.42f,0,0),new Vector3(.35f,.08f,.6f),VoidToyPalette.Graphite);prop.Box(new Vector3(0,0,.35f),new Vector3(.32f,.1f,.1f),glow);}
            else if(key=="Magnet core") {for(int side=-1;side<=1;side+=2) {prop.Box(new Vector3(side*.3f,0,0),new Vector3(.22f,.55f,.25f),blue);prop.Box(new Vector3(side*.3f,.3f,0),new Vector3(.22f,.12f,.25f),glow);}prop.Box(new Vector3(0,-.25f,0),new Vector3(.8f,.2f,.25f),blue);}
            else if(key=="Damage amplifier") {prop.Box(Vector3.zero,Vector3.one*.65f,VoidToyPalette.Graphite,.2f);prop.Box(new Vector3(0,.15f,.3f),Vector3.one*.38f,VoidToyMeshBuilder.Glow(VoidToyPalette.Warm),.2f,Quaternion.Euler(0,0,45));}
            else prop.Box(Vector3.zero,Vector3.one,new Color(.3f,.65f,.85f),.12f);
            var entries=new List<ToyArtLibrary.Model>(presentation.art.models);entries.Add(new ToyArtLibrary.Model {key=key,mesh=Save(key,prop.Build(key)),surface=presentation.art.models[0].surface,rig=Array.Empty<ToyArtLibrary.RigPart>()});presentation.art.models=entries.ToArray();presentation.art.InvalidateLookup();EditorUtility.SetDirty(presentation.art);
        }
        for(int i=0;i<8;i++)
        {
            string iconPath=Root+"/Weapon_"+i+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(iconPath);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();
            presentation.weapons[i]=AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);if(!presentation.weapons[i])throw new Exception("Missing weapon sprite "+i);
        }
        EditorUtility.SetDirty(presentation);AssetDatabase.SaveAssets();
        const string settingsPath="Assets/CubeInvasion/Resources/CampaignExpansionSettings.asset";
        var settings=AssetDatabase.LoadAssetAtPath<CampaignExpansionSettings>(settingsPath);if(!settings) {settings=ScriptableObject.CreateInstance<CampaignExpansionSettings>();AssetDatabase.CreateAsset(settings,settingsPath);}
        const string contentPath="Assets/CubeInvasion/Resources/CampaignContent.asset";
        var catalog=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>(contentPath);
        if(!catalog) {catalog=CampaignExpansionSettings.Expand(AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset"),settings.seed);catalog.name="Campaign Content";AssetDatabase.CreateAsset(catalog,contentPath);}
        settings.catalog=catalog;EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");settings=AssetDatabase.LoadAssetAtPath<CampaignExpansionSettings>(settingsPath);var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);data.FindProperty("campaignExpansion").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();Debug.Log("CUBE_CAMPAIGN_INSTALL_OK: 60 levels / 12 weapons / 8 new actor rigs / 8 weapon meshes / 6 music loops");
    }
    public static void RefreshWeaponIcons()
    {
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        for(int i=0;i<8;i++)presentation.weapons[i]=Thumbnail(presentation.art.models[i].mesh,presentation.art.models[i].surface,i);
        EditorUtility.SetDirty(presentation);AssetDatabase.SaveAssets();Debug.Log("CUBE_CAMPAIGN_ICONS_OK");
    }
    public static void RefreshActors()
    {
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>("Assets/CubeInvasion/Resources/CampaignPresentation.asset");
        for(int i=0;i<8;i++)
        {
            var model=presentation.art.models[i+8];var builder=Actor(i);model.rig=builder.BuildRig(model.key,SaveUpdated);model.mesh=SaveUpdated(model.key,builder.Build(model.key));
        }
        presentation.art.InvalidateLookup();EditorUtility.SetDirty(presentation.art);AssetDatabase.SaveAssets();Debug.Log("CUBE_EXPANSION_ACTORS_OK");
    }
    static Mesh SaveUpdated(string name,Mesh mesh)
    {var old=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/"+name+".asset");if(!old)return Save(name,mesh);EditorUtility.CopySerialized(mesh,old);EditorUtility.SetDirty(old);Object.DestroyImmediate(mesh);return old;}
    static Mesh Save(string name,Mesh mesh) {var path=Root+"/"+name+".asset";AssetDatabase.CreateAsset(mesh,path);return mesh;}
    static VoidToyMeshBuilder Weapon(int index)
    {
        var b=new VoidToyMeshBuilder {Scale=new Vector3(.2f,.2f,.65f)};var color=CampaignExpansionSettings.Colors[(index-4)%6];var dark=VoidToyPalette.Graphite;
        b.Box(new Vector3(0,0,0),new Vector3(.2f,.18f,.5f),dark);b.Box(new Vector3(0,-.13f,-.1f),new Vector3(.12f,.23f,.14f),dark);
        int barrels=index==5?2:index==6?3:index==11?4:1;
        for(int i=0;i<barrels;i++)
        {float x=(i-(barrels-1)*.5f)*.13f;float length=index==4?.7f:index==7?.38f:.48f;b.Box(new Vector3(x,.02f,.28f),new Vector3(.09f,index==7?.18f:.09f,length),dark);b.Box(new Vector3(x,.02f,.3f+length*.5f),new Vector3(.07f,.07f,.05f),VoidToyMeshBuilder.Glow(color));}
        for(int i=0;i<3;i++)b.Box(new Vector3(0,.12f,-.1f+i*.12f),new Vector3(.23f,.05f,.045f),VoidToyMeshBuilder.Glow(color));
        if(index==4)b.Box(new Vector3(0,.2f,.05f),new Vector3(.09f,.09f,.25f),dark);
        if(index==6||index==9)b.Box(new Vector3(0,.18f,.05f),new Vector3(.15f,.2f,.15f),VoidToyMeshBuilder.Glow(color),.3f);
        if(index==10)for(int i=-1;i<=1;i+=2)b.Box(new Vector3(i*.18f,.02f,.12f),new Vector3(.1f,.07f,.32f),color);
        return b;
    }
    static VoidToyMeshBuilder Actor(int index)
    {
        bool boss=index>=3;float size=boss?1.35f:.75f;var b=new VoidToyMeshBuilder {Scale=Vector3.one*.7f,Offset=new Vector3(0,-.5f,0)};var dark=VoidToyPalette.Graphite;var color=index>=3?CampaignExpansionSettings.Colors[index-2]:index==2?VoidToyPalette.Gold:CampaignExpansionSettings.Colors[index==1?2:0];
        b.PartAt("Core",Vector3.zero);b.Box(new Vector3(0,.35f,0),new Vector3(size,.5f,size*.65f),dark,.2f);
        b.Box(new Vector3(0,.45f,size*.35f),new Vector3(size*.65f,.12f,.08f),VoidToyMeshBuilder.Glow(color));
        int horns=index==0?3:index==1?1:index==4?3:2;
        for(int i=0;i<horns;i++)b.Box(new Vector3((i-(horns-1)*.5f)*.3f,.85f,0),new Vector3(.13f,.4f,.13f),color);
        if(index==4)for(int i=-1;i<=1;i++) {b.Box(new Vector3(i*.55f,1,.1f),new Vector3(.42f,.35f,.4f),dark);b.Box(new Vector3(i*.55f,1,.33f),new Vector3(.3f,.08f,.07f),VoidToyMeshBuilder.Glow(color));}
        if(index==3) {b.Box(new Vector3(0,.95f,-.2f),new Vector3(.38f,.55f,.38f),dark);for(int i=0;i<3;i++)b.Box(new Vector3(0,.75f+i*.12f,.03f),new Vector3(.25f,.05f,.05f),VoidToyMeshBuilder.Glow(color));}
        if(index==6)for(int side=-1;side<=1;side+=2)b.Box(new Vector3(side*.85f,.55f,0),new Vector3(.6f,.45f,.6f),dark,.3f);
        if(index==7)b.Box(new Vector3(0,1,.1f),Vector3.one*.35f,VoidToyMeshBuilder.Glow(color),.3f,Quaternion.Euler(0,45,45));
        foreach(int side in new[]{-1,1})
        {
            string prefix=side<0?"Left":"Right";b.PartAt(prefix+"Arm",new Vector3(side*size*.6f,.45f,0));b.Box(new Vector3(side*size*.6f,.25f,0),new Vector3(.2f,.5f,.3f),dark);b.Box(new Vector3(side*size*.6f,.2f,.22f),new Vector3(.15f,.16f,.3f),color);
            b.PartAt(prefix+"Leg",new Vector3(side*size*.26f,.1f,0));b.Box(new Vector3(side*size*.26f,-.08f,.06f),new Vector3(.22f,.3f,.32f),dark);
        }
        return b;
    }
    static Sprite Thumbnail(Mesh mesh,Material material,int index)
    {
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var model=new GameObject("Weapon thumbnail",typeof(MeshFilter),typeof(MeshRenderer));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model,scene);model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterial=material;model.transform.localScale=new Vector3(1,1,3.25f);
            var bounds=model.GetComponent<MeshRenderer>().bounds;var cameraObject=new GameObject("Thumbnail camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.backgroundColor=Color.clear;camera.clearFlags=CameraClearFlags.SolidColor;camera.orthographic=true;camera.allowHDR=false;camera.aspect=1;camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.transform.rotation=Quaternion.Euler(30,215,0);
            Vector2 min=Vector2.one*float.MaxValue,max=-min;
            for(int c=0;c<8;c++) {var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1));var local=Quaternion.Inverse(camera.transform.rotation)*(corner-bounds.center);min=Vector2.Min(min,local);max=Vector2.Max(max,local);}
            camera.orthographicSize=Mathf.Max((max-min).x,(max-min).y)*.54f;camera.transform.position=bounds.center+camera.transform.rotation*new Vector3((min.x+max.x)*.5f,(min.y+max.y)*.5f,-20);
            var target=new RenderTexture(384,384,24,RenderTextureFormat.ARGB32);var old=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;var texture=new Texture2D(384,384,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,384,384),0,0);texture.Apply();string path=Root+"/Weapon_"+index+".png";File.WriteAllBytes(path,texture.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
    }
}
