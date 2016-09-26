using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Excel = Microsoft.Office.Interop.Excel;
using CLog;

namespace OfficeExcel
{
    /// <summary>
    /// Sheets集合代表当前工作簿中的所有工作表,包括图表工作表、对话框工作表和宏表。 
    /// Worksheets集合仅代表当前工作簿中的所有工作表。
    /// </summary>
    public class ExcelDataClass
    {

        /// <summary>
        /// 把表格导入到 Excel 中
        /// </summary>
        /// <param name="reportData"></param>
        /// <param name="saveFilePath"></param>
        /// <param name="modelFilePath"></param>
        /// <param name="startCellRow"></param>
        /// <param name="startCellColumn"></param>
        public static void GetGeoReport(DataTable reportData, string saveFilePath,
            string modelFilePath, int startCellRow, int startCellColumn)
        {
            Excel._Application excelReportApp = null;
            try
            {
                if (reportData == null)
                { return; }

                if (reportData.Rows.Count == 0)
                {
                    MessageBox.Show("数据不存在，无法生成报表！", "提示：", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                int rowCount = reportData.Rows.Count;
                int columnCount = reportData.Columns.Count;


                //文件操作
                if (File.Exists(saveFilePath))
                {
                    //判断文件是否打开
                    try
                    {
                        File.Delete(saveFilePath);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("目标文件被打开，请关闭后重试。");
                        return;
                    }
                }
                File.Copy(modelFilePath, saveFilePath);

                object missing = System.Reflection.Missing.Value;
                //打开 Excel 模板
                excelReportApp = new Microsoft.Office.Interop.Excel.Application();
                //excelReportApp.Visible = true;

                Excel.Workbook workBook = excelReportApp.Workbooks.Open(saveFilePath, missing, missing, missing, missing,
                    missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);

                // Worksheets 与 Sheets 的区别 
                Excel.Sheets sheets = workBook.Sheets;
                sheets = workBook.Worksheets;
                int sheetCount = workBook.Worksheets.Count;

                //注意：工作表的页面索引是从 1 开始的
                Excel.Worksheet worksheet = null;

                /*
                 * 表的数据操作
                 */
                sheetCount = 1;//此处只生成一个表
                for (int i = 1; i < sheetCount + 1; i++)
                {
                    worksheet = (Excel.Worksheet)workBook.Worksheets.get_Item(1);

                    int tempColumnNum = 0;
                    int tempRowNum = startCellRow;
                    //生成表数据
                    for (int row = 0; row < rowCount; row++)
                    {
                        tempColumnNum = startCellColumn;
                        for (int col = 0; col < columnCount; col++)
                        {
                            Excel.Range range = (Excel.Range)worksheet.Cells[tempRowNum, tempColumnNum];
                            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                            range.Value2 = reportData.Rows[row][col];

                            tempColumnNum += 1;//列数递增
                        }
                        tempRowNum += 1;//行数递增
                    }
                }

                //生成后显示报表
                excelReportApp.Visible = true;
            }
            catch (Exception ex)
            {
                excelReportApp.Quit();
                Log.GetInstance().WriteError("ExcelDataClass--" + ex.Message);
            }
            finally
            {
                Marshal.ReleaseComObject(excelReportApp);
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        /// <summary>
        /// 获取 Excel 中所有表
        /// </summary>
        /// <returns></returns>
        public static List<DataTable> ExcelToTableList(string fileName)
        {
            try
            {
                // 如果文件不存在则返回 null
                if(!System.IO.File.Exists(fileName)) return null;

                /*HDR=Yes:第一行作为表头；IMEX=1:指定所有内容都转换为字符串*/
                string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};" +
                    "Excel 8.0;HDR=Yes;IMEX=1;";
                connString = string.Format(connString, fileName);

                List<DataTable> listTable = new List<DataTable>();
                DataTable dataTable;
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    conn.Open();

                    //历遍Sheet，显示每个Sheet名。 
                    DataTable sheetTable = conn.GetSchema("Tables");

                    string sheetName = "";
                    foreach (DataRow sheetRow in sheetTable.Rows)
                    {
                        if (sheetRow["Table_Type"].ToString() == "TABLE")
                        {
                            sheetName = sheetRow["Table_Name"].ToString();
                            //真正的表名
                            if (sheetName.IndexOf("$") != sheetName.Length - 1) continue;

                            //历遍某一sheet的所有
                            string sqlText = "select * from [" + sheetName + "];";
                            OleDbCommand oleCmd = new OleDbCommand(sqlText, conn);
                            OleDbDataAdapter oleDbAdp = new OleDbDataAdapter(oleCmd);

                            dataTable = new DataTable();
                            oleDbAdp.Fill(dataTable);
                            dataTable.TableName = sheetName.Substring(0, sheetName.Length - 1);//表名 Sheet1$
                            //所有列转为字符类型
                            //foreach (DataColumn col in dataTable.Columns)
                            //{
                            //    //在列包含数据的情况下不能更改其 DataType。
                            //    //col.DataType = System.Type.GetType("System.String");
                            //}
                            DeleteBlankRow(dataTable);//删除空行
                            listTable.Add(dataTable);
                        }
                    }
                    return listTable;
                }
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 获取Excel第一张表 “Sheet1$”的 DataTable
        /// </summary>
        /// <returns></returns>
        public static DataTable ExcelToTable(string excelFile)
        {
            try
            {
                if (!System.IO.File.Exists(excelFile)) return null;
                /*HDR=yes/no:第一行是否作为表头；IMEX=1:指定所有内容都转换为字符串*/
                string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + excelFile +
                    ";Excel 8.0;HDR=no;IMEX=1;";

                DataTable dataTable;
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接

                    string sqlText = "select * from [Sheet1$];";
                    OleDbCommand oleCmd = new OleDbCommand(sqlText, conn);
                    OleDbDataAdapter oleDbAdp = new OleDbDataAdapter(oleCmd);

                    dataTable = new DataTable();
                    oleDbAdp.Fill(dataTable);
                    dataTable.TableName = System.IO.Path.GetFileNameWithoutExtension(excelFile);

                    return DeleteBlankRow(dataTable);//删除空行;
                }
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 提取 Excel 中的某张表
        /// </summary>
        /// <param name="excelFile">文件名</param>
        /// <param name="sheetName">表名</param>
        /// <returns>表 DataTable</returns>
        public static DataTable ExcelToTable(string excelFile,string sheetName)
        {
            try
            {
                if (!System.IO.File.Exists(excelFile)) return null;
                /*HDR=yes/no:第一行是否作为表头；IMEX=1:指定所有内容都转换为字符串*/
                string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + excelFile +
                    ";Excel 8.0;HDR=yes;IMEX=1;";

                DataTable dataTable;
                using (OleDbConnection conn = new OleDbConnection(connString))
                {
                    if (conn.State != ConnectionState.Open) { conn.Open(); }//打开连接

                    string sqlText = "select * from [" + sheetName + "$];";
                    OleDbCommand oleCmd = new OleDbCommand(sqlText, conn);
                    OleDbDataAdapter oleDbAdp = new OleDbDataAdapter(oleCmd);

                    dataTable = new DataTable();
                    oleDbAdp.Fill(dataTable);
                    dataTable.TableName = System.IO.Path.GetFileNameWithoutExtension(excelFile);

                    return DeleteBlankRow(dataTable);//删除空行;
                }
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 删除空行
        /// </summary>
        /// <param name="regionTable"></param>
        /// <returns></returns>
        private static DataTable DeleteBlankRow(DataTable regionTable)
        {
            bool blank = false;
            if (regionTable == null) return null;
            foreach (DataRow row in regionTable.Rows)
            {
                blank = true;//默认为空
                for (int i = 0; i < regionTable.Columns.Count; i++)
                {
                    if (row[i].ToString().Trim() != "")
                    {
                        blank = false; //如果有一个不为空，则返回
                        break;
                    }
                }
                if (blank) 
                    row.Delete();
            }
            regionTable.AcceptChanges(); //提交更改
            return regionTable;
        }

        /// <summary>
        /// 获取坐标点目录下的 Excel 文件列表名
        /// </summary>
        /// <param name="directory">Excel 坐标表目录</param>
        /// <returns></returns>
        public static List<string> GetFileNamesList(string directory)
        {
            try
            {
                List<string> fileList = new List<string>();

                foreach (string file in GetFilesList(directory))
                {
                    fileList.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }

                return fileList;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 获取顶级目录下的文件列表
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static List<string> GetFilesList(string directory)
        {
            try
            {
                List<string> fileList = new List<string>();
                string[] files;
                if (System.IO.Directory.Exists(directory))
                {
                    files = System.IO.Directory.GetFiles(directory, "*.xls",
                        System.IO.SearchOption.TopDirectoryOnly);
                    fileList.AddRange(files);
                }
                return fileList;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 获取指定目录下，界址点的信息
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="formatType"></param>
        /// <returns></returns>
        public static Dictionary<string, DataTable> GetExcelFileTables(string directory)
        {
            try
            {
                if (!System.IO.Directory.Exists(directory)) return null;//目录不存在就返回

                Dictionary<string, DataTable> excelFileTables = new Dictionary<string, DataTable>();
                List<string> filesList = GetFilesList(directory);
                DataTable tempTable;

                foreach (string file in filesList)
                {
                    try
                    {
                        tempTable = ExcelToTable(file);
                        // 编号从表内部提取
                        excelFileTables.Add(tempTable.Rows[1][0].ToString(), tempTable);
                    }
                    catch (Exception ex)
                    {
                        Log.GetInstance().WriteError("ExcelHelper.ExcelDataClass--GetExcelFileTables" + ex.Message);
                    }
                }

                return excelFileTables;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 获取 Excel 中的所有表
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Dictionary<string, List<DataTable>> GetExcelTables(string directory)
        {
            try
            {
                if (!System.IO.Directory.Exists(directory)) return null;//目录不存在就返回

                Dictionary<string, List<DataTable>> excelFileTables = new Dictionary<string, List<DataTable>>();
                List<string> filesList = GetFilesList(directory);
                List<DataTable> tempTableList;

                foreach (string file in filesList)
                {
                    try
                    {
                        tempTableList = ExcelToTableList(file);
                        // 编号从表内部提取
                        excelFileTables.Add(file, tempTableList);
                    }
                    catch (Exception ex)
                    {
                        Log.GetInstance().WriteError("ExcelHelper.ExcelDataClass.GetExcelTables--"+ex.Message);
                    }
                }

                return excelFileTables;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 由 DataTable 创建新表
        /// </summary>
        /// <param name="dt"></param>
        public static void CreateDBTable(DataTable dt)
        {
            try
            {
                string dbConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" 
                + System.Windows.Forms.Application.StartupPath + "\\Data\\Data.mdb;" 
                + "Persist Security Info=True";
                
                if (dbConn == "") return;

                string tname = dt.TableName.Trim();   //检测表名
                using (OleDbConnection conn = new OleDbConnection(dbConn))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = conn;

                    if (tname == "") tname = "test";
                    //如果有同名表则删除
                    try
                    {
                        cmd.CommandText = "DROP TABLE " + tname;
                        cmd.ExecuteNonQuery();
                    }
                    catch { }

                    string cmdstr; // 命令字符串

                    // 创建表
                    cmdstr = "CREATE TABLE " + tname + " (";
                    foreach (DataColumn dc in dt.Columns)
                    {
                        cmdstr += "[" + dc.ColumnName + "] " + dc.DataType.ToString().Substring(7, 
                            dc.DataType.ToString().Length - 7) + ",";
                    }
                    cmdstr = cmdstr.Substring(0, cmdstr.Length - 1) + ")";// 去掉最后一个逗号
                    cmd.CommandText = cmdstr;
                    cmd.ExecuteNonQuery(); // 执行创建表

                    // 添加数据
                    foreach (DataRow dr in dt.Rows)
                    {
                        cmdstr = "INSERT INTO " + tname + " (";
                        // 字段名称
                        foreach (DataColumn dc in dt.Columns)
                        {
                            cmdstr += "[" + dc.ColumnName + "],";
                        }
                        cmdstr = cmdstr.Substring(0, cmdstr.Length - 1) + ") VALUES ("; // 去掉最后一个逗号
                        // 字段对应的值
                        foreach (DataColumn dc in dt.Columns)
                        {
                            cmdstr += "'" + dr[dc.ColumnName].ToString().Replace("'", "''") + "',";
                        }
                        
                        cmdstr = cmdstr.Substring(0, cmdstr.Length - 1) + ")";// 去掉最后一个逗号
                        cmd.CommandText = cmdstr;
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Dispose();
                    conn.Close();
                }
            }
            catch(Exception ex) 
            { MessageBox.Show("导入表出错!\n"+ ex.Message); }
        }

        /// <summary>
        /// 获取所有列名称
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<string> GetColumnNames(DataTable table)
        {
            List<string> listNames = new List<string>();
            if (table == null) return listNames;
            
            foreach (DataColumn dc in table.Columns)
            {
                listNames.Add(dc.ColumnName); 
            }
            return listNames;
        }
    }

}
