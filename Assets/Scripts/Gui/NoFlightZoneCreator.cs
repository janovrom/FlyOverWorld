using Assets.Scripts.Cameras;
using Assets.Scripts.Models;
using Assets.Scripts.Nav;
using Assets.Scripts.Network.Command;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Handles creation of No-fly-zone using 3 handles.
    /// For position, for radius and for height.
    /// </summary>
    class NoFlightZoneCreator : MonoBehaviour
    {

        private static int ID = 0;

        public InputField InputText;
        public InputField InputX;
        public InputField InputY;
        public InputField InputZ;
        public InputField InputAltitudeAGL;
        public InputField InputRadius;
        public InputField InputHeight;

        // <summary>
        /// Determines whether no-fly-zone is being edited, or we are 
        /// creating a new one.
        /// </summary>
        private bool m_IsEditMode = false;
        private string m_OriginalName;
        /// <summary>
        /// Game object of temporary no fly zone.
        /// </summary>
        private GameObject m_NFZ;
        private PickerCamera m_Camera;
        /// <summary>
        /// Viewport coordinates of mouse, when first pressed down.
        /// </summary>
        private Vector3 m_MouseStart;
        /// <summary>
        /// Id for line, which connects center of no flight zone and 
        /// its projection on ground.
        /// </summary>
        private int m_LineId = -1;
        /// <summary>
        /// Line renderer for OpenGL lines.
        /// </summary>
        private DrawLines m_LineRenderer;
        /// <summary>
        /// Handle for positioning of NFZ.
        /// </summary>
        private GameObject m_PositionHandle;
        /// <summary>
        /// Handle for changing radius of NFZ.
        /// </summary>
        private GameObject m_RadiusHandle;
        /// <summary>
        /// Handle for changing height of NFZ.
        /// </summary>
        private GameObject m_HeightHandle;
        /// <summary>
        /// Currently selected handle.
        /// </summary>
        private GameObject m_CurrentHandle;

        public void Start()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
            m_LineRenderer = FindObjectOfType<DrawLines>();
        }

        public void CreateNFZ()
        {
            this.gameObject.SetActive(true);
            Clean();
            // Create name only if not in edit mode
            if (!m_IsEditMode)
                InputText.text = "NFZ" + ID++;
            m_IsEditMode = false;
            // Select and activate name input field
            // Try to get tabshifter, if doesn't exist, focus only name
            TabInputShifter tis = GetComponent<TabInputShifter>();
            if (tis != null)
            {
                tis.Reset();
            }
            else
            {
                InputText.Select();
                InputText.ActivateInputField();
            }
        }

        /// <summary>
        /// Edits selected no-fly zone.
        /// </summary>
        /// <param name="noFlightZone">No-Fly Zone to be edited</param>
        internal void Edit(NoFlyZone noFlightZone)
        {
            // Same operations are needed as when creating
            // Inform about edit, to not generate name
            m_IsEditMode = true;
            CreateNFZ();
            // Change mode to edit
            m_IsEditMode = true;
            m_OriginalName = noFlightZone.name;
            m_NFZ = noFlightZone.gameObject;
            m_LineId = noFlightZone.LineId;
            // Get ground position for creating handles
            Ray ray = new Ray();
            ray.origin = new Vector3(m_NFZ.transform.position.x, 10000.0f, m_NFZ.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            // Check if center is really on terrain
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
            }
            CreateHandles(m_NFZ.transform.localScale, hit.point);
            UpdateValues();
        }

        /// <summary>
        /// Changes color of input field outline based on check value.
        /// </summary>
        /// <param name="check">Whether check was passed.</param>
        /// <param name="field">Input field tested.</param>
        /// <returns>Returns value of check</returns>
        private bool CheckInput(bool check, InputField field)
        {
            if (!check)
            {
                field.GetComponent<Outline>().effectColor = Color.red;
            }
            else
            {
                field.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            }
            return check;
        }

        /// <summary>
        /// Listener for on end edit for altitude above ground level.
        /// </summary>
        public void AltitudeAGLChanged()
        {
            if (m_NFZ == null)
                return;

            float alt;
            // Do check on float validity
            if (!CheckInput(float.TryParse(InputAltitudeAGL.text, out alt), InputAltitudeAGL))
                return;

            // Do check on feasibility
            if (alt < 0.0f)
            {
                InputAltitudeAGL.GetComponent<Outline>().effectColor = Color.red;
                return;
            }

            // Change y input to reflect changes in altitude above ground
            Ray ray = new Ray();
            ray.origin = new Vector3(m_NFZ.transform.position.x, 10000.0f, m_NFZ.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            // Check if center is really on terrain
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Make sure center is not under ground
                InputY.text = (hit.point.y + alt).ToString("0.0");
            }

            m_NFZ.transform.position = hit.point + Vector3.up * alt;
            UpdateHandles();
            UpdateValues();
        }

        /// <summary>
        /// Called when value in input field is changed. Does 
        /// check on validity of values and if they are not 
        /// valid, user is informed by red outline around input.
        /// </summary>
        public void ValuesChanged()
        {
            m_NFZ.name = InputText.text;
            float x, y, z, radius, height;
            // Test position
            if (!CheckInput(float.TryParse(InputX.text, out x), InputX))
                return;
            if (!CheckInput(float.TryParse(InputY.text, out y), InputY))
                return;
            if (!CheckInput(float.TryParse(InputZ.text, out z), InputZ))
                return;

            // Test height and radius
            if (!CheckInput(float.TryParse(InputRadius.text, out radius), InputRadius))
                return;
            if (!CheckInput(float.TryParse(InputHeight.text, out height), InputHeight))
                return;

            if (radius <= 0.0f)
            {
                InputRadius.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            if (height <= 0.0f)
            {
                InputHeight.GetComponent<Outline>().effectColor = Color.red;
                return;
            }

            // Scale is diameter!
            radius *= 2.0f;
            Vector3 center = new Vector3(x, y, z);
            Ray ray = new Ray();
            ray.origin = new Vector3(x, 10000.0f, z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            float terrainY = y;
            // Check if center is really on terrain
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Make sure center is not under ground
                center.y = Mathf.Max(y, hit.point.y);
                terrainY = hit.point.y;
            }
            else
            {
                // Point is outside terrain
                InputX.GetComponent<Outline>().effectColor = Color.red;
                InputZ.GetComponent<Outline>().effectColor = Color.red;
                return;
            }

            if (m_NFZ == null)
            {
                // Try to create it
                CreateZone(center, new Vector3(radius, height / 2.0f, radius), new Vector3(x, terrainY, z));
                UpdateHandles();
            }
            else
            {
                m_NFZ.transform.position = center;
                InputAltitudeAGL.text = (y - hit.point.y).ToString("0.0");
                m_NFZ.transform.localScale = new Vector3(radius, height / 2.0f, radius);
                // Reposition handles
                UpdateHandles();
            }
            UpdateValues();
        }

        /// <summary>
        /// Changes no fly zone altitude.
        /// </summary>
        private void HandleAltitude()
        {
            if (m_NFZ != null && PickerCamera.MouseButtonDown(0))
            {
                m_MouseStart = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint());
            }

            if (m_NFZ != null && PickerCamera.MouseButton(0))
            {
                Vector3 pos = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint());
                pos = pos - m_MouseStart;
                m_MouseStart += pos;
                pos.x = m_NFZ.transform.position.x;
                pos.y = m_NFZ.transform.position.y + pos.y * Settings.CREATION_ALTITUDE_SENSITIVITY;
                pos.z = m_NFZ.transform.position.z;
                m_NFZ.transform.position = pos;
                // Update position of height handle
                m_HeightHandle.transform.position = m_NFZ.transform.position + Vector3.up * m_NFZ.transform.localScale.y;
                UpdateValues();
            }
        }

        /// <summary>
        /// Does raycast and if handle is selected, it can be dragged to change 
        /// NFZ.
        /// </summary>
        private void HandleHandles()
        {
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            int editableMask = LayerMask.GetMask(Constants.LAYER_EDITABLE);
            // Do selection on left mouse down
            if (PickerCamera.MouseButtonDown(0))
            {
                m_MouseStart = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint());
                Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, editableMask))
                {
                    m_CurrentHandle = hit.collider.gameObject;
                }
            }

            // While left mouse button is pressed and we have something selected
            if (m_CurrentHandle != null && PickerCamera.MouseButton(0))
            {
                if (m_CurrentHandle == m_RadiusHandle)
                {
                    // Do raycast for gettin position on terrain
                    Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                    {
                        // shouldn't be negative
                        if (hit.point.x > m_PositionHandle.transform.position.x)
                            m_RadiusHandle.transform.position = new Vector3(hit.point.x, hit.point.y, m_RadiusHandle.transform.position.z);
                    }
                }
                else if (m_CurrentHandle == m_PositionHandle)
                {
                    // Do raycast for gettin position on terrain
                    Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                    {
                        m_PositionHandle.transform.position = hit.point;
                    }
                }
                else if (m_CurrentHandle == m_HeightHandle)
                {
                    Vector3 pos = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint()) - m_MouseStart;
                    m_MouseStart += pos;
                    pos *= Settings.CREATION_RESIZE_SENSITIVITY;
                    // We want only vertical change
                    pos.x = 0.0f;
                    pos.z = 0.0f;
                    // We don't want negative height
                    if (m_HeightHandle.transform.position.y + pos.y > m_NFZ.transform.position.y)
                        m_HeightHandle.transform.position = m_HeightHandle.transform.position + pos;
                }

                // Apply handles and update values
                ApplyHandles();
                UpdateValues();
            }
        }

        /// <summary>
        /// Converts handle positions to no fly zone sizes and assigns them.
        /// </summary>
        private void ApplyHandles()
        {
            // Position can't be lower than handle
            m_NFZ.transform.position = new Vector3(m_PositionHandle.transform.position.x, Mathf.Max(m_NFZ.transform.position.y, m_PositionHandle.transform.position.y), m_PositionHandle.transform.position.z);
            float radius = m_RadiusHandle.transform.localPosition.x * 2.0f;
            float height = m_HeightHandle.transform.position.y - m_NFZ.transform.position.y;
            m_NFZ.transform.localScale = new Vector3(radius, height, radius);
        }

        /// <summary>
        /// Something was changed not using handles, so handles has to be informed and recreated.
        /// </summary>
        private void UpdateHandles()
        {
            Ray ray = new Ray();
            ray.origin = new Vector3(m_NFZ.transform.position.x, 10000.0f, m_NFZ.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            // position center
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                m_PositionHandle.transform.position = hit.point;
            }
            // Again, radius is half of scale=diameter!
            ray.origin = new Vector3(ray.origin.x + m_NFZ.transform.localScale.x / 2.0f, ray.origin.y, ray.origin.z);
            // position radius
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                m_RadiusHandle.transform.position = hit.point;
            }
            // position height
            m_HeightHandle.transform.position = m_NFZ.transform.position + Vector3.up * m_NFZ.transform.localScale.y;
        }

        /// <summary>
        /// Handles creation of no fly zone and detecting user input. 
        /// It decides based on which spherical handle is currently
        /// selected. With draggin and holding shift altitude is changed.
        /// </summary>
        void Update()
        {
            // Enable enter confirm
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Confirm();
                m_Camera.OnFocusLost("");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                m_Camera.OnFocusLost("");
                return;
            }

            // Check if nfz created
            if (m_NFZ != null)
            {
                // Created
                bool isShift = m_Camera.Module.ShiftInput();
                if (isShift)
                    HandleAltitude();
                else
                {
                    // Do selection
                    // Base on what handle is currently selected
                    HandleHandles();
                }
            }
            else
            {
                // Create it
                Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                RaycastHit hit;
                int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
                if (PickerCamera.MouseButtonDown(0) && Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                {
                    CreateZone(hit.point, new Vector3(10.0f, 5.0f, 10.0f), hit.point);
                }
            }

            // Deselect on left mouse up
            if (PickerCamera.MouseButtonUp(0))
            {
                m_CurrentHandle = null;
            }
        }

        /// <summary>
        /// When called nfz is created with center and sizes as specified.
        /// </summary>
        /// <param name="pos">Position of NFZ center.</param>
        /// <param name="size">Size of NFZ.</param>
        private void CreateZone(Vector3 pos, Vector3 size, Vector3 groundPos)
        {
            // it is terrain, create new point
            if (m_NFZ == null)
            {
                m_NFZ = GameObject.Instantiate(ZoneManager.Instance.CylinderPrefab);
                // values = [diameter, height(both up and down)]
                m_NFZ.transform.localPosition = pos;
                m_NFZ.transform.localScale = size;
                m_NFZ.name = InputText.text;
                // Create new line
                m_LineId = m_LineRenderer.RegisterLine();
                m_LineRenderer.UpdateLine(m_LineId, new List<Vector3>() { new Vector3(pos.x, 0.0f, pos.z), pos });
                UpdateValues();
                // Create handles
                CreateHandles(size, groundPos);
                // Make position handle as default
                //m_CurrentHandle = m_PositionHandle;
            }
        }

        /// <summary>
        /// Create handles for no-fly zone with specified parameters.
        /// </summary>
        /// <param name="size">Size of NFZ</param>
        /// <param name="groundPos">Position of NFZ projected on ground</param>
        private void CreateHandles(Vector3 size, Vector3 groundPos)
        {
            m_HeightHandle = CreatePoint(m_NFZ.transform.localPosition + Vector3.up * size.y, true);
            m_RadiusHandle = CreatePoint(groundPos + Vector3.right * size.x / 2.0f, true);
            m_PositionHandle = CreatePoint(groundPos, false);
            m_PositionHandle.transform.localScale = Vector3.one;
            m_HeightHandle.transform.parent = m_PositionHandle.transform;
            m_RadiusHandle.transform.parent = m_PositionHandle.transform;
        }

        /// <summary>
        /// Creates new instance of sphere handle on specified location.
        /// </summary>
        /// <param name="pos">Position where point will be created.</param>
        /// <returns>Returns new instance of handle.</returns>
        private GameObject CreatePoint(Vector3 pos, bool addScaling)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (addScaling)
            {
                HelperPoint hp = o.AddComponent<Models.HelperPoint>();
                hp.MaxSize = 10.0f;
                hp.MinSize = 1.0f;
            }
            o.layer = LayerMask.NameToLayer(Constants.LAYER_EDITABLE);
            o.GetComponent<Renderer>().material.color = Color.red;
            o.transform.position = pos;
            return o;
        }

        /// <summary>
        /// Updates values in input field and line connecting center of nfz and
        /// its projection on terrain.
        /// </summary>
        private void UpdateValues()
        {
            // Update values
            InputText.text = m_NFZ.name;
            InputX.text = m_NFZ.transform.localPosition.x.ToString("0.0");
            InputY.text = m_NFZ.transform.localPosition.y.ToString("0.0");
            Ray ray = new Ray();
            ray.origin = new Vector3(m_NFZ.transform.position.x, 10000.0f, m_NFZ.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                InputAltitudeAGL.text = (m_NFZ.transform.localPosition.y - hit.point.y).ToString("0.0");
            }
            InputZ.text = m_NFZ.transform.localPosition.z.ToString("0.0");
            // Cylinder has base radius of 0.5 units, thus radius is twice smaller than scale=diameter
            InputRadius.text = (m_NFZ.transform.localScale.x / 2.0f).ToString("0.0");
            // Height is multiplied by two, since default cylinder has height of 2 units and thus scale is halved
            InputHeight.text = (m_NFZ.transform.localScale.y * 2.0f).ToString("0.0");
            // Update line
            m_LineRenderer.UpdateLine(m_LineId, new Vector3(m_NFZ.transform.localPosition.x, 0.0f, m_NFZ.transform.localPosition.z), m_NFZ.transform.localPosition);
        }

        /// <summary>
        /// If values are correct, new no fly zone is created and
        /// this creation dialog is closed.
        /// </summary>
        public void Confirm()
        {
            bool check = true;
            float x, y, z, radius, height;
            check &= CheckInput(float.TryParse(InputX.text, out x), InputX);
            check &= CheckInput(float.TryParse(InputY.text, out y), InputY);
            check &= CheckInput(float.TryParse(InputZ.text, out z), InputZ);
            Vector3 center = new Vector3(x, y, z);
            Vector3 position = new Vector3(x, y, z);
            check &= CheckInput(float.TryParse(InputRadius.text, out radius), InputRadius);
            check &= CheckInput(float.TryParse(InputHeight.text, out height), InputHeight);
            check &= CheckInput(InputText.text != null && InputText.text.Length > 0, InputText);
            // Check on name availability
            // We might have zone, but when in edit mode, it is not important
            bool hasZone = ZoneManager.Instance.HasZone(InputText.text) && !(m_IsEditMode && m_OriginalName == InputText.text);
            check &= !hasZone;
            if (hasZone)
            {
                GuiManager.LogWarning("No-fly zone with name " + InputText.text + " already exists.");
                InputText.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            // We passed check, create IT!
            if (check)
            {
                Ray ray = new Ray();
                ray.origin = new Vector3(x, 10000.0f, z);
                ray.direction = Vector3.down;
                RaycastHit hit;
                int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
                if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                {
                    center.y -= hit.point.y;
                }
                if (m_IsEditMode)
                {
                    NoFlyZone zone = m_NFZ.GetComponent<NoFlyZone>();
                    zone.name = InputText.text;
                    zone.Center = center;
                    zone.Radius = radius;
                    zone.Height = height / 2.0f;
                    CommandNoFlyZone rem = new CommandNoFlyZone(Commands.CommandRemoveNoFlyZone, Command.BROADCAST_ADDRESS,
                        m_OriginalName);
                    CommandNoFlyZone nfz = new CommandNoFlyZone(Commands.CommandCreateNoFlyZone, Command.BROADCAST_ADDRESS,
                    InputText.text, (byte)NoFlyZone.ZoneType.CYLINDER, center, radius, height);
                    Network.SimulatorClient.Instance.Send(rem);
                    Network.SimulatorClient.Instance.Send(nfz);
                    ZoneManager.Instance.ZoneRenamed(m_OriginalName, zone);
                }
                else
                {
                    ZoneManager.Instance.AddNoFlightZone(InputText.text, center, position, radius, height, NoFlyZone.ZoneType.CYLINDER);
                }
                Close();
            }
        }

        /// <summary>
        /// Closes creation window and cleans temporary variables.
        /// </summary>
        public void Close()
        {
            
            this.gameObject.SetActive(false);
            Clean();
        }

        /// <summary>
        /// Cleans all temporary variables and destroys
        /// temporarily created game objects.
        /// </summary>
        private void Clean()
        {
            // Clean text in inputs, so placeholder will be shown
            InputText.text = "";
            InputX.text = "";
            InputY.text = "";
            InputZ.text = "";
            InputRadius.text = "";
            InputHeight.text = "";
            InputAltitudeAGL.text = "";
            // Make outlines invisible
            InputText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputY.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputAltitudeAGL.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputRadius.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            InputHeight.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            // Destroy objects
            if (m_HeightHandle != null)
                Destroy(m_HeightHandle);
            if (m_RadiusHandle != null)
                Destroy(m_RadiusHandle);
            if (m_PositionHandle != null)
                Destroy(m_PositionHandle);
            m_CurrentHandle = null;
            m_PositionHandle = null;
            m_RadiusHandle = null;
            m_HeightHandle = null;
            // Destroy if not editing
            if (m_NFZ != null && !m_IsEditMode)
                Destroy(m_NFZ);
            m_NFZ = null;
            // Remove line if not editing
            if (m_LineId != -1 && !m_IsEditMode)
            {
                m_LineRenderer.RemoveLine(m_LineId);
            }
            m_LineId = -1;
        }

    }
}
