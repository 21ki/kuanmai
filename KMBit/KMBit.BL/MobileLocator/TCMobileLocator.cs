using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using Newtonsoft.Json.Linq;
namespace KMBit.BL.MobileLocator
{
    public class TCMobileLocator:HttpService,IMobileLocator
    {
        private static string ApiURL = "http://v.showji.com/Locating/showji.com20150416273007.aspx";
        public TCMobileLocator():base(ApiURL)
        {

        }

        public BMobileLocation GetMobileLocation(string phone)
        {
            BMobileLocation location = null;
            List<WebRequestParameters> parameters = new List<WebRequestParameters>();
            parameters.Add(new WebRequestParameters("m", phone,false));
            //parameters.Add(new WebRequestParameters("amount", "10000", false));
            bool succeed = false;
            SendRequest(parameters,false,out succeed, RequestType.GET);
            //JObject json = JObject.Parse(this.Response);
            location = new BMobileLocation();
            return location;
        }
    }
}
