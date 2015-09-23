using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KMBit.Controllers.api
{
    public class ApiMessage
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Item { get; set; }
    }
}