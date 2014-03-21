using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.DBA;
using KM.JXC.BL;
using KM.JXC.BL.Open;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace test.AccessToken
{
    public class MockAccessToken:BaseAccessToken,IAccessToken
    {
        public Open_Key OpenKey { get; set; }

        public MockAccessToken(int mall_type_id)
            : base(mall_type_id)
        { 
        
        }

        public Access_Token RequestAccessToken(string code)
        {
            Access_Token token = new Access_Token();

            return token;
        }
    }
}
