﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections;
using System.Collections.Specialized;

using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.DBA;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
namespace KM.JXC.BL.Open.TaoBao
{
    internal class TaoBaoAccessToken:BaseAccessToken,IAccessToken
    {        
        public TaoBaoAccessToken(int mall_type_id)
            : base(mall_type_id)
        {
           
        }       

        public Access_Token RequestAccessToken(string code){
            Access_Token token = new Access_Token();
            token.Request_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            
            if (this.Open_Key == null)
            {
                throw new Exception("Didn't find app key configuration for Mall Type ID:" + this.Mall_Type_ID);
            }

            String auth_host = this.Open_Key.Auth_Main_Url;
            NameValueCollection paras = new NameValueCollection();
            paras.Add("grant_type", "authorization_code");
            paras.Add("code", code);
            paras.Add("client_id", this.Open_Key.AppKey);
            paras.Add("client_secret", this.Open_Key.AppSecret);
            paras.Add("redirect_uri", this.Open_Key.CallBack_Url);

            string response = HttpRequester.PostHttpRequest(this.Open_Key.Auth_Main_Url + "/token", paras);
            //string response = KM.JXC.Common.Util.HttpWebRequester.PostHttpRequest(this.Open_Key.Auth_Main_Url + "/token", paras);

            JObject json = JObject.Parse(response);
           
            token.Access_Token1 = (string)json["access_token"];
            token.Expirse_In = (int)json["expires_in"];
            token.Mall_Type_ID = this.Mall_Type_ID;
            token.Mall_User_ID = (string)json["taobao_user_id"];
            token.Mall_User_Name = (string)json["taobao_user_nick"];
            token.Refresh_Token = (string)json["refresh_token"];
            token.RExpirse_In = (int)json["re_expires_in"];            
            return token;
        }

        public Access_Token RefreshToken(Access_Token oldToken)
        {
            throw new NotImplementedException();
        }
    }
}
