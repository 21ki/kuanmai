using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class PageItemsResult<T>
    {
        public PageItemsResult()
        {
            PageQueryParameterName = "page";
        }
        public List<T> Items{ get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecords { get; set; }
        public string PageQueryParameterName { get; set; }
    }
}
