using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KM.JXC.Web.Controllers
{
    public class PermissionController : Controller
    {
        //
        // GET: /Permission/

        public ActionResult Account()
        {
            return View();
        }
        public ActionResult Role()
        {
            return View();
        }
    }
}
