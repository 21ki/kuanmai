using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KMBit.Beans;
using KMBit.Util;
using WeChat.Adapter.Requests;
using WeChat.Adapter.Responses;
using WeChat.Adapter;
namespace KMBit.BL
{
    public class PersistentValueManager
    {
        public static object o = new object();
        public static WeChatPayConfig config;
        private static AccessToken WeChatAccessToken;
        private static JSAPITicket WeChatJsApiTicket;
        static PersistentValueManager()
        {
            KMLogger.GetLogger().Info(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatPayConfig.xml"));
            KMLogger.GetLogger().Info(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatAccessToken.xml"));
            KMLogger.GetLogger().Info(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatJSAPITicket.xml"));
            config = XMLUtil.DeserializeXML<WeChatPayConfig>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatPayConfig.xml"));
            WeChatAccessToken = XMLUtil.DeserializeXML<AccessToken>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatAccessToken.xml"));
            WeChatJsApiTicket= XMLUtil.DeserializeXML<JSAPITicket>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatJSAPITicket.xml"));
        }

        private static AccessToken RequestWeChatAccessToken()
        {
            AccessToken token = null;
            TokenRequest request = null;
            request = new TokenRequest(config);
            BaseResponse res = request.Execute();
            if (res != null)
            {
                AccessTokenResponse tokenRes = (AccessTokenResponse)res;
                if (tokenRes.Access_Token != null)
                {
                    token = tokenRes.Access_Token;  
                }
            }
            if(token!=null)
            {
                XMLUtil.SerializeObject(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatAccessToken.xml"), token);
            }
            return token;
        }
        private static JSAPITicket RequestWeChatJsApiTicket()
        {
            JSAPITicket ticket = null;
            JSAPITicketRequest request = null;
            request = new JSAPITicketRequest(config);
            request.Access_Token = GetWeChatAccessToken();
            BaseResponse res = request.Execute();
            if (res != null)
            {
                JSAPITicketResponse jsRes = (JSAPITicketResponse)res;
                ticket = jsRes.Ticket;
            }
            if (ticket != null)
            {
                XMLUtil.SerializeObject(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatJSAPITicket.xml"), ticket);
            }
            return ticket;
        }

        public static JSAPITicket GetWeChatJsApiTicket()
        {
            if(WeChatJsApiTicket==null || WeChatJsApiTicket.ExpiresTime < DateTime.Now)
            {
                WeChatJsApiTicket = RequestWeChatJsApiTicket();
            }

            return WeChatJsApiTicket;
        }
        public static AccessToken GetWeChatAccessToken()
        {          
            if (WeChatAccessToken==null)
            {
                WeChatAccessToken = RequestWeChatAccessToken();
            }
            else
            {
                if(WeChatAccessToken.ExpiresTime < DateTime.Now)
                {
                    WeChatAccessToken = RequestWeChatAccessToken();
                }
                else if(DateTimeUtil.ConvertDateTimeToInt(WeChatAccessToken.ExpiresTime) - DateTimeUtil.ConvertDateTimeToInt(DateTime.Now)<20)
                {                   
                    WeChatAccessToken = RequestWeChatAccessToken();
                }
            }

            return WeChatAccessToken;
        }
    }
}
