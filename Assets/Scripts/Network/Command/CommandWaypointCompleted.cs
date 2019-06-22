using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Informs or requests about completion of waypoint.
    /// </summary>
    class CommandWaypointCompleted : Command
    {

        public string UAVId;
        public string WaypointName;

        public CommandWaypointCompleted(Commands id) : base(id)
        {
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteString(WaypointName, output);
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            WaypointName = Convertor.ReadString(input, next, out next);
        }

        public override string ToString()
        {
            return string.Format("Drone {0} {1} through waypoint {2}.", UAVId, UAVId.Contains(Utility.Constants.ROVER) ? "rode" : "flew", WaypointName);
        }
    }
}
