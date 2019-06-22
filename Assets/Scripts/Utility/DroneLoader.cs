#pragma warning disable

using UnityEngine;
using System.Collections;
using System.IO;
using Assets.Scripts.Nav;
using Assets.Scripts.Agents;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Utility
{

    [Obsolete]
    /// <summary>
    /// Currently obsolete static class used for loading drone logs.
    /// </summary>
    public static class DroneLoader
    {

        private const float Feet2M = 0.3048f;

        static int latitude = 0;
        static int longitude = 1;
        static int altitudeFt = 2;
        static int ascentFt = 3;
        static int speedMph = 4;
        static int distanceFt = 5;
        static int timeMs = 6;
        static int datetimeUtc = 7;
        static int satellites = 8;
        static int voltageV = 9;
        static int max_altitudeFt = 10;
        static int max_ascentFt = 11;
        static int max_speedMph = 12;
        static int max_distanceFt = 13;
        static int compass_headingDeg = 14;
        static int isPhoto = 15;
        static int isVideo = 16;
        static int rc_elevator = 17;
        static int rc_aileron = 18;
        static int rc_throttle = 19;
        static int rc_rudder = 20;
        static int gimbal_headingDeg = 21;
        static int gimbal_pitchDeg = 22;
        static int battery_percent = 23;
        static int voltageCell1 = 24;
        static int voltageCell2 = 25;
        static int voltageCell3 = 26;
        static int voltageCell4 = 27;
        static int voltageCell5 = 28;
        static int voltageCell6 = 29;
        static int flycStateRaw = 30;
        static int flycState = 31;
        static int message = 32;

        // Rest of the parameters
        // datetime(utc)   satellites voltage(v)  max_altitude(feet)  max_ascent(feet)    max_speed(mph)  max_distance(feet)  compass_heading(degrees)    isPhoto isVideo rc_elevator rc_aileron  rc_throttle rc_rudder   gimbal_heading(degrees) gimbal_pitch(degrees)   battery_percent voltageCell1    voltageCell2 voltageCell3    voltageCell4 voltageCell5    voltageCell6 flycStateRaw    flycState message

        // Data from David's drone
        [Obsolete("Fly path created only using Simulator", true)]
        public static Rect LoadDrone(string path, World w)
        {
            if (!File.Exists(path))
            {
                Debug.Log("Drone log file " + path + " does not exist.");
                return new Rect();
            }

            StreamReader sr = new StreamReader(File.OpenRead(path));
            string line = sr.ReadLine();
            List<Waypoint> waypoints = new List<Waypoint>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                float lat = float.Parse(data[latitude]);
                float lon = float.Parse(data[longitude]);
                float altFt = float.Parse(data[altitudeFt]);
                float gPitch = float.Parse(data[gimbal_pitchDeg]);
                float gHeading = float.Parse(data[gimbal_headingDeg]);
                float cHeading = float.Parse(data[compass_headingDeg]);
                float speed = float.Parse(data[speedMph]);
                float distFt = float.Parse(data[distanceFt]);
                int tMs = int.Parse(data[timeMs]);
                Vector2 xz = w.GetPositionInWorld(new Vector2(lat, lon));

                Waypoint wp = new Waypoint(xz, altFt * Feet2M, tMs, gPitch, gHeading, lat, lon, cHeading, speed, distFt * Feet2M);
                waypoints.Add(wp);
            }

            GameObject flypath = new GameObject("Fly Path:"+path);
            flypath.transform.parent = w.transform;
            FlyPath fp = flypath.AddComponent<FlyPath>();
            Rect r = fp.BuildPath(waypoints);
            GameObject drone = new GameObject(path);
            drone.transform.parent = flypath.transform;
            drone.AddComponent<Drone>();
            drone.name = Path.GetFileName(path);
            return r;
        }

    }
}
