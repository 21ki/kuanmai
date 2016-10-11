using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WeChat.Adapter;
namespace WeChat.Adapter.Responses
{
    public class BaseResponse
    {
        public JObject Json { get; set; }
        public ResuleState return_code { get; set; }
        public string return_msg { get; set; }
        public string appid { get; set; }
        public string mch_id { get; set; }
        public string nonce_str { get; set; }
        public string device_info { get; set; }
        public string openid { get; set; }
        public string sign { get; set; }
        public string result_code { get; set; }
        public string err_code { get; set; }
        public string err_code_des { get; set; }

        public BaseResponse()
        {
            
        }
        public BaseResponse(JObject json)
        {
            this.Json = json;
            ParseJson();
        }

        protected virtual void ParseJson()
        {

        }
    }
}
