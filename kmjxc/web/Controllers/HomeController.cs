using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KM.JXC.BL;
using KM.JXC.DBA;
using KM.JXC.Web.Filters;
namespace KM.JXC.Web.Controllers
{   
    [HandleError]
    public class HomeController : Controller
    {
        [AccessTokenValidation]
        public ActionResult Index()
        {
            string user = HttpContext.User.Identity.Name;
            return View();
        }        

        [AllowAnonymous]
        public ActionResult CallBack()
        {           
            string code = Request["code"];
            string mall_type_id=Request["mall"];

            int mall=0;

            if (!int.TryParse(mall_type_id, out mall)) {
                RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(code))
            {
                RedirectToAction("Login");
            }

            AccessTokenManager tokenManager = new AccessTokenManager(mall);
            Access_Token token = tokenManager.AuthorizationCallBack(code);
            FormsAuthentication.RedirectFromLoginPage(token.User_ID.ToString(), false);
            RedirectToAction("Index");
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
    }
}
