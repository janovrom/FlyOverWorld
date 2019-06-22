using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// This component can for given list of InputFields change 
    /// their focus by tab/shift+tab. What component is next depends
    /// on their order in given list. Tab moves to next input field
    /// and shift+tab move to previous input field. Movement is wrapped.
    /// </summary>
    class TabInputShifter : MonoBehaviour
    {

        public List<InputField> InputFields;
        private int m_Current = 0;
        private Cameras.PickerCamera m_Camera;


        void OnEnable()
        {
            m_Camera = FindObjectOfType<Cameras.PickerCamera>();
        }

        public void Reset()
        {
            m_Current = 0;
            InputFields[m_Current].Select();
            InputFields[m_Current].ActivateInputField();
            m_Camera.OnFocusGained("");
        }

        /// <summary>
        /// Selects input field on next position.
        /// Move can be wrapped to first.
        /// </summary>
        private void Next()
        {
            ++m_Current;
            if (m_Current >= InputFields.Count)
                m_Current = 0;
            InputFields[m_Current].Select();
            InputFields[m_Current].ActivateInputField();
            m_Camera.OnFocusGained("");
        }

        /// <summary>
        /// Selects input field on previous position.
        /// Move can be wrapped to last.
        /// </summary>
        private void Previous()
        {
            --m_Current;
            if (m_Current < 0)
                m_Current = InputFields.Count - 1;
            InputFields[m_Current].Select();
            InputFields[m_Current].ActivateInputField();
            m_Camera.OnFocusGained("");
        }

        /// <summary>
        /// Checks for input on tab or shift+tab and moves on next or previous
        /// respectively.
        /// </summary>
        void Update()
        {
            bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (isShift)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Previous();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    Next();
                }
            }
        }

    }
}
