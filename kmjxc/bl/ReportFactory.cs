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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="product_id"></param>
        /// <returns></returns>
        public string GetSalesReport(int startDate, int endDate, int product_id)
        {
            StringBuilder json = new StringBuilder("[");
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from sale in db.Sale_Detail
                          select sale;

                var sales = from s in db.Sale
                            select s;
                int[] childShops = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                List<Product> products = (from p in db.Product
                                          where (p.Shop_ID == this.Shop.Shop_ID || p.Shop_ID == this.Main_Shop.Shop_ID || childShops.Contains(p.Shop_ID)) && p.Parent_ID==0
                                          select p).ToList<Product>();
                int[] product_ids=(from p in products select p.Product_ID).ToArray<int>();
                int[] child_product_ids = (from p in db.Product where product_ids.Contains(p.Parent_ID) select p.Product_ID).ToArray<int>();

                List<BProductProperty> childs = (from prop in db.Product_Specifications
                                                 join ps in db.Product_Spec on prop.Product_Spec_ID equals ps.Product_Spec_ID
                                                 join psv in db.Product_Spec_Value on prop.Product_Spec_Value_ID equals psv.Product_Spec_Value_ID
                                                 where child_product_ids.Contains(prop.Product_ID)
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
                    sales = sales.Where(s => s.Created >= startDate);
                }

                if (endDate>0)
                {
                    sales = sales.Where(s => s.Created < endDate);
                }

                if (product_id > 0)
                {                   
                    tmp = tmp.Where(s => (s.Product_ID == product_id || s.Parent_Product_ID==product_id));
                }

                var saleids=from s in sales select s.Mall_Trade_ID;
                tmp = tmp.Where(s => saleids.Contains(s.Mall_Trade_ID));

                var saleObj = from saled in tmp
                              join sale in db.Sale on saled.Mall_Trade_ID equals sale.Mall_Trade_ID into lSale
                              from l_sale in lSale.DefaultIfEmpty()
                              select new
                              {
                                  Created=l_sale.Created,
                                  Quantity=saled.Quantity,
                                  Amount=saled.Amount,
                                  ProductID=saled.Product_ID,
                                  ParentProductID=saled.Parent_Product_ID,
                                  ShopId=l_sale.Shop_ID
                              };

                saleObj = saleObj.OrderBy(s => s.Created);

                int total = saleObj.Count();
                int count = 1;
                foreach (var item in saleObj)
                {
                    //if (item.ProductID == 0 && item.ParentProductID == 0)
                    //{
                    //    count++;
                    //    continue;
                    //}

                    string productName = "";
                    if (item.ParentProductID > 0)
                    {
                        productName = (from p in products where p.Product_ID == item.ParentProductID select p.Name).FirstOrDefault<string>();
                    }
                    else
                    {
                        productName = (from p in products where p.Product_ID == item.ProductID select p.Name).FirstOrDefault<string>();
                    }
                    if (productName == null)
                    {
                        productName = "没有关联到进销存的产品";
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
                    else if(childShops.Contains(item.ShopId))
                    {
                        shopName=(from s in this.ChildShops where s.Shop_ID==item.ShopId select s.Name).FirstOrDefault<string>();
                    }

                    DateTime time = DateTimeUtil.ConvertToDateTime(item.Created);
                    string month =time.Year.ToString()+"-"+ time.Month.ToString();

                    string jobj = "{\"ProductName\":\""+productName+"\",\"ShopName\":\""+shopName+"\",\"Month\":\""+month+"\",\"Quantity\":\""+item.Quantity+"\",\"Amount\":\""+item.Amount+"\"}";

                    if (count == 1)
                    {
                        json.Append(jobj);
                    }
                    else
                    {
                        json.Append(","+jobj);
                    }
                    count++;
                }

                json.Append("]");
            }
            return json.ToString();
        }
    }
}
