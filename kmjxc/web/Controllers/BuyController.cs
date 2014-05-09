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

        public ActionResult Buy()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }

        public ActionResult Suppliers()
        {
            return View();
        }

        public ActionResult BuyOrderDetail(int id)
        {
            return View();
        }

        public ActionResult BuyDetail(int id)
        {
            return View();
        }
    }
}
