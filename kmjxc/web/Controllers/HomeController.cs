﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Web.Filters;
using KM.JXC.Web.Models;
namespace KM.JXC.Web.Controllers
{   
    [HandleError]
    public class HomeController : Controller
    {
        [AccessTokenValidation]
        public ActionResult Index()
        {
            string user = HttpContext.User.Identity.Name;
            //UserActionLogManager logManager = new UserActionLogManager(new BUser() { ID=int.Parse(user) });
           
            //logManager.CreateActionLog(new BUserActionLog() { Action = new BUserAction() { Action_ID=UserLogAction.USER_LOGIN }, Description="用户登录" });
            return Redirect("/Shop/Dashboard");    
            
        }        

        [AllowAnonymous]
        public ActionResult CallBack()
        {           
            string code = Request["code"];
            string mall_type_id=Request["mall"];

            int mall=0;

            if (!int.TryParse(mall_type_id, out mall)) {
                return RedirectToAction("Login", new { message = "商城类型丢失,请不要随意更改URL" });
            }

            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Login", new { message = "商城授权码丢失，请不要随意更改URL" });
            }

            AccessManager accessManager = new AccessManager(mall);
            Access_Token token = null;
            try
            {
                token = accessManager.AuthorizationCallBack(code);
            }
            catch (KM.JXC.Common.KMException.KMJXCException ex)
            {
                if (ex.Level == Common.KMException.ExceptionLevel.ERROR)
                {
                    return RedirectToAction("Login", new { message = ex.Message });
                }
            }
            catch (Exception bex)
            {
                return RedirectToAction("Login", new { message = "未知错误，请重新授权" });
            }

            if (token == null)
            {
                return RedirectToAction("Login", new { message = "授权失败，请重新授权" });
            }

            FormsAuthentication.RedirectFromLoginPage(token.User_ID.ToString(), false);
            
            return Redirect("/Shop/Dashboard");            
        }

        [AllowAnonymous]
        public ActionResult Login(string message)
        {
            CommonManager sysMgr = new CommonManager();
            List<Open_Key> keys = sysMgr.GetOpenKeys();
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["message"] = message;
            }
            return View(keys);
        }

        public ActionResult Error()
        {
            string message = "";
            if (!string.IsNullOrEmpty(Request["message"]))
            {
                message=Request["message"];
            }

            ViewData["message"] = message;
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public JsonResult SetTheme()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            JsonResult result = new JsonResult();
            string theme = "smoothness";
            if (!string.IsNullOrEmpty(Request["theme"]))
            {
                theme = Request["theme"];
            }

            string themePath = Request.PhysicalApplicationPath + "\\Third\\jqueryui\\1.11.0\\" + theme;
            if (!System.IO.Directory.Exists(themePath))
            {
                theme = "smoothness";
            }

            Session["theme"] = theme;
            result.Data = message;
            return result;
        }
    }
}
