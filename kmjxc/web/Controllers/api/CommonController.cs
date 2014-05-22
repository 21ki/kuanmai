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
    public class CommonController : ApiController
    {
        [HttpPost]
        public List<Common_District> GetAreas()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;          
            List<Common_District> areas = new List<Common_District>();
            CommonManager commonMgr = new CommonManager();
            int parent_id = 0;
            int.TryParse(request["parent_id"],out parent_id);
            areas = commonMgr.GetAreas(parent_id);
            return areas;
        }

        [HttpPost]
        public List<MallTradeStatus> GetMallStatusForSyncTrades()
        {
            List<MallTradeStatus> status = new List<MallTradeStatus>();
            
            return status;
        }
    }
}