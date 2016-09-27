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
            config = XMLUtil.DeserializeXML<WeChatPayConfig>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatPayConfig.xml"));
            WeChatAccessToken = XMLUtil.DeserializeXML<AccessToken>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatAccessToken.xml"));
            WeChatJsApiTicket= XMLUtil.DeserializeXML<JSAPITicket>(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config\\WeChatJSAPITicket.xml"));
        }
    }
}
