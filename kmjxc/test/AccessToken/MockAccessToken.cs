using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KM.JXC.Common.Util;
using KM.JXC.Open.Interface;
using KM.JXC.DBA;
using KM.JXC.BL;
using KM.JXC.Open;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace test.AccessToken
{
    public class MockAccessToken:BaseAccessToken,IAccessToken
    {
        public MockAccessToken(long mall_type_id)
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
