using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;
using System.Text;

namespace KMBit.Controllers.api
{
    public class BaseApiController : ApiController
    {
        protected HttpContextBase context { get; private set; }
        protected HttpRequestBase request { get; private set; }
        public BaseApiController()
        {
           
        }   
        
        protected void IniRequest()
        {
            this.context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            this.request = context.Request;
        }

        public SortedDictionary<string, string> GetRequestParameters()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], request[requestItem[i]]);
            }

            string[] formKeys = request.Form.AllKeys;
            if(formKeys!=null)
            {
                for(int j=0;j<formKeys.Length;j++)
                {
                    sArray.Add(formKeys[j], request[formKeys[j]]);
                }
            }

            return sArray;
        }

        public void ParseSigantures(out string sign, out string accesstoken,out string queryStr)
        {
            sign = "";
            accesstoken = "";
            queryStr = "";
           
            SortedDictionary< string, string>  parameters= GetRequestParameters();
            if(parameters==null || parameters.Count==0)
            {
                return;
            }
            StringBuilder str = new StringBuilder();          
            
            foreach(KeyValuePair<string,string> parameter in parameters)
            {
                if (parameter.Key == "Sign")
                {
                    sign = parameter.Value;
                }
                else if (parameter.Key == "Token")
                {
                    accesstoken = parameter.Value;
                }
                
                if(parameter.Key!="Sign")
                {
                    if (str.ToString() == "")
                    {
                        str.Append(parameter.Key);
                        str.Append("=");
                        str.Append(parameter.Value);
                    }
                    else
                    {
                        str.Append("&");
                        str.Append(parameter.Key);
                        str.Append("=");
                        str.Append(parameter.Value);
                    }
                }
            }
            queryStr = str.ToString();
            return ;
        }
    }
}
