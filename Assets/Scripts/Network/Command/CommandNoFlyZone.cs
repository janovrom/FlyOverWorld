using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Command for creating or removing no-fly zone from simulator.
    /// If creating type, name, which uav it applies for, center, radius
    /// and heigth are provided. When removing only uavs and name is needed.
    /// </summary>
    class CommandNoFlyZone : Command
    {

        public byte Type = (byte)Nav.NoFlyZone.ZoneType.CYLINDER;
        public string UAVId;
        public string Name;
        public Vector3 Center;
        public float Radius;
        public float Height;


        public CommandNoFlyZone(Commands id) : base(id)
        {
        }

        public CommandNoFlyZone(Commands id, string uavId, string name) : base(id)
        {
            this.UAVId = uavId;
            this.Name = name;
        }

        public CommandNoFlyZone(Commands id, string uavId, string name, byte type, Vector3 center, float radius, float height) : this(id)
        {
            this.UAVId = uavId;
            this.Name = name;
            this.Type = type;
            this.Center = center;
            this.Radius = radius;
            this.Height = height;
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteString(Name, output);
            if (COMMAND_ID == Commands.CommandCreateNoFlyZone)
            {
                output.Add(Type);
                WritePosition3d(Center, output);
                Convertor.WriteDouble(Radius, output);
                Convertor.WriteDouble(Height, output);
            }
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            Name = Convertor.ReadString(input, next, out next);
            if (COMMAND_ID == Commands.CommandCreateNoFlyZone)
            {
                Type = input[next++];
                Center = ReadPosition3d(input, next, out next);
                Radius = (float)Convertor.ReadDouble(input, next, out next);
                Height = (float)Convertor.ReadDouble(input, next, out next);
            }
        }
    }
}
