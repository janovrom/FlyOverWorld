using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Cameras
{
    /// <summary>
    /// Class encapsulationg movement and picking and user input. Movement 
    /// and picking depends on camera State and user input depends on selected
    /// InputModule. Has four possible state: free camera, top down view, 
    /// looking from object and walking.
    /// </summary>
    public class PickerCamera : MonoBehaviour
    {
        // Transforms of gui elements, which reacts to camera change
        public RectTransform PopUpMenu;
        public RectTransform Compass;

        /// <summary>
        /// Main camera in scene.
        /// </summary>
        private Camera m_Camera;
        private World m_World;
        // Can be drones, ground targets, no-fly zones or surveillance areas
        private List<Gui.Pickable> m_SelectedObjects = new List<Gui.Pickable>();
        // Current state
        private State m_State;
        // All possible states
        private State m_TopState = new TopState();
        private State m_FreeState = new FreeState();
        private State m_CompassState = new CompassState();
        private State m_WalkerState = new WalkerState();
        
        /// <summary>
        /// Sphere showing to where will user teleport when walking selected.
        /// </summary>
        private GameObject m_WalkerPosition = null;

        /// <summary>
        /// Assigned input module: either use mouse or touch display.
        /// </summary>
        private InputModule m_InputModule;

        /// <summary>
        /// First terrain circle was generated.
        /// </summary>
        private bool m_Started = false;
        private CharacterController m_CC;


        // Initialization
        void Start()
        {
            // Set default state
            m_State = m_FreeState;

            m_World = GameObject.FindObjectOfType<World>();

            m_Camera = this.gameObject.GetComponentInChildren<Camera>();

            m_CC = GetComponentInParent<CharacterController>();

            // Assign input module
#if UNITY_ANDROID
            m_InputModule = new AndroidTouchModule();
#else
            m_InputModule = new ClickInputModule();
#endif
        }

        /// <summary>
        /// First deselects all objects and then select objects given as parameters.
        /// </summary>
        /// <param name="selected">Objects to select.</param>
        public void SelectOnlyObjects(params Gui.Pickable[] selected)
        {
            DeselectObjects();
            if (selected != null)
            {
                foreach (Gui.Pickable p in selected)
                {
                    // Select all gui elements
                    p.Select();
                    m_SelectedObjects.Add(p);
                }
            }
        }

        /// <summary>
        /// Selects given object if not selected.
        /// </summary>
        /// <param name="p">object to select</param>
        public void AddToSelection(Gui.Pickable p)
        {
            if (!m_SelectedObjects.Contains(p))
            {
                // Select all gui elements
                p.Select();
                m_SelectedObjects.Add(p);
            }
        }

        /// <summary>
        /// Selects, deselects or adds to selection given object.
        /// Depends on user input.
        /// </summary>
        /// <param name="selected">given object to select</param>
        public void SelectObject(Gui.Pickable selected)
        {
            bool isShift = m_InputModule.MultipleSelectionInput();
            // Can't multiple select when in CompassState
            if (isShift && !(m_State is CompassState))
            {
                // with shift it's multiple selection
                if (m_SelectedObjects.Contains(selected))
                {
                    // Deselect all gui elements
                    selected.Deselect();
                    m_SelectedObjects.Remove(selected);
                }
                else
                {
                    // Select all gui elements
                    selected.Select();
                    m_SelectedObjects.Add(selected);
                }
            }
            else
            {
                // If is selected, but only if none other are selected. If multiple objects selected
                // select this one.
                bool contains = m_SelectedObjects.Contains(selected) && m_SelectedObjects.Count == 1;
                // without shift, only one can be selected
                DeselectObjects();
                // Wasn't selected=>select
                if (!contains)
                {
                    // Drone was selected
                    if (m_State is CompassState)
                        selected.CompassSelected();
                    m_SelectedObjects.Add(selected);
                    // Select all gui elements
                    selected.Select();
                }
            }
        }

        public void RemoveObject(Gui.Pickable p)
        {
            if (p != null && p.IsDeletable())
            {
                // Deselect all gui elements
                p.Deselect();
                Gui.GuiManager.Instance.PickableDeselected(p);
                m_SelectedObjects.Remove(p);
                Destroy(p.gameObject);
            }
        }

        /// <summary>
        /// Removes and deselects all selected objects.
        /// </summary>
        public void RemoveObjects()
        {
            for (int i = 0; i < m_SelectedObjects.Count; ++i)
            {
                if (m_SelectedObjects[i].IsDeletable())
                {
                    // Deselect all gui elements
                    m_SelectedObjects[i].Deselect();
                    Destroy(m_SelectedObjects[i].gameObject);
                    m_SelectedObjects.RemoveAt(i);
                    --i;
                }
            }
            DeselectObjects();
        }

        /// <summary>
        /// Deselects all selected objects.
        /// </summary>
        public void DeselectObjects()
        {
            Gui.GuiManager.Instance.DeselectAll();
            foreach (Gui.Pickable o in m_SelectedObjects)
            {
                o.CompassDeselected();
                // Deselect all gui elements
                o.Deselect();
            }
            m_SelectedObjects.Clear();
        }

        /// <summary>
        /// Input field was focused.
        /// </summary>
        /// <param name="name">some string</param>
        public void OnFocusGained(string name)
        {
            m_InputModule.m_IsInputFocused = true;
        }

        /// <summary>
        /// Input field lsot focused.
        /// </summary>
        /// <param name="name">some string</param>
        public void OnFocusLost(string name)
        {
            m_InputModule.m_IsInputFocused = false;
        }

        /// <summary>
        /// Checks if mouse is over gui, which is labeled with 
        /// Constants.TAG_BLOCK_MOUSE.
        /// </summary>
        /// <returns>Returns true iff mouse is over gui, which is labeled with Constants.TAG_BLOCK_MOUSE.</returns>
        public static bool MouseOverGui()
        {
            PointerEventData ped = new PointerEventData(null);
            //Set required parameters, in this case, mouse position
            ped.position = Input.mousePosition;
            //Create list to receive all results
            List<RaycastResult> results = new List<RaycastResult>();
            //Raycast it
            EventSystem.current.RaycastAll(ped, results);
            foreach(RaycastResult res in results)
            {
                if (res.gameObject.tag == Constants.TAG_BLOCK_MOUSE)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles button down, only when mouse is not over gui.
        /// </summary>
        /// <param name="mouse">mouse button identifier</param>
        /// <returns>Returns true, if mouse pressed not over gui.</returns>
        public static bool MouseButtonDown(int mouse)
        {
            return !MouseOverGui() && Input.GetMouseButtonDown(mouse);
        }

        /// <summary>
        /// Handles button being pressed, only when mouse is not over gui.
        /// </summary>
        /// <param name="mouse">mouse button identifier</param>
        /// <returns>Returns true, if mouse pressed not over gui.</returns>
        public static bool MouseButton(int mouse)
        {
            return !MouseOverGui() && Input.GetMouseButton(mouse);
        }

        /// <summary>
        /// Handles button up, only when mouse is not over gui.
        /// </summary>
        /// <param name="mouse">mouse button identifier</param>
        /// <returns>Returns true, if mouse released not over gui.</returns>
        public static bool MouseButtonUp(int mouse)
        {
            return !MouseOverGui() && Input.GetMouseButtonUp(mouse);
        }


        /// <summary>
        /// Returns true if Physics.Raycast can be used.
        /// </summary>
        /// <returns>Returns true if Physics.Raycast can be used.</returns>
        public bool PhysicsPick()
        {
            return m_InputModule.PhysicsPick();
        }


        /// <summary>
        /// Handles state change and user input for given InputModule.
        /// Needs to be in LateUpdate to allow follow camera.
        /// </summary>
        void LateUpdate()
        {
            // Screenshot
            if (m_InputModule.ScreenshotInput())
            {
                ScreenCapture.CaptureScreenshot("../../screenshots/Screenshot" + System.DateTime.Now.ToString("_HH_mm_ss") + ".png", 2);
            }

            // Help 
            if (m_InputModule.HelpInput())
            {
                Gui.GuiManager.Instance.ShowHelp();
            }

            if (!m_Started)
                return;

            // Settings/Options
            if (m_InputModule.OptionsInput())
            {
                Gui.GuiManager.Instance.ShowSettings();
            }

            // Delete selected object, if allowed
            if (m_InputModule.DeleteInput())
            {
                RemoveObjects();
            }

            // Deselect
            if (m_InputModule.DeselectInput())
            {
                DeselectObjects();
                if (Gui.GuiManager.Instance.HelpScreen.gameObject.activeInHierarchy)
                {
                    Gui.GuiManager.Instance.ShowHelp();
                }
                // Just to be sure deselect all input fields
                OnFocusLost("");
            }

            // Handle Input
            // Handle Walker Mode
            if (m_InputModule.WalkerInput() == InputModule.InputStart)
            {
                Ray ray = m_InputModule.GetRay(m_Camera);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
                {
                    m_WalkerPosition = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_WalkerPosition.AddComponent<Models.HelperPoint>();
                    m_WalkerPosition.GetComponent<Renderer>().material.color = Color.red;
                    m_WalkerPosition.transform.position = hit.point;
                }
            }

            if (m_InputModule.WalkerInput() == InputModule.InputNow)
            {
                Ray ray = m_InputModule.GetRay(m_Camera);
                RaycastHit hit;
                if (m_WalkerPosition != null && Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    m_WalkerPosition.transform.position = hit.point;
                    // Switch to Walker Mode when mouse pressed
                    if (PickerCamera.MouseButtonUp(0))
                    {
                        //Destroy(m_WalkerPosition);
                        //m_WalkerPosition = null;
                        WalkerCamera(hit.point);
                    }
                }
            }

            if (m_InputModule.WalkerInput() == InputModule.InputEnd)
            {
                if (m_WalkerPosition != null)
                {
                    Destroy(m_WalkerPosition);
                    m_WalkerPosition = null;
                }
            }

            // Handle menu
            if (m_InputModule.MenuInput() == InputModule.InputStart)
            {
                PopUpMenu.localPosition = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2, 0);
                PopUpMenu.gameObject.SetActive(true);
                PickingEnabled = false;
            }

            if (m_InputModule.MenuInput() == InputModule.InputEnd)
            {
                PopUpMenu.gameObject.SetActive(false);
                PickingEnabled = true;
            }

            m_InputModule.Update();

#if UNITY_ANDROID
            // Only state is top state
            m_TopState.Update(this);
#else
            // Handle state change
            HandleStates();


            // Handle state transform
            if (m_State == null)
            {
                // After going out of editor, it is null
                m_State = m_FreeState;
            }
            m_State.Update(this);
#endif
            // Update GUI
            //Gui.GuiManager.Instance.DeselectAll();
            foreach (Gui.Pickable selected in m_SelectedObjects)
                Gui.GuiManager.Instance.PickableSelected(selected);

            // Handle terrain creation
            // Only one bulk download on the beginning
            m_World.PositionMoved(transform.position);
        }


        /// <summary>
        /// Handles changing between camera states - TopState, CompassState and FreeState.
        /// </summary>
        private void HandleStates()
        {
            // Handle compass camera
            // Check for invalid compass state - can't be without selection or without drone
            if (Target == null && m_State is CompassState)
            {
                FreeCamera();
            }
            // Check if it is agent - pedestrian or drone
            if (Target == null && m_State is CompassState/* && !(Target.GetComponent<Gui.Pickable>() is Agent)*/)
            {
                FreeCamera();
            }

            if (MoveEnabled && Target != null && Input.GetKeyDown(KeyCode.C))
            {
                Gui.Pickable p = Target.GetComponent<Gui.Pickable>();
                //if (p is Agent)
                    CompassCamera();
                p.CompassSelected();
            }

            // Handle Free camera
            if (MoveEnabled && Input.GetKeyDown(KeyCode.F))
            {
                FreeCamera();
            }

            // Handle Top camera
            if (MoveEnabled && Input.GetKeyDown(KeyCode.T))
            {
                TopCamera();
            }
        }

        // Can be called using ctrl+left click on ground, user projects there
        public void WalkerCamera(Vector3 position)
        {
            m_State = m_WalkerState;
            m_Camera.orthographic = false;
            if (m_SelectedObjects.Count == 0)
            {
                (m_State as WalkerState).ResetLook(position);
            }
            else
            {
                (m_State as WalkerState).LookAt(m_SelectedObjects[0].transform.position, position);
            }
        }

        /// <summary>
        /// Selects FreeState.
        /// </summary>
        public void FreeCamera()
        {
            m_State = m_FreeState;
            foreach (Gui.Pickable o in m_SelectedObjects)
            {
                o.CompassDeselected();
            }
        }
        /// <summary>
        /// Selects TopState.
        /// </summary>
        public void TopCamera()
        {
            m_State = m_TopState;
            foreach (Gui.Pickable o in m_SelectedObjects)
            {
                o.CompassDeselected();
            }
        }

        /// <summary>
        /// Selects CompassState.
        /// </summary>
        public void CompassCamera()
        {
            m_State = m_CompassState;
            m_Camera.orthographic = false;
        }


        /// <summary>
        /// Positions camera 500 metres above ground.
        /// </summary>
        public void PositionCamera()
        {
            Ray ray = new Ray();
            Vector3 pos = transform.position;
            pos.y = 10000.0f;
            ray.origin = pos;
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                transform.position = hit.point + Vector3.up * 500.0f;
            }
        }

        /// <summary>
        /// Enables user interaction and positions camera in scene.
        /// </summary>
        public void EnableInteraction()
        {
            PositionCamera();
            m_Started = true;
        }

        public Camera Camera
        {
            get
            {
                return m_Camera;
            }
        }

        /// <summary>
        /// Returns selected object, if only one selected.
        /// </summary>
        public Gui.Pickable Target
        {
            get
            {
                return m_SelectedObjects.Count == 1 ? m_SelectedObjects[0] : null;
            }
        }

        public List<Gui.Pickable> Selection
        {
            get
            {
                return m_SelectedObjects;
            }
        }

        public bool MoveEnabled
        {
            get
            {
                return m_InputModule.MoveEnabled;
            }
        }

        public bool PickingEnabled
        {
            get
            {
                return m_InputModule.PickingEnabled;
            }

            set
            {
                m_InputModule.PickingEnabled = value;
            }
        }

        internal InputModule Module
        {
            get
            {
                return m_InputModule;
            }
        }

        public CharacterController Character
        {
            get
            {
                return m_CC;
            }
        }

    }
}