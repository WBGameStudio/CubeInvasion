using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;
using Object=UnityEngine.Object;

public static class PrismArtBuilder
{
    public const string Root="Assets/CubeInvasion/Art/Prism";
    static readonly string[] Actors={"Armored body","Grunt","Sprinter","Gunner","Pusher","Sniper","Bruiser","Crusher"};
    static readonly string[] Weapons={"Pulse pistol","Twin blasters","Scatter shot","Rapid smg"};
    static readonly string[] Props={"Visor","Gunner barrel","Threat crest","Deep sides","Arena surface","Low cover","Explosive barrel","Explosive band","Health +20","Gold pickup","Gate left post","Gate right post","Gate header","Locked until clear","Right bridge","Forward bridge","Edge light","Reward beacon","Reward beacon vertical","Health cross H","Health cross V"};
    public static string Slug(string key)=>key.Replace(' ','_').Replace("+","Plus");
    [MenuItem("Cube Invasion/Art/Build Prism Vanguard")]
    public static void BuildAssets()
    {
        foreach(string folder in new[]{"Meshes","Materials","Prefabs","Thumbnails"})Directory.CreateDirectory(Root+"/"+folder);AssetDatabase.Refresh();
        var shader=Shader.Find("CubeInvasion/Prism Ceramic");if(!shader)throw new InvalidOperationException("Prism Ceramic shader did not import.");
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");
        if(!library)throw new InvalidOperationException("Base art library is missing.");
        var entries=new List<ToyArtLibrary.Model>(library.models);var csv=new StringBuilder("Model,Vertices,Triangles,LOD1Triangles,RigParts,DrawsPerVisiblePose\n");
        foreach(string key in Actors.Concat(Weapons).Concat(Props).Distinct())
        {
            string name=Slug(key);var builder=PrismArtGeometry.Make(key);var mesh=SaveMesh(name,builder.Build(name));
            var rig=builder.BuildRig(name,SaveMesh);
            Mesh low=null;
            if(PrismArtGeometry.Actor(key)||PrismArtGeometry.Weapon(key))
            {
                var lowBuilder=PrismArtGeometry.Make(key,.55f);low=SaveMesh(name+"_LOD1",lowBuilder.Build(name+"_LOD1"));
                var lowRig=lowBuilder.BuildRig(name+"_LOD1",SaveMesh);
                foreach(var part in rig)part.lodMesh=lowRig.First(p=>p.name==part.name).mesh;
            }
            string path=Root+"/Materials/"+name+".mat";var paint=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!paint) {paint=new Material(shader);AssetDatabase.CreateAsset(paint,path);}paint.shader=shader;paint.color=PrismArtGeometry.Tint(key);paint.SetFloat("_Glossiness",.48f);paint.enableInstancing=true;EditorUtility.SetDirty(paint);
            var model=new ToyArtLibrary.Model {key=key,mesh=mesh,lodMesh=low,rig=rig,surface=paint,trim=paint,accent=paint,integratedFace=PrismArtGeometry.Actor(key),mechanicalRig=PrismArtGeometry.Actor(key),useRuntimeTint=!PrismArtGeometry.Actor(key)&&!PrismArtGeometry.Weapon(key)&&key!="Arena surface"&&key!="Deep sides"};
            entries.RemoveAll(m=>m.key==key);entries.Add(model);
            csv.AppendLine($"{key},{mesh.vertexCount},{mesh.triangles.Length/3},{(low?low.triangles.Length/3:0)},{rig.Length},{Mathf.Max(1,rig.Length)}");
            var prefab=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));prefab.GetComponent<MeshFilter>().sharedMesh=mesh;prefab.GetComponent<MeshRenderer>().sharedMaterial=paint;
            if(PrismArtGeometry.Actor(key))
            {
                prefab.transform.localScale=key=="Armored body"?new Vector3(.65f,.55f,.65f):key=="Crusher"?Vector3.one*1.05f:Vector3.one*.7f;
                prefab.AddComponent<ToyActorAnimator>().ConfigurePrefab(key);
            }
            if(key=="Armored body")
            {
                // Fire anchor remains separate from the facial and locomotion joints.
                var weapon=new GameObject("Pulse pistol",typeof(MeshFilter),typeof(MeshRenderer));weapon.transform.SetParent(prefab.transform,false);
                weapon.transform.localPosition=new Vector3(.35f/.65f,0,.4f/.65f);weapon.transform.localScale=new Vector3(.2f/.65f,.2f/.55f,.65f/.65f);
                // Linked after all weapon assets exist, below.
            }
            PrefabUtility.SaveAsPrefabAsset(prefab,Root+"/Prefabs/"+name+".prefab");Object.DestroyImmediate(prefab);
        }
        library.models=entries.ToArray();library.InvalidateLookup();EditorUtility.SetDirty(library);
        var scout=PrefabUtility.LoadPrefabContents(Root+"/Prefabs/Armored_body.prefab");library.TryGet("Pulse pistol",out var pistol);var gun=scout.transform.Find("Pulse pistol");gun.GetComponent<MeshFilter>().sharedMesh=pistol.mesh;gun.GetComponent<MeshRenderer>().sharedMaterial=pistol.surface;
        PrefabUtility.SaveAsPrefabAsset(scout,Root+"/Prefabs/Armored_body.prefab");PrefabUtility.UnloadPrefabContents(scout);
        File.WriteAllText(Root+"/MeshBudget.csv",csv.ToString());AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate(library);
        Debug.Log("CUBE_PRISM_ASSETS_OK: 8 redesigned articulated actors, 4 weapons and 21 rounded prop models.");
    }
    static Mesh SaveMesh(string name,Mesh generated)
    {
        string path=Root+"/Meshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh)
        {
            // Use mesh setters so native rendering buffers update during the same editor session.
            mesh.Clear();mesh.indexFormat=generated.indexFormat;mesh.vertices=generated.vertices;mesh.normals=generated.normals;
            mesh.colors=generated.colors;mesh.uv=generated.uv;mesh.triangles=generated.triangles;mesh.bounds=generated.bounds;
            Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);
        }
        else {mesh=generated;AssetDatabase.CreateAsset(mesh,path);}return mesh;
    }
    public static void Validate(ToyArtLibrary library)
    {
        foreach(string key in Actors.Concat(Weapons))
        {
            if(!library.TryGet(key,out var model)||!model.surface||model.mesh.subMeshCount!=1)throw new Exception("Missing single-material prism model "+key);
            int triangles=model.mesh.triangles.Length/3;
            if(triangles<1000||triangles>95000)throw new Exception("Unexpected geometry budget "+key+": "+triangles);
            if(model.mesh.colors.Length!=model.mesh.vertexCount||model.mesh.normals.Any(n=>!float.IsFinite(n.x)||n.sqrMagnitude<.9f))throw new Exception("Invalid palette / normals "+key);
            if(!model.lodMesh||model.lodMesh.triangles.Length>=model.mesh.triangles.Length*.45f)throw new Exception("Insufficient LOD reduction "+key);
            if(PrismArtGeometry.Actor(key))
            {
                if(!model.mechanicalRig||model.rig.Any(p=>!p.lodMesh)||model.rig.Sum(p=>p.lodMesh.triangles.Length)!=model.lodMesh.triangles.Length)throw new Exception("Incomplete mechanical LOD rig "+key);
                if(!model.rig.Any(p=>p.name=="Head")||!model.rig.Any(p=>p.name=="LeftEye"&&p.parent=="Head"))throw new Exception("Missing face rig "+key);
                if(model.rig.Sum(p=>p.mesh.triangles.Length)!=model.mesh.triangles.Length)throw new Exception("Rig loses geometry "+key);
            }
        }
        Debug.Log("CUBE_PRISM_GEOMETRY_OK: analytic normals, vertex palettes, budgets and complete hierarchical rigs.");
    }
    [MenuItem("Cube Invasion/Art/Install Prism Vanguard In Scene")]
    public static void Install()
    {
        BuildAssets();var game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game)throw new InvalidOperationException("Open gameplay scene first.");
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");
        var data=new SerializedObject(game);
        foreach(var pair in new[]{new[]{"playerBody","Armored body"},new[]{"gun","Pulse pistol"}})
        {
            var body=(Transform)data.FindProperty(pair[0]).objectReferenceValue;library.TryGet(pair[1],out var model);
            body.GetComponent<MeshFilter>().sharedMesh=model.mesh;body.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(null);EditorUtility.SetDirty(body.GetComponent<MeshFilter>());EditorUtility.SetDirty(body.GetComponent<MeshRenderer>());
        }
        var visor=(Transform)data.FindProperty("playerVisor").objectReferenceValue;if(visor)visor.gameObject.SetActive(false);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);Debug.Log("CUBE_PRISM_INSTALL_OK: authored player replaced; camera, transforms, fire anchor and UI layout preserved.");
    }
    public static void InstallGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
    [MenuItem("Cube Invasion/Art/Refresh Prism Menu Thumbnails")]
    public static void RefreshThumbnails()
    {
        var art=AssetDatabase.LoadAssetAtPath<FrontEndArt>("Assets/CubeInvasion/Resources/FrontEndArt.asset");var oldScout=art.scout;var oldWeapons=art.weapons.ToArray();
        art.scout=PrismArtReview.Thumbnail("Armored body");for(int i=0;i<4;i++)art.weapons[i]=PrismArtReview.Thumbnail(Weapons[i]);EditorUtility.SetDirty(art);
        foreach(var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(image.sprite==oldScout)image.sprite=art.scout;
            else for(int i=0;i<oldWeapons.Length;i++)if(image.sprite==oldWeapons[i]) {image.sprite=art.weapons[i];break;}
            EditorUtility.SetDirty(image);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("CUBE_PRISM_THUMBNAILS_OK");
    }
    public static void BuildInstallAndReview() {InstallGameplayScene();RefreshThumbnails();PrismArtReview.Catalog();}
}
