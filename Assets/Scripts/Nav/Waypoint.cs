using UnityEngine;
using System.Collections;
using Assets.Scripts.Utility;

namespace Assets.Scripts.Nav
{
    /// <summary>
    /// Waypoint representing position on fly path. It serves only as container
    /// and serves to provide information from simulator to drones. It also
    /// switched XYAltitude to XAltitudeY.
    /// </summary>
    public class Waypoint
    {

        private Vector3 m_Position;
        private int m_TimeMs;
        private float m_GimbalPitchDeg;
        private float m_GimbalHeadingDeg;
        private float m_LatitudeDeg;
        private float m_LongitudeDeg;
        private float m_CompassHeadingDeg;
        private float m_SpeedMph;
        private float m_DistanceM;
        private int m_Autopilot;
        private float m_BatteryState;

        public Waypoint(Vector2 pos, float altitudeM, int timeMs, float gimbalPitchDeg, 
            float gimbalHeadingDeg, float latitude, float longitude,
            float compassHeadingDeg, float speedMph, float distanceM)
        {
            m_Position = new Vector3(pos.x, altitudeM, pos.y);
            m_TimeMs = timeMs;
            m_GimbalPitchDeg = gimbalPitchDeg;
            m_GimbalHeadingDeg = gimbalHeadingDeg;
            m_LatitudeDeg = latitude;
            m_LongitudeDeg = longitude;
            m_CompassHeadingDeg = compassHeadingDeg;
            m_SpeedMph = speedMph;
            m_DistanceM = distanceM;
        }

        public Waypoint(Vector3 pos, float compassHeadingDeg, float groundSpeedMs, int autopilotMode, float battery)
        {
            m_Position = pos;
            m_CompassHeadingDeg = compassHeadingDeg;
            m_SpeedMph = groundSpeedMs * Constants.METERS_PER_SEC_2_MILES_PER_HOUR;
            m_Autopilot = autopilotMode;
            m_BatteryState = battery;
        }

        public float SpeedMetersPerSec
        {
            get
            {
                return m_SpeedMph * Constants.MILES_PER_HOUR_2_METERS_PER_SEC;
            }

        }

        public float Battery
        {
            get
            {
                return m_BatteryState;
            }
        }

        public float SpeedMilesPerHour
        {
            get
            {
                return m_SpeedMph;
            }

            set
            {
                m_SpeedMph = value;
            }
        }

        public Vector3 GimbalRotation
        {
            get
            {
                return new Vector3(m_GimbalPitchDeg, m_GimbalHeadingDeg, 0);
            }
        }

        public float HeadingDeg
        {
            get
            {
                return m_CompassHeadingDeg;
            }

            set
            {
                m_CompassHeadingDeg = value;
            }
        }

        public float AltitudeM
        {
            get
            {
                return m_Position.y;
            }
        }

        public Vector2 Gps
        {
            get
            {
                return new Vector2(m_LatitudeDeg, m_LongitudeDeg);
            }
        }

        public int TimeMs
        {
            get
            {
                return m_TimeMs;
            }
        }

        public Vector3 Position
        {
            get
            {
                return m_Position;
            }

            set
            {
                m_Position = value;
            }
        }

    }
}
