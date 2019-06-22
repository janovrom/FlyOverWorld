using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Models;

namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Represents fly path for agents. Can be predicted which is end from
    /// simulator or past, which is generated after each update of values
    /// from simulator.
    /// </summary>
    public class FlyPath : MonoBehaviour
    {
        private const float Feet2M = 1.0f / 3.28084f;

        /// <summary>
        /// Positions to track current waypoint.
        /// </summary>
        private List<Vector3> m_Positions;
        private float m_InitAltitude = -1;
        private FlyPathPolygon m_FlyPathPolygon;
        private float m_DistanceToClosest = Mathf.Infinity;


        void Awake()
        {
            GameObject o = new GameObject("FlyPathPolygon");
            o.isStatic = true;
            m_FlyPathPolygon = o.AddComponent<FlyPathPolygon>();
            m_FlyPathPolygon.transform.parent = transform;
            m_FlyPathPolygon.InitializeDynamic();
            m_FlyPathPolygon.GetComponent<MeshRenderer>().material = Agents.AgentManager.Instance.FlyPathMaterial;
        }

        /// <summary>
        /// Updates fly path.
        /// </summary>
        /// <param name="endPos">new end position</param>
        /// <param name="drawLines">should lines ne drawn</param>
        /// <param name="color">color of fly path</param>
        /// <param name="dropVisibility">drop visibility from middle to end</param>
        /// <param name="type">type of fly path</param>
        public void UpdateFlyPath(Vector3 endPos, bool drawLines, Color color, bool dropVisibility = false, FlyPathPolygon.FlyPathType type = FlyPathPolygon.FlyPathType.RULER)
        {
            m_FlyPathPolygon.AddSegment(endPos, drawLines, color, dropVisibility, type);
        }

        public void Init(List<Vector3> positions)
        {
            m_InitAltitude = -1;
            m_Positions = positions.GetRange(1, positions.Count-1);
            m_DistanceToClosest = Mathf.Infinity;
            foreach (Vector3 v in positions)
            {
                Ray ray = new Ray();
                ray.origin = new Vector3(v.x, 10000.0f, v.z);
                // Terrain should be below - for Earth at least
                ray.direction = Vector3.down;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
                {
                }
                UpdateFlyPath(v + hit.point.y * Vector3.up, false, Utility.Constants.FLY_PATH_COLOR_FUTURE);
            }
        }

        /// <summary>
        /// Adds new position for predicted fly path.
        /// </summary>
        /// <param name="pos"></param>
        public void ChangePosition(Vector3 pos)
        {
            m_FlyPathPolygon.UpdateFirst(pos, FlyPathPolygon.FlyPathType.RULER);
        }

        /// <summary>
        /// Completes waypoint given current position. If we start to get further from
        /// waypoint, we already flew through it and we set next waypoint as our target.
        /// </summary>
        /// <param name="currentPosition">current position of drone</param>
        public void WaypointCompleted(Vector3 currentPosition)
        {
            // Check distance to waypoint
            if (m_Positions != null && m_Positions.Count > 0 && m_InitAltitude > 0.0f)
            {
                float dist = Vector3.Distance(m_Positions[0], currentPosition);
                if (m_DistanceToClosest < dist)
                {
                    // We started to getting further from this waypoint
                    m_FlyPathPolygon.RemoveFirstSegment(1);
                    m_Positions.RemoveAt(0);
                    m_DistanceToClosest = Mathf.Infinity;
                }
                else
                {
                    m_DistanceToClosest = dist;
                }
            }
        }

        /// <summary>
        /// Builds path from waypoints. Should never be used again. God (me) forbids it.
        /// Currently only returns bounding box with float min/max values.
        /// </summary>
        /// <param name="waypoints">waypoints on fly path</param>
        /// <returns>Returns rectangular boundign area around fly path.</returns>
        [System.Obsolete]
        public Rect BuildPath(List<Waypoint> waypoints)
        {
            // Bounding box around waypoints
            Rect bbox = new Rect();
            bbox.xMin = float.MaxValue;
            bbox.yMin = float.MaxValue;
            bbox.xMax = -float.MaxValue;
            bbox.yMax = -float.MaxValue;

            return bbox;
        }

    }
}
