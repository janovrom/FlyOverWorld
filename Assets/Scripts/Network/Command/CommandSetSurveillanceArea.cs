using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Sets surveillance area to mission. Requires name, start and
    /// end positions and start and end time.
    /// </summary>
    class CommandSetSurveillanceArea : Command
    {

        public string AreaName;
        public UnityEngine.Vector3 StartPosition;
        public UnityEngine.Vector3 EndPosition;
	    public long StartTimeMin;
        public long EndTimeMin;


        public CommandSetSurveillanceArea(Commands id) : base(id)
        {
        }

        public CommandSetSurveillanceArea(Commands id, UnityEngine.Vector3 startPosition, UnityEngine.Vector3 endPosition, string name) : base(id)
        {
            AreaName = name;
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartTimeMin = TIME_ASAP;
            EndTimeMin = TIME_UNDEFINED;
        }

        public override byte[] GetBytes()
        {
            List<byte> output = new List<byte>();
            WritePosition3d(StartPosition, output);
            WritePosition3d(EndPosition, output);
            Convertor.WriteLong(StartTimeMin, output);
            Convertor.WriteLong(EndTimeMin, output);
            return output.ToArray();
        }

        protected override void ReadInput(byte[] input, int start, out int next)
        {
            next = start;
            StartPosition = ReadPosition3d(input, next, out next);
            EndPosition = ReadPosition3d(input, next, out next);
            StartTimeMin = Convertor.ReadLong(input, next, out next);
            EndTimeMin = Convertor.ReadLong(input, next, out next);
        }

        public override string ToString()
        {
            return "Survey area is rectangle min=" + StartPosition + ", max=" + EndPosition;
        }
    }
}
