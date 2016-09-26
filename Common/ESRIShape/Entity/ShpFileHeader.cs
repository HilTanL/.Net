using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape.Entity
{
    public struct ShpFileHeader
    {
        /// <summary>
        /// 1 文件代码
        /// </summary>
        public int FileCode;

        //2 未被使用

        /// <summary>
        /// 3 文件长度
        /// </summary>
        public int FileLength;

        /// <summary>
        /// 4 版本
        /// </summary>
        public int Version;

        /// <summary>
        /// 5 SHAPE类型
        /// </summary>
        public int ShapeType;

        /// <summary>
        /// 6 边界盒
        /// </summary>
        public double Xmin;

        /// <summary>
        /// 7 边界盒
        /// </summary>
        public double Ymin;

        /// <summary>
        /// 8 边界盒
        /// </summary>
        public double Xmax;

        /// <summary>
        /// 9 边界盒
        /// </summary>
        public double Ymax;

        /// <summary>
        /// 10 边界盒
        /// </summary>
        public double Zmin;

        /// <summary>
        /// 11 边界盒
        /// </summary>
        public double Zmax;

        /// <summary>
        /// 12 边界盒-最小 Measure 值
        /// </summary>
        public double Mmin;

        /// <summary>
        /// 13 边界盒-最大 Measure 值
        /// </summary>
        public double Mmax;
    }
}
