using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        sealed class ActorShell {public Transform body;public ToyActorAnimator animation;}
        readonly Dictionary<string,Stack<ActorShell>> actorPools=new Dictionary<string,Stack<ActorShell>>();
        readonly Stack<Transform> coinPool=new Stack<Transform>(),healthPool=new Stack<Transform>();
        readonly Dictionary<Transform,List<Tween>> pickupTweens=new Dictionary<Transform,List<Tween>>();
        readonly List<MeshRenderer> deathRenderScratch=new List<MeshRenderer>();
        readonly List<Transform> deathShells=new List<Transform>();
        Transform poolRoot;
        Transform PoolRoot
        {get {if(!poolRoot) {poolRoot=new GameObject("Reusable actors and pickups").transform;poolRoot.SetParent(world,false);}return poolRoot;}}
        bool RentActor(string key,out Transform body,out ToyActorAnimator animation)
        {
            body=null;animation=null;if(!Expanded||!actorPools.TryGetValue(key,out var pool))return false;
            while(pool.Count>0)
            {
                var shell=pool.Pop();if(!shell.body)continue;body=shell.body;animation=shell.animation;
                foreach(var part in body.GetComponentsInChildren<Transform>(true))if(part.name=="Elite crown") {part.gameObject.SetActive(false);Destroy(part.gameObject);}body.SetParent(dynamicRoot,false);body.gameObject.SetActive(true);animation?.ResetForReuse(body);return true;
            }
            return false;
        }
        bool RecycleActor(Foe foe,bool animate)
        {
            if(!Expanded||foe.kind==EnemyKind.Crusher||!foe.body)return false;
            if(animate)PooledDeath(foe);
            string key=foe.body.GetComponent<MeshFilter>().sharedMesh.name;
            // The authored key is stored on the root; mesh names can differ from library keys.
            key=foe.body.gameObject.GetComponent<ActorPoolIdentity>()?.key??foe.body.name;
            if(!actorPools.TryGetValue(key,out var pool))actorPools[key]=pool=new Stack<ActorShell>();
            foe.body.gameObject.SetActive(false);foe.body.SetParent(PoolRoot,false);pool.Push(new ActorShell {body=foe.body,animation=foe.animation});return true;
        }
        void PooledDeath(Foe foe)
        {
            Transform shell=null;foreach(var candidate in deathShells)if(candidate&&!candidate.gameObject.activeSelf) {shell=candidate;break;}
            if(!shell) {shell=new GameObject("Pooled actor death").transform;shell.SetParent(PoolRoot,false);deathShells.Add(shell);}
            var sources=foe.animation?foe.animation.VisualRoot.GetComponentsInChildren<MeshRenderer>():foe.body.GetComponentsInChildren<MeshRenderer>();
            deathRenderScratch.Clear();foreach(var source in sources)if(source.GetComponent<MeshFilter>())deathRenderScratch.Add(source);var renderers=deathRenderScratch;
            while(shell.childCount<renderers.Count)new GameObject("Death fragment",typeof(MeshFilter),typeof(MeshRenderer)).transform.SetParent(shell,false);
            shell.position=foe.body.position;shell.rotation=Quaternion.identity;shell.localScale=Vector3.one;shell.gameObject.SetActive(true);
            for(int i=0;i<shell.childCount;i++)
            {
                var child=shell.GetChild(i);child.gameObject.SetActive(i<renderers.Count);if(i>=renderers.Count)continue;var source=renderers[i];child.GetComponent<MeshFilter>().sharedMesh=source.GetComponent<MeshFilter>().sharedMesh;child.GetComponent<MeshRenderer>().sharedMaterials=source.sharedMaterials;child.position=source.transform.position;child.rotation=source.transform.rotation;child.localScale=source.transform.lossyScale;
            }
            if(Mathf.Abs(foe.body.position.x-centre.x)>ArenaHalfX||Mathf.Abs(foe.body.position.z-centre.z)>Half)WorldTween(shell.DOMoveY(shell.position.y-3,.25f).SetEase(Ease.InQuad));
            var captured=shell;WorldTween(shell.DOScale(Vector3.zero,.25f).SetEase(Ease.InBack)).OnComplete(()=> {if(captured)captured.gameObject.SetActive(false);});
        }
        void RecyclePickup(Pickup pickup)
        {
            if(!pickup.body)return;
            if(!Expanded) {Destroy(pickup.body.gameObject);return;}
            if(pickupTweens.TryGetValue(pickup.body,out var list)) {foreach(var tween in list)tween.Kill();pickupTweens.Remove(pickup.body);}
            pickup.body.gameObject.SetActive(false);pickup.body.SetParent(PoolRoot,false);(pickup.healing?healthPool:coinPool).Push(pickup.body);
        }
        void RecycleActiveActors()
        {
            if(!Expanded)return;
            foreach(var foe in foes) {SupportEliminated(foe);RecycleActor(foe,false);}
            foreach(var shell in deathShells)if(shell)shell.gameObject.SetActive(false);
        }
    }
}
