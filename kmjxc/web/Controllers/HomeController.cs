using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KM.JXC.BL;

namespace KM.JXC.Web.Controllers
{   
    [HandleError]
    public class HomeController : BaseController
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            string user = HttpContext.User.Identity.Name;
            return View();
        }        

        [AllowAnonymous]
        public ActionResult CallBack()
        {
            UserManager userMgr = new UserManager();
            string code = Request["code"];
            string mall_id=Request["mall"];

            int mall=0;
            if (int.TryParse(mall_id,out mall))
            {
                if (!string.IsNullOrEmpty(code))
                {
                    userMgr.AuthorizationCallBack(code, mall);
                    FormsAuthentication.RedirectFromLoginPage(userMgr.CurrentUser.User_ID.ToString(), false);
                    RedirectToAction("Index");
                }
                else
                {
                    RedirectToAction("Login");
                }
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
    }
}
