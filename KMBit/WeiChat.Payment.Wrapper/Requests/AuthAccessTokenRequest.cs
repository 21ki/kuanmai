using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeChat.Adapter.Responses;
namespace WeChat.Adapter.Requests
{
    public class AuthAccessTokenRequest:BaseRequest
    {
        public string code { get; set; }
        public AuthAccessTokenRequest(WeChatPayConfig config):base(config)
        {
            logger.Info("AuthAccessTokenRequest............................");
            this.url = config.GetAccessTokenUrl;
        }

        public override BaseResponse Execute()
        {
            if (string.IsNullOrEmpty(appid))
            {
                logger.Error("invalid appid");
                throw new Exception("invalid appid");
            }
            //logger.Info(string.Format("appid:{0}", this.appid));
            if (string.IsNullOrEmpty(secret))
            {
                logger.Error("invalid secret");
                throw new Exception("invalid secret");
            }
            //logger.Info(string.Format("app secret:{0}",this.secret));
            if (string.IsNullOrEmpty(code))
            {
                logger.Error("invalid authorization_code");
                throw new Exception("invalid authorization_code");
            }
            //logger.Info(string.Format("authorization_code:{0}", this.code));
            AccessTokenResponse tokenRes = null;
            NameValueCollection col = new NameValueCollection();            
            col.Add("appid", this.appid);
            col.Add("secret", this.secret);
            col.Add("code", this.code);
            col.Add("grant_type", "authorization_code");
            DateTime now = DateTime.Now;
            string res = HttpSercice.PostHttpRequest(this.url, col, RequestType.GET, null);
            if(string.IsNullOrEmpty(res))
            {
                return null;
            }
            logger.Info(string.Format("AccessTokenRequest response:{0}", res));
            try
            {
                JObject json = null;
                json = JObject.Parse(res);
                if (json != null)
                {
                    tokenRes = new AccessTokenResponse();
                    AccessToken accessToken = new AccessToken();
                    accessToken.Access_Token = json["access_token"] != null ? json["access_token"].ToString() : "";
                    if (!string.IsNullOrEmpty(accessToken.Access_Token))
                    {
                        accessToken.ExpiresIn = json["expires_in"] != null ? int.Parse(json["expires_in"].ToString()) : 0;
                    }
                    accessToken.ExpiresTime = now.AddSeconds(accessToken.ExpiresIn);
                    if (!string.IsNullOrEmpty(accessToken.Access_Token) && accessToken.ExpiresIn > 0)
                    {
                        tokenRes.Access_Token = accessToken;
                    }
                    if(json["openid"] !=null)
                    {
                        tokenRes.openid = json["openid"].ToString();
                        accessToken.OpenId = tokenRes.openid;
                    }
                    if (json["scope"] != null)
                    {
                        accessToken.Scope = json["scope"].ToString();
                    }
                    if (json["errcode"] != null)
                    {
                        tokenRes.err_code = json["errcode"].ToString();
                    }
                    if (json["errmsg"] != null)
                    {
                        tokenRes.err_code_des = json["errmsg"].ToString();
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            if(tokenRes!=null && !string.IsNullOrEmpty(tokenRes.err_code))
            {
                logger.Error(string.Format("Error code:{0}, Error message:{1}",tokenRes.err_code, tokenRes.err_code_des!=null?tokenRes.err_code_des:""));
            }
            logger.Info("AccessTokenRequest completed.");
            return tokenRes;
        }
    }
}
