using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

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

        [HttpPost]
        public PQGridData SearchExpresses()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            data.data = shopManager.SearchExpresses();            
            return data;
        }

        [HttpPost]
        public PQGridData SearchExpressFee()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int express_id = 0;
            int page=0;
            int pageSize=0;
            long total=0;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"], out pageSize);
            int.TryParse(request["express_id"],out express_id);
            if (page <= 0)
            {
                page = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            data.data = shopManager.SearchExpressFee(express_id, 0, 0, page, pageSize, out total);
            data.totalRecords = total;
            data.curPage = page;
            return data;
        }

        [HttpPost]
        public List<Express> GetNonAddedExpresses()
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            return shopManager.GetNonAddedExpresses();
        }

        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ApiMessage CreateShopExpress()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);

            string jsonStr = request["express"];

            jsonStr = HttpUtility.UrlDecode(jsonStr);

            JObject json = JObject.Parse(jsonStr);
            int express_id = (int)json["id"];
            int isDefault = (int)json["isdefault"];
            BShopExpress express = new BShopExpress() { ID = express_id };
            if (isDefault == 1)
            {
                express.IsDefault = true;
            }
            else {
                express.IsDefault = false;
            }
            JArray fees = (JArray)json["fees"];
            if (fees.Count > 0)
            {
                express.Fees = new List<BExpressFee>();
            }

            for (int i = 0; i < fees.Count; i++)
            {
                JObject o = (JObject)fees[i];
                int pid=(int)o["pid"];
                int cid = (int)o["cid"];
                double fee = (double)o["fee"];
                int hid = (int)o["hid"];
                BExpressFee feeObj = new BExpressFee() { Fee = fee, Province = new BArea() { ID = pid }, City = new BArea() { ID = cid }, StoreHouse = new BStoreHouse { ID = hid } };
                express.Fees.Add(feeObj);
            }

            try
            {
                shopManager.CreateShopExpress(express);
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }

            return message;
        }

        [HttpPost]
        public List<BStoreHouse> GetNonExpressedHouses()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);

            int expId = 0;
            int.TryParse(request["express_id"],out expId);
            return shopManager.GetNonExpressedHouses(expId);
        }
        [HttpPost]
        public ApiMessage SetDefaultExpress()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);

            int expId = 0;
            int.TryParse(request["express_id"], out expId);
            try
            {
                shopManager.SetDefaultExpress(expId);
                message.Status = "ok";
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }

            return message;
        }
    }
}