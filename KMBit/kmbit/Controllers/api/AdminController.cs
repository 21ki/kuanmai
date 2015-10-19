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
                message.Status = "ERROR";
                message.Message = "代理商编号不能为空";
                message.Item = null;
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
            if(agencyId==0 || resourceId==0)
            {
                message.Status = "ERROR";
                message.Item = null;
                message.Message = "代理商编号和资源编号都不能为空";
                return message;
            }
            List<BResourceTaocan> taocans = agentMgtMgr.FindAgencyResourceTaocans(agencyId,resourceId);
            message.Status = "OK";
            message.Item = taocans;
            return message;
        }
    }
}
