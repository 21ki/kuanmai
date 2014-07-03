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

namespace KM.JXC.Web.Controllers.api
{
    public class BugController : BaseApiController
    {
        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ApiMessage CreateBug()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            int feature=0;
            int.TryParse(request["feature"],out feature);
            try
            {
                BBug newBug = new BBug();
                newBug.Created_By = bugManager.CurrentUser;
                newBug.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                newBug.Modified_By = bugManager.CurrentUser;
                newBug.Modified = newBug.Created;
                newBug.Status = new BBugStatus() {  ID=1};
                newBug.Title = HttpUtility.HtmlDecode(request["title"]);
                newBug.Description = HttpUtility.HtmlDecode(request["description"]);
                newBug.Feature = new BBugFeature() { ID = feature };
               
                bool result = bugManager.CreateNewBug(newBug);
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }

            return message;
        }

        [HttpPost]
        public ApiMessage CreateBugResponse()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            try
            {
                int bugId = 0;
                int.TryParse(request["bug_id"],out bugId);
                string comments = "";
                if (request["description"] != null)
                {
                    comments = request["description"];
                }
                bugManager.CreateNewResponse(bugId,comments);
                
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }
            return message;
        }

        [HttpPost]
        public List<BBugStatus> GetBugStatuses()
        {
            List<BBugStatus> statuses = new List<BBugStatus>();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            try
            {
                statuses = bugManager.GetBugStatuses();
            }
            catch (KMJXCException kex)
            {
               
            }
            catch (Exception ex)
            {
                
            }
            return statuses;
        }

        [HttpPost]
        public List<BBugFeature> GetBugFeatures()
        {
            List<BBugFeature> statuses = new List<BBugFeature>();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            try
            {
                statuses = bugManager.GetBugFeatures();
            }
            catch (KMJXCException kex)
            {

            }
            catch (Exception ex)
            {

            }
            return statuses;
        }

        [HttpPost]
        public PQGridData SearchBugs()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));

            int page = 0;
            int pageSize = 0;
            int u_id = 0;
            int feature_id = 0;
            int total = 0;
            int status_id=0;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);
            int.TryParse(request["status"],out status_id);
            int.TryParse(request["feature"],out feature_id);
            int.TryParse(request["user"],out u_id);

            if (pageSize <= 0)
            {
                pageSize = 20;
            }

            if (page <= 0)
            {
                page = 1;
            }
            data.data = bugManager.SearchBugs(u_id, feature_id, status_id, page, pageSize, out total);
            data.curPage = page;
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public ApiMessage GetBugFullInfo()
        {
            BBug data = new BBug();
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            try
            {
                int bugId = 0;
                int.TryParse(request["bug_id"], out bugId);
                data = bugManager.GetBugInfo(bugId);
                message.Item = data;
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }

            return message;
        }

        [HttpPost]
        public ApiMessage UpdateStatus()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            BugManager bugManager = new BugManager(int.Parse(user_id));
            int bug_id = 0;
            int status = 0;
            try
            {
                int.TryParse(request["bug_id"],out bug_id);
                int.TryParse(request["status"],out status);
                if (bug_id == 0)
                {
                    message.Status = "failed";
                    message.Message = "没有问题编号数据";
                    return message;
                }

                if (status == 0)
                {
                    message.Status = "failed";
                    message.Message = "没有状态数据";
                    return message;
                }

                bugManager.UpdateStatus(bug_id,status);
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