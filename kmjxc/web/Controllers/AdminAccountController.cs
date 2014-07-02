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
    public class AdminAccountController : Controller
    {
        //
        // GET: /AdminAccount/

        public ActionResult Info()
        {
            string user = HttpContext.User.Identity.Name;
            SystemAdmin admim = new SystemAdmin(int.Parse(user));
            return View(admim.CurrentUser);
        }

        public ActionResult Password()
        {
            string user = HttpContext.User.Identity.Name;
            SystemAdmin admim = new SystemAdmin(int.Parse(user));
            return View(admim.CurrentUser);
        }

        public ActionResult Permission()
        {
            return View();
        }

    }
}
