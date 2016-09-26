using CLog;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace OfficeExcel
{
    public class WorkbookUtil2
    {
        private Microsoft.Office.Interop.Excel.Application excelApp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;

        public WorkbookUtil2(Microsoft.Office.Interop.Excel.Workbook workbook)
        {
            this.workbook = workbook;
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
                Log.GetInstance().WriteError("WorkbookUtil2.CopySheet()" + e.Message);
            }
        }

        public void DelSheet(string strSheet)
        {
            try
            {
                if (!IsExit(strSheet)) return;
                if (GetSheetCount() <= 1) return;

                Worksheet worksheet = (Worksheet)workbook.Worksheets[strSheet];
                worksheet.Delete();
                workbook.Save();
                Util.Release(worksheet);
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("WorkbookUtil2.CopySheet()" + e.Message);
            }
        }

        public void SetCellVal(string strSheet, int iRow, int iCol, string strVal)
        {
            try
            {
                Worksheet worksheet = (Worksheet)workbook.Worksheets[strSheet];
                worksheet.Cells[iRow, iCol] = strVal;
                Util.Release(worksheet);
                worksheet = null;
            }
            catch (Exception)
            {
            }
        }
        
        public bool IsExit(string strSheet)
        {
            if (workbook == null) return false;

            List<string> listNames = new List<string>();
            foreach (Worksheet item in workbook.Worksheets)
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
                if(excelApp!=null)
                {
                    excelApp = null;
                    Util.Release(excelApp);
                }
            }
            catch (Exception e)
            {
                Log.GetInstance().WriteError("WorkbookUtil2.Dispose()", e.Message);
            }
        }

    }
}
