using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Provides information about drone assignment to mission and its status.
    /// </summary>
    class InfoMissionExecution : Command
    {

        public string UAVId;
        public string MissionId;
        public string MissionStatus;

        public InfoMissionExecution(Commands id) : base(id)
        {
        }


        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            MissionId = Convertor.ReadString(input, next, out next);
            MissionStatus = Convertor.ReadString(input, next, out next);
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteString(MissionId, output);
            Convertor.WriteString(MissionStatus, output);
            return output.ToArray();
        }

        override
        public string ToString()
        {
            return string.Format("Command " + COMMAND_ID + "::UAV={0};MISSION={1};STATUS={2}", UAVId, MissionId, MissionStatus);
        }
    }
}
