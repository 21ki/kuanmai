using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GridMvc;
using GridMvc.Pagination;
using KMBit.Beans;
namespace KMBit.Grids
{
    public class KMGrid<K>:GridMvc.Grid<K> where K:class
    {
        public KMGrid(PageItemsResult<K> result):base(result.Items.AsQueryable())
        {           
            Pager = new KMGridPager<K>(result);
            EnablePaging = true;
            Pager= new KMGridPager<K>(result);
        }

        protected override IEnumerable<K> GetItemsToDisplay()
        {
            Console.WriteLine(this.BeforeItems);
           
            base.PrepareItemsToDisplay();
            return this.AfterItems;
        }        
    }
}