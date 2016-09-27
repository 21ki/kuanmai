using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
namespace WeChat.Adapter.Responses
{
    public class AccessTokenResponse:BaseResponse
    {
        public AccessToken Access_Token { get; set; }
    }
}
