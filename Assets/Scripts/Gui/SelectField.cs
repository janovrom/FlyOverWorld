using Assets.Scripts.Cameras;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Added to each InputField. When InputField is focused/selected, PickerCamera 
    /// is informed about InputField gaining/losing focus, so it can disable movement.
    /// </summary>
    class SelectField : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {

        PickerCamera m_Camera;

        void Awake()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
        }

        /// <summary>
        /// Free movement.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnDeselect(BaseEventData eventData)
        {
            m_Camera.OnFocusLost(name);
        }

        /// <summary>
        /// FOcus camera.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnSelect(BaseEventData eventData)
        {
            m_Camera.OnFocusGained(name);
        }

        /// <summary>
        /// When enter is pressed make sure to inform PickerCamera about losing focus.
        /// </summary>
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                m_Camera.OnFocusLost(name);
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                m_Camera.OnFocusLost(name);
        }

        /// <summary>
        /// FOcus camera.
        /// </summary>
        /// <param name="eventData">event data</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            m_Camera.OnFocusGained(name);
        }
    }
}
