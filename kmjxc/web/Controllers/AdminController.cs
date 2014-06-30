using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KM.JXC.Web.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            string type = Request["type"];
            if (string.IsNullOrEmpty(type))
            {
                return View();
            }
            else
            {
                if (type.ToLower() == "do")
                {

                }
                else
                {
                    RedirectToAction("Login");
                }
                return RedirectToAction("Index");
            }
        }
    }
}
