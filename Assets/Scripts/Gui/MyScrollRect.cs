using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Child of ScrollRect, which disables draggin in scroll area.
    /// Now it can be only scrolled using middle wheel or dragging
    /// of scrollbar.
    /// </summary>
    class MyScrollRect : ScrollRect
    {

        public override void OnBeginDrag(PointerEventData eventData) { }

        public override void OnDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }

    }
}
