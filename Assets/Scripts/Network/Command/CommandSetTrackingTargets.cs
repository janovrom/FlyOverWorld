using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Set to mission tracking some targets. It contains start and end time.
    /// </summary>
    class CommandSetTrackingTargets : Command
    {

        public List<string> Targets;
        public long StartTimeMin;
        public long EndTimeMin;


        public CommandSetTrackingTargets(Commands id) : base(id)
        {
            StartTimeMin = Command.TIME_ASAP;
            EndTimeMin = Command.TIME_UNDEFINED;
            Targets = new List<string>();
        }

        public CommandSetTrackingTargets(Commands id, List<string> targets) : base(id)
        {
            Targets = targets;
            StartTimeMin = Command.TIME_ASAP;
            EndTimeMin = Command.TIME_UNDEFINED;
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteStringList(Targets, output);
            Convertor.WriteLong(StartTimeMin, output);
            Convertor.WriteLong(EndTimeMin, output);
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            Targets = Convertor.ReadStringList(input, next, out next);
            StartTimeMin = Convertor.ReadLong(input, next, out next);
            EndTimeMin = Convertor.ReadLong(input, next, out next);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string target in Targets)
            {
                sb.Append(target);
                sb.Append(";");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);

            return "Tracking targets: " + sb.ToString();
        }

    }
}
