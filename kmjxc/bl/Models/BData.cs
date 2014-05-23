using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BPageData
    {
        public long TotalRecords { get; set; }
        public Object Data { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string URL { get; set; }
    }
}
