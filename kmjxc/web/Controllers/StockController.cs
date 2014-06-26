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
        public ActionResult Wastage()
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

        public ActionResult LeaveDetail(int id)
        {           
            return View();
        }

        public ActionResult EnterDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的入库单编号"));
            }

            int eId = 0;
            int.TryParse(id,out eId);

            if (eId <= 0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的入库单编号"));
            }
            string uid = HttpContext.User.Identity.Name;
            BEnterStock stock = null;
            UserManager userMgr = new UserManager(int.Parse(uid), null);
            BUser user = userMgr.CurrentUser;
            try
            {
                StockManager stockManager = new StockManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
                stock = stockManager.GetEnterStockFullInfo(eId);
            }
            catch (KMJXCException kex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode(kex.Message));
            }
            catch
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("未知错误"));
            }
            return View(stock);
        }
    }
}
