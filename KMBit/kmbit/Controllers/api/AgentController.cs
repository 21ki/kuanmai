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
    public class AgentController : BaseApiController
    {
        [AcceptVerbs("POST", "GET")]
        public ApiMessage SearchDirectChargeTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentManagement baseMgt = new AgentManagement(User.Identity.Name);
            if(request["scope"]==null || (request["scope"].ToLower()!="global" && request["scope"].ToLower()!="local"))
            {
                message.Status = "ERROR";
                message.Message = "scope must be global or local and in lower case.";
                return message;
            }
            if (request["sp"] == null || string.IsNullOrEmpty(request["sp"]))
            {
                message.Status = "ERROR";
                message.Message = "Unknow mobile phone sp name";
                return message;
            }
            if (request["province"] == null || string.IsNullOrEmpty(request["province"]))
            {
                message.Status = "ERROR";
                message.Message = "Unknow mobile phone province name";
                return message;
            }
            BitScope scope = BitScope.Global;
            if (request["scope"].Trim().ToLower() == "local") {
                scope = BitScope.Local;
            }
            List<BAgentRoute> tcs = baseMgt.FindTaocans(0,request["sp"],request["province"], scope, true);
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
