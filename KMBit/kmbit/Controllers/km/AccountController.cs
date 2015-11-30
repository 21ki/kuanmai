using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Controllers.api;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.BL.API;
using KMBit.Beans.API;
namespace KMBit.Controllers.km
{
    public class AccountController: BaseApiController
    {
        //[HttpPost]
        [AcceptVerbs("POST", "GET")]
        public APIChargeResult Charge()
        {
            APIChargeResult message = new APIChargeResult();
            base.IniRequest();
            string siganture = string.Empty;
            string accessToken = string.Empty;
            string queryStr = string.Empty;
            base.ParseSigantures(out siganture, out accessToken, out queryStr);
            if (string.IsNullOrEmpty(siganture))
            {
                message.Status = 3;
                message.Message = "签名不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                message.Status = 3;
                message.Message = "AccessToken不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(queryStr))
            {
                message.Status = 3;
                message.Message = "参数列表不正确";
                return message;
            }

            ApiAccessManagement accessMgt = new ApiAccessManagement();
            BUser user = accessMgt.GetUserByAccesstoken(accessToken);
            if (user == null)
            {
                message.Status = 3;
                message.Message = "AccessToken不正确";
                return message;
            }

            bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
            if (!verifySign)
            {
                message.Status =3;
                message.Message = "签名不正确";
                return message;
            }

            int routeId = 0;
            string callbackUrl= request["CallBackUrl"] != null ? request["CallBackUrl"] : "";
            string province = request["Province"] != null ? request["Province"] : "";
            string city = request["City"] != null ? request["City"] : "";
            string mobile = request["Mobile"] != null ? request["Mobile"] : "";
            int.TryParse(request["Id"],out routeId);
            if (string.IsNullOrEmpty(mobile) || mobile.Trim().Length!=11)
            {
                message.Status = 3;
                message.Message = "手机号码不正确";
                return message;
            }
            if(routeId<=0)
            {
                message.Status = 3;
                message.Message = "产品Id不正确";
                return message;
            }

            ProductManagement pdtMger = new ProductManagement();
            try
            {
                message = pdtMger.Charge(user.User.Id, routeId, mobile, province, city, callbackUrl);
               
            }catch(KMBitException kex)
            {
                message.Status = 3;
                message.Message = kex.Message;
            }catch(Exception ex)
            {
                message.Status = 3;
                message.Message = "未知错误，联系平台管理员";
            }
            return message;
        }

        //[HttpPost]
        [AcceptVerbs("POST", "GET")]
        public ApiMessage Products()
        {
            ApiMessage message = new ApiMessage();
            base.IniRequest();
            string siganture = string.Empty;
            string accessToken = string.Empty;
            string queryStr = string.Empty;
            base.ParseSigantures(out siganture, out accessToken, out queryStr);
            if (string.IsNullOrEmpty(siganture))
            {
                message.Status = "error";
                message.Message = "签名不能为空";               
                return message;
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                message.Status = "error";
                message.Message = "AccessToken不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(queryStr))
            {
                message.Status = "error";
                message.Message = "参数列表不正确";
                return message;
            }

            ApiAccessManagement accessMgt = new ApiAccessManagement();
            BUser user = accessMgt.GetUserByAccesstoken(accessToken);
            if (user == null)
            {
                message.Status = "error";
                message.Message = "AccessToken不正确";
                return message;
            }

            bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
            if (!verifySign)
            {
                message.Status = "error";
                message.Message = "签名不正确";
                return message;
            }

            ProductManagement pdtManager = new ProductManagement();
            List<Beans.API.AgentProduct> products = pdtManager.GetAgentProducts(user.User.Id);
            message.Status = "ok";
            message.Message = "操作成功";
            message.Item = products;
            return message;
        }

        //[HttpPost]
        [AcceptVerbs("POST", "GET")]
        public ApiMessage Info()
        {
            ApiMessage message = new ApiMessage();
            base.IniRequest();
            string siganture = string.Empty;
            string accessToken = string.Empty;
            string queryStr = string.Empty;
            base.ParseSigantures(out siganture, out accessToken, out queryStr);
            if(string.IsNullOrEmpty(siganture))
            {
                message.Status = "error";
                message.Message = "签名不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                message.Status = "error";
                message.Message = "AccessToken不能为空";
                return message;
            }
            if(string.IsNullOrEmpty(queryStr))
            {
                message.Status = "error";
                message.Message = "参数列表不正确";
                return message;
            }
            ApiAccessManagement accessMgt = new ApiAccessManagement();
            BUser user = accessMgt.GetUserByAccesstoken(accessToken);
            if(user==null)
            {
                message.Status = "error";
                message.Message = "AccessToken不正确";
                return message;
            }

            bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
            if (!verifySign)
            {
                message.Status = "error";
                message.Message = "签名不正确";
                return message;
            }
            message.Status = "ok";
            message.Message = "操作成功";
            user.User.PasswordHash = "";
            user.User.SecurityStamp = "";
            user.User.AccessToken = "";
            message.Item = user.User;
            return message;
        }
    }
}