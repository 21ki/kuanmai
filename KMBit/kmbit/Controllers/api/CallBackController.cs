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
                chargeMgr.CallBack(paramters);
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

        [AcceptVerbs("POST", "GET")]
        public ApiMessage ChargeBack()
        {
            ApiMessage message = new ApiMessage();
            try
            {
                this.IniRequest();
                SortedDictionary<string, string> sArray = GetRequestParameters();
                ChargeBridge bridge = new ChargeBridge();
                ChargeResult result = bridge.ChargeCallBack(sArray);
                message.Message = result.Message;
                message.Status = "OK";
                if (result.Status!=  ChargeStatus.SUCCEED)
                {
                    message.Status = "ERROR";
                }
            }
            catch (KMBitException kex)
            {
                message.Status = "ERROR";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "ERROR";
                message.Message = "未知错误";
            }

            return message;
        }

        [AcceptVerbs("POST", "GET")]
        public HttpResponseMessage BeibeiBack()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            string result = "";
            try
            {
                this.IniRequest();
                SortedDictionary<string, string> sArray = GetRequestParameters();
                ChargeBridge bridge = new ChargeBridge();
                result = bridge.ChargeCallBack(sArray, ResourceType.BeiBeiFlow);
            }            
            catch
            {
                result = "fail";
            }

            resp.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }
    }
}
