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
            ApiMessage message = new ApiMessage() { Status = "OK", Message = "成功执行回调函数" };
            try
            {
                this.IniRequest();                
                ICharge chargeMgr = new ChongBaCharge();
                List<WebRequestParameters> paramters = new List<WebRequestParameters>();
                paramters.Add(new WebRequestParameters("orderId", request["orderId"], false));
                paramters.Add(new WebRequestParameters("respCode", request["respCode"], false));
                paramters.Add(new WebRequestParameters("respMsg", request["respMsg"], false));
                paramters.Add(new WebRequestParameters("transNo", request["transNo"], false));
                if (paramters.Count > 0)
                {
                    chargeMgr.CallBack(paramters);
                }
                else
                {
                    message.Status = "ERROE";
                    message.Message = "经销商充值系统没有回调数据，充值失败";
                }
            }
            catch(KMBitException e)
            {
                message.Status = "ERROE";
                message.Message = e.Message;
            }
            catch (Exception ex)
            {
                message.Status = "ERROE";
                message.Message = ex.Message;
            }
           
            
            return message;
        }
    }
}
