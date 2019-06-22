using Assets.Scripts.Agents;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.Cameras;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Area in which DragItem can be dropped. It handles adding billboards or labels 
    /// to missions.
    /// </summary>
    class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        /// <summary>
        /// Assigned mission text. Can be null, since used is MissionNameStr.
        /// </summary>
        public UnityEngine.UI.Text MissionName;

        /// <summary>
        /// Name of mission to be assigned.
        /// </summary>
        public string MissionNameStr;
        private PickerCamera m_Camera;


        /// <summary>
        /// Initialize MissionNameStr if we have assigned Text MissionName.
        /// </summary>
        void Start()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
            if (MissionName != null)
                MissionNameStr = MissionName.text;
        }

        /// <summary>
        /// If dragged object is DragItem, take its dragged pickable and assign to mission.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            Mission m = AgentManager.Instance.GetMission(MissionNameStr);
            DragItem di = eventData.pointerDrag.gameObject.GetComponent<DragItem>();
            if (di)
            {
                // If it is drag item, drop it
                Pickable p = di.DraggedObject;
                if (!m_Camera.Selection.Contains(p))
                    AddPickable(p, m);
                foreach (Pickable o in m_Camera.Selection)
                {
                    AddPickable(o, m);
                }
            }
        }

        private void AddPickable(Pickable p, Mission m)
        {
            m.Add(p);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Check if we are not draggin itself or if we really drag the right item
            if (eventData.dragging && eventData.selectedObject != gameObject && eventData.pointerDrag.gameObject.GetComponent<DragItem>())
                Cursor.SetCursor(GuiManager.Instance.DropSprite, Vector2.one * 16.0f, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.dragging && eventData.selectedObject != gameObject)
                Cursor.SetCursor(GuiManager.Instance.DragSprite, Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

    }
}
