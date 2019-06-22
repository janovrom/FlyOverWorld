using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Sends mission name, which is then created, and for each drone waypoints are
    /// assigned.
    /// </summary>
    class CommandSetWaypointsToGroup : Command
    {

        public string MissionName;
        public List<string> Drones = new List<string>();
        public List<UnityEngine.Vector3> Waypoints = new List<UnityEngine.Vector3>();


        public CommandSetWaypointsToGroup(Commands id) : base(id)
        {
        }

        public CommandSetWaypointsToGroup(Commands id, string name, List<string> drones, List<UnityEngine.Vector3> waypoints) : this(id)
        {
            MissionName = name;
            Drones.AddRange(drones);
            Waypoints.AddRange(waypoints);
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(MissionName, output);
            Convertor.WriteStringList(Drones, output);
            Convertor.WriteInt(Waypoints.Count, output);
            foreach (UnityEngine.Vector3 cp in Waypoints)
            {
                WritePosition3d(cp, output);
            }
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            MissionName = Convertor.ReadString(input, next, out next);
            List<string> assets = Convertor.ReadStringList(input, next, out next);
            if (assets != null)
            {
                Drones.AddRange(assets);
            }
            int len = Convertor.ReadInt(input, next, out next);
            for (int i = 0; i < len; ++i)
            {
                Waypoints.Add(ReadPosition3d(input, next, out next));
            }
        }

    }
}
