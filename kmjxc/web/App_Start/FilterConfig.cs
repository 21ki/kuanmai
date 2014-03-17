using System.Web;
using System.Web.Mvc;

using KM.JXC.Web.Filters;

namespace KM.JXC.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new KmJxcAuthorize());
            filters.Add(new AccessTokenValidation());
        }
    }
}