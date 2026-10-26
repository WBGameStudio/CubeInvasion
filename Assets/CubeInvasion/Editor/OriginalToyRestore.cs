using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class OriginalToyRestore
{
    const string Art="Assets/CubeInvasion/Art";
    static string Slug(string key)=>key.Replace(' ','_').Replace("+","Plus");
    static GameObject Prefab(string key)=>AssetDatabase.LoadAssetAtPath<GameObject>(key=="Armored body"?Art+"/Prefabs/Scout_Robot.prefab":Art+"/P2/Models/"+Slug(key)+".prefab");
    [MenuItem("Cube Invasion/Art/Restore Original Toy Models")]
    public static void Install()
    {
        var original=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(Art+"/OriginalToyLibrary.asset");
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");
        if(!original||original.models.Length!=33)throw new Exception("Original 33-model library is missing");
        foreach(var m in original.models)if(!m.mesh||!m.trim||!m.accent||m.mesh.subMeshCount!=3)throw new Exception("Original model dependency missing: "+m.key);
        var custom=library.models.Where(m=>!original.models.Any(o=>o.key==m.key));
        library.models=original.models.Concat(custom).ToArray();library.InvalidateLookup();EditorUtility.SetDirty(library);
        EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");
        var game=Object.FindAnyObjectByType<InvasionPrototype>();var data=new SerializedObject(game);
        foreach(var entry in new[]{new[]{"playerBody","Armored body"},new[]{"gun","Pulse pistol"},new[]{"playerVisor","Visor"}})
        {
            var node=(Transform)data.FindProperty(entry[0]).objectReferenceValue;library.TryGet(entry[1],out var model);
            node.GetComponent<MeshFilter>().sharedMesh=model.mesh;
            var paint=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Paint_"+Slug(entry[1])+".mat");if(!paint)throw new Exception("Missing original paint "+entry[1]);
            node.GetComponent<MeshRenderer>().sharedMaterials=model.Materials(paint);node.gameObject.SetActive(true);
            EditorUtility.SetDirty(node.GetComponent<MeshFilter>());EditorUtility.SetDirty(node.GetComponent<MeshRenderer>());EditorUtility.SetDirty(node.gameObject);
        }
        var front=AssetDatabase.LoadAssetAtPath<FrontEndArt>("Assets/CubeInvasion/Resources/FrontEndArt.asset");var oldScout=front.scout;var oldWeapons=front.weapons.ToArray();
        front.scout=Thumbnail("Armored body");string[] guns={"Pulse pistol","Twin blasters","Scatter shot","Rapid smg"};
        for(int i=0;i<4;i++)front.weapons[i]=Thumbnail(guns[i]);EditorUtility.SetDirty(front);
        foreach(var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            bool change=false;if(image.sprite==oldScout) {image.sprite=front.scout;change=true;}
            else for(int i=0;i<4;i++)if(image.sprite==oldWeapons[i]) {image.sprite=front.weapons[i];change=true;break;}
            if(change)EditorUtility.SetDirty(image);
        }
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
        Debug.Log("CUBE_ORIGINAL_TOY_RESTORED: 33 original meshes/materials/rigs; authored player/gun/visor and five menu thumbnails; gameplay and camera preserved.");
    }
    static Sprite Thumbnail(string key)
    {
        string directory=Art+"/OriginalThumbnails";Directory.CreateDirectory(directory);AssetDatabase.Refresh();
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject obj;
            if(key=="Pulse pistol")
            {
                var source=Prefab("Armored body").transform.Find("Pulse Pistol");obj=Object.Instantiate(source.gameObject);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);obj.transform.position=Vector3.zero;obj.transform.rotation=Quaternion.identity;
            }
            else obj=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(key),scene);
            var renderers=obj.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var camObj=new GameObject("Original model thumbnail camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camObj,scene);var cam=camObj.AddComponent<Camera>();cam.scene=scene;cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.allowHDR=false;cam.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.62f;
            cam.transform.rotation=Quaternion.Euler(27,155,0);cam.transform.position=bounds.center-cam.transform.forward*10;
            for(int i=0;i<2;i++) {var lightObj=new GameObject("Studio light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObj,scene);var light=lightObj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=i==0?1.5f:.6f;light.transform.rotation=Quaternion.Euler(i==0?40:20,i==0?145:-35,0);}
            string path=directory+"/"+Slug(key)+".png";var rt=new RenderTexture(512,512,24);cam.targetTexture=rt;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(rt);Object.DestroyImmediate(image);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
    }
}
