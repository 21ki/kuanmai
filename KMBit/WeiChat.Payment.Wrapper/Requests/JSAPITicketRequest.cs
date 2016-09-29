using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using WeChat.Adapter.Responses;

namespace WeChat.Adapter.Requests
{
    public class JSAPITicketRequest:BaseRequest
    {
        public AccessToken Access_Token { get; set; }
        public JSAPITicketRequest(WeChatPayConfig config):base(config)
        {
            this.url = config.GetJsTicketUrl;
        }

        /// <summary>
        /// Returns JSAPITicketResponse
        /// </summary>
        /// <returns>JSAPITicketResponse</returns>
        public override BaseResponse Execute()
        {
            JSAPITicketResponse ticketRes = null;
            if(Access_Token==null)
            {
                throw new Exception("Please input the valid accesstoken instance");
            }

            if(string.IsNullOrEmpty(Access_Token.Access_Token))
            {
                throw new Exception("invalid access token.");
            }

            if(DateTime.Now>Access_Token.ExpiresTime)
            {
                throw new Exception("access token is expired");
            }
            DateTime now = DateTime.Now;
            NameValueCollection col = new NameValueCollection();
            col.Add("access_token", this.Access_Token.Access_Token);
            col.Add("type", "jsapi");
            string res = HttpSercice.PostHttpRequest(this.url, col, RequestType.GET, null);
            if (!string.IsNullOrEmpty(res))
            {
                JObject json = null;
                try
                {
                    json = JObject.Parse(res);
                    if (json != null)
                    {
                        ticketRes = new JSAPITicketResponse();
                        JSAPITicket ticket = new JSAPITicket();
                        if(json["ticket"]!=null)
                        {
                            ticket.Ticket = json["ticket"].ToString();
                        }

                        if (json["expires_in"] != null)
                        {
                            ticket.ExipresIn = int.Parse(json["expires_in"].ToString());
                            ticket.ExpiresTime = now.AddSeconds(ticket.ExipresIn);
                        }
                        if (json["errcode"] != null)
                        {
                            ticketRes.err_code = json["errcode"].ToString();
                        }
                        if (json["errmsg"] != null)
                        {
                            ticketRes.err_code_des = json["errmsg"].ToString();
                        }
                        ticketRes.Ticket = ticket;
                    }
                }
                catch
                {
                }
            }
            return ticketRes;
        }
    }
}
