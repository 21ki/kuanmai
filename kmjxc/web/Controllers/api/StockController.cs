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
    public class StockController : ApiController
    {
        [HttpPost]
        public ApiMessage EnterStockFromBuy()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            ApiMessage message = new ApiMessage();

            int buy_id = 0;
            string stockInfo = request["stocks"];
            int updateStock = 0;
            int shouseId = 0;

            int.TryParse(request["buy_id"], out buy_id);
            int.TryParse(request["update_stock"], out updateStock);
            int.TryParse(request["house_id"], out shouseId);
            try
            {
                BEnterStock stock = new BEnterStock();
                stock.BuyID = buy_id;
                stock.EnterTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                stock.StoreHouse = new Store_House() { Shop_ID = stockManager.Shop.Shop_ID, StoreHouse_ID = shouseId };
                if (updateStock == 1)
                {
                    stock.UpdateStock = true;
                }
                if (!string.IsNullOrEmpty(stockInfo))
                {
                    stock.Details = new List<BEnterStockDetail>();
                    string[] ss = stockInfo.Split(';');
                    foreach (string s in ss)
                    {
                        string[] sss = s.Split(',');
                        foreach (string ssss in sss)
                        {
                            BEnterStockDetail detail = new BEnterStockDetail();
                            detail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            detail.Quantity = int.Parse(ssss.Split(':')[1]);
                            detail.Price = double.Parse(ssss.Split(':')[2]);
                            detail.Product = new BProduct() { ID = int.Parse(ssss.Split(':')[0]) };
                            stock.Details.Add(detail);
                        }
                    }
                }

                stockManager.CreateEnterStock(stock);
                message.Status = "ok";
                message.Message ="";
            }
            catch (JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public PQGridData GetStoreHouses()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);

            List<BStoreHouse> houses = stockManager.GetStoreHouses();
            data.totalRecords = houses.Count;
            data.data = houses;
            data.curPage = 1;
            return data;
        }

        [HttpPost]
        public PQGridData SearchProductsStore()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int page = 1;
            int pageSize = 30;
            int category = 0;
            int storeHouse = 0;

            int.TryParse(request["cid"],out category);
            int.TryParse(request["house"],out storeHouse);
            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);
            string keyword = request["keyword"];
            int total = 0;
            data.data=stockManager.SearchProductStocks(null, category, storeHouse, keyword, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }
    }
}