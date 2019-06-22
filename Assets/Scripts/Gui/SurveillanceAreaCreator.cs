using Assets.Scripts.Cameras;
using Assets.Scripts.Nav;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Handles creation of rectangular surveillance area.
    /// Area is created on ground using two blue spherical handles.
    /// Handles can be selected by clicking on them and while left mouse
    /// button is held, it can be repositioned (if both handles are
    /// created, then area is also resized).
    /// </summary>
    public class SurveillanceAreaCreator : MonoBehaviour
    {

        private static int ID = 0;

        public InputField InputText;
        public InputField StartInputX;
        public InputField StartInputZ;
        public InputField EndInputX;
        public InputField EndInputZ;

        /// <summary>
        /// Start of rectangular area. Doesn't have to necessarily be minimum.
        /// </summary>
        private GameObject m_Start;
        /// <summary>
        /// Currently selected spherical handle.
        /// </summary>
        private GameObject m_Current;
        /// <summary>
        /// End of rectangular area. Doesn't have to necessarily be maximum.
        /// </summary>
        private GameObject m_End;
        private PickerCamera m_Camera;
        /// <summary>
        /// Temporary rectangular area.
        /// </summary>
        private SurveillanceArea m_Area;
        /// <summary>
        /// Determines whether surveillance area is being edited, or we are 
        /// creating a new one.
        /// </summary>
        private bool m_IsEditMode = false;
        private string m_OriginalName;

        public void Start()
        {
            m_Camera = FindObjectOfType<PickerCamera>();
        }

        /// <summary>
        /// Updates surveillance area according to new start and end positions.
        /// </summary>
        private void AreaChanged()
        {
            if (m_IsEditMode)
                m_Area.UpdateArea(InputText.text, m_Start.transform.position, m_End.transform.position);
            else
                m_Area.UpdateArea("", m_Start.transform.position, m_End.transform.position);
        }

        public void CreateArea()
        {
            this.gameObject.SetActive(true);
            Clean();
            if (!m_IsEditMode)
                InputText.text = "SA" + ID++;
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
        }

        /// <summary>
        /// Edits an existing surveillance area.
        /// </summary>
        /// <param name="surveillanceArea">Surveillance area to change.</param>
        internal void Edit(SurveillanceArea surveillanceArea)
        {
            // Same operations are needed
            m_IsEditMode = true;
            CreateArea();
            // Change mode to edit
            m_IsEditMode = true;
            RaycastHit hit;
            Ray ray = new Ray();
            ray.origin = new Vector3(surveillanceArea.Min.x, 10000.0f, surveillanceArea.Min.z);
            ray.direction = Vector3.down;
            int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Coordinates were above terrain, so are valid
                m_Start = CreatePoint(hit.point);
            }
            ray.origin = new Vector3(surveillanceArea.Max.x, 10000.0f, surveillanceArea.Max.z);
            if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
            {
                // Coordinates were above terrain, so are valid
                m_End = CreatePoint(hit.point);
            }
            m_Area = surveillanceArea;
            InputText.text = surveillanceArea.name;
            m_OriginalName = surveillanceArea.name;
            AreaChanged();
            UpdatePositionsInputs();
        }

        /// <summary>
        /// Changes color of input field outline based on check value.
        /// </summary>
        /// <param name="check">Whether check was passed.</param>
        /// <param name="field">Input field tested.</param>
        private void CheckInput(bool check, InputField field)
        {
            if (!check)
            {
                field.GetComponent<Outline>().effectColor = Color.red;
            }
            else
            {
                field.GetComponent<Outline>().effectColor = new Color(0,0,0,0);
            }
        }

        public void PositionChanged(string val)
        {
            float x, y, z;
            bool check1 = float.TryParse(StartInputX.text, out x);
            CheckInput(check1, StartInputX);
            bool check2 = float.TryParse(StartInputZ.text, out z);
            CheckInput(check2, StartInputZ);
            // Try to create it
            if (m_Start == null && check1 && check2)
            {
                // Both values are valid and we don't have start point, so create it
                RaycastHit hit;
                Ray ray = new Ray();
                ray.origin = new Vector3(x, 10000.0f, z);
                ray.direction = Vector3.down;
                int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
                if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                {
                    // Coordinates were above terrain, so are valid
                    m_Start = CreatePoint(hit.point);
                }
                else
                {
                    // Didn't hit terrain, so coordinate is somewhere outside, make it invalid
                    StartInputX.text = "";
                    StartInputZ.text = "";
                    StartInputX.GetComponent<Outline>().effectColor = Color.red;
                    StartInputZ.GetComponent<Outline>().effectColor = Color.red;
                }
            }
            if (m_Start != null)
            {
                Ray ray = new Ray();
                ray.origin = new Vector3(x, 10000.0f, z);
                ray.direction = Vector3.down;
                RaycastHit hit;
                y = m_Start.transform.position.y;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    y = hit.point.y;
                }
                m_Start.transform.position = new Vector3(x, y, z);
            }
            // Do the same check for end point
            check1 = float.TryParse(EndInputX.text, out x);
            CheckInput(check1, EndInputX);
            check2 = float.TryParse(EndInputZ.text, out z);
            CheckInput(check2, EndInputZ);
            // Try to create it
            if (m_End == null && check1 && check2)
            {
                // Both values are valid and we don't have start point, so create it
                RaycastHit hit;
                Ray ray = new Ray();
                ray.origin = new Vector3(x, 10000.0f, z);
                ray.direction = Vector3.down;
                int terrainMask = LayerMask.GetMask(Constants.LAYER_TERRAIN);
                if (Physics.Raycast(ray, out hit, 30000.0f, terrainMask))
                {
                    // Coordinates were above terrain, so are valid
                    m_End = CreatePoint(hit.point);
                }
                else
                {
                    // Didn't hit terrain, so coordinate is somewhere outside, make it invalid
                    EndInputX.text = "";
                    EndInputZ.text = "";
                    EndInputX.GetComponent<Outline>().effectColor = Color.red;
                    EndInputZ.GetComponent<Outline>().effectColor = Color.red;
                }
            }
            if (m_End != null)
            {
                Ray ray = new Ray();
                ray.origin = new Vector3(x, 10000.0f, z);
                ray.direction = Vector3.down;
                RaycastHit hit;
                y = m_End.transform.position.y;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    y = hit.point.y;
                }
                m_End.transform.position = new Vector3(x, y, z);
            }

            // We might have created both points and don't have area
            if (m_Area == null && m_Start != null && m_End != null)
            {
                m_Area = GameObject.Instantiate(ZoneManager.Instance.SurveillanceAreaPrefab).GetComponent<SurveillanceArea>();
            }

            if (m_Area != null)
            {
                AreaChanged();
            }
        }

        /// <summary>
        /// Creates new sphere handle on specified location.
        /// </summary>
        /// <param name="pos">Position where point will be created.</param>
        /// <returns>Returns new spherical handle</returns>
        private GameObject CreatePoint(Vector3 pos)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.AddComponent<Models.HelperPoint>();
            o.layer = LayerMask.NameToLayer(Constants.LAYER_EDITABLE);
            o.GetComponent<Renderer>().material.color = Color.blue;
            o.transform.position = pos;
            m_Current = o;
            return o;
        }

        /// <summary>
        /// Tracks left mouse button input and creates two spherical handles when clicked on 
        /// terrain. After both handles are created, then rectangular surveillance area
        /// is created itself.
        /// </summary>
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

            if (PickerCamera.MouseButtonDown(0))
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
                    }
                    else
                    {
                        // it is terrain, create new point
                        if (m_Start == null)
                        {
                            m_Start = CreatePoint(hit.point);
                        }
                        else if (m_End == null)
                        {
                            m_End = CreatePoint(hit.point);
                            m_Area = GameObject.Instantiate(ZoneManager.Instance.SurveillanceAreaPrefab).GetComponent<SurveillanceArea>();
                            AreaChanged();
                        }
                    }
                }
            }

            if (m_Current != null && PickerCamera.MouseButton(0))
            {
                // Reposition current point and if area is created, resize it
                Ray ray = m_Camera.Camera.ScreenPointToRay(m_Camera.Module.InputPoint());
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
                {
                    m_Current.transform.position = hit.point;
                    // Update informations
                    UpdatePositionsInputs();

                    if (m_Area != null)
                    {
                        // Resize area
                        AreaChanged();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                // When mouse up, deselect currently selected sphere handle
                m_Current = null;
            }
        }

        /// <summary>
        /// Updates min and max position inputs for rectangular surveillance area.
        /// </summary>
        private void UpdatePositionsInputs()
        {
            if (m_Start != null)
            {
                StartInputX.text = m_Start.transform.position.x.ToString("0.0");
                StartInputZ.text = m_Start.transform.position.z.ToString("0.0");
            }

            if (m_End != null)
            {
                EndInputX.text = m_End.transform.position.x.ToString("0.0");
                EndInputZ.text = m_End.transform.position.z.ToString("0.0");
            }
        }

        /// <summary>
        /// Creates new surveillance area if valid area was created, ie. it has 
        /// two spherical handles (is created) and has name.
        /// </summary>
        public void Confirm()
        {
            // Check if area is created, if not, inform user by red outline
            bool check = m_Start != null;
            CheckInput(check, StartInputX);
            CheckInput(check, StartInputZ);
            check &= m_End != null;
            CheckInput(m_End != null, EndInputX);
            CheckInput(m_End != null, EndInputZ);
            check &= InputText.text != null && InputText.text.Length > 0;
            CheckInput(InputText.text != null && InputText.text.Length > 0, InputText);
            // Check on name availability
            if (m_IsEditMode && m_OriginalName != InputText.text)
                check &= !ZoneManager.Instance.HasZone(InputText.text);
            // Check on name availability
            // We might have zone, but when in edit mode, it is not important
            bool hasZone = ZoneManager.Instance.HasZone(InputText.text) && !(m_IsEditMode && m_OriginalName == InputText.text);
            check &= !hasZone;
            if (hasZone)
            {
                GuiManager.LogWarning("Surveillance area with name " + InputText.text + " already exists.");
                InputText.GetComponent<Outline>().effectColor = Color.red;
                return;
            }
            if (check)
            {
                // It passed all checks, create it
                if (m_IsEditMode)
                {
                    //ZoneManager.Instance.RemoveZone(ZoneManager.Instance.GetSurveillanceArea(m_OriginalName));
                    //m_Area.name = InputText.text;
                    //m_Area.LabelAnchor = m_Area.Max;
                    m_Area.AreaEdited(m_OriginalName, InputText.text,m_Area.Max);
                }
                else
                {
                    ZoneManager.Instance.AddSurveillanceArea(InputText.text, m_Start.transform.position, m_End.transform.position);
                }
                Close();
            }
        }

        /// <summary>
        /// Cleans temporary variables and hides creation window.
        /// </summary>
        public void Close()
        {
            Clean();
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Cleans and destroys all temporary objects.
        /// </summary>
        private void Clean()
        {
            // Make outlines invisible
            InputText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            StartInputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            StartInputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            EndInputX.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            EndInputZ.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0);
            // Set input texts to empty string, so placeholder will be shown
            InputText.text = "";
            StartInputX.text = "";
            StartInputZ.text = "";
            EndInputX.text = "";
            EndInputZ.text = "";
            m_Current = null;
            if (m_Start != null)
                Destroy(m_Start);
            if (m_End != null)
                Destroy(m_End);
            // Destroy object if not editing
            if (m_Area != null && !m_IsEditMode)
                Destroy(m_Area.gameObject);
            m_Area = null;
        }

    }
}
