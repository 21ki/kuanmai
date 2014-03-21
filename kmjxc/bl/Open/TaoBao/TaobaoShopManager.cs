using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
namespace KM.JXC.BL.Open.TaoBao
{
    public class TaoBaoShopManager:BaseManager,IShopManager
    {
        public Mall_Type MallType { get; set; }
        public Access_Token Access_Token { get; set; }
        public Open_Key Open_Key { get; set; }
        public TaoBaoShopManager(Access_Token token,int mall_type_id)
            : base(mall_type_id)
        {
            this.Access_Token = token;
            this.Open_Key = this.GetAppKey();
            this.MallType = this.GetMallType();
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
        public Shop GetShop(string mall_user_id,string mall_user_name)
        {
            Shop shop = null;

            return shop;
        }
    }
}
