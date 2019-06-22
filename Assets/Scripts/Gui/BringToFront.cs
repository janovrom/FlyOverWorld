using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Component, which sets assigned object as last in parent's transform hierarchy
    /// and this way bringing it to the front. It is called when pointer is down.
    /// </summary>
    class BringToFront : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }
    }
}
