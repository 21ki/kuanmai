using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GridMvc.Pagination;
namespace GridMvc.DBGrid
{
    public class DBGridPager<T>: BaseGridPager
    {
        public DBGridPager(PageItemsResult<T> result)
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