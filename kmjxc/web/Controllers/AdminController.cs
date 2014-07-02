using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KM.JXC.BL.Admin;
using KM.JXC.Common.KMException;
namespace KM.JXC.Web.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            string user = HttpContext.User.Identity.Name;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            string type = Request["type"];
            if (!string.IsNullOrEmpty(Request["message"]))
            {
                ViewData["message"] = Request["message"];
            }
            if (string.IsNullOrEmpty(type))
            {
                return View();
            }
            else
            {
                if (type.ToLower() == "do")
                {
                    try
                    {
                        SystemAdmin instance=SystemAdmin.Login(Request["sysUser"], Request["sysPass"]);
                        FormsAuthentication.RedirectFromLoginPage(instance.CurrentUser.ID.ToString(), false);
                        return Redirect("/AdminAccount/Info");
                    }
                    catch (KMJXCException kex)
                    {                        
                        return Redirect("Login?message="+kex.Message);
                    }
                }
                else
                {
                    RedirectToAction("Login");
                }
            }

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult CorpInfo()
        {
            return View();
        }
    }
}
