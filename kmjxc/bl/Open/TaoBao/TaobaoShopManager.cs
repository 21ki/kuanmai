using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;
using KM.JXC.Common.KMException;
using KM.JXC.Common;

using Top.Api;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;
namespace KM.JXC.BL.Open.TaoBao
{
    public class TaoBaoShopManager:OBaseManager,IShopManager
    {
        public TaoBaoShopManager(Access_Token token,int mall_type_id)
            : base(mall_type_id,token)
        {
           
        }

        /// <summary>
        /// Get shop from Mall
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Shop object</returns>
        public Shop GetShop(BUser user)
        {
            Shop shop = null;
            ShopGetRequest req = new ShopGetRequest();
            req.Fields = "sid,cid,title,nick,desc,bulletin,pic_path,created,modified";
            req.Nick = user.Mall_Name;           
            ShopGetResponse response = client.Execute(req);
            if (response.IsError)
            {
                throw new KMJXCException("从"+this.MallType.Name+" 获取 "+user.Mall_Name +"的店铺信息失败");
            }

            if (response.Shop != null)
            {
                shop.Description = response.Shop.Desc;
                shop.Name = response.Shop.Title;
                shop.Mall_Shop_ID = response.Shop.Sid.ToString();
                shop.Mall_Type_ID = this.MallType.Mall_Type_ID;
                shop.Parent_Shop_ID = 0;
                shop.Shop_ID = 0;
                shop.User_ID = user.ID;
            }
            return shop;
        }
    }
}
