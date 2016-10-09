using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using log4net;
using WeChat.Adapter.Responses;
using WeChat.Adapter.Requests;
namespace WeChat.Adapter
{
    public class WeChatPaymentWrapper
    {
        static ILog logger = WeChatLogger.GetLogger();
        public static AccessToken GetWeChatToken(WeChatPayConfig config, AccessToken oldToken,out bool changed)
        {
            changed = false;
            AccessToken token = null;
            TokenRequest request = null;
            bool needGet = false;
            if (oldToken==null)
            {
                needGet = true;
            }
            else
            {
                if(oldToken.ExpiresTime<DateTime.Now)
                {
                    needGet = true;
                }
            }
           
            if(needGet)
            {
                request = new TokenRequest(config);
                BaseResponse res = request.Execute();
                if (res != null)
                {
                    changed = true;
                    AccessTokenResponse tokenRes = (AccessTokenResponse)res;
                    if (tokenRes.Access_Token != null)
                    {
                        token = tokenRes.Access_Token;
                        oldToken = token;
                    }
                }
            }
            return token;
        }

        public static JSAPITicket GetJSAPITicket(WeChatPayConfig config,AccessToken oldToken,JSAPITicket oldTicket, out bool changed)
        {
            changed = false;
            JSAPITicket ticket = null;
            bool needGet = false;
            JSAPITicketRequest request = null;
            if(oldTicket==null)
            {
                needGet = true;
            }
            else
            {
                if(oldTicket.ExpiresTime<DateTime.Now)
                {
                    needGet = true;
                }
            }
            if(needGet)
            {
                changed = true;
                bool tChanged = false;
                request = new JSAPITicketRequest(config);
                AccessToken token = WeChatPaymentWrapper.GetWeChatToken(config,oldToken,out tChanged);
                request.Access_Token = token;
                BaseResponse res = request.Execute();
                if(res!=null)
                {
                    JSAPITicketResponse jsRes = (JSAPITicketResponse)res;
                    ticket = jsRes.Ticket;
                }
            }
            return ticket;
        }
        public static BaseResponse ParsePaymentNotify(string xml)
        {
            logger.Info("ParsePaymentNotify.............");
            PaymentNotifyResponse res = null;
            XmlDocument doc = null;
            try
            {
                if (!string.IsNullOrEmpty(xml))
                {
                    doc = new XmlDocument();
                    doc.LoadXml(xml);
                    res = new PaymentNotifyResponse();
                    XmlNode return_code = doc.SelectSingleNode("/xml/return_code");
                    if (return_code != null)
                    {
                        logger.Info(return_code.InnerText);
                        res.return_code = BaseRequest.ParseResuleState(return_code.InnerText);
                    }
                    XmlNode return_msg = doc.SelectSingleNode("/xml/return_msg");
                    if (return_msg != null)
                    {
                        res.return_msg = return_msg.InnerText.Trim();
                    }

                    if (res.return_code == ResuleState.FAIL)
                    {
                        return res;
                    }
                    XmlNode sign = doc.SelectSingleNode("/xml/sign");
                    if (sign != null)
                    {
                        res.sign = sign.InnerText.Trim();
                    }
                    XmlNode transaction_id = doc.SelectSingleNode("/xml/transaction_id");
                    if (transaction_id != null)
                    {
                        res.transaction_id = transaction_id.InnerText.Trim();
                    }
                    XmlNode out_trade_no = doc.SelectSingleNode("/xml/out_trade_no");
                    if (out_trade_no != null)
                    {
                        res.out_trade_no = out_trade_no.InnerText.Trim();
                    }
                    XmlNode result_code = doc.SelectSingleNode("/xml/result_code");
                    if (result_code != null)
                    {
                        res.result_code = result_code.InnerText.Trim();
                    }

                    XmlNode appid = doc.SelectSingleNode("/xml/appid");
                    if (appid != null)
                    {
                        logger.Info(appid.InnerText);
                        res.appid = appid.InnerText.Trim();
                    }

                    XmlNode mch_id = doc.SelectSingleNode("/xml/mch_id");
                    if (mch_id != null)
                    {
                        res.mch_id = mch_id.InnerText.Trim();
                    }

                    XmlNode sub_mch_id = doc.SelectSingleNode("/xml/sub_mch_id");
                    if (sub_mch_id != null)
                    {
                        res.sub_mch_id = sub_mch_id.InnerText.Trim();
                    }

                    XmlNode is_subscribe = doc.SelectSingleNode("/xml/is_subscribe");
                    if (is_subscribe != null)
                    {
                        res.is_subscribe = is_subscribe.InnerText.Trim();
                    }

                    XmlNode nonce_str = doc.SelectSingleNode("/xml/nonce_str");
                    if (nonce_str != null)
                    {
                        res.nonce_str = nonce_str.InnerText.Trim();
                    }

                    XmlNode openid = doc.SelectSingleNode("/xml/openid");
                    if (openid != null)
                    {
                        res.openid = openid.InnerText.Trim();
                    }

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

                    XmlNode trade_type = doc.SelectSingleNode("/xml/trade_type");
                    if (trade_type != null)
                    {
                        res.trade_type = BaseRequest.ParseTradeType(trade_type.InnerText);
                    }

                    XmlNode bank_type = doc.SelectSingleNode("/xml/bank_type");
                    if (bank_type != null)
                    {
                        res.bank_type = bank_type.InnerText.Trim();
                    }

                    XmlNode total_fee = doc.SelectSingleNode("/xml/total_fee");
                    if (total_fee != null)
                    {
                        res.total_fee = int.Parse(total_fee.InnerText.Trim());
                    }

                    XmlNode cash_fee = doc.SelectSingleNode("/xml/cash_fee");
                    if (cash_fee != null)
                    {
                        res.cash_fee = int.Parse(cash_fee.InnerText.Trim());
                    }

                    XmlNode fee_type = doc.SelectSingleNode("/xml/fee_type");
                    if (total_fee != null)
                    {
                        res.fee_type = fee_type.InnerText.Trim();
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
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }

            logger.Info("Done ParsePaymentNotify.............");
            return res;
        }
        public static string ParsePaymentNotifySignParas(string xml)
        {
            string str = null;
            if(!string.IsNullOrEmpty(xml))
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
                    doc.LoadXml(xml);
                    XmlNode xmlNode = doc.GetElementsByTagName("xml")[0];
                    XmlNodeList nodeList = xmlNode.ChildNodes;
                    foreach(XmlNode node in nodeList)
                    {
                        if(node.Name.ToLower()=="sign")
                        {
                            continue;
                        }
                        parameters.Add(node.Name, node.InnerText!=null? node.InnerText:"");
                    } 
                    
                    foreach(KeyValuePair<string,string> kp in parameters)
                    {
                        if(str==null)
                        {
                            str = kp.Key + "=" + kp.Value;
                        }
                        else
                        {
                            str +="&"+ kp.Key + "=" + kp.Value;
                        }
                    }                   
                }
                catch
                {

                }
            }
            return str;
        }
        public static string GetPrepayId(WeChatPayConfig config,string openId,string out_trade_no,string body,string clientIp,int totalFee, TradeType type)
        { 
            string prepayId = string.Empty;
            BaseResponse response = null;
            PreOrderRequest request = new PreOrderRequest(config);
            request.out_trade_no = out_trade_no;
            request.spbill_create_ip = clientIp;
            request.total_fee = totalFee;
            request.trade_type = type;
            request.body = body;
            request.detail = "";
            request.openid = openId;
            response = request.Execute();
            if(response!=null)
            {
                prepayId = ((PreOrderResponse)response).prepay_id;
            }
            return prepayId;
        }

        public string GetJsApiPaySign(WeChatPayConfig config, SortedDictionary<string,string> param)
        {
            string sign = null;

            return sign;
        }

        public static TradeState GetPaymentState(string out_trade_id)
        {
            TradeState state = TradeState.NONE;
            return state;
        }
    }
}
