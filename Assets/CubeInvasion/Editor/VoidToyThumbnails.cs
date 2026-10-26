using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

// Renders transparent menu thumbnails (Scout + four weapons) from the Void Toy prefabs
// and swaps them into FrontEndArt and every scene Image that showed the previous art.
public static class VoidToyThumbnails
{
    const string Folder=VoidToyArtBuilder.Root+"/Thumbnails";

    public static Sprite Thumbnail(string key)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(VoidToyArtBuilder.Prefab(key));
        if(!prefab)throw new InvalidOperationException("Missing Void Toy prefab "+key);
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            // Same in-hand proportion as the model catalog: the weapon mesh is authored short on Z.
            if(VoidToyArtGeometry.Weapon(key))obj.transform.localScale=new Vector3(1,1,3.25f);
            var renderers=obj.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)if(r.enabled)bounds.Encapsulate(r.bounds);
            var cameraObject=new GameObject("Void Toy thumbnail camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);
            var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.orthographic=true;camera.allowHDR=false;camera.aspect=1;camera.transform.rotation=Quaternion.Euler(30,215,0);
            // Frame by the silhouette the camera actually sees, so long thin weapons fill the tile like the Scout does.
            var view=camera.transform.rotation;Vector2 min=Vector2.one*float.MaxValue,max=-min;
            for(int c=0;c<8;c++)
            {
                var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1));
                var local=Quaternion.Inverse(view)*(corner-bounds.center);min=Vector2.Min(min,local);max=Vector2.Max(max,local);
            }
            var centre=(min+max)*.5f;var size=max-min;
            // The projected box of an actor is looser than its silhouette, so actors frame tighter than weapons.
            camera.orthographicSize=Mathf.Max(size.x,size.y)*.5f*(VoidToyArtGeometry.Weapon(key)?1.04f:.82f);
            camera.transform.position=bounds.center+view*new Vector3(centre.x,centre.y,0)-camera.transform.forward*20;
            Directory.CreateDirectory(Folder);
            string path=Folder+"/"+VoidToyArtBuilder.Slug(key)+".png";
            var target=new RenderTexture(640,640,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(640,640,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,640,640),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=512;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
    }

    [MenuItem("Cube Invasion/Art/Void Toy/Refresh Void Toy Menu Thumbnails")]
    public static void Refresh()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode before refreshing thumbnails.");
        var art=AssetDatabase.LoadAssetAtPath<FrontEndArt>("Assets/CubeInvasion/Resources/FrontEndArt.asset");
        var oldScout=art.scout;var oldWeapons=art.weapons.ToArray();
        art.scout=Thumbnail("Armored body");
        for(int i=0;i<4;i++)art.weapons[i]=Thumbnail(VoidToyArtGeometry.Weapons[i]);
        EditorUtility.SetDirty(art);
        int swapped=0;
        foreach(var image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(oldScout&&image.sprite==oldScout) {image.sprite=art.scout;swapped++;EditorUtility.SetDirty(image);continue;}
            for(int i=0;i<oldWeapons.Length;i++)
                if(oldWeapons[i]&&image.sprite==oldWeapons[i]) {image.sprite=art.weapons[i];swapped++;EditorUtility.SetDirty(image);break;}
        }
        AssetDatabase.SaveAssets();
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("CUBE_VOIDTOY_THUMBNAILS_OK: Scout and four weapons rendered from Void Toy prefabs; "+swapped+" scene images updated.");
    }

    public static void RefreshGameplayScene()
    { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Refresh(); }
}
