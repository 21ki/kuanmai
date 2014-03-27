using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Models;
namespace KM.JXC.BL.Open.Interface
{
    public interface IOShopManager
    {
        KM.JXC.DBA.Shop GetShop(BUser user);
        List<Product_Class> GetCategories(BUser user);
        List<Product_Spec> GetProperities(Product_Class category, Shop shop);
    }
}
