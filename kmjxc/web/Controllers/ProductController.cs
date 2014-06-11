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
       
        public ActionResult List()
        {
            return View();
        }

        public ActionResult Detail(string id)
        {
            if (id == null)
            {
                return Redirect("/Home/Error?message="+HttpUtility.UrlEncode("请输入正确的产品ID"));
            }

            int product_id = 0;
            int.TryParse(id, out product_id);
            if (product_id == 0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的产品ID"));
            }

            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            BProduct product = pdtManager.GetProductFullInfo(product_id);
            return View(product);
        }

        public ActionResult New()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BCategory> categories = cateMgr.GetCategories(0);
            ViewData["category"] = categories;
            return View();
        }

        public ActionResult NewStock()
        {
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BCategory> categories = cateMgr.GetCategories(0);
            ViewData["category"] = categories;
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
            string user_id = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BCategory> categories = cateMgr.GetCategories(0);
            List<BProperty> properties = cateMgr.GetProperties(0);
            ViewData["category"] = categories;
            ViewData["mproperty"] = properties;
            return View();
        }

        public ActionResult NewProperty()
        {
            return View();
        }
    }
}
