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
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Login");
            }

            AccessManager tokenManager = new AccessManager(mall);
            Access_Token token = null;
            try
            {
                token = tokenManager.AuthorizationCallBack(code);
            }
            catch (KM.JXC.Common.KMException.KMJXCException ex)
            {
                if (ex.Level == Common.KMException.ExceptionLevel.ERROR)
                {
                    return RedirectToAction("Login", new { message=ex.Message});
                }
            }

            if (token == null)
            {
                return RedirectToAction("Login", new { message = "授权失败，请重新授权" });
            }

            FormsAuthentication.RedirectFromLoginPage(token.User_ID.ToString(), false);
            return RedirectToAction("Index");
            //return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string message)
        {
            SystemManager sysMgr = new SystemManager();
            List<Open_Key> keys = sysMgr.GetOpenKeys();
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["message"] = message;
            }
            return View(keys);
        }
    }
}
