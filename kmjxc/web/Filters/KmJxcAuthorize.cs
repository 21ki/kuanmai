using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace KM.JXC.Web.Filters
{
    public class KmJxcAuthorize : AuthorizeAttribute 
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //filterContext.HttpContext.Response.Write("AuthorizeAttribute");
            base.OnAuthorization(filterContext);
        }
    }
}