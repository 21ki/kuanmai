using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;

namespace KM.JXC.Web.Controllers
{
    public class StockController : Controller
    {      

        public ActionResult Back()
        {
            return View();
        }
        public ActionResult Enter()
        {
            return View();
        }
        public ActionResult Search()
        {
            return View();
        }
        public ActionResult Waste()
        {
            return View();
        }
        public ActionResult Leave()
        {
            return View();
        }
        public ActionResult Stores()
        {
            return View();
        }

        public ActionResult StockDetail(int id)
        {
            return View();
        }

        public ActionResult EnterDetail(int id)
        {
            string uid = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(uid), null);
            BUser user = userMgr.CurrentUser;
            StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BEnterStock stock = stockManager.GetEnterStockFullInfo(id);
            return View(stock);
        }
    }
}
