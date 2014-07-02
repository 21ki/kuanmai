using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KM.JXC.Web.Controllers
{
    public class AdminDataController : Controller
    {
        //
        // GET: /AdminData/

        public ActionResult Shop()
        {
            return View();
        }

        public ActionResult Customer()
        {
            return View();
        }

        public ActionResult Sales()
        {
            return View();
        }

    }
}
