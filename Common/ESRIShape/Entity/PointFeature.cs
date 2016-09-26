using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape.Entity
{
    public class PointFeature:Feature
    {
        public double Xmin { get { return X; } set { } }
        public double Ymin { get { return Y; } set { } }
        public double Xmax { get { return X; } set { } }
        public double Ymax { get { return Y; } set { } }
        public double Zmin { get; set; }
        public double Zmax { get; set; }
        public double Mmin { get; set; }
        public double Mmax { get; set; }
        public int ShapeType { get { return 1; } set { } }
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// 属性性,也字段列表中的顺序一一对应
        /// </summary>
        public List<string> ListPropValues { get; set; }

        public int GetContentLength()
        {
            return 10;
        }

    }

}
