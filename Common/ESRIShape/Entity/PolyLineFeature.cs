using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape.Entity
{
    public class PolyLineFeature:Feature
    {
        #region 属性信息
        public double Xmin { get { return Box[0]; } set { } }
        public double Ymin { get { return Box[1]; } set { } }
        public double Xmax { get { return Box[2]; } set { } }
        public double Ymax { get { return Box[3]; } set { } }
        public double Zmin { get; set; }
        public double Zmax { get; set; }
        public double Mmin { get; set; }
        public double Mmax { get; set; }

        /// <summary>
        /// 1-几何类型
        /// </summary>
        public int ShapeType { get { return 3; } set { } }

        /// <summary>
        /// 2-坐标范围-大小为4
        /// </summary>
        public double[] Box { get; set; }

        /// <summary>
        /// 3-子线段个数
        /// </summary>
        public int NumParts { get; set; }

        /// <summary>
        /// 4-线所包含的点数
        /// </summary>
        public int NumPoints { get; set; }

        /// <summary>
        /// 5-表示在Points中每个子线段的起点坐标位置-大小为NumPoints
        /// </summary>
        public List<int> Parts { get; set; }

        /// <summary>
        /// 6-记录了所有坐标信息
        /// </summary>
        public List<Point> Points { get; set; }

        public List<string> ListPropValues { get; set; } 
        #endregion

        public PolyLineFeature()
        {
            this.Box = new double[4];
            this.NumParts = 0;
            this.NumPoints = 0;
            this.Parts=new List<int>();
            this.Points=new List<Point>();
        }

        public void AddLine(Segment seg)
        {
            UpdateBox(seg);

            this.NumParts += 1;
            this.NumPoints += 2;
            this.Parts.Add(this.Points.Count);
            this.Points.Add(seg.StartPt);
            this.Points.Add(seg.EndPt);            
        }

        public int GetContentLength()
        {
            return (4 + 32 + 4 + 4 + 4 * NumParts + 2 * NumPoints * 8) / 2;
        }

        private void UpdateBox(Segment seg)
        {
            Point startPt=seg.StartPt;
            Point endPt=seg.EndPt;
            double minX = startPt.X < endPt.X ? startPt.X : endPt.X;
            double minY = startPt.Y < endPt.Y ? startPt.Y : endPt.Y;
            double maxX = startPt.X > endPt.X ? startPt.X : endPt.X;
            double maxY = startPt.Y > endPt.Y ? startPt.Y : endPt.Y;
            if (NumParts == 0)
            {
                Box[0] = minX;
                Box[1] = minY;
                Box[2] = maxX;
                Box[3] = maxY;
            }
            else
            {
                Box[0] = Box[0] < minX ? Box[0] : minX;
                Box[1] = Box[1] < minY ? Box[1] : minY;
                Box[2] = Box[2] > maxX ? Box[2] : maxX;
                Box[3] = Box[3] > maxY ? Box[3] : maxY;
            }
        }



    }

}
