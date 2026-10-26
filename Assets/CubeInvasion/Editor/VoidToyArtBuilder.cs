using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using CubeInvasion.ArtTools;
using Object=UnityEngine.Object;

public static class VoidToyArtBuilder
{
    public const string Root="Assets/CubeInvasion/Art/VoidToy";
    const string LibraryPath="Assets/CubeInvasion/Resources/ToyArtLibrary.asset";
    public const string SnapshotPath=Root+"/VoidToyLibrary.asset";
    public static string Slug(string key)=>key.Replace(' ','_').Replace("+","Plus");
    public static string Prefab(string key)=>Root+"/Prefabs/"+Slug(key)+".prefab";
    static IEnumerable<string> Keys=>VoidToyArtGeometry.Actors.Concat(VoidToyArtGeometry.Weapons).Concat(VoidToyArtGeometry.Props);
    public static bool IsActive(ToyArtLibrary.Model model)=>model!=null&&model.mesh&&AssetDatabase.GetAssetPath(model.mesh).StartsWith(Root+"/");
    [MenuItem("Cube Invasion/Art/Void Toy/Build Void Toy Models")]
    public static ToyArtLibrary BuildAssets()
    {
        foreach(string folder in new[]{"Meshes","Materials","Prefabs"})Directory.CreateDirectory(Root+"/"+folder);AssetDatabase.Refresh();
        var shader=Shader.Find("CubeInvasion/Void Toy Plastic");if(!shader)throw new InvalidOperationException("Void Toy Plastic shader did not import.");
        // One shared material for the whole cast: palette in vertex colours, intent lights in vertex alpha.
        var plastic=Paint("VoidToy_Plastic",shader,false);var tinted=Paint("VoidToy_Tinted",shader,true);
        var snapshot=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(SnapshotPath);
        if(!snapshot) {snapshot=ScriptableObject.CreateInstance<ToyArtLibrary>();AssetDatabase.CreateAsset(snapshot,SnapshotPath);}
        var models=new List<ToyArtLibrary.Model>();var csv=new StringBuilder("Model,Vertices,Triangles,Budget,RigParts,Parts\n");
        foreach(string key in Keys)
        {
            string name=Slug(key);var builder=VoidToyArtGeometry.Make(key);var mesh=SaveMesh(name,builder.Build(name));
            bool actor=VoidToyArtGeometry.Actor(key);var rig=actor?builder.BuildRig(name,SaveMesh):new ToyArtLibrary.RigPart[0];
            var paint=VoidToyArtGeometry.Tinted(key)?tinted:plastic;
            models.Add(new ToyArtLibrary.Model {key=key,mesh=mesh,rig=rig,surface=paint,trim=paint,accent=paint,integratedFace=actor,mechanicalRig=actor,useRuntimeTint=paint==tinted});
            csv.AppendLine($"{key},{mesh.vertexCount},{mesh.triangles.Length/3},{VoidToyArtGeometry.Budget(key)},{rig.Length},{string.Join(" ",rig.Select(p=>p.name))}");
            var prefab=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));prefab.GetComponent<MeshFilter>().sharedMesh=mesh;prefab.GetComponent<MeshRenderer>().sharedMaterial=paint;
            if(VoidToyArtGeometry.Props.Contains(key))prefab.transform.localScale=VoidToyArtGeometry.GameplayScale(key);
            if(actor)
            {
                prefab.transform.localScale=VoidToyArtGeometry.GameplayScale(key);prefab.AddComponent<ToyActorAnimator>().ConfigurePrefab(key);
                if(key=="Armored body")
                {
                    // Same authored fire-anchor offset as the gameplay scene; linked to the pistol mesh after the loop.
                    var weapon=new GameObject("Pulse pistol",typeof(MeshFilter),typeof(MeshRenderer));weapon.transform.SetParent(prefab.transform,false);
                    var s=prefab.transform.localScale;weapon.transform.localPosition=new Vector3(.35f/s.x,0,.4f/s.z);weapon.transform.localScale=Vector3.Scale(VoidToyArtGeometry.GameplayScale("Pulse pistol"),new Vector3(1/s.x,1/s.y,1/s.z));
                    weapon.GetComponent<MeshRenderer>().sharedMaterial=plastic;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(prefab,Prefab(key));Object.DestroyImmediate(prefab);
        }
        snapshot.models=models.ToArray();snapshot.InvalidateLookup();EditorUtility.SetDirty(snapshot);
        var scout=PrefabUtility.LoadPrefabContents(Prefab("Armored body"));snapshot.TryGet("Pulse pistol",out var pistol);scout.transform.Find("Pulse pistol").GetComponent<MeshFilter>().sharedMesh=pistol.mesh;
        PrefabUtility.SaveAsPrefabAsset(scout,Prefab("Armored body"));PrefabUtility.UnloadPrefabContents(scout);
        File.WriteAllText(Root+"/MeshBudget.csv",csv.ToString());AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate(snapshot);
        Debug.Log("CUBE_VOIDTOY_ASSETS_OK: 8 articulated actors, 4 weapons and "+VoidToyArtGeometry.Props.Length+" arena kit props in "+Root);
        return snapshot;
    }
    static Material Paint(string name,Shader shader,bool tint)
    {
        // Two materials for everything: palette in vertex colours, intent lights in vertex alpha; the tinted one serves runtime-coloured props.
        string path=Root+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material) {material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
        material.shader=shader;material.color=new Color(1,1,1,tint?1:0);material.SetFloat("_UseTint",tint?1:0);material.SetFloat("_Gloss",.35f);material.SetFloat("_Glow",1.15f);material.enableInstancing=true;
        EditorUtility.SetDirty(material);return material;
    }
    static Mesh SaveMesh(string name,Mesh generated)
    {
        string path=Root+"/Meshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh)
        {
            // Setters keep native buffers in sync while the same editor session renders them.
            mesh.Clear();mesh.indexFormat=generated.indexFormat;mesh.vertices=generated.vertices;mesh.normals=generated.normals;mesh.colors=generated.colors;mesh.uv=generated.uv;mesh.triangles=generated.triangles;mesh.bounds=generated.bounds;
            Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);
        }
        else {mesh=generated;AssetDatabase.CreateAsset(mesh,path);}return mesh;
    }
    public static void Validate(ToyArtLibrary library)
    {
        foreach(string key in Keys)
        {
            if(!library.TryGet(key,out var model)||!model.surface||model.mesh.subMeshCount!=1)throw new Exception("Missing single-material Void Toy model "+key);
            int triangles=model.mesh.triangles.Length/3;
            if(triangles<40||triangles>VoidToyArtGeometry.Budget(key))throw new Exception($"Triangle budget {key}: {triangles}/{VoidToyArtGeometry.Budget(key)}");
            if(model.mesh.colors.Length!=model.mesh.vertexCount||model.mesh.normals.Any(n=>!float.IsFinite(n.x)||n.sqrMagnitude<.9f))throw new Exception("Invalid palette / normals "+key);
            if(VoidToyArtGeometry.Tinted(key)!=model.useRuntimeTint||VoidToyArtGeometry.Tinted(key)&&!model.mesh.uv.Any(m=>m.x>.5f))throw new Exception("Runtime tint mask mismatch "+key);
            if(VoidToyArtGeometry.Props.Contains(key))continue;
            if(!model.mesh.colors.Any(c=>c.a<.5f))throw new Exception("Missing emissive intent / energy light "+key);
            if(!VoidToyArtGeometry.Actor(key))continue;
            if(!model.mechanicalRig||!model.integratedFace||model.rig.Length<5)throw new Exception("Incomplete rig "+key);
            if(model.rig.Sum(p=>p.mesh.triangles.Length)!=model.mesh.triangles.Length)throw new Exception("Rig loses geometry "+key);
            if(!model.rig.Any(p=>p.name.EndsWith("Eye")))throw new Exception("Missing blink joint "+key);
            foreach(var part in model.rig)if(!string.IsNullOrEmpty(part.parent)&&!model.rig.Any(p=>p.name==part.parent))throw new Exception("Dangling rig parent "+key+"/"+part.name);
        }
        library.TryGet("Armored body",out var scout);if(scout.rig.Length!=7||!scout.rig.Any(p=>p.name=="RightHand"))throw new Exception("Scout needs the 7-part rig with a weapon hand");
        library.TryGet("Crusher",out var boss);if(boss.rig.Count(p=>p.name.EndsWith("Armor"))!=2)throw new Exception("Crusher armour panels must be separate meshes");
        Debug.Log("CUBE_VOIDTOY_GEOMETRY_OK: budgets, palette, intent lights, tint masks, complete rigs, separate boss armour.");
    }
    // Swaps the active library entries and the authored player / gun meshes only; transforms, camera and UI stay as they are.
    [MenuItem("Cube Invasion/Art/Void Toy/Build And Install Void Toy Models")]
    public static void Install()
    {
        var snapshot=BuildAssets();InstallFrom(snapshot);
    }
    [MenuItem("Cube Invasion/Art/Void Toy/Reinstall Built Void Toy Models")]
    public static void Reinstall()
    {
        var snapshot=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(SnapshotPath);if(!snapshot)throw new InvalidOperationException("Build the Void Toy models first.");
        Validate(snapshot);InstallFrom(snapshot);
    }
    static void InstallFrom(ToyArtLibrary snapshot)
    {
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(LibraryPath);if(!library)throw new InvalidOperationException("Active art library is missing.");
        var entries=library.models.Where(m=>m!=null&&!snapshot.models.Any(v=>v.key==m.key)).ToList();entries.AddRange(snapshot.models);
        library.models=entries.ToArray();library.InvalidateLookup();EditorUtility.SetDirty(library);
        var scene=EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var game=Object.FindAnyObjectByType<InvasionPrototype>();if(!game)throw new InvalidOperationException("Gameplay scene has no InvasionPrototype.");
        var data=new SerializedObject(game);
        foreach(var pair in new[]{new[]{"playerBody","Armored body"},new[]{"gun","Pulse pistol"}})
        {
            var node=(Transform)data.FindProperty(pair[0]).objectReferenceValue;library.TryGet(pair[1],out var model);
            node.GetComponent<MeshFilter>().sharedMesh=model.mesh;node.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(null);
            EditorUtility.SetDirty(node.GetComponent<MeshFilter>());EditorUtility.SetDirty(node.GetComponent<MeshRenderer>());
        }
        // The Scout head carries its own visor; the restore tool turns the separate one back on.
        var visor=(Transform)data.FindProperty("playerVisor").objectReferenceValue;if(visor) {visor.gameObject.SetActive(false);EditorUtility.SetDirty(visor.gameObject);}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("CUBE_VOIDTOY_INSTALL_OK: "+snapshot.models.Length+" library entries and the authored player / gun meshes replaced; camera, transforms, fire anchor and UI untouched.");
    }
    [MenuItem("Cube Invasion/Art/Void Toy/Restore Original Toy Models")]
    public static void Restore()
    {
        OriginalToyRestore.Install();
        // The restore keeps keys the original set never had; drop the Void Toy crown so elites go back to primitives.
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(LibraryPath);
        library.models=library.models.Where(m=>m==null||!(m.key=="Elite crown"&&IsActive(m))).ToArray();library.InvalidateLookup();EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
    }
    public static void BuildInstallAndReview() {Install();VoidToyArtReview.Catalog();VoidToyArtReview.ArenaKit();VoidToyArtReview.Animation();}
}
