using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Assigns uav to group.
    /// </summary>
    class CommandUAVGroup : Command
    {

	    public string UAVId;
        public string GroupId;

        public CommandUAVGroup(Commands id) : base(id)
        {
        }

        public CommandUAVGroup(Commands id, string uavId, string groupId) : base(id)
        {
            UAVId = uavId;
            GroupId = groupId;
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteString(GroupId, output);
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            GroupId = Convertor.ReadString(input, next, out next);
        }
    }
}
