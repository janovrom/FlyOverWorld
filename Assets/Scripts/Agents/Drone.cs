using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Nav;
using Assets.Scripts.Utility;
using Assets.Scripts.Models;

namespace Assets.Scripts.Agents
{

    /// <summary>
    /// Represents flying agent. Its position is driven by simulation.
    /// Values are interpolated in time between start and end position.
    /// This means, there will always be some small delay considered reality,
    /// since interpolating on first value from simulator is still position (depends on sim speed).
    /// </summary>
    public class Drone : Agent
    {

        /// <summary>
        /// Start and end waypoint of current fly path segment.
        /// </summary>
        private Waypoint[] m_Waypoints = new Waypoint[2];

        /// <summary>
        /// Current time from getting new waypoint. Used for interpolation.
        /// </summary>
        private float m_TimeMs;
        // Drone flies in altitude relative to start position
        private Vector3 m_InitPosition;

        // Fly paths and its renderers, so they can be easily turned off when drone camera is selected.
        private FlyPath m_PredictedFlyPath;
        private FlyPath m_PastFlyPath;
        private MeshRenderer m_PastFlyPathMeshRenderer;
        private MeshRenderer m_PredictedFlyPathMeshRenderer;
        /// <summary>
        /// If drone was already initialized.
        /// </summary>
        private bool m_SimulationStarted = false;
        // Drawing line connecting drone and ground
        private DrawLines m_LineRenderer;
        private int m_LineId;


        protected override Color GetColor()
        { 
            return Constants.COLOR_GREEN;
        }

        /// <summary>
        /// Creates new predicted fly path from given positions.
        /// </summary>
        /// <param name="positions">positions on fly path</param>
        public void CreateFlyPath(List<Vector3> positions)
        {
            if (m_PredictedFlyPath != null)
                Destroy(m_PredictedFlyPath.gameObject);

            GameObject flypath = new GameObject("Predicted Fly Path:" + name);
            m_PredictedFlyPath = flypath.AddComponent<FlyPath>();
            m_PredictedFlyPath.Init(positions);
            m_PredictedFlyPathMeshRenderer = m_PredictedFlyPath.transform.GetChild(0).GetComponent<MeshRenderer>();
            m_PredictedFlyPathMeshRenderer.material.color = Constants.FLY_PATH_COLOR_FUTURE;
        }

        /// <summary>
        /// Initialize drone given its start and end waypoint positions, which
        /// are the same.
        /// </summary>
        /// <param name="start">start position on fly path</param>
        /// <param name="end">end position on fly path</param>
        public void InitDrone(Waypoint start, Waypoint end)
        {
            m_Waypoints[0] = start;
            m_Waypoints[1] = end;
            m_TimeMs = 0.0f;
            m_InitPosition = start.Position;
            m_SimulationStarted = true;
            // Add altitude for start
            Ray ray = new Ray();
            ray.origin = new Vector3(start.Position.x, 10000.0f, start.Position.z);
            // Terrain should be below - for Earth at least
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                start.Position = new Vector3(start.Position.x, hit.point.y + start.Position.y, start.Position.z);
            }
            // Add altitude for end
            ray.origin = new Vector3(end.Position.x, 10000.0f, end.Position.z);
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                end.Position = new Vector3(end.Position.x, hit.point.y + end.Position.y, end.Position.z);
            }

            // Don't use billboard
            //AddBillboard();
            AddLabel();
            m_LineRenderer = FindObjectOfType<DrawLines>();
            m_LineId = m_LineRenderer.RegisterLine();
            m_LineRenderer.SetLineColor(m_LineId, GetColor());
            m_LineRenderer.UpdateLine(m_LineId, new List<Vector3>() { m_InitPosition, new Vector3(m_InitPosition.x, 0, m_InitPosition.z) });
        }

        /// <summary>
        /// Update drone's position by adding new end waypoint.
        /// </summary>
        /// <param name="end">new waypoint on drone's path</param>
        public void UpdateDrone(Waypoint end)
        {
            m_Waypoints[0] = m_Waypoints[1];
            m_Waypoints[1] = end;
            m_TimeMs = 0.0f;
            // Add altitude
            Ray ray = new Ray();
            ray.origin = new Vector3(end.Position.x, 10000.0f, end.Position.z);
            // Terrain should be below - for Earth at least
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                end.Position = new Vector3(end.Position.x, hit.point.y + end.Position.y, end.Position.z);
            }
            // If didn't move at least by 150cm ignore it
            if (Vector3.Distance(m_Waypoints[0].Position, m_Waypoints[1].Position) > 1.5f)
            {
                if (m_PredictedFlyPath != null)
                    m_PredictedFlyPath.ChangePosition(m_Waypoints[1].Position);
                if (m_PastFlyPath != null)
                    m_PastFlyPath.UpdateFlyPath(m_Waypoints[1].Position, true, Constants.FLY_PATH_COLOR_PAST, true, Models.FlyPathPolygon.FlyPathType.POINTS);
            }
        }

        /// <summary>
        /// When drone camera is selected, its predicted and past fly paths are turned off.
        /// </summary>
        public override void CompassSelected()
        {
            m_Label.CompassSelected();
            if (m_PastFlyPath != null)
            {
                m_PastFlyPathMeshRenderer.enabled = false;
            }

            if (m_PredictedFlyPath != null)
            {
                m_PastFlyPathMeshRenderer.enabled = false;
            }
        }

        /// <summary>
        /// When drone camera is deselected, its predicted and past fly paths are turned on.
        /// </summary>
        public override void CompassDeselected()
        {
            if (m_PastFlyPathMeshRenderer != null)
                m_PastFlyPathMeshRenderer.enabled = true;
            if (m_PredictedFlyPathMeshRenderer != null)
                m_PredictedFlyPathMeshRenderer.enabled = true;
        }

        public void StartSimulation()
        {
            m_SimulationStarted = true;
        }

        /// <summary>
        /// Initializes past fly path and adds model scaler to drone.
        /// </summary>
        void Start()
        {
            GameObject flypath = new GameObject("Past Fly Path:" + name);
            m_PastFlyPath = flypath.AddComponent<FlyPath>();
            //gameObject.tag = Constants.TAG_UAV;
            gameObject.AddComponent<Models.ModelScaler>();
            m_PastFlyPathMeshRenderer = m_PastFlyPath.transform.GetChild(0).GetComponent<MeshRenderer>();
            m_PastFlyPathMeshRenderer.material.color = Constants.FLY_PATH_COLOR_PAST;
        }

        /// <summary>
        /// If drone was created, updates its position between start and end waypoint
        /// and updates its colored line between center of drone and its projection
        /// on ground.
        /// </summary>
        void Update()
        {
            if (!m_SimulationStarted)
                return;

            Vector3 pos = GetPositionInTime();
            transform.position = pos;
            transform.rotation = Quaternion.Euler(GetRotationInTime());

            m_TimeMs += Time.deltaTime * 1000.0f;
            m_TimeMs = Mathf.Clamp(m_TimeMs, 0.0f, (float)Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);

            if (m_PredictedFlyPath != null)
            {
                m_PredictedFlyPath.WaypointCompleted(transform.position);
            }

            m_LineRenderer.UpdateLine(m_LineId, new List<Vector3>() { pos, new Vector3(pos.x, 0, pos.z) });
        }

        #region Getters for values, which changes in time. These values are linearly interpolated.

        public Vector3 GetGimbalRotationInTime()
        {
            Vector3 rot1 = m_Waypoints[0].GimbalRotation;
            Vector3 rot2 = m_Waypoints[1].GimbalRotation;
            return Vector3.Lerp(rot1, rot2, m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);
        }

        public Vector3 GetPositionInTime()
        {
            Vector3 pos1 = m_Waypoints[0].Position;
            Vector3 pos2 = m_Waypoints[1].Position;
            return Vector3.Lerp(pos1, pos2, m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);
        }

        public Vector2 GetGpsInTime()
        {
            Vector2 pos1 = m_Waypoints[0].Gps;
            Vector2 pos2 = m_Waypoints[1].Gps;
            return Vector2.Lerp(pos1, pos2, m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);
        }

        public Vector3 GetRotationInTime()
        {
            float pos1 = m_Waypoints[0].HeadingDeg;
            float pos2 = m_Waypoints[1].HeadingDeg;

            if (pos1 < 0.0f) pos1 += 360.0f;
            if (pos2 < 0.0f) pos2 += 360.0f;

            float max = Mathf.Max(pos1, pos2);
            float min = Mathf.Min(pos1, pos2);
            float dist = max - min;
            //if (name == "Plane3")
            //    Debug.Log(name + "::" + pos1 + " " + pos2);
            if (dist > 180.0f)
            {
                if (pos1 > pos2)
                {
                    pos1 -= 360.0f;
                }
                else
                {
                    pos2 -= 360.0f;
                }
            }
            float t = m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS;
            float heading = (1 - t) * pos1 + t * pos2;
            return new Vector3(0.0f, heading, 0.0f);
        }

        public float GetSpeedInTimeMph()
        {
            float pos1 = m_Waypoints[0].SpeedMilesPerHour;
            float pos2 = m_Waypoints[1].SpeedMilesPerHour;
            float t = m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS;
            return (1 - t) * pos1 + t * pos2;
        }

        public float GetBatteryInTime()
        {
            float pos1 = m_Waypoints[0].Battery;
            float pos2 = m_Waypoints[1].Battery;
            float t = m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS;
            return (1 - t) * pos1 + t * pos2;
        }
        #endregion

        /// <summary>
        /// Each string in returned array represents one current information
        /// about this drone. Information consists of its position, altitude 
        /// AGL, heading, speed in meters per second and battery state.
        /// </summary>
        /// <returns>Returns information about current state of this drone.</returns>
        public override string[] Values()
        {
            Vector3 pos = GetPositionInTime();
            float altAGL = pos.y;
            Ray ray = new Ray();
            ray.origin = new Vector3(pos.x, 10000.0f, pos.z);
            // Terrain should be below - for Earth at least
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Constants.LAYER_TERRAIN)))
            {
                altAGL -= hit.point.y;
            }
            return new string[] { "Position: " + GetPositionInTime(),
                "Altitude (AGL): " + Mathf.RoundToInt(altAGL),
                "Heading: " + GetRotationInTime().y.ToString("0.0"),
                "Speed(mps): " + GetSpeedInTimeMph() * Constants.MILES_PER_HOUR_2_METERS_PER_SEC,
                "Battery state: " + GetBatteryInTime().ToString("0.0")
            };
        }

        public override bool IsDeletable()
        {
            return false;
        }

    }

}