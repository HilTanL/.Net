using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace CoordTrans
{
    public class AppConfig
    {
        /// <summary>
        /// 坐标文本的分隔符号
        /// </summary>
        public static char[] SPLIT_CHAR = new char[] { ',', ' ', '\t' };

        /// <summary>
        /// 参数 UI 控件，key 表示类型，四参数或七参数
        /// </summary>
        public static Dictionary<string, List<Control>> ParsUI = new Dictionary<string, List<Control>>();

        ///// <summary>
        ///// 清空文本框
        ///// </summary>
        //public static void ClearTextBox(Control ctrl)
        //{
        //    foreach (Control c in ctrl.Controls)
        //    {
        //        if (c is TextBox) (c as TextBox).Text = "";
        //        else if (c is GroupBox || c is Janus.Windows.EditControls.UIGroupBox) ClearTextBox(c);
        //    }
        //}

        /// <summary>
        /// 检查是否为空
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns>true 表示存在空选项</returns>
        public static bool CheckBlank(Control ctrl)
        {
            foreach (Control c in ctrl.Controls)
            {
                if (c is TextBox && (c as TextBox).Text == "") return true;
                else if (c is GroupBox) CheckBlank(c);
            }
            return false;
        }

        /// <summary>
        /// 经纬度采用的角度格式
        /// </summary>
        public AngleFormat CurrentAngleFormat = AngleFormat.Angle;

        /// <summary>
        /// 记录文件格式信息
        /// </summary>
        public Dictionary<string, FileFormat> FileDictionary = new Dictionary<string, FileFormat>();

        /// <summary>
        /// 解析角度值，统一返回弧度值，如果格式不正确则抛出异常
        /// </summary>
        /// <param name="_angle"></param>
        /// <returns></returns>
        public double AngleParse(string _angle)
        {
            switch (this.CurrentAngleFormat)
            {
                case AngleFormat.Angle:
                    return double.Parse(_angle) / 180 * Math.PI;
                case AngleFormat.AngleSplit:
                    string[] s = _angle.Split(new char[] { ':' });
                    return (double.Parse(s[0]) + double.Parse(s[1]) / 60 + double.Parse(s[2]) / 3600) / 180 * Math.PI;
                case AngleFormat.Radian:
                    return double.Parse(_angle);
                default:
                    break;
            }

            throw new Exception("角度格式不正确！");
        }
    }

    /// <summary>
    /// 自定义文件格式类
    /// </summary>
    public class FileFormat
    {
        public string Name;
        public string Description;
        public string SplitChar = ",";
        public string Extension = ".txt";

        public FileFormat(string _name, string _des, string _format)
        {
            this.Name = _name;
            this.Description = _des;

            this.FormatInfo = _format;
        }

        public FileFormat(string _name, string _des, string _char, string _ex, string _format)
        {
            this.Name = _name;
            this.Description = _des;
            this.SplitChar = _char;
            this.Extension = _ex;

            this.FormatInfo = _format;
        }

        private string FormatInfo = "";
        public List<string> FormatList
        {
            get
            {
                return new List<string>(FormatInfo.Split(this.SplitChar.ToCharArray()));
            }
        }

        public int PointIndex
        {
            get
            {
                return FormatList.IndexOf("点号");
            }
        }

        public int LatIndex
        {
            get
            {
                return FormatList.IndexOf("经度");
            }
        }

        public int LonIndex
        {
            get
            {
                return FormatList.IndexOf("纬度");
            }
        }

    }

    /// <summary>
    /// 经纬度格式
    /// </summary>
    public enum AngleFormat
    {
        /// <summary>
        /// 以度为单位
        /// </summary>
        Angle = 0,
        /// <summary>
        /// 度:分:秒
        /// </summary>
        AngleSplit = 1,
        /// <summary>
        /// 弧度
        /// </summary>
        Radian = 2
    }
}


