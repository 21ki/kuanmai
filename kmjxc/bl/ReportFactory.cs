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
        /// Gets stock report in json format
        /// </summary>
        /// <param name="products">A array of products' id</param>
        /// <param name="page">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="total">Total records</param>
        /// <param name="paging">Paging</param>
        /// <returns>Json string</returns>
        public string GetStockReport(int[] products,int page,int pageSize,out int total,bool paging=true)
        {
            if (this.CurrentUserPermission.VIEW_STOCK_REPORT == 0)
            {
                throw new KMJXCException("没有权限查看库存报表");
            }
            string json="";
            total = 0;
            if (page <= 0)
            {
                page = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var stocks = from stock in db.Stock_Pile
                          select stock;

                var pdts = from product in db.Product
                           join shop in db.Shop on product.Shop_ID equals shop.Shop_ID into Lshop
                           from l_shop in Lshop.DefaultIfEmpty()
                           where product.Parent_ID==0
                           select new BProduct
                           {
                               ID = product.Product_ID,
                               Title = product.Name,
                               BShop = l_shop != null ? new BShop { ID = product.Shop_ID, Title = l_shop.Name } : new BShop { ID = 0, Title = "" }
                           };

                if (products != null && products.Length > 0)
                {
                    pdts = pdts.Where(p=>products.Contains(p.ID));
                }
                List<BProduct> dbproducts = null;
                total = pdts.Count();
                if (paging)
                {
                    dbproducts = pdts.OrderBy(p => p.ID).Skip((page - 1) * pageSize).Take(pageSize).ToList<BProduct>();
                }
                else
                {
                    dbproducts = pdts.ToList<BProduct>();
                }
                int[] pPIds=(from p in dbproducts select p.ID).ToArray<int>();
                List<Product> dbChildrenProducts = (from p in db.Product where pPIds.Contains(p.Parent_ID) select p).ToList<Product>();
                int[] cPIds = (from p in dbChildrenProducts select p.Product_ID).ToArray<int>();
                List<Stock_Pile> sPiles=(from s in stocks where pPIds.Contains(s.Product_ID) || cPIds.Contains(s.Product_ID) select s).ToList<Stock_Pile>();
                List<BProductProperty> properties = (from pv in db.Product_Specifications
                                                     join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                     from l_prop in LProp.DefaultIfEmpty()
                                                     join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                     from l_propv in LPropv.DefaultIfEmpty()
                                                     where cPIds.Contains(pv.Product_ID)
                                                     select new BProductProperty
                                                     {
                                                         PID = pv.Product_Spec_ID,
                                                         PName = l_prop.Name,
                                                         ProductID = pv.Product_ID,
                                                         PValue = l_propv.Name,
                                                         PVID = pv.Product_Spec_Value_ID
                                                     }).OrderBy(p=>p.PID).ToList<BProductProperty>();

                foreach (BProduct pdt in dbproducts)
                {
                    string sJson = "";
                    string productName = pdt.ID+" "+pdt.Title;
                    string propName = "";
                    int quantity = 0;
                    string shopName = pdt.BShop.Title;
                    List<Product> children=(from p in dbChildrenProducts where p.Parent_ID==pdt.ID select p).ToList<Product>();
                    if (children == null || children.Count <= 0)
                    {
                        propName = "--";
                        quantity = (from s in sPiles where s.Product_ID == pdt.ID select s.Quantity).Sum();
                        sJson = "{\"product_name\":\"" + productName + "\",\"prop_name\":\"" + propName + "\",\"shop_name\":\"" + shopName + "\",\"quantity\":" + quantity.ToString() + ",\"pivot\":\"库存\"}";
                    }
                    else
                    {
                        foreach (Product child in children)
                        {
                            propName = "";
                            quantity = (from s in sPiles where s.Product_ID == child.Product_ID select s.Quantity).Sum();
                            List<BProductProperty> props=(from p in properties where p.ProductID==child.Product_ID select p).ToList<BProductProperty>();
                            foreach (BProductProperty prop in props)
                            {
                                if (propName == "")
                                {
                                    propName = prop.PName + ":" + prop.PValue;
                                }
                                else
                                {
                                    propName +=";"+ prop.PName + ":" + prop.PValue;
                                }                                
                            }

                            if (sJson == "")
                            {
                                sJson = "{\"product_name\":\"" + productName + "\",\"prop_name\":\"" + child.Product_ID +" "+ propName + "\",\"shop_name\":\"" + shopName + "\",\"quantity\":" + quantity.ToString() + ",\"pivot\":\"库存\"}";
                            }
                            else
                            {
                                sJson += ",{\"product_name\":\"" + productName + "\",\"prop_name\":\"" +child.Product_ID+" "+ propName + "\",\"shop_name\":\"" + shopName + "\",\"quantity\":" + quantity.ToString() + ",\"pivot\":\"库存\"}";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(sJson))
                    {
                        if (json == "")
                        {
                            json = "[" + sJson;
                        }
                        else
                        {
                            json += "," + sJson;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(json))
                {
                    json += "]";
                }
            }
            return json;
        }

        /// <summary>
        /// Gets sale report in json format
        /// </summary>
        /// <param name="startDate">Trade start date</param>
        /// <param name="endDate">Trade end date</param>
        /// <param name="product_id">A Array of on sale product id</param>
        /// <returns>Json string</returns>
        public string GetSalesReport(long startDate, long endDate, string[] product_id, int page, int pageSize, out int totalProducts, bool paging = true, bool includeNoSales = false)
        {
            if (this.CurrentUserPermission.VIEW_SALE_REPORT == 0)
            {
                throw new KMJXCException("没有权限查看销售报表");
            }
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
                          where (sale.Status1 != (int)SaleDetailStatus.BACK_STOCK && sale.Status1 != (int)SaleDetailStatus.REFOUND_BEFORE_SEND && sale.Status1 != (int)SaleDetailStatus.REFOUND_HANDLED && sale.Status1 != (int)SaleDetailStatus.REFOUNDED_WAIT_HANDLE)
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

                //string[] saleids=(from s in sales select s.Mall_Trade_ID).ToArray<string>();

                var saleids = from s in sales select s.Mall_Trade_ID;
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
                int total = tmp.Count();
                var saleObj = from saled in tmp
                              join sale in db.Sale on saled.Mall_Trade_ID equals sale.Mall_Trade_ID into lSale
                              from l_sale in lSale.DefaultIfEmpty()
                              join mpt in db.Mall_Product_Sku on saled.Mall_SkuID equals mpt.SKU_ID into LMpt
                              from l_mpt in LMpt.DefaultIfEmpty()
                              select new
                              {
                                  SaleTime = l_sale.Sale_Time,
                                  Created = l_sale.Created,
                                  Quantity = saled.Quantity,
                                  Amount = saled.Amount,
                                  ProductID = saled.Product_ID != null ? saled.Product_ID: l_mpt.Outer_ID ,
                                  ParentProductID = saled.Parent_Product_ID != null ? saled.Parent_Product_ID : 0,
                                  ShopId = l_sale.Shop_ID,
                                  MallSkuID = saled.Mall_SkuID,
                                  MallPID = saled.Mall_PID,
                                  Title=saled.Title
                              };

                saleObj = saleObj.OrderBy(s => s.Created);
                total = saleObj.Count();
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
                        Product locpdt=(from p in localProducts where p.Product_ID==item.ProductID select p).FirstOrDefault<Product>();
                        if (locpdt != null)
                        {
                            productName = locpdt.Name;
                        }
                        else 
                        {
                            if (mpdt != null)
                            {
                                productName = mpdt.Title;
                            }
                        }
                        if (string.IsNullOrEmpty(productName))
                        {
                            if (!string.IsNullOrEmpty(item.Title))
                            {
                                productName = item.Title;
                            }
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
                        string jobj = "{\"ProductName\":\"" + productName + "\",\"PropName\":\""+propNames+"\",\"ShopName\":\"" + shopName + "\",\"Month\":\"" + month + "\",\"Quantity\":\"" + item.Quantity + "\",\"Amount\":\"" + Math.Round((double)item.Amount,0) + "\"}";
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

        /// <summary>
        /// Gets buy orders report in json format
        /// </summary>
        /// <param name="startDate">Buy order start date</param>
        /// <param name="endDate">Buy order end date</param>
        /// <param name="product_id">A array of product ids</param>
        /// <param name="page">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalProducts">Total records according to the conditions</param>
        /// <param name="paging">Paging(true/false)</param>        
        /// <returns></returns>
        public string GetBuyReport(long startDate, long endDate, int[] product_id, int page, int pageSize, out int totalProducts, bool paging = true)
        {
            string json = "";
            totalProducts = 0;
            if (this.CurrentUserPermission.VIEW_BUY_REPORT == 0)
            {
                throw new KMJXCException("没有权限查看采购报表");
            }
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 20;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] childShops = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                List<BProduct> products = null;
                var tmpProducts = from p in db.Product
                                  join shop in db.Shop on p.Shop_ID equals shop.Shop_ID into LShop
                                  from l_shop in LShop.DefaultIfEmpty()
                                  where p.Parent_ID == 0
                                  select new BProduct
                                  {
                                      ID=p.Product_ID,
                                      BShop = new BShop { ID=p.Shop_ID,Title=l_shop.Name},
                                      Title=p.Name
                                  };

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    tmpProducts = tmpProducts.Where(p => (p.BShop.ID == this.Shop.Shop_ID || childShops.Contains(p.BShop.ID)));
                }
                else
                {
                    tmpProducts = tmpProducts.Where(p => p.BShop.ID == this.Shop.Shop_ID);
                }

                if (product_id != null && product_id.Length > 0)
                {
                    tmpProducts = tmpProducts.Where(p => product_id.Contains(p.ID));
                }

                var tmpBuyOrders = from bo in db.Buy_Order
                                   select bo;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    tmpBuyOrders = tmpBuyOrders.Where(o => (o.Shop_ID == this.Shop.Shop_ID || childShops.Contains(o.Shop_ID)));
                }
                else
                {
                    tmpBuyOrders = tmpBuyOrders.Where(o => o.Shop_ID == this.Shop.Shop_ID);
                }

                if (startDate > 0)
                {
                    tmpBuyOrders = tmpBuyOrders.Where(o => o.Create_Date > startDate);
                }
                if (endDate > 0)
                {
                    tmpBuyOrders = tmpBuyOrders.Where(o => o.Create_Date <= endDate);
                }

                var tmpBuyOrderIds = from o in tmpBuyOrders select o.Buy_Order_ID;

                var tmpBuyOrderDetails = from bd in db.Buy_Order_Detail
                                    where tmpBuyOrderIds.Contains(bd.Buy_Order_ID)
                                    select bd;

                var orderProductIds = from o in tmpBuyOrderDetails select o.Product_ID;
                tmpProducts = tmpProducts.Where(p=>orderProductIds.Contains(p.ID));
                if (paging)
                {
                    products = tmpProducts.OrderBy(p => p.ID).Skip((page - 1) * page).Take(pageSize).ToList<BProduct>();
                }
                else
                {
                    products = tmpProducts.OrderBy(p => p.ID).ToList<BProduct>();
                }
                int[] parent_pIds = (from p in products select p.ID).ToArray<int>();
                List<Product> childrenProducts = (from p in db.Product where parent_pIds.Contains(p.Parent_ID) select p).ToList<Product>();
                int[] child_pIds = (from p in childrenProducts select p.Product_ID).ToArray<int>();
                List<BProductProperty> properties = (from pv in db.Product_Specifications
                                                     join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                     from l_prop in LProp.DefaultIfEmpty()
                                                     join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                     from l_propv in LPropv.DefaultIfEmpty()
                                                     where child_pIds.Contains(pv.Product_ID)
                                                     select new BProductProperty
                                                     {
                                                         PID = pv.Product_Spec_ID,
                                                         PName = l_prop.Name,
                                                         ProductID = pv.Product_ID,
                                                         PValue = l_propv.Name,
                                                         PVID = pv.Product_Spec_Value_ID
                                                     }).OrderBy(p => p.PID).ToList<BProductProperty>();

                var details = from detail in tmpBuyOrderDetails
                              join order in db.Buy_Order on detail.Buy_Order_ID equals order.Buy_Order_ID into LOrder
                              from l_order in LOrder.DefaultIfEmpty()
                              select new
                              {
                                  ProductId = detail.Product_ID,
                                  Quantity = detail.Quantity,
                                  DateTime = l_order.Create_Date
                              };
                foreach (BProduct product in products)
                {
                    string productName = product.Title;
                    string shopName = product.BShop.Title;
                    List<Product> children=(from c in childrenProducts where c.Parent_ID==product.ID select c).ToList<Product>();
                    if (children == null || children.Count <= 0)
                    {
                        var vdetails = from d in details where d.ProductId == product.ID select d;
                        foreach (var detail in vdetails)
                        {
                            string month = DateTimeUtil.ConvertToDateTime(detail.DateTime).ToString("yyyy-M");
                            if (json == "")
                            {
                                json = "[{\"product_name\":\"" + productName + "\",\"prop_name\":\"--\",\"shop_name\":\"" + shopName + "\",\"month\":\"" + month + "\",\"quantity\":\"" + detail.Quantity + "\"}";
                            }
                            else
                            {
                                json += ",{\"product_name\":\"" + productName + "\",\"prop_name\":\"--\",\"shop_name\":\"" + shopName + "\",\"month\":\"" + month + "\",\"quantity\":\"" + detail.Quantity + "\"}";
                            }
                        }
                    }
                    else
                    {
                        int[] childrenIds=(from c in children select c.Product_ID).ToArray<int>();
                        var vdetails = from d in details where childrenIds.Contains(d.ProductId) select d;
                        foreach (var detail in vdetails)
                        {
                            string pNames = "";
                            List<BProductProperty> props=(from p in properties where p.ProductID==detail.ProductId select p).ToList<BProductProperty>();
                            foreach (BProductProperty prop in props)
                            {
                                if (pNames == "")
                                {
                                    pNames = prop.PName + ":" + prop.PValue;
                                }
                                else
                                {
                                    pNames +=","+ prop.PName + ":" + prop.PValue;
                                }
                            }
                            string month = DateTimeUtil.ConvertToDateTime(detail.DateTime).ToString("yyyy-M");
                            if (json == "")
                            {
                                json = "[{\"product_name\":\"" + productName + "\",\"prop_name\":\"" + pNames + "\",\"shop_name\":\"" + shopName + "\",\"month\":\"" + month + "\",\"quantity\":\"" + detail.Quantity + "\"}";
                            }
                            else
                            {
                                json += ",{\"product_name\":\"" + productName + "\",\"prop_name\":\"" + pNames + "\",\"shop_name\":\"" + shopName + "\",\"month\":\"" + month + "\",\"quantity\":\"" + detail.Quantity + "\"}";
                            }
                        }
                    }
                }
            }
            if (json != "")
            {
                json += "]";
            }
            return json;
        }
    }
}
