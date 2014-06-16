using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using KM.JXC.BL;
using KM.JXC.BL.Models;
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
            int stime = 0;
            int etime = 0;
            int page = 1;
            int pageSize = 50;
            int totalProducts = 0;
            int[] product_id = null;

            int.TryParse(request["stime"],out stime);
            int.TryParse(request["stime"], out etime);
            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);

            if (!string.IsNullOrEmpty(request["products"]))
            {
                product_id = base.ConvertToIntArrar(request["products"]);
            }

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }

            string json = reportManager.GetSalesReport(stime, etime, product_id, page, pageSize, out totalProducts, true, false);
            data.totalRecords = totalProducts;
            data.data = JArray.Parse(json);
            data.curPage = page;
            return data;
        }
    }
}