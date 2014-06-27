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
    public class SaleExcelReport:BaseExcel
    {
        public SaleExcelReport():base(ReportType.SaleReport)
        {
        
        }

        public override string Export(string json)
        {
            string fileName = "";
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    throw new KMJXCException("没有销售数据，不能导出");
                }

                JArray jObject = JArray.Parse(json);
                if(jObject==null || jObject.Count()==0)
                {
                    throw new KMJXCException("没有销售数据，不能导出");
                }

                Worksheet sheet = (Worksheet)this.WorkBook.ActiveSheet;
                sheet.Name = "销售报表";
                int startRow = 2;
                for (int i = 0; i < jObject.Count(); i++)
                {
                    JObject obj = (JObject)jObject[i];
                    string productName = obj["ProductName"].ToString();
                    string propName = obj["PropName"].ToString();
                    string shopName = obj["ShopName"].ToString();
                    string month = obj["Month"].ToString();
                    string quantity =obj["Quantity"].ToString();
                    string amount = obj["Amount"].ToString();
                    Range range1 = (Range)sheet.Cells[startRow+i,1];
                    Range range2 = (Range)sheet.Cells[startRow + i, 2];
                    Range range3 = (Range)sheet.Cells[startRow + i, 3];
                    Range range4 = (Range)sheet.Cells[startRow + i, 4];
                    Range range5 = (Range)sheet.Cells[startRow + i, 5];
                    Range range6 = (Range)sheet.Cells[startRow + i, 6];
                    range1.Value = productName;
                    range2.Value = propName;
                    range3.Value = shopName;
                    range4.Value = month;
                    range5.Value = quantity;
                    range6.Value = amount;
                }

                Worksheet pivotTableSheet = (Worksheet)this.WorkBook.Worksheets[2];

                pivotTableSheet.Name = "销售透视表";
                PivotCaches pch = WorkBook.PivotCaches();
                sheet.Activate();
                pch.Add(XlPivotTableSourceType.xlDatabase, "'" + sheet.Name + "'!A1:'" + sheet.Name + "'!F" + (jObject.Count() + 1)).CreatePivotTable(pivotTableSheet.Cells[4, 1], "PivTbl_1", Type.Missing, Type.Missing);
                PivotTable pvt = pivotTableSheet.PivotTables("PivTbl_1") as PivotTable;
                pvt.Format(XlPivotFormatType.xlTable1);
                pvt.TableStyle2 = "PivotStyleLight16";
                //pvt.TableStyle2 = "PivotStyleMedium6";

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

                PivotField monthField = (PivotField)pvt.PivotFields("年月");
                monthField.Orientation = XlPivotFieldOrientation.xlRowField;
                monthField.set_Subtotals(1, false);
                //poField.LabelRange.HorizontalAlignment = L10NExcel.XlHAlign.xlHAlignCenter;

                pvt.AddDataField(pvt.PivotFields(5), "销量", XlConsolidationFunction.xlSum);
                pvt.AddDataField(pvt.PivotFields(6), "销售额", XlConsolidationFunction.xlSum);


                ((PivotField)pvt.DataFields["销量"]).NumberFormat = "#,##0";
                ((PivotField)pvt.DataFields["销售额"]).NumberFormat = "#,##0";

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
