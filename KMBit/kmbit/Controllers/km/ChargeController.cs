using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.Controllers.api;
namespace KMBit.Controllers.km
{
    public class ChargeController : BaseApiController
    {
        [HttpPost]
        public ApiMessage Push()
        {
            ApiMessage message = new ApiMessage();

            return message;
        }
    }
}
