using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
namespace KM.JXC.BL.Open.TaoBao
{
    public class TaobaoShopManager:IShopManager
    {
        public TaobaoShopManager()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Shop GetShop(User user)
        {
            Shop shop = null;

            return shop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mall_shop_name"></param>
        /// <returns></returns>
        public Shop GetShop(string mall_shop_name)
        {
            Shop shop = null;

            return shop;
        }
    }
}
