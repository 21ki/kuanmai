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
            List<BAgentRoute> tcs = baseMgt.FindTaocans(0,request["sp"],request["province"]);
            message.Status = "OK";
            message.Item = tcs;
            return message;
        }

    }
}
