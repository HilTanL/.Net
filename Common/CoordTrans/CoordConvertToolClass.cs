using System;
using System.Collections.Generic;
using System.Text;
using CoordTranBase;

namespace CoordTrans
{
    public struct PointCoord
    {
        public string ID;
        public double X;
        public double Y;
    }

    /// <summary>
    /// 坐标格式
    /// </summary>
    public enum CoordType
    {
        SZ,//深圳坐标系
        BJ54,//北京54坐标系
        WGS84//WGS84坐标系
    }

    public class CommonToolsClass
    {
        /// <summary>
        /// 从××.××°转成××°××′××″
        /// </summary>
        /// <param name="coord"></param>
        public static string ConvertDecToLongandLat(double coord)
        {
            //var du = (int)coord;
            //var fen = (int)((coord - (int)coord) * 60);
            double miao = Math.Round(((coord - (int)coord) * 60 - (int)((coord - (int)coord) * 60)) * 60, 6);
            string coord1 = "";
            coord1 = (int)coord + "°" + (int)((coord - (int)coord) * 60) + "′" + miao.ToString("00.000000") + "″";
            return coord1;
        }

        /// <summary>
        /// 从××°××′××″转成××.××°
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static double ConvertLongandlatToDec(string coord)
        {
            string[] arr = coord.Split(new char[3] { '°', '′', '″' });
            double[] coords = new double[3];
            for (int i = 0; i < arr.Length - 1; i++)
            {
                coords[i] = Convert.ToDouble(arr[i]);
            }
            return coords[0] + (coords[1] + coords[2] / 60) / 60;
        }
    }

    public class CoordConvertToolClass
    {
        /// <summary>
        /// 将北京54坐标点批量转换为深圳坐标点
        /// </summary>
        /// <param name="p_PointCollection"></param>
        /// <returns></returns>
        public List<PointCoord> FromBJ54ToSZCoords(List<PointCoord> pointCoords)
        {
            Ellipsoid ellipsoid = EllipsoidHelper.FindEllipsoid("北京54");
            AngleConversion ac = new AngleConversion();
            GeodeticPoint geopoint = null;
            List<PointCoord> p_PointCoords = new List<PointCoord>();
            PointCoord pointCoord;
            List<CartesianPoint> cartesianPoints = new List<CartesianPoint>();
            double[] r = new double[2];
            for (int i = 0; i < pointCoords.Count; i++)
            {
                geopoint = new GeodeticPoint();
                geopoint.L = ac.SplitToRadian(pointCoords[i].X.ToString());
                geopoint.B = ac.SplitToRadian(pointCoords[i].Y.ToString());
                geopoint.CenterLon = (114 + 0 / 60.0 + 0 / 3600.0) * Math.PI / 180.0;
                CartesianPoint cPoint = GaussianConversion.Instance.GaussZheng(geopoint, ellipsoid);
                r = LocalCoordConvert.Instance.BJ_SZ(cPoint.X, cPoint.Y);
                pointCoord = new PointCoord();
                pointCoord.ID = pointCoords[i].ID;
                pointCoord.Y = r[0];
                pointCoord.X = r[1];
                p_PointCoords.Add(pointCoord);
                //r = LocalCoordConvert.Instance.BJ_SZ(cPoint.X, cPoint.Y);
                //cartesianPoints.Add(cPoint);
            }
            return p_PointCoords;
        }
       
        /// <summary>
        /// 将单个北京54坐标点转换为深圳坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public double[] FromBJ54ToSZCoordSingle(double[] coords)
        {
            double[] r = new double[2];
            Ellipsoid ellipsoid = EllipsoidHelper.FindEllipsoid("北京54");
            AngleConversion ac = new AngleConversion();
            GeodeticPoint geopoint = new GeodeticPoint();
            geopoint.L = ac.SplitToRadian(coords[0].ToString());
            geopoint.B = ac.SplitToRadian(coords[1].ToString());
            geopoint.CenterLon = (114 + 0 / 60.0 + 0 / 3600.0) * Math.PI / 180.0;
            CartesianPoint cPoint = GaussianConversion.Instance.GaussZheng(geopoint, ellipsoid);
            r = LocalCoordConvert.Instance.BJ_SZ(cPoint.X, cPoint.Y);
            return r;
        }

        /// <summary>
        /// 将单个深圳坐标点转换为北京54坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public string[] FromSZToBJ54CoordSingle(double[] coords)
        {
            string[] coordStr = new string[2];
            double[] r = new double[2];
            r = LocalCoordConvert.Instance.SZ_BJ(coords[1], coords[0]);           
            Ellipsoid ellipsoid = EllipsoidHelper.FindEllipsoid("北京54");
            AngleConversion ac = new AngleConversion();
            CartesianPoint cPoint = new CartesianPoint();
            cPoint.X = r[0];
            cPoint.Y = r[1];
            cPoint.CenterLon = 114 + 0 / 60 + 0 / 3600;
            GeodeticPoint geopoint = GaussianConversion.Instance.GaussFan(cPoint, ellipsoid);
            coordStr[0] = ac.RadianToSplit(geopoint.L);
            coordStr[1] = ac.RadianToSplit(geopoint.B);
            return coordStr;
        }

        /// <summary>
        /// 将深圳坐标点批量转换为北京54坐标点
        /// </summary>
        /// <param name="pointCoords"></param>
        /// <returns></returns>
        public List<PointCoord> FromSZToBJ54Coords(List<PointCoord> pointCoords)
        {
            List<PointCoord> coords = new List<PointCoord>();
            Ellipsoid ellipsoid = EllipsoidHelper.FindEllipsoid("北京54");
            AngleConversion ac = new AngleConversion();
            CartesianPoint cPoint;
            PointCoord coord;
            GeodeticPoint geopoint;
            double[] r = new double[2];
            for (int i = 0; i < pointCoords.Count; i++)
            {
                coord = new PointCoord();
                coord.ID = pointCoords[i].ID;
                coord.X = pointCoords[i].Y;
                coord.Y = pointCoords[i].X;
                r = LocalCoordConvert.Instance.SZ_BJ(coord.X, coord.Y);
                cPoint = new CartesianPoint();
                cPoint.X = r[0];
                cPoint.Y = r[1];
                cPoint.CenterLon = 114 + 0 / 60 + 0 / 3600;
                geopoint = GaussianConversion.Instance.GaussFan(cPoint, ellipsoid);
                coord.X = CommonToolsClass.ConvertLongandlatToDec(ac.RadianToSplit(geopoint.L));
                coord.Y = CommonToolsClass.ConvertLongandlatToDec(ac.RadianToSplit(geopoint.B));
                coords.Add(coord);
            }
            return coords;
        }

        /// <summary>
        /// 将单个北京54坐标点转换为WGS84坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public double[] FromBJ54ToWGS84CoordSingle(double[] coords)
        {
            double[] coords1 = LocalCoordConvert.Instance.BJ_WGS(coords[1], coords[0]);
            return (new double[] { coords1[0] * 180 / Math.PI, coords1[1] * 180 / Math.PI });
        }

        /// <summary>
        /// 将北京54坐标点批量转换为WGS84坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public List<PointCoord> FromBJ54ToWGS84Coords(List<PointCoord> pointcoords)
        {
            List<PointCoord> points = new List<PointCoord>();
            PointCoord point;
            double[] coords;
            for (int i = 0; i < pointcoords.Count; i++)
            {
                point = new PointCoord();
                point.ID = pointcoords[i].ID;
                coords = LocalCoordConvert.Instance.BJ_WGS((pointcoords[i].Y * Math.PI )/ 180, (pointcoords[i].X * Math.PI) / 180);
                point.X = coords[1] * 180 / Math.PI;
                point.Y = coords[0] * 180 / Math.PI;
                points.Add(point);
            }
            return points;
        }

        /// <summary>
        /// 将单个WGS84坐标点转换为北京54坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public double[] FromWGS84ToBJ54CoordSingle(double[] coords)
        {
            double[] coords1 = LocalCoordConvert.Instance.WGS_BJ(coords[1] * Math.PI / 180, coords[0] * Math.PI / 180);
            return (new double[] { coords1[1] * 180 / Math.PI, coords1[0] * 180 / Math.PI });
        }

        /// <summary>
        /// 将WGS84坐标点批量转换为北京54坐标点
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public List<PointCoord> FromWGS84ToBJ54Coords(List<PointCoord> pointcoords)
        {
            List<PointCoord> points = new List<PointCoord>();
            PointCoord point;
            double[] coords;
            for (int i = 0; i < pointcoords.Count; i++)
            {
                point = new PointCoord();
                point.ID = pointcoords[i].ID;
                coords = LocalCoordConvert.Instance.WGS_BJ((pointcoords[i].Y * Math.PI) / 180, (pointcoords[i].X * Math.PI) / 180);
                point.X = coords[1] * 180 / Math.PI;
                point.Y = coords[0] * 180 / Math.PI;
                points.Add(point);
            }
            return points;
        }

        /// <summary>
        /// 将单个WGS84坐标点转换为深圳坐标点
        /// </summary>
        /// <param name="p_PointCollection"></param>
        /// <returns></returns>
        public double[] FromWGS84ToSZCoordSingle(double[] coords)
        {
            double[] pointCoords = FromWGS84ToBJ54CoordSingle(coords);
            return FromBJ54ToSZCoordSingle(pointCoords);
        }

        /// <summary>
        /// 将WGS84坐标点批量转换为深圳坐标点
        /// </summary>
        /// <param name="p_PointCollection"></param>
        /// <returns></returns>
        public List<PointCoord> FromWGS84ToSZCoords(List<PointCoord> points)
        {
            List<PointCoord> pointCoords = FromWGS84ToBJ54Coords(points);
            return FromBJ54ToSZCoords(pointCoords);
        }

        /// <summary>
        /// 将单个深圳坐标点转换为WGS84坐标点
        /// </summary>
        /// <param name="pointCoords"></param>
        /// <returns></returns>
        public double[] FromSZToWGS84CoordSingle(double[] coords)
        {
            double[] de = new double[2];
            string[] coord = FromSZToBJ54CoordSingle(coords);
            de[0] = CommonToolsClass.ConvertLongandlatToDec(coord[0]) * Math.PI / 180.0;
            de[1] = CommonToolsClass.ConvertLongandlatToDec(coord[1]) * Math.PI / 180.0;
            return FromBJ54ToWGS84CoordSingle(de);
        }

        /// <summary>
        /// 将深圳坐标点批量转换为WGS84坐标点
        /// </summary>
        /// <param name="pointCoords"></param>
        /// <returns></returns>
        public List<PointCoord> FromSZToWGS84Coords(List<PointCoord> pointCoords)
        {
            List<PointCoord> coords = FromSZToBJ54Coords(pointCoords);
            return FromBJ54ToWGS84Coords(coords);
        }
    }
}
