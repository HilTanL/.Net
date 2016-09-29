using System;
using System.Collections.Generic;
using System.Text;
using CoordTranBase;
using CoordTranBase.Core;

namespace CoordTrans
{

    /// <summary>
    /// 四参数：名称，X平移，Y平移，旋转，缩放，
    /// 示例：北京54 》深圳,-2465703.47656,-433214.67538,-0.0170807871073908,1.00000892668697,
    /// </summary>
    public class LocalCoordConvert : CoordTranBase.Singleton<LocalCoordConvert>
    {
        //由深圳系统换算北京坐标系统
        //Xk(i)=Sa*Xs(i)-Sb*Ys(i)+P
        //Yk(i)=Sb*Xs(i)+Sa*Ys(i)+Q
        double Sa = 0.9998451733929467;
        double Sb = -0.0170804815937289;
        double P = 2472721.23534;
        double Q = 391032.19937;

        /// <summary>
        /// 深圳坐标转北京54，三位有效数字
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double[] SZ_BJ(double x, double y)
        {
            double[] d = new double[2];
            d[0] = Math.Round(Sa * x - Sb * y + P, 5);
            d[1] = Math.Round(Sb * x + Sa * y + Q, 5);
            return d;
        }

        //由北京坐标系统换算深圳系统
        //Xs(i)=Sa*Xk(i)-Sb*Yk(i)+P
        //Ys(i)=Sb*Xk(i)+Sa*Yk(i)+Q
        double Sa1 = 0.9998630573363253;
        double Sb1 = 0.0170807871073908;
        double P1 = -2465703.47656;
        double Q1 = -433214.67538;

        /// <summary>
        /// 北京54转深圳坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double[] BJ_SZ(double x, double y)
        {
            double[] d = new double[2];
            d[0] = Math.Round(Sa1 * x - Sb1 * y + P1, 5);
            d[1] = Math.Round(Sb1 * x + Sa1 * y + Q1, 5);
            return d;
        }

        /// <summary>
        /// 北京54经纬度 到 WGS84经纬度，单位为弧度
        /// </summary>
        /// <param name="B">纬度</param>
        /// <param name="L">经度</param>
        /// <returns></returns>
        public double[] BJ_WGS(double B, double L)
        {
            Ellipsoid bj = EllipsoidHelper.FindEllipsoid("北京54");
            Ellipsoid wgs = EllipsoidHelper.FindEllipsoid("WGS84");
            //BJ54 转 空间直角坐标
            double[] BJ_BLH = new double[] { B, L, 0 };
            double[] BJ_XYZ = GeodeticCore.BLH_XYZ(bj, BJ_BLH);
            // BJ_XYZ 转为 ITRF93 XYZ
            ParsObject pars = SerializeHelper.FindParsObject(
                SerializeHelper.Read(), "七参数", "北京54 >> ITRF93");
            // ITRF93 XYZ
            CoordPoint coord = ComputeSevenPars.CalCoord(pars as SevenPars,
                new CoordPoint(BJ_XYZ[0], BJ_XYZ[1], BJ_XYZ[2]));
            // ITRF93 BLH
            coord = GeodeticCore.XYZ_BLH(wgs, coord);
            return new double[] { coord.X, coord.Y, coord.Z };
        }
        /// <summary>
        /// WGS84经纬度 到 北京54经纬度，单位为弧度
        /// </summary>
        /// <param name="B">纬度</param>
        /// <param name="L">经度</param>
        /// <returns></returns>
        public double[] WGS_BJ(double B, double L)
        {
            Ellipsoid bj = EllipsoidHelper.FindEllipsoid("北京54");
            Ellipsoid wgs = EllipsoidHelper.FindEllipsoid("WGS84");
            //ITRF93 转 空间直角坐标
            double[] BLH = new double[] { B, L, 0 };
            double[] XYZ = GeodeticCore.BLH_XYZ(wgs, BLH);
            // ITRF93 XYZ 转为 BJ_XYZ
            ParsObject pars = SerializeHelper.FindParsObject(
                SerializeHelper.Read(), "七参数", "ITRF93 >> 北京54");
            // BJ54 XYZ
            CoordPoint coord = ComputeSevenPars.CalCoord(pars as SevenPars,
                new CoordPoint(XYZ[0], XYZ[1], XYZ[2]));
            // BJ54 BLH
            coord = GeodeticCore.XYZ_BLH(bj, coord);
            return new double[] { coord.X, coord.Y, coord.Z };
        }
    }
}
