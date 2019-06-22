using Assets.Scripts.Nav;
using Assets.Scripts.Network.Command;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Agents
{

    /// <summary>
    /// Stores mission and if started, creates commands and sends them to simulator client.
    /// </summary>
    public class Mission
    {

        /// <summary>
        /// Id counter for waypoints, since waypoint can be the same, but added multiple times.
        /// </summary>
        private int m_WaypointId = 0;
        private string m_MissionName;
        private List<string> m_AssignedDrones;
        private List<string> m_Zones;
        private List<Command> m_Commands;
        /// <summary>
        /// Fly path waypoints stored by they specific integer key.
        /// </summary>
        private Dictionary<int, CartesianWaypoint> m_Waypoints;
        /// <summary>
        /// Sorted list of integer keys for given waypoints.
        /// </summary>
        private List<int> m_WaypointsKeysOrder;
        /// <summary>
        /// If mission is currently running. It can run without any agent doing the mission.
        /// </summary>
        private bool m_IsPlaying = false;
    

        public Mission(string missionName)
        {
            m_MissionName = missionName;
            m_AssignedDrones = new List<string>();
            m_Zones = new List<string>();
            m_Commands = new List<Command>();
            m_Waypoints = new Dictionary<int, CartesianWaypoint>();
            m_WaypointsKeysOrder = new List<int>();
        }

        /// <summary>
        /// Stops mission and adds new waypoint on fly path.
        /// </summary>
        /// <param name="waypoint">added waypoint</param>
        /// <returns>Returns true if waypoint can be added.</returns>
        public bool AddWaypoint(CartesianWaypoint waypoint)
        {
            StopActions();
            int wId = m_WaypointId++;
            m_WaypointsKeysOrder.Add(wId);
            m_Waypoints.Add(wId, waypoint);
            ValuesChanged();
            return true;
        }

        /// <summary>
        /// Moves waypoint from old position to new position.
        /// </summary>
        /// <param name="oldPosition">old position of waypoint</param>
        /// <param name="newPosition">new position of waypoint</param>
        public void MoveWaypointBefore(int oldPosition, int newPosition)
        {
            StopActions();
            int key = m_WaypointsKeysOrder[oldPosition];
            m_WaypointsKeysOrder.RemoveAt(oldPosition);
            // Insert(index, item)
            m_WaypointsKeysOrder.Insert(newPosition, key);
            // Don't inform gui, it was already moved another way
        }

        /// <summary>
        /// Adds new drone specified by name. Stops the mission.
        /// </summary>
        /// <param name="name">name of added drone</param>
        /// <returns>Returns true iff drone cane be added.</returns>
        public bool AddDrone(string name)
        {
            Drone drone = AgentManager.Instance.GetDrone(name);
            return AddDrone(drone);
        }

        /// <summary>
        /// Adds new drone specified by name. Stops the mission.
        /// </summary>
        /// <param name="drone">drone to be added</param>
        /// <returns>Returns true iff drone cane be added.</returns>
        public bool AddDrone(Drone drone)
        {
            if (m_AssignedDrones.Contains(drone.name))
                return false;

            StopActions();
            if (drone == null)
                return false;
            else
            {
                m_AssignedDrones.Add(drone.name);
                ValuesChanged();
                return true;
            }
        }

        /// <summary>
        /// Adds pickable to mission if it can be added.
        /// </summary>
        /// <param name="p">pickable to add to mission</param>
        public void Add(Gui.Pickable p)
        {
            if (p is Drone)
            {
                AddDrone(p as Drone);
            }
            else if (p is GroundTarget)
            {
                TrackTargets(new System.Collections.Generic.List<string>() { p.name });
            }
            else if (p is Nav.SurveillanceArea)
            {
                SurveyRectangleArea(p as Nav.SurveillanceArea);
            }
            else if (p is Nav.TargetWaypoint)
            {
                AddWaypoint((p as Nav.TargetWaypoint).CartesianWaypoint);
            }
        }

        /// <summary>
        /// Stops executing this mission.
        /// </summary>
        public void StopActions()
        {
            if (m_IsPlaying)
            {
                Gui.GuiManager.Instance.MissionStopped(m_MissionName);
                Stop();
            } // else: Not playing. It has no reason to stop.
        }

        /// <summary>
        /// Adds stop command to mission.
        /// </summary>
        public void AddStop()
        {
            m_Commands.Add(new CommandStop(Commands.Stop));
        }

        /// <summary>
        /// Informs gui about change in mission and requests its redraw.
        /// </summary>
        private void ValuesChanged()
        {
            Gui.GuiManager.Instance.RedrawMission(m_MissionName);
        }

        /// <summary>
        /// Removes drone from mission and if successfully removed, stops the mission.
        /// </summary>
        /// <param name="droneName">drone's name</param>
        public void RemoveDrone(string droneName)
        {
            if (m_AssignedDrones.Contains(droneName))
            {
                StopActions();
                m_AssignedDrones.Remove(droneName);
                ValuesChanged();
            }
        }

        /// <summary>
        /// Renames all waypoints in mission since one waypoint can be there multiple times.
        /// Stops the mission execution and requests redraw on gui.
        /// </summary>
        /// <param name="oldName">old name of waypoint</param>
        /// <param name="newName">new name of waypoint</param>
        public void RenameAllWaypoints(string oldName, string newName)
        {
            StopActions();
            foreach (KeyValuePair<int, CartesianWaypoint> rec in m_Waypoints)
            {
                if (rec.Value.Name == oldName)
                {
                    rec.Value.Name = newName;
                }
            }

            ValuesChanged();
        }

        /// <summary>
        /// Removes all waypoints with given names (even multiplicities).
        /// </summary>
        /// <param name="name">name of waypoins(s) to be removed</param>
        /// <returns>Returns true iff at least one waypoint removed.</returns>
        public bool RemoveAllWaypoints(string name)
        {
            StopActions();
            List<int> indexToRemove = new List<int>();
            foreach (KeyValuePair<int, CartesianWaypoint> rec in m_Waypoints)
            {
                if (rec.Value.Name == name)
                {
                    indexToRemove.Add(rec.Key);
                }
            }
            for (int i = 0; i < indexToRemove.Count; ++i)
            {
                m_Waypoints.Remove(indexToRemove[i]);
                m_WaypointsKeysOrder.Remove(indexToRemove[i]);
            }

            ValuesChanged();
            // If nothing removed, return false
            return indexToRemove.Count != 0;
        }

        /// <summary>
        /// Removes waypoint by its assigned index. Stops mission execution and requests redraw on gui.
        /// </summary>
        /// <param name="index">index unique to one waypoint</param>
        public void RemoveWaypoint(int index)
        {
            StopActions();

            m_Waypoints.Remove(index);
            m_WaypointsKeysOrder.Remove(index);

            ValuesChanged();
        }

        /// <summary>
        /// Removes ground target from being monitored. Stops mission execution and requests redraw on gui when removed.
        /// </summary>
        /// <param name="targetName">ground target to stop monitoring</param>
        public void RemoveTarget(string targetName)
        {
            Debug.Log("Removing target " + targetName);
            bool hasTarget = false;
            foreach (Command command in m_Commands)
            {
                // find all commands for tracking targets and remove this target
                if (command is CommandSetTrackingTargets)
                {
                    CommandSetTrackingTargets cmd = command as CommandSetTrackingTargets;
                    cmd.Targets.Remove(targetName);
                    hasTarget = true;
                }
            }
            if (hasTarget)
            {
                StopActions();
                ValuesChanged();
            }
        }

        /// <summary>
        /// Removes surveillance area. Stops mission execution and requests redraw on gui when removed.
        /// </summary>
        /// <param name="zoneName">name of zone to remove</param>
        /// <returns>Return true iff zone removed.</returns>
        public bool RemoveZone(string zoneName)
        {
            if (m_Zones.Contains(zoneName))
            {
                StopActions();
                m_Zones.Remove(zoneName);
                // Remove zone command since zone is no longer
                for (int i = 0; i < m_Commands.Count; ++i)
                {
                    if (m_Commands[i] is CommandSetSurveillanceArea)
                    {
                        if ((m_Commands[i] as CommandSetSurveillanceArea).AreaName == zoneName)
                        {
                            m_Commands.RemoveAt(i);
                            --i;
                        }
                    }
                }

                ValuesChanged();
                return true;
            }

            return false;
        }

        [Obsolete]
        public void SurveyRectangleArea(Vector3 start, Vector3 end)
        {
            StopActions();
            //m_Commands.Add(new CommandSetSurveillanceArea(Commands.SetSurveillanceArea, start, end));
            ValuesChanged();
        }

        /// <summary>
        /// Adds surveillance area to monitor. Specified by name. Stops mission execution and requests redraw on gui when added.
        /// </summary>
        /// <param name="areaName">name of area to add</param>
        public void SurveyRectangleArea(string areaName)
        {
            if (m_Zones.Contains(areaName)) 
                return;
            SurveillanceArea a = ZoneManager.Instance.GetSurveillanceArea(areaName);
            if (a != null)
                SurveyRectangleArea(a);
        }

        /// <summary>
        /// Adds surveillance area to monitor. Stops mission execution and requests redraw on gui when added.
        /// </summary>
        /// <param name="area"></param>
        public void SurveyRectangleArea(SurveillanceArea area)
        {
            if (m_Zones.Contains(area.name))
                return;

            StopActions();
            m_Zones.Add(area.name);
            Vector3 min = area.Min;
            Vector3 max = area.Max;
            m_Commands.Add(new CommandSetSurveillanceArea(Commands.CommandSetSurveillanceArea, min, max, area.name));
            ValuesChanged();
        }

        /// <summary>
        /// Adds surveillance area to monitor
        /// </summary>
        /// <param name="area"></param>
        public void SurveyRectangleAreaNoStop(string area)
        {
            SurveillanceArea a = ZoneManager.Instance.GetSurveillanceArea(area);
            if (a != null)
            {
                Vector3 min = a.Min;
                Vector3 max = a.Max;
                m_Commands.Add(new CommandSetSurveillanceArea(Commands.CommandSetSurveillanceArea, min, max, a.name));

            }
            ValuesChanged();
        }

        /// <summary>
        /// Generates mission name for given objectives. it can be very long.
        /// </summary>
        /// <param name="pickables">array of pickables, from which the name is generated</param>
        /// <returns>Returns possible name of mission (probably very long).</returns>
        public static string GetMissionName(params Gui.Pickable[] pickables)
        {
            if (pickables == null)
                return "";

            StringBuilder sbDrones = new StringBuilder();
            StringBuilder sbTargets = new StringBuilder();
            StringBuilder sbZones = new StringBuilder();
            StringBuilder sbWaypoints = new StringBuilder();

            foreach (Gui.Pickable p in pickables)
            {
                if (p is Drone)
                {
                    sbDrones.Append(p.name);
                    sbDrones.Append(",");
                }
                else if (p is GroundTarget)
                {
                    sbTargets.Append(p.name);
                    sbTargets.Append(",");
                }
                else if (p is Nav.SurveillanceArea)
                {
                    sbZones.Append(p.name);
                    sbZones.Append(",");
                }
                else if (p is Nav.TargetWaypoint)
                {
                    sbWaypoints.Append(p.name);
                    sbWaypoints.Append(",");
                }
            }

            if (sbTargets.Length > 0)
            {
                sbDrones.Append("Track-");
                sbDrones.Append(sbTargets);
            }

            if (sbZones.Length > 0)
            {
                sbDrones.Append("Survey-");
                sbDrones.Append(sbZones);
            }

            if (sbWaypoints.Length > 0)
            {
                sbDrones.Append("FlyThrough-");
                sbDrones.Append(sbWaypoints);
            }

            return sbDrones.ToString();
        }

        /// <summary>
        /// Returns command for tracking target.
        /// </summary>
        /// <returns>Returns command for tracking target if any.</returns>
        private CommandSetTrackingTargets GetTrackCmd()
        {
            int count = 0;
            CommandSetTrackingTargets ret = null;

            foreach (Command command in m_Commands)
            {
                // find all commands for tracking targets (can be only one!!!)
                if (command is CommandSetTrackingTargets)
                {
                    ++count;
                    ret = command as CommandSetTrackingTargets;
                }
            }
            if (count > 1)
                throw new Exception("More than one command for tracking targets in mission!");
            else if (count == 0)
            {
                // No tracking command, add it
                ret = new CommandSetTrackingTargets(Commands.CommandSetTrackingTargets);
                m_Commands.Add(ret);
            }

            return ret;
        }

        /// <summary>
        /// Creates or adds targets to CommandSetTrackingTargets. Stops mission execution and requests redraw on gui.
        /// </summary>
        /// <param name="targets"></param>
        public void TrackTargets(List<string> targets)
        {
            StopActions();
            // Find or create command for tracking targets
            CommandSetTrackingTargets cmd = GetTrackCmd();
            foreach (string target in targets)
            {
                GroundTarget groundTarget = AgentManager.Instance.GetTrackableTarget(target);
                if (groundTarget != null)
                {
                    // If such target exists, add it to command (only if unique)
                    if (!cmd.Targets.Contains(target))
                    {
                        Debug.Log("Adding " + target);
                        cmd.Targets.Add(target);
                    }
                }
            }

            ValuesChanged();
        }

        /// <summary>
        /// Stops mission from execution. It informs all drones which are currently
        /// doing this mission. If both Mission1 and Mission2 have drone Plane2 and
        /// Plane2 is doing mission Mission2 and we stop Mission1, Plane2 won't get
        /// stop command.
        /// </summary>
        public void Stop()
        {
            // Stop simulator
            Mission stopMission = new Mission("IDLE");
            foreach (string drone in m_AssignedDrones)
            {
                // Check if drone really on this mission
                if (AgentManager.Instance.IsDroneOnMission(drone, m_MissionName))
                    stopMission.AddDrone(drone);
            }
            //stopMission.AddStop();
            Network.SimulatorClient.Instance.SendMission(stopMission);
            m_IsPlaying = false;
        }

        /// <summary>
        /// Starts planning this mission for all assigned drones to this mission.
        /// Even if the drone is already on another mission.
        /// </summary>
        public void Start()
        {
            // Create all waypoint commands
            // foreach drone add new command
            // Remove all waypoint commands
            //for (int i = 0; i < m_Commands.Count; ++i)
            //{
            //    if (m_Commands[i] is CommandSetWaypoints)
            //    {
            //        m_Commands.RemoveAt(i);
            //        --i;
            //    }
            //}
            CommandSetTrackingTargets tcmd = GetTrackCmd();
            m_Commands.Clear();
            m_Commands.Add(tcmd);
            // For each drone add new command
            foreach (string droneName in m_AssignedDrones)
            {
                CommandSetWaypoints cmd = new CommandSetWaypoints(Commands.CommandSetWaypoints);
                foreach (int index in m_WaypointsKeysOrder)
                {
                    cmd.Waypoints.Add(m_Waypoints[index]);
                }
                //cmd.Waypoints = m_Waypoints.Values.ToList();
                cmd.UAVId = droneName;
                m_Commands.Add(cmd);
                // Assign drones on mission
                AgentManager.Instance.DroneOnMission(droneName, m_MissionName);
            }

            // Add zones tracking
            foreach (string zone in m_Zones)
            {
                SurveyRectangleAreaNoStop(zone);
            }

            // Assign/Reassing mission in simulator
            Network.SimulatorClient.Instance.SendMission(this);
            m_IsPlaying = true;
        }

        public string Name
        {
            get
            {
                return m_MissionName;
            }
        }

        public List<string> Drones
        {
            get
            {
                return m_AssignedDrones;
            }
        }

        public List<Command> Tasks
        {
            get
            {
                return m_Commands;
            }
        }

        public List<SurveillanceArea> SurveillanceAreas
        {
            get
            {
                List<SurveillanceArea> ret = new List<SurveillanceArea>();
                foreach (string name in m_Zones)
                {
                    SurveillanceArea area = ZoneManager.Instance.GetSurveillanceArea(name);
                    if (area != null)
                        ret.Add(area);
                    else
                        m_Zones.Remove(name);
                }

                return ret;
            }
        } 

        public List<string> Targets
        {
            get
            {
                List<string> ret = new List<string>();
                foreach (Command command in m_Commands)
                {
                    // find all commands for tracking targets
                    if (command is CommandSetTrackingTargets)
                    {
                        CommandSetTrackingTargets cmd = command as CommandSetTrackingTargets;
                        // add command targets to list
                        foreach (string s in cmd.Targets)
                        {
                            // Add only unique
                            if (!ret.Contains(s))
                                ret.Add(s);
                        }
                    }
                }

                return ret;
            }
        }

        public List<int> WaypointOrder
        {
            get
            {
                return m_WaypointsKeysOrder;
            }
        }

        public Dictionary<int, CartesianWaypoint> Waypoints
        {
            get
            {
                return m_Waypoints;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return m_IsPlaying;
            }

        }

    }
}
