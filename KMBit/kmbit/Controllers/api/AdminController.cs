using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.BL.Agent;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.Controllers.api
{
    [Authorize]
    public class AdminController : BaseApiController
    {

        [AcceptVerbs("POST")]
        public ApiMessage GetAgencyResources()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentAdminMenagement agentMgtMgr = new AgentAdminMenagement(User.Identity.Name);
            int agencyId = 0;
            int.TryParse(request["agencyId"],out agencyId);
            if(agencyId>0)
            {
                List<BResource> rs = agentMgtMgr.FindAgentResources(agencyId);
                message.Status = "OK";
                message.Item = rs;
            }else
            {
                ResourceManagement resourceMgr = new ResourceManagement(agentMgtMgr.CurrentLoginUser);
                int total = 0;
                List<BResource> rs = resourceMgr.FindResources(0, null, 0, out total);
                message.Status = "OK";
                message.Item = rs;
            }
           
            return message;
        }

        [AcceptVerbs("POST")]
        public ApiMessage GetAgencyResourceTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentAdminMenagement agentMgtMgr = new AgentAdminMenagement(User.Identity.Name);
            int agencyId = 0;
            int resourceId = 0;
            int.TryParse(request["agencyId"], out agencyId);
            int.TryParse(request["resourceId"], out resourceId);
            if(agencyId==0 && resourceId==0)
            {
                message.Status = "ERROR";
                message.Item = null;
                message.Message = "代理商编号和资源编号都不能为空";
                return message;
            }
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            if(resourceId>0 && agencyId>0)
            {
                taocans = agentMgtMgr.FindAgencyResourceTaocans(agencyId, resourceId);
            }else if(resourceId>0 && agencyId<=0)
            {
                ResourceManagement resourceMgr = new ResourceManagement(agentMgtMgr.CurrentLoginUser);
                taocans = resourceMgr.FindResourceTaocans(resourceId, 0, false);
            }
           
            message.Status = "OK";
            message.Item = taocans;
            return message;
        }

        [AcceptVerbs("POST")]
        public ApiMessage GetResourceAndAgentReports()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            OrderManagement orderMgr = new OrderManagement(User.Identity.Name);
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(request["startTime"]))
            {
                DateTime.TryParse(request["startTime"], out sDate);
            }
            if (!string.IsNullOrEmpty(request["endTime"]))
            {
                DateTime.TryParse(request["endTime"], out eDate);
            }
            long sintDate = sDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(sDate) : 0;
            long eintDate = eDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(eDate) : 0;
            ChartReport report = orderMgr.SearchResourceAndAgentReport(sintDate,eintDate);
            message.Status = "OK";
            message.Item = report;
            return message;
        }
    }
}
