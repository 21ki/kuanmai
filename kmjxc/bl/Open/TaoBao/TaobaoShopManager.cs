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
using KM.JXC.Common.Util;

using TB=Top.Api.Domain;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;
namespace KM.JXC.BL.Open.TaoBao
{
    internal class TaoBaoShopManager:OBaseManager,IOShopManager
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
                shop = new Shop();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Product_Class> GetCategories(BUser user)
        {
            List<Product_Class> categories = new List<Product_Class>();
            SellercatsListGetRequest req = new SellercatsListGetRequest();
            req.Nick = user.Mall_Name;
            SellercatsListGetResponse response = client.Execute(req);
            if (response.IsError)
            {
                throw new KMJXCException(response.ErrMsg);
            }

            if (response.SellerCats != null)
            {
                foreach (TB.SellerCat cat in response.SellerCats)
                {
                    Product_Class category = new Product_Class();
                    category.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    category.Create_User_ID = user.ID;
                    category.Enabled = true;
                    category.Mall_CID = cat.Cid.ToString();
                    category.Mall_PCID = cat.ParentCid.ToString();
                    category.Name = cat.Name;
                    category.Parent_ID = 0;
                    category.Product_Class_ID = 0;
                    categories.Add(category);
                }
            }
            return categories;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public List<BProperty> GetProperities(Product_Class category,Shop shop)
        {
            List<BProperty> properities = new List<BProperty>();
            ItempropsGetRequest req = new ItempropsGetRequest();
            req.Fields = "pid,name,must,multi,prop_values";
            if (category != null && !string.IsNullOrEmpty(category.Mall_CID))
            {
                req.Cid = long.Parse(category.Mall_CID);
            }
            else 
            {
                req.Cid = 0;
            }
            //req.IsKeyProp = true;
            req.IsSaleProp = true;
            req.IsColorProp = true;
            req.IsEnumProp = true;
            req.IsInputProp = true;
            req.IsItemProp = true;   
            //1=>Taobao
            //2=>TMall, need to check Mall Type from Shop object
            req.Type = 1L;
         
            ItempropsGetResponse response = client.Execute(req);
            if (response.IsError)
            {
                throw new KMJXCException(response.ErrMsg);
            }

            if (response.ItemProps != null)
            {
                foreach (TB.ItemProp prop in response.ItemProps)
                {
                    BProperty pro = new BProperty();
                    pro.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    pro.MID = prop.Pid.ToString();
                    pro.Name = prop.Name;
                    pro.CategoryId = category.Product_Class_ID;
                    pro.ID = 0;
                    pro.Shop = new BShop() { ID = category.Shop_ID };
                    pro.Created_By = new BUser() { ID = category.Create_User_ID };
                    properities.Add(pro);
                    if (prop.PropValues != null)
                    {
                        foreach (TB.PropValue pv in prop.PropValues)
                        {
                            Product_Spec_Value psv = new Product_Spec_Value();
                            psv.Name = pv.Name;
                            pro.Values.Add(psv);
                        }
                    }
                }
            }

            return properities;
        }
    }
}
