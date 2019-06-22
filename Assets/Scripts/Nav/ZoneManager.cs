using Assets.Scripts.Network.Command;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Handles operations on zones and stores their state.
    /// </summary>
    class ZoneManager : MonoBehaviour
    {

        private static ZoneManager m_Instance;
        // public prefabs
        public GameObject CylinderPrefab;
        public GameObject SurveillanceAreaPrefab;
        public GameObject WaypointsPrefab;
        // Zones containers
        Dictionary<string, List<string>> m_DronesInZone = new Dictionary<string, List<string>>();
        List<NoFlyZone> m_NoFlyZones = new List<NoFlyZone>();
        List<SurveillanceArea> m_SurveillanceAreas = new List<SurveillanceArea>();
        List<TargetWaypoint> m_Waypoints = new List<TargetWaypoint>();
        private Cameras.PickerCamera m_Camera;

        private ZoneManager()
        {
        }

        public static ZoneManager Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType(typeof(ZoneManager)) as ZoneManager;

                    if (!m_Instance)
                    {
                        Debug.LogError("There needs to be one active ZoneManager script on a GameObject in your scene.");
                    }
                    else
                    {
                        m_Instance.Init();
                    }
                }
                return m_Instance;
            }
        }

        /// <summary>
        /// Selects surveillance area given position, since it would be hard to give bounding box
        /// to projector.
        /// </summary>
        /// <param name="point">point which is tested if it lies in surveillance area</param>
        public void SelectZone(Vector3 point)
        {
            foreach (SurveillanceArea area in m_SurveillanceAreas)
            {
                if (point.x >= area.Min.x && point.x <= area.Max.x && point.z >= area.Min.z && point.z <= area.Max.z)
                {
                    m_Camera.SelectObject(area);
                }
            }
        }

        /// <summary>
        /// Renames zone to new name. Renames zones in missions.
        /// </summary>
        /// <param name="oldName">old name</param>
        /// <param name="zone">updated zone</param>
        public void ZoneRenamed(string oldName, Zone zone)
        {
            // Don't do anything, when not actually renamed
            if (oldName == zone.name)
                return;

            zone.GuiLabel.Name = zone.name;

            List<string> names;
            if (m_DronesInZone.TryGetValue(oldName, out names))
            {
                foreach (string name in names)
                {
                    Gui.GuiManager.Instance.OnZoneExit(oldName, name);
                }
                m_DronesInZone.Remove(oldName);
                Gui.GuiManager.Instance.OnZoneDestroy(oldName);
                Gui.GuiManager.Instance.OnZoneEnter(zone, null);
                m_DronesInZone.Add(zone.name, names);

                foreach (Agents.Mission m in Agents.AgentManager.Instance.Missions)
                {
                    if (zone is TargetWaypoint)
                    {
                        m.RenameAllWaypoints(oldName, zone.name);
                    }
                    else
                    {
                        if (m.RemoveZone(oldName))
                        {
                            m.Add(zone);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove zone from scene and associated mission.
        /// </summary>
        /// <param name="zone"></param>
        public void RemoveZone(Zone zone)
        {
            if (zone is NoFlyZone)
            {
                m_NoFlyZones.Remove(zone as NoFlyZone);
            }
            else if (zone is SurveillanceArea)
            {
                // when SA removed, remove it from all missions
                foreach (Agents.Mission m in Agents.AgentManager.Instance.Missions)
                {
                    m.RemoveZone(zone.name);
                }
                m_SurveillanceAreas.Remove(zone as SurveillanceArea);
            }
            else if (zone is TargetWaypoint)
            {
                foreach (Agents.Mission m in Agents.AgentManager.Instance.Missions)
                {
                    m.RemoveAllWaypoints(zone.name);
                }
                m_Waypoints.Remove(zone as TargetWaypoint);
            }

            List<string> names;
            if (m_DronesInZone.TryGetValue(zone.name, out names))
            {
                foreach (string name in names)
                {
                    Gui.GuiManager.Instance.OnZoneExit(zone.name, name);
                }

                Gui.GuiManager.Instance.OnZoneDestroy(zone.name);
            }

            m_DronesInZone.Remove(zone.name);
            //Gui.GuiManager.Instance.RemoveBillboard(zone.name);
        }

        /// <summary>
        /// Returns NoFlyZone given a name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Returns NoFlyZone given a name or null if it doesn't exist.</returns>
        public NoFlyZone GetNoFlyZone(string name)
        {
            foreach (NoFlyZone area in m_NoFlyZones)
            {
                if (area.name == name)
                    return area;
            }

            return null;
        }

        /// <summary>
        /// Returns TargetWaypoint given a name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Returns TargetWaypoint given a name or null if it doesn't exist.</returns>
        public TargetWaypoint GetWaypoint(string name)
        {
            foreach (TargetWaypoint area in m_Waypoints)
            {
                if (area.name == name)
                    return area;
            }

            return null;
        }

        /// <summary>
        /// Returns SurveillanceArea given a name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Returns SurveillanceArea given a name or null if it doesn't exist.</returns>
        public SurveillanceArea GetSurveillanceArea(string name)
        {
            foreach(SurveillanceArea area in m_SurveillanceAreas)
            {
                if (area.name == name)
                    return area;
            }

            return null;
        }

        /// <summary>
        /// Add new no-fly zone if it doesn't yet exist.
        /// </summary>
        /// <param name="name">name of zone</param>
        /// <param name="center">center of zone relative to ground</param>
        /// <param name="position">absolute position of zone</param>
        /// <param name="radius">radius of zone</param>
        /// <param name="height">height of zone</param>
        /// <param name="type">type of zone</param>
        /// <returns>Returns true iff zone was created.</returns>
        public bool AddNoFlightZone(string name, Vector3 center, Vector3 position, float radius, float height, NoFlyZone.ZoneType type)
        {
            if (!m_DronesInZone.ContainsKey(name))
            {
                m_DronesInZone.Add(name, new List<string>());
                NoFlyZone zone = NoFlyZone.Initialize(type, center, position, name, radius, height).GetComponent<NoFlyZone>();
                m_NoFlyZones.Add(zone);
                CommandNoFlyZone nfz = new CommandNoFlyZone(Commands.CommandCreateNoFlyZone, Command.BROADCAST_ADDRESS,
                    name, (byte)type, center, radius, height);
                Network.SimulatorClient.Instance.Send(nfz);
                Gui.GuiManager.Instance.OnZoneEnter(zone, null);
                return true;
            }
            else
            {
                Gui.GuiManager.LogWarning("No-fly zone with name " + name + " already exists.");
                return false;
            }
        }

        /// <summary>
        /// Creates new TargetWaypoint from given CartesianWaypoint.
        /// </summary>
        /// <param name="waypoint">given CartesianWaypoint</param>
        /// <param name="position">given absolute position</param>
        /// <returns>Return true iff created.</returns>
        public bool AddWaypoint(CartesianWaypoint waypoint, Vector3 position)
        {
            if(!m_DronesInZone.ContainsKey(waypoint.Name))
            {
                m_DronesInZone.Add(waypoint.Name, new List<string>());
                TargetWaypoint w = TargetWaypoint.Initialize(waypoint.Name, waypoint, position).GetComponent<TargetWaypoint>();
                m_Waypoints.Add(w);
                // command will be send when mission is created
                Gui.GuiManager.Instance.OnZoneEnter(w, null);
                return true;
            }
            else
            {
                Gui.GuiManager.LogWarning("Waypoint with name " + waypoint.Name + " already exists.");
                return false;
            }
        }

        /// <summary>
        /// Creates new surveillance area if it doesn't yet exist.
        /// </summary>
        /// <param name="name">name of zone</param>
        /// <param name="start">start of zone</param>
        /// <param name="end">end of zone</param>
        /// <returns>Returns true iff zone was created.</returns>
        public bool AddSurveillanceArea(string name, Vector3 start, Vector3 end)
        {
            if (!m_DronesInZone.ContainsKey(name))
            {
                m_DronesInZone.Add(name, new List<string>());
                SurveillanceArea zone = SurveillanceArea.Initialize(name, start, end).GetComponent<SurveillanceArea>();
                m_SurveillanceAreas.Add(zone);
                // command will be send when mission is created
                Gui.GuiManager.Instance.OnZoneEnter(zone, null);
                return true;
            }
            else
            {
                Gui.GuiManager.LogWarning("Surveillance area with name " + name + " already exists.");
                return false;
            }
        }

        /// <summary>
        /// Drone entered some zone, especially no-fly zone. Also
        /// inform GuiManager about this.
        /// </summary>
        /// <param name="noFlightZone">name of no-fly zone</param>
        /// <param name="droneName">name of intruding drone</param>
        public void OnDroneEnter(string noFlightZone, string droneName)
        {
            if (!m_DronesInZone.ContainsKey(noFlightZone))
            {
                m_DronesInZone.Add(noFlightZone, new List<string>());
            }
            List<string> drones;
            m_DronesInZone.TryGetValue(noFlightZone, out drones);
            if (!drones.Contains(droneName))
            {
                drones.Add(droneName);
                Gui.GuiManager.Instance.OnZoneEnter(GetNoFlyZone(noFlightZone), Agents.AgentManager.Instance.GetDrone(droneName));
                // Inform GUI on troubles
                Gui.GuiManager.LogWarning("Drone " + droneName + " entered no-flight zone " + noFlightZone + ".");
            }
        }

        /// <summary>
        /// Checks if there is some zone of given name.
        /// </summary>
        /// <param name="name">inquired zone name</param>
        /// <returns>Returns true if no zone with given name exist.</returns>
        public bool HasZone(string name)
        {
            return m_DronesInZone.ContainsKey(name);
        }

        /// <summary>
        /// Intruding drone left some zone, especially no-fly zone. Also
        /// informs GuiManager about this change.
        /// </summary>
        /// <param name="noFlightZone">zone name</param>
        /// <param name="droneName">drone name</param>
        public void OnDroneExit(string noFlightZone, string droneName)
        {
            List<string> drones;
            m_DronesInZone.TryGetValue(noFlightZone, out drones);
            drones.Remove(droneName);
            Gui.GuiManager.Instance.OnZoneExit(noFlightZone, droneName);
        }

        void Init()
        {
            m_Camera = FindObjectOfType<Cameras.PickerCamera>();
        }

    }
}
