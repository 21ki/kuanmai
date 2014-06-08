using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Models;

namespace KM.JXC.BL.Open.Interface
{
    public interface IOProductManager
    {
        List<BMallProduct> GetOnSaleProducts(BUser user, Shop shop,long pageIndex,long pageSize,out long total);
        bool MappingSku(string outer_id, string mall_sku_id,string mall_item_id, string properities);
        bool MappingProduct(string outer_id, string mall_item_id);
        bool UpdateProductQuantity(string mall_item_id, string sku_id,string outer_id, long quantity);       
    }
}
