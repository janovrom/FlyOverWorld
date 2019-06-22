using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Contains planned trajectory for uav. Contains points on trajectory and
    /// uav name.
    /// </summary>
    class InfoUAVTrajectory : Command
    {

        public string UAVId;
	    public List<UnityEngine.Vector3> Trajectory = new List<UnityEngine.Vector3>();


        public InfoUAVTrajectory(Commands id) : base(id)
        {
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteString(UAVId, output);
            Convertor.WriteInt(Trajectory.Count, output);
            foreach (UnityEngine.Vector3 pos in Trajectory)
                WritePosition3d(pos, output);
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            UAVId = Convertor.ReadString(input, next, out next);
            int len = Convertor.ReadInt(input, next, out next);
            for (int i = 0; i < len; ++i)
            {
                Trajectory.Add(ReadPosition3d(input, next, out next));
            }
        }

    }
}
