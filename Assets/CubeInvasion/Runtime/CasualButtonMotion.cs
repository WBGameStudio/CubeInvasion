using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
namespace CubeInvasion.Prototype
{
    [RequireComponent(typeof(Button))]
    public sealed class CasualButtonMotion : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ICancelHandler
    {
        Tween motion;
        Button button;
        bool pressed;
        void Awake() { button=GetComponent<Button>(); }
        public void OnPointerDown(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||!button||!button.IsInteractable())return;
            pressed=true;motion?.Kill();motion=transform.DOScale(.95f,.08f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
        public void OnPointerUp(PointerEventData e)=>Release();
        public void OnPointerExit(PointerEventData e)=>Release();
        public void OnCancel(BaseEventData e)=>Release();
        void Release()
        { if(!isActiveAndEnabled||!pressed)return;pressed=false;motion?.Kill();motion=transform.DOScale(1,.22f).SetEase(Ease.OutBack).SetUpdate(true); }
        void OnDisable() { pressed=false;motion?.Kill();transform.localScale=Vector3.one; }
        void OnDestroy() { motion?.Kill(); }
    }
}
