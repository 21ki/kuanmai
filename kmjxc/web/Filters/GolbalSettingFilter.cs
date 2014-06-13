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
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;

            if (user == null)
            {
                filterContext.HttpContext.Response.Redirect("/Home/Login?message=登录信息丢失，请重新登录并授权");
            }

            filterContext.Controller.ViewData["CurrentShop"] = userMgr.Shop;
            filterContext.Controller.ViewData["MainShop"] = userMgr.Main_Shop;
            filterContext.Controller.ViewData["ChildShop"] = userMgr.ChildShops;
            filterContext.Controller.ViewData["CurrentPermission"] = userMgr.CurrentUserPermission;
            filterContext.Controller.ViewData["CurrentUser"] = userMgr.CurrentUser;
        }
    }
}
