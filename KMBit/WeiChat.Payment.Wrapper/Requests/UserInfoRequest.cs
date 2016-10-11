using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WeChat.Adapter.Responses;
namespace WeChat.Adapter.Requests
{
    public class UserInfoRequest : BaseRequest
    {
        public string AccessToken { get; set; }
        public string OpenId { get; set; }
        public UserInfoRequest(WeChatPayConfig config) : base(config)
        {
            logger.Info(this.GetType().FullName);
            this.url = config.GetUserInfoUrl;
        }

        public override BaseResponse Execute()
        {
            if (string.IsNullOrEmpty(this.url))
            {
                logger.Error("invalid url");
                throw new Exception("invalid url");
            }
            if (string.IsNullOrEmpty(appid))
            {
                logger.Error("invalid appid");
                throw new Exception("invalid appid");
            }
            if (string.IsNullOrEmpty(secret))
            {
                logger.Error("invalid secret");
                throw new Exception("invalid secret");
            }
           
            if (string.IsNullOrEmpty(AccessToken))
            {
                logger.Error("invalid AccessToken");
                throw new Exception("invalid AccessToken");
            }
            if (string.IsNullOrEmpty(OpenId))
            {
                logger.Error("invalid OpenId");
                throw new Exception("invalid OpenId");
            }
           
            UserInfoResponse userRes = null;
            NameValueCollection col = new NameValueCollection();
            col.Add("access_token", this.AccessToken);
            col.Add("openid", this.OpenId);
            string res = HttpSercice.PostHttpRequest(this.url, col, RequestType.GET, null);
            if (string.IsNullOrEmpty(res))
            {
                logger.Error("Nothing returned by wechat");
                return null;
            }
            logger.Info(string.Format("UserInfoRequest response:{0}", res));
            try
            {
                JObject json = JObject.Parse(res);
                if(json!=null)
                {
                    userRes = new UserInfoResponse(json);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            
            return userRes;
        }
    }
}
