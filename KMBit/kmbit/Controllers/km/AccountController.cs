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
using log4net;
namespace KMBit.Controllers.km
{
    public class AccountController: BaseApiController
    {
        //[HttpPost]
        [AcceptVerbs("POST", "GET")]
        public APIChargeResult Charge()
        {
            logger.Info("Client system call is coming...");
            APIChargeResult message = new APIChargeResult();
            try
            {               
                base.IniRequest();
                string siganture = string.Empty;
                string accessToken = string.Empty;
                string queryStr = string.Empty;
                base.ParseSigantures(out siganture, out accessToken, out queryStr);
                if (string.IsNullOrEmpty(siganture))
                {
                    message.Status = "FAILED";
                    message.Message = "sign不能为空";
                    return message;
                }
                if (string.IsNullOrEmpty(accessToken))
                {
                    message.Status = "FAILED";
                    message.Message = "token不能为空";
                    return message;
                }
                if (string.IsNullOrEmpty(queryStr))
                {
                    message.Status = "FAILED";
                    message.Message = "传入的参数不合法";
                    return message;
                }

                ApiAccessManagement accessMgt = new ApiAccessManagement();
                BUser user = accessMgt.GetUserByAccesstoken(accessToken);
                if (user == null)
                {
                    message.Status = "FAILED";
                    message.Message = "token不正确";
                    return message;
                }
                logger.Info(string.Format("Client system post data:{0}", queryStr));
                logger.Info(string.Format("Signature:{0}", siganture != null ? siganture : ""));
                logger.Info(string.Format("Agent - {0}", user != null ? user.User.Name : ""));
                bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
                if (!verifySign)
                {
                    logger.Info(string.Format("Failed to verify signature."));
                    message.Status = "FAILED";
                    message.Message = "签名不正确，请使用正确的SecurityToken进行签名";
                    return message;
                }
                logger.Info("Signature verification passed.");
                int routeId = 0;
                string callbackUrl = request["CallBackUrl"] != null ? request["CallBackUrl"] : "";
                string province = request["Province"] != null ? request["Province"] : "";
                string city = request["City"] != null ? request["City"] : "";
                string mobile = request["Mobile"] != null ? request["Mobile"] : "";
                string clientOrderId = request["Client_order_id"];
                string spName= request["MobileSP"];
                int.TryParse(request["Id"], out routeId);
                if (string.IsNullOrEmpty(mobile) || mobile.Trim().Length != 11)
                {
                    message.Status = "FAILED";
                    message.Message = "手机号码不正确";
                    return message;
                }

                if (string.IsNullOrEmpty(province))
                {
                    message.Status = "FAILED";
                    message.Message = "手机归属省份（参数Province）不能为空";
                    return message;
                }

                //if (string.IsNullOrEmpty(city))
                //{
                //    message.Status = "FAILED";
                //    message.Message = "手机归属城市（参数City）不能为空";
                //    return message;
                //}

                if (string.IsNullOrEmpty(spName))
                {
                    message.Status = "FAILED";
                    message.Message = "手机归属运营商（参数MobileSP）不能为空";
                    return message;
                }
                else
                {
                    if(spName!="中国移动" && spName!="中国联通" && spName!="中国电信")
                    {
                        message.Status = "FAILED";
                        message.Message = "手机归属运营商（参数MobileSP）值必须为 中国移动，中国联通或者中国电信";
                        return message;
                    }
                }

                if (routeId <= 0)
                {
                    message.Status = "FAILED";
                    message.Message = "非法路由产品编号(ID)";
                    return message;
                }
                
                ProductManagement pdtMger = new ProductManagement();
                message = pdtMger.Charge(user.User.Id, routeId, mobile,spName, province, city, callbackUrl, clientOrderId);
                logger.Info(message.Status);
                logger.Info(message.Message);
            }
            catch (KMBitException kex)
            {
                logger.Error(kex);
                message.Status = "FAILED";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                message.Status = "FAILED";
                message.Message = "未知错误，联系平台管理员";
            }
            logger.Info("Finished processing client calling.");
            logger.Info("...................................");
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
                message.Status = "FAILED";
                message.Message = "签名不能为空";               
                return message;
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                message.Status = "FAILED";
                message.Message = "AccessToken不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(queryStr))
            {
                message.Status = "FAILED";
                message.Message = "参数列表不正确";
                return message;
            }

            ApiAccessManagement accessMgt = new ApiAccessManagement();
            BUser user = accessMgt.GetUserByAccesstoken(accessToken);
            if (user == null)
            {
                message.Status = "FAILED";
                message.Message = "AccessToken不正确";
                return message;
            }

            bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
            if (!verifySign)
            {
                message.Status = "FAILED";
                message.Message = "签名不正确";
                return message;
            }

            ProductManagement pdtManager = new ProductManagement();
            List<Beans.API.AgentProduct> products = pdtManager.GetAgentProducts(user.User.Id);
            message.Status = "SUCCEED";
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
                message.Status = "FAILED";
                message.Message = "签名不能为空";
                return message;
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                message.Status = "FAILED";
                message.Message = "AccessToken不能为空";
                return message;
            }
            if(string.IsNullOrEmpty(queryStr))
            {
                message.Status = "FAILED";
                message.Message = "参数列表不正确";
                return message;
            }
            ApiAccessManagement accessMgt = new ApiAccessManagement();
            BUser user = accessMgt.GetUserByAccesstoken(accessToken);
            if(user==null)
            {
                message.Status = "FAILED";
                message.Message = "AccessToken不正确";
                return message;
            }

            bool verifySign = accessMgt.VerifyApiSignature(user.User.SecurityStamp, queryStr, siganture);
            if (!verifySign)
            {
                message.Status = "FAILED";
                message.Message = "签名不正确";
                return message;
            }
            message.Status = "SUCCEED";
            message.Message = "操作成功";
            user.User.PasswordHash = "";
            user.User.SecurityStamp = "";
            user.User.AccessToken = "";
            message.Item = user.User;
            return message;
        }
    }
}