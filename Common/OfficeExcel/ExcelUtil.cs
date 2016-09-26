using CLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace OfficeExcel
{
    public class ExcelUtil
    {
        private string strConnRead = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Excel 8.0;HDR=no;IMEX=1;";
        private string strConnWrite = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Excel 8.0;HDR=no;IMEX=0;";

        public ExcelUtil(string strFilePath)
        {
            strConnWrite = string.Format(strConnWrite, strFilePath);
            strConnRead = string.Format(strConnRead, strFilePath);
        }

        /// <summary>
        /// 获取指定表名的Excel数据表
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <returns></returns>
        public DataTable GetSheetTable(string strFilePath ,string strSheetName)
        {
            strConnRead = string.Format(strConnRead, strFilePath);
            DataTable dataTable = null;
            try
            {
                using (OleDbConnection conn = new OleDbConnection(strConnRead))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接

                    string sqlText = "select * from [" + strSheetName + "$];";
                    OleDbCommand oleCmd = new OleDbCommand(sqlText, conn);
                    OleDbDataAdapter oleDbAdp = new OleDbDataAdapter(oleCmd);

                    dataTable = new DataTable();
                    oleDbAdp.Fill(dataTable);
                    dataTable.TableName = System.IO.Path.GetFileNameWithoutExtension(strSheetName);
                }
                return dataTable;
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("GetSheetTable--" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 向Excel中插入数据
        /// </summary>
        /// <param name="strSheetName"></param>
        /// <param name="dt"></param>
        public void Insert(string strSheetName, DataTable dt)
        {
            try
            {
                using (OleDbConnection ole_conn = new OleDbConnection(strConnWrite))
                {
                    ole_conn.Open();
                    using (OleDbCommand ole_cmd = ole_conn.CreateCommand())
                    {
                        for (int i = 0; i < dt.Rows.Count; ++i)
                        {
                            string strVals = "";
                            for (int j = 0; j < dt.Columns.Count; ++j)
                            {
                                strVals += "'" + dt.Rows[i][j].ToString() + "',";
                            }
                            strVals=strVals.Remove(strVals.Length - 1);
                            ole_cmd.CommandText = "insert into [" + strSheetName + "$] values(" + strVals + ")";
                            ole_cmd.ExecuteNonQuery();
                        }
                    }
                    ole_conn.Close();
                    Utils.Util.Release(ole_conn);
                }
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("Insert", e.Message, strSheetName);
            }
        }

        private void ExcuteSql(string strFilePath,string strSql)
        {
            strConnRead = string.Format(strConnRead, strFilePath);
            //实例化一个Oledbconnection类(实现了IDisposable,要using)
            using (OleDbConnection ole_conn = new OleDbConnection(strConnRead))
            {
                ole_conn.Open();
                using (OleDbCommand ole_cmd = ole_conn.CreateCommand())
                {
                    ole_cmd.CommandText = strSql;
                    ole_cmd.ExecuteNonQuery();
                }
            }
        }



    }
}
