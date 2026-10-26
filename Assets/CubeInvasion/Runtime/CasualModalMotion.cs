using UnityEngine;
using DG.Tweening;
namespace CubeInvasion.Prototype
{
    public sealed class CasualModalMotion : MonoBehaviour
    {
        [SerializeField] GameObject backdrop;
        public void ConfigureBackdrop(GameObject value) { backdrop=value; }
        Tween motion;
        CanvasGroup group;
        void OnEnable()
        {
            if(!Application.isPlaying)return;
            if(backdrop)backdrop.SetActive(true);
            group=GetComponent<CanvasGroup>();if(!group)group=gameObject.AddComponent<CanvasGroup>();group.alpha=0;transform.localScale=Vector3.one*.9f;
            motion=DOTween.Sequence().Append(group.DOFade(1,.16f)).Join(transform.DOScale(1,.28f).SetEase(Ease.OutBack)).SetUpdate(true);
        }
        void OnDisable() { if(backdrop)backdrop.SetActive(false);motion?.Kill();transform.localScale=Vector3.one;if(group)group.alpha=1; }
        void OnDestroy() { motion?.Kill(); }
    }
}
