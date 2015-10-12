using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.BL;
using KMBit.Beans;
namespace KMBit.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Charge()
        {
            //BaseManagement baseMgr = new BaseManagement(0);
            //List<BTaocan> taocans = baseMgr.FindBTaocans();
            //int total = taocans.Count();
            //PageItemsResult<BTaocan> result = new PageItemsResult<BTaocan>() { CurrentPage = 1, Items = taocans, PageSize = total, TotalRecords = total };
            //KMBit.Grids.KMGrid<BTaocan> grid = new KMBit.Grids.KMGrid<BTaocan>(result);
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}