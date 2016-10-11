using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter
{
    public class WeChatPayConfig
    {
        public string APPID { get; set; }
        public string Secret { get; set; }
        public string ShopID { get; set; }
        public string ShopSecret { get; set; }
        public string SignType { get; set; }
        public string CharSet { get; set; }
        public string CreateOrderUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string PaymentQueryUrl { get; set; }
        public string GetTokenUrl { get; set; }
        public string GetJsTicketUrl { get; set; }
        public string GetAccessTokenUrl { get; set; }
        public string GetUserInfoUrl { get; set; }
    }
}
