using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GridMvc.Pagination;
using KMBit.Beans;
namespace KMBit.Grids
{
    public class KMGridPager<T>: BaseGridPager
    {
        public KMGridPager(PageItemsResult<T> result)
        {
            if(result!=null)
            {
                CurrentPage = result.CurrentPage;
                PageSize = result.PageSize;
                ItemsCount = result.TotalRecords;
            }           
           
            ParameterName = result.PageQueryParameterName;
            CurrentPage = result.CurrentPage;
        }
    }
}