using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Common.Util;

namespace KM.JXC.Web.Filters
{
    public class GolbalSettingFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string user_id = filterContext.HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(user_id))
            {
                return;
            }
            //Verify if the cookie user is a valid user
            UserManagement userManagement = new UserManagement();
            BUser loginuser = userManagement.GetUserInfo(int.Parse(user_id));
            if (loginuser == null)
            {
                return;
            }

            if (!loginuser.IsSystemUser)
            {
                //normal user login
                UserManager userMgr = new UserManager(int.Parse(user_id), null);
                BUser user = userMgr.CurrentUser;
                ShopManager shopManager = new ShopManager(user, userMgr.Shop, userMgr.CurrentUserPermission, userMgr);
                filterContext.Controller.ViewData["CurrentShop"] = userMgr.Shop;
                filterContext.Controller.ViewData["MainShop"] = userMgr.Main_Shop;
                filterContext.Controller.ViewData["ChildShop"] = userMgr.ChildShops;
                filterContext.Controller.ViewData["CurrentPermission"] = userMgr.CurrentUserPermission;
                filterContext.Controller.ViewData["CurrentUser"] = userMgr.CurrentUser;
                filterContext.Controller.ViewData["SPStatistic"] = shopManager.GetShopStatistic(0, true);
            }
            else
            {
                //system user login
                //filterContext.HttpContext.Response.Redirect("/Admin/Index");
                filterContext.Controller.ViewData["CurrentUser"] = loginuser;
            }
        }
    }
}
