using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using CLog;

namespace OfficeExcel
{
    public class ExcelDataHelper
    {
        private string conn_format = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Excel 8.0;HDR=no;IMEX=1;";
        private string conn_str = "";

        public ExcelDataHelper(string file)
        {
            conn_str = string.Format(conn_format, file);
        }

        /// <summary>
        /// 获取指定表名的Excel数据表
        /// </summary>
        /// <param name="sheet_name"></param>
        /// <returns></returns>
        public DataTable GetSheetTable(string sheet_name)
        {
            DataTable dataTable = null;
            try
            {
                using (OleDbConnection conn = new OleDbConnection(conn_str))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接


                    string sqlText = "select * from [" + sheet_name + "$];";
                    OleDbCommand oleCmd = new OleDbCommand(sqlText, conn);
                    OleDbDataAdapter oleDbAdp = new OleDbDataAdapter(oleCmd);

                    dataTable = new DataTable();
                    oleDbAdp.Fill(dataTable);
                    dataTable.TableName = System.IO.Path.GetFileNameWithoutExtension(sheet_name);
                }
                return dataTable;
            }
            catch(Exception e)
            {
                Log.GetInstance().WriteError("GetSheetTable--" + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取Excel数据表的数量
        /// </summary>
        /// <returns></returns>
        public int GetSheetCount()
        {
            try
            {
                int iCount = 0;
                using (OleDbConnection conn = new OleDbConnection(conn_str))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接
                    DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, null);
                    iCount = dt.Select("TABLE_NAME LIKE '*$'").Length;
                }
                return iCount;
            }
            catch
            {
                return 0;
            }
        }

        public string[] GetSheetNames()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(conn_str))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接
                    DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, null);
                    DataRow[] rows = dt.Select("TABLE_NAME LIKE '*$'");
                    string[] result = new string[rows.Length];
                    for(int i=0;i<rows.Length;++i)
                    {
                        result[i] = rows[i]["TABLE_NAME"].ToString().Replace("$", "");
                    }
                    return result;
                }
            }
            catch(Exception e)
            {
                Log.GetInstance().WriteError("GetSheetNames--"+e.Message);
                return new string[0];
            }
        }

        /// <summary>
        /// 获取指定索引的数据表名
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetSheetName(int index)
        {
            try
            {
                string strTableName = "";
                using (OleDbConnection conn = new OleDbConnection(conn_str))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接
                    DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Statistics, null);
                    strTableName = dt.Select("cardinality='1'")[index]["TABLE_NAME"].ToString();
                    strTableName = strTableName.Substring(0, strTableName.Length - 1);
                }
                return strTableName;
            }
            catch
            {
                return "";
            }
        }

        public List<DataTable> GetAllSheetData()
        {
            List<DataTable> listTables = new List<DataTable>();
            int tableCount = GetSheetCount();
            for (int i = 0; i < tableCount; ++i)
            {
                DataTable dt = GetSheetTable(GetSheetName(i));
                if (dt != null)
                    listTables.Add(dt);
            }
            return listTables;
        }
        

    }
}
