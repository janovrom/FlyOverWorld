using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Contains current state of UAV. Its name, position, autopilot mode,
    /// battery state, heading and ground speed.
    /// </summary>
    class InfoUAVTelemetry : Command
    {

        public string UAVId;
        public Vector3 Position;
        public int AutopilotMode;
        public double Battery;
        public double Heading;
        public double GroundSpeedMs;

        public InfoUAVTelemetry(Commands id) : base(id)
        {
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            Position = ReadPosition3d(input, next, out next);
            AutopilotMode = Convertor.ReadInt(input, next, out next);
            Battery = Convertor.ReadDouble(input, next, out next);
            Heading = Convertor.ReadDouble(input, next, out next) * Mathf.Rad2Deg + 90.0;
            GroundSpeedMs = Convertor.ReadDouble(input, next, out next);
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            WritePosition3d(Position, output);
            Convertor.WriteInt(AutopilotMode, output);
            Convertor.WriteDouble(Battery, output);
            Convertor.WriteDouble(-(Heading - 90.0f) * Mathf.Deg2Rad, output);
            Convertor.WriteDouble(GroundSpeedMs, output);
            return output.ToArray();
        }

        override
        public string ToString()
        {
            return string.Format("Command " + COMMAND_ID + "::ID={0};Location=({1},{2},{3});Autopilot={4};Battery={5:0.00};Heading={6:0.00};GroundSpeedMS={7:0.00}", 
                UAVId, Position.x, Position.y, Position.z, AutopilotMode, Battery, Heading, GroundSpeedMs);
        }

    }
}
