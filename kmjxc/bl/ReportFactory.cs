using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Open;

namespace KM.JXC.BL
{
    public class ReportFactory : BBaseManager
    {
        public ReportFactory(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
        }

        public string ExportBuyOrders(List<BBuyOrder> orders)
        {
            string path = "";
            return path;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="product_id"></param>
        /// <returns></returns>
        public string GetSalesReport(long startDate, long endDate, string[] product_id, int page, int pageSize, out int totalProducts, bool paging = true, bool includeNoSales = false)
        {
            totalProducts = 0;
            StringBuilder json = new StringBuilder("[");
            if (page <= 0)
            {
                page = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 50;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from sale in db.Sale_Detail
                          where sale.Status1 != (int)SaleDetailStatus.BACK_STOCK && sale.Status1 != (int)SaleDetailStatus.REFOUND_BEFORE_SEND
                          select sale;

                var sales = from s in db.Sale
                            select s;
                int[] childShops = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                List<Mall_Product> products=null;
                List<Product> localProducts = (from lp in db.Product
                                               where (lp.Shop_ID == this.Shop.Shop_ID || lp.Shop_ID == this.Main_Shop.Shop_ID || childShops.Contains(lp.Shop_ID))
                                               select lp).ToList<Product>();
                var tmpProducts = from p in db.Mall_Product orderby p.Mall_ID ascending
                                  where (p.Shop_ID == this.Shop.Shop_ID || p.Shop_ID == this.Main_Shop.Shop_ID || childShops.Contains(p.Shop_ID))
                                  select p;

                if (product_id != null && product_id.Length > 0)
                {
                    tmpProducts=tmpProducts.Where(p => product_id.Contains(p.Mall_ID));                    
                }
               
                int[] local_product_ids = (from p in localProducts select p.Product_ID).ToArray<int>();               
                int [] local_child_product_ids=(from p in db.Product where local_product_ids.Contains(p.Parent_ID) select p.Product_ID).ToArray<int>();
                List<BProductProperty> childs = (from prop in db.Product_Specifications
                                                 join ps in db.Product_Spec on prop.Product_Spec_ID equals ps.Product_Spec_ID
                                                 join psv in db.Product_Spec_Value on prop.Product_Spec_Value_ID equals psv.Product_Spec_Value_ID
                                                 where local_child_product_ids.Contains(prop.Product_ID)
                                                 select new BProductProperty
                                                 {
                                                     ProductID = prop.Product_ID,
                                                     PID = prop.Product_Spec_ID,
                                                     PName = ps.Name,
                                                     PVID = prop.Product_Spec_Value_ID,
                                                     PValue = psv.Name
                                                 }).ToList<BProductProperty>();

                

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {                       
                    sales = sales.Where(s=>(s.Shop_ID==this.Shop.Shop_ID || childShops.Contains(s.Shop_ID)));                    
                }                
                else
                {
                    sales = sales.Where(s=>s.Shop_ID==this.Shop.Shop_ID);
                }

                if (startDate>0)
                {
                    sales = sales.Where(s => s.Sale_Time >= startDate);
                }

                if (endDate>0)
                {
                    sales = sales.Where(s => s.Sale_Time < endDate);
                }

                string[] saleids=(from s in sales select s.Mall_Trade_ID).ToArray<string>();
                string[] mallSoldProducts= (from msp in db.Sale_Detail where saleids.Contains(msp.Mall_Trade_ID) select msp.Mall_PID).Distinct().ToArray<string>();
                if (mallSoldProducts != null && mallSoldProducts.Length>0)
                {
                    tmpProducts = tmpProducts.Where(mp => mallSoldProducts.Contains(mp.Mall_ID));
                }
                
                totalProducts = tmpProducts.Count();
                if (paging)
                {
                    tmpProducts = tmpProducts.Skip((page - 1) * pageSize).Take(pageSize);
                }
                products = tmpProducts.ToList<Mall_Product>();
                string[] product_ids = (from p in products select p.Mall_ID).ToArray<string>();
                string[] child_product_ids = (from p in db.Mall_Product_Sku where product_ids.Contains(p.Mall_ID) select p.SKU_ID).ToArray<string>();
                List<Mall_Product_Sku> skus = (from sku in db.Mall_Product_Sku where child_product_ids.Contains(sku.SKU_ID) select sku).ToList<Mall_Product_Sku>();

                
                tmp = tmp.Where(s => saleids.Contains(s.Mall_Trade_ID)).Where(s => (product_ids.Contains(s.Mall_PID)));

                var saleObj = from saled in tmp
                              join sale in db.Sale on saled.Mall_Trade_ID equals sale.Mall_Trade_ID into lSale
                              from l_sale in lSale.DefaultIfEmpty()
                              select new
                              {
                                  SaleTime=l_sale.Sale_Time,
                                  Created=l_sale.Created,
                                  Quantity=saled.Quantity,
                                  Amount=saled.Amount,
                                  ProductID=saled.Product_ID,
                                  ParentProductID=saled.Parent_Product_ID,
                                  ShopId=l_sale.Shop_ID,
                                  MallSkuID=saled.Mall_SkuID,
                                  MallPID=saled.Mall_PID
                              };

                saleObj = saleObj.OrderBy(s => s.Created);

                int total = saleObj.Count();
                bool firstRow = true;
                foreach (Mall_Product pdt in products)
                {
                    var items = from sd in saleObj where sd.MallPID == pdt.Mall_ID select sd;
                    if (items.Count() == 0)
                    {
                        if (includeNoSales)
                        {
                            string jobj = "{\"ProductName\":\"" + pdt.Title + "\",\"PropName\":\"\",\"ShopName\":\"\",\"Month\":\"\",\"Quantity\":\"0\",\"Amount\":\"0\"}";
                            if (firstRow)
                            {
                                firstRow = false;
                                json.Append(jobj);
                            }
                            else
                            {
                                json.Append(","+jobj);
                            }
                        }
                        continue;
                    }
                    foreach (var item in items)
                    {
                        string productName = "";
                        Mall_Product mpdt=(from p in products where p.Mall_ID==item.MallPID select p).FirstOrDefault<Mall_Product>();
                        Product locpdt=(from p in localProducts where p.Product_ID==item.ParentProductID select p).FirstOrDefault<Product>();
                        if (locpdt != null)
                        {
                            productName = locpdt.Name;
                        }
                        else 
                        {
                            productName = "未关联";
                        }

                        if (mpdt != null)
                        {
                            productName += "("+mpdt.Title+")";
                        }

                        string shopName = "";
                        if (item.ShopId == this.Shop.Shop_ID)
                        {
                            shopName = this.Shop.Name;
                        }
                        else if (item.ShopId == this.Main_Shop.Shop_ID)
                        {
                            shopName = this.Main_Shop.Name;
                        }
                        else if (childShops.Contains(item.ShopId))
                        {
                            shopName = (from s in this.DBChildShops where s.Shop_ID == item.ShopId select s.Name).FirstOrDefault<string>();
                        }

                        if (shopName == null)
                        {
                            shopName = "";
                        }

                        string propNames = "";

                        if (item.ProductID > 0)
                        {
                            foreach (var prop in (from p in childs where p.ProductID == item.ProductID select p))
                            {
                                if(!string.IsNullOrEmpty(prop.PName) && !string.IsNullOrEmpty(prop.PValue))
                                if (propNames == "")
                                {
                                    propNames = prop.PName + ":" + prop.PValue;
                                }
                                else
                                {
                                    propNames += ";" + prop.PName + ":" + prop.PValue;
                                }
                            }
                        }
                        else
                        {
                            Mall_Product_Sku sku=(from s in skus where s.Mall_ID==item.MallPID && s.SKU_ID==item.MallSkuID select s).FirstOrDefault<Mall_Product_Sku>();
                           
                            if (sku != null)
                            {
                                propNames = sku.Properties_name;
                                if (!string.IsNullOrEmpty(propNames))
                                {
                                     string[] pops = sku.Properties.Split(';');
                                     foreach (string p in pops)
                                     {
                                         propNames = propNames.Replace(p + ":", "");
                                     }
                                }
                            }
                        }
                        
                        if (item.SaleTime <= 0)
                        {
                            continue;
                        }


                        if (propNames =="") {
                            propNames = "--";
                        }
                        DateTime time = DateTimeUtil.ConvertToDateTime(item.SaleTime);
                        string month = time.Year.ToString() + "-" + time.Month.ToString();

                        string jobj = "{\"ProductName\":\"" + productName + "\",\"PropName\":\""+propNames+"\",\"ShopName\":\"" + shopName + "\",\"Month\":\"" + month + "\",\"Quantity\":\"" + item.Quantity + "\",\"Amount\":\"" + item.Amount + "\"}";

                        if (firstRow)
                        {
                            firstRow = false;
                            json.Append(jobj);
                        }
                        else
                        {
                            json.Append(","+jobj);
                        }
                    }
                }

                json.Append("]");
            }
            return json.ToString();
        }
    }
}
