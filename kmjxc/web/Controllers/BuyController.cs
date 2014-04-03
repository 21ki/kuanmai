using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KM.JXC.Web.Controllers
{
    public class BuyController : Controller
    {
        //
        // GET: /Buy/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewOrder()
        {
            return View();
        }
    }
}
