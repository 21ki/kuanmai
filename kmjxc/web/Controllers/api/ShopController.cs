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
        public List<BStoreHouse> GetStoreHouses()
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager shopManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            return shopManager.GetStoreHouses();
        }

        [HttpPost]
        public ApiMessage UpdateExpressFee()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int express_fee_id = 0;
            double fee = 0;
            int.TryParse(request["express_fee_id"],out express_fee_id);
            double.TryParse(request["fee"],out fee);
            try
            {
                shopManager.UpdateExpressFee(express_fee_id, fee);
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误，请联系管理员";
            }
            finally
            {
                
            }
            return message;
        }

        [HttpPost]
        public ApiMessage CreateExpressFees()
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
            BShopExpress express = new BShopExpress() { ID = express_id };
            express.IsDefault = false;
            JArray fees = (JArray)json["fees"];
            if (fees.Count > 0)
            {
                express.Fees = new List<BExpressFee>();
            }

            for (int i = 0; i < fees.Count; i++)
            {
                JObject o = (JObject)fees[i];
                int pid = (int)o["pid"];
                int cid = (int)o["cid"];
                double fee = (double)o["fee"];
                int hid = (int)o["hid"];
                BExpressFee feeObj = new BExpressFee() { Fee = fee, Province = new BArea() { ID = pid }, City = new BArea() { ID = cid }, StoreHouse = new BStoreHouse { ID = hid } };
                express.Fees.Add(feeObj);
            }

            try
            {
                shopManager.CreateExpressFees(express);
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }

            return message;
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

        [HttpPost]
        public PQGridData SearchChildShop()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            List<BShop> shops= shopManager.SearchChildShops();
            data.totalRecords = shops.Count;
            data.data = shops;
            data.curPage = 1;
            return data;
        }

        [HttpPost]
        public ApiMessage AddChildShop()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            string child_shop = request["shop_name"];
            int mtype = 0;
            int.TryParse(request["type"],out mtype);
             
            try
            {
                if (shopManager.AddChildShop(mtype, child_shop))
                {
                    message.Status = "failed";
                    message.Message = "添加失败";
                }
                else
                {
                    message.Message = "添加子店铺请求已经发出，等待子店铺主账户登录进销存批准请求";
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
                message.Status = "failed";
                message.Message = "未知错误，请联系管理员";
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public PQGridData SearchSentChildRequets()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            List<BAddChildRequest> requests = shopManager.SearchSentAddChildRequests();
            data.data = requests;
            data.totalRecords = requests.Count;
            data.curPage = 1;
            return data;
        }

        [HttpPost]
        public PQGridData SearchReceivedChildRequets()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            List<BAddChildRequest> requests = shopManager.SearchReceivedAddChildRequests();
            data.data = requests;
            data.totalRecords = requests.Count;
            data.curPage = 1;
            return data;
        }

        [HttpPost]
        public ApiMessage HandleAddChildRequest()
        {
            ApiMessage message = new ApiMessage() { Status="ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int reqId = 0;
            int status = 0;
            int.TryParse(request["reqId"],out reqId);
            int.TryParse(request["status"],out status);
            try
            {
                shopManager.HandleAddChildRequest(reqId, status);
            }
            catch (KMJXCException kex)
            {
                message.Message = kex.Message;
                message.Status = "failed";
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public PQGridData SearchShopUsers()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);

            int page = 1;
            int pageSize = 30;
            int shop_id = 0;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);
            int.TryParse(request["shop"], out shop_id);
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            long total = 0;
            List<BUser> users = shopManager.SearchShopUsers(page, pageSize, out total, shop_id);
            data.data = users;
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public ApiMessage SyncMallSoldProducts()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            try
            {
                List<BMallProduct> newProducts= shopManager.SyncMallOnSaleProducts();
                message.Item = newProducts.Count;
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch(Exception ex)
            {
            }
            finally
            {
            }
            return message;
        }

        [HttpPost]
        public PQGridData SearchOnSaleProducts()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int total = 0;
            int page = 1;
            int pageSize = 30;
            bool? connected = null;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            if (request["connected"] != null)
            {
                if (request["connected"] == "1")
                {
                    connected = true;
                }
                else if (request["connected"] == "0")
                {
                    connected = false;
                }
            }

            List<BMallProduct> products = shopManager.SearchOnSaleMallProducts(page, pageSize, out total, connected);
            data.data = products;
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }
    }
}