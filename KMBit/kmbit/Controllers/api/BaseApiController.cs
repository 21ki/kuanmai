using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;

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
    }
}
