using UnityEngine;

namespace Assets.Scripts.Agents
{

    /// <summary>
    /// Represents ground target in scene. Its position is driven by simulation.
    /// Ground target is represented by purple sphere with outline. Its position
    /// is interpolated between two positions send by simulator, which means
    /// it will be slowed by one cycle of simulation.
    /// </summary>
    public class GroundTarget : Agent
    {
        private Vector3[] m_Positions = new Vector3[2];

        /// <summary>
        /// Current time from getting new waypoint. Used for interpolation.
        /// </summary>
        private float m_TimeMs = 0.0f;
        private bool m_SimulationStarted;
        private MeshRenderer m_Renderer;


        protected override Color GetColor()
        {
            return Utility.Constants.COLOR_PURPLE;
        }

        /// <summary>
        /// Adds model scaler to ground target.
        /// </summary>
        void Start()
        {
            m_Renderer = GetComponent<MeshRenderer>();
            gameObject.AddComponent<Models.ModelScaler>();
        }

        /// <summary>
        /// Initializes ground target given its positions.
        /// </summary>
        /// <param name="start">start position on initial segment</param>
        /// <param name="end">end position on initial segment</param>
        public void InitTarget(Vector3 start, Vector3 end)
        {
            m_Positions[0] = new Vector3(start.x, start.y, start.z);
            m_Positions[1] = new Vector3(end.x, end.y, end.z);
            m_TimeMs = 0.0f;

            m_SimulationStarted = true;
            //AddBillboard();
            AddLabel();
        }

        /// <summary>
        /// Updates end position on predicted path of this ground target.
        /// </summary>
        /// <param name="position">new end position</param>
        public void UpdateTarget(Vector3 position)
        {
            m_Positions[0] = new Vector3(m_Positions[1].x, m_Positions[1].y, m_Positions[1].z);
            m_Positions[1] = new Vector3(position.x, position.y, position.z);
            m_TimeMs = 0.0f;
        }

        /// <summary>
        /// Interpolates its position in time.
        /// </summary>
        void Update()
        {
            if (!m_SimulationStarted)
                return;

            m_TimeMs += Time.deltaTime * 1000.0f;
            m_TimeMs = Mathf.Clamp(m_TimeMs, 0.0f, (float)Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);
            Vector3 pos = GetPositionInTime();
            // Add altitude for position
            Ray ray = new Ray();
            ray.origin = new Vector3(pos.x, 10000.0f, pos.z);
            // Terrain should be below - for Earth at least
            ray.direction = Vector3.down;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 30000.0f, LayerMask.GetMask(Utility.Constants.LAYER_TERRAIN)))
            {
                pos.y += hit.point.y;
            }

            transform.position = pos;
        }

        public Vector3 GetPositionInTime()
        {
            return Vector3.Lerp(m_Positions[0], m_Positions[1], m_TimeMs / Network.SimulatorClient.UAV_TELEMETRY_REFRESH_RATE_MS);
        }

        /// <summary>
        /// Each string in returned array represents one current information
        /// about this ground target. Information consists of its position.
        /// </summary>
        /// <returns>Returns information about current state of this ground target.</returns>
        public override string[] Values()
        {
            return new string[] { "Position: " + transform.position };
        }

        public override bool IsDeletable()
        {
            return false;
        }

        /// <summary>
        /// When compass camera is selected, stop drawing this object.
        /// </summary>
        public override void CompassSelected()
        {
            m_Renderer.enabled = false;
        }

        /// <summary>
        /// When compass camera is deselected, start drawing this object.
        /// </summary>
        public override void CompassDeselected()
        {
            m_Renderer.enabled = true;
        }
    }
}
