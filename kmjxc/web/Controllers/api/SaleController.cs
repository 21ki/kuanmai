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
            long stime = 0;
            long etime = 0;
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
            string[] saleids = null;
            if (sale_id > 0)
            {
                saleids = new string[] { sale_id.ToString() };
            }
            data.data = saleManager.SearchBackSales(saleids, userids, stime, etime, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public PQGridData SearchBackSaleDetails()
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
            long stime = 0;
            long etime = 0;
            int? status = null;
            int status1 = 0;
            string state=request["status"];
            if (!string.IsNullOrEmpty(state))
            {
                int.TryParse(state, out status1);
                status = status1;
            }

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
            string[] saleids = null;
            if (sale_id > 0)
            {
                saleids = new string[] { sale_id.ToString() };
            }
            data.data = saleManager.SearchBackSaleDetails(saleids, userids,status, stime, etime, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public PQGridData SearchSales()
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
            long stime = 0;
            long etime = 0;
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
            int total = 0;
            data.data = saleManager.SearchSales(null,null,null,null, null,null, stime, etime, page, pageSize, out total);
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public ApiMessage SyncMallTrades()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int start = 0;
            int end = 0;
            int syncType = 0;
            int shop = 0;
            string status = request["status"];
            int.TryParse(request["start"],out start);
            int.TryParse(request["end"], out end);
            int.TryParse(request["syncType"],out syncType);
            int.TryParse(request["shop"], out shop);
            try
            {
                saleManager.SyncMallTrades(start, end, status, syncType, shop);
                message.Status = "ok";
            }
            catch (KMJXCMallException mex)
            {
                message.Status = "failed";
                message.Message = mex.Message;
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
        public BSyncTime GetLastSyncTime()
        {
            BSyncTime time = new BSyncTime();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;          
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);

            time = saleManager.GetSyncTime();
            return time;
        }

        [HttpPost]
        public PQGridData SearchTradeSyncLog()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int page = 1;
            int.TryParse(request["page"],out page);
            if (page == 0)
            {
                page = 1;
            }
            int pageSize = 20;
            int.TryParse(request["pageSize"],out pageSize);
            if (pageSize == 0)
            {
                pageSize = 20;
            }
            int total = 0;

            data.data = saleManager.SearchTradeSyncLog(0, 0, 0, null, page, pageSize, out total);
            data.totalRecords = total;
            data.curPage = page;
            return data;
        }

        [HttpPost]
        public ApiMessage HandleBackSaleDetail()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            string backSales = request["back_sales"];
            int status=0;                
            int.TryParse(request["status"],out status);
           
            if (string.IsNullOrEmpty(backSales))
            {
                message.Status = "failed";
                message.Message = "没有任何退货信息";
                return message;
            }

            backSales = HttpUtility.UrlDecode(backSales);

            try
            {
                JArray jsons = JArray.Parse(backSales);
                if (jsons != null && jsons.Count > 0)
                {
                    for (int i = 0; i < jsons.Count; i++)
                    {
                        JObject json = (JObject)jsons[i];
                        int back_Sale_ID = (int)json["back_sale_id"];
                        JArray orders=(JArray)json["orders"];
                        List<BOrder> bOrders = new List<BOrder>();
                        for(int j = 0; j < orders.Count; j++)
                        {
                            JObject o=(JObject)orders[j];
                            string order_id=(string)o["order_id"];
                            int quantity = (int)o["quantity"];
                            bOrders.Add(new BOrder() {  Order_ID=order_id,Quantity=quantity});
                        }

                        switch (status)
                        {
                            case 1:
                                saleManager.HandleBackSaleDetail_BackStock(back_Sale_ID, bOrders, status);
                                break;
                            case 2:
                                saleManager.HandleBackSaleDetail_PartialWaste(back_Sale_ID, bOrders, status);
                                break;
                            case 3:
                                saleManager.HandleWastageBackSale_TotalWaste(back_Sale_ID, bOrders, status);
                                break;
                            default:
                                break;
                        }
                    }
                }
                message.Status = "ok";
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
    }
}
