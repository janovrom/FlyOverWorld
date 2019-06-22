using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Provides information - position and name - about ground target.
    /// </summary>
    public class InfoGroundTarget : Command
    {

        public string GroundTargetId;
        public UnityEngine.Vector3 Position;

        public InfoGroundTarget(Commands id) : base(id)
        {
        }


        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            GroundTargetId = Convertor.ReadString(input, next, out next);
            Position = ReadPosition3d(input, next, out next);
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(GroundTargetId, output);
            WritePosition3d(Position, output);
            return output.ToArray();
        }

        override
        public string ToString()
        {
            return string.Format("Command " + COMMAND_ID + "::ID={0};Location=({1},{2},{3})", GroundTargetId, Position.x, Position.y, Position.z);
        }

    }
}
