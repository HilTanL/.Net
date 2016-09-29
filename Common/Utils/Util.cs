using System;
using System.Diagnostics;

namespace Utils
{
    public class Util
    {
        /// <summary>
        /// 清除字符串的空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ClearSpace(string str)
        {
            if (str == null) return "";

            string result = "";
            foreach (char c in str)
            {
                if (c == ' ')
                { }
                else
                {
                    result += c;
                }
            }
            return result;
        }

        /// <summary>
        /// 给定excel中的数字文本日期,获取指定格式的日期字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetDate(string str)
        {
            string result = str;

            try
            {
                double lDate = double.Parse(str);
                DateTime date = DateTime.FromOADate(lDate);
                result = string.Format("{0}年{1}月{2}日", date.Year, date.Month, date.Day);
            }
            catch (Exception)
            {

            }

            return result;
        }

        /// <summary>
        /// 通过空格扩展字符串，字体串的长度至少为length
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length">英文占一个字符，中文占两个字符</param>
        /// <param name="isMiddle">内容是否位于中间，默认为true。false表示将空格填入右侧</param>
        /// <returns></returns>
        public static string ExpandStr(string str, int length, bool isMiddle = true)
        {
            string result = str.Trim();
            int len = StrLen(str);
            if (len >= length) return result;
            else if (isMiddle == true)
            {
                int num = (int)Math.Floor((length - len) / 2.0);
                result = new String(' ', num) + str + new String(' ', length - num - len);
            }
            else
            {
                int num = length - len;
                result = str + new String(' ', num);
            }
            return result;
        }

        /// <summary>
        /// 计算字符串长度，要求中文占两个字符，英文占一个字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int StrLen(string str)
        {
            int nLength = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= 0x3000 && str[i] <= 0x9FFF)
                    nLength += 2;
                else
                    nLength++;
            }
            return nLength;
        }

        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double GetDis(double x1,double y1,double x2,double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 获取当前程序所在的路径
        /// </summary>
        /// <param name="up">上级目录的层数</param>
        /// <returns></returns>
        public static string GetAppPath(int up=0)
        {
            String result = "";
            try
            {
                result =System.IO.Directory.GetCurrentDirectory();
                for (int i = 0; i < up; ++i)
                {
                    int lastIndex = result.LastIndexOf("\\");
                    result = result.Substring(0, lastIndex);
                }
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// 根据X Y生成图幅号
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public static string CreateMapCode(double X,double Y)
        {
            //深圳1:1000图幅号计算方式：(Y-80000)/500为前三位，（X-8000）/500为后两位。直接截取，请勿四舍五入。
            string strY = ((Y - 80000.0) / 500.0).ToString().Split('.')[0];
            string strX = ((X - 8000.0) / 500.0).ToString().Split('.')[0];
            while (strY.Length < 3)
            {
                strY = "0" + strY;
            }
            while (strX.Length < 2)
            {
                strX = "0" + strX;
            }
            strY = strY.Substring(0, 3);
            strX = strX.Substring(strX.Length - 2, 2);
            return strY + strX;
        }

        public static void StopProcessByName(string name)
        {
            foreach (Process p in System.Diagnostics.Process.GetProcessesByName(name))
            {
                try
                {
                    p.Kill();
                    p.WaitForExit();
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    System.Diagnostics.EventLog.WriteEntry("AlchemySearch:KillProcess", exp.Message, System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="o"></param>
        public static void Release(object o)
        {
            try
            {
                while (System.Runtime.InteropServices.Marshal.ReleaseComObject(o) > 0) ;
            }
            catch { }
            finally
            {
                o = null;
            }
        }

    }
}
