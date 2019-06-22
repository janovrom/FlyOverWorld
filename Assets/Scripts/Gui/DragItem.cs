using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Enables item to be dragged and dropped on DropZone or another DropItem.
    /// When dropped on zone, it is added, and when dropped on item, new misson 
    /// is created. Also changes cursor based on possible actions and current state.
    /// </summary>
    class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IDropHandler
    {

        /// <summary>
        /// Pickable assigned to this drag item.
        /// </summary>
        public Pickable DraggedObject;


        public void OnBeginDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.DragSprite, Vector2.zero, CursorMode.Auto);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// Create new mission from cameras selection when object is dragged on another.
        /// </summary>
        /// <param name="eventData">event data assigned to this event</param>
        public void OnDrop(PointerEventData eventData)
        {
            // Get item that is dragged
            DragItem di = eventData.pointerDrag.gameObject.GetComponent<DragItem>();
            if (di)
            {
                // Add these two dragged objects to selection, if any exists
                Cameras.PickerCamera cam = FindObjectOfType<Cameras.PickerCamera>();
                Menu menu = cam.PopUpMenu.GetComponent<Menu>();
                cam.AddToSelection(di.DraggedObject);
                cam.AddToSelection(this.DraggedObject);
                string name = Agents.Mission.GetMissionName(cam.Selection.ToArray());
                // Create mission based on drop object
                menu.CreateMission(name);
            }
        }
    }
}
