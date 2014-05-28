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
            int stime = 0;
            int etime = 0;
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
            int stime = 0;
            int etime = 0;
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
            data.data = saleManager.SearchBackSaleDetails(saleids, userids, stime, etime, page, pageSize, out total);
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
            int stime = 0;
            int etime = 0;
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
            data.data = saleManager.SearchSales(null,null, null,null, stime, etime, page, pageSize, out total);
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
            string status = request["status"];
            int.TryParse(request["start"],out start);
            int.TryParse(request["end"], out end);
            int.TryParse(request["syncType"],out syncType);
            saleManager.SyncMallTrades(start, end, status, syncType);
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
            string backSaleIDs = request["back_sale_id"];
            int status=0;
            //int product_id = 0;
            string orderId = request["order_id"];            
            int.TryParse(request["status"],out status);
            //int.TryParse(request["product_id"],out product_id);
            if (string.IsNullOrEmpty(backSaleIDs) || string.IsNullOrEmpty(orderId))
            {
                message.Status = "failed";
                message.Message = "输入参数不正确";
                return message;
            }
            try
            {                
                int[] backSaleID=base.ConvertToIntArrar(backSaleIDs);
                string[] orders=orderId.Split(';');
                if (backSaleID.Length == orders.Length)
                {
                    for (int i = 0; i < backSaleID.Length; i++)
                    {
                        string[] os = orders[i].Split(',');
                        saleManager.HandleBackSaleDetail(backSaleID[i], os, status);
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
