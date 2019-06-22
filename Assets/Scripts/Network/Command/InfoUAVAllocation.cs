using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Contains information about allocation of uav. Contains allocated command
    /// which is either surveying area or tracking target.
    /// </summary>
    class InfoUAVAllocation : Command
    {

        public static short NO_COMMAND_ALLOCATED = -1;

        public string UAVId;
        /// <summary>
        /// Command which UAV should do. It can be CommandSetSurveillanceArea
        /// or CommandSetTrackingTargets
        /// </summary>
        public Command AllocatedCommand;


        public InfoUAVAllocation(Commands id) : base(id)
        {
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            Commands cmdID = (Commands) Convertor.ReadShort(input, next, out next);

            if (cmdID == Commands.CommandSetSurveillanceArea || cmdID == Commands.CommandSetTrackingTargets)
            {
                AllocatedCommand = Command.CreateCommand(cmdID, input, next, out next);
            }
            else if ((short)cmdID != NO_COMMAND_ALLOCATED)
            {
                UnityEngine.Debug.LogError("InfoUAVAllocation::Unrecognized command send=" + cmdID);
            }
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            if (AllocatedCommand == null)
                Convertor.WriteShort(NO_COMMAND_ALLOCATED, output);
            else
            {
                Convertor.WriteShort((short)AllocatedCommand.COMMAND_ID, output);
                output.AddRange(AllocatedCommand.GetBytes());
            }
            return output.ToArray();
        }

        public override string ToString()
        {
            return UAVId + " allocated to: " + (AllocatedCommand != null ? AllocatedCommand.ToString() : " no mission");
        }
    }
}
