using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;

using KM.JXC.BL;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;

namespace KM.JXC.BL.Excel
{
    public class BuyExcelReport : BaseExcel
    {
        public BuyExcelReport()
            : base(ReportType.BuyOrderReport)
        {
        }

        public override string Export(JToken[] jObject)
        {
            string fileName = "";
            try
            {
                if (jObject == null || jObject.Length<=0)
                {
                    throw new KMJXCException("没有符合要求的采购数据，不能导出");
                }

                Worksheet sheet = (Worksheet)this.WorkBook.ActiveSheet;
                sheet.Name = "采购报表";
                int startRow = 2;
                object[,] os = new object[jObject.Length, 5];
                for (int i = 0; i < jObject.Count(); i++)
                {
                    JToken obj = (JToken)jObject[i];
                    string productName = obj["product_name"].ToString();
                    string propName = obj["prop_name"].ToString();                  
                    string month = obj["month"].ToString();
                    string quantity = obj["quantity"].ToString();
                    string amount = obj["amount"].ToString();
                    os[i, 0] = productName;
                    os[i, 1] = propName;                  
                    os[i, 2] = month;
                    os[i, 3] = quantity;
                    os[i, 4] = amount;
                }
                Range range1 = sheet.Cells[startRow, 1];
                Range range2 = sheet.Cells[startRow + jObject.Length - 1, 5];
                Range range = sheet.get_Range(range1, range2);
                range.Value2 = os;
                Worksheet pivotTableSheet = (Worksheet)this.WorkBook.Worksheets[2];
                pivotTableSheet.Name = "采购透视表";
                PivotCaches pch = WorkBook.PivotCaches();
                sheet.Activate();
                pch.Add(XlPivotTableSourceType.xlDatabase, "'" + sheet.Name + "'!A1:'" + sheet.Name + "'!E" + (jObject.Count() + 1)).CreatePivotTable(pivotTableSheet.Cells[4, 1], "PivTbl_1", Type.Missing, Type.Missing);
                PivotTable pvt = pivotTableSheet.PivotTables("PivTbl_1") as PivotTable;
                pvt.Format(XlPivotFormatType.xlTable1);
                pvt.TableStyle2 = "PivotStyleLight16";
                pvt.InGridDropZones = true;
                foreach (PivotField pf in pvt.PivotFields() as PivotFields)
                {
                    pf.ShowDetail = false;
                }
                PivotField productField = (PivotField)pvt.PivotFields("产品");
                productField.Orientation = XlPivotFieldOrientation.xlRowField;
                productField.set_Subtotals(1, false);
                PivotField propField = (PivotField)pvt.PivotFields("属性");
                propField.Orientation = XlPivotFieldOrientation.xlRowField;
                propField.set_Subtotals(1, false);   
                PivotField monthField = (PivotField)pvt.PivotFields("年月");
                monthField.Orientation = XlPivotFieldOrientation.xlRowField;
                monthField.set_Subtotals(1, false);
                pvt.AddDataField(pvt.PivotFields(4), "采购数量", XlConsolidationFunction.xlSum);
                pvt.AddDataField(pvt.PivotFields(5), "采购金额", XlConsolidationFunction.xlSum);
                ((PivotField)pvt.DataFields["采购数量"]).NumberFormat = "#,##0";
                ((PivotField)pvt.DataFields["采购金额"]).NumberFormat = "#,##0";
                pivotTableSheet.Activate();
                this.WorkBook.Saved = true;
                this.WorkBook.SaveCopyAs(this.ReportFilePath);
                this.WorkBook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return fileName;
        }
    }
}
