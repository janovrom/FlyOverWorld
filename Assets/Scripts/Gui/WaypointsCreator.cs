using Assets.Scripts.Cameras;
using Assets.Scripts.Models;
using Assets.Scripts.Nav;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{
    /// <summary>
    /// Handles creation of waypoints, through which can UAVs fly.
    /// Waypoint is created, when first clicked on terrain or when
    /// filled positions in input fields. Each additional waypoint
    /// can be created by clicking on terrain. Altitude can be 
    /// changed by dragging and holding SHIFT.
    /// Before completion, each waypoint can be edited by selecting
    /// it.
    /// </summary>
    class WaypointsCreator : MonoBehaviour
    {

        /// <summary>
        /// Counter for automatically named waypoints - W{Count}.
        /// </summary>
        private static int Count = 0;

        public InputField InputText;
        public InputField CurrentInputX;
        public InputField CurrentInputY;
        public InputField CurrentInputZ;
        public InputField CurrentAltitudeAGL;

        /// <summary>
        /// Determines whether first waypoint is being edited, or we are 
        /// creating everything new.
        /// </summary>
        private bool m_IsEditMode = false;
        /// <summary>
        /// Currently selected waypoint.
        /// </summary>
        private GameObject m_Current;
        /// <summary>
        /// Renderer for lines connecting waypoint with ground.
        /// </summary>
        private DrawLines m_LineRenderer;
        /// <summary>
        /// Contains all waypoints, which are being created.
        /// </summary>
        private List<GameObject> m_Waypoints = new List<GameObject>();
        /// <summary>
        /// Dictionary, which for each waypoint assign its corresponding
        /// id for line renderer.
        /// </summary>
        private Dictionary<GameObject, int> m_WaypointLines = new Dictionary<GameObject, int>();
        private PickerCamera m_Camera;
        /// <summary>
        /// Initial position when mouse was pressed down.
        /// </summary>
        private Vector3 m_MouseStart;
        /// <summary>
        /// Checks if drag started in this script.
        /// </summary>
        private bool m_DragStarted = false;
        /// <summary>
        /// Start position when draggin point in space.
        /// </summary>
        private Vector3 m_PositionDragStartGround;
        private string m_OriginalEditName;

        public void Start()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
            m_LineRenderer = FindObjectOfType<DrawLines>();
        }

        public void CreateWaypoints()
        {
            this.gameObject.SetActive(true);
            m_IsEditMode = false;
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
            Clean();
        }

        /// <summary>
        /// Starts creation of new waypoints, with one already created.
        /// </summary>
        /// <param name="targetWaypoint">Waypoint to edit</param>
        internal void Edit(TargetWaypoint targetWaypoint)
        {
            // Same operations are needed
            CreateWaypoints();
            // Change mode to edit
            m_IsEditMode = true;
            m_Waypoints.Add(targetWaypoint.gameObject);
            m_WaypointLines.Add(targetWaypoint.gameObject, targetWaypoint.LineId);
            m_Current = targetWaypoint.gameObject;
            m_OriginalEditName = targetWaypoint.name;
            UpdateInputPositions();
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
            if (m_Current == null)
                return;

            float alt;
            // Do check on float validity
            if (!CheckInput(float.TryParse(CurrentAltitudeAGL.text, out alt), CurrentAltitudeAGL))
                return;

            // Do check on feasibility
            if (alt < 0.0f)
            {
                CurrentAltitudeAGL.GetComponent<Outline>().effectColor = Color.red;
                return;
            }

            // Change y input to reflect changes in altitude above ground
            Ray ray = new Ray();
            ray.origin = new Vector3(m_Current.transform.position.x, 10000.0f, m_Current.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            // Check if center is really on terrain
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Make sure center is not under ground
                CurrentInputY.text = (hit.point.y + alt).ToString("0.0");
            }

            m_Current.transform.position = hit.point + Vector3.up * alt;
            UpdateInputPositions();
        }

        public void ValuesChanged(string val)
        {
            float x, y, z;
            // If input values wrong, don't change anything
            // Inform user by red outline
            if (!float.TryParse(CurrentInputX.text, out x))
            {
                CurrentInputX.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            else
            {
                CurrentInputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            }
            if (!float.TryParse(CurrentInputY.text, out y))
            {
                CurrentInputY.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            else
            {
                CurrentInputY.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            }
            if (!float.TryParse(CurrentInputZ.text, out z))
            {
                CurrentInputZ.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            else
            {
                CurrentInputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            }
            // Make sure it is not under ground
            Ray ray = new Ray();
            ray.origin = new Vector3(x, 10000.0f, z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                y = Mathf.Max(hit.point.y, y);
            }

            if (m_Waypoints.Count == 0)
            {
                m_Current = CreatePoint(new Vector3(x, y, z));
                if (InputText.text.Length < 1)
                {
                    string newName = "W" + Count++;
                    m_Current.name = newName;
                    //InputText.text = newName;
                }
                else
                {
                    m_Current.name = InputText.text;
                }
                m_Current.transform.position = new Vector3(x, y, z);
                UpdateInputPositions();
            }
            else
            {
                m_Current.transform.position = new Vector3(x, y, z);
                m_Current.name = InputText.text;
                CurrentAltitudeAGL.text = (y - hit.point.y).ToString("0.0");
                UpdateInputPositions();
            }
        }

        /// <summary>
        /// Creates new instance of waypoint prefab on specified location.
        /// </summary>
        /// <param name="pos">Position where point will be created.</param>
        /// <returns>Returns new instance of waypoint prefab.</returns>
        private GameObject CreatePoint(Vector3 pos)
        {
            GameObject o = Instantiate(ZoneManager.Instance.WaypointsPrefab);
            o.layer = LayerMask.NameToLayer(Constants.LAYER_EDITABLE);
            //o.GetComponent<Renderer>().material.color = Color.yellow;
            o.transform.position = pos;
            o.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
            m_Current = o;
            int lineId = m_LineRenderer.RegisterLine();
            m_LineRenderer.UpdateLine(lineId, new List<Vector3>() { new Vector3(pos.x, 0, pos.z), pos });
            m_WaypointLines.Add(o, lineId);
            return o;
        }

        /// <summary>
        /// While clicking on terrain new point is created. If user clicked on
        /// existing waypoint, it is selected instead.
        /// </summary>
        private void HandleCreation()
        {
            Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
            RaycastHit hit;
            // We can pick and create - add mask for terrain and editable
            int editableMask = LayerMask.GetMask(Constants.LAYER_EDITABLE);
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            if (Physics.Raycast(ray, out hit, 30000.0f, editableMask | terrainMask))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_EDITABLE))
                {
                    // pick it
                    m_Current = hit.collider.gameObject;
                    m_DragStarted = true;
                    // Get its position on ground
                    ray.origin = m_Current.transform.position;
                    if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                    {
                        m_PositionDragStartGround = hit.point;
                    }
                    else
                    {
                        m_PositionDragStartGround = m_Current.transform.position;
                    }
                }
                else
                {
                    // it is terrain, create new point
                    string prevName = "";
                    if (m_Current != null)
                    {
                        prevName = m_Current.name;
                    }
                    Vector3 pos = hit.point;
                    if (m_Waypoints.Count > 0)
                    {
                        pos.y = m_Waypoints[m_Waypoints.Count - 1].transform.position.y;
                    }
                    m_Current = CreatePoint(pos);
                    m_Waypoints.Add(m_Current);
                    if (InputText.text.Length == 0 || InputText.text.Equals(prevName))
                    {
                        // No input name was filled, use default, it can be changed anytime
                        string newName = "W" + Count++;
                        m_Current.name = newName;
                        //InputText.text = newName;
                    }
                    else
                    {
                        // Use user's name
                        m_Current.name = InputText.text;
                    }
                }

                if (m_Current != null)
                {
                    UpdateInputPositions();
                }
            }
        }

        /// <summary>
        /// While holding shift, altitude can be changed while holding left mouse
        /// button and dragging.
        /// </summary>
        private void HandleAltitude()
        {
            if (m_Current != null && PickerCamera.MouseButton(0))
            {
                // Get altitude change, keep position in XZ
                Vector3 pos = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint());
                pos = pos - m_MouseStart;
                m_MouseStart += pos;
                pos.x = m_Current.transform.position.x;
                pos.y = m_Current.transform.position.y + pos.y * Settings.CREATION_ALTITUDE_SENSITIVITY;
                pos.z = m_Current.transform.position.z;

                // Make sure it is not under ground
                Ray ray = new Ray();
                ray.origin = new Vector3(pos.x, 10000.0f, pos.z);
                ray.direction = Vector3.down;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    pos.y = Mathf.Max(hit.point.y, pos.y);
                }
                // Update values
                m_Current.transform.position = pos;
                UpdateInputPositions();
            }
        }

        /// <summary>
        /// If waypoint is selected, it can be repositioned by dragging while
        /// holding left mouse button.
        /// </summary>
        private void HandlePosition()
        {
            if (m_Current != null && PickerCamera.MouseButton(0) && m_DragStarted)
            {
                Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    if (m_Current)
                    {
                        // Reposition point and keep its altitude
                        Vector3 v = new Vector3(hit.point.x - m_PositionDragStartGround.x, hit.point.y - m_PositionDragStartGround.y, hit.point.z - m_PositionDragStartGround.z);
                        m_Current.transform.position = v + m_Current.transform.position;
                        m_PositionDragStartGround = hit.point;
                        UpdateInputPositions();
                    }
                }
            }
        }

        /// <summary>
        /// Updates position input fields and connecting line.
        /// </summary>
        private void UpdateInputPositions()
        {
            // Make outlines invisible since it is for sure, the point is valid
            InputText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputY.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            // Update inputs
            InputText.text = m_Current.name;
            CurrentInputX.text = m_Current.transform.position.x.ToString("0.0");
            CurrentInputY.text = m_Current.transform.position.y.ToString("0.0");
            CurrentInputZ.text = m_Current.transform.position.z.ToString("0.0");
            // Update AGL
            Ray ray = new Ray();
            ray.origin = new Vector3(m_Current.transform.position.x, 10000.0f, m_Current.transform.position.z);
            ray.direction = Vector3.down;
            RaycastHit hit;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            // Check if center is really on terrain
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Make sure center is not under ground
                CurrentAltitudeAGL.text = (m_Current.transform.position.y - hit.point.y).ToString("0.0");
            }
            // Update line
            int lineId;
            m_WaypointLines.TryGetValue(m_Current, out lineId);
            m_LineRenderer.UpdateLine(lineId, new Vector3(m_Current.transform.position.x, 0.0f, m_Current.transform.position.z), m_Current.transform.position);
        }

        void Update()
        {
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

            bool isShift = m_Camera.Module.ShiftInput();
            if (isShift)
            {
                if (PickerCamera.MouseButtonDown(0))
                {
                    // Initialize dragging
                    m_MouseStart = m_Camera.Camera.ScreenToViewportPoint(m_Camera.Module.InputPoint());
                }

                HandleAltitude();
            }
            else
            {
                if (PickerCamera.MouseButtonDown(0))
                {
                    // Create additional points
                    HandleCreation();
                }
                // Position new or current point
                HandlePosition();
            }

            if (Input.GetMouseButtonUp(0))
                m_DragStarted = false;
        }

        /// <summary>
        /// Creates waypoints, if all conditions are met, ie. at least one waypoint
        /// is created and its values are valid.
        /// </summary>
        public void Confirm()
        {
            bool check = m_Waypoints.Count > 0;
            int i = 0;
            // If there exists such a name, inform user
            foreach (GameObject o in m_Waypoints)
            {
                if (m_IsEditMode && i++ == 0 && o.name == m_OriginalEditName)
                {
                    continue;
                }
                if (ZoneManager.Instance.HasZone(o.name))
                {
                    GuiManager.LogWarning("Waypoint with name " + o.name + " already exists.");
                    check = false;
                    break;
                }
            }

            if (check)
            {
                i = 0;
                foreach (GameObject o in m_Waypoints)
                {
                    // Position for waypoint has to be relative to the ground
                    Vector3 pos = o.transform.position;
                    Ray ray = new Ray();
                    ray.origin = new Vector3(pos.x, 10000.0f, pos.z);
                    ray.direction = Vector3.down;
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                    {
                        pos.y -= hit.point.y;
                    }

                    // Skip first in edit mode
                    if (m_IsEditMode && i++ == 0)
                    {
                        // Change cartesian waypoint
                        o.GetComponent<TargetWaypoint>().CartesianWaypoint.Position = pos;
                        ZoneManager.Instance.ZoneRenamed(m_OriginalEditName, o.GetComponent<TargetWaypoint>());
                    }
                    else
                    {
                        CartesianWaypoint w = new CartesianWaypoint(o.name, pos);
                        ZoneManager.Instance.AddWaypoint(w, o.transform.position);
                    }
                }
                Close();
            }
            else
            {
                InputText.GetComponent<Outline>().effectColor = Color.red;
                CurrentInputX.GetComponent<Outline>().effectColor = Color.red;
                CurrentInputY.GetComponent<Outline>().effectColor = Color.red;
                CurrentInputZ.GetComponent<Outline>().effectColor = Color.red;
            }
        }

        /// <summary>
        /// Cleans temporary variables and hides creation window.
        /// </summary>
        public void Close()
        {
            Clean();
            this.gameObject.SetActive(false);
            m_IsEditMode = false;
        }

        /// <summary>
        /// Cleans all lists and dictionaries used while creating
        /// waypoints. All temporary game objects are destroyed.
        /// </summary>
        private void Clean()
        {
            // Make outlines invisible
            InputText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputY.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentInputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            CurrentAltitudeAGL.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            // Set input texts to empty string, so placeholder will be shown
            InputText.text = "";
            CurrentInputX.text = "";
            CurrentInputY.text = "";
            CurrentInputZ.text = "";
            CurrentAltitudeAGL.text = "";
            // Destroy all temporarily created game objects and lines
            m_Current = null;
            int i = 0;
            foreach (GameObject o in m_Waypoints)
            {
                // Skip first in edit mode
                if (m_IsEditMode && i++ == 0)
                    continue;
                Destroy(o);
            }
            m_Waypoints.Clear();
            i = 0;
            foreach (int line in m_WaypointLines.Values)
            {
                // Skip first in edit mode
                if (m_IsEditMode && i++ == 0)
                    continue;
                m_LineRenderer.RemoveLine(line);
            }
            m_WaypointLines.Clear();
        }

    }
}
