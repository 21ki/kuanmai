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
    public class StockController : BaseApiController
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

            int[] buy_ids =null;
            int updateStock = 0;
            int shouseId = 0;

            buy_ids = base.ConvertToIntArrar(request["buy_ids"]);
            int.TryParse(request["update_stock"], out updateStock);
            int.TryParse(request["house_id"], out shouseId);
            try
            {
                if (buy_ids != null)
                {
                    foreach (int buy_id in buy_ids)
                    {
                        BEnterStock stock = new BEnterStock();
                        stock.BuyID = buy_id;
                        stock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        stock.StoreHouse = new BStoreHouse() { ID = shouseId, Shop = new BShop() { ID = stockManager.Shop.Shop_ID } };
                        if (updateStock == 1)
                        {
                            stock.UpdateStock = true;
                        }

                        stockManager.CreateEnterStock(stock);
                    }
                }

                
                message.Status = "ok";
                message.Message = "";
            }
            catch (JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
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
        public ApiMessage CreateStoreHouse()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);

            try
            {
                BStoreHouse house = new BStoreHouse();
                house.Name=request["name"];
                house.Address=request["address"];
                house.Phone=request["phone"];
                house.IsDefault = false;
                if (!string.IsNullOrEmpty(request["isdefault"]))
                {
                    if (request["isdefault"] == "1")
                    {
                        house.IsDefault = true;
                    }                 
                }
                stockManager.CreateStoreHouse(house);
                message.Status = "ok";
                message.Message = "创建成功";
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }
            return message;
        }

        [HttpPost]
        public ApiMessage UpdateStoreHouse()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);

            try
            {
                BStoreHouse house = new BStoreHouse();
                house.ID = int.Parse(request["id"]);
                house.Name = request["name"];
                house.Address = request["address"];
                house.Phone = request["phone"];
                house.IsDefault = false;
                if (!string.IsNullOrEmpty(request["isdefault"]))
                {
                    if (request["isdefault"] == "1")
                    {
                        house.IsDefault = true;
                    }
                }
                stockManager.UpdateStoreHouse(house);
                message.Status = "ok";
                message.Message = "修改成功";
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }
            return message;
        }

        [HttpPost]
        public List<BProduct> GetProductStockDetails()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int product_id = 0;

            int.TryParse(request["product_id"],out product_id);
            List<BProduct> stores = stockManager.GetProductStockDetails(product_id);
            return stores;
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

        [HttpPost]
        public PQGridData SearchEnterStock()
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
          

            int.TryParse(request["cid"], out category);
            int.TryParse(request["house"], out storeHouse);
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            string keyword = request["keyword"];
            int total = 0;
            data.data = stockManager.SearchEnterStocks(0,0,0,0,0,0,storeHouse,page,pageSize,out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public PQGridData SearchLeaveStock()
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
            int sale_id = 0;
            int leave_id=0;
            int uid = 0;
            long stime = 0;
            long etime = 0;
            if (!string.IsNullOrEmpty(request["sdate"]))
            {
                DateTime sdate = DateTime.MinValue;
                DateTime.TryParse(request["sdate"],out sdate);
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
            int.TryParse(request["leave_id"], out leave_id);
            int.TryParse(request["user_id"], out uid);
            int total = 0;

            int[] leave_ids = null;
            int[] user_ids = null;
            string[] sale_ids = null;
            if (leave_id > 0)
            {
                leave_ids = new int[] { leave_id };
            }
            if (uid > 0)
            {
                user_ids = new int[] { uid };
            }

            if (sale_id>0)
            {
                sale_ids = new string[] { sale_id.ToString() };
            }

            data.data = stockManager.SearchLeaveStocks(leave_ids, sale_ids, user_ids, stime, etime, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public PQGridData SearchBackStock()
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
            int sale_id = 0;
            int back_id = 0;
            int uid = 0;
            long stime = 0;
            long etime = 0;
            int storeHouseId = 0;
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
                backids = new int[] { back_id};
            }
            int[] userids = null;
            if (uid > 0)
            {
                userids = new int[] { uid};
            }
            string[] saleids = null;
            if (sale_id > 0)
            {
                saleids = new string[] { sale_id.ToString() };
            }
            data.data = stockManager.SearchBackStockDetails(backids, saleids, userids,storeHouseId, stime, etime, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public ApiMessage UpdateEnterStockToProductStock()
        {
            ApiMessage message = new ApiMessage() { Status="ok",Message="更新成功"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int enter_id = 0;
            int.TryParse(request["enter_id"],out enter_id);
            try
            {
                bool result = stockManager.UpdateProductStockByEnterStock(enter_id);
                if (!result)
                {
                    message.Status = "failed";
                    message.Message = "更新库存失败";
                }
                else
                {
                    message.Status = "ok";
                    message.Message = "成功更新库存";
                }
            }
            catch (JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误:"+ex.Message;
            }
            return message;
        }

        [HttpPost]
        public PQGridData SearchProductWastage()
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
            int? category_id = null;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            string keyword = request["keyword"];
            int[] sids = null;
            string suppliers = request["suppliers"];
            sids = base.ConvertToIntArrar(suppliers);
            if (request["cid"] != null && request["cid"].ToString() != "" && request["cid"].ToString() != "0")
            {
                int cid = 0;
                int.TryParse(request["cid"], out cid);
                if (cid > 0)
                {
                    category_id = cid;
                }
            }
            int total = 0;
            data.data = stockManager.SearchProductWastage(sids, keyword, category_id, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public ApiMessage UpdateProductsStocks()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);

            return message;
        }
    }
}