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
    public class StockExcelReport:BaseExcel
    {
        public StockExcelReport()
            : base(ReportType.StockReport)
        {
        }
        public override string Export(string json)
        {
            string fileName = "";
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    throw new KMJXCException("没有库存数据，不能导出");
                }

                JArray jObject = JArray.Parse(json);
                if (jObject == null || jObject.Count() == 0)
                {
                    throw new KMJXCException("没有库存数据，不能导出");
                }

                Worksheet sheet = (Worksheet)this.WorkBook.ActiveSheet;
                sheet.Name = "库存报表";
                int startRow = 2;
                object[,] os = new object[jObject.Count,4];
                for (int i = 0; i < jObject.Count(); i++)
                {
                    JObject obj = (JObject)jObject[i];
                    string productName = obj["product_name"].ToString();
                    string propName = obj["prop_name"].ToString();
                    string shopName = obj["shop_name"].ToString();                    
                    string quantity = obj["quantity"].ToString();
                    os[i, 0] = productName;
                    os[i, 1] = propName;
                    os[i, 2] = shopName;
                    os[i, 3] = quantity;
                }
                Range range1=sheet.Cells[startRow, 1];
                Range range2=sheet.Cells[startRow + jObject.Count-1, 4];
                Range range=sheet.get_Range(range1,range2);
                range.Value2 = os;
                Worksheet pivotTableSheet = (Worksheet)this.WorkBook.Worksheets[2];
                pivotTableSheet.Name = "库存透视表";                
                PivotCaches pch = WorkBook.PivotCaches();
                sheet.Activate();
                pch.Add(XlPivotTableSourceType.xlDatabase, "'" + sheet.Name + "'!A1:'" + sheet.Name + "'!D" + (jObject.Count() + 1)).CreatePivotTable(pivotTableSheet.Cells[4, 1], "PivTbl_1", Type.Missing, Type.Missing);
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
                PivotField shopField = (PivotField)pvt.PivotFields("店铺");
                shopField.Orientation = XlPivotFieldOrientation.xlRowField;
                shopField.set_Subtotals(1, false);
                PivotField stockField = pvt.PivotFields(4);
                pvt.AddDataField(stockField, "库存总数", XlConsolidationFunction.xlSum);
                ((PivotField)pvt.DataFields["库存总数"]).NumberFormat = "#,##0";
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
