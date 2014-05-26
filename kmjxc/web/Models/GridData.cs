using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KM.JXC.Web.Models
{
    public class PQGridData
    {
        public long totalRecords { get; set; }
        public long curPage { get; set; }
        public object data { get; set; }
    }
}