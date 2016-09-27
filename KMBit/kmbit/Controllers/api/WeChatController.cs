using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.BL.Agent;
using KMBit.DAL;
using KMBit.Util;

namespace KMBit.Controllers.api
{
    public class WeChatController : ApiController
    {
        public ApiMessage GetPaySign()
        {
            ApiMessage message = new ApiMessage();

            return message;
        }
    }
}
