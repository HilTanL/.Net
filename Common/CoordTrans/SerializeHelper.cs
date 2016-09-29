using System;
using System.Collections.Generic;
using System.Text;
using CoordTranBase.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace CoordTrans
{

    public class SerializeHelper
    {
        public static string ParsConfig = AppDomain.CurrentDomain.BaseDirectory + "data.dll";

        /// <summary>
        /// 读取参数
        /// </summary>
        /// <returns></returns>
        public static List<ParsObject> Read()
        {
            if (!File.Exists(ParsConfig)) return new List<ParsObject>();

            using (FileStream fs = new FileStream(ParsConfig, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(fs) as List<ParsObject>;
            }
        }
        /// <summary>
        /// 记录参数
        /// </summary>
        /// <param name="pars"></param>
        /// <param name="refreshUI">是否更新界面，默认更新</param>
        public static void Record(List<ParsObject> pars, params bool[] refreshUI)
        {
            if (pars == null) pars = new List<ParsObject>();

            using (FileStream fs = new FileStream(ParsConfig, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, pars);
                fs.Close();
            }
            // 为 false 不更新界面
            if (refreshUI.Length > 0 && refreshUI[0] == false) return;

            RefreshParsUI();//记录时刷新界面
        }

        /// <summary>
        /// 删除参数对象
        /// </summary>
        /// <param name="par"></param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteObject(ParsObject par)
        {
            if (par == null) return false;
            List<ParsObject> list = Read();
            if (FindParsObject(list, par.type, par.name) == null)
                return false;
            // 删除更新数据
            //list.Remove(par);//无法删除
            for (int i = 0; i < list.Count; i++)
            {
                ParsObject p = list[i];
                if (p.name == par.name && p.type == par.type)
                {
                    list.Remove(p); 
                }
            }
            Record(list, true);
            return true;
        }

        /// <summary>
        /// 记录参数对象，返回true表示存在同名对象
        /// </summary>
        /// <param name="pars"></param>
        /// <param name="replace">是否替换</param>
        /// <returns>是否存在</returns>
        public static bool RecordObject(ParsObject pars, bool replace)
        {
            if (pars == null) return false;
            pars.type = (pars is FourPars) ? "四参数" : "七参数";

            List<ParsObject> list = Read();
            if (list.Count == 0 || FindParsObject(list, pars.type, pars.name) == null)
            {
                list.Add(pars);
                Record(list);
                return false;
            }
            else if (replace) // 覆盖同名对象
            {
                for (int i = 0; i < list.Count; i++)
                {
                CHECK:
                    if (i == list.Count) break;
                    ParsObject p = list[i];
                    if (p.name == pars.name && p.type == pars.type)
                    {
                        list.Remove(p); goto CHECK;
                    }
                }
                list.Add(pars); Record(list); return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 查找参数对象
        /// </summary>
        /// <param name="pars"></param>
        /// <param name="par_name"></param>
        /// <param name="type">四参数或七参数</param>
        /// <returns>null 未找到</returns>
        public static ParsObject FindParsObject(List<ParsObject> pars, string type, string par_name)
        {
            if (pars == null) return null;

            foreach (ParsObject p in pars)
            {
                if (p.name == par_name && p.type == type) return p;
            }
            return null;
        }

        public static void IniLvPars(ListView lv, string type)
        {
            List<ParsObject> list = Read();
            lv.Items.Clear();

            foreach (ParsObject p in list)
            {
                if (p.type != type) continue;

                ListViewItem item = new ListViewItem(p.name);
                item.Tag = p;
                lv.Items.Add(item);
            }
        }

        public static void IniCobPars(ComboBox cb, string type)
        {
            List<ParsObject> list = Read();
            cb.Items.Clear();
            cb.Text = "";

            foreach (ParsObject p in list)
            {
                if (p.type != type) continue;
                cb.Items.Add(p.name);
            }
            if (cb.Items.Count > 0) cb.SelectedIndex = 0;
        }

        /// <summary>
        /// 刷新 UI 界面
        /// </summary>
        public static void RefreshParsUI()
        {
            foreach (KeyValuePair<string, List<Control>> kv in AppConfig.ParsUI)
            {
                foreach (Control c in kv.Value)
                {
                    if (c is ListView) IniLvPars(c as ListView, kv.Key);
                    else if (c is ComboBox) IniCobPars(c as ComboBox, kv.Key);
                }
            }
        }
    }
}
