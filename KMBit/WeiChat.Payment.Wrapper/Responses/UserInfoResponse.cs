using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
using Newtonsoft.Json.Linq;
namespace WeChat.Adapter.Responses
{
    public class UserInfoResponse:BaseResponse
    {     
        public WeChatUserInfo UserInfo { get; private set; }
        public UserInfoResponse(JObject json):base(json)
        {

        }

        protected override void ParseJson()
        {
            if(this.Json==null)
            {
                return;
            }
            if (this.UserInfo == null)
            {
                this.UserInfo = new WeChatUserInfo();
            }
            IDictionary<string, JToken> userAsDictionary = this.Json;
            UserInfo.UnionId = PropertyValueIfExists("unionid", userAsDictionary);
            UserInfo.Name = PropertyValueIfExists("nickname", userAsDictionary);
            UserInfo.City = PropertyValueIfExists("city", userAsDictionary);
            UserInfo.Province = PropertyValueIfExists("province", userAsDictionary);
            UserInfo.Gendar = PropertyValueIfExists("sex", userAsDictionary) !=null? int.Parse(PropertyValueIfExists("sex", userAsDictionary)):0;
            UserInfo.Country = PropertyValueIfExists("country", userAsDictionary);
            UserInfo.AvtorUrl = PropertyValueIfExists("headimgurl", userAsDictionary);
        }

        private static string PropertyValueIfExists(string property, IDictionary<string, JToken> dictionary)
        {
            return dictionary.ContainsKey(property) ? dictionary[property].ToString() : null;
        }
    }
}
