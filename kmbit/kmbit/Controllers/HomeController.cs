using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.Beans;
using KMBit.DAL;
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
            SiteManagement siteMgr = new SiteManagement(0);
            Help_Info info = siteMgr.GetHelpInfo();
            return View(info);
        }      

        public ActionResult Contact()
        {
            SiteManagement siteMgr = new SiteManagement(0);
            Help_Info info = siteMgr.GetHelpInfo();
            return View(info);
        }
    }
}