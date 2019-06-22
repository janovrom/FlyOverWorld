using Assets.Scripts.Agents;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Component, which allows to move dragged game object before the one
    /// on which we drop it.
    /// </summary>
    class DragMoveBefore : MonoBehaviour, IDropHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {

        public string MissionName;

        /// <summary>
        /// Currently dragged object. Dragged object is static, since it can be only
        /// one at the time and this way it is allowed to share it between other instances.
        /// </summary>
        private static DragMoveBefore Dragged = null;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.DragSprite, Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.dragging && eventData.selectedObject != gameObject)
                Cursor.SetCursor(GuiManager.Instance.DropSprite, Vector2.one * 16.0f, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.dragging && eventData.selectedObject != gameObject)
                Cursor.SetCursor(GuiManager.Instance.DragSprite, Vector2.zero, CursorMode.Auto);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// Checks dragged object and if it is not null, move the drag item in hierarchy before
        /// drop area. Drop area is only another DragMoveBefore. Also informs mission about its 
        /// change.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnDrop(PointerEventData eventData)
        {
            // Check if we have the same parent
            // Lost in parents? Look on Mission window prefab
            if (Dragged && Dragged.transform.parent.parent == this.transform.parent.parent)
            {
                Mission m = AgentManager.Instance.GetMission(MissionName);
                // We dragged another DragMoveBefore
                int beforeIdx = -1;
                int myIdx = -1;
                for (int i = 0; i < transform.parent.parent.childCount; ++i)
                {
                    // Find our position
                    // Why -1? We have text as first in hierarchy
                    if (this.transform.parent.parent.GetChild(i) == this.transform.parent)
                    {
                        // this is object before which we should drop it
                        beforeIdx = i - 1;
                    }
                    else if (this.transform.parent.parent.GetChild(i) == Dragged.transform.parent)
                    {
                        myIdx = i - 1;
                    }
                }
                Dragged.transform.parent.SetSiblingIndex(beforeIdx + 1);
                m.MoveWaypointBefore(myIdx, beforeIdx);
            } // Dragging something else, ignore it
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
            Dragged = null;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// Initializes currently dragged object. Dragged object is static, since it can be only
        /// one at the time and this way it is allowed to share it between other instances.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            Dragged = this;
        }
    }
}
