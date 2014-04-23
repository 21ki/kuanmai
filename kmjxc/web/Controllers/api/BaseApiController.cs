using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;
namespace KM.JXC.Web.Controllers.api
{
    public class BaseApiController : ApiController, IActionFilter
    {
        protected HttpRequestBase HttpRequest { get; set; }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.HttpRequest = filterContext.HttpContext.Request;
        }
    }
}
