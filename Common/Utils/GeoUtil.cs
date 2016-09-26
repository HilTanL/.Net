using CLog;
using System;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// 象限
    /// </summary>
    enum Quadrant
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,

        Else = 0
    }

    public struct Point
    {
        public double X;
        public double Y;
        public Point(double x,double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Line
    {
        public Point StartPt;
        public Point EndPt;
        public Line(Point startPt,Point endPt)
        {
            StartPt = startPt;
            EndPt = endPt;
        }
    }

    public class GeoUtil
    {

        // 1、找到当前管线点所有方向上的管沟节点，判断是直连还是三通
        // 2、直连：找到对应方向节点坐标，根据管沟宽度计算两侧节点，分左右两侧记录
        //    三通：找到所有方向节点坐标，计算附近节点坐标

        /// <summary>
        /// 计算起点位置的侧边点，格式{左侧点，右侧点}，部分点无法返回结果
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="distance">节点到起点的垂距</param>
        /// <returns>{左侧点，右侧点}</returns>
        public static List<Point> GetSidePoint1(Point startPoint, Point endPoint, double distance)
        {
            List<Point> sidePoints = new List<Point>();

            #region 仅能求出不分点
            // 限制条件：|x - x1| < distance，取x = x1 + distance/2
            // 取初始值
            double X = startPoint.X + distance / 2;
            double Y = Math.Sqrt(distance * distance - Math.Pow(X - startPoint.X, 2)) + startPoint.Y;

            // 采用迭代法计算
            double tempY = 0;
            while (Math.Abs(tempY - Y) > Math.Pow(0.1, 5))
            {
                tempY = Y;
                // 向量乘积为零
                X = (endPoint.Y - startPoint.Y) * (startPoint.Y - tempY) / (endPoint.X - startPoint.X) + startPoint.X;
                // 斜距计算公式
                Y = Math.Sqrt(distance * distance - Math.Pow(X - startPoint.X, 2)) + startPoint.Y;
            }
            #endregion

            double x2, y2;
            y2 = startPoint.Y - Math.Sqrt(distance * distance - Math.Pow(X - startPoint.X, 2));
            x2 = (endPoint.Y - startPoint.Y) * (startPoint.Y - y2) / (endPoint.X - startPoint.X) + startPoint.X;

            // 判断XY是否为左侧点
            Point temp1 = new Point((double)X, (double)Y);
            Point temp2 = new Point((double)x2, (double)y2);

            double lineDegree = GetVectorDegree(startPoint, endPoint);
            double tempDegree1 = GetVectorDegree(startPoint, temp1);

            double tempAdd = lineDegree + GetVectorInsectDegree(startPoint, endPoint, startPoint, temp1);
            tempAdd = (tempAdd > 360) ? tempAdd - 360 : tempAdd;
            if (tempAdd - tempDegree1 < 1.0)
            {
                sidePoints.Add(temp1);
                sidePoints.Add(temp2);
            }
            else
            {
                sidePoints.Add(temp2);
                sidePoints.Add(temp1);
            }

            return sidePoints;
        }

        /// <summary>
        /// 计算直线逆时针起点位置的侧边点
        /// 要求:1 侧边点与起点的连线垂直于直线
        ///      2 侧边点到起点的距离等于dis
        ///      3 侧边点位于直线的逆时针方向
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static Point GetSidePointCCW(Point startPt, Point endPt, double dis)
        {
            double dx = endPt.X - startPt.X;
            double dy = endPt.Y - startPt.Y;
            double dAngle = Math.Atan2(dy, dx);

            Point pt;
            pt.X = startPt.X - dis * (Math.Sin(dAngle));
            pt.Y = startPt.Y + dis * (Math.Cos(dAngle));

            return pt;
        }

        /// <summary>
        /// 计算直线顺时针起点位置的侧边点
        /// 要求:1 侧边点与起点的连线垂直于直线
        ///      2 侧边点到起点的距离等于dis
        ///      3 侧边点位于直线的逆时针方向
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static Point GetSidePointCW(Point startPt, Point endPt, double dis)
        {
            double dx = endPt.X - startPt.X;
            double dy = endPt.Y - startPt.Y;
            double dAngle = Math.Atan2(dy, dx);

            Point pt;
            pt.X = startPt.X + dis * (Math.Sin(dAngle));
            pt.Y = startPt.Y - dis * (Math.Cos(dAngle));

            return pt;
        }

        /// <summary>
        /// 计算直线顺时针中点位置的侧边点
        /// 要求:1 侧边点与直线中点的连线垂直于直线
        ///      2 侧边点到直线中点的距离等于dis
        ///      3 侧边点位于直线的顺时针方向
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static Point GetCSicePointCW(Point startPt, Point endPt, double dis)
        {
            double dx = endPt.X - startPt.X;
            double dy = endPt.Y - startPt.Y;
            double dAngle = Math.Atan2(dy, dx);

            Point pt;
            pt.X = (startPt.X + endPt.X) / 2.0 - dis * (Math.Sin(dAngle));
            pt.Y = (startPt.Y + endPt.Y) / 2.0 + dis * (Math.Cos(dAngle));

            return pt;
        }

        public static double GetDis(Point startPt,Point endPt)
        {
            double dx = endPt.X - startPt.X;
            double dy = endPt.Y - startPt.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算起点位置的侧边点，格式{左侧点，右侧点}
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="distance">节点到起点的垂距</param>
        /// <returns>{左侧点，右侧点}</returns>
        public static List<Point> GetSidePoint(Point startPoint, Point endPoint, double distance)
        {
            List<Point> sidePoints = new List<Point>();
            double X = 0; double Y = 0;

            #region 直接解算

            double lineDegree = GetVectorDegree(startPoint, endPoint);
            double leftDegree = lineDegree + 90;
            leftDegree = leftDegree > 360 ? leftDegree - 360 : leftDegree;

            double dx = distance * Math.Cos(leftDegree / 180.0 * Math.PI);
            double dy = distance * Math.Sin(leftDegree / 180.0 * Math.PI);
            X = startPoint.X + dx;
            Y = startPoint.Y + dy;

            #endregion

            double x2, y2;
            x2 = startPoint.X - dx;
            y2 = startPoint.Y - dy;

            sidePoints.Add(new Point((double)X, (double)Y));
            sidePoints.Add(new Point((double)x2, (double)y2));

            return sidePoints;
        }

        /// <summary>
        /// 计算平行偏移线,存在两条线
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="dis"></param>
        /// <returns>[直线1,直线2]</returns>
        //public static Line[] GetSideLine(Point startPt, Point endPt, double dis)
        //{
        //    Line[] lines = new Line[4];
        //    double lineDegree = GetVectorDegree(startPt, endPt);
        //    double leftDegree = lineDegree + 90;
        //    leftDegree = leftDegree > 360 ? leftDegree - 360 : leftDegree;

        //    double dx = dis * Math.Cos(leftDegree / 180.0 * Math.PI);
        //    double dy = dis * Math.Sin(leftDegree / 180.0 * Math.PI);

        //    //1 一侧线
        //    lines[0].StartPt=new Point(startPt.X + dx, startPt.Y + dy);
        //    lines[0].EndPt=new Point(endPt.X + dx, endPt.Y + dy);

        //    //2 另一侧线
        //    lines[1].StartPt=new Point(startPt.X - dx, startPt.Y - dy);
        //    lines[1].EndPt=new Point(endPt.X - dx, endPt.Y - dy);

        //    return lines;
        //}

        /// <summary>
        /// 返回起止点向量所在象限
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        static  Quadrant GetVectorZone(Point start, Point end)
        {
            double x = end.X - start.X;
            double y = end.Y - start.Y;

            if (x == 0 || y == 0)
                return Quadrant.Else;
            else if (x > 0)
                return (y > 0) ? Quadrant.One : Quadrant.Four;
            else
                return (y > 0) ? Quadrant.Two : Quadrant.Three;
        }

        /// <summary>
        /// 获取起止点向量的角度方向，返回0-360
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns>角度，以度为单位</returns>
        public static double GetVectorDegree(Point start, Point end)
        {
            double x = end.X - start.X;
            double y = end.Y - start.Y;

            if (x == 0 && y == 0)
            {
                Log.GetInstance().WriteError("起止点相同异常！");
            }
            if (x == 0)
                return (y > 0) ? 90 : 270;
            else if (y == 0)
                return (x > 0) ? 0 : 180;

            double r = Math.Atan(y / x);
            double d = r / Math.PI * 180;
            Quadrant zone = GetVectorZone(start, end);
            switch (zone)
            {
                case Quadrant.One:
                    break;
                case Quadrant.Two:
                    d = 180 + d;
                    break;
                case Quadrant.Three:
                    d = 180 + d;
                    break;
                case Quadrant.Four:
                    d = 360 + d;
                    break;
            }
            return d;
        }

        /// <summary>
        /// 获取向量的角度,返回0-2π
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <returns></returns>
        public static double GetVectorAngle(Point startPt,Point endPt)
        {
            double dx = endPt.X - startPt.X;
            double dy = endPt.Y - startPt.Y;

            double dAngle = Math.Atan2(dy, dx);
            if(dAngle>-Math.PI&&dAngle<0)
            {
                dAngle = 2.0 * Math.PI + dAngle;
            }
            return dAngle;            
        }

        /// <summary>
        /// 获取向量夹角（0-180）
        /// </summary>
        /// <param name="startPoint1"></param>
        /// <param name="endPoint1"></param>
        /// <param name="startPoint2"></param>
        /// <param name="endPoint2"></param>
        /// <returns></returns>
        public static double GetVectorInsectDegree(Point startPoint1, Point endPoint1, Point startPoint2, Point endPoint2)
        {
            double x1 = endPoint1.X - startPoint1.X;
            double y1 = endPoint1.Y - startPoint1.Y;

            double x2 = endPoint2.X - startPoint2.X;
            double y2 = endPoint2.Y - startPoint2.Y;
            //cosθ=(x1x2+y1y2)/[√(x1²+y1²)*√(x2²+y2²)]
            double cos = (x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2));

            return Math.Acos(cos) / Math.PI * 180;
        }

        /// <summary>
        /// 获取两向量的夹角0-π
        /// </summary>
        /// <param name="startPt1"></param>
        /// <param name="endPt1"></param>
        /// <param name="startPt2"></param>
        /// <param name="endPt2"></param>
        /// <returns></returns>
        //public static double GetVectorIncludeAngle(Point startPt1, Point endPt1, Point startPt2, Point endPt2)
        //{
        //    double x1 = endPt1.X - startPt1.X;
        //    double y1 = endPt1.Y - startPt1.Y;

        //    double x2 = endPt2.X - startPt2.X;
        //    double y2 = endPt2.Y - startPt2.Y;
        //    //cosθ=(x1x2+y1y2)/[√(x1²+y1²)*√(x2²+y2²)]
        //    double cos = (x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2));

        //    return Math.Acos(cos);
        //}

        /// <summary>
        /// 计算两条直线的交点
        /// </summary>
        /// <param name="lineFirstStar">L1的点1坐标</param>
        /// <param name="lineFirstEnd">L1的点2坐标</param>
        /// <param name="lineSecondStar">L2的点1坐标</param>
        /// <param name="lineSecondEnd">L2的点2坐标</param>
        /// <returns></returns>
        public static  Point GetLineInsectPt(Point lineFirstStar, Point lineFirstEnd, Point lineSecondStar, Point lineSecondEnd)
        {
            /*
             * L1，L2都存在斜率的情况：
             * 直线方程L1: ( y - y1 ) / ( y2 - y1 ) = ( x - x1 ) / ( x2 - x1 ) 
             * => y = [ ( y2 - y1 ) / ( x2 - x1 ) ]( x - x1 ) + y1
             * 令 a = ( y2 - y1 ) / ( x2 - x1 )
             * 有 y = a * x - a * x1 + y1   .........1
             * 直线方程L2: ( y - y3 ) / ( y4 - y3 ) = ( x - x3 ) / ( x4 - x3 )
             * 令 b = ( y4 - y3 ) / ( x4 - x3 )
             * 有 y = b * x - b * x3 + y3 ..........2
             * 
             * 如果 a = b，则两直线平等，否则， 联解方程 1,2，得:
             * x = ( a * x1 - b * x3 - y1 + y3 ) / ( a - b )
             * y = a * x - a * x1 + y1
             * 
             * L1存在斜率, L2平行Y轴的情况：
             * x = x3
             * y = a * x3 - a * x1 + y1
             * 
             * L1 平行Y轴，L2存在斜率的情况：
             * x = x1
             * y = b * x - b * x3 + y3
             * 
             * L1与L2都平行Y轴的情况：
             * 如果 x1 = x3，那么L1与L2重合，否则平等
             * 
            */
            double a = 0, b = 0;
            int state = 0;
            if (lineFirstStar.X != lineFirstEnd.X)
            {
                a = (lineFirstEnd.Y - lineFirstStar.Y) / (lineFirstEnd.X - lineFirstStar.X);
                state |= 1;
            }
            if (lineSecondStar.X != lineSecondEnd.X)
            {
                b = (lineSecondEnd.Y - lineSecondStar.Y) / (lineSecondEnd.X - lineSecondStar.X);
                state |= 2;
            }
            switch (state)
            {
                case 0: //L1与L2都平行Y轴
                    {
                        if (lineFirstStar.X == lineSecondStar.X)
                        {
                            //throw new Exception("两条直线互相重合，且平行于Y轴，无法计算交点。");
                            //首尾相连的情况
                            if (lineFirstStar.Equals(lineSecondStar) || lineFirstStar.Equals(lineSecondEnd)) return lineFirstStar;
                            if (lineFirstEnd.Equals(lineSecondStar) || lineFirstEnd.Equals(lineSecondEnd)) return lineFirstEnd;
                            return new Point(0, 0);
                        }
                        else
                        {
                            //throw new Exception("两条直线互相平行，且平行于Y轴，无法计算交点。");
                            //首尾相连的情况
                            if (lineFirstStar.Equals(lineSecondStar) || lineFirstStar.Equals(lineSecondEnd)) return lineFirstStar;
                            if (lineFirstEnd.Equals(lineSecondStar) || lineFirstEnd.Equals(lineSecondEnd)) return lineFirstEnd;
                            return new Point(0, 0);
                        }
                    }
                case 1: //L1存在斜率, L2平行Y轴
                    {
                        double x = lineSecondStar.X;
                        double y = (lineFirstStar.X - x) * (-a) + lineFirstStar.Y;
                        return new Point(x, y);
                    }
                case 2: //L1 平行Y轴，L2存在斜率
                    {
                        double x = lineFirstStar.X;
                        //网上有相似代码的，这一处是错误的。你可以对比case 1 的逻辑 进行分析
                        //源code:lineSecondStar * x + lineSecondStar * lineSecondStar.X + p3.Y;
                        double y = (lineSecondStar.X - x) * (-b) + lineSecondStar.Y;
                        return new Point(x, y);
                    }
                case 3: //L1，L2都存在斜率
                    {
                        if (a == b)
                        {
                            // throw new Exception("两条直线平行或重合，无法计算交点。");
                            //首尾相连的情况
                            if (lineFirstStar.Equals(lineSecondStar) || lineFirstStar.Equals(lineSecondEnd)) return lineFirstStar;
                            if (lineFirstEnd.Equals(lineSecondStar) || lineFirstEnd.Equals(lineSecondEnd)) return lineFirstEnd;
                            return new Point(0, 0);
                        }
                        double x = (a * lineFirstStar.X - b * lineSecondStar.X - lineFirstStar.Y + lineSecondStar.Y) / (a - b);
                        double y = a * x - a * lineFirstStar.X + lineFirstStar.Y;
                        return new Point(x, y);
                    }
            }
            //throw new Exception("不可能发生的情况");
            return new Point(0, 0);
        }
        
        public static Point GetLineInsectPt(Line line1,Line line2)
        {
            return GetLineInsectPt(line1.StartPt, line1.EndPt, line2.StartPt, line2.EndPt);
        }

    }
}
