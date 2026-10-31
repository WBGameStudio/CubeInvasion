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
    static IEnumerable<string> Keys=>VoidToyArtGeometry.Actors.Concat(VoidToyArtGeometry.Weapons).Concat(VoidToyArtGeometry.Props)
        .Concat(VoidToyArtGeometry.ExpansionActors).Concat(VoidToyArtGeometry.ExpansionWeapons).Append("Frozen patch").Append(VoidToyArtGeometry.Hero);
    // Loaded at runtime (Resources.Load "RogueCubeFaces") to swap KİP's screen-face expressions.
    public const string HeroFacesPath="Assets/CubeInvasion/Resources/RogueCubeFaces.asset";
    const string PresentationPath="Assets/CubeInvasion/Resources/CampaignPresentation.asset",ExpansionBackupPath=Root+"/ExpansionArtBackup.asset";
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
        var models=new List<ToyArtLibrary.Model>();var csv=new StringBuilder("Model,Vertices,Triangles,Budget,RigParts,Parts,LOD1Triangles\n");
        foreach(string key in Keys)
        {
            string name=Slug(key);var builder=VoidToyArtGeometry.Make(key);var mesh=SaveMesh(name,builder.Build(name));
            bool actor=VoidToyArtGeometry.Actor(key);var rig=actor?builder.BuildRig(name,SaveMesh):new ToyArtLibrary.RigPart[0];
            var paint=VoidToyArtGeometry.Tinted(key)?tinted:plastic;Mesh lod=null;
            if(VoidToyArtGeometry.HasLod(key))
            {
                // LOD1: same parts and pivots, plain blocks and halved round segments (ToyActorAnimator builds the LODGroup).
                var lowBuilder=VoidToyArtGeometry.Make(key,true);lod=SaveMesh(name+"_LOD1",lowBuilder.Build(name+"_LOD1"));
                var lowRig=lowBuilder.BuildRig(name+"_LOD1",SaveMesh);foreach(var part in rig)part.lodMesh=lowRig.First(p=>p.name==part.name).mesh;
            }
            models.Add(new ToyArtLibrary.Model {key=key,mesh=mesh,lodMesh=lod,rig=rig,surface=paint,trim=paint,accent=paint,integratedFace=actor,mechanicalRig=actor,useRuntimeTint=paint==tinted});
            csv.AppendLine($"{key},{mesh.vertexCount},{mesh.triangles.Length/3},{VoidToyArtGeometry.Budget(key)},{rig.Length},{string.Join(" ",rig.Select(p=>p.name))},{(lod?lod.triangles.Length/3:0)}");
            var prefab=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));prefab.GetComponent<MeshFilter>().sharedMesh=mesh;prefab.GetComponent<MeshRenderer>().sharedMaterial=paint;
            if(VoidToyArtGeometry.Props.Contains(key))prefab.transform.localScale=VoidToyArtGeometry.GameplayScale(key);
            if(actor)
            {
                prefab.transform.localScale=VoidToyArtGeometry.GameplayScale(key);prefab.AddComponent<ToyActorAnimator>().ConfigurePrefab(key);
                if(key=="Armored body"||key==VoidToyArtGeometry.Hero)
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
        snapshot.TryGet("Pulse pistol",out var pistol);
        foreach(string armed in new[]{"Armored body",VoidToyArtGeometry.Hero})
        {var scout=PrefabUtility.LoadPrefabContents(Prefab(armed));scout.transform.Find("Pulse pistol").GetComponent<MeshFilter>().sharedMesh=pistol.mesh;PrefabUtility.SaveAsPrefabAsset(scout,Prefab(armed));PrefabUtility.UnloadPrefabContents(scout);}
        BuildHeroFaces();
        File.WriteAllText(Root+"/MeshBudget.csv",csv.ToString());AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate(snapshot);
        Debug.Log("CUBE_VOIDTOY_ASSETS_OK: "+Keys.Count(VoidToyArtGeometry.Actor)+" articulated actors, "+Keys.Count(VoidToyArtGeometry.Weapon)+" weapons and "+Keys.Count(k=>!VoidToyArtGeometry.Actor(k)&&!VoidToyArtGeometry.Weapon(k))+" props / tiles in "+Root);
        return snapshot;
    }
    // KİP expression variants: one entry per expression, rig = LeftEye / RightEye / Mouth meshes on the default face pivots.
    static void BuildHeroFaces()
    {
        var faces=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(HeroFacesPath);if(!faces) {faces=ScriptableObject.CreateInstance<ToyArtLibrary>();AssetDatabase.CreateAsset(faces,HeroFacesPath);}
        var entries=new List<ToyArtLibrary.Model>();
        foreach(string expression in VoidToyArtGeometry.HeroFaces)
        {
            string name=Slug(VoidToyArtGeometry.Hero)+"_Face_"+expression;var builder=VoidToyArtGeometry.HeroFace(expression);
            entries.Add(new ToyArtLibrary.Model {key=VoidToyArtGeometry.Hero+"/"+expression,mesh=SaveMesh(name,builder.Build(name)),rig=builder.BuildRig(name,SaveMesh),integratedFace=true});
        }
        faces.models=entries.ToArray();faces.InvalidateLookup();EditorUtility.SetDirty(faces);
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
            if(!VoidToyArtGeometry.Actor(key)&&!VoidToyArtGeometry.Weapon(key))continue;
            if(!model.mesh.colors.Any(c=>c.a<.5f))throw new Exception("Missing emissive intent / energy light "+key);
            if(!VoidToyArtGeometry.Actor(key))continue;
            if(!model.mechanicalRig||!model.integratedFace||model.rig.Length<5)throw new Exception("Incomplete rig "+key);
            if(model.rig.Sum(p=>p.mesh.triangles.Length)!=model.mesh.triangles.Length)throw new Exception("Rig loses geometry "+key);
            if(!model.rig.Any(p=>p.name.EndsWith("Eye")))throw new Exception("Missing blink joint "+key);
            foreach(var part in model.rig)if(!string.IsNullOrEmpty(part.parent)&&!model.rig.Any(p=>p.name==part.parent))throw new Exception("Dangling rig parent "+key+"/"+part.name);
        }
        library.TryGet("Armored body",out var scout);if(scout.rig.Length!=7||!scout.rig.Any(p=>p.name=="RightHand"))throw new Exception("Scout needs the 7-part rig with a weapon hand");
        ValidateHero(library);
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
        var own=snapshot.models.Where(m=>!VoidToyArtGeometry.ExpansionOwned(m.key)).ToList();InstallExpansion(snapshot);
        var entries=library.models.Where(m=>m!=null&&!own.Any(v=>v.key==m.key)).ToList();entries.AddRange(own);
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
        // The restore keeps keys the original set never had; drop Void Toy-only keys so those fall back to the borrowed rigs / primitives.
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(LibraryPath);
        library.models=library.models.Where(m=>m==null||!IsActive(m)).ToArray();library.InvalidateLookup();EditorUtility.SetDirty(library);
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>(PresentationPath);var backup=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(ExpansionBackupPath);
        if(presentation&&presentation.art&&backup)
        {
            var art=presentation.art.models;for(int i=0;i<art.Length;i++) {var old=backup.models.FirstOrDefault(m=>m.key==art[i].key);if(old!=null)art[i]=old;}
            presentation.art.InvalidateLookup();EditorUtility.SetDirty(presentation.art);AssetDatabase.SaveAssets();CampaignExpansionBuilder.RefreshWeaponIcons();
        }
        AssetDatabase.SaveAssets();
    }
    // Expansion keys are replaced in place inside ExpansionArt (order matters: market icons read models[0..7]).
    static void InstallExpansion(ToyArtLibrary snapshot)
    {
        var presentation=AssetDatabase.LoadAssetAtPath<CampaignPresentation>(PresentationPath);
        if(!presentation||!presentation.art) {Debug.LogWarning("Campaign expansion is not installed; expansion restyles skipped.");return;}
        var art=presentation.art;
        if(!AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(ExpansionBackupPath))
        {
            var backup=ScriptableObject.CreateInstance<ToyArtLibrary>();backup.models=art.models.Where(m=>VoidToyArtGeometry.ExpansionOwned(m.key)&&!IsActive(m)).ToArray();
            AssetDatabase.CreateAsset(backup,ExpansionBackupPath);
        }
        var models=art.models;
        for(int i=0;i<models.Length;i++)if(VoidToyArtGeometry.ExpansionOwned(models[i].key)&&snapshot.TryGet(models[i].key,out var restyled))models[i]=restyled;
        var missing=snapshot.models.Where(m=>VoidToyArtGeometry.ExpansionOwned(m.key)&&!models.Any(a=>a.key==m.key));art.models=models.Concat(missing).ToArray();
        art.InvalidateLookup();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        CampaignExpansionBuilder.RefreshWeaponIcons();
    }
    public static void BuildInstallAndReview() {Install();VoidToyArtReview.Catalog();VoidToyArtReview.ArenaKit();VoidToyArtReview.Animation();VoidToyArtReview.RogueCube();}
    // Brief §3-§5 contract: names, parents, LOD per part, budgets, real-world size.
    static void ValidateHero(ToyArtLibrary library)
    {
        if(!library.TryGet(VoidToyArtGeometry.Hero,out var hero))throw new Exception("Missing Rogue cube");
        var parents=new Dictionary<string,string>{{"Hips",""},{"LeftLeg","Hips"},{"RightLeg","Hips"},{"Core",""},{"LeftEye","Core"},{"RightEye","Core"},{"Mouth","Core"},
            {"LeftArm","Core"},{"RightHand","Core"},{"Antenna","Core"},{"ScarfA","Core"},{"ScarfB","ScarfA"},{"ScarfC","ScarfB"}};
        if(hero.rig.Length!=parents.Count)throw new Exception("Rogue cube needs exactly "+parents.Count+" parts, has "+hero.rig.Length);
        foreach(var part in hero.rig)if(!parents.TryGetValue(part.name,out var parent)||(part.parent??"")!=parent)throw new Exception("Rogue cube part / parent mismatch: "+part.name+" <- "+part.parent);
        if(!hero.integratedFace||!hero.mechanicalRig||!hero.lodMesh||hero.rig.Any(p=>!p.lodMesh))throw new Exception("Rogue cube needs integratedFace, mechanicalRig and LOD1 for every part");
        int Tris(Mesh m)=>m.triangles.Length/3;int PartTris(params string[] names)=>hero.rig.Where(p=>names.Contains(p.name)).Sum(p=>Tris(p.mesh));
        if(hero.rig.Sum(p=>Tris(p.lodMesh))!=Tris(hero.lodMesh))throw new Exception("Rogue cube LOD1 rig loses geometry");
        if(Tris(hero.lodMesh)>Tris(hero.mesh)*.6f)throw new Exception("Rogue cube LOD1 reduction too small: "+Tris(hero.lodMesh)+"/"+Tris(hero.mesh));
        if(PartTris("LeftEye","RightEye","Mouth")>200||PartTris("ScarfA","ScarfB","ScarfC")+176>300)throw new Exception("Rogue cube face / scarf budget");
        var size=hero.mesh.bounds.size;
        if(Mathf.Abs(size.y-1.25f)>.02f)throw new Exception("Rogue cube height "+size.y.ToString("F3")+" (brief: 1.25)");
        var faces=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(HeroFacesPath);
        if(!faces||faces.models.Length!=VoidToyArtGeometry.HeroFaces.Length||faces.models.Any(f=>f.rig.Length!=3||f.rig.Sum(p=>Tris(p.mesh))>200))throw new Exception("Rogue cube expressions incomplete or over the 200-triangle face budget");
        Debug.Log($"CUBE_ROGUECUBE_OK: {Tris(hero.mesh)} tris (LOD1 {Tris(hero.lodMesh)}), {hero.rig.Length} parts, {size.x:F2} x {size.y:F2} x {size.z:F2} m, face {PartTris("LeftEye","RightEye","Mouth")}, scarf chain {PartTris("ScarfA","ScarfB","ScarfC")} (+176 collar on Core), {faces.models.Length} expressions.");
    }
}
