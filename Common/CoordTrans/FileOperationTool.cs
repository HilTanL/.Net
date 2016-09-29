using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CoordTrans
{
    public class FileOperationTool
    {
        static CoordTransHelper.AngleConversion ac;
        /// <summary>
        /// 从文本文件获取点坐标序列
        /// </summary>
        /// <param name="fileName">坐标文本文件</param>
        /// <returns>点序列</returns>
        public static  List<PointCoord> PointsFromText()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                open.Multiselect = false;
                open.Filter = "文本文件(*.txt)|*.txt";

                if (open.ShowDialog() != System.Windows.Forms.DialogResult.OK) return null;


                List<string> lines = ReadLines(open.FileName);
                if (ac == null)
                {
                    ac = new CoordTransHelper.AngleConversion();
                }
                List<PointCoord> pointList = new List<PointCoord>();
                PointCoord point;
                string[] pointArray;
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = SpaceConvert(lines[i].Trim());
                    pointArray = lines[i].Split(new char[] { ',', ' ', ':', '，', '\t' });

                    point.ID = pointArray[0];
                    point.X = double.Parse(ac.ConvertToAngle(pointArray[1], CoordTransHelper.AngleType.SplitDegree,
                         CoordTransHelper.AngleType.Degree));
                    point.Y = double.Parse(ac.ConvertToAngle(pointArray[2], CoordTransHelper.AngleType.SplitDegree,
                         CoordTransHelper.AngleType.Degree));
                    pointList.Add(point);

                }
                return pointList;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 从文本文件获取点坐标序列
        /// </summary>
        /// <param name="fileName">坐标文本文件</param>
        /// <returns>点序列</returns>
        public static List<PointCoord> PointsFromText(string fileName)
        {
            try
            {

                List<string> lines = ReadLines(fileName);
                if (ac == null)
                {
                    ac = new CoordTransHelper.AngleConversion();
                }
                List<PointCoord> pointList = new List<PointCoord>();
                PointCoord point;
                string[] pointArray;
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = SpaceConvert(lines[i].Trim());
                    pointArray = lines[i].Split(new char[] { ',', ' ', ':', '，', '\t' });

                    point.ID = pointArray[0];
                    point.X = double.Parse(ac.ConvertToAngle(pointArray[1], CoordTransHelper.AngleType.SplitDegree,
                         CoordTransHelper.AngleType.Degree));
                    point.Y = double.Parse(ac.ConvertToAngle(pointArray[2], CoordTransHelper.AngleType.SplitDegree,
                         CoordTransHelper.AngleType.Degree));
                    pointList.Add(point);

                }
                return pointList;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        //将字符串里有多个不同的空格，转化成都是一个空格
        public static string SpaceConvert(string originStr)
        {
            string newStr = "";
            string[] splits = originStr.Split(' '); //以空格为标志
            for (int i = 0; i < splits.Length; i++)
            {
                if (splits[i].Trim().Equals(""))  //这里不是空格
                {
                    continue;
                }
                else
                {
                    newStr += splits[i] + " ";  //这里加一个空格
                }
            }
            return newStr;
        }

        public static  List<string> ReadLines(string txtFile)
        {
            List<string> lines = new List<string>();
            string line;
            Encoding encoding = EncodingType.GetType(txtFile);
            using (System.IO.StreamReader reader = new System.IO.StreamReader(txtFile, encoding))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
    }

    public class EncodingType
    {
        /// <summary>
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型
        /// </summary>
        /// <param name="FILE_NAME">文件路径</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }

        /// <summary>
        /// 通过给定的文件流，判断文件的编码类型
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }
        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;　 //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
    }
}
