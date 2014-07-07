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
    public class ShopController : Controller
    {
        //
        // GET: /Shop/

        public ActionResult Account()
        {
            return View();
        }

        public ActionResult Log()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserActionLogManager logManager = new UserActionLogManager(new BUser() { ID=int.Parse(user_id) });
            int page = 0;
            int pageSize = 30;
            int total = 0;
            int userid = 0;
            int action_id = 0;
            long stime = 0;
            long etime = 0;

            int.TryParse(Request["page"],out page);
            int.TryParse(Request["pageSize"], out pageSize);
            int.TryParse(Request["log_user"], out userid);
            int.TryParse(Request["log_action"], out action_id);

            if (!string.IsNullOrEmpty(Request["log_startdate"]))
            {
                DateTime tmp = DateTime.MinValue;
                DateTime.TryParse(Request["log_startdate"],out tmp);
                if (tmp != DateTime.MinValue)
                {
                    stime = DateTimeUtil.ConvertDateTimeToInt(tmp);
                }
            }

            if (!string.IsNullOrEmpty(Request["log_enddate"]))
            {
                DateTime tmp = DateTime.MinValue;
                DateTime.TryParse(Request["log_enddate"], out tmp);
                if (tmp != DateTime.MinValue)
                {
                    etime = DateTimeUtil.ConvertDateTimeToInt(tmp);
                }
            }

            if(page<=0)
            {
                page=1;
            }
            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            List<BUserActionLog> logs= logManager.SearchUserActionLog(userid, action_id,stime,etime,page, pageSize, out total);
            BPageData data = new BPageData();
            data.Data = logs;
            data.TotalRecords = total;
            data.Page = page;
            data.PageSize = pageSize;
            data.URL = Request.RawUrl;

            List<BUserAction> actions = logManager.GetActions();
            ViewData["action_list"] = actions;

            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            int totalUser=0;
            List<BUser> users = userMgr.GetUsers(1, 1, out totalUser, 0, false);
            ViewData["user_list"] = users;
            return View(data);
        }

        public ActionResult Dashboard()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            BShopStatistic statistic = shopManager.GetShopStatistic(0, true);
            return View(statistic);
        }

        public ActionResult Product()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;           
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
            int total = 0;
            int page = 1;
            int pageSize = 30;
            bool? connected = null;
            int.TryParse(Request["page"],out page);
            int.TryParse(Request["pageSize"], out pageSize);
            string keyword = Request["txt_product_name"];
            int shop = 0;
            int.TryParse(Request["txt_product_shop"],out shop);
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            if (Request["connected"] != null)
            {
                if (Request["connected"] == "1")
                {
                    connected = true;
                }
                else if (Request["connected"] == "0")
                {
                    connected = false;
                }
            }
            BMallSync lastSync = shopManager.GetMallSync(0, 0);
            List<BMallProduct> products = shopManager.SearchOnSaleMallProducts(keyword, page, pageSize, out total, connected, shop);
            BPageData data = new BPageData();
            data.Data = products;
            data.TotalRecords = total;
            data.Page = page;
            data.PageSize = pageSize;
            data.URL = Request.RawUrl;
            ViewData["LastSync"] = lastSync;

            ViewData["ChildShop"] = shopManager.ChildShops;
            ViewData["CurrentShop"] = shopManager.Shop;
            return View(data);
        }

        public ActionResult Express()
        {
            return View();
        }
        public ActionResult Child()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;          
            ShopManager shopManager = new ShopManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);

            List<Mall_Type> mtypes = shopManager.GetMallTypes();
            ViewData["t"] = "";
            return View(mtypes);
        }

        public ActionResult ChildRequests()
        {
            return View();
        }
    }
}
