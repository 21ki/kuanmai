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
using log4net;
namespace KMBit.Controllers.api
{
    public class BaseApiController : ApiController
    {
        protected HttpContextBase context { get; private set; }
        protected HttpRequestBase request { get; private set; }

        protected ILog logger = KMBit.BL.KMLogger.GetLogger();
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
            try
            {
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
                if (formKeys != null)
                {
                    for (int j = 0; j < formKeys.Length; j++)
                    {
                        sArray.Add(formKeys[j], request[formKeys[j]]);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
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
                logger.Info("Parameter:"+parameter.Key);
                if (parameter.Key.ToLower() == "sign")
                {
                    sign = parameter.Value;
                }
                else if (parameter.Key.ToLower() == "token")
                {
                    accesstoken = parameter.Value;
                }
                
                if(parameter.Key.ToLower()!="sign")
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
            logger.Info("Parameters have been handled.");
            queryStr = str.ToString();
            return ;
        }
    }
}
