using UnityEngine;
using UnityEngine.EventSystems;
namespace CubeInvasion.Prototype
{
        // Free-hand stick: it appears under the thumb wherever the touch lands and fades out on release.
        public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public Vector2 Value { get; private set; }
            public RectTransform knob;
            int finger=int.MinValue;Vector2 home;bool homed;CanvasGroup group;
            public bool Held => finger!=int.MinValue;
            public void OnPointerDown(PointerEventData data)
            {
                if(finger!=int.MinValue)return;finger=data.pointerId;
                var rect=(RectTransform)transform;if(!homed) {homed=true;home=rect.anchoredPosition;}
                if(rect.parent is RectTransform parent&&RectTransformUtility.ScreenPointToWorldPointInRectangle(parent,data.position,data.pressEventCamera,out var world))rect.position=world;
                Show(true);OnDrag(data);
            }
            public void OnDrag(PointerEventData data)
            {
                if(data.pointerId!=finger)return;
                var rect=(RectTransform)transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var point);
                Value=Vector2.ClampMagnitude(point/(rect.rect.width*.38f),1);
                knob.anchoredPosition=Value*(rect.rect.width*.28f);
            }
            public void OnPointerUp(PointerEventData data) { if(data.pointerId==finger)ResetInput(); }
            public void ResetInput()
            {
                finger=int.MinValue;Value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;
                if(homed)((RectTransform)transform).anchoredPosition=home;Show(false);
            }
            void Start() { Show(false); }
            void Show(bool visible)
            {
                if(!group) {group=GetComponent<CanvasGroup>();if(!group)group=gameObject.AddComponent<CanvasGroup>();}
                group.alpha=visible?1:0;
            }
            void OnDisable() { ResetInput(); }
        }

        // Transparent catch-all behind the HUD buttons: any touch on open screen space becomes the stick's centre.
        public sealed class JoystickTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public MobileJoystick stick;
            public void OnPointerDown(PointerEventData data) { if(stick)stick.OnPointerDown(data); }
            public void OnDrag(PointerEventData data) { if(stick)stick.OnDrag(data); }
            public void OnPointerUp(PointerEventData data) { if(stick)stick.OnPointerUp(data); }
        }
}
