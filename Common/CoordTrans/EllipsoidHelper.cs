using System;
using System.Collections.Generic;
using System.Text;
using CoordTranBase;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace CoordTrans
{
    public class EllipsoidHelper
    {
        public static List<string> EllipsoidList = new List<string>() { "北京54", "西安80", "国家2000", "WGS84"};
        public static List<Control> EllipsoidUI = new List<Control>();
        public static readonly string EllipsoidConfig = AppDomain.CurrentDomain.BaseDirectory + "ellipsoid.dll";

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public static void IniConfig()
        {
            List<Ellipsoid> list = new List<Ellipsoid>();
            foreach (string name in EllipsoidList)
            {
                list.Add(FindEllipsoid(name));
            }
            Record(list,false);//记录不刷新
        }

        /// <summary>
        /// 读取参数
        /// </summary>
        /// <returns></returns>
        public static List<Ellipsoid> Read()
        {
            if (!File.Exists(EllipsoidConfig)) return new List<Ellipsoid>();

            using (FileStream fs = new FileStream(EllipsoidConfig, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(fs) as List<Ellipsoid>;
            }
        }
        /// <summary>
        /// 记录参数
        /// </summary>
        /// <param name="pars"></param>
        /// <param name="refreshUI">是否更新界面，默认更新</param>
        public static void Record(List<Ellipsoid> pars, params bool[] refreshUI)
        {
            if (pars == null) pars = new List<Ellipsoid>();

            using (FileStream fs = new FileStream(EllipsoidConfig, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, pars);
                fs.Close();
            }
            // 为 false 不更新界面
            if (refreshUI.Length > 0 && refreshUI[0] == false) return;

            RefreshUI();//记录时刷新界面
        }

        /// <summary>
        /// 刷新界面控件
        /// </summary>
        public static void RefreshUI()
        {
            foreach (Control ctrl in EllipsoidUI)
            {
                if (ctrl is ComboBox)
                    IniComboBox(ctrl as ComboBox);
                else if (ctrl is ListView)
                    IniListView(ctrl as ListView);
            }
        }

        public static void IniListView(ListView lv)
        {
            lv.Items.Clear();

            foreach (string p in EllipsoidList)
            {
                Ellipsoid e = FindEllipsoid(p);

                ListViewItem item = new ListViewItem(p);
                item.Tag = e;
                lv.Items.Add(item);
            }
        }

        public static void IniComboBox(ComboBox cb)
        {
            cb.Items.Clear();cb.Text = "";

            foreach (string p in EllipsoidList)
            {
                cb.Items.Add(p);
            }
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
        }

        /// <summary>
        /// 根据椭球名称查找椭球对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Ellipsoid，找不到范围 null</returns>
        public static Ellipsoid FindEllipsoid(string name)
        {
            
            Ellipsoid gaussEllipsoid = new Ellipsoid();
            switch (name)
            {
                case "WGS84":
                    gaussEllipsoid.MaxAxis = 6378137;
                    gaussEllipsoid.MinAxis = 6356752.3142;
                    gaussEllipsoid.Type = EllipsoidType.WGS84;
                    gaussEllipsoid.Name = "WGS 84";
                    break;
                case "北京54":
                    gaussEllipsoid.MaxAxis = 6378245;
                    gaussEllipsoid.MinAxis = 6356863.0187730473;
                    gaussEllipsoid.Type = EllipsoidType.BJ54;
                    gaussEllipsoid.Name = "Krasovsky_1940";
                    break;
                case "西安80":
                    gaussEllipsoid.MaxAxis = 6378140;
                    gaussEllipsoid.MinAxis = 6356755.2881575287;
                    gaussEllipsoid.Type = EllipsoidType.XA80;
                    gaussEllipsoid.Name = "IAG 75";
                    break;
                case "国家2000":
                    gaussEllipsoid.MaxAxis = 6378137;
                    gaussEllipsoid.MinAxis = 6356752.31414;
                    gaussEllipsoid.Type = EllipsoidType.GJ2000;
                    gaussEllipsoid.Name = "CGCS 2000";
                    break;
                default:
                    gaussEllipsoid = null;
                    break;
            }
            return gaussEllipsoid;
        }
    }
}
