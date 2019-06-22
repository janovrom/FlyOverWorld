using Assets.Scripts.Nav;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Command;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Agents
{

    /// <summary>
    /// Manages all agent like objects in scene, which includes drones, ground targets 
    /// and even mission. It also registers handlers with simulator client, so it 
    /// can be informed about incoming messages. It is implemented as singleton.
    /// </summary>
    public class AgentManager : MonoBehaviour
    {

        // UAV prefabs
        public Drone WingPrefab;
        public Drone HeliPrefab;
        public Drone RoverPrefab;
        public GroundTarget TargetPrefab;
        public Material FlyPathMaterial;

        // Dictionaries, in which drones, ground targets and missions are hold. Key is their name.
        private Dictionary<string, Drone> m_DroneDictionary;
        private Dictionary<string, GroundTarget> m_GroundTargetDictionary;
        private Dictionary<string, Mission> m_MissionDictionary;
        
        /// <summary>
        /// Assignment of drones to missions. In gui, drone can be in multiple missions, but 
        /// can actively do only one mission, which is recorded in this dictionary.
        /// </summary>
        private Dictionary<string, string> m_DroneOnMission = new Dictionary<string, string>();
        private World m_World;
        private static AgentManager m_Instance;


        private AgentManager() { }

        public static AgentManager Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType(typeof(AgentManager)) as AgentManager;

                    if (!m_Instance)
                    {
                        Debug.LogError("There needs to be one active AgentManager script on a GameObject in your scene.");
                    }
                    else
                    {
                        m_Instance.Init();
                    }
                }

                return m_Instance;
            }
        }


        void Awake()
        {
            // Register commands, which can actually be handled.
            SimulatorClient.Instance.RegisterCommand(Commands.CommandWaypointCompleted, AgentManager.HandleWaypointCompleted);
            SimulatorClient.Instance.RegisterCommand(Commands.InfoUAVAllocation, AgentManager.HandleDroneAllocation);
            SimulatorClient.Instance.RegisterCommand(Commands.InfoMissionExecution, AgentManager.HandleMissionInfo);
            SimulatorClient.Instance.RegisterCommand(Commands.InfoGroundTarget, AgentManager.HandleGroundTarget);
            SimulatorClient.Instance.RegisterCommand(Commands.InfoUAVTelemetry, AgentManager.HandleDroneTelemetry);
            SimulatorClient.Instance.RegisterCommand(Commands.InfoUAVTrajectory, AgentManager.HandleDroneTrajectory);
        }

        void Init()
        {
            if (m_DroneDictionary == null)
                m_DroneDictionary = new Dictionary<string, Drone>();
            if (m_GroundTargetDictionary == null)
                m_GroundTargetDictionary = new Dictionary<string, GroundTarget>();
            if (m_MissionDictionary == null)
                m_MissionDictionary = new Dictionary<string, Mission>();

            m_World = FindObjectOfType<World>();
        }

        /// <summary>
        /// Returns true iff drone is on mission.
        /// </summary>
        /// <param name="drone">inquired drone</param>
        /// <param name="mission">inquired mission</param>
        /// <returns>Returns true iff drone is on mission.</returns>
        public bool IsDroneOnMission(string drone, string mission)
        {
            string outMission;
            if (m_DroneOnMission.TryGetValue(drone, out outMission))
                return mission.Equals(outMission);

            return false;
        }

        /// <summary>
        /// Assigns drone to mission.
        /// </summary>
        /// <param name="drone">assigned drone</param>
        /// <param name="mission">assigned mission</param>
        public void DroneOnMission(string drone, string mission)
        {
            if (m_DroneOnMission.ContainsKey(drone))
            {
                // Already on some mission
                m_DroneOnMission[drone] = mission;
            }
            else
            {
                m_DroneOnMission.Add(drone, mission);
            }
        }

        /// <summary>
        /// Removes drone from current mission, if any, since
        /// it isn't currently doing any mission.
        /// </summary>
        /// <param name="drone">idle drone</param>
        public void DroneIdle(string drone)
        {
            if (m_DroneOnMission.ContainsKey(drone))
            {
                m_DroneOnMission.Remove(drone);
            }
        }

        /// <summary>
        /// Creates new drone or updates its information. Information is
        /// given by simulator.
        /// </summary>
        /// <param name="command">InfoUAVTelemetry command</param>
        private static void HandleDroneTelemetry(Command command)
        {
            InfoUAVTelemetry cmd = command as InfoUAVTelemetry;
            Drone drone;
            if (Instance.m_DroneDictionary.TryGetValue(cmd.UAVId, out drone))
            {
                // Drone already exists, update it
                Nav.Waypoint end = new Waypoint(cmd.Position, (float)cmd.Heading, (float)cmd.GroundSpeedMs,
                    cmd.AutopilotMode, (float)cmd.Battery);
                drone.UpdateDrone(end);
            }
            else
            {
                // Create new drone
                if (cmd.UAVId.Contains(Utility.Constants.PLANE))
                {
                    drone = GameObject.Instantiate(AgentManager.Instance.WingPrefab);
                }
                else if(cmd.UAVId.Contains(Utility.Constants.HELI))
                {
                    drone = GameObject.Instantiate(AgentManager.Instance.HeliPrefab);
                }
                else if (cmd.UAVId.Contains(Utility.Constants.ROVER))
                {
                    drone = GameObject.Instantiate(AgentManager.Instance.RoverPrefab);
                }
                else
                {
                    Debug.LogError("I've gotten unidentifiable UAV. Using wing.");
                    drone = GameObject.Instantiate(AgentManager.Instance.WingPrefab);
                }
                GameObject go = drone.gameObject;
                go.transform.parent = Instance.m_World.transform;
                go.name = cmd.UAVId;
                Waypoint start = new Nav.Waypoint(cmd.Position, (float)cmd.Heading, (float)cmd.GroundSpeedMs,
                    cmd.AutopilotMode, (float)cmd.Battery);
                Waypoint end = start;
                drone.InitDrone(start, end);

                // Add it to dictionary
                Instance.m_DroneDictionary.Add(cmd.UAVId, drone);
            }
        }

        /// <summary>
        /// Creates new ground target or updates its information. Information is
        /// given by simulator.
        /// </summary>
        /// <param name="command">InfoGroundTarget command</param>
        private static void HandleGroundTarget(Command command)
        {
            InfoGroundTarget cmd = command as InfoGroundTarget;
            GroundTarget target;
            if (Instance.m_GroundTargetDictionary.TryGetValue(cmd.GroundTargetId, out target))
            {
                // Ground target already exists
                target.UpdateTarget(cmd.Position);
            }
            else
            {
                // Create new ground target
                target = GameObject.Instantiate(AgentManager.Instance.TargetPrefab);
                GameObject go = target.gameObject;
                go.transform.parent = Instance.m_World.transform;
                go.name = cmd.GroundTargetId;
                target.InitTarget(cmd.Position, cmd.Position);

                // Add it to dictionary
                Instance.m_GroundTargetDictionary.Add(cmd.GroundTargetId, target);
            }
        }

        /// <summary>
        /// Information about mission execution. It is splendidly ignored.
        /// </summary>
        /// <param name="command">InfoMissionExecution command</param>
        private static void HandleMissionInfo(Command command)
        {
            //InfoMissionExecution cmd = command as InfoMissionExecution;
        }

        /// <summary>
        /// Information about drone's assignment. It is splendidly ignored.
        /// </summary>
        /// <param name="command">InfoUAVAllocation command</param>
        private static void HandleDroneAllocation(Command command)
        {
            //InfoUAVAllocation cmd = command as InfoUAVAllocation;
        }

        /// <summary>
        /// Informs user through log about finishing one of his
        /// waypoints.
        /// </summary>
        /// <param name="command">CommandWaypointCompleted command</param>
        private static void HandleWaypointCompleted(Command command)
        {
            CommandWaypointCompleted cmd = command as CommandWaypointCompleted;
            // Check if it is user created waypoint.
            if (ZoneManager.Instance.GetWaypoint(cmd.WaypointName) != null)
            {
                Gui.GuiManager.Log(cmd.ToString());
            }
        }

        /// <summary>
        /// Creates new prediction fly path for drone.
        /// </summary>
        /// <param name="command">InfoUAVTrajectory command</param>
        private static void HandleDroneTrajectory(Command command)
        {
            InfoUAVTrajectory cmd = command as InfoUAVTrajectory;
            Drone drone;
            if (Instance.m_DroneDictionary.TryGetValue(cmd.UAVId, out drone))
            {
                drone.CreateFlyPath(cmd.Trajectory);
            } 
            else
            {
                Debug.LogError("I got trajectory without drone.");
            }
        }

        /// <summary>
        /// Returns ground target specified by name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">inquired name of ground target</param>
        /// <returns>Returns ground target or null if it doesn't exist.</returns>
        public GroundTarget GetTrackableTarget(string name)
        {
            GroundTarget target;
            if (m_GroundTargetDictionary.TryGetValue(name, out target))
                return target;
            else
                return null;
        }

        /// <summary>
        /// Returns drone specified by name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">inquired name of drone</param>
        /// <returns>Returns drone or null if it doesn't exist.</returns>
        public Drone GetDrone(string name)
        {
            Drone drone;
            if (m_DroneDictionary.TryGetValue(name, out drone))
            {
                return drone;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Stops all missions except the one specified.
        /// </summary>
        /// <param name="missionName">mission to be exemp of stopping</param>
        public void StopMissionsExcept(string missionName)
        {

            foreach (Mission m in m_MissionDictionary.Values)
            {
                if (m.Name != missionName)
                    Gui.GuiManager.Instance.MissionStopped(m.Name);
            }
        }

        /// <summary>
        /// Indiscriminately stops all missions.
        /// </summary>
        public void StopAllMissions()
        {
            
            foreach (Mission m in m_MissionDictionary.Values)
            {
                Gui.GuiManager.Instance.MissionStopped(m.Name);
            }
        }

        /// <summary>
        /// Removes given mission and stop its execution for all assigned UAVs.
        /// Even if the UAVs are assigned elsewhere. Removes it also from gui.
        /// </summary>
        /// <param name="m">Mission to be removed.</param>
        public void RemoveMission(Mission m)
        {
            m.Stop();
            m_MissionDictionary.Remove(m.Name);
            Gui.GuiManager.Instance.RemoveMission(m.Name);
        }

        /// <summary>
        /// Returns mission specified by name or null if it doesn't exist.
        /// </summary>
        /// <param name="name">inquired name of mission</param>
        /// <returns>Returns mission or null if it doesn't exist.</returns>
        public Mission GetMission(string name)
        {
            Mission m;
            if (m_MissionDictionary.TryGetValue(name, out m))
                return m;

            return null;
        }

        /// <summary>
        /// Returns true iff mission specified by name exists.
        /// </summary>
        /// <param name="name">name of inquired mission</param>
        /// <returns>Returns true iff mission specified by name exists.</returns>
        private bool MissionExists(string name)
        {
            return m_MissionDictionary.ContainsKey(name);
        }

        /// <summary>
        /// Creates new mission specified by name and list of pickable objects,
        /// which usually come from current selection. These objects are 
        /// assigned to mission.
        /// </summary>
        /// <param name="name">name of mission</param>
        /// <param name="objects">objects assigned to this mission</param>
        /// <returns>Returns false, iff mission with given name already exists.</returns>
        public bool CreateMission(string name, List<Gui.Pickable> objects)
        {
            bool exists = MissionExists(name);
            if (exists)
            {
                Gui.GuiManager.LogWarning("Mission with name " + name + " already exists.");
                return false;
            }
            else
            {
                Mission m = new Mission(name);
                List<string> targets = new List<string>();

                foreach (Gui.Pickable o in objects)
                {
                    m.Add(o);
                }
                m.TrackTargets(targets);
                m_MissionDictionary.Add(name, m);
                return true;
            }
        }

        public List<Mission> Missions
        {
            get
            {
                return m_MissionDictionary.Values.ToList<Mission>();
            }
        }

        public List<string> DroneNameList
        {
            get
            {
                return m_DroneDictionary.Keys.ToList<string>();
            }
        }

    }
}
