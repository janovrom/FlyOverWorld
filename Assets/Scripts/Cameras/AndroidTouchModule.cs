using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Implementation of InputModule for Android based on touch.
    /// </summary>
    class AndroidTouchModule : InputModule
    {

        /// <summary>
        /// Recognize up to 5 fingers touch times.
        /// </summary>
        private float[] m_InputTouchTime = new float[5];


        /// <summary>
        /// Updates touch times for up to 5 fingers.
        /// </summary>
        public override void Update()
        {
            base.Update();
            int i = 0;
            // Update each input times
            for (; i < Input.touchCount; ++i)
            {
                m_InputTouchTime[i] += Input.GetTouch(i).deltaTime;
            }
            // Reset all other input times
            for (; i < 5; ++i)
            {
                m_InputTouchTime[i] = 0.0f;
            }
        }

        public override bool DeleteInput()
        {
            return false;
        }

        public override bool DeselectInput()
        {
            return false;
        }

        public override Ray GetRay(Camera camera)
        {
            return camera.ScreenPointToRay(Input.GetTouch(0).position);
        }

        public override bool HelpInput()
        {
            return false;
        }

        public override Vector3 InputPoint()
        {
            return Input.GetTouch(0).position;
        }

        public override short MenuInput()
        {
            if (Input.touchCount < 1)
                return InputNone;
            else if (m_InputTouchTime[0] > 0.5f)
                return InputStart;
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                return InputEnd;
            else
                return InputNone;
        }

        public override bool MultipleSelectionInput()
        {
            return true;
        }

        public override bool OptionsInput()
        {
            return false;
        }

        public override short PanningInput()
        {
            if (Input.touchCount < 1 || !PickingEnabled || EventSystem.current.IsPointerOverGameObject(0))
                return InputNone;
            else if (Input.GetTouch(0).phase == TouchPhase.Began)
                return InputStart;
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                return InputNow;
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                return InputEnd;
            else
                return InputNone;
        }

        public override bool PhysicsPick()
        {
            return Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(0) && PickingEnabled && Input.GetTouch(0).phase == TouchPhase.Began;
        }

        public override bool ScreenshotInput()
        {
            return false;
        }

        public override bool ShiftInput()
        {
            return false;
        }

        public override short WalkerInput()
        {
            return InputNone;
        }
    }
}
