using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace OfficeExcel
{
    public class ExcelChartClass
    {
        public static void ShowChart()
        {
            try
            {
                Application excelApplication = null;
                object missing = System.Type.Missing;
                excelApplication = new Microsoft.Office.Interop.Excel.Application();

                string file = "d:\\1.xls";

                Workbook workbook = excelApplication.Workbooks.Open(file, missing, missing, missing, missing, missing,
                    missing, missing, missing, missing, missing, missing, missing, missing, missing);
                excelApplication.Visible = true;

                //Workbook wb = excelApplication.Workbooks.Add(XlSheetType.xlWorksheet);
                Worksheet workSheet = (Worksheet)excelApplication.ActiveSheet;

                // Now create the chart.
                //ChartObjects charts = (ChartObjects)workSheet.ChartObjects(Type.Missing);
                ChartObjects charts = (ChartObjects)workSheet.ChartObjects(0);



                //设置图表大小。
                //ChartObject chartObj = charts.Add(0, 0, 400, 300);
                ChartObject chartObj = charts.Item(0) as ChartObject;

                Chart chart = chartObj.Chart;


                //设置图表数据区域。
                Range range = workSheet.get_Range("K10", "AN7");
                chart.ChartWizard(range, XlChartType.xlLine, missing, XlRowCol.xlColumns,
                    1, 1, true, "标题", "X轴标题", "Y轴标题", missing);

                //将图表移到数据区域之下。
                chartObj.Left = Convert.ToDouble(range.Left);
                chartObj.Top = Convert.ToDouble(range.Top) + Convert.ToDouble(range.Height);
            }
            catch 
            { }
        }

    }
}
