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

        [HttpPost]
        public ApiMessage CreateBuyPrice()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                string details = request["price_details"];
                string desc = request["desc"];
                string title = request["title"];
                int shopId = 0;
                int.TryParse(request["shop_id"], out shopId);
                if (!string.IsNullOrEmpty(details))
                {
                    details = HttpUtility.UrlDecode(details);
                }

                JArray jDetails = JArray.Parse(details);
                BBuyPrice buyPrice = new BBuyPrice() { Desc = desc, Title = title, Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                buyPrice.Shop = new BShop() { ID = buyManager.Shop.Shop_ID };
                if (shopId > 0)
                {
                    buyPrice.Shop = new BShop() { ID = shopId };
                }
                buyPrice.Details = new List<BBuyPriceDetail>();
                for (int i = 0; i < jDetails.Count(); i++)
                {
                    JObject jDetail = (JObject)jDetails[i];
                    BBuyPriceDetail bDetail = new BBuyPriceDetail();
                    bDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    bDetail.Price = (double)jDetail["price"];
                    bDetail.Product = new BProduct() { ID = (int)jDetail["sku_id"], ParentID = (int)jDetail["product_id"] };
                    bDetail.Supplier = new BSupplier() { ID = (int)jDetail["supplier_id"] };
                    buyPrice.Details.Add(bDetail);
                }

                bool result = buyManager.CreateBuyPrice(buyPrice);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            {
                message.Status = "failed";
                message.Message = dbex.Message;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public ApiMessage SaveBuyPrice()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                int buyPriceId = 0;
                string details = request["price_details"];
                string desc = request["desc"];
                string title = request["title"];
                int shopId = 0;
                int.TryParse(request["shop_id"], out shopId);
                if (!string.IsNullOrEmpty(details))
                {
                    details = HttpUtility.UrlDecode(details);
                }

                int.TryParse(request["price_id"], out buyPriceId);

                JArray jDetails = JArray.Parse(details);
                BBuyPrice buyPrice = new BBuyPrice() { ID = buyPriceId, Desc = desc, Title = title, Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                buyPrice.Shop = new BShop() { ID = buyManager.Shop.Shop_ID };
                if (shopId > 0)
                {
                    buyPrice.Shop = new BShop() { ID = shopId };
                }
                buyPrice.Details = new List<BBuyPriceDetail>();
                for (int i = 0; i < jDetails.Count(); i++)
                {
                    JObject jDetail = (JObject)jDetails[i];
                    BBuyPriceDetail bDetail = new BBuyPriceDetail();
                    bDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    bDetail.Price = (double)jDetail["price"];
                    bDetail.Product = new BProduct() { ID = (int)jDetail["sku_id"], ParentID = (int)jDetail["product_id"] };
                    bDetail.Supplier = new BSupplier() { ID = (int)jDetail["supplier_id"] };
                    buyPrice.Details.Add(bDetail);
                }

                bool result = buyManager.SaveBuyPrice(buyPrice);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            {
                message.Status = "failed";
                message.Message = dbex.Message;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public ApiMessage GetBuyPriceFullInfo()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BBuyPrice buyPrice = null;
            int priceId = 0;
            try
            {
                int.TryParse(request["buy_price_id"], out priceId);
                if (priceId <= 0)
                {
                    message.Status = "failed";
                    message.Message = "没有输入询价单编号";
                    return message;
                }

                buyPrice = buyManager.GetBuyPriceFullInfo(priceId);

                if (buyPrice == null)
                {
                    message.Status = "failed";
                    message.Message = "询价单编号错误";
                    return message;
                }

                message.Item = buyPrice;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            {
                message.Status = "failed";
                message.Message = dbex.Message;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public PQGridData GetBuyPrices()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            string keyword = request["keyword"];
            int total = 0;
            int page = 1;
            int pageSize = 30;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            int price_user_id = 0;
            int supplier_id = 0;
            int.TryParse(request["user_id"], out price_user_id);
            int.TryParse(request["supplier_id"], out supplier_id);
            int buyPriceId = 0;
            int.TryParse(request["price_id"], out buyPriceId);
            string keyWord = request["keyword"];
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            data.data = buyManager.SearchBuyPrices(buyPriceId, price_user_id, supplier_id, 0,keyword, page, pageSize, out total);
            data.totalRecords = total;
            data.curPage = page;
            data.pageSize = pageSize;
            return data;
        }
    }
}