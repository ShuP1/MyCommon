using System;
using System.Linq;
using System.Text;

namespace MyCommon
{
    public static class Binary
    {
        public static bool TryToBool(ref byte[] bytes, out bool res)
        {
            if (bytes.Length < 1)
            {
                res = false;
                return false;
            }

            byte[] data = new byte[1];
            data = bytes.Take(1).ToArray();
            RemoveFirst(ref bytes, 1);
            if (data[1] == 1)
            {
                res = true;
            }
            else
            {
                if (data[1] == 0)
                {
                    res = false;
                }
                else
                {
                    res = false;
                    return false;
                }
            }
            return true;
        }

        ///<remarks>1 byte</remarks>
        public static byte[] FromBool(bool x)
        {
            return x ? new byte[1] { 1 } : new byte[1] { 0 };
        }

        public static bool TryToString(ref byte[] bytes, out string res)
        {
            res = null;
            int len;
            if (!TryToInt(ref bytes, out len))
                return false;

            if (bytes.Length < len)
                return false;

            res = Encoding.ASCII.GetString(bytes.Take(len).ToArray());
            RemoveFirst(ref bytes, len);
            return res != null;
        }

        ///<remarks>len(in bytes) + string</remarks>
        public static byte[] FromString(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return AddBytes(FromInt(data.Length), data);
        }

        public static bool TryToInt(ref byte[] bytes, out int res)
        {
            res = int.MinValue;

            if (bytes.Length < 4)
                return false;

            byte[] data = new byte[4];
            data = bytes.Take(4).ToArray();
            data.Reverse();
            res = BitConverter.ToInt32(data, 0);
            RemoveFirst(ref bytes, 4);
            return res != int.MinValue;
        }

        ///<remarks>4 bytes</remarks>
        public static byte[] FromInt(int x)
        {
            byte[] data = new byte[4];
            data = BitConverter.GetBytes(x);
            return data;
        }

        public static bool TryToStringArray(ref byte[] bytes, out string[] data)
        {
            data = null;

            int len;
            if (!TryToInt(ref bytes, out len))
                return false;

            if (len < 1 || len > 10000)
                return false;

            data = new string[len];
            for (int i = 0; i < len; i++)
            {
                if (!TryToString(ref bytes, out data[i]))
                    return false;
            }
            return data != null;
        }

        public static byte[] FromStringArray(string[] array)
        {
            if (array == null)
                return new byte[0];

            byte[] data = FromInt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                data = AddBytes(data, FromString(array[i]));
            }
            return data;
        }

        public static bool TryToIntArray(ref byte[] bytes, out int[] res)
        {
            res = null;
            int len;
            if (!TryToInt(ref bytes, out len))
                return false;

            res = new int[len];
            for (int i = 0; i < len; i++)
            {
                if (!TryToInt(ref bytes, out res[i]))
                    return false;
            }
            return res != null;
        }

        public static byte[] FromIntArray(int[] array)
        {
            byte[] data = FromInt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                data = AddBytes(data, FromInt(array[i]));
            }
            return data;
        }

        public static byte[] AddBytes(params byte[][] arguments)
        {
            byte[] res = new byte[arguments.Sum(a => a.Length)];
            int offset = 0;
            for (int i = 0; i < arguments.Length; i++)
            {
                Buffer.BlockCopy(arguments[i], 0, res, offset, arguments[i].Length);
                offset += arguments[i].Length;
            }
            return res;
        }

        public static void RemoveFirst(ref byte[] bytes, int count)
        {
            if (bytes.Length - count < 0)
            {
                bytes = new byte[0] { };
            }
            else
            {
                byte[] newbytes = new byte[bytes.Length - count];
                newbytes = bytes.Skip(count).ToArray();
                bytes = newbytes;
            }
        }
    }
}