using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Implementation of InputModule. Based on mouse and keyboard input.
    /// </summary>
    class ClickInputModule : InputModule
    {


        public override bool DeleteInput()
        {
            return MoveEnabled && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace));
        }

        public override bool DeselectInput()
        {
            return Input.GetKeyUp(KeyCode.Escape);
        }

        public override Ray GetRay(Camera camera)
        {
            return camera.ScreenPointToRay(Input.mousePosition);
        }

        public override bool HelpInput()
        {
            return MoveEnabled && (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.H));
        }

        public override Vector3 InputPoint()
        {
            return Input.mousePosition;
        }

        public override short MenuInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
                return InputStart;
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                return InputNow;
            else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
                return InputEnd;
            else
                return InputNone;
        }

        public override bool MultipleSelectionInput()
        {
            return ShiftInput();
        }

        public override bool OptionsInput()
        {
            return MoveEnabled && Input.GetKeyDown(KeyCode.O);
        }

        public override short PanningInput()
        {
            if (PickerCamera.MouseButtonDown(2))
                return InputStart;
            else if (Input.GetMouseButton(2))
                return InputNow;
            else if (Input.GetMouseButtonUp(2))
                return InputEnd;
            else
                return InputNone;
        }

        public override bool PhysicsPick()
        {
            return !EventSystem.current.IsPointerOverGameObject(-1) && PickingEnabled && Input.GetMouseButtonUp(0);
        }

        public override bool ScreenshotInput()
        {
            return false;
        }

        public override bool ShiftInput()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public override short WalkerInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.LeftCommand) || Input.GetKeyDown(KeyCode.RightCommand))
                return InputStart;
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))
                return InputNow;
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.LeftCommand) || Input.GetKeyUp(KeyCode.RightCommand))
                return InputEnd;
            else
                return InputNone;
        }
    }
}
