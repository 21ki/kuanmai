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

using TB = Top.Api.Domain;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;

namespace KM.JXC.BL.Open.TaoBao
{
    internal class TaobaoProductManager : OBaseManager,IOProductManager
    {
        public TaobaoProductManager(Access_Token token, int mall_type_id)
            : base(mall_type_id,token)
        {
           
        }

        /// <summary>
        /// Update on sale product stock
        /// </summary>
        /// <param name="mall_item_id"></param>
        /// <param name="sku_id"></param>
        /// <param name="outer_id"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public bool UpdateProductQuantity(string mall_item_id, string sku_id,string outer_id,long quantity)
        {
            bool ret = false;

            ItemQuantityUpdateRequest req = new ItemQuantityUpdateRequest();
            req.NumIid = long.Parse(mall_item_id);
            if (!string.IsNullOrEmpty(sku_id))
            {
                req.SkuId = long.Parse(sku_id);
            }

            if (!string.IsNullOrEmpty(outer_id))
            {
                req.OuterId = outer_id;
            }
            req.Quantity = quantity;
            req.Type = 1L;
            ItemQuantityUpdateResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if (!response.IsError)
            {
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outer_id"></param>
        /// <param name="mall_sku_id"></param>
        /// <returns></returns>
        public bool MappingSku(string outer_id, string mall_sku_id,string mall_item_id,string properities)
        {
            bool ret = false;
           
            TB.Sku sku = null;
            if (string.IsNullOrEmpty(properities))
            {
                sku = this.GetSku(mall_sku_id);
                if (sku != null)
                {
                    properities = sku.Properties;
                }
            }

            ItemSkuUpdateRequest req = new ItemSkuUpdateRequest();
            req.NumIid = long.Parse(mall_item_id);
            req.Properties = properities;
            req.OuterId = outer_id;
            ItemSkuUpdateResponse response= this.client.Execute(req,this.Access_Token.Access_Token1);
            if (response.IsError)
            {
                throw new KMJXCException("关联本地产品与商城宝贝出错");
            }
            ret = true;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sku_id"></param>
        /// <returns></returns>
        private TB.Sku GetSku(string sku_id)
        {
            TB.Sku sku = null;

            ItemSkuGetRequest req = new ItemSkuGetRequest();
            req.Fields = "sku_id,iid,properties,quantity,price,outer_id,created,modified,status";
            req.SkuId = long.Parse(sku_id); 
            ItemSkuGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);

            if (!response.IsError && response.Sku!=null)
            {
                sku = response.Sku;
            }

            return sku;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outer_id"></param>
        /// <param name="mall_item_id"></param>
        /// <returns></returns>
        public bool MappingProduct(string outer_id, string mall_item_id)
        {
            ItemUpdateRequest req=new ItemUpdateRequest();
            req.NumIid = long.Parse(mall_item_id);
            req.OuterId = outer_id;
            ItemUpdateResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if (response.IsError)
            {
                throw new KMJXCException("进销存产品与商城宝贝绑定失败");
            }

            if (response.Item == null)
            {
                throw new KMJXCException("进销存产品与商城宝贝绑定失败");
            }

            if (response.Item.OuterId == outer_id)
            {
                return true;
            }

            return false;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        public List<BMallProduct> GetOnSaleProducts(BUser user, Shop shop, long pageIndex,long pageSize, out long total)
        {
            List<BMallProduct> products = new List<BMallProduct>();
            total = 0;
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 50;
            }
            List<TB.Item> items = GetItems(out total, pageIndex, pageSize);

            if (items == null) {
                return products;
            }

            foreach (TB.Item item in items)
            {
                BMallProduct product = new BMallProduct();
                product.Created = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(item.Created));
                product.ID = item.NumIid.ToString();
                product.Modified = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(item.Modified));
                product.OuterID = item.OuterId;
                product.PicUrl = item.PicUrl;
                product.Price = item.Price;
                product.Quantity = item.Num;
                product.Description = item.Desc;
                product.Shop = shop;                
                if (product.Skus == null)
                {
                    product.Skus = new List<BMallSku>();
                }

                if (item.Skus != null)
                {
                    foreach (TB.Sku sku in item.Skus)
                    {
                        BMallSku msku = new BMallSku();
                        msku.OuterID = sku.OuterId;
                        msku.MallProduct_ID = product.ID;
                        msku.PropertiesName = sku.PropertiesName;
                        msku.Quantity = sku.Quantity;
                        msku.SkuID = sku.SkuId.ToString();
                        msku.Properities = sku.Properties;
                        if (!string.IsNullOrEmpty(sku.Price))
                        {
                            msku.Price = double.Parse(sku.Price);
                        }
                        product.Skus.Add(msku);
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="total"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        private List<TB.Item> GetItems(out long total, long pageNo, long pageSize)
        {
            total = 0;
            ItemsOnsaleGetRequest req = new ItemsOnsaleGetRequest();
            req.Fields = "num_iid,title,price,pic_url,outer_id,approve_status,num,created,modified,skus";           
            req.PageNo = pageNo;         
            req.OrderBy = "list_time:desc";
            req.IsTaobao = true;
            req.IsEx = true;
            req.PageSize = pageSize;
           
            ItemsOnsaleGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if(!response.IsError)
            {
                total = response.TotalResults;
                return response.Items;
            }

            return null;
        }
    }
}
