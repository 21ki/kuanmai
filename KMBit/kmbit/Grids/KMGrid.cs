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
            IGridPager p = new KMGridPager<K>(result);
            Pager = p;
            EnablePaging = true;
        }

        protected override IEnumerable<K> GetItemsToDisplay()
        {
            Console.WriteLine(this.BeforeItems);
           
            base.PrepareItemsToDisplay();
            return this.AfterItems;
        }        
    }
}