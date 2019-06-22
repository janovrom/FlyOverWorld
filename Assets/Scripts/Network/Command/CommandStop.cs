using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// When assigned to mission, its execution is stopped.
    /// </summary>
    class CommandStop : Command
    {

        public short StopCommandId;
        public long StartTime = 0L;
        public bool Relative = true;


        public CommandStop(Commands id) : base(id)
        {
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            StopCommandId = Convertor.ReadShort(input, next, out next);
            StartTime = Convertor.ReadLong(input, next, out next);
            Relative = Convertor.ReadBool(input, next, out next);
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteShort(StopCommandId, output);
            Convertor.WriteLong(StartTime, output);
            Convertor.WriteBool(Relative, output);
            return output.ToArray();
        }
    }
}
