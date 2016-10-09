using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter.Responses;
using WeChat.Adapter.Requests;
using WeChat.Adapter;
namespace WeChat.Adapter.Authorization
{
    public class AuthHelper
    {
        public static AccessToken GetAccessToken(WeChatPayConfig config, string code)
        {
            AccessToken token = null;
            AuthAccessTokenRequest authRequest = new AuthAccessTokenRequest(config);
            authRequest.code = code;
            BaseResponse res = authRequest.Execute();
            if(res!=null)
            {
                token = ((AccessTokenResponse)res).Access_Token;
            }
            return token;
        }
    }
}
