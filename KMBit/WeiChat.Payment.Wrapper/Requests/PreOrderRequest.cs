using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;

using log4net;
using WeChat.Adapter;
using WeChat.Adapter.Responses;
namespace WeChat.Adapter.Requests
{   
    public class PreOrderRequest : BaseRequest
    {        
        public int total_fee { get; set; }
        public string spbill_create_ip { get; set; }
        public string notify_url { get; private set; }
        public TradeType trade_type { get; set; }
        public string out_trade_no { get; set; }
        public string attach { get; set; }
        public string openid { get; set; }
        public string detail { get; set; }
        public string device_info { get; set; }
        public PreOrderRequest(WeChatPayConfig config):base(config)
        {           
            this.notify_url = config.NotifyUrl;
            this.url = config.CreateOrderUrl;
        }
        public override BaseResponse Execute()
        {
            logger.Info(this.GetType().FullName+"................");
            logger.Info("url:"+this.url);
            if(string.IsNullOrEmpty(out_trade_no))
            {
                logger.Info("out_trade_no cannot be empty");
                throw new Exception("out_trade_no cannot be empty");
            }
            if (string.IsNullOrEmpty(notify_url))
            {
                logger.Info("notify_url cannot be empty");
                throw new Exception("notify_url cannot be empty");
            }
            if (string.IsNullOrEmpty(spbill_create_ip))
            {
                logger.Info("spbill_create_ip cannot be empty");
                throw new Exception("spbill_create_ip cannot be empty");
            }
            PreOrderResponse response = null;
            SortedDictionary<string, object> paras = new SortedDictionary<string, object>();
            Type type =this.GetType();
            PropertyInfo[] properties= type.GetProperties();
            if(properties!=null)
            {
                for(int i=0;i<properties.Length;i++)
                {                   
                    string key = properties[i].Name;
                    object value = properties[i].GetValue(this);
                    paras.Add(key, value);
                }
            }
            string paraUrl = string.Empty;
            foreach(KeyValuePair<string,object> param in paras)
            {
                if (param.Value != null && !string.IsNullOrEmpty(param.Value.ToString()))
                {
                    if (paraUrl == string.Empty)
                    {
                        paraUrl = param.Key + "=" + (param.Value != null ? param.Value.ToString() : "");
                    }
                    else
                    {
                        paraUrl += "&" + param.Key + "=" + (param.Value != null ? param.Value.ToString() : "");
                    }
                }
            }
            paraUrl += "&key=" + this.shop_secret;
            logger.Info("parameters:"+paraUrl);
            string sign = null;
            if(signType.Trim().ToLower()=="md5")
            {
                sign = HashWrapper.MD5_Hash(paraUrl);
            }
            sign = sign.ToUpper();
            logger.Info("sign:" + sign);
            paras.Add("sign",sign);
            NameValueCollection col = new NameValueCollection();
            foreach (KeyValuePair<string, object> param in paras)
            {
                if(param.Value!=null && !string.IsNullOrEmpty(param.Value.ToString()))
                {
                    col.Add(param.Key,  param.Value.ToString());
                }                
            }
            string str = HttpSercice.PostHttpRequest(this.url, col, RequestType.POST,"text/xml");
            logger.Info("response:"+str);
            response = ParseXML(str);
            logger.Info("Done.");
            return response;
        }

        public PreOrderResponse ParseXML(string str)
        {
            PreOrderResponse res = null;
            if (string.IsNullOrEmpty(str))
            {
                return res;
            }
            try
            {                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                res = new PreOrderResponse();
                XmlNode return_code = doc.SelectSingleNode("/xml/return_code");
                if(return_code!=null)
                {
                    res.return_code = BaseRequest.ParseResuleState(return_code.InnerText);
                }

                //If fail
                if(res.return_code== ResuleState.FAIL)
                {
                    XmlNode err_code = doc.SelectSingleNode("/xml/err_code");
                    if (err_code != null)
                    {
                        res.err_code = err_code.InnerText.Trim();
                    }
                    XmlNode err_code_des = doc.SelectSingleNode("/xml/err_code_des");
                    if (err_code_des != null)
                    {
                        res.err_code_des = err_code_des.InnerText.Trim();
                    }
                }

                XmlNode return_msg = doc.SelectSingleNode("/xml/return_msg");
                if (return_msg != null)
                {
                    res.return_msg = return_msg.InnerText.Trim();
                }

                XmlNode appid = doc.SelectSingleNode("/xml/appid");
                if (appid != null)
                {
                    res.appid = appid.InnerText.Trim();
                }

                XmlNode mch_id = doc.SelectSingleNode("/xml/mch_id");
                if (mch_id != null)
                {
                    res.mch_id = mch_id.InnerText.Trim();
                }

                XmlNode nonce_str = doc.SelectSingleNode("/xml/nonce_str");
                if (nonce_str != null)
                {
                    res.nonce_str = nonce_str.InnerText.Trim();
                }

                XmlNode sign = doc.SelectSingleNode("/xml/sign");
                if (sign != null)
                {
                    res.sign = sign.InnerText.Trim();
                }

                XmlNode result_code = doc.SelectSingleNode("/xml/result_code");
                if (result_code != null)
                {
                    res.result_code = result_code.InnerText.Trim();
                }

                XmlNode prepay_id = doc.SelectSingleNode("/xml/prepay_id");
                if (prepay_id != null)
                {
                    res.prepay_id = prepay_id.InnerText.Trim();
                }

                XmlNode trade_type = doc.SelectSingleNode("/xml/trade_type");
                if (trade_type != null)
                {
                    res.trade_type = trade_type.InnerText.Trim();
                }
            }
            catch (Exception ex)
            {

            }

            return res;
        }
    }
}
