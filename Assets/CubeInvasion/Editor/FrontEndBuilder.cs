using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class FrontEndBuilder
{
    const string Root="Assets/CubeInvasion/Art/UI";
    [MenuItem("Cube Invasion/UI/Install Illustrated Front End")]
    public static void Install()
    {
        Directory.CreateDirectory(Root);AssetDatabase.Refresh();
        const string path="Assets/CubeInvasion/Resources/FrontEndArt.asset";
        var art=AssetDatabase.LoadAssetAtPath<FrontEndArt>(path);
        if(!art) { art=ScriptableObject.CreateInstance<FrontEndArt>();AssetDatabase.CreateAsset(art,path); }
        art.menuHero=Sprite(Root+"/MenuHero.png");art.campaignIslands=Sprite(Root+"/CampaignIslands.png");
        art.panel=ShapeSprite("RoundedPanel",false);art.disc=ShapeSprite("Disc",true);
        art.weapons=new Sprite[4];
        string[] modelPaths={"Assets/CubeInvasion/Art/Meshes/Pulse_pistol.asset","Assets/CubeInvasion/Art/P2/Models/Twin_blasters.prefab","Assets/CubeInvasion/Art/P2/Models/Scatter_shot.prefab","Assets/CubeInvasion/Art/P2/Models/Rapid_smg.prefab"};
        for(int i=0;i<4;i++)art.weapons[i]=Preview("Weapon_"+i,modelPaths[i]);
        art.scout=Preview("Scout","Assets/CubeInvasion/Art/Prefabs/Scout_Robot.prefab");
        EditorUtility.SetDirty(art);
        var runtime=Object.FindAnyObjectByType<InvasionPrototype>();if(!runtime)throw new System.InvalidOperationException("Open gameplay scene first.");
        var mobile=Object.FindAnyObjectByType<MobileCombatHud>();
        var menu=Object.FindAnyObjectByType<MenuFrontEnd>(FindObjectsInactive.Include);
        if(!menu)menu=new GameObject("Illustrated Front End",typeof(RectTransform)).AddComponent<MenuFrontEnd>();
        var content=AssetDatabase.LoadAssetAtPath<P2ContentCatalog>("Assets/CubeInvasion/Resources/P2Content.asset");menu.Initialize(mobile.SafeRoot,art,content);
        runtime.ConfigureFrontEnd(menu,art);EditorUtility.SetDirty(runtime);EditorUtility.SetDirty(menu);
        var rules=AssetDatabase.LoadAssetAtPath<P3MetaSettings>("Assets/CubeInvasion/Resources/P3MetaSettings.asset");EditorUtility.SetDirty(rules);
        var legacy=Object.FindAnyObjectByType<P2MissionHud>(FindObjectsInactive.Include);legacy.UseFrontEnd(true);
        var meta=Object.FindAnyObjectByType<P3MetaHud>(FindObjectsInactive.Include);meta.UseFrontEnd(true);
        // Hide legacy menu panels in edit mode while keeping result and save-recovery panels intact.
        var oldMenu=legacy.transform.Find("Mission loadout");if(oldMenu)oldMenu.gameObject.SetActive(false);
        var oldShop=meta.transform.Find("Arsenal progression");if(oldShop)oldShop.gameObject.SetActive(false);
        var preview=new MetaProfile {weapons=new WeaponProgress[4],completed=new bool[content.missions.Length]};
        for(int i=0;i<4;i++)preview.weapons[i]=new WeaponProgress {owned=i==0};
        menu.Refresh(preview,rules,content,0,0,0,"Loadout",null);menu.SyncSettings(1,1,false,false,true);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(runtime.gameObject.scene);EditorSceneManager.SaveScene(runtime.gameObject.scene);
        Debug.Log("CUBE_UI_INSTALL_OK: generated backgrounds, five authored menu pages and campaign path installed.");
    }
    [MenuItem("Cube Invasion/UI/Preview/Home")]
    static void Home()=>PreviewPage(MenuPage.Home);
    [MenuItem("Cube Invasion/UI/Preview/Road Map")]
    static void Road()=>PreviewPage(MenuPage.Road);
    [MenuItem("Cube Invasion/UI/Preview/Arsenal")]
    static void Arsenal()=>PreviewPage(MenuPage.Arsenal);
    [MenuItem("Cube Invasion/UI/Preview/Pilot")]
    static void Pilot()=>PreviewPage(MenuPage.Pilot);
    [MenuItem("Cube Invasion/UI/Preview/Settings")]
    static void Settings()=>PreviewPage(MenuPage.Settings);
    static void PreviewPage(MenuPage page)
    {
        var menu=Object.FindAnyObjectByType<MenuFrontEnd>(FindObjectsInactive.Include);
        if(!menu)throw new System.InvalidOperationException("Open the gameplay scene and install the front end first.");
        menu.gameObject.SetActive(true);menu.ShowPage(page,false);EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
    }
    public static void InstallGameplayScene() { EditorSceneManager.OpenScene("Assets/CubeInvasion/CubeInvasionPrototype.unity");Install(); }
    static Sprite Sprite(string path)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static Sprite ShapeSprite(string name,bool circle)
    {
        string path=Root+"/"+name+".asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite)return sprite;
        const int n=96;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.name=name;texture.filterMode=FilterMode.Bilinear;
        for(int y=0;y<n;y++)for(int x=0;x<n;x++)
        {
            float distance;
            if(circle)distance=new Vector2(x-47.5f,y-47.5f).magnitude-46;
            else { var p=new Vector2(Mathf.Max(Mathf.Abs(x-47.5f)-31,0),Mathf.Max(Mathf.Abs(y-47.5f)-31,0));distance=p.magnitude-15; }
            texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(.5f-distance)));
        }
        texture.Apply();AssetDatabase.CreateAsset(texture,Root+"/"+name+"Texture.asset");
        sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,n,n),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,circle?Vector4.zero:Vector4.one*24);
        AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static Sprite Preview(string name,string modelPath)
    {
        string path=Root+"/"+name+".png";
        if(!File.Exists(path))
        {
            var scene=EditorSceneManager.NewPreviewScene();GameObject model;
            if(modelPath.EndsWith(".prefab"))model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath),scene);
            else
            {
                model=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model,scene);
                model.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(modelPath);
                var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>("Assets/CubeInvasion/Resources/ToyArtLibrary.asset");library.TryGet("Pulse pistol",out var data);
                var paint=new Material(Shader.Find("Standard"));paint.color=new Color(.2f,.4f,.9f);model.GetComponent<Renderer>().sharedMaterials=new[]{paint,data.trim,data.accent};
            }
            var cameraObject=new GameObject("Thumbnail camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;
            var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            camera.orthographicSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*.6f;
            camera.transform.rotation=Quaternion.Euler(22,155,0);camera.transform.position=bounds.center-camera.transform.forward*10;
            var key=new GameObject("Thumbnail key light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(key,scene);var light=key.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.6f;key.transform.rotation=Quaternion.Euler(35,150,0);
            var target=new RenderTexture(384,384,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(384,384,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,384,384),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;Object.DestroyImmediate(texture);Object.DestroyImmediate(target);EditorSceneManager.ClosePreviewScene(scene);AssetDatabase.ImportAsset(path);
        }
        return Sprite(path);
    }
}
