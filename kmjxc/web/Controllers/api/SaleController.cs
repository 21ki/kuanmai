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
    public class SaleController : BaseApiController
    {
        [HttpPost]
        public PQGridData SearchBackSale()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int page = 1;
            int pageSize = 30;
            int sale_id = 0;
            int back_id = 0;
            int uid = 0;
            int stime = 0;
            int etime = 0;
            if (!string.IsNullOrEmpty(request["sdate"]))
            {
                DateTime sdate = DateTime.MinValue;
                DateTime.TryParse(request["sdate"], out sdate);
                if (sdate != DateTime.MinValue)
                {
                    stime = DateTimeUtil.ConvertDateTimeToInt(sdate);
                }
            }
            if (!string.IsNullOrEmpty(request["edate"]))
            {
                DateTime edate = DateTime.MinValue;
                DateTime.TryParse(request["edate"], out edate);
                if (edate != DateTime.MinValue)
                {
                    etime = DateTimeUtil.ConvertDateTimeToInt(edate);
                }
            }
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            int.TryParse(request["sale_id"], out sale_id);
            int.TryParse(request["back_id"], out back_id);
            int.TryParse(request["user_id"], out uid);
            int total = 0;
            int[] backids = null;
            if (back_id > 0)
            {
                backids = new int[] { back_id };
            }
            int[] userids = null;
            if (uid > 0)
            {
                userids = new int[] { uid };
            }
            int[] saleids = null;
            if (sale_id > 0)
            {
                saleids = new int[] { sale_id };
            }
            data.data = saleManager.SearchBackSales(saleids, userids, stime, etime, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }
    }
}
