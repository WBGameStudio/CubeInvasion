using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CubeInvasion.Prototype
{
    public sealed partial class InvasionPrototype
    {
        ToyActorAnimator playerAnimation;
        readonly List<Tween> worldTweens=new List<Tween>();
        ToyActorAnimator AnimateActor(Transform body,Transform motionRoot,string key,params Transform[] accessories)
        {
            if(!artLibrary||!artLibrary.TryGet(key,out var model)||model.rig==null||model.rig.Length==0)return null;
            var animator=body.gameObject.AddComponent<ToyActorAnimator>();animator.Bind(model,motionRoot);
            foreach(var accessory in accessories)animator.Attach(accessory);
            return animator;
        }
        T WorldTween<T>(T tween) where T:Tween
        {
            tween.SetUpdate(UpdateType.Manual);worldTweens.Add(tween);return tween;
        }
        void TickAnimations(float dt)
        {
            bool active=phase==Phase.Combat||phase==Phase.Choice||phase==Phase.Transfer;
            if(playerAnimation)playerAnimation.TickFromRoot(dt,paused,active,phase==Phase.Combat&&currentTarget!=null);
            foreach(var foe in foes)if(foe.animation)foe.animation.TickFromRoot(dt,paused,phase==Phase.Combat,foe.kind==EnemyKind.Gunner);
            if(paused)return;
            for(int i=worldTweens.Count-1;i>=0;i--)
            {
                var tween=worldTweens[i];
                if(!tween.IsActive()) {worldTweens.RemoveAt(i);continue;}
                tween.ManualUpdate(dt,dt);
            }
        }
        void AnimateDeath(Foe foe)
        {
            if(!foe.animation)return;
            var visual=foe.animation.DetachVisual(dynamicRoot?dynamicRoot:world);if(!visual)return;
            if(foe.animation.IsMechanical)
            {
                // The visual shell collapses, while the gameplay root can be removed immediately.
                var shatter=DOTween.Sequence().Join(visual.DOMoveY(visual.position.y-.18f,.24f).SetEase(Ease.InQuad))
                    .Join(visual.DORotate(visual.eulerAngles+new Vector3(42,0,18),.24f)).Append(visual.DOScale(Vector3.zero,.14f).SetEase(Ease.InBack));
                WorldTween(shatter).OnComplete(()=> {if(visual)Destroy(visual.gameObject);}).SetLink(visual.gameObject);return;
            }
            var sequence=DOTween.Sequence().Join(visual.DOScale(Vector3.zero,.22f).SetEase(Ease.InBack))
                .Join(visual.DORotate(visual.eulerAngles+new Vector3(20,0,20),.22f));
            WorldTween(sequence).OnComplete(()=> {if(visual)Destroy(visual.gameObject);}).SetLink(visual.gameObject);
        }
        void AnimatePickup(Transform pickup)
        {
            WorldTween(pickup.DOMoveY(pickup.position.y+.12f,.6f).SetEase(Ease.InOutSine).SetLoops(-1,LoopType.Yoyo)).SetLink(pickup.gameObject);
            WorldTween(pickup.DOLocalRotate(new Vector3(0,360,0),1.6f,RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1)).SetLink(pickup.gameObject);
        }
        void ClearAnimationTweens()
        {
            foreach(var tween in worldTweens)tween.Kill();worldTweens.Clear();
        }
    }
}
