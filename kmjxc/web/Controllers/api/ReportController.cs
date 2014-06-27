using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.BL.Excel;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;

namespace KM.JXC.Web.Controllers.api
{
    public class ReportController : BaseApiController
    {
        [HttpPost]
        public PQGridData GetSalesReport()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ReportFactory reportManager = new ReportFactory(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            long stime = 0;
            long etime = 0;
            int page = 1;
            int pageSize = 50;
            int totalProducts = 0;
            string[] product_id = null;
            bool paging = false;
            long.TryParse(request["stime"], out stime);
            long.TryParse(request["etime"], out etime);
            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);

            if (!string.IsNullOrEmpty(request["products"]))
            {
                product_id = request["products"].Split(',');
            }

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }
            if (!string.IsNullOrEmpty(request["paging"]) && request["paging"] == "1")
            {
                paging = true;
            }
            else
            {
                paging = false;
            }
            try
            {
                string json = reportManager.GetSalesReport(stime, etime, product_id, page, pageSize, out totalProducts, paging, false);
                data.totalRecords = totalProducts;
                if (!string.IsNullOrEmpty(json))
                {
                    data.data = JArray.Parse(json);
                }
                data.curPage = page;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                
            }
            return data;
        }

        [HttpPost]
        public ApiMessage GetExcelSaleReport()
        {
            ApiMessage message = new ApiMessage() {Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ReportFactory reportManager = new ReportFactory(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            long stime = 0;
            long etime = 0;
         
            int totalProducts = 0;
            string[] product_id = null;
           
            long.TryParse(request["stime"], out stime);
            long.TryParse(request["etime"], out etime);           

            if (!string.IsNullOrEmpty(request["products"]))
            {
                product_id = request["products"].Split(',');
            }
           
            try
            {
                string json = reportManager.GetSalesReport(stime, etime, product_id, 0, 0, out totalProducts, false, false);
                SaleExcelReport excel = new SaleExcelReport();
                excel.Export(json);
                message.Item ="http://"+ request.Url.Authority+"/Content/reports/tmp/"+ excel.ReportFileName;               
            }
            catch (Exception ex)
            {
                message.Status = "failed";
            }
            finally
            {

            }
            return message;
        }
    }
}