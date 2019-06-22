using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Creates panel, which will move parent window on drag.
    /// The drag area is specified by parent of this window.
    /// </summary>
    public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {

        private Vector2 m_OriginalLocalPointerPosition;
        private Vector3 m_OriginalPanelLocalPosition;
        // Draggable window.
        private RectTransform m_PanelRectTransform;
        // Area where the window is dragged.
        private RectTransform m_ParentRectTransform;
        public bool saveToFile = true;


        void Start()
        {
            m_PanelRectTransform = transform.parent as RectTransform;
            m_ParentRectTransform = m_PanelRectTransform.parent as RectTransform;
            // If saving window position, register itself and get position if already saved.
            if (saveToFile)
            {
                m_PanelRectTransform.localPosition = GuiManager.Instance.RegisterPanel(m_PanelRectTransform);
            }
        }

        /// <summary>
        /// Moves window according to position of mouse.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (m_PanelRectTransform == null || m_ParentRectTransform == null)
                return;

            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - m_OriginalLocalPointerPosition;
                m_PanelRectTransform.localPosition = m_OriginalPanelLocalPosition + offsetToOriginal;
            }

            ClampToWindow();
        }

        // Clamp panel to area of parent
        void ClampToWindow()
        {
            Vector3 pos = m_PanelRectTransform.localPosition;

            Vector3 minPosition = m_ParentRectTransform.rect.min - m_PanelRectTransform.rect.min;
            Vector3 maxPosition = m_ParentRectTransform.rect.max - m_PanelRectTransform.rect.max;

            pos.x = Mathf.Clamp(m_PanelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(m_PanelRectTransform.localPosition.y, minPosition.y, maxPosition.y);

            m_PanelRectTransform.localPosition = pos;
        }

        /// <summary>
        /// Initialize starting position and also bring the window on top.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // Bring window to front.
            transform.parent.SetAsLastSibling();
            m_OriginalPanelLocalPosition = m_PanelRectTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, eventData.position, eventData.pressEventCamera, out m_OriginalLocalPointerPosition);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Save position to file
            if (saveToFile)
            {
                GuiManager.Instance.PanelChanged(m_PanelRectTransform.name, m_PanelRectTransform.localPosition);
            }
        }
    }

}