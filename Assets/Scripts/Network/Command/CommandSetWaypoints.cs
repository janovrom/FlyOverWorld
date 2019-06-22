using Assets.Scripts.Nav;
using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Sets to uav some waypoints it should fly through.
    /// </summary>
    class CommandSetWaypoints : Command
    {

        public string UAVId;
        public List<CartesianWaypoint> Waypoints = new List<CartesianWaypoint>();

        public CommandSetWaypoints(Commands id) : base(id)
        {
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteInt(Waypoints.Count, output);
            foreach (CartesianWaypoint cp in Waypoints)
            {
                WriteCheckpoint(cp, output);
            }
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            int len = Convertor.ReadInt(input, next, out next);
            for (int i = 0; i < len; ++i)
            {
                Waypoints.Add(ReadCheckpoint(input, next, out next));
            }
        }

        private void WriteCheckpoint(CartesianWaypoint cp, List<byte> output)
        {
            WritePosition3d(cp.Position, output);
            Convertor.WriteDouble(0.0, output);
            Convertor.WriteDouble(0.0, output);
            Convertor.WriteDouble(0.0, output);
            Convertor.WriteDouble(cp.Velocity, output);
            Convertor.WriteLong(cp.Time, output);
            Convertor.WriteString(cp.Name, output);

            //Convertor.WriteString(cp.Name, output);
            //WritePosition3d(cp.Position, output);
            //Convertor.WriteLong(cp.Time, output);
            //Convertor.WriteLong(cp.TimeAllowance, output);
            //Convertor.WriteDouble(cp.Velocity, output);
        }

        private CartesianWaypoint ReadCheckpoint(byte[] input, int start, out int next)
        {
            next = start;
            UnityEngine.Vector3 pos = ReadPosition3d(input, next, out next);
            double d = Convertor.ReadDouble(input, next, out next);
            d = Convertor.ReadDouble(input, next, out next);
            d = Convertor.ReadDouble(input, next, out next);
            double velocity = Convertor.ReadDouble(input, next, out next);
            long time = Convertor.ReadLong(input, next, out next);
            string name = Convertor.ReadString(input, next, out next);

            return new CartesianWaypoint(
                name,
                pos
                );
        }
    }
}
