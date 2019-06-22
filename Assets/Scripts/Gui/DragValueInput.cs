using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Component, which when attached to input field for decimal numbers, 
    /// that can change value of the input field by dragging left or right.
    /// After each change, onEndEdit is called. Minimum and maximum can
    /// be specified and respective default values are 0 and +Infinity.
    /// </summary>
    class DragValueInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        public InputField Input;
        public float Min = 0.0f;
        public float Max = float.PositiveInfinity;


        void Start()
        {
            Input = GetComponent<InputField>();
            if (Input.contentType != InputField.ContentType.DecimalNumber)
                Debug.LogError("DragValueInput assigned to wrong ContentType input");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorDragValue, Vector2.one * 16.0f, CursorMode.Auto);
        }

        /// <summary>
        /// Change value of input field if it is decimal. Value is changed by 0.25f/px.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnDrag(PointerEventData eventData)
        {
            float val;
            if (float.TryParse(Input.text, out val)) 
            {
                // Four pixels are 1 unit
                val += eventData.delta.x / 4.0f;
                val = Mathf.Max(val, Min);
                val = Mathf.Min(val, Max);
                Input.text = (val).ToString("0.0");
                Input.onEndEdit.Invoke("");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);
        }

    }
}
