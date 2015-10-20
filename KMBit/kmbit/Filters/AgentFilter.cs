using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace KMBit.Filters
{
    public class AgentFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public string Message { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string username = HttpContext.Current.User.Identity.Name;
            KMBit.BL.BaseManagement baseMgt = new BL.BaseManagement(username);
            if (baseMgt.CurrentLoginUser.IsAdmin)
            {
                HttpContext.Current.Response.Redirect("/Account/LoginError?message=" + this.Message);
            }
        }
    }
}