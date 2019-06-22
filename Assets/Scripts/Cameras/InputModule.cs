using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Cameras
{

    /// <summary>
    /// Abstract class defining user input handling and state constants.
    /// User input is specified by action, which it solves - ie. MenuInput().
    /// </summary>
    abstract class InputModule
    {

        public const short InputStart = 0;
        public const short InputNow = 1;
        public const short InputEnd = 2;
        public const short InputNone = 3;

        /// <summary>
        /// If we are editing in one of input fields.
        /// </summary>
        internal bool m_IsInputFocused = false;

        /// <summary>
        /// If we can pick objects.
        /// </summary>
        internal bool m_IsPickingEnabled = true;

        /// <summary>
        /// Based on input method creates new ray from point on screen.
        /// </summary>
        /// <param name="camera">Camera, from which the ray will be casted.</param>
        /// <returns>Returns Ray from mouse or touch location on screen.</returns>
        public abstract Ray GetRay(Camera camera);

        /// <summary>
        /// Checks for input assigned to screenshot acquisition.
        /// </summary>
        /// <returns>Returns true if screenshot should be captured.</returns>
        public abstract bool ScreenshotInput();

        /// <summary>
        /// Checks for input assigned to help.
        /// </summary>
        /// <returns>Returns true if help should be handled.</returns>
        public abstract bool HelpInput();

        /// <summary>
        /// Checks for input assigned to options.
        /// </summary>
        /// <returns>Returns true if options should be handled.</returns>
        public abstract bool OptionsInput();

        /// <summary>
        /// Checks for input assigned to Delete.
        /// </summary>
        /// <returns>Returns true if Delete should be handled.</returns>
        public abstract bool DeleteInput();

        /// <summary>
        /// Checks for input assigned to Deselect.
        /// </summary>
        /// <returns>Returns true if Deselect should be handled.</returns>
        public abstract bool DeselectInput();

        /// <summary>
        /// Checks for input for walker camera.
        /// </summary>
        /// <returns>Returns InputStart, InputNow or InputOn based on action done.</returns>
        public abstract short WalkerInput();

        /// <summary>
        /// Checks for input for Menu.
        /// </summary>
        /// <returns>Returns InputStart, InputNow or InputOn based on action done.</returns>
        public abstract short MenuInput();

        /// <summary>
        /// Return point on screen where we touch or where mouse is.
        /// </summary>
        /// <returns>Returns point of touch or mouse position.</returns>
        public abstract Vector3 InputPoint();

        /// <summary>
        /// Checks for input for panning.
        /// </summary>
        /// <returns>Returns InputStart, InputNow or InputOn based on action done.</returns>
        public abstract short PanningInput();

        /// <summary>
        /// Returns true, if physics raycast can be done. UI blocks it.
        /// </summary>
        /// <returns></returns>
        public abstract bool PhysicsPick();

        /// <summary>
        /// Checks if we should select multiple objects.
        /// </summary>
        /// <returns>Returns true iff we can select multiple objects.</returns>
        public abstract bool MultipleSelectionInput();

        /// <summary>
        /// Checks if we are pressing left or right shift.
        /// </summary>
        /// <returns>Returns true iff we are pressing left or right shift.</returns>
        public abstract bool ShiftInput();

        /// <summary>
        /// Returns true, if we can move in scene, which is banned
        /// when input field is focused.
        /// </summary>
        public bool MoveEnabled
        {
            get
            {
                return !m_IsInputFocused;
            }
        }

        /// <summary>
        /// Update InputModule state. When used, should be called every 
        /// Unity Update().
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// True iff we can pick objects.
        /// </summary>
        public bool PickingEnabled
        {
            get
            {
                return m_IsPickingEnabled;
            }

            set
            {
                m_IsPickingEnabled = value;
            }
        }


    }
}
