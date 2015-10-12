using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;

namespace KMBit.Controllers.api
{
    public class CallBackController : BaseApiController
    {        
        [AcceptVerbs("POST","GET")]
        public ApiMessage Chongba()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage() { Status="OK",Message="成功执行回调函数" };
            ICharge chargeMgr = new ChongBaCharge();
            List<WebRequestParameters> paramters = new List<WebRequestParameters>();
            paramters.Add(new WebRequestParameters("orderId",request["orderId"],false));
            paramters.Add(new WebRequestParameters("respCode", request["respCode"], false));
            paramters.Add(new WebRequestParameters("respMsg", request["respMsg"], false));
            paramters.Add(new WebRequestParameters("transNo", request["transNo"], false));
            chargeMgr.CallBack(paramters);
            return message;
        }
    }
}
