namespace Assets.Scripts.Nav
{

    /// <summary>
    /// Waypoint which is passed to mission, created in WaypointsCreator 
    /// and send throught SimulatorClient to simulation. Defined by
    /// name, position, time in which it should be passed, time allownce
    /// for delay and required speed. It is only container and doesn't
    /// contain any methods.
    /// </summary>
    public class CartesianWaypoint
    {

        private string m_Name;
        /// <summary>
        /// Position relative to ground.
        /// </summary>
        private UnityEngine.Vector3 m_Position;
        private long m_Time;
        private long m_TimeAllowance;
        private double m_Velocity;

        public CartesianWaypoint(string name, UnityEngine.Vector3 position) : this(name, position, 4611686018427387903L, 9223372036854775807L, 0.0)
        {
        }

        public CartesianWaypoint(string name, UnityEngine.Vector3 position, long time, long timeAllowance, double velocity)
        {
            m_Name = name;
            m_Position = position;
            m_Time = time;
            m_TimeAllowance = timeAllowance;
            m_Velocity = velocity;
        }

        public override int GetHashCode()
        {
            return m_Name.GetHashCode();
        }

        public static bool operator ==(CartesianWaypoint obj1, CartesianWaypoint obj2)
        {
            if (object.ReferenceEquals(obj1, null) || object.ReferenceEquals(obj2, null))
                return false;

            return obj1.m_Name == obj2.m_Name;
        }

        public static bool operator !=(CartesianWaypoint obj1, CartesianWaypoint obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            var other = obj as CartesianWaypoint;
            if (other == null)
                return false;

            return this.m_Name == other.m_Name;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                m_Name = value;
            }
        }

        public UnityEngine.Vector3 Position
        {
            get
            {
                return new UnityEngine.Vector3(m_Position.x, m_Position.y, m_Position.z);
            }

            set
            {
                m_Position = value;
            }
        }

        public long Time
        {
            get
            {
                return m_Time;
            }
        }

        public long TimeAllowance
        {
            get
            {
                return m_TimeAllowance;
            }
        }

        public double Velocity
        {
            get
            {
                return m_Velocity;
            }
        }

    }
}
