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
    public class AccessTokenValidation:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string user_id = filterContext.HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(user_id)) {
                filterContext.HttpContext.Response.Redirect("/Home/Login?message=登录信息过期，请重新登录");
            }
            //Verify if the cookie user is a valid user
            UserManager userMgr = new UserManager(int.Parse(user_id),null);
            BUser user = userMgr.CurrentUser;

            if (user == null)
            {
                filterContext.HttpContext.Response.Redirect("/Home/Login?message=登录信息丢失，请重新登录并授权");
            }

            //Verify if logon user already has access token in db
            KuanMaiEntities db = new KuanMaiEntities();
            
            Access_Token token = (from t in db.Access_Token where t.User_ID == user.ID && t.Mall_Type_ID == user.Type.ID select t).FirstOrDefault<Access_Token>();

            if (token == null) {
                filterContext.HttpContext.Response.Redirect("/Home/Login?message=没有授权信息，请登录并授权");
            }

            //Verify if the existed access token is expired          
            int timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            if (timeNow >= token.Request_Time + token.Expirse_In)
            {
                filterContext.HttpContext.Response.Redirect("/Home/Login?message=授权信息已经过期，请重新登录并授权");
            }
        }
    }
}