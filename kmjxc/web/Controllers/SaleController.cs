using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Web.Filters;
using KM.JXC.Common.Util;
namespace KM.JXC.Web.Controllers
{
    public class SaleController : Controller
    {
        //
        // GET: /Sale/

        public ActionResult Search()
        {
            string sCreated = Request["trade_sdate"];          
            string sHours = Request["trade_sdate_hour"];
            string sMinutes = Request["trade_sdate_minute"];
            string eCreated = Request["trade_edate"];
            string eHours = Request["trade_edate_hour"];
            string eMinutes = Request["trade_edate_minute"];
            string productName=Request["pdt_name"];
            string buyer_nick = Request["buyer_nick"];
            int shop = 0;
            int page = 1;
            int pageSize = 30;

            int.TryParse(Request["page"],out page);
            int.TryParse(Request["pagesize"], out pageSize);
            int.TryParse(Request["trade_shop"],out shop);
            if(page<=0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;

            if (!string.IsNullOrEmpty(sCreated) && !string.IsNullOrEmpty(sHours) && !string.IsNullOrEmpty(sMinutes))
            {
                sDate = Convert.ToDateTime(sCreated);
                int h = 0;
                int m = 0;
                int.TryParse(sHours, out h);
                int.TryParse(sMinutes, out m);
                sDate = new DateTime(sDate.Year, sDate.Month, sDate.Day, h, m,0);
            }

            if (!string.IsNullOrEmpty(eCreated) && !string.IsNullOrEmpty(eHours) && !string.IsNullOrEmpty(eMinutes))
            {
                sDate = Convert.ToDateTime(eCreated);
                int h = 0;
                int m = 0;
                int.TryParse(eHours, out h);
                int.TryParse(eMinutes, out m);
                sDate = new DateTime(sDate.Year, sDate.Month, sDate.Day, h, m, 0);
            }
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            SalesManager saleManager = new SalesManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int total = 0;
            long sTime = 0;
            long eTime = 0;
            if (sDate != DateTime.MinValue)
            {
                sTime = DateTimeUtil.ConvertDateTimeToInt(sDate);
            }
            if (eDate != DateTime.MinValue)
            {
                eTime = DateTimeUtil.ConvertDateTimeToInt(eDate);
            }
            List<BSale> sales = saleManager.SearchSales(null, productName, null, buyer_nick, sTime, eTime, page, pageSize, out total,shop);
            BPageData data = new BPageData();
            data.Data = sales;
            data.TotalRecords = total;
            data.Page = page;
            data.PageSize = pageSize;
            data.URL = Request.RawUrl;
            List<BShop> childShops = shopManager.SearchChildShops();
            ViewData["ChildShop"] = childShops;
            ViewData["CurrentShop"] = userMgr.Shop;
            return View(data);
        }

        public ActionResult Back()
        {
            return View();
        }
        public ActionResult Change()
        {
            return View();
        }
        public ActionResult Customer()
        {
            return View();
        }

        public ActionResult Sync()
        {
            return View();
        }
    }
}
