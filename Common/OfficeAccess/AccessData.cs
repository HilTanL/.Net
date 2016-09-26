using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeAccess
{
    public class AccessData
    {
        #region 变量
        private static OleDbConnection conn = new OleDbConnection();
        private static OleDbCommand comm = new OleDbCommand();
        //private static string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=ahwildlife.mdb;Persist Security Info=False;Jet OLEDB:Database Password=sa;";
        #endregion

        #region 打开数据库
        /// <summary>
        /// 打开数据库
        /// </summary>
        private static void openConnection()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=ahwildlife.mdb;Persist Security Info=False;Jet OLEDB:Database Password=sa;";
                comm.Connection = conn;
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #region 关闭数据库
        /// <summary>
        /// 关闭数据库
        /// </summary>
        private static void closeConnection()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
                conn.Dispose();
                comm.Dispose();
            }
        }
        #endregion

        #region 执行sql语句
        /// <summary>
        /// 执行sql语句
        /// </summary>
        public static void ExecuteSql(string sqlstr)
        {
            try
            {
                openConnection();
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                closeConnection();
            }
        }
        #endregion

        #region 返回指定sql语句的OleDbDataReader对象,使用时请注意关闭这个对象。
        /// <summary>
        /// 返回指定sql语句的OleDbDataReader对象,使用时请注意关闭这个对象。
        /// </summary>
        public static OleDbDataReader DataReader(string sqlstr)
        {
            OleDbDataReader dr = null;
            try
            {
                openConnection();
                comm.CommandText = sqlstr;
                comm.CommandType = CommandType.Text;

                dr = comm.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                try
                {
                    dr.Close();
                    closeConnection();
                }
                catch { }
            }
            return dr;
        }
        #endregion

        #region 返回指定sql语句的OleDbDataReader对象,使用时请注意关闭
        /// <summary>
        /// 返回指定sql语句的OleDbDataReader对象,使用时请注意关闭
        /// </summary>
        public static void DataReader(string sqlstr, ref OleDbDataReader dr)
        {
            try
            {
                openConnection();
                comm.CommandText = sqlstr;
                comm.CommandType = CommandType.Text;
                dr = comm.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                try
                {
                    if (dr != null && !dr.IsClosed)
                        dr.Close();
                }
                catch
                {
                }
                finally
                {
                    closeConnection();
                }
            }
        }
        #endregion

        #region 返回指定sql语句的DataSet
        /// <summary>
        /// 返回指定sql语句的DataSet
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        public static DataSet DataSet(string sqlstr)
        {
            DataSet ds = new DataSet();
            OleDbDataAdapter da = new OleDbDataAdapter();
            try
            {
                openConnection();
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                da.SelectCommand = comm;
                da.Fill(ds);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                closeConnection();
            }
            return ds;
        }
        #endregion

        #region 返回指定sql语句的DataSet
        /// <summary>
        /// 返回指定sql语句的DataSet
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="ds"></param>
        public static void DataSet(string sqlstr, ref DataSet ds)
        {
            OleDbDataAdapter da = new OleDbDataAdapter();
            try
            {
                openConnection();
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                da.SelectCommand = comm;
                da.Fill(ds);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                closeConnection();
            }
        }
        #endregion

        #region 返回指定sql语句的DataTable
        /// <summary>
        /// 返回指定sql语句的DataTable
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        //public static DataTable DataTable(string sqlstr)
        //{
        //    DataTable dt = Common.GetDataTableCache(sqlstr);//读缓存
        //   if (dt != null)
        //    {
        //        return dt.Copy();
        //    }
        //    else
        //    {
        //        dt = new DataTable();
        //        OleDbDataAdapter da = new OleDbDataAdapter();
        //        try
        //        {
        //            using (OleDbConnection conn = new OleDbConnection())
        //            {
        //                conn.ConnectionString = connectionString;
        //                conn.Open();
        //                using (OleDbCommand comm = new OleDbCommand())
        //                {
        //                    comm.Connection = conn;
        //                    comm.CommandType = CommandType.Text;
        //                    comm.CommandText = sqlstr;
        //                    da.SelectCommand = comm;
        //                    da.Fill(dt);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            throw new Exception(e.Message);
        //        }
        //        finally
        //        {
        //            closeConnection();
        //        }
        //        Common.InsertDataTableCache(sqlstr, dt);//添加缓存
        //       return dt.Copy();
        //    }
        //}
        #endregion

        #region 返回指定sql语句的DataTable
        /// <summary>
        /// 返回指定sql语句的DataTable
        /// </summary>
        public static void DataTable(string sqlstr, ref DataTable dt)
        {
            OleDbDataAdapter da = new OleDbDataAdapter();
            try
            {
                openConnection();
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                da.SelectCommand = comm;
                da.Fill(dt);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                closeConnection();
            }
        }
        #endregion

        #region 返回指定sql语句的DataView
        /// <summary>
        /// 返回指定sql语句的DataView
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        public static DataView DataView(string sqlstr)
        {
            OleDbDataAdapter da = new OleDbDataAdapter();
            DataView dv = new DataView();
            DataSet ds = new DataSet();
            try
            {
                openConnection();
                comm.CommandType = CommandType.Text;
                comm.CommandText = sqlstr;
                da.SelectCommand = comm;
                da.Fill(ds);
                dv = ds.Tables[0].DefaultView;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                closeConnection();
            }
            return dv;
        }
        #endregion
    }
}