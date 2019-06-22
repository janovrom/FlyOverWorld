using System;
using System.Collections.Generic;

namespace Assets.Scripts.Network
{

    /// <summary>
    /// Contains static methods, which converts some value to 
    /// byte array, or reads this value from byte array.
    /// Bytes are stored in big endian. Anything that is not 
    /// a single value (string, lists) are read/written as length
    /// and then each part of this array/list.
    /// </summary>
    class Convertor
    {

        public static void WriteBool(bool value, List<byte> output)
        {
            output.Add(value ? (byte)1 : (byte)0);
        }

        public static void WriteLong(long value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteInt(int value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteChar(char value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteShort(short value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteString(string value, List<byte> output)
        {
            if (value == null)
            {
                WriteInt(-1, output);
            }
            else
            {
                WriteInt(value.Length, output);
                for (int i = 0; i < value.Length; ++i)
                    WriteChar(value[i], output);
                //output.AddRange(Encoding.ASCII.GetBytes(value));
            }
        }

        public static void WriteFloat(float value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteDouble(double value, List<byte> output)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            output.AddRange(bytes);
        }

        public static void WriteVector3f(UnityEngine.Vector3 value, List<byte> output)
        {
            WriteFloat(value.x, output);
            WriteFloat(value.y, output);
            WriteFloat(value.z, output);
        }

        public static void WriteVector3d(UnityEngine.Vector3 value, List<byte> output)
        {
            WriteDouble((double)value.x, output);
            WriteDouble((double)value.y, output);
            WriteDouble((double)value.z, output);
        }

        public static void WriteStringList(List<string> list, List<byte> output)
        {
            if (list == null)
            {
                WriteInt(-1, output);
            }
            else
            {
                WriteInt(list.Count, output);
                foreach (string s in list)
                    WriteString(s, output);
            }
        }

        public static void WriteCommandList(List<Command.Command> list, List<byte> output)
        {
            if (list == null)
            {
                WriteInt(-1, output);
            }
            else
            {
                WriteInt(list.Count, output);
                foreach (Command.Command cmd in list)
                    output.AddRange(cmd.GetBytes());
            }
        }

        public static List<Command.Command> ReadCommandList(byte[] input, int start, out int next, Command.Commands id)
        {
            List<Command.Command> list = null;
            int len = ReadInt(input, start, out next);
            if (len != -1)
            {
                list = new List<Command.Command>(len);
                for (int i = 0; i < len; ++i)
                {
                    list[i] = Command.Command.CreateCommand(id, input, next, out next);
                }
            }
            else
            {
                list = new List<Command.Command>();
            }
            return list;
        }

        public static List<string> ReadStringList(byte[] input, int start, out int next)
        {
            List<string> list = null;
            int len = ReadInt(input, start, out next);
            if (len != -1)
            {
                list = new List<string>(len);
                for (int i = 0; i < len; ++i)
                {
                    list.Add(ReadString(input, next, out next));
                }
            }
            return list;
        }

        public static int ReadInt(byte[] input, int start, out int next)
        {
            int val = 0;
            byte[] bytes = { input[start], input[start+1], input[start+2], input[start+3] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToInt32(bytes, 0);
            next = start + 4;
            return val;
        }

        public static long ReadLong(byte[] input, int start, out int next)
        {
            long val = 0;
            byte[] bytes = { input[start], input[start + 1], input[start + 2], input[start + 3],
                input[start + 4], input[start + 5], input[start + 6], input[start + 7] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToInt64(bytes, 0);
            next = start + 8;
            return val;
        }

        public static bool ReadBool(byte[] input, int start, out int next)
        {
            next = start + 1;
            return input[start] == 1;
        }

        public static short ReadShort(byte[] input, int start, out int next)
        {
            short val = 0;
            byte[] bytes = { input[start], input[start + 1] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToInt16(bytes, 0);
            next = start + 2;
            return val;
        }

        public static char ReadChar(byte[] input, int start, out int next)
        {
            char val;
            byte[] bytes = { input[start], input[start + 1] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToChar(bytes, 0);
            next = start + 2;
            return val;
        }

        public static string ReadString(byte[] input, int start, out int next)
        {
            int len = ReadInt(input, start, out next);
            if (len < 0)
            {
                return null;
            }
            else
            {
                if (len == 0)
                {
                    return "";
                }

                char[] buf = new char[len];
                for (int i = 0; i < len; i++)
                {
                    buf[i] = ReadChar(input, next, out next);
                }

                return new string(buf);
            }
        }

        public static float ReadFloat(byte[] input, int start, out int next)
        {
            float val = 0;
            byte[] bytes = { input[start], input[start + 1], input[start + 2], input[start + 3] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToSingle(bytes, 0);
            next = start + 4;
            return val;
        }

        public static double ReadDouble(byte[] input, int start, out int next)
        {
            double val = 0;
            byte[] bytes = { input[start], input[start + 1], input[start + 2], input[start + 3],
                input[start + 4], input[start + 5], input[start + 6], input[start + 7]};
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            val = BitConverter.ToDouble(bytes, 0);
            next = start + 8;
            return val;
        }

        public static UnityEngine.Vector3 ReadVector3f(byte[] input, int start, out int next)
        {
            UnityEngine.Vector3 vec3d = new UnityEngine.Vector3();
            vec3d.x = ReadFloat(input, start, out next);
            vec3d.y = ReadFloat(input, next, out next);
            vec3d.z = ReadFloat(input, next, out next);

            return vec3d;
        }

        public static UnityEngine.Vector3 ReadVector3d(byte[] input, int start, out int next)
        {
            UnityEngine.Vector3 vec3d = new UnityEngine.Vector3();
            vec3d.x = (float) ReadDouble(input, start, out next);
            vec3d.y = (float) ReadDouble(input, next, out next);
            vec3d.z = (float) ReadDouble(input, next, out next);

            return vec3d;
        }

    }
}
