using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using KMEXCEL = Microsoft.Office.Interop.Excel;

namespace KM.JXC.BL
{
    public enum ReportType
    {
        SaleReport,
        BuyOrderReport
    }

    public class BaseExcel:IDisposable
    {
        public KMEXCEL.Application ExcelApp { get; set; }
        public KMEXCEL.Workbook WorkBook { get; set; }
        private bool IsDisposed = false;
        ReportType type;
        public string TemplatePath { get; set; }
        public string ReportTmpFilePath { get; set; }
        public string ReportFileName { get;set; }
        public string ReportFilePath { get; set; }
        public BaseExcel(ReportType t)
        {
            try
            {
                this.type = t;
                this.ExcelApp = new KMEXCEL.Application();
                this.ExcelApp.ScreenUpdating = true;
                this.ExcelApp.Visible = false;
                this.ExcelApp.DisplayAlerts = false;
                string tmpPath = ConfigurationManager.AppSettings["ReportsPath"];
                this.TemplatePath = tmpPath + "\\templates";
                this.ReportTmpFilePath = tmpPath + "\\tmp";

                if (this.type == ReportType.SaleReport)
                {
                    string reportTmpName = System.IO.Path.Combine(TemplatePath, this.type.ToString() + ".xlsx");
                    ReportFileName = Guid.NewGuid().ToString() + ".xlsx";
                    ReportFilePath = System.IO.Path.Combine(ReportTmpFilePath, ReportFileName);
                    System.IO.File.Copy(reportTmpName, ReportFilePath);
                    this.WorkBook = this.ExcelApp.Workbooks.Open(ReportFilePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                }
            }
            catch (Exception ex)
            {
                throw new KM.JXC.Common.KMException.KMJXCException("服务器没有安装 Microsoft Excel 2007");
            }
        }

        ~BaseExcel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {

                }

                //Release unmanaged code.              

                //if (XLApp != null)
                //{
                //    XLApp.Quit();
                //}

                //Kill EXCEL.EXE
                Process[] xlProcesses = Process.GetProcessesByName("EXCEL");

                if (xlProcesses != null && xlProcesses.Length > 0)
                {
                    for (int i = 0; i < xlProcesses.Length; i++)
                    {
                        xlProcesses[i].Kill();
                    }
                }

                IsDisposed = true;

            }
        }

        public virtual string Export(string JsonStr)
        {
            string path=null;
            return path;
        }

        public virtual string Export(System.Data.DataTable table)
        {
            string path = null;
            return path;
        }
    }
}
