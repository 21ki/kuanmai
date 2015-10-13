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
            List<BResourceTaocan> tcs = baseMgt.SearchResourceTaocans(request["sp"],request["province"]);
            message.Status = "OK";
            message.Item = tcs;
            return message;
        }
    }
}
