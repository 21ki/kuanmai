using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace KM.JXC.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi0",
            //    routeTemplate: "api/{controller}/{action}",
            //    defaults: new { }
            //);
            config.Routes.MapHttpRoute(
                name: "DefaultApi1",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
