using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.BL.Models.Admin;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Admin;
namespace KM.JXC.Web.Controllers.api
{
    public class SystemAdminController : BaseApiController
    {
        [HttpPost]
        public ApiMessage SetCorpInfo()
        {
            ApiMessage message = new ApiMessage() { Status="ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            try
            {
                SystemAdmin admin = new SystemAdmin(int.Parse(user_id));
                Corp_Info info = new Corp_Info() { About=request["about"],Contact=request["contact"],Help=request["help"] };
                admin.SetCorpInfo(info);
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            return message;
        }

        [HttpPost]
        public ApiMessage VerifyPassword()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            int uid = 0;
            try
            {
                int.TryParse(request["uid"], out uid);
                SystemAdmin admin = new SystemAdmin(int.Parse(user_id));
                string password = request["password"];
                if (!admin.VerifyPassword(password, uid))
                {
                    message.Status = "failed";
                    message.Message = "旧密码输入错误";
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            return message;
        }

        [HttpPost]
        public ApiMessage UpdatePassword()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            int uid = 0;
            try
            {
                int.TryParse(request["uid"],out uid);
                SystemAdmin admin = new SystemAdmin(int.Parse(user_id));
                string password = request["password"];
                if (!admin.UpdatePassword(password,uid))
                {
                    message.Status = "failed";
                    message.Message = "密码修改失败";
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            return message;
        }
    }
}