using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.Web;

namespace KM.JXC.Web.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult Sale()
        {
            return View();
        }

        public ActionResult Buy()
        {
            return View();
        }
    }
}
