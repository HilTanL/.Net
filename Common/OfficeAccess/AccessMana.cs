using CLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeAccess
{ 
    /// <summary>
    /// 数据库文件操作相关类
    /// </summary>
    public class AccessMana
    {
        private string str_conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5";

        private OleDbConnection conn=null;

        /// <summary>
        /// 新建数据库
        /// </summary>
        /// <returns>-1表示数据库路径有误,0表示异常,1表示成功创建,2表示文件已存在</returns>
        public int CreateAccessDB(string filePath)
        {
            if (File.Exists(filePath)) return 2;
            ADOX.Catalog catalog = new ADOX.Catalog();
            try
            {

                catalog.Create(string.Format(str_conn, filePath));
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                //关闭数据库：
                ADODB.Connection connection = catalog.ActiveConnection as ADODB.Connection;
                if (connection != null)
                {
                    connection.Close();
                }
                catalog.ActiveConnection = null;
                catalog = null;
            }

            return 1;
        }

        /// <summary>
        /// 在指定数据库中创建数据表
        /// </summary>
        /// <param name="mdbPath">数据库路径</param>
        /// <param name="tableName">数据表名</param>
        /// <param name="mdbHead">数据表头</param>
        /// <returns>-1表示异常,1表示成功创建,2表示表存在</returns>
        public int CreateAccessTable(string mdbPath, string tableName, IList<ADOX.Column> listCol)
        {
            //判断表是否存在
            if (IsTableExit(tableName)) return 2;

            ADOX.Catalog cat = new ADOX.Catalog();
            string sAccessConnection = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath;
            ADODB.Connection cn = new ADODB.Connection();
            try
            {
                cn.Open(sAccessConnection, null, null, -1);
                cat.ActiveConnection = cn;
                //新建一个表  
                ADOX.Table table = new ADOX.Table();
                table.ParentCatalog = cat;
                table.Name = tableName;
                for (int i = 0; i < listCol.Count; ++i)
                {
                    ADOX.Column col = listCol[i];
                    col.ParentCatalog = cat;
                    table.Columns.Append(col);
                    col.Properties[2].Value = "项目编号";
                }
                cat.Tables.Append(table);
                cn.Close();
                return 1;
            }
            catch (Exception)
            {
                cn.Close();
                return -1;
            }
        }

        public void CopyAccessTable(string sourcePath, string desPath, string sourceTableName, string desTableName)
        {
            string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + desPath + ";Persist Security Info=False";
            string sql = "Select * into " + desTableName + "  From [;database=" + sourcePath + "]." + sourceTableName;

            OleDbConnection conn = null;
            OleDbCommand command = null;
            try
            {
                conn = new OleDbConnection(connStr);
                conn.Open();
                command = new OleDbCommand(sql, conn);
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch
            { }
            finally
            {
                if (conn != null) conn.Close();
                if (command != null) command.Dispose();
            }
        }

        public List<string> GetAccessTableNames()
        {
            List<string> listTables = new List<string>();
            try
            {
                DataTable shemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                int n = shemaTable.Rows.Count;
                int m = shemaTable.Columns.IndexOf("TABLE_NAME");
                for (int i = 0; i < n; i++)
                {
                    DataRow m_DataRow = shemaTable.Rows[i];
                    listTables.Add(m_DataRow.ItemArray.GetValue(m).ToString());
                }
                return listTables;
            }
            catch
            {
                return listTables;
            }
        }

        public bool IsTableExit(string tableName)
        {
            List<string> listTableNames = GetAccessTableNames();
            if (listTableNames.IndexOf(tableName) < 0) return false;
            else return true;
        }

        public void OpenConn(string strMdbPath)
        {
            try
            {
                conn = new OleDbConnection(string.Format(str_conn, strMdbPath));
                conn.Open();
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("OpenConn()"+strMdbPath+"-"+ e.Message);
            }
        }
        public void CloseConn()
        {
            if (conn != null)
                conn.Close();
        }

        public int ExecuteSql(string strSql)
        {
            if (conn != null)
            {
                OleDbCommand comm = new OleDbCommand(strSql, conn);
                return comm.ExecuteNonQuery();
            }
            else
                return 0;
        }

        public int ExecuteSql(string strSql,params OleDbParameter[] paras)
        {
            if (conn != null)
            {
                try
                {
                    OleDbCommand comm = new OleDbCommand(strSql, conn);
                    if (paras != null)
                        comm.Parameters.AddRange(paras);
                    return comm.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log.GetInstance().WriteError("异常",strSql, e.Message);
                    return 0;
                }
            }
            else
                return 0;
        }

        public object ExecuteScalar(string strSql)
        {
            try
            {
                if (conn != null)
                {
                    OleDbCommand comm = new OleDbCommand(strSql, conn);
                    return comm.ExecuteScalar();
                }
                else
                    return null;
            }
            catch (Exception )
            {
                return null;
            }
        }

    }

}
