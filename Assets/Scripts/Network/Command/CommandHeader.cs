using System;
using System.Collections.Generic;

namespace Assets.Scripts.Network.Command
{

    /// <summary>
    /// Represents header associated with each command. It defines data length, sequence 
    /// number, which command is send and version. It is always 12 bytes.
    /// <b>Bytes listing:</b>
    /// Length - bytes 0..3
    /// Sequence Number - bytes 4..7
    /// Command Id - bytes 8..9
    /// Version - bytes 10..11
    /// </summary>
    class CommandHeader
    {

        public int Length;
        public int SequenceNumber;
        public Commands CommandId;
        public short Version;

        public CommandHeader(byte[] input)
        {
            if (input.Length != 12)
            {
                throw new Exception("Wrong byte stream for CommandHeader!");
            }

            int next;
            // Length - bytes 0..3
            Length = Convertor.ReadInt(input, 0, out next);
            // Sequence Number - bytes 4..7
            SequenceNumber = Convertor.ReadInt(input, next, out next);
            // Command Id - bytes 8..9
            CommandId = (Commands) Convertor.ReadShort(input, next, out next);
            // Version - bytes 10..11
            Version = Convertor.ReadShort(input, next, out next);
        }

        public CommandHeader(int length, int seqNum, Commands commandId, short version)
        {
            this.Length = length;
            this.SequenceNumber = seqNum;
            this.CommandId = commandId;
            this.Version = version;
        }

        public List<byte> GetBytes()
        {
            List<byte> output = new List<byte>();
            Convertor.WriteInt(Length, output);
            Convertor.WriteInt(SequenceNumber, output);
            Convertor.WriteShort((short)CommandId, output);
            Convertor.WriteShort(Version, output);

            return output;
        }

        override
        public string ToString()
        {
            return string.Format("Length={0};SequenceNumber={1};CommandId={2}({3});Verions={4}", Length, SequenceNumber, CommandId, (short) CommandId, Version);
        }

    }
}
