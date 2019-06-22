using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Sizes to be saved in panels.txt.
    /// </summary>
    public class ScrollSizes
    {
        public float PanelX;
        public float PanelY;
        public float HeaderX;
        public float ContentX;
        public float ScrollY;
    }

    /// <summary>
    /// Component which enables resizing of scrollarea. It can be resized min to 100x100 or 
    /// max to size of screen. It also resizes inner components in ScrollRect. When mouse is
    /// over, cursor is changed.
    /// </summary>
    public class ResizeScroll : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // Range of sizes.
        public Vector2 minSize = new Vector2(100, 100);
        public Vector2 maxSize = new Vector2(Screen.width, Screen.height);
        // Should its sizes be saved in file.
        public bool saveToFile = true;
        // Transforms changed by resizing.
        public RectTransform ResizePanel;
        public RectTransform ResizeHeaderX;
        public RectTransform ResizeContentX;
        public RectTransform ResizeScrollareaY;
        // Sizes and position on start of resizing.
        private Vector2 m_OriginalLocalPointerPosition;
        private Vector2 m_OriginalSizeDelta;


        /// <summary>
        /// If saving to file enabled, register panel and if it already exists,
        /// read its saved sizes.
        /// </summary>
        void Start()
        {
            if (saveToFile)
            {
                // Current sizes
                ScrollSizes sizes = new ScrollSizes();
                sizes.PanelX = ResizePanel.sizeDelta.x;
                sizes.PanelY = ResizePanel.sizeDelta.y;
                sizes.ScrollY = ResizeScrollareaY.sizeDelta.y;
                sizes.ContentX = ResizeContentX.sizeDelta.x;
                sizes.HeaderX = ResizeHeaderX.sizeDelta.x;
                // Register and get sizes
                sizes = GuiManager.Instance.RegisterPanelSize(ResizePanel.name, sizes);
                // Apply sizes
                ResizePanel.sizeDelta = new Vector2(sizes.PanelX, sizes.PanelY);
                ResizeScrollareaY.sizeDelta = new Vector2(ResizeScrollareaY.sizeDelta.x, sizes.ScrollY);
                ResizeContentX.sizeDelta = new Vector2(sizes.ContentX, ResizeContentX.sizeDelta.y);
                ResizeHeaderX.sizeDelta = new Vector2(sizes.HeaderX, ResizeHeaderX.sizeDelta.y);
            }
        }

        /// <summary>
        /// Brings window to front and sets initial size and position.
        /// </summary>
        /// <param name="data">event data</param>
        public void OnPointerDown(PointerEventData data)
        {
            // Bring window to front.
            ResizePanel.SetAsLastSibling();
            m_OriginalSizeDelta = ResizePanel.sizeDelta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ResizePanel, data.position, data.pressEventCamera, out m_OriginalLocalPointerPosition);
        }

        /// <summary>
        /// Resize the window relatively to initial position.
        /// </summary>
        /// <param name="data">event data</param>
        public void OnDrag(PointerEventData data)
        {
            if (ResizePanel == null)
                return;

            Vector2 localPointerPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ResizePanel, data.position, data.pressEventCamera, out localPointerPosition);
            Vector3 offsetToOriginal = localPointerPosition - m_OriginalLocalPointerPosition;

            Vector2 sizeDelta = m_OriginalSizeDelta + new Vector2(offsetToOriginal.x, -offsetToOriginal.y);
            sizeDelta = new Vector2(
                Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
                Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
            );

            ResizePanel.sizeDelta = sizeDelta;
            ResizeScrollareaY.sizeDelta = new Vector2(ResizeScrollareaY.sizeDelta.x, sizeDelta.y - 40);
            ResizeContentX.sizeDelta = new Vector2(sizeDelta.x - 18, ResizeContentX.sizeDelta.y - 20);
            ResizeHeaderX.sizeDelta = new Vector2(sizeDelta.x, ResizeHeaderX.sizeDelta.y);
        }

        /// <summary>
        /// If enabled, saves new sizes to panels.txt.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            // Save position to file
            if (saveToFile)
            {
                ScrollSizes sizes = new ScrollSizes();
                sizes.PanelX = ResizePanel.sizeDelta.x;
                sizes.PanelY = ResizePanel.sizeDelta.y;
                sizes.ScrollY = ResizeScrollareaY.sizeDelta.y;
                sizes.ContentX = ResizeContentX.sizeDelta.x;
                sizes.HeaderX = ResizeHeaderX.sizeDelta.x;
                GuiManager.Instance.PanelChanged(ResizePanel.name, sizes);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.dragging)
                Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector3.zero, CursorMode.Auto);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.dragging)
                Cursor.SetCursor(GuiManager.Instance.CursorCorner, Vector2.one * 31.0f, CursorMode.Auto);
        }
    }
}
