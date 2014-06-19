using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.BL.Models.Admin;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.Web;

namespace KM.JXC.Web.Controllers
{
    
    public class CorpController : Controller
    {
        [AllowAnonymous]
        public ActionResult About()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Help()
        {
            return View();
        }

        public ActionResult Bug()
        {
            string user_id = HttpContext.User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            BPageData data = new BPageData();
            int page = 0;
            int pageSize = 0;
            int u_id = 0;
            int feature_id = 0;
            int total = 0;
            int status_id = 0;

            int.TryParse(Request["page"], out page);
            int.TryParse(Request["pageSize"], out pageSize);
            int.TryParse(Request["status"], out status_id);
            int.TryParse(Request["feature"], out feature_id);
            int.TryParse(Request["user"], out u_id);

            if (pageSize <= 0)
            {
                pageSize = 20;
            }

            if (page <= 0)
            {
                page = 1;
            }
            data.Data = bugManager.SearchBugs(u_id, feature_id, status_id, page, pageSize, out total);
            data.Page = page;
            data.TotalRecords = total;
            data.PageSize = pageSize;
            data.URL = Request.RawUrl;
            ViewData["CurrentUser"] = bugManager.CurrentUser;
            return View(data);
        }

        public ActionResult BugDetail(string id)
        {
            string user_id = HttpContext.User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));

            int bug_id = 0;
            int.TryParse(id,out bug_id);
            if (bug_id == 0)
            {
                Redirect("/Home/Error?message=问题编号不能为空");
            }

            BBug bug=bugManager.GetBugInfo(bug_id);
            return View(bug);
        }

        public ActionResult Blog()
        {
            return View();
        }

    }
}
