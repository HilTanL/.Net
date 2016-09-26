using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ESRIShape
{
    internal static class Helper
    {
        public static int BigAndLittleSwitch(UInt32 val)
        {
            UInt32 result = (((val & 0xff000000) >> 24) |
                   ((val & 0xff000000) >> 24) |
                   ((val & 0x00ff0000) >> 8) |
                   ((val & 0x0000ff00) << 8) |
                   ((val & 0x000000ff) << 24));
            return (int)result;
        }

        #region byte[]转为特定类型的值
        

        public static Int16 BytesToInt16(byte[] value, int offset = 0)
        {
            return System.BitConverter.ToInt16(value, offset);
        }

        public static Int32 BytesToInt32(byte[] value, int offset = 0)
        {
            return System.BitConverter.ToInt32(value, offset);
        }

        public static char BytesAToChar(byte[] value, int offset = 0)
        {
            return Encoding.ASCII.GetChars(value, 0, 1)[0];
        }

        public static char BytesToChar(byte[] value, int offset = 0)
        {
            return System.BitConverter.ToChar(value, 0);
        }

        public static string BytesAToString(byte[] value, int offset = 0, int length = -1)
        {
            if (length == -1) length = value.Length;
            char[] chars = Encoding.ASCII.GetChars(value, 0, length);
            string result = new string(chars);
            return result.Replace("\0", "");
        }

        public static string BytesToString(byte[] value, int offset = 0, int length = -1)
        {
            if (length == -1) length = value.Length;
            return System.BitConverter.ToString(value, offset, length);
        }

        public static UInt32 BytesToUInt32(byte[] value, int offset = 0)
        {
            return System.BitConverter.ToUInt32(value, offset);
        }

        public static double BytesToDouble(byte[] value, int offset = 0)
        {
            return System.BitConverter.ToDouble(value, offset);
        }

        public static string BytesToValue(byte[] value, int offset, int length)
        {
             return Encoding.Default.GetString(value, 0, length);
        }
        
        #endregion

        #region 特定类型的值转为byte[]
        public static byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
        }

        public static byte[] UIntToBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ShortToBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] DoubleToBytes(double d)
        {
            return BitConverter.GetBytes(d);
        }

        public static byte[] CharToBytes (char c)
        {
            return System.BitConverter.GetBytes(c);
        }

        public static byte[] StringToBytes(string s)
        {
            if (s == null) return new byte[0];
            else return Encoding.Default.GetBytes(s);
        }

        #endregion

    }
}
