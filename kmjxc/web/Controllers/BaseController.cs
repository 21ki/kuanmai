using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KM.JXC.DBA;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
using KM.JXC.Common.Util;

namespace KM.JXC.Web.Controllers
{
    public class BaseController : Controller
    {
        public Access_Token token { get; private set; }

        public User CurrentLoginUser { get; private set; }        

        public bool IsLogin { get; private set; }

        public bool AccessTokenExpired { get; set; }

        public BaseController()
        {
            string user = HttpContext.User.Identity.Name;
        }

        protected void CheckAuthorization()
        {
            if (Session["loginuser"] == null)
            {
                AccessTokenExpired = true;
                IsLogin = false;
                return;
            }

            KM.JXC.DBA.User loginUser = (KM.JXC.DBA.User)Session["loginuser"];
            if (loginUser != null && loginUser.User_ID > 0)
            {
                this.CurrentLoginUser = loginUser;
                this.IsLogin = true;
                if (Session["token"] != null)
                {
                    token = (Access_Token)Session["token"];
                }
                int timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                if (token != null)
                {
                    if (token.Request_Time + token.Expirse_In > timeNow)
                    {
                        AccessTokenExpired = false;
                    }
                    else
                    {
                        AccessTokenExpired = true;
                    }
                }
                else
                {
                    AccessTokenExpired = true;
                }
            }
            else {
                this.IsLogin = false;
            }
        }
    }
}
