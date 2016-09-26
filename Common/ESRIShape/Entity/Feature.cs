using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRIShape.Entity
{
    public struct Point
    {
        public double X;
        public double Y;
    }

    public struct Segment
    {
        public Point StartPt;
        public Point EndPt;
    }
    public interface  Feature
    {
        double Xmin { get; set; }
        double Ymin { get; set; }
        double Xmax { get; set; }
        double Ymax { get; set; }
        double Zmin { get; set; }
        double Zmax { get; set; }
        double Mmin { get; set; }
        double Mmax { get; set; }
        List<string> ListPropValues { get; set; }

        int  GetContentLength();
    }
}
