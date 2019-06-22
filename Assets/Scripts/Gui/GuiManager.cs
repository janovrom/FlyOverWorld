using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;
using Assets.Scripts.Agents;
using Assets.Scripts.Nav;
using System;

namespace Assets.Scripts.Gui
{

    /// <summary>
    /// Complete killer class between all classes and managers. Contains all UI prefabs
    /// and handles communication between any other component and gui. It is implemented
    /// as singleton.
    /// </summary>
    public class GuiManager : MonoBehaviour
    {

        private static GuiManager m_Instance;
        // Registered panel's positions and sizes for draggable and resizable windows.
        private Dictionary<string, Vector3> m_RegisteredPanels = new Dictionary<string, Vector3>();
        private Dictionary<string, ScrollSizes> m_RegisteredPanelsSizes = new Dictionary<string, ScrollSizes>();
        private Cameras.PickerCamera m_Camera;
        // Resizable draggable windows
        public RectTransform UAVWindow;
        public RectTransform InfoWindow;
        public RectTransform MissionWindow;
        public RectTransform ZonesWindow;
        // All expand layouts for informations
        public ExpandLayout OffScreenPanel;
        public ExpandLayout OnScreenPanel;
        public ExpandLayout UavInfo;
        public ExpandLayout Mission;
        public ExpandLayout Zones;
        // Additional information screens
        public Logger Logger;
        public RectTransform BillboardPanel;
        public RectTransform LoadingScreen;
        public RectTransform AttributionScreen;
        public RectTransform HelpScreen;
        public SettingsManager SettingsScreen;
        // Prefabs
        public GameObject ElementPrefab;
        public GameObject PanelPrefab;
        public RectTransform MissionButtonPrefab;
        public RectTransform BillboardPrefab;
        public GameObject DeletableElementPrefab;
        public GameObject DividingLinePrefab;
        // Gui icons
        public Texture2D DragSprite;
        public Texture2D DropSprite;
        public Texture2D CursorSprite;
        public Texture2D CursorCorner;
        public Texture2D CursorDragValue;
        public Material GuiMaterial;
        // Position in world
        public Text Position;
        public Text GPSLatitude;
        public Text GPSLongitude;


        private GuiManager()
        {
        }

        public static GuiManager Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType(typeof(GuiManager)) as GuiManager;

                    if (!m_Instance)
                    {
                        Debug.LogError("There needs to be one active GuiManager script on a GameObject in your scene.");
                    }
                }
                return m_Instance;
            }
        }

        void Awake()
        {
            Instance.Init();
        }

        /// <summary>
        /// Initialize cursor and load saved window's positions and sizes.
        /// </summary>
        void Init()
        {
            // Init cursor
            Cursor.SetCursor(GuiManager.Instance.CursorSprite, Vector2.zero, CursorMode.Auto);

            // Init variables
            m_Camera = FindObjectOfType<Cameras.PickerCamera>();

            // Load panels
            FileStream f;
            if (!File.Exists(Utility.Settings.GUI_PANELS_FILE_PATH))
            {
                f = File.Create(Utility.Settings.GUI_PANELS_FILE_PATH);
            }
            else
            {
                f = File.OpenRead(Utility.Settings.GUI_PANELS_FILE_PATH);
            }

            StreamReader sr = new StreamReader(f);
            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                string[] data = line.Split(';');
                if (data.Length < 4)
                    continue;
                if (data[0].Equals("@sizes"))
                {
                    // Handle sizes
                    if (data.Length != 7)
                        continue;
                    ScrollSizes sizes = new ScrollSizes();
                    if (!float.TryParse(data[2], out sizes.PanelX))
                        continue;
                    if (!float.TryParse(data[3], out sizes.PanelY))
                        continue;
                    if (!float.TryParse(data[4], out sizes.ScrollY))
                        continue;
                    if (!float.TryParse(data[5], out sizes.ContentX))
                        continue;
                    if (!float.TryParse(data[6], out sizes.HeaderX))
                        continue;
                    m_RegisteredPanelsSizes.Add(data[1], sizes);
                }
                else
                {
                    // Handle position
                    if (data.Length != 4)
                        continue;
                    float x, y, z;
                    if (!float.TryParse(data[1], out x))
                        continue;
                    if (!float.TryParse(data[2], out y))
                        continue;
                    if (!float.TryParse(data[3], out z))
                        continue;
                    m_RegisteredPanels.Add(data[0], new Vector3(x, y, z));
                }
            }
            sr.Close();
        }

        /// <summary>
        /// Redraw gui element with new gps and cartesian position.
        /// </summary>
        /// <param name="xyz">cartesian position</param>
        /// <param name="gps">gps position</param>
        public void PositionInWorldChanged(Vector3 xyz, Vector2 gps)
        {
            Position.text = xyz.ToString();
            GPSLatitude.text = "Latitude="+gps.x.ToString()+"°";
            GPSLongitude.text = "Longitude=" + gps.y.ToString() + "°";
        }

        /// <summary>
        /// Show all or only mission window.
        /// </summary>
        /// <param name="use">if true, shows only mission window</param>
        public void UseGameUI(bool use)
        {
            UAVWindow.gameObject.SetActive(!use);
            InfoWindow.gameObject.SetActive(!use);
            ZonesWindow.gameObject.SetActive(!use);
        }

        /// <summary>
        /// Display settings window.
        /// </summary>
        public void ShowSettings()
        {
            SettingsScreen.Show();
        }

        /// <summary>
        /// Display help window.
        /// </summary>
        public void ShowHelp()
        {
            HelpScreen.gameObject.SetActive(!HelpScreen.gameObject.activeInHierarchy);
        }

        /// <summary>
        /// Wait for one second and then disable loading screen.
        /// </summary>
        System.Collections.IEnumerator WaitAndDisableLoadingScreen()
        {
            yield return new WaitForSeconds(1.0f);
            LoadingScreen.gameObject.SetActive(false);
        }

        /// <summary>
        /// Slowly increase transparency. Speed is defined by time.
        /// </summary>
        /// <param name="timeS">How long will be object visible.</param>
        /// <returns></returns>
        System.Collections.IEnumerator DisipateAlpha(float timeS)
        {
            Image img = LoadingScreen.gameObject.GetComponent<Image>();
            float time = timeS;
            while (time > 0.0f)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a - 0.8f * Time.deltaTime);
                time -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Inform gui and PickerCamera about loading of first terrain chunk. 
        /// Also start making laoding screen transparent.
        /// </summary>
        public void FirstChunkLoaded()
        {
            m_Camera.PositionCamera();
            Network.SimulatorClient.Instance.StartCommunication();
            AttributionScreen.gameObject.SetActive(false);
            StartCoroutine(DisipateAlpha(1.0f));
        }

        /// <summary>
        /// Inform PickerCamera about loading first circle of terrain, enable interaction
        /// and hide loading screen.
        /// </summary>
        public void LoadingTerrainFinished()
        {
            if (LoadingScreen.gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitAndDisableLoadingScreen());
                m_Camera.EnableInteraction();
            } // else: already enabled once
        }

        #region MISSION Contains methods handling gui for missions
        /// <summary>
        /// Mission specified by name was changed and needs to be redrawn in gui.
        /// </summary>
        /// <param name="name">name of changed mission</param>
        public void RedrawMission(string name)
        {
            // Get transform for mission - button with MissionStartStop
            if (Mission == null)
                return;
            Transform missionTransform = GetChild(Mission.transform, name);
            if (missionTransform == null)
                return;

            // Add content to mission
            MissionStartStop mss = missionTransform.gameObject.GetComponent<MissionStartStop>();
            mss.AddContent();
        }

        /// <summary>
        /// Add buttons to mission.
        /// </summary>
        /// <param name="m">changed mission</param>
        /// <param name="drones">expandLayout for drones</param>
        /// <param name="targets">ExpandLayout for ground targets</param>
        /// <param name="areas">ExpandLayout for surveillance areas</param>
        /// <param name="waypoints">ExpandLayout for waypoints</param>
        internal void AddMissionContent(Mission m, ExpandLayout drones, ExpandLayout targets, ExpandLayout areas, ExpandLayout waypoints)
        {
            if (m == null)
                return;

            if (drones != null)
            {
                drones.AddText("Assigned drones:");
                foreach (string drone in m.Drones)
                {
                    Drone agent = AgentManager.Instance.GetDrone(drone);
                    drones.AddMissionDeletableButton(agent,
                        delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(agent); },
                        delegate () { m.RemoveDrone(drone); },
                        false
                        );
                }
            }

            if (targets != null)
            {
                targets.AddText("Tracking targets:");
                foreach (string target in m.Targets)
                {
                    GroundTarget agent = AgentManager.Instance.GetTrackableTarget(target);
                    targets.AddMissionDeletableButton(agent,
                        delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(agent); },
                        delegate () { m.RemoveTarget(target); },
                        false
                        );
                }
            }

            if (areas != null)
            {
                areas.AddText("Surveillance areas:");
                foreach (SurveillanceArea area in m.SurveillanceAreas)
                {
                    string n = area.name;
                    areas.AddMissionDeletableButton(area,
                        delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(area); },
                        delegate () { m.RemoveZone(n); },
                        false
                        );
                }
            }

            if (waypoints != null)
            {
                waypoints.AddText("Waypoints:");
                Dictionary<int, CartesianWaypoint> Waypoints = m.Waypoints;
                foreach (int index in m.WaypointOrder)
                {
                    CartesianWaypoint cw = Waypoints[index];
                    Pickable p = ZoneManager.Instance.GetWaypoint(cw.Name);
                    int id = index;
                    waypoints.AddMissionDeletableButton(p,
                        delegate () { FindObjectOfType<Cameras.PickerCamera>().SelectObject(p); },
                        delegate () { m.RemoveWaypoint(id); },
                        false, m.Name
                        );
                }
            }
        }

        /// <summary>
        /// Remove mission and its children from gui.
        /// </summary>
        /// <param name="name">name of removed mission</param>
        public void RemoveMission(string name)
        {
            RemoveChild(Mission.transform, name);
        }

        /// <summary>
        /// Mission stopped. Change its start/play button.
        /// </summary>
        /// <param name="name">name of stopped mission</param>
        public void MissionStopped(string name)
        {
            // Find button with start/stop
            Transform m = GetChild(Mission.transform, name);
            if (m != null)
            {
                m.GetComponentInChildren<MissionStartStop>().Stop();
            }
        }

        /// <summary>
        /// New mission was created. Add it to mission window
        /// </summary>
        /// <param name="name"></param>
        public void MissionCreated(string name)
        {
            Mission m = AgentManager.Instance.GetMission(name);
            if (m != null)
            {
                Mission.AddButton(name, MissionButtonPrefab);
            }
        }
        #endregion

        #region SELECTION Handles selection of pickable objects
        /// <summary>
        /// Selects pickable and adds its information to selection window.
        /// </summary>
        /// <param name="o"></param>
        public void PickableSelected(Pickable o)
        {
            if (UavInfo.Contains(o.name))
            {
                // Update
                ExpandLayout panel = UavInfo.Get(o.name + "_panel").GetComponent<ExpandLayout>();
                string[] values = o.Values();
                for (int i = 0; i < panel.transform.childCount; ++i)
                {
                    panel.transform.GetChild(i).GetComponentInChildren<Text>().text = values[i];
                }
            }
            else
            {
                // Create new
                UavInfo.AddText(o.name);
                ExpandLayout panel = UavInfo.AddPanel(o.name + "_panel");
                foreach (string val in o.Values())
                    panel.AddText(val);
            }
        }

        /// <summary>
        /// Deselects pickable so remove its information from selection window.
        /// </summary>
        /// <param name="o"></param>
        public void PickableDeselected(Pickable o)
        {
            UavInfo.Remove(o.name);
            UavInfo.Remove(o.name + "_panel");
        }

        /// <summary>
        /// Remove all information from selection window.
        /// </summary>
        public void DeselectAll()
        {
            UavInfo.Clear();
        }
        #endregion

        /// <summary>
        /// Return child for specified transform and name.
        /// </summary>
        /// <param name="transform">parent transform</param>
        /// <param name="name">name of child</param>
        /// <returns>Returns child transform for specified transform and name.</returns>
        internal Transform GetChild(Transform transform, string name)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                if (transform.GetChild(i).name == name)
                {
                    return transform.GetChild(i);
                }
            }

            return null;
        }

        /// <summary>
        /// Removes child specified by name from parent transform.
        /// </summary>
        /// <param name="transform">parent transform</param>
        /// <param name="name">name of child</param>
        internal void RemoveChild(Transform transform, string name)
        {
            for (int i = transform.childCount-1; i >= 0; --i)
            {
                if (transform.GetChild(i).name == name)
                {
                    Destroy(transform.GetChild(i).gameObject);
                    return;
                }
            }
        }

        #region ZONE Handles zones creation and destruction and whether drone entered particular zone.
        /// <summary>
        /// Remove zone and its children from gui.
        /// </summary>
        /// <param name="zoneName">name of destroyed zone</param>
        public void OnZoneDestroy(string zoneName)
        {
            if (Zones && Zones.transform == null)
                return;

            for (int i = 0; i < Zones.transform.childCount; ++i)
            {
                if (Zones.transform.GetChild(i).name == zoneName)
                {
                    // Destroy immediate changes immediately hierarchy, so object on i+1 will be on i.
                    // Text
                    DestroyImmediate(Zones.transform.GetChild(i).gameObject);
                    // ExpandLayout
                    DestroyImmediate(Zones.transform.GetChild(i).gameObject);
                    break;
                }
            }
        }

        /// <summary>
        /// Drone moved out from zone.
        /// </summary>
        /// <param name="zoneName">name of zone</param>
        /// <param name="agentName">name of agent who got out</param>
        public void OnZoneExit(string zoneName, string agentName)
        {
            if (Zones && Zones.transform == null)
                return;

            for (int i = 0; i < Zones.transform.childCount; ++i)
            {
                if (Zones.transform.GetChild(i).name == zoneName)
                {
                    // ExpandLayout
                    RemoveChild(Zones.transform.GetChild(i+1), agentName);
                    break;
                }
            }
        }

        /// <summary>
        /// Called when some agent appears in zone.
        /// </summary>
        /// <param name="zone">zone in which agent intruded</param>
        /// <param name="agent">intruder agent</param>
        public void OnZoneEnter(Zone zone, Agents.Agent agent)
        {
            if (zone == null || Zones.transform == null)
                return;

            ExpandLayout panel = null;
            for (int i = 0; i < Zones.transform.childCount; ++i)
            {
                if (Zones.transform.GetChild(i).name == zone.name)
                {
                    panel = Zones.transform.GetChild(i+1).GetComponent<ExpandLayout>();
                    break;
                }
            }

            // doesn't contain zone
            if (panel == null)
            {
                Zones.AddButton(zone, true);
                panel = Zones.AddPanel(zone.name + "_panel");
            }

            // can be null when zone is created
            if (agent == null)
                return;

            if (agent is Agents.Drone)
            {
                panel.AddButton(agent, true);
            } else
            {
                panel.AddButton(agent.name, null);
            }
        }

        #endregion

        /// <summary>
        /// Switch agent in agent window from on screen to off screen.
        /// </summary>
        /// <param name="agent">agent who got off screen</param>
        public void AgentOffScreen(Agents.Agent agent)
        {
            OnScreenPanel.Remove(agent.name);
            OffScreenPanel.AddButton(agent, true);
        }

        /// <summary>
        /// Switch agent in agent window from of screen to on screen.
        /// </summary>
        /// <param name="agent">agent who got on screen</param>
        public void AgentOnScreen(Agents.Agent agent)
        {
            OffScreenPanel.Remove(agent.name);
            OnScreenPanel.AddButton(agent, true);
        }

        /// <summary>
        /// Registers panel for future saving in file. Only position is
        /// saved. 
        /// </summary>
        /// <param name="panel">DragPanel to save</param>
        /// <returns>Returns saved position or default value if not yet registered.</returns>
        public Vector3 RegisterPanel(RectTransform panel)
        {
            if (!m_RegisteredPanels.ContainsKey(panel.name))
                m_RegisteredPanels.Add(panel.name, panel.localPosition);

            Vector3 retPos;
            m_RegisteredPanels.TryGetValue(panel.name, out retPos);
            return retPos;
        }

        /// <summary>
        /// Registers panel size for future saving in file. Saves
        /// sizes of Unity ScrollArea (prefab ResizableDragPanel).
        /// </summary>
        /// <param name="name">name of panel to save</param>
        /// <param name="sizes">object containing 5 required sizes</param>
        /// <returns></returns>
        public ScrollSizes RegisterPanelSize(string name, ScrollSizes sizes)
        {
            if (!m_RegisteredPanelsSizes.ContainsKey(name))
                m_RegisteredPanelsSizes.Add(name, sizes);

            ScrollSizes retSizes;
            m_RegisteredPanelsSizes.TryGetValue(name, out retSizes);
            return retSizes;
        }

        #region LOGGING Methods which access Logger component assigned to this manager.

        /// <summary>
        /// Logs error, which is shown as a red colored popup panel on side of screen.
        /// This method can be used to show already used message or refresh its
        /// screen time.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        /// <param name="priority">Can be said, how important the message is. If error is of Message.TopPriority
        /// it is shown indefinitely. Default value is your usual error.</param>
        public static void LogError(Logger.Message message, int priority = 0)
        {
            Instance.Logger.LogError(message, priority);
        }

        /// <summary>
        /// Logs warning, which is shown as a orange colored popup panel on side of screen.
        /// This method can be used to show already used message or refresh its
        /// screen time.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        public static void LogWarning(Logger.Message message)
        {
            Instance.Logger.LogWarning(message);
        }

        /// <summary>
        /// Logs message, which is shown as a green colored popup panel on side of screen.
        /// This method can be used to show already used message or refresh its
        /// screen time.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        public static void Log(Logger.Message message)
        {
            Instance.Logger.Log(message);
        }

        /// <summary>
        /// Logs error, which is shown as a red colored popup panel on side of screen.
        /// This method is used when loggin one time message. Cannot be forcibly removed
        /// using scripting.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        /// <param name="priority">Can be said, how important the message is. If error is of Message.TopPriority
        /// it is shown indefinitely. Default value is your usual error.</param>
        public static void LogError(string message, int priority = 0)
        {
            Instance.Logger.LogError(new Logger.Message(message), priority);
        }

        /// <summary>
        /// Logs warning, which is shown as a orange colored popup panel on side of screen.
        /// This method is used when loggin one time message. Cannot be forcibly removed
        /// using scripting.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        public static void LogWarning(string message)
        {
            Instance.Logger.LogWarning(new Logger.Message(message));
        }

        /// <summary>
        /// Logs message, which is shown as a green colored popup panel on side of screen.
        /// This method is used when loggin one time message. Cannot be forcibly removed
        /// using scripting.
        /// </summary>
        /// <param name="message">Message to be displayed.</param>
        public static void Log(string message)
        {
            Instance.Logger.Log(new Logger.Message(message));
        }

        /// <summary>
        /// Removes message immediately withou any animation or waiting.
        /// </summary>
        /// <param name="message">Message to be removed</param>
        public static void RemoveMessageImmediate(Logger.Message message)
        {
            Instance.Logger.RemoveMessageImmediate(message);
        }

        /// <summary>
        /// Removes message after some time and with animation.
        /// </summary>
        /// <param name="message">Message to be removed</param>
        public static void RemoveMessage(Logger.Message message)
        {
            Instance.Logger.RemoveMessage(message);
        }
        #endregion

        /// <summary>
        /// Save size of panel specified by its name.
        /// </summary>
        /// <param name="name">name of changed panel</param>
        /// <param name="sizes">changed sizes</param>
        public void PanelChanged(string name, ScrollSizes sizes)
        {
            m_RegisteredPanelsSizes[name] = sizes;
            Save();
        }

        /// <summary>
        /// Save position of panel specified by its name.
        /// </summary>
        /// <param name="name">name of changed panel</param>
        /// <param name="position">changed position</param>
        public void PanelChanged(string name, Vector3 position)
        {
            m_RegisteredPanels[name] = position;
            Save();
        }

        /// <summary>
        /// Save all registered panels to file panels.txt. In format name;x;y;z and
        /// @sizes;name;panelx;panely;scroll area y;content x;header x
        /// </summary>
        private void Save()
        {
            if (!File.Exists(Utility.Settings.GUI_PANELS_FILE_PATH))
            {
                File.Create(Utility.Settings.GUI_PANELS_FILE_PATH);
            }

            StreamWriter sw = new StreamWriter(File.OpenWrite(Utility.Settings.GUI_PANELS_FILE_PATH));

            foreach (KeyValuePair<string, Vector3> panel in m_RegisteredPanels)
            {
                sw.WriteLine(string.Format("{0};{1};{2};{3}", panel.Key, panel.Value.x, panel.Value.y, panel.Value.z));
            }

            foreach (KeyValuePair<string, ScrollSizes> panel in m_RegisteredPanelsSizes)
            {
                sw.WriteLine(string.Format("@sizes;{0};{1};{2};{3};{4};{5}", panel.Key, panel.Value.PanelX, 
                    panel.Value.PanelY, panel.Value.ScrollY, panel.Value.ContentX, panel.Value.HeaderX));
            }
            sw.Close();
        }

        [Obsolete]
        public void AddBillboard(Pickable center, float dist, string name, Color color, UnityAction action)
        {
            RectTransform rect = Instantiate(BillboardPrefab);
            rect.transform.SetParent(BillboardPanel);
            rect.name = name;
            rect.gameObject.GetComponent<Button>().onClick.AddListener(action);
            rect.GetComponent<Billboard>().Init(center, dist, name, color);
        }

        [Obsolete]
        public void RemoveBillboard(string name)
        {
            for (int i = 0; i < BillboardPanel.childCount; ++i)
            {
                if (BillboardPanel.GetChild(i).name == name)
                {
                    DestroyImmediate(BillboardPanel.GetChild(i).gameObject);
                    return;
                }
            }
        }
    }
}