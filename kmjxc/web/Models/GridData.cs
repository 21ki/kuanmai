using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KM.JXC.Web.Models
{
    public class PQGridData
    {
        public int totalRecords { get; set; }
        public int curPage { get; set; }
        public object data { get; set; }
    }
}