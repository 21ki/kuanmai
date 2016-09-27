using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeChat.Adapter.Responses;
namespace WeChat.Adapter.Requests
{
    public class TokenRequest:BaseRequest
    {
        public TokenRequest(WeChatPayConfig config):base(config)
        {
            this.url = config.GetTokenUrl;
        }

        public override BaseResponse Execute()
        {
            if (string.IsNullOrEmpty(appid))
            {
                throw new Exception("invalid appid");
            }

            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("invalid secret");
            }

            AccessTokenResponse tokenRes = null;
            NameValueCollection col = new NameValueCollection();
            col.Add("grant_type", "client_credential");
            col.Add("appid", this.appid);
            col.Add("secret", this.secret);
            DateTime now = DateTime.Now;
            string res = HttpSercice.PostHttpRequest(this.url, col, RequestType.GET, null);
            if(!string.IsNullOrEmpty(res))
            {
                JObject json = null;
                try
                {
                    json = JObject.Parse(res);
                    if(json!=null)
                    {
                        tokenRes = new AccessTokenResponse();
                        AccessToken accessToken = new AccessToken();                        
                        accessToken.Access_Token = json["access_token"]!=null? json["access_token"].ToString():"";
                        if(!string.IsNullOrEmpty(accessToken.Access_Token))
                        {
                            accessToken.ExpiresIn = json["expires_in"] != null ? int.Parse(json["expires_in"].ToString()) : 0;
                        }
                        accessToken.ExpiresTime = now.AddSeconds(accessToken.ExpiresIn);
                        if(!string.IsNullOrEmpty(accessToken.Access_Token) && accessToken.ExpiresIn>0)
                        {
                            tokenRes.Access_Token = accessToken;
                        }

                        if(json["errcode"]!=null)
                        {
                            tokenRes.err_code = json["errcode"].ToString();
                        }
                        if (json["errmsg"] != null)
                        {
                            tokenRes.err_code_des = json["errmsg"].ToString();
                        }
                    }
                }
                catch
                {
                }                
            }
            return tokenRes;
        }
    }
}
