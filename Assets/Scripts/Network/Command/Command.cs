using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Abstract class providing interface for other commands - how to read and write bytes.
    /// Also has factory method, which given creates specific command given input and command id.
    /// </summary>
    public abstract class Command
    {

        public readonly Commands COMMAND_ID;
        public const string BROADCAST_ADDRESS = "*";
        public const string RECEIVER_ALL = "all";
        public const long TIME_ASAP = -1L;
        public const long TIME_UNDEFINED = -2L;

        protected Command(Commands id)
        {
            COMMAND_ID = id;
        }

        public static Command CreateCommand(Commands CommandId, byte[] input, int start, out int next)
        {
            Command command = null;
            next = start;

            switch (CommandId)
            {
                // 100+
                case Commands.CommandCreateNoFlyZone:
                    command = new CommandNoFlyZone(CommandId);
                    break;
                case Commands.CommandRemoveNoFlyZone:
                    command = new CommandNoFlyZone(CommandId);
                    break;
                case Commands.CommandWaypointCompleted:
                    command = new CommandWaypointCompleted(CommandId);
                    break;
                // 300+
                case Commands.CommandSetSurveillanceArea:
                    command = new CommandSetSurveillanceArea(CommandId);
                    break;
                case Commands.CommandSetTrackingTargets:
                    command = new CommandSetTrackingTargets(CommandId);
                    break;
                // 400+
                case Commands.InfoUAVAllocation:
                    command = new InfoUAVAllocation(CommandId);
                    break;
                case Commands.InfoMissionExecution:
                    command = new InfoMissionExecution(CommandId);
                    break;
                case Commands.InfoGroundTarget:
                    command = new InfoGroundTarget(CommandId);
                    break;
                case Commands.InfoUAVTelemetry:
                    command = new InfoUAVTelemetry(CommandId);
                    break;
                case Commands.InfoUAVTrajectory:
                    command = new InfoUAVTrajectory(CommandId);
                    break;
                default:
                    Debug.LogError("Command " + CommandId + " Not Implemented yet.");
                    break;
            }
            if (command != null)
                command.ReadInput(input, next, out next);

            return command;
        }

        /// <summary>
        /// Reads position from byte array. Y and Z axis are switched and Z is negated.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        protected Vector3 ReadPosition3d(byte[] input, int start, out int next)
        {
            next = start;
            // Switch Y,Z for position
            Vector3 pos = ChangeYZ(Convertor.ReadVector3d(input, next, out next));
            pos.z = -pos.z;
            return pos;
        }

        /// <summary>
        /// Negates Z axis, switches Y,Z axis and writes position to byte array.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="output"></param>
        protected void WritePosition3d(Vector3 pos, List<byte> output)
        {
            Vector3 tmp = pos;
            tmp.z = -tmp.z;
            Convertor.WriteVector3d(ChangeYZ(tmp), output);
        }

        /// <summary>
        /// Switched position of Y,Z axis.
        /// </summary>
        /// <param name="pos">position to switch</param>
        /// <returns>Returns new position with switched Y,Z.</returns>
        private Vector3 ChangeYZ(Vector3 pos)
        {
            float tmp = pos.y;
            pos.y = pos.z;
            pos.z = tmp;
            return pos;
        }

        protected abstract void ReadInput(byte[] input, int start, out int next);

        public abstract byte[] GetBytes();

    }
}
