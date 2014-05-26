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
    public class ShopController : ApiController
    {
        [HttpPost]
        public PQGridData SearchCustomer()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager stockManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission,userMgr);
            long totalRecords = 0;
            int page = 1;
            int pageSize = 30;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"], out pageSize);
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            data.data = stockManager.SearchCustomers(page,pageSize,out totalRecords);
            data.curPage = page;
            data.totalRecords = totalRecords;
            return data;
        }
    }
}