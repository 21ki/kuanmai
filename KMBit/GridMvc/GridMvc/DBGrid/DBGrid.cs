using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GridMvc;
using GridMvc.Pagination;

namespace GridMvc.DBGrid
{
    public class DBGrid<K>:GridMvc.Grid<K> where K:class
    {
        public DBGrid(PageItemsResult<K> result):base(result.Items.AsQueryable())
        {           
            Pager = new DBGridPager<K>(result);
            EnablePaging = result.EnablePaging;
            Pager= new DBGridPager<K>(result);
        }   
    }
}