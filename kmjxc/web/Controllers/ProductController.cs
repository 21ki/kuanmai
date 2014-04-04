using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.Web;
namespace KM.JXC.Web.Controllers
{
    public class ProductController : Controller
    {
        //
        // GET: /Product/

        public ActionResult Search()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        public ActionResult Categories()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);

            List<BCategory> categories = cateMgr.GetCategories(0);

            return View(categories);
        }
        public ActionResult NewCategory()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BCategory> categories = cateMgr.GetCategories(0);
            return View((from cate in categories where cate.Parent==null || cate.Parent.ID == 0 select cate).ToList<BCategory>());           
        }
        public ActionResult Properties()
        {
           
            return View();
        }

        public ActionResult NewProperty()
        {
            return View();
        }
    }
}
