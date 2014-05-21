using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;

namespace KM.JXC.BL
{
    public class SalesManager : BBaseManager
    {
        private StockManager stockManager = null;
        private IOTradeManager tradeManager = null;
        public SalesManager(BUser user, int shop_id, Permission permission)
            : base(user, shop_id, permission)
        {
            tradeManager = new TaobaoTradeManager(this.AccessToken, this.Shop.Mall_Type_ID);
        }

        public SalesManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            stockManager = new StockManager(user, shop, permission);
            tradeManager = new TaobaoTradeManager(this.AccessToken, this.Shop.Mall_Type_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool SyncMallTrades(int startTime, int endTime, string status)
        {
            bool result = false;
            IOTradeManager tradeManager = new TaobaoTradeManager(this.AccessToken, this.Shop.Mall_Type_ID);
            List<BSale> sales = tradeManager.SyncTrades(new DateTime(2014, 5, 1), new DateTime(2014, 5, 30),null);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mall_trade_id"></param>
        /// <returns></returns>

        public bool SyncSingleMallTrade(string mall_trade_id)
        {
            bool result = false;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customers"></param>
        /// <param name="sSaleTime"></param>
        /// <param name="eSaleTime"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BSale> SearchSales(int[] products, int[] customers, int sSaleTime, int eSaleTime, int page, int pageSize, out int totalRecords)
        {
            List<BSale> sales = null;
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                var trades = from sale in db.Sale
                             where sale.Shop_ID == this.Shop.Shop_ID || sale.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(sale.Shop_ID)
                             select sale;

                if (products != null && products.Length > 0)
                {
                    int[] sale_ids = (from order in db.Sale_Detail
                                      where products.Contains(order.Parent_Product_ID)
                                      select order.Sale_ID).Distinct<int>().ToArray<int>();
                    if (sale_ids != null && sale_ids.Length > 0)
                    {
                        trades = trades.Where(t => sale_ids.Contains(t.Sale_ID));
                    }
                }

                if (customers != null && customers.Length > 0)
                {
                    trades = trades.Where(t => customers.Contains((int)t.Buyer_ID));
                }

                if (sSaleTime > 0)
                {
                    trades = trades.Where(t => t.Sale_Time >= sSaleTime);
                }

                if (eSaleTime > 0)
                {
                    trades = trades.Where(t => t.Sale_Time <= eSaleTime);
                }

                sales = (from t in trades
                         join customer in db.Customer on t.Buyer_ID equals customer.Customer_ID
                         join shop in db.Shop on t.Shop_ID equals shop.Shop_ID
                         select new BSale
                         {
                             ID = t.Sale_ID,
                             Created = (int)t.Created,
                             Synced = (int)t.Synced,
                             Modified = (int)t.Modified,
                             SaleDateTime = t.Sale_Time,
                             Amount = t.Amount,
                             Buyer = new BCustomer
                             {
                                 ID = customer.Customer_ID,
                                 Name = customer.Name,
                                 Mall_ID = customer.Mall_ID,
                                 Mall_Name = customer.Mall_Name
                             },
                             Mall_Trade_ID = t.Mall_Trade_ID,
                             Post_Fee = (double)t.Post_Fee,
                             Status = t.Status,
                             Shop = new BShop
                             {
                                 ID = shop.Shop_ID,
                                 Title = shop.Name
                             }
                         }).OrderBy(s => s.ID).Skip((page - 1) * pageSize).Take(pageSize).ToList<BSale>();

                int[] bsale_ids = (from sale in sales select sale.ID).ToArray<int>();
                var sale_details = from sdetail in db.Sale_Detail where bsale_ids.Contains(sdetail.Sale_ID) select sdetail;
                int[] product_ids = (from sd in sale_details select sd.Parent_Product_ID).ToArray<int>();
                int[] cproduct_ids = (from sd in sale_details select sd.Product_ID).Distinct<int>().ToArray<int>();
                var dbProducts = from product in db.Product where product_ids.Contains(product.Product_ID) select product;
                var childs = from prop in db.Product_Specifications
                             join ps in db.Product_Spec on prop.Product_Spec_ID equals ps.Product_Spec_ID
                             join psv in db.Product_Spec_Value on prop.Product_Spec_Value_ID equals psv.Product_Spec_Value_ID
                             where cproduct_ids.Contains(prop.Product_ID)
                             select new
                             {
                                 ProductID = prop.Product_ID,
                                 PID = prop.Product_Spec_ID,
                                 PName = ps.Name,
                                 PVID = prop.Product_Spec_Value_ID,
                                 PValue = psv.Name
                             };
                foreach (BSale sale in sales)
                {
                    sale.Orders = (from order in sale_details
                                   join product in dbProducts on order.Parent_Product_ID equals product.Product_ID into l_products
                                   from lproduct in l_products.DefaultIfEmpty()
                                   select new BOrder
                                   {
                                       ID = order.Sale_DID,
                                       Amount = (double)order.Amount,
                                       Mall_Order_ID = order.Mall_Order_ID,
                                       Quantity = order.Quantity,
                                       Price = order.Price,
                                       Parent_Product_ID = order.Parent_Product_ID,
                                       Product_ID = order.Product_ID,
                                       Product = new BProduct
                                       {
                                           ID = lproduct.Product_ID,
                                           Title = lproduct.Name
                                       }
                                   }).ToList<BOrder>();

                    foreach (BOrder order in sale.Orders)
                    {
                        if (order.Product != null)
                        {
                            order.Product.Children = new List<BProduct>();
                            BProduct child = new BProduct();
                            child.Title = order.Product.Title;
                            child.ID = order.Product_ID;
                            child.Properties = (from p in childs
                                                where p.ProductID == child.ID
                                                select new BProductProperty
                                                {
                                                    PID = p.PID,
                                                    PName = p.PName,
                                                    PValue = p.PValue,
                                                    PVID = p.PVID
                                                }).ToList<BProductProperty>();

                            order.Product.Children.Add(child);
                        }
                    }
                }
            }

            return sales;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sale_ids"></param>
        /// <param name="user_ids"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BBackSale> SearchBackSales(int[] sale_ids, int[] user_ids, int stime, int etime, int pageIndex, int pageSize, out int totalRecords)
        {
            List<BBackSale> backSales = new List<BBackSale>();
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                if (cspids == null)
                {
                    cspids = new int[] { 0 };
                }
                var dbBackSales = from sale in db.Back_Sale
                                  where sale.Shop_ID == this.Shop.Shop_ID || sale.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(sale.Shop_ID)
                                  select sale;

                if (sale_ids != null && sale_ids.Length > 0)
                {
                    dbBackSales = dbBackSales.Where(s => sale_ids.Contains(s.Sale_ID));
                }

                if (user_ids != null && user_ids.Length > 0)
                {
                    dbBackSales = dbBackSales.Where(s => user_ids.Contains(s.User_ID));
                }

                if (stime > 0)
                {
                    dbBackSales = dbBackSales.Where(s => s.Back_Date >= stime);
                }

                if (etime > 0)
                {
                    dbBackSales = dbBackSales.Where(s => s.Back_Date <= etime);
                }

                backSales = (from sale in dbBackSales
                             join user in db.User on sale.User_ID equals user.User_ID
                             join mtype in db.Mall_Type on user.Mall_Type equals mtype.Mall_Type_ID
                             join order in db.Sale on sale.Sale_ID equals order.Sale_ID
                             join customer in db.Customer on order.Buyer_ID equals customer.Customer_ID
                             join shop in db.Shop on sale.Shop_ID equals shop.Shop_ID
                             select new BBackSale
                             {
                                 BackTime = (int)sale.Back_Date,
                                 Created = sale.Created,
                                 CreatedBy = new BUser
                                 {
                                     ID = user.User_ID,
                                     Mall_ID = user.Mall_ID,
                                     Mall_Name = user.Mall_Name,
                                     Created = (int)user.Created,
                                     Type = mtype
                                 },
                                 Description = sale.Description,
                                 ID = sale.Back_Sale_ID,
                                 Sale = new BSale
                                 {
                                     Amount = order.Amount,
                                     Buyer = new BCustomer
                                     {
                                         ID = customer.Customer_ID,
                                         Mall_ID = customer.Mall_ID,
                                         Mall_Name = customer.Mall_Name,
                                         Type = mtype
                                     },
                                     Created = (int)order.Created,
                                     ID = order.Sale_ID,
                                     Mall_Trade_ID = order.Mall_Trade_ID,
                                     Modified = (int)order.Modified,
                                     Post_Fee = (double)order.Post_Fee,
                                     Status = order.Status,
                                     Synced = (int)order.Synced

                                 },

                                 Shop = new BShop
                                 {
                                     ID = shop.Shop_ID,
                                     Title = shop.Name,
                                     Mall_ID = shop.Mall_Shop_ID,
                                     Type = mtype
                                 }

                             }).OrderBy(a => a.ID).OrderBy(a => a.Shop.ID).ToList<BBackSale>();


                int[] bsaleids = (from bsale in backSales select bsale.ID).ToArray<int>();
                List<BBackSaleDetail> details = (from detail in db.Back_Sale_Detail
                                                 where bsaleids.Contains(detail.Back_Sale_ID)
                                                 select
                                                     new BBackSaleDetail
                                                     {

                                                     }).ToList<BBackSaleDetail>();
            }
            return backSales;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backSale"></param>
        /// <param name="details"></param>
        public void CreateBackSale(BBackSale backSale)
        {
            if (backSale == null)
            {
                return;
            }

            List<BBackSaleDetail> details = backSale.Details;

            if (this.CurrentUserPermission.ADD_BACK_STOCK == 0)
            {
                throw new KMJXCException("没有权限进行退货操作");
            }
            if (backSale.Sale == null || backSale.Sale.ID <= 0)
            {
                throw new KMJXCException("请选择一个销售单进行退货");
            }

            if (details == null)
            {
                throw new KMJXCException("");
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                List<Product> products = (from p in db.Product where p.Shop_ID == this.Shop.Shop_ID || p.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(p.Shop_ID) select p).ToList<Product>();
                Back_Sale dbbackSale = new Back_Sale();
                dbbackSale.Back_Date = backSale.BackTime;
                dbbackSale.Back_Sale_ID = 0;
                dbbackSale.Description = backSale.Description;
                dbbackSale.Sale_ID = backSale.Sale.ID;
                dbbackSale.Shop_ID = this.Shop.Shop_ID;
                dbbackSale.User_ID = this.CurrentUser.ID;
                db.Back_Sale.Add(dbbackSale);
                if (dbbackSale.Back_Sale_ID <= 0)
                {
                    throw new KMJXCException("退货单创建失败");
                }

                List<BBackStockDetail> bdetails = new List<BBackStockDetail>();
                foreach (BBackSaleDetail sDetail in details)
                {
                    Back_Sale_Detail dbSaleDetail = new Back_Sale_Detail();
                    dbSaleDetail.Back_Sale_ID = dbbackSale.Back_Sale_ID;
                    dbSaleDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbSaleDetail.Description = sDetail.Description;
                    dbSaleDetail.Parent_Product_ID = sDetail.ParentProductID;
                    if (dbSaleDetail.Parent_Product_ID == 0)
                    {
                        dbSaleDetail.Parent_Product_ID = (from p in products where p.Product_ID == sDetail.ProductID select p.Parent_ID).FirstOrDefault<int>();
                    }
                    dbSaleDetail.Price = sDetail.Price;
                    dbSaleDetail.Product_ID = sDetail.ProductID;
                    dbSaleDetail.Quantity = sDetail.Quantity;
                    dbSaleDetail.Status = 0;
                    db.Back_Sale_Detail.Add(dbSaleDetail);
                }
                db.SaveChanges();

                if (backSale.GenerateBackStock)
                {
                    stockManager.CreateBackStock(dbbackSale.Back_Sale_ID, backSale.UpdateStock);
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BBackSaleDetail> GetBackDetails(int backSaleId)
        {
            List<BBackSaleDetail> details = new List<BBackSaleDetail>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                details = (from bsd in db.Back_Sale_Detail
                           where bsd.Back_Sale_ID == backSaleId
                           select new BBackSaleDetail
                           {
                               Created = (int)bsd.Created,
                               Description = bsd.Description,
                               Price = double.Parse(bsd.Price.ToString("0.00")),
                               Quantity = bsd.Quantity,
                               Product = (from p in db.Product
                                          where bsd.Product_ID == p.Product_ID
                                          select new BProduct
                                           {
                                               ID = p.Product_ID,
                                               Title = p.Name,
                                               Description = p.Description,
                                               Category = new BCategory() { ID = p.Product_Class_ID },
                                               Code = p.Code,
                                               CreateTime = p.Create_Time,
                                               Price = p.Price,
                                               Quantity = (from sp in db.Stock_Pile where sp.Product_ID == p.Product_ID select sp.Quantity).FirstOrDefault<int>()
                                           }).FirstOrDefault<BProduct>()
                           }).ToList<BBackSaleDetail>();
            }

            return details;
        }

    }
}
