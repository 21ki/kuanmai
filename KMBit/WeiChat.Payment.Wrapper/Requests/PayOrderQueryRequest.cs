using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Specialized;
using System.Xml;
using WeChat.Adapter;
using WeChat.Adapter.Responses;
namespace WeChat.Adapter.Requests
{
    public class PayOrderQueryRequest:BaseRequest
    {
        public string out_trade_no { get; set; }
       
        public PayOrderQueryRequest(WeChatPayConfig config):base(config)
        {
            //this.url = "https://api.mch.weixin.qq.com/pay/orderquery";  
            this.url = config.PaymentQueryUrl;
        }
        public override BaseResponse Execute()
        {
            PayOrderQueryResponse response = null;
            SortedDictionary<string, object> paras = new SortedDictionary<string, object>();
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            if (properties != null)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    string key = properties[i].Name;
                    object value = properties[i].GetValue(this);
                    paras.Add(key, value);
                }
            }
            string paraUrl = string.Empty;
            foreach (KeyValuePair<string, object> param in paras)
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
            paraUrl += "&key=" + this.secret;
            string sign = null;
            if (signType.Trim().ToLower() == "md5")
            {
                sign = HashWrapper.MD5_Hash(paraUrl);
            }
            paras.Add("sign", sign);
            NameValueCollection col = new NameValueCollection();
            foreach (KeyValuePair<string, object> param in paras)
            {
                col.Add(param.Key, param.Value != null ? param.Value.ToString() : "");
            }
            string str = HttpSercice.PostHttpRequest(this.url, col, RequestType.POST, "xml");
            response = ParseXML(str);
            return response;
        }

        public PayOrderQueryResponse ParseXML(string str)
        {
            PayOrderQueryResponse res = null;
            if (string.IsNullOrEmpty(str))
            {
                return res;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(str);
                res = new PayOrderQueryResponse();
                XmlNode return_code = doc.SelectSingleNode("/xml/return_code");
                if (return_code != null)
                {
                    res.return_code = BaseRequest.ParseResuleState(return_code.InnerText);
                }
                if (res.return_code == ResuleState.FAIL)
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

                XmlNode device_info = doc.SelectSingleNode("/xml/device_info");
                if (device_info != null)
                {
                    res.device_info = device_info.InnerText.Trim();
                }

                XmlNode openid = doc.SelectSingleNode("/xml/openid");
                if (openid != null)
                {
                    res.openid = openid.InnerText.Trim();
                }

                XmlNode is_subscribe = doc.SelectSingleNode("/xml/is_subscribe");
                if (is_subscribe != null)
                {
                    res.is_subscribe = is_subscribe.InnerText.Trim();
                }
                XmlNode trade_type = doc.SelectSingleNode("/xml/trade_type");
                if (trade_type != null)
                {
                    res.trade_type =BaseRequest.ParseTradeType(trade_type.InnerText);
                }

                XmlNode bank_type = doc.SelectSingleNode("/xml/bank_type");
                if (bank_type != null)
                {
                    res.bank_type = bank_type.InnerText.Trim();
                }

                XmlNode total_fee = doc.SelectSingleNode("/xml/total_fee");
                if (total_fee != null)
                {
                    res.total_fee =int.Parse( total_fee.InnerText.Trim());
                }

                XmlNode fee_type = doc.SelectSingleNode("/xml/fee_type");
                if (total_fee != null)
                {
                    res.fee_type = fee_type.InnerText.Trim();
                }
                XmlNode transaction_id = doc.SelectSingleNode("/xml/transaction_id");
                if (transaction_id != null)
                {
                    res.fee_type = transaction_id.InnerText.Trim();
                }
                XmlNode out_trade_no = doc.SelectSingleNode("/xml/out_trade_no");
                if (out_trade_no != null)
                {
                    res.out_trade_no = out_trade_no.InnerText.Trim();
                }
                XmlNode attach = doc.SelectSingleNode("/xml/attach");
                if (attach != null)
                {
                    res.attach = attach.InnerText.Trim();
                }
                XmlNode time_end = doc.SelectSingleNode("/xml/time_end");
                if (time_end != null)
                {
                    res.time_end = time_end.InnerText.Trim();
                }
                XmlNode trade_state = doc.SelectSingleNode("/xml/trade_state");
                if (trade_state != null)
                {
                    res.trade_state = BaseRequest.ParseTraseState(trade_state.InnerText);
                }
            }
            catch (Exception ex)
            {

            }

            return res;
        }
    }
}
