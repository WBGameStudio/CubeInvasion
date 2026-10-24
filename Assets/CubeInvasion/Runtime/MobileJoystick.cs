using UnityEngine;
using UnityEngine.EventSystems;
namespace CubeInvasion.Prototype
{
        public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public Vector2 Value { get; private set; }
            public RectTransform knob;
            int finger=int.MinValue;
            public void OnPointerDown(PointerEventData data) { if(finger!=int.MinValue)return;finger=data.pointerId;OnDrag(data); }
            public void OnDrag(PointerEventData data)
            {
                if(data.pointerId!=finger)return;
                var rect=(RectTransform)transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var point);
                Value=Vector2.ClampMagnitude(point/(rect.rect.width*.38f),1);
                knob.anchoredPosition=Value*(rect.rect.width*.28f);
            }
            public void OnPointerUp(PointerEventData data) { if(data.pointerId==finger)ResetInput(); }
            public void ResetInput() { finger=int.MinValue;Value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero; }
            void OnDisable() { ResetInput(); }
        }
}
