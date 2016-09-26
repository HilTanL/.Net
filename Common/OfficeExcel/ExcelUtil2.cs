using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLog;
using System.Data;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using System.IO;
using Utils;

namespace OfficeExcel
{
    /// <summary>
    /// 使用Office提供的接口进行操作
    /// </summary>
    public class ExcelUtil2
    {
        Microsoft.Office.Interop.Excel.Application excelApp;
        Microsoft.Office.Interop.Excel.Application excelAppFake;//用于屏蔽手动打开的excel
        Microsoft.Office.Interop.Excel.Workbook workbook;
        private object locker = new object();

        public ExcelUtil2()
        {
            excelAppFake = new Application();
            excelApp = new Application();
            excelApp.DisplayAlerts = false;
        }

        public Microsoft.Office.Interop.Excel.Workbook GetWorkbook(string strFilePath)
        {
            lock (locker)
            {
                return excelApp.Workbooks.Open(strFilePath);
            }
        }

        public void OpenExcel(string strFilePath)
        {
            if (workbook != null)
            {
                if (!workbook.Saved)
                    workbook.Save();
                workbook.Close();
                Util.Release(workbook);
                workbook = null;
            }
            workbook = excelApp.Workbooks.Open(strFilePath);
        }

        public void CloseExcel()
        {
            if (workbook != null)
            {
                if (!workbook.Saved)
                    workbook.Save();
                workbook.Close();
                Util.Release(workbook);
                workbook = null;
            }
        }

        public void CopySheet(string strSheetSrc, string strDes)
        {
            try
            {
                if (IsExit(strDes)) return;

                Worksheet worksheet = (Worksheet)workbook.Worksheets[strSheetSrc];
                worksheet.Copy((Worksheet)workbook.Sheets[workbook.Sheets.Count], Type.Missing);

                Worksheet worksheetNew = (Worksheet)workbook.Worksheets[workbook.Sheets.Count - 1];
                worksheetNew.Name = strDes;

                workbook.Save();
                Util.Release(worksheet);
                Util.Release(worksheetNew);
            }
            catch (Exception e)
            {
                //if (workbook != null)
                //    workbook.Save();
                excelApp.Quit();
                Log.GetInstance().WriteError("CopySheet"+ e.Message);
            }
        }

        public void SetCellVal(string strSheet, int iRow, int iCol, string strVal)
        {
            try
            {
                Worksheet worksheet = (Worksheet)workbook.Worksheets[strSheet];
                worksheet.Cells[iRow, iCol] = strVal;
            }
            catch(Exception)
            {
            }
        }

        /// <summary>
        /// 将模板中的sheet复制到新路径中
        /// </summary>
        /// <param name="strSheetSrc"></param>
        /// <param name="strDesFilePath"></param>
        /// <param name="strSheetDes"></param>
        public void CopySheet(string strSheetSrc,string strDesFilePath,string strSheetDes)
        {
            //Microsoft.Office.Interop.Excel.Application excelOld = new Microsoft.Office.Interop.Excel.Application();
            //Microsoft.Office.Interop.Excel.Workbook wordOld = null;
            //Microsoft.Office.Interop.Excel.Workbook wordNew = null;
            //try
            //{
            //    //文件存在性检查
            //    if (!File.Exists(strFilePath) || !File.Exists(strDesFilePath)) return;

            //    //源工作表
            //    wordOld = excelOld.Workbooks.Open(strFilePath);
            //    Worksheet sheetSrc = wordOld.Worksheets[strSheetSrc];

            //    //新工作表
            //    wordNew = excelOld.Workbooks.Open(strDesFilePath);
            //    sheetSrc.Copy((Worksheet)wordNew.Sheets[wordNew.Sheets.Count], Type.Missing);
            //    Worksheet sheetNew = wordOld.Worksheets[wordNew.Sheets.Count - 1];
            //    sheetNew.Name = strSheetDes;

            //    wordOld.Save();
            //    wordNew.Save();
            //    excelOld.Quit();
            //}
            //catch (Exception e)
            //{
            //    if (wordOld != null)
            //        wordOld.Save();

            //    excelOld.Quit();
            //    Log.GetInstance().WriteError("CopySheet" + strFilePath + e.Message);
            //}
        }

        public void DelSheet(string strSheet)
        {
            try
            {
                if (!IsExit(strSheet)) return;
                if (GetSheetCount() <= 1) return;

                Worksheet worksheet = (Worksheet)workbook.Worksheets[strSheet];
                excelApp.DisplayAlerts = false;
                worksheet.Delete();
                excelApp.DisplayAlerts = true;

                workbook.Save();
                Util.Release(worksheet);
            }
            catch (Exception e)
            {
                excelApp.Quit();
                Log.GetInstance().WriteError("CopySheet" + e.Message);
            }
        }

        public bool IsExit(string strSheet)
        {
            if (workbook == null) return false;

            List<string> listNames = new List<string>();
            foreach(Worksheet item in workbook.Worksheets)
            {
                listNames.Add(item.Name);
                Util.Release(item);
            }
            return listNames.Contains(strSheet);
        }

        public int GetSheetCount()
        {
            if (workbook == null) return 0;

            return workbook.Worksheets.Count;
        }

        public void Dispose()
        {
            try
            {
                if (workbook != null)
                {
                    if (!workbook.Saved)
                        workbook.Save();
                    workbook.Close();
                    Util.Release(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Util.Release(excelApp);
                }
                if (excelAppFake != null)
                {
                    excelAppFake.Quit();
                    Util.Release(excelAppFake);
                }
            }
            catch (Exception)
            {
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Util.Release(excelApp);
                }
            }

            
        }

    }
}
