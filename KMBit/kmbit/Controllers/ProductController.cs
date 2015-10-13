using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.Beans;
using KMBit.Models;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.Util;
namespace KMBit.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        [HttpGet]
        public ActionResult Charge()
        {
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public ActionResult Charge(DirectChargeModel model)
        {
            if (ModelState.IsValid)
            {
                ChargeBridge cb = new ChargeBridge();
                ChargeOrder order = new ChargeOrder() { AgencyId = 0, Id = 0, MobileNumber = model.Mobile, OutId = "", ResourceId = 0, ResourceTaocanId = model.ResourceTaocanId, RouteId = 0, CreatedTime=DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                //
                OrderManagement orderMgt = new OrderManagement();
                order = orderMgt.GenerateOrder(order);
                //Redirct to the payment page.
                //TBD
                //After the payment is done then process below steps
                ChargeResult result = cb.Charge(order);
                ViewBag.Message = result.Message;
            }

            return View();
        }

        public ActionResult Agent()
        {
            return View();
        }

        public ActionResult Code()
        {
            return View();
        }
    }
}