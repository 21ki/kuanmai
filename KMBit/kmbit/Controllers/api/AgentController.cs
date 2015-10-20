using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Agent;
using KMBit.DAL;
using KMBit.Filters;
using KMBit.Util;
namespace KMBit.Controllers.api
{
    [Authorize]
    [AgentFilter(Message ="管理员账户请不要试图访问代理商后台页面")]
    public class AgentController : BaseApiController
    {
        [AcceptVerbs("POST", "GET")]
        public ApiMessage SearchDirectChargeTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentManagement baseMgt = new AgentManagement(User.Identity.Name);
            List<BAgentRoute> tcs = baseMgt.FindTaocans(0,request["sp"],request["province"]);
            message.Status = "OK";
            message.Item = tcs;
            return message;
        }

        [HttpPost]
        public ApiMessage GetAgentReports()
        {
            this.IniRequest();
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
            ApiMessage message = new ApiMessage();
            OrderManagement orderMgt = new OrderManagement(User.Identity.Name);
            List<ReportTemplate> reportList = orderMgt.SearchAgentReport(0, sintDate, eintDate);
            message.Status = "OK";
            message.Item = reportList;
            return message;
        }
    }
}
