using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
using WeChat.Adapter.Responses;
using log4net;
namespace WeChat.Adapter.Requests
{
   
    public class BaseRequest
    {
        protected string url = null;
        protected string signType = "md5";
        protected string secret = null;
        protected string shop_secret = null;
        public string appid { get; protected set; }        
        public string mch_id { get; protected set; }
        public string nonce_str { get; protected set; }
        public string body { get; set; }
        protected ILog logger = null;

        public BaseRequest(WeChatPayConfig config)
        {
            logger = WeChatLogger.GetLogger();
            this.appid = config.APPID;
            this.secret = config.Secret;
            this.shop_secret = config.ShopSecret;
            this.mch_id = config.ShopID;
            this.signType = config.SignType.ToLower();
            this.nonce_str = Guid.NewGuid().ToString().Replace("-","");
            if(nonce_str.Length>32)
            {
                nonce_str = nonce_str.Substring(0,32);
            }
            ConfigVerification();
        }

        public virtual BaseResponse Execute()
        {
            throw new NotImplementedException("Please implement this function in inherited object.");
        }
        private void ConfigVerification()
        {
            if(string.IsNullOrEmpty(appid))
            {
                throw new Exception("AppId cannot be empty");
            }
            if (string.IsNullOrEmpty(mch_id))
            {
                throw new Exception("mch_id cannot be empty");
            }
            if (string.IsNullOrEmpty(signType))
            {
                throw new Exception("signType cannot be empty,it should be MD5 or sha");
            }
        }

        public static ResuleState ParseResuleState(string text)
        {
            if(string.IsNullOrEmpty(text))
            {
                return ResuleState.NONE;
            }
            if(text.Trim().ToUpper()=="SUCCESS")
            {
                return ResuleState.SUCCESS;
            }
            else if (text.Trim().ToUpper() == "FAIL")
            {
                return ResuleState.FAIL;
            }
            return ResuleState.NONE;
        }

        public static TradeType ParseTradeType(string type)
        {
            TradeType tradeType = TradeType.NONE;
            if(string.IsNullOrEmpty(type))
            {
                return tradeType;
            }
            if (type.Trim().ToUpper() == "MICROPAY")
            {
                tradeType = TradeType.MICROPAY;
            }
            else if (type.Trim().ToUpper() == "JSAPI")
            {
                tradeType= TradeType.JSAPI;
            }
            else if (type.Trim().ToUpper() == "NATIVE")
            {
                tradeType = TradeType.NATIVE;
            }
            else if (type.Trim() == "APP")
            {
                tradeType = TradeType.APP;
            }
            return tradeType;
        }
        public static TradeState ParseTraseState(string state)
        {
            TradeState tradeState = TradeState.NONE;
            if (string.IsNullOrEmpty(state))
            {
                return tradeState;
            }
            if (!string.IsNullOrEmpty(state))
            {
                switch(state.Trim().ToUpper())
                {
                    case "SUCCESS":
                        tradeState = TradeState.SUCCESS;
                        break;
                    case "REFUND":
                        tradeState = TradeState.REFUND;
                        break;
                    case "NOTPAY":
                        tradeState = TradeState.NOTPAY;
                        break;
                    case "CLOSED":
                        tradeState = TradeState.CLOSED;
                        break;
                    case "REVOKED":
                        tradeState = TradeState.REVOKED;
                        break;
                    case "USERPAYING":
                        tradeState = TradeState.USERPAYING;
                        break;
                    case "PAYERROR":
                        tradeState = TradeState.PAYERROR;
                        break;
                }
            }
            return tradeState;
        }
    }
}
