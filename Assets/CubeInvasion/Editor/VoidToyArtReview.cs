using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CubeInvasion.Prototype;
using Object=UnityEngine.Object;

public static class VoidToyArtReview
{
    const string Previews="Documentation/Previews/VoidToy";
    static GameObject Load(string key,UnityEngine.SceneManagement.Scene scene)
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(VoidToyArtBuilder.Prefab(key));
        if(!prefab)throw new InvalidOperationException("Missing Void Toy prefab "+key);
        return (GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
    }
    static Camera Camera(UnityEngine.SceneManagement.Scene scene,Color background)
    {
        var obj=new GameObject("Void Toy studio camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);var camera=obj.AddComponent<Camera>();
        camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=background;camera.orthographic=true;camera.allowHDR=false;
        camera.transform.rotation=Quaternion.Euler(30,215,0);return camera;
    }
    static Bounds Bounds(GameObject obj)
    {
        var renderers=obj.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)if(r.enabled)b.Encapsulate(r.bounds);return b;
    }
    static TextMesh Label(UnityEngine.SceneManagement.Scene scene,Camera camera,string text,Vector3 at,int size,Color color)
    {
        var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/CubeInvasion/Art/UI/Reference/CubeRounded-Bold.ttf");
        var obj=new GameObject("Label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,scene);var label=obj.AddComponent<TextMesh>();
        if(font) {label.font=font;obj.GetComponent<MeshRenderer>().sharedMaterial=font.material;}
        label.text=text;label.fontSize=size;label.characterSize=.032f;label.anchor=TextAnchor.MiddleCenter;label.color=color;obj.transform.rotation=camera.transform.rotation;obj.transform.position=at;return label;
    }
    static Texture2D Grab(Camera camera,int width,int height)
    {
        var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
        camera.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(target);return texture;
    }
    static void Write(Texture2D texture,string path)
    {Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));File.WriteAllBytes(path,texture.EncodeToPNG());}
    [MenuItem("Cube Invasion/Art/Void Toy/Render Void Toy Model Catalog")]
    public static void Catalog()
    {
        CatalogSheet(VoidToyArtGeometry.Actors.Take(8).Concat(VoidToyArtGeometry.Weapons).ToList(),4,"Model_Catalog.png");
        CatalogSheet(new List<string>{"Shielder","Healer","Bomber","Summoner","Phaser","Elite","Gold crate","Jackpot chest","Spike tile","Electric coil","Lava tile","Frozen patch"}.Concat(VoidToyArtGeometry.ExpansionWeapons).ToList(),5,"Expansion_Catalog.png");
    }
    static void CatalogSheet(List<string> keys,int columns,string file)
    {
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(VoidToyArtBuilder.SnapshotPath);
        var scene=EditorSceneManager.NewPreviewScene();int rows=(keys.Count+columns-1)/columns;
        try
        {
            var camera=Camera(scene,VoidToyPalette.Floor);camera.orthographicSize=Mathf.Max(columns*3.05f,rows*3.7f)*.5f+.4f;camera.aspect=1;camera.transform.position=-camera.transform.forward*30;
            var ink=VoidToyPalette.Ink;
            for(int i=0;i<keys.Count;i++)
            {
                var obj=Load(keys[i],scene);
                // Weapons are shown at the fire anchor's in-game length (0.65 / 0.2).
                if(VoidToyArtGeometry.Weapon(keys[i]))obj.transform.localScale=new Vector3(1,1,3.25f);
                if(keys[i]=="Frozen patch")obj.transform.localScale=new Vector3(1.9f,.03f,1.9f);
                if(keys[i]=="Spike tile")Load("Spike tile spikes",scene).transform.SetParent(obj.transform,false);
                // Hazard tiles shown in their active tint, as InvasionPrototype.HazardTiles paints them.
                var tint=keys[i]=="Spike tile"?VoidToyPalette.Danger:keys[i]=="Electric coil"?VoidToyPalette.CyanText:VoidToyPalette.Warm;
                if(VoidToyArtGeometry.Tinted(keys[i])) {var block=new MaterialPropertyBlock();block.SetColor("_Color",tint);obj.GetComponent<Renderer>().SetPropertyBlock(block);}
                var b=Bounds(obj);
                // Actors keep their relative gameplay size; weapons and props are fitted so their silhouettes read.
                float fit=!VoidToyArtGeometry.Actor(keys[i])?2f/Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z)):1.55f;obj.transform.localScale*=fit;b=Bounds(obj);
                var point=camera.transform.right*((i%columns-(columns-1)*.5f)*3.05f)+camera.transform.up*(((rows-1)*.5f-i/columns)*3.7f+.25f);obj.transform.position+=point-b.center;
                library.TryGet(keys[i],out var model);int triangles=model.mesh.triangles.Length/3;
                string title=keys[i]=="Armored body"?"SCOUT MK-II":keys[i].ToUpperInvariant();
                Label(scene,camera,title,point-camera.transform.up*1.45f,56,ink);
                Label(scene,camera,triangles+" tris"+(model.rig.Length>0?" · "+model.rig.Length+" parts":""),point-camera.transform.up*1.8f,40,VoidToyPalette.Side);
            }
            var image=Grab(camera,2048,2048);Write(image,Previews+"/"+file);Object.DestroyImmediate(image);
        }
        finally {EditorSceneManager.ClosePreviewScene(scene);}
        Debug.Log("CUBE_VOIDTOY_CATALOG_OK: "+Previews+"/"+file);
    }
    // One assembled combat cube in the gameplay layout of InvasionPrototype.Arena(), props tinted the way Shape() tints them.
    [MenuItem("Cube Invasion/Art/Void Toy/Render Void Toy Arena Kit")]
    public static void ArenaKit()
    {
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(VoidToyArtBuilder.SnapshotPath);
        var scene=EditorSceneManager.NewPreviewScene();var animators=new List<ToyActorAnimator>();
        try
        {
            var root=new GameObject("Combat cube").transform;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root.gameObject,scene);
            Transform Spawn(string key,Vector3 at,Transform parent,Color tint,Vector3? scale=null)
            {
                var obj=Load(key,scene);obj.transform.SetParent(parent,false);obj.transform.localPosition=at;if(scale.HasValue)obj.transform.localScale=scale.Value;
                library.TryGet(key,out var model);
                if(model.useRuntimeTint) {var block=new MaterialPropertyBlock();block.SetColor("_Color",tint);obj.GetComponent<Renderer>().SetPropertyBlock(block);}
                return obj.transform;
            }
            Spawn("Deep sides",new Vector3(0,-1.2f,0),root,Color.white);Spawn("Arena surface",new Vector3(0,-.04f,0),root,Color.white);
            for(int i=0;i<4;i++)
            {
                bool alongX=i<2;
                Spawn("Edge light",new Vector3(alongX?0:(i==2?-4.45f:4.45f),.03f,alongX?(i==0?-4.45f:4.45f):0),root,i==0?VoidToyPalette.Warm:VoidToyPalette.Cyan,alongX?new Vector3(9,.06f,.07f):new Vector3(.07f,.06f,9));
            }
            Spawn("Low cover",new Vector3(0,.45f,.6f),root,Color.white);
            var barrel=Spawn("Explosive barrel",new Vector3(2.7f,.3f,-.6f),root,Color.white);Spawn("Explosive band",new Vector3(0,.2f,0),barrel,Color.white);
            var health=Spawn("Health +20",new Vector3(-2.6f,.25f,-1.4f),root,Color.white);Spawn("Health cross H",new Vector3(0,.6f,0),health,Color.white);Spawn("Health cross V",new Vector3(0,.6f,0),health,Color.white);
            for(int i=0;i<4;i++)Spawn("Gold pickup",new Vector3(-2.9f+i*.6f,.08f,2.6f),root,Color.white);
            var gate=new GameObject("Gate").transform;gate.SetParent(root,false);gate.localPosition=new Vector3(2.4f,0,3.6f);
            Spawn("Gate left post",new Vector3(-1,1,0),gate,VoidToyPalette.Health);Spawn("Gate right post",new Vector3(1,1,0),gate,VoidToyPalette.Health);Spawn("Gate header",new Vector3(0,2,0),gate,VoidToyPalette.Health);
            Spawn("Locked until clear",new Vector3(0,1,0),gate,VoidToyPalette.Danger);
            Spawn("Right bridge",new Vector3(6,-.05f,0),root,VoidToyPalette.Cyan);
            Spawn("Reward beacon",new Vector3(-2.6f,1.6f,3.6f),root,VoidToyPalette.Gold);Spawn("Reward beacon vertical",new Vector3(-2.6f,1.6f,3.6f),root,VoidToyPalette.Gold);
            foreach(var actor in new[]{("Armored body",new Vector3(0,.65f,-2.4f),0f),("Grunt",new Vector3(-1.4f,.6f,1.6f),180f),("Gunner",new Vector3(1.3f,.6f,1.9f),200f),("Pusher",new Vector3(-.2f,.6f,2.8f),180f)})
            {
                var obj=Load(actor.Item1,scene);obj.transform.SetParent(root,false);obj.transform.localPosition=actor.Item2;obj.transform.localRotation=Quaternion.Euler(0,actor.Item3,0);
                var animator=obj.GetComponent<ToyActorAnimator>();animator.enabled=false;var accessories=new List<Transform>();foreach(Transform child in obj.transform)accessories.Add(child);
                library.TryGet(actor.Item1,out var model);animator.Bind(model,obj.transform);foreach(var child in accessories)animator.Attach(child);animator.TickPose(Vector3.zero,1f/24,false);animators.Add(animator);
            }
            var camera=Camera(scene,VoidToyPalette.Void);camera.transform.rotation=Quaternion.Euler(42,24,0);camera.orthographicSize=6.4f;camera.aspect=4f/3;camera.transform.position=new Vector3(.8f,0,.6f)-camera.transform.forward*40;
            var image=Grab(camera,1600,1200);Write(image,Previews+"/Arena_Kit.png");Object.DestroyImmediate(image);
        }
        finally
        {
            foreach(var animator in animators)if(animator)animator.SendMessage("OnDestroy",SendMessageOptions.DontRequireReceiver);
            EditorSceneManager.ClosePreviewScene(scene);
        }
        Debug.Log("CUBE_VOIDTOY_ARENA_OK: "+Previews+"/Arena_Kit.png");
    }
    // Drives every actor through idle / walk / run+fire / windup / slam / hit and checks the rig contract.
    [MenuItem("Cube Invasion/Art/Void Toy/Render Void Toy Animation Review")]
    public static void Animation()
    {
        // The snapshot holds every Void Toy actor, including the restyles installed into ExpansionArt.
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(VoidToyArtBuilder.SnapshotPath);VoidToyArtBuilder.Validate(library);
        var keys=VoidToyArtGeometry.Actors.Concat(VoidToyArtGeometry.ExpansionActors).ToArray();var scene=EditorSceneManager.NewPreviewScene();var animators=new List<ToyActorAnimator>();
        var roots=new List<Transform>();var positions=new List<Vector3>();var blinked=new bool[keys.Length];var moved=new bool[keys.Length];
        var fire=new List<Transform>();var firePositions=new List<Vector3>();
        int[] sheetFrames={12,40,78,104,114,130};string[] sheetTitles={"IDLE","WALK","RUN + FIRE","WINDUP","SLAM","HIT"};
        Texture2D sheet=null;
        try
        {
            var camera=Camera(scene,VoidToyPalette.Floor);camera.orthographicSize=4.3f;camera.aspect=16f/9;camera.transform.position=-camera.transform.forward*30;
            for(int i=0;i<keys.Length;i++)
            {
                var obj=Load(keys[i],scene);var bounds=Bounds(obj);obj.transform.localScale*=1.2f;bounds=Bounds(obj);
                var point=camera.transform.right*((i%5-2)*2.75f)+camera.transform.up*((1-i/5)*2.55f-.1f);obj.transform.position+=point-bounds.center;
                roots.Add(obj.transform);positions.Add(obj.transform.position);
                var animator=obj.GetComponent<ToyActorAnimator>();animator.enabled=false;
                var accessories=new List<Transform>();foreach(Transform child in obj.transform)accessories.Add(child);
                library.TryGet(keys[i],out var model);animator.Bind(model,obj.transform);foreach(var child in accessories)animator.Attach(child);
                var anchor=obj.transform.Find("Pulse pistol");fire.Add(anchor);firePositions.Add(anchor?anchor.localPosition:Vector3.zero);
                animators.Add(animator);
                Label(scene,camera,keys[i]=="Armored body"?"SCOUT":keys[i].ToUpperInvariant(),point-camera.transform.up*1.08f,40,VoidToyPalette.Ink);
            }
            var title=Label(scene,camera,"",camera.transform.up*3.95f,56,VoidToyPalette.Ink);
            sheet=new Texture2D(640*3,360*2,TextureFormat.RGB24,false);
            for(int frame=0;frame<144;frame++)
            {
                bool walk=frame>=24&&frame<60,run=frame>=60&&frame<96;
                title.text=frame<24?"IDLE / BLINK":walk?"WALK":run?"RUN / AUTO FIRE":frame<110?"ATTACK WINDUP":frame<122?"SLAM":"HIT / RECOVER";
                for(int i=0;i<animators.Count;i++)
                {
                    var animator=animators[i];if(run&&frame%8==0)animator.Shoot();if(frame==96)animator.Telegraph(.6f);if(frame==110)animator.Slam();if(frame==122)animator.Hit();
                    animator.TickPose(walk?Vector3.forward*1.2f:run?Vector3.forward*4.5f:Vector3.zero,1f/24,run);
                    if(roots[i].position!=positions[i]||roots[i].rotation!=Quaternion.identity)throw new Exception("Animation moved gameplay root: "+keys[i]);
                    if(fire[i]&&fire[i].localPosition!=firePositions[i])throw new Exception("Animation moved the fire anchor: "+keys[i]);
                    if(!float.IsFinite(animator.VisualRoot.localPosition.y))throw new Exception("Non-finite animation: "+keys[i]);
                    foreach(var node in animator.VisualRoot.GetComponentsInChildren<Transform>())
                    {
                        if(node.name.EndsWith("Eye")&&node.localScale.y<.4f)blinked[i]=true;
                        if(node.name.EndsWith("Leg")&&(walk||run)&&Quaternion.Angle(node.localRotation,Quaternion.identity)>5)moved[i]=true;
                        if(node.name.EndsWith("Armor")&&node.localRotation!=Quaternion.identity)throw new Exception("Boss armour panel must stay fixed: "+node.name);
                    }
                }
                int shot=Array.IndexOf(sheetFrames,frame);
                if(shot>=0)
                {
                    title.text=sheetTitles[shot];var image=Grab(camera,640,360);
                    sheet.SetPixels(shot%3*640,(1-shot/3)*360,640,360,image.GetPixels());Object.DestroyImmediate(image);
                }
            }
            for(int i=0;i<keys.Length;i++)if(!blinked[i]||!moved[i])throw new Exception("Missing blink / leg motion: "+keys[i]);
            // Enemy muzzle flashes come from these joints (InvasionPrototype.EnemyMuzzle).
            foreach(var pair in new[]{new[]{"Sniper","DOTween Reactions/RightHand"},new[]{"Gunner","DOTween Reactions/Head/RightHand"},new[]{"Crusher","DOTween Reactions/RightHand"},new[]{"Phaser","DOTween Reactions/RightHand"},new[]{"Bruiser","DOTween Reactions/RightArm/RightHand"}})
                if(!animators[Array.IndexOf(keys,pair[0])].VisualRoot.Find(pair[1]))throw new Exception("Missing muzzle joint "+pair[0]);
            sheet.Apply();Write(sheet,Previews+"/Animation_Review.png");
            Debug.Log("CUBE_VOIDTOY_ANIMATION_OK: eight actors, blink, stride, recoil, windup, slam, hit; fixed roots, fire anchor and boss armour.");
        }
        finally
        {
            if(sheet)Object.DestroyImmediate(sheet);
            foreach(var animator in animators)if(animator)animator.SendMessage("OnDestroy",SendMessageOptions.DontRequireReceiver);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
    // KİP review: turntable, the six screen-face expressions, and the in-game 45/45 shot among graphite enemies.
    [MenuItem("Cube Invasion/Art/Void Toy/Render Rogue Cube Review")]
    public static void RogueCube()
    {
        var library=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(VoidToyArtBuilder.SnapshotPath);VoidToyArtBuilder.Validate(library);
        var faces=AssetDatabase.LoadAssetAtPath<ToyArtLibrary>(VoidToyArtBuilder.HeroFacesPath);library.TryGet(VoidToyArtGeometry.Hero,out var hero);
        var animators=new List<ToyActorAnimator>();var scene=EditorSceneManager.NewPreviewScene();Texture2D sheet=null;
        ToyActorAnimator Spawn(string key,Vector3 at,float yaw)
        {
            var obj=Load(key,scene);obj.transform.position=at;obj.transform.rotation=Quaternion.Euler(0,yaw,0);
            var animator=obj.GetComponent<ToyActorAnimator>();animator.enabled=false;var accessories=new List<Transform>();foreach(Transform child in obj.transform)accessories.Add(child);
            library.TryGet(key,out var model);animator.Bind(model,obj.transform);foreach(var child in accessories)animator.Attach(child);
            animator.TickPose(Vector3.zero,.001f,false);foreach(var node in animator.VisualRoot.GetComponentsInChildren<Transform>())if(node.name.EndsWith("Eye"))node.localScale=Vector3.one;
            animators.Add(animator);return animator;
        }
        try
        {
            // 1. Turntable and expression sheet.
            var kip=Spawn(VoidToyArtGeometry.Hero,Vector3.zero,0);var bounds=Bounds(kip.gameObject);
            var camera=Camera(scene,VoidToyPalette.Floor);camera.aspect=1;
            var title=Label(scene,camera,"",Vector3.zero,52,VoidToyPalette.Ink);
            sheet=new Texture2D(480*6,480*2,TextureFormat.RGB24,false);
            var views=new[]{("FRONT",new Vector3(8,0,0),180f),("3/4",new Vector3(22,0,0),215f),("SIDE",new Vector3(8,0,0),270f),("BACK",new Vector3(18,0,0),20f),("TOP",new Vector3(70,0,0),215f),("3/4 HIGH",new Vector3(45,0,0),225f)};
            for(int i=0;i<views.Length;i++)
            {
                camera.transform.rotation=Quaternion.Euler(views[i].Item2.x,views[i].Item3,0);camera.orthographicSize=.95f;camera.transform.position=bounds.center+Vector3.up*.05f-camera.transform.forward*20;
                title.text=views[i].Item1;title.transform.rotation=camera.transform.rotation;title.transform.position=bounds.center+camera.transform.up*.82f;
                var image=Grab(camera,480,480);sheet.SetPixels(i*480,480,480,480,image.GetPixels());Object.DestroyImmediate(image);
            }
            var reactions=kip.VisualRoot.Find("DOTween Reactions/Core");
            for(int i=0;i<VoidToyArtGeometry.HeroFaces.Length;i++)
            {
                faces.TryGet(VoidToyArtGeometry.Hero+"/"+VoidToyArtGeometry.HeroFaces[i],out var face);
                foreach(var part in face.rig)reactions.Find(part.name).GetComponent<MeshFilter>().sharedMesh=part.mesh;
                camera.transform.rotation=Quaternion.Euler(4,180,0);camera.orthographicSize=.3f;var screen=reactions.Find("Mouth").position+Vector3.up*.07f;camera.transform.position=screen-camera.transform.forward*20;
                title.text=VoidToyArtGeometry.HeroFaces[i].ToUpperInvariant();title.transform.rotation=camera.transform.rotation;title.transform.position=screen+Vector3.up*.25f+Vector3.forward*.3f;title.characterSize=.009f;
                var image=Grab(camera,480,480);sheet.SetPixels(i*480,0,480,480,image.GetPixels());Object.DestroyImmediate(image);title.characterSize=.032f;
            }
            foreach(var part in hero.rig)if(VoidToyArtGeometry.HeroFaceParts.Contains(part.name))reactions.Find(part.name).GetComponent<MeshFilter>().sharedMesh=part.mesh;
            sheet.Apply();Write(sheet,Previews+"/RogueCube_Review.png");Object.DestroyImmediate(title.gameObject);
            // 2. In-game: 1080x1920, ortho 5.6, 45/45 camera, light floor over the dark void, six graphite enemies.
            var arena=new GameObject("Combat cube").transform;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(arena.gameObject,scene);
            Transform Prop(string key,Vector3 at,Vector3? scale=null)
            {var obj=Load(key,scene);obj.transform.SetParent(arena,false);obj.transform.localPosition=at;if(scale.HasValue)obj.transform.localScale=scale.Value;return obj.transform;}
            Prop("Deep sides",new Vector3(0,-1.2f,0));Prop("Arena surface",new Vector3(0,-.04f,0));
            for(int i=0;i<4;i++)
            {
                bool alongX=i<2;var edge=Prop("Edge light",new Vector3(alongX?0:(i==2?-4.45f:4.45f),.03f,alongX?(i==0?-4.45f:4.45f):0),alongX?new Vector3(9,.06f,.07f):new Vector3(.07f,.06f,9));
                var block=new MaterialPropertyBlock();block.SetColor("_Color",VoidToyPalette.Cyan);edge.GetComponent<Renderer>().SetPropertyBlock(block);
            }
            var at=new Vector3(0,.65f,-.4f);kip.transform.position=at;kip.transform.rotation=Quaternion.Euler(0,20,0);kip.TickPose(Vector3.zero,.001f,false);
            string[] cast={"Grunt","Sprinter","Gunner","Pusher","Sniper","Bruiser"};
            for(int i=0;i<cast.Length;i++)
            {
                float a=(-70+i*28)*Mathf.Deg2Rad;var position=at+new Vector3(Mathf.Sin(a)*2.6f,-.05f,Mathf.Cos(a)*2.6f+.4f);
                var foe=Spawn(cast[i],position,0);foe.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(at-position,Vector3.up));foe.TickPose(Vector3.zero,.001f,false);
            }
            var game=Camera(scene,VoidToyPalette.Hex(0x0E1626));game.transform.rotation=Quaternion.Euler(45,45,0);game.orthographicSize=5.6f;game.aspect=1080f/1920;
            game.transform.position=at+Vector3.forward*.8f-game.transform.forward*30;
            var shot=Grab(game,1080,1920);Write(shot,Previews+"/RogueCube_InGame.png");Object.DestroyImmediate(shot);
            // Screen share of the character's height: foot on the floor to the antenna ball top.
            var foot=new Vector3(at.x,.04f,at.z);float low=game.WorldToViewportPoint(foot).y,high=game.WorldToViewportPoint(foot+Vector3.up*1.25f).y;
            // 3. Rig sanity with the current animator: blink, stride, recoil, hit; root and fire anchor stay put.
            kip.transform.position=Vector3.right*20;var root=kip.transform.position;var anchor=kip.transform.Find("Pulse pistol");var anchorAt=anchor.localPosition;bool blinked=false,strode=false;
            for(int frame=0;frame<144;frame++)
            {
                bool run=frame>=40;if(run&&frame%8==0)kip.Shoot();if(frame==100)kip.Telegraph(.5f);if(frame==120)kip.Hit();
                kip.TickPose(frame<24?Vector3.zero:Vector3.forward*(run?4.5f:1.2f),1f/24,run);
                foreach(var node in kip.VisualRoot.GetComponentsInChildren<Transform>()) {if(node.name.EndsWith("Eye")&&node.localScale.y<.4f)blinked=true;if(node.name.EndsWith("Leg")&&Quaternion.Angle(node.localRotation,Quaternion.identity)>5)strode=true;}
                if(kip.transform.position!=root||anchor.localPosition!=anchorAt||!float.IsFinite(kip.VisualRoot.localPosition.y))throw new Exception("Rogue cube animation moved root / fire anchor");
            }
            if(!blinked||!strode)throw new Exception("Rogue cube blink / stride missing");
            Debug.Log($"CUBE_ROGUECUBE_REVIEW_OK: {Previews}/RogueCube_Review.png, RogueCube_InGame.png; KIP spans {(high-low)*100:F1}% of the 1080x1920 frame height at ortho 5.6 (brief: >= 6%); blink and stride OK with the current animator.");
        }
        finally
        {
            if(sheet)Object.DestroyImmediate(sheet);
            foreach(var animator in animators)if(animator)animator.SendMessage("OnDestroy",SendMessageOptions.DontRequireReceiver);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
