using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KM.JXC.Web.Controllers
{
    public class AdminUserController : Controller
    {
        //
        // GET: /AdminUser/

        public ActionResult SysUser()
        {
            return View();
        }

        public ActionResult ShopUser()
        {
            return View();
        }

    }
}
