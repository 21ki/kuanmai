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
    public class HomeController : Controller
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
            
            FormsAuthentication.RedirectFromLoginPage("TTTEE", false);
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
