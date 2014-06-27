using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.BL.Models.Admin;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;
namespace KM.JXC.Web.Controllers.api
{
    public class BuyController : BaseApiController
    {
        [HttpPost]
        public PQGridData ExportBuyOrders()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int total = 0;
            int page = 1;
            int pageSize = 30;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            int order_id = 0;
            int supplier_id = 0;
            int.TryParse(request["order_id"], out order_id);
            int.TryParse(request["supplier_id"], out supplier_id);
            string keyword = request["keyword"];
            int[] orders = null;
            if (order_id > 0)
            {
                orders = new int[] { order_id };
            }
            int[] suppliers = null;
            if (supplier_id > 0)
            {
                suppliers = new int[] { supplier_id };
            }
            data.data = buyManager.SearchBuyOrders(orders, null, suppliers, null, keyword, 0, 0, page, pageSize, out total);
            data.totalRecords = total;
            return data;
        }
    }
}