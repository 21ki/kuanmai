using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
namespace KMBit.Controllers.api
{
    public class ProductController : BaseApiController
    {
        [AcceptVerbs("POST","GET")]
        public ApiMessage SearchDirectChargeTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            BaseManagement baseMgt = new BaseManagement(0);
            if (request["scope"] == null || (request["scope"].ToLower() != "global" && request["scope"].ToLower() != "local"))
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
            if (request["scope"].Trim().ToLower() == "local")
            {
                scope = BitScope.Local;
            }
            List<BResourceTaocan> tcs = baseMgt.SearchResourceTaocans(request["sp"],request["province"], scope);
            message.Status = "OK";
            message.Item = tcs;
            return message;
        }
    }
}
