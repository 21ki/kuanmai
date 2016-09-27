using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WeChat.Adapter.Responses;
using WeChat.Adapter.Requests;
namespace WeChat.Adapter
{
    public class WeChatPaymentWrapper
    {
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

        public static string GetPrepayId(WeChatPayConfig config,string out_trade_no,string clientIp,int totalFee, TradeType type)
        {
            string prepayId = string.Empty;
            BaseResponse response = null;
            PreOrderRequest request = new PreOrderRequest(config);
            request.out_trade_no = out_trade_no;
            request.spbill_create_ip = clientIp;
            request.total_fee = totalFee;
            request.trade_type = type;
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
