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

namespace KM.JXC.Web.Controllers.api
{
    public class UsersController : ApiController
    {
        [HttpPost]
        public PQGridData GetUsers()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);

            int page = 1;
            int pageSize = 30;
            int total = 0;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);
            data.data = userMgr.GetUsers(page, pageSize, out total);
            data.totalRecords = total;
            return data;
        }
    }
}