﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data;
using System.Transactions;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class StockManager:BBaseManager
    {
        public StockManager(BUser user, int shop_id, Permission permission)
            : base(user, shop_id,permission)
        {
        }

        public StockManager(BUser user, Shop shop, Permission permission)
            : base(user,shop,permission)
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backSaleId"></param>
        /// <param name="backSaleDetailId"></param>
        /// <param name="productId"></param>
        private void CreateBackStock(BBackStock backStock)
        {
            List<BBackStockDetail> details = backStock.Details;

            if (this.CurrentUserPermission.HANDLE_BACK_SALE == 0)
            {
                throw new KMJXCException("没有权限进行退货退库存操作");
            }

            if (backStock == null)
            {
                throw new KMJXCException("输入错误", ExceptionLevel.SYSTEM);
            }

            if (backStock.BackSale == null || backStock.BackSale.ID <= 0)
            {
                throw new KMJXCException("请选择退货单进行退库存操作", ExceptionLevel.SYSTEM);
            }

            if (details == null)
            {
                throw new KMJXCException("没有选择产品进行退库存");
            }


            using(KuanMaiEntities db = new KuanMaiEntities())
            {
                using (TransactionScope tran = new TransactionScope())
                {
                    Back_Stock dbBackStock = (from dbStock in db.Back_Stock where dbStock.Back_Sale_ID == backStock.BackSale.ID select dbStock).FirstOrDefault<Back_Stock>();
                    if (dbBackStock == null)
                    {
                        dbBackStock = new Back_Stock();
                        if (backStock.BackDateTime > 0)
                        {
                            dbBackStock.Back_Date = backStock.BackDateTime;
                        }
                        else
                        {
                            dbBackStock.Back_Date = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        }
                        dbBackStock.Back_Sale_ID = backStock.BackSale.ID;
                        dbBackStock.Back_Sock_ID = 0;
                        dbBackStock.Description = backStock.Description;
                        dbBackStock.Shop_ID = this.Shop.Shop_ID;
                        if (backStock.Created > 0)
                        {
                            dbBackStock.Created = backStock.Created;
                        }
                        else
                        {
                            dbBackStock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        }
                        dbBackStock.User_ID = this.CurrentUser.ID;
                        db.Back_Stock.Add(dbBackStock);
                        db.SaveChanges();
                    }

                    if (dbBackStock.Back_Sock_ID > 0)
                    {
                        foreach (BBackStockDetail detail in details)
                        {
                            Back_Stock_Detail dbDetail = new Back_Stock_Detail();
                            dbDetail.Back_Stock_ID = dbBackStock.Back_Sock_ID;
                            dbDetail.Price = detail.Price;
                            dbDetail.Product_ID = detail.ProductID;
                            dbDetail.Parent_Product_ID = detail.ParentProductID;
                            dbDetail.Quantity = detail.Quantity;
                            if (detail.Batch != null)
                            {
                                dbDetail.Batch_ID = detail.Batch.ID;
                            }

                            if (detail.StoreHouse != null)
                            {
                                dbDetail.StoreHouse_ID = detail.StoreHouse.ID;
                            }                           

                            //Update stock pile
                            if (backStock.UpdateStock)
                            {
                                Stock_Pile pile = (from spile in db.Stock_Pile where spile.Product_ID == dbDetail.Product_ID && spile.StockHouse_ID == detail.StoreHouse.ID && spile.Batch_ID == dbDetail.Batch_ID select spile).FirstOrDefault<Stock_Pile>();
                                if (pile != null)
                                {
                                    pile.Quantity = pile.Quantity + dbDetail.Quantity;
                                }

                                Product product = (from p in db.Product
                                                   join p1 in db.Product on p.Product_ID equals p1.Parent_ID
                                                   where p1.Product_ID == dbDetail.Product_ID
                                                   select p).FirstOrDefault<Product>();
                                if (product != null)
                                {
                                    product.Quantity += dbDetail.Quantity;
                                }
                                //1表示退库并更新了库存
                                dbDetail.Status = 1;
                            }

                            db.Back_Stock_Detail.Add(dbDetail);

                        }

                        db.SaveChanges();
                    }
                    else
                    {
                        throw new KMJXCException("退库存操作失败");
                    }

                    tran.Complete();                    
                }
            }          
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sale_id"></param>
        public void CreateBackStock(int backsale_id,List<BOrder> orders,int backSaleStatus)
        {
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                if (orders == null || orders.Count == 0)
                {
                    throw new KMJXCException("没有退货信息");
                }
                
                Back_Sale dbbackSale = (from bsale in db.Back_Sale where bsale.Back_Sale_ID == backsale_id select bsale).FirstOrDefault<Back_Sale>();
                if (dbbackSale == null)
                {
                    throw new KMJXCException("退货单信息不存在");
                }

                //dbbackSale.Status = backSaleStatus;

                var bdetails = from bsd in db.Back_Sale_Detail
                               where bsd.Back_Sale_ID == backsale_id
                               select bsd;

                string[] order_id = (from o in orders select o.Order_ID).ToArray<string>();
                if (order_id != null)
                {
                    bdetails = bdetails.Where(d => order_id.Contains(d.Order_ID));
                }
                List<Sale_Detail> saleDetails=(from s in db.Sale_Detail where order_id.Contains(s.Mall_Order_ID) select s).ToList<Sale_Detail>();
                List<Back_Sale_Detail> backSaleDetails = bdetails.ToList<Back_Sale_Detail>();
                //Check if current sale trade has leave stock records
                //if the sale doesn't have leave stock, so no need to back stock
                Leave_Stock leave_Stock=(from ls in db.Leave_Stock where ls.Sale_ID==dbbackSale.Sale_ID select ls).FirstOrDefault<Leave_Stock>();
                if (leave_Stock!=null)
                {
                    List<Leave_Stock_Detail> leaveDetails=(from ld in db.Leave_Stock_Detail where ld.Leave_Stock_ID==leave_Stock.Leave_Stock_ID select ld).ToList<Leave_Stock_Detail>();

                    BBackStock backStock = new BBackStock();
                    backStock.BackSaleID = dbbackSale.Back_Sale_ID;
                    backStock.BackSale = new BBackSale() { ID = dbbackSale.Back_Sale_ID };
                    if (backSaleStatus == 1 || backSaleStatus==2)
                    {
                        backStock.UpdateStock = true;
                    }
                    else
                    {
                        backStock.UpdateStock = false;
                    }
                    backStock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    backStock.BackDateTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    backStock.Created_By = new BUser() { ID = this.CurrentUser.ID };
                    if (!string.IsNullOrEmpty(dbbackSale.Description))
                    {
                        backStock.Description = dbbackSale.Description + "<br/> 生成了退库单";
                    }
                    else
                    {
                        backStock.Description = "生成了退库单";
                    }

                    backStock.Shop = new BShop() { ID = dbbackSale.Shop_ID };

                    //collect back stock details info from leave stock details
                    
                    backStock.Details = new List<BBackStockDetail>();

                    foreach (Back_Sale_Detail bsd in backSaleDetails)
                    {
                        BOrder order=(from o in orders where bsd.Order_ID == o.Order_ID select o).FirstOrDefault<BOrder>();
                        Sale_Detail saleDetail=(from s in saleDetails where s.Mall_Order_ID==bsd.Order_ID select s).FirstOrDefault<Sale_Detail>();
                        Leave_Stock_Detail leaveStockDetail = (from lsd in leaveDetails where lsd.Order_ID == bsd.Order_ID select lsd).FirstOrDefault<Leave_Stock_Detail>();
                        if (leaveStockDetail == null)
                        {
                            continue;
                        }
                        BBackStockDetail bsDetail = new BBackStockDetail();
                        bsDetail.Price = leaveStockDetail.Price; 
                        bsDetail.Quantity = leaveStockDetail.Quantity;
                        if (order != null && order.Quantity > 0 && order.Quantity<=leaveStockDetail.Quantity)
                        {
                            bsDetail.Quantity = order.Quantity;
                        }

                        bsDetail.ProductID = leaveStockDetail.Product_ID;
                        bsDetail.ParentProductID = leaveStockDetail.Parent_Product_ID;
                        bsDetail.Batch = new BStockBatch() { ID = leaveStockDetail.Batch_ID };
                        bsDetail.StoreHouse = new BStoreHouse() { ID = leaveStockDetail.StoreHouse_ID };
                        backStock.Details.Add(bsDetail);                       
                        bsd.Status = backSaleStatus;
                        //5 means refound and successfully back to sock
                        saleDetail.Status1 = (int)SaleDetailStatus.REFOUND_HANDLED;
                        saleDetail.SyncResultMessage = "退货已经处理";
                    }

                    if (backStock.Details.Count > 0)
                    {
                        this.CreateBackStock(backStock);
                    }                    
                }

                db.SaveChanges();
            }
            catch (KMJXCException kex)
            {
                throw kex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.Dispose();
            }            
        }

        /// <summary>
       /// 
       /// </summary>
       /// <param name="suppliers"></param>
       /// <param name="keyword"></param>
       /// <param name="category_id"></param>
       /// <param name="pageIndex"></param>
       /// <param name="pageSize"></param>
       /// <param name="total"></param>
       /// <returns></returns>
        public List<BProductWastage> SearchProductWastage(int[] suppliers, string keyword,int? category_id, int pageIndex, int pageSize, out int total)
        {
            List<BProductWastage> products = new List<BProductWastage>();
            total = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                var wastage = from waste_product in db.Stock_Waste
                              where waste_product.Shop_ID == this.Shop.Shop_ID || waste_product.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(waste_product.Shop_ID)
                              group waste_product by waste_product.Parent_ProductID into wp
                              orderby wp.Key
                              select new
                              {
                                  ProductID = wp.Key,
                                  Quantity = wp.Sum(a => a.Quantity) == null ? 0 : wp.Sum(a => a.Quantity),
                              };

                var dbProducts = from product in db.Product
                                 where (product.Shop_ID == this.Shop.Shop_ID || product.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(product.Shop_ID)) && product.Parent_ID == 0
                                 orderby product.Shop_ID
                                 select new { 
                                    pdt=product,
                                 };

                if (!string.IsNullOrEmpty(keyword))
                {
                    dbProducts = dbProducts.Where(p => p.pdt.Name.Contains(keyword.Trim()));
                }

                if (suppliers != null && suppliers.Length > 0)
                {
                    int[] pdtids = (from ps in db.Product_Supplier where suppliers.Contains(ps.Supplier_ID) select ps.Product_ID).ToArray<int>();
                    if (pdtids != null && pdtids.Length > 0)
                    {
                        dbProducts = dbProducts.Where(p => pdtids.Contains(p.pdt.Product_ID));
                    }
                }

                if (category_id != null)
                {
                    Product_Class cate = (from ca in db.Product_Class where ca.Product_Class_ID == category_id select ca).FirstOrDefault<Product_Class>();
                    if (cate != null)
                    {
                        if (cate.Parent_ID == 0)
                        {
                            int[] ccids = (from c in db.Product_Class where c.Parent_ID == category_id select c.Product_Class_ID).ToArray<int>();
                            if (ccids != null && ccids.Length > 0)
                            {
                                dbProducts = dbProducts.Where(a => ccids.Contains(a.pdt.Product_Class_ID));
                            }
                            else
                            {
                                dbProducts = dbProducts.Where(a => a.pdt.Product_Class_ID == category_id);
                            }
                        }
                        else
                        {
                            dbProducts = dbProducts.Where(a => a.pdt.Product_Class_ID == category_id);
                        }
                    }                    
                }               

                var product_wastage = from product in dbProducts
                                      join waste in wastage on product.pdt.Product_ID equals waste.ProductID into product_waste
                                      from productwaste in product_waste.DefaultIfEmpty()
                                      join category in db.Product_Class on product.pdt.Product_Class_ID equals category.Product_Class_ID
                                      join shop in db.Shop on product.pdt.Shop_ID equals shop.Shop_ID
                                      join mtype in db.Mall_Type on shop.Mall_Type_ID equals mtype.Mall_Type_ID
                                      orderby product.pdt.Product_ID
                                      
                                      select new BProductWastage
                                         {
                                             Product = new BProduct
                                             {
                                                 ID = product.pdt.Product_ID,
                                                 Title = product.pdt.Name,
                                                 Category = new BCategory
                                                 {
                                                     ID = category.Product_Class_ID,
                                                     Name = category.Name
                                                 },
                                                 Quantity = (int)product.pdt.Quantity,
                                                 BShop = new BShop
                                                 {
                                                     ID = shop.Shop_ID,
                                                     Mall_ID = shop.Mall_Shop_ID,
                                                     Type = mtype,
                                                     Title = shop.Name
                                                 }
                                             },

                                             Quantity = productwaste.Quantity == null ? 0 : productwaste.Quantity
                                         };

                

                total = product_wastage.Count();
                products = product_wastage.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BProductWastage>();

                foreach (BProductWastage bpw in products)
                {
                    if (bpw.Product != null && bpw.Product.BShop != null)
                    {
                        if (this.Main_Shop.Shop_ID == bpw.Product.BShop.ID)
                        {
                            bpw.Product.FromMainShop = true;
                        }
                        else if (cspids != null && cspids.Length > 0 && cspids.Contains(bpw.Product.BShop.ID))
                        {
                            bpw.Product.FromChildShop = true;
                        }
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// Get product detailed stock wastage
        /// </summary>
        /// <param name="product_ids"></param>
        /// <returns></returns>
        public List<BProduct> GetProductsWastageDetail(int[] product_ids)
        {
            if (product_ids == null || product_ids.Length<=0)
            {
                throw new KMJXCException("没有输入产品编号");
            }

            List<BProduct> products = new List<BProduct>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                products = (from p in db.Product 
                            join pw in db.Stock_Waste on p.Product_ID equals pw.Product_ID into LPW
                            from l_pw in LPW.DefaultIfEmpty()
                            where product_ids.Contains(p.Product_ID) && p.Parent_ID == 0 
                            select new BProduct 
                            {
                                ID = p.Product_ID,
                                Title = p.Name,
                                Quantity = l_pw!=null?l_pw.Quantity:0
                            }).ToList<BProduct>();
                List<BProduct> childProducts = (from p in db.Product
                                                join pw in db.Stock_Waste on p.Product_ID equals pw.Product_ID into LPW
                                                from l_pw in LPW.DefaultIfEmpty()
                                                where product_ids.Contains(p.Parent_ID) 
                                                select new BProduct 
                                                {
                                                    ID=p.Product_ID,
                                                    ParentID=p.Parent_ID,
                                                    Title=p.Name,
                                                    Quantity = l_pw != null ? l_pw.Quantity : 0
                                                }).ToList<BProduct>();

                int[] child_ids=(from p in childProducts select p.ID).ToArray<int>();
                List<BProductProperty> properties = (from pv in db.Product_Specifications
                                                     join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                     from l_prop in LProp.DefaultIfEmpty()
                                                     join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                     from l_propv in LPropv.DefaultIfEmpty()
                                                     where child_ids.Contains(pv.Product_ID)
                                                     select new BProductProperty
                                                     {
                                                         PID = pv.Product_Spec_ID,
                                                         PName = l_prop.Name,
                                                         ProductID = pv.Product_ID,
                                                         PValue = l_propv.Name,
                                                         PVID = pv.Product_Spec_Value_ID
                                                     }).ToList<BProductProperty>();

                foreach (BProduct product in childProducts)
                {
                    product.Properties = (from prop in properties where prop.ProductID == product.ID select prop).ToList<BProductProperty>();
                }

                foreach (BProduct product in products)
                {
                    product.Children=(from p in childProducts where p.ParentID==product.ID select p).ToList<BProduct>();
                }
            }
            return products;
        }

        /// <summary>
        /// Update product wastage quantity
        /// </summary>
        /// <param name="products">List of BProduct object, ID and Quantity fields must have valid values.</param>
        public void UpdateProductsWastage(List<BProduct> products)
        {
            if (products == null || products.Count == 0)
            {
                return;
            }

            if (this.CurrentUserPermission.UPDATE_WASTAGE == 0)
            {
                throw new KMJXCException("没有权限更新产品损耗数量");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] product_ids=(from p in products select p.ID).ToArray<int>();

                List<Product> dbProducts=(from p in db.Product where product_ids.Contains(p.Product_ID) select p).ToList<Product>();
                List<Stock_Waste> wss = (from s in db.Stock_Waste where product_ids.Contains(s.Product_ID) select s).ToList<Stock_Waste>();
                foreach (Product product in dbProducts)
                {
                    BProduct bProduct=(from bp in products where bp.ID==product.Product_ID select bp).FirstOrDefault<BProduct>();
                    Stock_Waste sw=(from s in wss where s.Product_ID == product.Product_ID select s).FirstOrDefault<Stock_Waste>();
                    if (sw != null)
                    {
                        sw.Quantity = bProduct.Quantity;
                    }
                    else
                    {
                        sw = new Stock_Waste();
                        sw.Product_ID = product.Product_ID;
                        sw.Parent_ProductID = product.Parent_ID;
                        if (sw.Parent_ProductID == 0)
                        {
                            sw.Parent_ProductID = product.Product_ID;
                        }
                        sw.Shop_ID = product.Shop_ID;
                        sw.Quantity = bProduct.Quantity;
                        db.Stock_Waste.Add(sw);
                    }
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// search leave stocks from database
        /// </summary>
        /// <param name="enter_stock_id">a array of leave stock id</param>
        /// <param name="user_id">a arrary of created user</param>
        /// <param name="leaveStartTime"></param>
        /// <param name="leaveEndTime"></param>
        /// <param name="storeHouseId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BLeaveStock> SearchLeaveStocks(int[] leave_stock_ids, string[] sale_ids, int[] user_ids, long leaveStartTime, long leaveEndTime, int pageIndex, int pageSize, out int totalRecords)
        {
            List<BLeaveStock> stocks = new List<BLeaveStock>();
            totalRecords = 0;
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }

            if (pageSize == 0)
            {
                pageSize = 30;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                var dbStocks = from stock in db.Leave_Stock
                               //where stock.Shop_ID == this.Shop.Shop_ID || stock.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(stock.Shop_ID)
                               select stock;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    dbStocks = dbStocks.Where(stock => (stock.Shop_ID == this.Shop.Shop_ID || stock.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(stock.Shop_ID)));
                }
                else
                {
                    dbStocks = dbStocks.Where(stock => (stock.Shop_ID == this.Shop.Shop_ID));

                }

                if (leave_stock_ids != null && leave_stock_ids.Length > 0)
                {
                    dbStocks = dbStocks.Where(s => leave_stock_ids.Contains(s.Leave_Stock_ID));
                }

                if (user_ids != null && user_ids.Length > 0)
                {
                    dbStocks = dbStocks.Where(s => user_ids.Contains(s.User_ID));
                }

                if (leaveStartTime > 0)
                {
                    dbStocks = dbStocks.Where(s => s.Leave_Date >= leaveStartTime);
                }

                if (leaveEndTime > 0)
                {
                    dbStocks = dbStocks.Where(s => s.Leave_Date <= leaveEndTime);
                }

                if (sale_ids != null && sale_ids.Length > 0)
                {
                    dbStocks = dbStocks.Where(s => sale_ids.Contains(s.Sale_ID));
                }

                var tmp = from stock in dbStocks
                          join sale in db.Sale on stock.Sale_ID equals sale.Mall_Trade_ID into lsales
                          from l_sale in lsales.DefaultIfEmpty()
                          join cus in db.Customer on l_sale.Buyer_ID equals cus.Customer_ID into lcuss
                          from l_cus in lcuss.DefaultIfEmpty()
                          join dist in db.Common_District on l_cus.City_ID equals dist.id into ldist
                          from l_dist in ldist.DefaultIfEmpty()
                          join distp in db.Common_District on l_cus.Province_ID equals distp.id into ldistp
                          from l_distp in ldistp.DefaultIfEmpty()
                          join mtype in db.Mall_Type on l_cus.Mall_Type_ID equals mtype.Mall_Type_ID into lmtype
                          from l_mtype in lmtype.DefaultIfEmpty()
                          join createdby in db.User on stock.User_ID equals createdby.User_ID into lcreatedby
                          from l_createdby in lcreatedby.DefaultIfEmpty()
                          join shop in db.Shop on stock.Shop_ID equals shop.Shop_ID into lshop
                          from l_shop in lshop.DefaultIfEmpty()
                          select new BLeaveStock
                          {
                              Sale = new BSale
                                      {
                                          Buyer = new BCustomer
                                          {
                                              Address = l_cus.Address,
                                              Phone = l_cus.Phone,
                                              Mall_Name = l_cus.Mall_Name,
                                              Mall_ID = l_cus.Mall_ID,
                                              City = l_dist,
                                              Province = l_distp,
                                              Email = l_cus.Email,
                                              Type = l_mtype != null ? new BMallType { ID=l_mtype.Mall_Type_ID,Name=l_mtype.Name } : new BMallType { ID=0,Name=""}
                                          },
                                          Sale_ID = l_sale.Mall_Trade_ID,
                                          Amount = l_sale.Amount,
                                          Created = (long)l_sale.Created,
                                          Modified = (long)l_sale.Modified,
                                          Post_Fee = (double)l_sale.Post_Fee,
                                          Synced = (int)l_sale.Synced,
                                          Status = l_sale.Status,
                                      },
                              Created = stock.Created,
                              Created_By = new BUser
                                            {
                                                ID = l_createdby.User_ID,
                                                Mall_ID = l_createdby.Mall_ID,
                                                Mall_Name = l_createdby.Mall_Name
                                            },
                              ID = stock.Leave_Stock_ID,
                              LeaveDate = stock.Leave_Date,
                              Shop = new BShop
                                      {
                                          ID = l_shop.Shop_ID,
                                          Title = l_shop.Name
                                      },

                          };
                totalRecords = tmp.Count();
                stocks = tmp.OrderByDescending(s => s.Created).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BLeaveStock>();

                int[] stock_ids = (from stock in stocks select stock.ID).ToArray<int>();
                List<Leave_Stock_Detail> dbdetails = (from detail in db.Leave_Stock_Detail
                                                      where stock_ids.Contains(detail.Leave_Stock_ID)
                                                      select detail).ToList<Leave_Stock_Detail>();
                int[] child_product_ids=(from lsd in dbdetails select lsd.Product_ID).Distinct().ToArray<int>();
                int[] product_ids = (from lsd in dbdetails select lsd.Parent_Product_ID).Distinct().ToArray<int>();
                List<Product> dbProducts = (from product in db.Product where product_ids.Contains(product.Product_ID) select product).ToList<Product>();
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

                List<Store_House> houses=(from house in db.Store_House where house.Shop_ID==this.Shop.Shop_ID || house.Shop_ID==this.Main_Shop.Shop_ID || cspids.Contains(house.Shop_ID) select house).ToList<Store_House>();

                foreach (BLeaveStock stock in stocks)
                {
                    stock.Details = (from detail in dbdetails
                                     join product in dbProducts on detail.Parent_Product_ID equals product.Product_ID into lproduct
                                     from l_product in lproduct.DefaultIfEmpty()

                                     where detail.Leave_Stock_ID == stock.ID
                                     select new BLeaveStockDetail
                                     {
                                         OrderID=detail.Order_ID,
                                         ProductID = detail.Product_ID,
                                         Parent_ProductID = detail.Parent_Product_ID,
                                         Price = detail.Price,
                                         Quantity = detail.Quantity,
                                         Amount=detail.Amount,
                                         Product = new BProduct
                                         {
                                             Title = l_product.Name,
                                             ID = l_product.Product_ID,
                                             Properties = (from p in childs
                                                           where p.ProductID == detail.Product_ID
                                                           select p).ToList<BProductProperty>()
                                         },
                                         StoreHouse = (from house in houses
                                                       where house.StoreHouse_ID == detail.StoreHouse_ID
                                                       select new BStoreHouse
                                                       {
                                                           ID = house.StoreHouse_ID,
                                                           Name = house.Title,
                                                           Phone = house.Phone,
                                                           Address = house.Address,
                                                           Created = (int)house.Create_Time
                                                       }).FirstOrDefault<BStoreHouse>(),

                                     }).ToList<BLeaveStockDetail>();

                    if (stock.Shop.ID == this.Main_Shop.Shop_ID)
                    {
                        stock.FromMainShop = true;
                    }
                    else if (cspids != null && cspids.Contains(stock.Shop.ID))
                    {
                        stock.FromChildShop = true;
                    }
                }
            }
            return stocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="back_stock_ids"></param>
        /// <param name="sale_ids"></param>
        /// <param name="user_ids"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BBackStockDetail> SearchBackStockDetails(int[] back_stock_ids, string[] sale_ids, int[] user_ids, int storeHouseId, long startTime, long endTime, int pageIndex, int pageSize, out int totalRecords)
        {
            List<BBackStockDetail> bStockDetails = new List<BBackStockDetail>();
            List<BBackStock> stocks = new List<BBackStock>();
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (cspids == null)
                {
                    cspids = new int[] { 0 };
                }
                var dbstocks = from stock in db.Back_Stock
                               where stock.Shop_ID == this.Shop.Shop_ID || stock.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(stock.Shop_ID)
                               select stock;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    dbstocks=dbstocks.Where(stock=>(stock.Shop_ID == this.Shop.Shop_ID || stock.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(stock.Shop_ID)));
                }
                else
                {
                    dbstocks = dbstocks.Where(stock => (stock.Shop_ID == this.Shop.Shop_ID));
                }

                if (back_stock_ids != null)
                {
                    dbstocks = dbstocks.Where(s => back_stock_ids.Contains(s.Back_Sock_ID));
                }

                if (sale_ids != null)
                {
                    int[] backSaleIds = (from backSale in db.Back_Sale where sale_ids.Contains(backSale.Sale_ID) select backSale.Back_Sale_ID).ToArray<int>();
                    if (backSaleIds != null && backSaleIds.Length > 0)
                    {
                        dbstocks = dbstocks.Where(s => backSaleIds.Contains(s.Back_Sale_ID));
                    }
                }

                if (user_ids != null && user_ids.Length > 0)
                {
                    dbstocks = dbstocks.Where(s => user_ids.Contains(s.User_ID));
                }

                if (startTime > 0)
                {
                    dbstocks = dbstocks.Where(s => s.Back_Date >= startTime);
                }

                if (endTime > 0)
                {
                    dbstocks = dbstocks.Where(s => s.Back_Date <= endTime);
                }


                int[] backStockId = (from bstock in dbstocks select bstock.Back_Sock_ID).ToArray<int>();
                if (backStockId != null && backStockId.Length > 0)
                {
                    var tmpObj = from stockDetail in db.Back_Stock_Detail
                                 where backStockId.Contains(stockDetail.Back_Stock_ID)
                                 join backStock in db.Back_Stock on stockDetail.Back_Stock_ID equals backStock.Back_Sock_ID into lBackStock
                                 from l_backStock in lBackStock.DefaultIfEmpty()
                                 join backSale in db.Back_Sale on l_backStock.Back_Sale_ID equals backSale.Back_Sale_ID into lBackSale
                                 from l_backSale in lBackSale.DefaultIfEmpty()
                                 join user in db.User on l_backSale.User_ID equals user.User_ID into lUser
                                 from l_user in lUser.DefaultIfEmpty()
                                 join product in db.Product on stockDetail.Parent_Product_ID equals product.Product_ID into lProduct
                                 from l_product in lProduct.DefaultIfEmpty()
                                 join house in db.Store_House on stockDetail.StoreHouse_ID equals house.StoreHouse_ID into lHouse
                                 from l_house in lHouse.DefaultIfEmpty()
                                 join shop in db.Shop on l_backStock.Shop_ID equals shop.Shop_ID into lShop
                                 from l_shop in lShop.DefaultIfEmpty()
                                 select new BBackStockDetail
                                 {
                                     BackStock = new BBackStock
                                     {
                                         BackDateTime = l_backStock.Back_Date,
                                         BackSaleID = l_backStock.Back_Sale_ID,
                                         BackSale = new BBackSale
                                         {
                                             Amount = l_backSale.Amount,
                                             BackTime = l_backSale.Back_Date,
                                             Created = l_backSale.Created,
                                             Sale = new BSale
                                             {
                                                 Sale_ID = l_backSale.Sale_ID
                                             }
                                         },
                                         Created = l_backStock.Created,
                                         Created_By = new BUser
                                         {
                                             ID = l_user.User_ID,
                                             Mall_ID = l_user.Mall_ID,
                                             Mall_Name = l_user.Mall_Name
                                         },
                                         Shop = new BShop
                                         {
                                             ID = l_shop.Shop_ID,
                                             Title = l_shop.Name
                                         },
                                         ID = l_backStock.Back_Sock_ID
                                     },
                                     ParentProductID = stockDetail.Parent_Product_ID,
                                     ProductID = stockDetail.Product_ID,
                                     Price = stockDetail.Price,
                                     Quantity = stockDetail.Quantity,
                                     Product = new BProduct
                                     {
                                         ID = l_product.Product_ID,
                                         Title = l_product.Name
                                     },
                                     StoreHouse = new BStoreHouse
                                     {
                                         ID = l_house.StoreHouse_ID,
                                         Name = l_house.Title
                                     },
                                     Status = stockDetail.Status
                                 };

                    if (storeHouseId > 0)
                    {
                        tmpObj = tmpObj.Where(t => t.StoreHouse.ID == storeHouseId);
                    }

                    tmpObj = tmpObj.OrderBy(t => t.BackStock.ID);

                    totalRecords = tmpObj.Count();
                    if (totalRecords > 0)
                    {
                        bStockDetails = tmpObj.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BBackStockDetail>();
                    }
                }
            }
            return bStockDetails;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="back_stock_ids"></param>
        /// <param name="sale_ids"></param>
        /// <param name="user_ids"></param>
        /// <param name="leaveStartTime"></param>
        /// <param name="leaveEndTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BBackStock> SearchBackStocks(int[] back_stock_ids, string[] sale_ids, int[] user_ids, int startTime, int endTime, int pageIndex, int pageSize, out int totalRecords)
        {
            List<BBackStock> stocks = new List<BBackStock>();
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (cspids == null)
                {
                    cspids = new int[] { 0 };
                }
                var dbstocks = from stock in db.Back_Stock
                               where stock.Shop_ID == this.Shop.Shop_ID || stock.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(stock.Shop_ID)
                               select stock;

                if (back_stock_ids != null)
                {
                    dbstocks = dbstocks.Where(s => back_stock_ids.Contains(s.Back_Sock_ID));
                }

                if (sale_ids != null)
                {
                    int[] backSaleIds = (from backSale in db.Back_Sale where sale_ids.Contains(backSale.Sale_ID) select backSale.Back_Sale_ID).ToArray<int>();
                    if (backSaleIds != null && backSaleIds.Length > 0)
                    {
                        dbstocks = dbstocks.Where(s => backSaleIds.Contains(s.Back_Sale_ID));
                    }
                }

                if (user_ids != null && user_ids.Length > 0)
                {
                    dbstocks = dbstocks.Where(s => user_ids.Contains(s.User_ID));
                }

                if (startTime > 0)
                {
                    dbstocks = dbstocks.Where(s => s.Back_Date >= startTime);
                }

                if (endTime > 0)
                {
                    dbstocks = dbstocks.Where(s => s.Back_Date <= endTime);
                }

                var obj = from stock in dbstocks
                          join backsale in db.Back_Sale on stock.Back_Sale_ID equals backsale.Back_Sale_ID
                          join order in db.Sale on backsale.Sale_ID equals order.Mall_Trade_ID
                          join shop in db.Shop on stock.Shop_ID equals shop.Shop_ID
                          join user in db.User on stock.User_ID equals user.User_ID
                          join customer in db.Customer on order.Buyer_ID equals customer.Customer_ID
                          join mtype in db.Mall_Type on user.Mall_Type equals mtype.Mall_Type_ID
                          select new BBackStock
                          {
                              ID = stock.Back_Sock_ID,
                              BackSale = new BBackSale
                              {
                                  ID = backsale.Back_Sale_ID,
                                  BackTime = backsale.Back_Date,
                                  Created = backsale.Created,
                                  Description = backsale.Description,
                                  Sale = new BSale
                                  {
                                      Amount = order.Amount,
                                      Modified = (int)order.Modified,
                                      Sale_ID = order.Mall_Trade_ID,
                                      Created = (int)order.Created,
                                      Buyer = new BCustomer
                                      {
                                          ID = customer.Customer_ID,
                                          Mall_Name = customer.Mall_Name,
                                          Mall_ID = customer.Mall_ID,
                                          Type = new BMallType { ID = mtype.Mall_Type_ID, Name = mtype.Name, Description = mtype.Description }
                                      }
                                  },
                              },
                              BackDateTime = stock.Back_Date,
                              BackSaleID = backsale.Back_Sale_ID,
                              Created = stock.Created,
                              Created_By = new BUser
                              {
                                  ID = user.User_ID,
                                  Mall_Name = user.Mall_Name,
                                  Mall_ID = user.Mall_ID,
                                  Type = new BMallType {  ID=mtype.Mall_Type_ID,Name=mtype.Name,Description=mtype.Description}
                              },
                              Description = stock.Description,
                              Shop = new BShop
                              {
                                  ID = shop.Shop_ID,
                                  Mall_ID = shop.Mall_Shop_ID,
                                  Title = shop.Name,
                                  Description = shop.Description,
                              }
                          };

                totalRecords = dbstocks.Count();
                if (totalRecords > 0)
                {
                    stocks = obj.OrderBy(s => s.ID).OrderBy(s => s.Shop.ID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BBackStock>();

                    int[] bstockids = (from s in stocks select s.ID).ToArray<int>();

                    List<BBackStockDetail> details = (from detail in db.Back_Stock_Detail

                                                      where bstockids.Contains(detail.Back_Stock_ID)
                                                      select new BBackStockDetail
                                                      {
                                                          BackStock = new BBackStock
                                                          {
                                                              ID = detail.Back_Stock_ID
                                                          },
                                                          ParentProductID = detail.Parent_Product_ID,
                                                          Price = detail.Price,
                                                          ProductID = detail.Product_ID,
                                                          Quantity = detail.Quantity,
                                                          StoreHouse = (from house in db.Store_House
                                                                        where house.StoreHouse_ID == detail.StoreHouse_ID
                                                                        select new BStoreHouse
                                                                        {
                                                                            ID = house.StoreHouse_ID,
                                                                            Address = house.Address,
                                                                            Name = house.Title,
                                                                        }).FirstOrDefault<BStoreHouse>()
                                                      }).ToList<BBackStockDetail>();

                    foreach (BBackStock bstock in stocks)
                    {
                        bstock.Details = (from detail in details where detail.BackStock.ID == bstock.ID select detail).ToList<BBackStockDetail>();
                        if (this.Main_Shop.Shop_ID == bstock.Shop.ID)
                        {
                            bstock.FromMainShop = true;
                        }
                        else if (cspids != null && cspids.Contains(bstock.Shop.ID))
                        {
                            bstock.FromChildShop = true;
                        }
                    }
                }

            }
            return stocks;
        }
        
        /// <summary>
        /// Get leave stock details
        /// </summary>
        /// <param name="leavestock_id">leave stock id</param>
        /// <returns>A list of BLeaveStockDetail</returns>
        public List<BLeaveStockDetail> GetLeaveStockDetails(int leavestock_id)
        {
            List<BLeaveStockDetail> details = new List<BLeaveStockDetail>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Leave_Stock dbstock=(from stock in db.Leave_Stock where stock.Leave_Stock_ID==leavestock_id select stock).FirstOrDefault<Leave_Stock>();
                if (dbstock == null)
                {
                    throw new KMJXCException("编号为:"+leavestock_id +"的出库单不存在");
                }

                details = (from detail in db.Leave_Stock_Detail
                           where detail.Leave_Stock_ID == leavestock_id
                           select new BLeaveStockDetail
                           {
                               ProductID = detail.Product_ID,
                               Product = (from product in db.Product
                                          where product.Product_ID == detail.Product_ID
                                          select new BProduct
                                          {
                                              ID = product.Product_ID,
                                              Title = product.Name
                                          }).FirstOrDefault<BProduct>(),
                               Parent_ProductID = detail.Parent_Product_ID,
                               ParentProduct = (from product in db.Product
                                                where product.Product_ID == detail.Parent_Product_ID
                                                select new BProduct
                                                {
                                                    ID = product.Product_ID,
                                                    Title = product.Name
                                                }).FirstOrDefault<BProduct>(),
                               Price = detail.Price,
                               Quantity = detail.Quantity,
                               StoreHouse = (from house in db.Store_House
                                             where house.StoreHouse_ID == detail.StoreHouse_ID
                                             select new BStoreHouse
                                             {
                                                 ID = house.StoreHouse_ID,
                                                 Name = house.Title,
                                                 Phone = house.Phone,
                                                 Address = house.Address,
                                                 Created = (int)house.Create_Time
                                             }).FirstOrDefault<BStoreHouse>(),
                           }).ToList<BLeaveStockDetail>();
            }
            return details;
        }
        /// <summary>
        /// Search Enter stocks
        /// </summary>
        /// <param name="user_id">who create the enter stock</param>
        /// <param name="startTime">date time range for enter date</param>
        /// <param name="endTime">date time range for enter date</param>
        /// <param name="storeHouseId">store house</param>
        /// <param name="pageIndex">page</param>
        /// <param name="pageSize">page size</param>
        /// <param name="totalRecords">total records fitting the input conditions</param>
        /// <returns>a list of enter stock</returns>
        public List<BEnterStock> SearchEnterStocks(int enter_stock_id,int buy_order_id,int buy_id,int user_id,int startTime,int endTime,int storeHouseId, int pageIndex,int pageSize,out int totalRecords)
        {            
            List<BEnterStock> stocks = new List<BEnterStock>();
            totalRecords = 0;
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageSize == 0)
            {
                pageSize = 30;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var os = from o in db.Enter_Stock
                         select o;  

                int[] cshop_ids=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    os = os.Where(o1 => o1.Shop_ID == this.Shop.Shop_ID || cshop_ids.Contains(o1.Shop_ID));
                }
                else
                {
                    os = os.Where(o1 => o1.Shop_ID == this.Shop.Shop_ID);
                }

                if (enter_stock_id > 0)
                {
                    os = os.Where(o11 => o11.Enter_Stock_ID == enter_stock_id);
                }

                if (user_id > 0)
                {
                    os = os.Where(o11 => o11.User_ID == user_id);
                }

                if (startTime > 0)
                {
                    os=os.Where(o11=>o11.Enter_Date>=startTime);
                }

                if (endTime > 0)
                {
                    os = os.Where(o11 => o11.Enter_Date <= endTime);
                }

                totalRecords = os.Count();
                var oos = from o2 in os
                          select new BEnterStock()
                          {
                              ID = (int)o2.Enter_Stock_ID,
                              Status = (int)o2.Status,
                              Shop = (from sp in db.Shop
                                      where sp.Shop_ID == o2.Shop_ID
                                      select new BShop
                                      {
                                          Created = (int)sp.Created,
                                          Description = sp.Description,
                                          ID = sp.Shop_ID,
                                          Mall_ID = sp.Mall_Shop_ID,
                                          Title = sp.Name,
                                      }).FirstOrDefault<BShop>(),
                              Created_By = (from u in db.User
                                            where u.User_ID == o2.User_ID
                                            select new BUser
                                            {
                                                ID = u.User_ID,
                                                Mall_ID = u.Mall_ID,
                                                Mall_Name = u.Mall_Name,
                                                Name = u.Name,
                                                Password = u.Password,
                                            }).FirstOrDefault<BUser>(),
                              BuyID = (int)o2.Buy_ID,
                              Created = (int)o2.Enter_Date,
                              StoreHouse = (from house in db.Store_House where house.StoreHouse_ID == o2.StoreHouse_ID 
                                            select new BStoreHouse
                                            {
                                                ID=house.StoreHouse_ID,
                                                Name=house.Title,
                                                Phone=house.Phone,
                                                Address=house.Address
                                            }).FirstOrDefault<BStoreHouse>()
                          };
                                
                oos.OrderBy(a=>a.ID).OrderBy(a=>a.Status).Skip((pageIndex-1)*pageSize).Take(pageSize);

                stocks = oos.ToList<BEnterStock>();
            }
           
            return stocks;
        }

        /// <summary>
        /// Get enter stock details for one single enter stock
        /// </summary>
        /// <param name="enter_stock_id"></param>
        /// <returns></returns>
        public List<BEnterStockDetail> GetEnterStockDetails(int enter_stock_id)
        {
            List<BEnterStockDetail> details = new List<BEnterStockDetail>();
            int stockCount=0;
            List<BEnterStock> stocks = this.SearchEnterStocks(enter_stock_id,0,0,0,0,0,0,1,1,out stockCount);
            if (stockCount == 0)
            {
                throw new KMJXCException("编号为:"+enter_stock_id+ " 的入库单信息不存在");
            }

            if (stockCount > 1)
            {
                throw new KMJXCException("此入库单信息错误:"+enter_stock_id+" 对应条数据",ExceptionLevel.SYSTEM);
            }

            BEnterStock stock = stocks[0];
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var sdo = from sdd in db.Enter_Stock_Detail
                          where sdd.Enter_Stock_ID == enter_stock_id
                          orderby sdd.Product_ID ascending
                          select sdd;

                var sdoo = from sddd in sdo
                           select new BEnterStockDetail
                           {
                               EnterStock = stock,
                               Product = (from p in db.Product
                                          where p.Product_ID == sddd.Product_ID
                                          select new BProduct
                                          {
                                              Title = p.Name,
                                              ID = p.Product_ID,
                                              Code = p.Code,
                                              CreateTime = p.Create_Time,
                                          }).ToList<BProduct>()[0],
                               Quantity = sddd.Quantity,
                               Price = (double)sddd.Price,
                               Created = (int)sddd.Create_Date,
                               Invoiced = (bool)sddd.Have_Invoice,
                               InvoiceAmount = (double)sddd.Invoice_Amount,
                               InvoiceNumber = sddd.Invoice_Num
                           };

                details = sdoo.OrderBy(a => a.Created).ToList<BEnterStockDetail>();
            }
            
            return details;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BEnterStock GetEnterStockFullInfo(int id)
        {
            BEnterStock bstock = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {

                bstock = (from stock in db.Enter_Stock
                          where stock.Enter_Stock_ID == id
                          select new BEnterStock
                          {
                              ID = stock.Enter_Stock_ID,
                              Created = (Int32)stock.Enter_Date,
                              Created_By = (from user in db.User
                                            where user.User_ID == stock.User_ID
                                            select new BUser
                                            {
                                                ID=user.User_ID,
                                                Name=user.Name,
                                                Mall_ID=user.Mall_ID,
                                                Mall_Name=user.Mall_Name
                                            }).FirstOrDefault<BUser>(),
                              Shop = (from shop in db.Shop
                                      where shop.Shop_ID == stock.Shop_ID
                                      select new BShop
                                      {
                                          ID = shop.Shop_ID,
                                          Mall_ID = shop.Mall_Shop_ID,
                                          Created = (int)shop.Created,
                                          Description = shop.Description,
                                          Title = shop.Name
                                      }).FirstOrDefault<BShop>(),
                              Status = (int)stock.Status,
                              StoreHouse = (from house in db.Store_House
                                            where house.StoreHouse_ID == stock.StoreHouse_ID
                                            select new BStoreHouse
                                            {
                                                ID=house.StoreHouse_ID,
                                                Name=house.Title,
                                                Phone=house.Phone,
                                                Address=house.Address
                                            }).FirstOrDefault<BStoreHouse>(),
                              BuyID = (int)stock.Buy_ID                              

                          }).FirstOrDefault<BEnterStock>();

                if (bstock == null)
                {
                    throw new KMJXCException("编号为:"+id+" 的入库单信息不存在");
                }

                if (bstock != null)
                {
                    BuyManager buyManager = new BuyManager(this.CurrentUser,this.Shop,this.CurrentUserPermission); 
                    bstock.Buy = buyManager.GetBuyFullInfo(bstock.BuyID);
                    bstock.Details = (from detail in db.Enter_Stock_Detail
                                      where detail.Enter_Stock_ID == id
                                      select new BEnterStockDetail
                                      {
                                          Created = (int)detail.Create_Date,
                                          InvoiceAmount = (double)detail.Invoice_Amount,
                                          Invoiced = (bool)detail.Have_Invoice,
                                          InvoiceNumber = detail.Invoice_Num,
                                          Price = (double)detail.Price,
                                          Quantity = detail.Quantity,
                                          StockProductId = detail.Product_ID
                                      }).ToList<BEnterStockDetail>();
                }                           
            }
            return bstock;
        }

        /// <summary>
        /// Add new enter stock record
        /// </summary>
        /// <param name="stock">Instance of Enter_Stock object</param>
        /// <returns></returns>
        public bool CreateEnterStock(BEnterStock stock)
        {
            bool result = false;
            if (stock == null)
            {
                return result;
            }

            if (stock.BuyID <= 0) {
                throw new KMJXCException("入库单未包含验货单信息");
            }

            if (stock.Shop==null)
            {
                stock.Shop = new BShop() { ID = this.Shop_Id, Title=this.Shop.Name };
            }

            if (stock.StoreHouse ==null)
            {
                throw new KMJXCException("入库单未包含仓库信息");
            }

            if (stock.Created_By == null)
            {
                stock.Created_By = this.CurrentUser;
            }

            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                //update buy
                Buy dbBuy = (from buy in db.Buy where buy.Buy_ID == stock.BuyID select buy).FirstOrDefault<Buy>();
                if (dbBuy == null)
                {
                    throw new KMJXCException("编号为:"+stock.BuyID+" 的验货单没有找到");
                }

                if (dbBuy.Shop_ID != this.Shop.Shop_ID)
                {
                    throw new KMJXCException("编号为:" + stock.BuyID + " 为别的店铺的验货单，您不能操作，请不要再次尝试");
                }

                if (dbBuy.Status == 1)
                {
                    throw new KMJXCException("编号为:" + stock.BuyID + " 的验货单已经入库，不能再次入库");
                }
                Enter_Stock dbStock = new Enter_Stock();

                dbStock.Buy_ID = stock.BuyID;
                dbStock.Enter_Date = stock.Created;
                dbStock.Enter_Stock_ID = 0;
                dbStock.Shop_ID = this.Shop.Shop_ID;
                dbStock.StoreHouse_ID = stock.StoreHouse.ID;
                dbStock.User_ID = stock.Created_By.ID;
                dbStock.Status = 0;
                db.Enter_Stock.Add(dbStock);               
                db.SaveChanges();
                if (dbStock.Enter_Stock_ID <= 0)
                {
                    throw new KMJXCException("入库单创建失败");
                }
                result = true;

                if (stock.Details == null || stock.Details.Count == 0)
                {
                    stock.Details = (from d in db.Buy_Detail
                                     where d.Buy_ID == stock.BuyID
                                     select new BEnterStockDetail
                                     {
                                         Price = d.Price,
                                         Product = new BProduct() { ID=d.Product_ID },
                                         Quantity=d.Quantity
                                     }).ToList<BEnterStockDetail>();
                }

                result = result & this.CreateEnterStockDetails(dbStock, stock.Details, stock.UpdateStock);

                if (result)
                {
                    if (stock.UpdateStock)
                    {
                        dbStock.Status = 1;
                    }

                    if (dbBuy != null)
                    {
                        base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = dbBuy.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.CREATE_ENTER_STOCK }, Description = "" });
                        dbBuy.Status = 1;
                        db.SaveChanges();
                    }
                }               
            }

            return result;
        }

        /// <summary>
        /// Add multiple stock detail records
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool CreateEnterStockDetails(Enter_Stock dbstock,List<BEnterStockDetail> details,bool updateStock=false)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单产品信息的权限");
            }

            if (dbstock.Enter_Stock_ID <= 0)
            {
                throw new KMJXCException("");
            }

            if (details == null)
            {
                throw new KMJXCException("输入错误",ExceptionLevel.SYSTEM);
            }            

            KuanMaiEntities db = new KuanMaiEntities();
            List<Stock_Pile> stockPiles = (from sp in db.Stock_Pile where sp.Shop_ID == dbstock.Shop_ID select sp).ToList<Stock_Pile>();
          
            int totalQuantity = 0;

            List<Stock_Batch> batches=(from sb in db.Stock_Batch where sb.ShopID==dbstock.Shop_ID select sb ).ToList<Stock_Batch>();

            foreach (BEnterStockDetail detail in details)
            {
                Product tmp = (from p in db.Product where p.Product_ID == detail.Product.ID select p).FirstOrDefault<Product>();
                if (tmp == null)
                {
                    continue;
                }

                Enter_Stock_Detail dbDetail = new Enter_Stock_Detail();
                dbDetail.Create_Date = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                dbDetail.Enter_Stock_ID = dbstock.Enter_Stock_ID;
                dbDetail.Have_Invoice = detail.Invoiced;
                dbDetail.Invoice_Amount = detail.InvoiceAmount;
                dbDetail.Invoice_Num = detail.InvoiceNumber;
                dbDetail.Price = detail.Price;
                dbDetail.Product_ID = detail.Product.ID;
                dbDetail.Quantity = (int)detail.Quantity;
               
                totalQuantity += dbDetail.Quantity;
                int parentProductId = dbDetail.Product_ID;
                if (tmp.Parent_ID > 0)
                {
                    parentProductId = tmp.Parent_ID;
                }
                dbDetail.Parent_Product_ID = parentProductId;

                List<Stock_Batch> pBatches = (from b in batches where b.ProductID == parentProductId select b).ToList<Stock_Batch>();
                Stock_Batch batch = (from b in pBatches where b.Price==dbDetail.Price select b).FirstOrDefault<Stock_Batch>();
                if (batch == null)
                {                    
                    if (pBatches.Count == 1 && pBatches[0].Price == 0)
                    {
                        batch = pBatches[0];
                        batch.Price = dbDetail.Price;
                        batch.Name = "P" + batch.Price.ToString("0.00");
                    }
                    else
                    {
                        batch = new Stock_Batch() { ProductID = parentProductId, ParentProductID = 0, Price = dbDetail.Price, ShopID = dbstock.Shop_ID, Desc = "" };
                        batch.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        batch.Created_By = this.CurrentUser.ID;
                        batch.Name = "P" + batch.Price.ToString("0.00");
                        db.Stock_Batch.Add(batch);
                        db.SaveChanges();
                    }
                }

                dbDetail.Batch_ID = batch.ID;
                db.Enter_Stock_Detail.Add(dbDetail);

                if (updateStock)
                {                    
                    //update stock pile
                    Stock_Pile zeroPile = (from sp in stockPiles where sp.Product_ID == dbDetail.Product_ID && sp.StockHouse_ID == dbstock.StoreHouse_ID && sp.Quantity==0 select sp).FirstOrDefault<Stock_Pile>();
                    if (zeroPile != null && zeroPile.Batch_ID!=batch.ID)
                    {
                        db.Stock_Pile.Remove(zeroPile);
                        db.SaveChanges();
                    }

                    Stock_Pile stockPile = (from sp in stockPiles where sp.Product_ID == dbDetail.Product_ID && sp.StockHouse_ID == dbstock.StoreHouse_ID && sp.Batch_ID==dbDetail.Batch_ID select sp).FirstOrDefault<Stock_Pile>();
                    if (stockPile == null)
                    {
                        stockPile = new Stock_Pile();
                        stockPile.Product_ID = dbDetail.Product_ID;
                        stockPile.Shop_ID = this.Shop.Shop_ID;
                        stockPile.StockHouse_ID = dbstock.StoreHouse_ID;
                        stockPile.Quantity = dbDetail.Quantity;
                        stockPile.Price = dbDetail.Price;
                        stockPile.Batch_ID = dbDetail.Batch_ID;
                        stockPile.First_Enter_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        db.Stock_Pile.Add(stockPile);
                    }
                    else
                    {
                        stockPile.Quantity = stockPile.Quantity + dbDetail.Quantity;
                        stockPile.Price = dbDetail.Price;                        
                    }
                    Product product = null;

                    if (tmp.Parent_ID > 0)
                    {
                        product = (from p in db.Product
                                   join p1 in db.Product on p.Product_ID equals p1.Parent_ID
                                   where p1.Product_ID == dbDetail.Product_ID
                                   select p).FirstOrDefault<Product>();
                    }
                    else if (tmp.Parent_ID == 0)
                    {
                        product = tmp;
                    }

                    if (product != null)
                    {
                        product.Quantity += dbDetail.Quantity;
                    }
                }
            }
            
            try
            {
                db.SaveChanges();
                result = true;
            }
            catch(Exception ex)
            {
                throw new KMJXCException(ex.Message, ExceptionLevel.SYSTEM);
            }
            finally
            {
                db.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UpdateProductStockByEnterStock(int id)
        {
            bool result = false;
            if (this.CurrentUserPermission.UPDATE_ENTERSTOCK_TO_PRODUCT_STOCK == 0)
            {
                throw new KMJXCException("没有权限更新入库单到库存");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Enter_Stock stock=(from dbstock in db.Enter_Stock where dbstock.Enter_Stock_ID==id select dbstock).FirstOrDefault<Enter_Stock>();
                if (stock == null)
                {
                    throw new KMJXCException("编号为:"+id+" 的入库单不存在");
                }

                List<Enter_Stock_Detail> details=(from d in db.Enter_Stock_Detail where d.Enter_Stock_ID==id select d).ToList<Enter_Stock_Detail>();
                foreach (Enter_Stock_Detail eDetail in details)
                {
                    Product tmp=(from p in db.Product where p.Product_ID==eDetail.Product_ID select p).FirstOrDefault<Product>();
                    if (tmp == null)
                    {
                        continue;
                    }

                    Stock_Pile stockPile = (from sp in db.Stock_Pile where sp.Product_ID == eDetail.Product_ID && sp.StockHouse_ID == stock.StoreHouse_ID && sp.Batch_ID == eDetail.Batch_ID select sp).FirstOrDefault<Stock_Pile>();
                    if (stockPile == null)
                    {
                        stockPile = new Stock_Pile();
                        stockPile.Product_ID = eDetail.Product_ID;
                        stockPile.Shop_ID = this.Shop.Shop_ID;
                        stockPile.StockHouse_ID = stock.StoreHouse_ID;
                        stockPile.Quantity = eDetail.Quantity;
                        stockPile.Price = eDetail.Price;
                        stockPile.First_Enter_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        stockPile.Batch_ID = eDetail.Batch_ID;
                        db.Stock_Pile.Add(stockPile);
                    }
                    else
                    {
                        stockPile.Quantity = stockPile.Quantity + eDetail.Quantity;
                        stockPile.Price = eDetail.Price;
                    }
                    Product product = null;
                    if (tmp.Parent_ID > 0)
                    {
                        product = (from p in db.Product
                                   join p1 in db.Product on p.Product_ID equals p1.Parent_ID
                                   where p1.Product_ID == eDetail.Product_ID
                                   select p).FirstOrDefault<Product>();
                    }
                    else
                    {
                        product = tmp;
                    }

                    if (product != null)
                    {
                        product.Quantity += eDetail.Quantity;
                    }
                }

                stock.Status = 1;
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Add one enter stock detail record
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public bool EnterStockDetail(int stockId,BEnterStockDetail detail)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }

            if (detail.EnterStock == null && stockId<=0)
            {
                throw new KMJXCException("必须选择入库单");                
            }

            KuanMaiEntities db = new KuanMaiEntities();
            if (stockId > 0)
            {                
                Enter_Stock dbStock= (from st in db.Enter_Stock where st.Enter_Stock_ID == stockId select st).FirstOrDefault<Enter_Stock>();
                detail.EnterStock = new BEnterStock() { ID = stockId, StoreHouse = new BStoreHouse() { ID = dbStock.StoreHouse_ID } };
            }
            else
            {
                if (detail.EnterStock.ID <= 0)
                {
                    throw new KMJXCException("必须选择入库单");
                }
            }

            if (detail.Product == null)
            {
                throw new KMJXCException("必须指定商品");
            }

            if (detail.Quantity == 0)
            {
                throw new KMJXCException("数量必须大于零");
            }
            try
            {

                Enter_Stock_Detail dbDetail = new Enter_Stock_Detail();
                dbDetail.Create_Date = detail.Created;
                dbDetail.Enter_Stock_ID = detail.EnterStock.ID;
                dbDetail.Have_Invoice = detail.Invoiced;
                dbDetail.Invoice_Amount = detail.InvoiceAmount;
                dbDetail.Invoice_Num = detail.InvoiceNumber;
                dbDetail.Price = detail.Price;
                dbDetail.Product_ID = detail.Product.ID;
                dbDetail.Quantity = (int)detail.Quantity;
                db.Enter_Stock_Detail.Add(dbDetail);
                db.SaveChanges();

                //update stock pile
                Stock_Pile stockPile = (from sp in db.Stock_Pile where sp.Product_ID == dbDetail.Product_ID && sp.StockHouse_ID==detail.EnterStock.StoreHouse.ID select sp).FirstOrDefault<Stock_Pile>();
                if (stockPile != null)
                {
                    stockPile.Quantity = stockPile.Quantity + dbDetail.Quantity;
                    stockPile.Price = dbDetail.Price;
                    if (stockPile.First_Enter_Time == 0)
                    {
                        stockPile.First_Enter_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    }
                }

                result = true;
            }
            catch
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Single order leave stock
        /// </summary>
        /// <param name="lstock"></param>
        /// <returns></returns>
        public bool CreateLeaveStock(BLeaveStock leaveStock)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_LEAVE_STOCK == 0)
            {
                throw new KMJXCException("没有权限出库");
            }

            if (leaveStock.Sale == null || string.IsNullOrEmpty(leaveStock.Sale.Sale_ID))
            {
                throw new KMJXCException("必须选择订单出库");
            }

            if (leaveStock.Shop == null)
            {
                leaveStock.Shop = new BShop() { ID=this.Shop.Shop_ID};
                //throw new KMJXCException("必须选择店铺");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] csp_ids=(from child in this.DBChildShops select child.Shop_ID).ToArray<int>();
                if (csp_ids == null)
                {
                    csp_ids = new int[1];
                }

                List<Product> products=(from pdt in db.Product where pdt.Shop_ID==this.Shop.Shop_ID || pdt.Shop_ID==this.Main_Shop.Shop_ID || csp_ids.Contains(pdt.Shop_ID) select pdt).ToList<Product>();

                Leave_Stock dbStock = new Leave_Stock();
                dbStock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                dbStock.Leave_Date = leaveStock.LeaveDate;
                dbStock.Leave_Stock_ID = 0;
                dbStock.Sale_ID = leaveStock.Sale.Sale_ID;
                dbStock.Shop_ID = leaveStock.Shop.ID;
                dbStock.User_ID = this.CurrentUser.ID;
                db.Leave_Stock.Add(dbStock);
                db.SaveChanges();

                if (dbStock.Leave_Stock_ID <= 0)
                {
                    throw new KMJXCException("出库单创建失败");
                }

                if (leaveStock.Details != null)
                {
                    foreach (BLeaveStockDetail detail in leaveStock.Details)
                    {
                        Leave_Stock_Detail dbDetail = new Leave_Stock_Detail();
                        dbDetail.Leave_Stock_ID = dbStock.Leave_Stock_ID;
                        dbDetail.Price = detail.Price;
                        dbDetail.Quantity = detail.Quantity;
                        dbDetail.StoreHouse_ID = detail.StoreHouse.ID;
                        dbDetail.Product_ID = detail.ProductID;
                        if (detail.Parent_ProductID == 0)
                        {
                            dbDetail.Product_ID = (from p in products where p.Product_ID == detail.ProductID select p.Parent_ID).FirstOrDefault<int>();
                        }

                        db.Leave_Stock_Detail.Add(dbDetail);
                    }
                }

                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Batch leave stocks
        /// </summary>
        /// <param name="stocks"></param>
        public void CreateLeaveStocks(List<BLeaveStock> stocks)
        {
            foreach (BLeaveStock stock in stocks)
            {
                this.CreateLeaveStock(stock);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade_id"></param>
        /// <param name="order_id"></param>
        /// <param name="mall_product_id"></param>
        /// <param name="mall_sku_id"></param>
        /// <param name="parent_product_id"></param>
        /// <param name="product_id"></param>
        /// <param name="mapproduct"></param>
        public void CreateLeaveStockForMallTrade(string trade_id, string order_id, string mall_product_id, string mall_sku_id, int parent_product_id, int product_id, bool mapproduct = false)
        {
            if (string.IsNullOrEmpty(mall_product_id))
            {
                throw new KMJXCException("丢失商城宝贝编号");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Sale trade = (from t in db.Sale where t.Mall_Trade_ID == trade_id select t).FirstOrDefault<Sale>();
                if (trade == null)
                {
                    throw new KMJXCException("交易号为:" + trade_id + " 的交易不存在");
                }

                Sale_Detail order = (from sd in db.Sale_Detail where sd.Mall_Order_ID == order_id && sd.Mall_Trade_ID == trade_id select sd).FirstOrDefault<Sale_Detail>();
                if (order == null)
                {
                    throw new KMJXCException("交易号为:" + trade_id + " 的交易不存在");
                }

                Product dbproduct=(from p in db.Product where p.Product_ID==parent_product_id select p).FirstOrDefault<Product>();
                if (dbproduct == null)
                {                   
                    throw new KMJXCException("进销存产品编号为:" + parent_product_id + " 的产品不存在");
                }

                order.Parent_Product_ID = parent_product_id;
                order.Product_ID = parent_product_id;

                if (!string.IsNullOrEmpty(mall_sku_id))
                {
                    if (product_id <= 0)
                    {
                        throw new KMJXCException("交易产品有SKU属性，更新库存时必须选择进销存产品对应的销售属性");
                    }

                    Product childproduct = (from p in db.Product where p.Product_ID == product_id select p).FirstOrDefault<Product>();
                    if (childproduct == null)
                    {
                        throw new KMJXCException("进销存产品编号为:" + parent_product_id + ", 库存编号为:"+product_id+" 的产品不存在");
                    }

                    order.Product_ID = product_id;
                }
                
                Leave_Stock leaveStock = (from ls in db.Leave_Stock where ls.Sale_ID == trade_id select ls).FirstOrDefault<Leave_Stock>();
                if (leaveStock == null)
                {
                    leaveStock = new Leave_Stock();
                    leaveStock.Sale_ID = trade_id;
                    leaveStock.Leave_Date = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    leaveStock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    leaveStock.Shop_ID = trade.Shop_ID;
                    leaveStock.Status = 0;
                    leaveStock.User_ID = this.CurrentUser.ID;
                    db.Leave_Stock.Add(leaveStock);                   
                }

                Leave_Stock_Detail lsDetail = (from lsd in db.Leave_Stock_Detail where lsd.Order_ID == order_id select lsd).FirstOrDefault<Leave_Stock_Detail>();
                if (lsDetail != null)
                {
                    throw new KMJXCException("交易号为:" + trade_id + ",订单号为:" + order_id + " 的出库记录已经存在，不能重复出库");
                }
                int[] csp_ids = (from child in this.DBChildShops select child.Shop_ID).ToArray<int>();
                Store_House house = (from store in db.Store_House where store.Default == true && (store.Shop_ID == trade.Shop_ID || store.Shop_ID == this.Main_Shop.Shop_ID || csp_ids.Contains(store.Shop_ID)) select store).FirstOrDefault<Store_House>();
                int stockPileProductId = parent_product_id;
                if (product_id > 0)
                {
                    stockPileProductId = product_id;
                }

                Stock_Pile stockPile = null;
                lsDetail = new Leave_Stock_Detail();
                if (house != null)
                {
                    stockPile = (from sp in db.Stock_Pile where sp.Product_ID == stockPileProductId && sp.StockHouse_ID == house.StoreHouse_ID && sp.Quantity >= order.Quantity select sp).OrderBy(s=>s.Batch_ID).FirstOrDefault<Stock_Pile>();
                }

                if (stockPile == null)
                {                   
                    //get store house when it has the specific product
                    var tmpstockPile = from sp in db.Stock_Pile where sp.Product_ID == stockPileProductId && sp.Quantity >= order.Quantity select sp;
                    if (tmpstockPile.Count() > 0)
                    {
                        stockPile = tmpstockPile.OrderBy(s => s.Batch_ID).ToList<Stock_Pile>()[0];
                        Store_House tmpHouse = (from h in db.Store_House where h.StoreHouse_ID == stockPile.StockHouse_ID select h).FirstOrDefault<Store_House>();
                        if (tmpHouse != null)
                        {
                            house = tmpHouse;
                        }
                    }
                    else
                    {
                        //cannot leave stock, no stock pile
                        order.Status1 = (int)SaleDetailStatus.NO_ENOUGH_STOCK;
                        order.SyncResultMessage = "没有足够的库存，不能出库";
                    }
                }

                //no stock cannot leave stock
                if (stockPile != null)
                {
                    db.SaveChanges();
                    order.Status1 = (int)SaleDetailStatus.LEAVED_STOCK;
                    order.SyncResultMessage = "出库仓库：" + house.Title;
                    lsDetail.Leave_Stock_ID = leaveStock.Leave_Stock_ID;
                    lsDetail.Quantity = order.Quantity;
                    lsDetail.Price = order.Price;
                    lsDetail.StoreHouse_ID = house.StoreHouse_ID;
                    lsDetail.Order_ID = order_id;
                    lsDetail.Amount = (double)order.Amount;
                    lsDetail.Parent_Product_ID = parent_product_id;
                    lsDetail.Product_ID = product_id;
                    if (string.IsNullOrEmpty(mall_sku_id))
                    {
                        lsDetail.Product_ID = parent_product_id;
                    }

                    lsDetail.StoreHouse_ID = stockPile.StockHouse_ID;
                    lsDetail.Batch_ID = stockPile.Batch_ID;
                    //Update stock
                    stockPile.Quantity = stockPile.Quantity - order.Quantity;

                    //Update stock field in Product table
                    Product product = (from pdt in db.Product where pdt.Product_ID == lsDetail.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                    if (product != null)
                    {
                        product.Quantity = product.Quantity - order.Quantity;
                    }
                    lsDetail.Order_ID = order_id;

                    db.Leave_Stock_Detail.Add(lsDetail);
                    base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = leaveStock.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.CREATE_LEAVE_STOCK }, Description = "同步订单时未能成功自动出库更新库存，手动出库并更新库存" });
                    db.SaveChanges();
                }

                if (mapproduct)
                {
                    Mall_Product mallProduct=(from mp in db.Mall_Product where mp.Mall_ID==mall_product_id select mp).FirstOrDefault<Mall_Product>();
                    if (mallProduct != null)
                    {
                       
                        ShopManager shopManager = new ShopManager(this.CurrentUser, this.Shop, this.CurrentUserPermission);
                        if (shopManager.MapMallProduct(mall_product_id, parent_product_id))
                        {
                            mallProduct.Outer_ID = order.Parent_Product_ID;
                            if (!string.IsNullOrEmpty(mall_sku_id) && product_id>0)
                            {
                                Mall_Product_Sku sku = (from sk in db.Mall_Product_Sku where sk.Mall_ID == mallProduct.Mall_ID && sk.SKU_ID == mall_sku_id select sk).FirstOrDefault<Mall_Product_Sku>();

                                if (shopManager.MapSku(mall_sku_id, product_id))
                                {
                                    sku.Outer_ID = order.Product_ID;
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product_id"></param>
        public void CreateDefaultStockPile(Stock_Pile stockPile)
        {
            if (stockPile == null)
            {
                throw new KMJXCException("");
            }

            if (stockPile.Shop_ID < 0)
            {
                throw new KMJXCException("");
            }

            if (stockPile.Product_ID <= 0)
            {
                throw new KMJXCException("");
            }

            if (stockPile.Price < 0)
            {
                throw new KMJXCException("");
            }

            //if (stockPile.StockHouse_ID <= 0)
            //{
            //    throw new KMJXCException("");
            //}

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                stockPile.First_Enter_Time = 0;
                stockPile.LastLeave_Time = 0;
                db.Stock_Pile.Add(stockPile);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="house"></param>
        public void CreateStoreHouse(BStoreHouse house)
        {
            if (this.CurrentUserPermission.ADD_STORE_HOUSE == 0)
            {
                throw new KMJXCException("没有创建仓库的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Store_House dbHouse = new Store_House();
                int existing = (from h in db.Store_House where house.Name.Contains(h.Title) select h).Count();
                if (existing > 0)
                {
                    throw new KMJXCException("类似的仓库名称已经存在");
                }

                dbHouse.Phone = house.Phone;
                dbHouse.Title = house.Name;
                dbHouse.Address = house.Address;
                dbHouse.Guard = 0;
                dbHouse.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                dbHouse.User_ID = this.CurrentUser.ID;
                dbHouse.Default = house.IsDefault;
                dbHouse.Shop_ID = this.Shop.Shop_ID;
                if ((bool)dbHouse.Default)
                {
                    Store_House defaultHouse=(from hu in db.Store_House where hu.Default==true select hu).FirstOrDefault<Store_House>();
                    if (defaultHouse != null)
                    {
                        defaultHouse.Default = false;
                    }
                }
                db.Store_House.Add(dbHouse);               
                db.SaveChanges();
                base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = dbHouse.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.CREATE_STOREHOUSE }, Description = "" });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="house"></param>
        public void UpdateStoreHouse(BStoreHouse house)
        {
            if (this.CurrentUserPermission.UPDATE_STORE_HOUSE == 0)
            {
                throw new KMJXCException("没有创建仓库的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Store_House dbHouse = (from huse in db.Store_House where huse.StoreHouse_ID==house.ID select huse).FirstOrDefault<Store_House>();

                if (dbHouse == null) 
                {
                    throw new KMJXCException("编辑的仓库不存在");
                }

                int existing = (from h in db.Store_House where house.Name.Contains(h.Title) && h.StoreHouse_ID!=house.ID select h).Count();
                if (existing > 0)
                {
                    throw new KMJXCException("类似的仓库名称已经存在");
                }

                dbHouse.Phone = house.Phone;
                dbHouse.Title = house.Name;
                dbHouse.Address = house.Address;
                dbHouse.Guard = 0;
                dbHouse.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                dbHouse.User_ID = this.CurrentUser.ID;
                dbHouse.Default = house.IsDefault;
                dbHouse.Shop_ID = this.Shop.Shop_ID;
                if ((bool)dbHouse.Default)
                {
                    Store_House defaultHouse = (from hu in db.Store_House where hu.Default == true select hu).FirstOrDefault<Store_House>();
                    if (defaultHouse != null)
                    {
                        defaultHouse.Default = false;
                    }
                }

                base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = dbHouse.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.UPDATE_STOREHOUSE }, Description = "" });
                db.SaveChanges();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BStoreHouse> GetStoreHouses()
        {
            List<BStoreHouse> houses = new List<BStoreHouse>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] spids = (from sp in this.DBChildShops select sp.Shop_ID).ToArray<int>();
                var hs = from house in db.Store_House select house;
                if (this.Shop.Shop_ID==this.Main_Shop.Shop_ID)
                {
                    hs = hs.Where(a => (a.Shop_ID == this.Shop.Shop_ID || spids.Contains(a.Shop_ID)));
                }
                else
                {
                    hs = hs.Where(a => a.Shop_ID == this.Shop.Shop_ID || a.Shop_ID == this.Main_Shop.Shop_ID);
                }

                var tmp = from hos in hs
                          select new BStoreHouse
                          {
                              ID = hos.StoreHouse_ID,
                              Name = hos.Title,
                              Created = (int)hos.Create_Time,
                              Address = hos.Address,
                              Phone = hos.Phone,
                              IsDefault = (bool)hos.Default,
                              Guard = (from user in db.User
                                       where user.User_ID == hos.User_ID
                                       select new BUser
                                       {
                                           ID = user.User_ID,
                                           Mall_ID = user.Mall_ID,
                                           Mall_Name = user.Mall_Name,
                                           Name = user.Name
                                       }).FirstOrDefault<BUser>(),
                              Created_By = (from user in db.User
                                            where user.User_ID == hos.User_ID
                                            select new BUser
                                                {
                                                    ID = user.User_ID,
                                                    Mall_ID = user.Mall_ID,
                                                    Mall_Name = user.Mall_Name,
                                                    Name = user.Name
                                                }).FirstOrDefault<BUser>(),
                              Shop = (from shop in db.Shop
                                      where shop.Shop_ID == hos.Shop_ID
                                      select new BShop
                                          {
                                              ID = shop.Shop_ID,
                                              Title = shop.Name
                                          }).FirstOrDefault<BShop>()
                          };

                houses = tmp.ToList<BStoreHouse>();

                foreach (BStoreHouse house in houses)
                {
                    if (house.Shop.ID != this.Shop.Shop_ID)
                    {
                        if (house.Shop.ID == this.Main_Shop.Shop_ID)
                        {
                            house.FromMainShop = true;
                        }
                        else
                        {
                            house.FromChildShop = true;
                        }
                    }
                    else
                    {
                        house.FromMainShop = false;
                        house.FromChildShop = false;
                    }
                }
            }

            return houses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product_id"></param>
        public List<BProduct> GetProductStockDetails(int product_id)
        {
            List<BProduct> stores = new List<BProduct>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] child_product=(from product in db.Product where product.Parent_ID==product_id select product.Product_ID).ToArray<int>();

                List<BProductProperty> childs = (from prop in db.Product_Specifications
                                                 join ps in db.Product_Spec on prop.Product_Spec_ID equals ps.Product_Spec_ID
                                                 join psv in db.Product_Spec_Value on prop.Product_Spec_Value_ID equals psv.Product_Spec_Value_ID
                                                 where child_product.Contains(prop.Product_ID)
                                                 select new BProductProperty
                                                 {
                                                     ProductID = prop.Product_ID,
                                                     PID = prop.Product_Spec_ID,
                                                     PName = ps.Name,
                                                     PVID = prop.Product_Spec_Value_ID,
                                                     PValue = psv.Name
                                                 }).OrderBy(p=>p.PID).ToList<BProductProperty>();

                var stocks = from stock in db.Stock_Pile
                             where child_product.Contains(stock.Product_ID)
                             group stock by stock.Product_ID into lStock
                             select new
                             {
                                 Product_ID=lStock.Key,
                                 Quantity = lStock.Sum(s=>s.Quantity)
                             };

                var tmp = from product in db.Product
                          join stock in stocks on product.Product_ID equals stock.Product_ID into lStock
                          from l_stock in lStock.DefaultIfEmpty()
                          join pproduct in db.Product on product.Parent_ID equals pproduct.Product_ID into lPProduct
                          from l_ppproduct in lPProduct.DefaultIfEmpty()
                          where child_product.Contains(product.Product_ID)
                          select new BProduct
                          {
                              Title = l_ppproduct.Name,
                              Quantity = l_stock.Quantity,
                              ID = product.Product_ID
                              
                          };

                stores = tmp.ToList<BProduct>();
                foreach (BProduct product in stores)
                {
                    product.Properties = (from prop in childs where prop.ProductID == product.ID select prop).ToList<BProductProperty>();
                }
            }

            return stores;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product_ids"></param>
        /// <param name="categories"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<BProduct> SearchProductsStocks(int[] product_ids, int category_id, int storeHouse, string keywords, int page, int pageSize, out int total)
        {
            total = 0;
            List<BProduct> bProducts = new List<BProduct>();
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
                int[] child_ids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (child_ids == null)
                {
                    child_ids = new int[] { 0 };
                }

                var products = from product in db.Product      
                               where product.Parent_ID==0
                               select product;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    products = products.Where(product => (product.Shop_ID == this.Shop.Shop_ID || child_ids.Contains(product.Shop_ID)));
                }
                else
                {
                    products = products.Where(product => (product.Shop_ID == this.Shop.Shop_ID || product.Shop_ID==this.Main_Shop.Shop_ID));
                }

                if (category_id >0)
                {
                    Product_Class cate = (from ca in db.Product_Class where ca.Product_Class_ID == category_id select ca).FirstOrDefault<Product_Class>();
                    if (cate != null)
                    {
                        if (cate.Parent_ID == 0)
                        {
                            int[] ccids = (from c in db.Product_Class where c.Parent_ID == category_id select c.Product_Class_ID).ToArray<int>();
                            if (ccids != null && ccids.Length > 0)
                            {
                                products = products.Where(a => ccids.Contains(a.Product_Class_ID));
                            }
                            else
                            {
                                products = products.Where(a => a.Product_Class_ID == category_id);
                            }
                        }
                        else
                        {
                            products = products.Where(a => a.Product_Class_ID == category_id);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(keywords))
                {
                    products = products.Where(a=>a.Name.Contains(keywords.Trim()));
                }

                if (product_ids != null)
                {
                    products = products.Where(a=>product_ids.Contains(a.Product_ID));
                }

                if (storeHouse > 0)
                {
                    int[] pids = (from sp in db.Stock_Pile where sp.StockHouse_ID==storeHouse select sp.Product_ID).ToArray<int>();

                    if (pids != null && pids.Length > 0)
                    {
                       
                    }

                    products = products.Where(a => pids.Contains(a.Product_ID));
                }

                products = products.OrderBy(a=>a.Shop_ID);

                total = products.Count();

                if (total > 0)
                {
                    bProducts = (from Pdt in products
                              select new BProduct
                              {
                                  Description = Pdt.Description,
                                  Shop = (from sp in db.Shop where sp.Shop_ID == Pdt.Shop_ID select sp).FirstOrDefault<Shop>(),
                                  Price = Pdt.Price,
                                  ID = Pdt.Product_ID,
                                  Title = Pdt.Name,
                                  CreateTime = Pdt.Create_Time,
                                  Code = Pdt.Code,
                                  Quantity = (int)Pdt.Quantity,
                                  Unit = (from u in db.Product_Unit where u.Product_Unit_ID == Pdt.Product_Unit_ID select u).FirstOrDefault<Product_Unit>(),
                                  Category = (from c in db.Product_Class
                                              where Pdt.Product_Class_ID == c.Product_Class_ID
                                              select new BCategory
                                              {
                                                  Name = c.Name,
                                                  ID = c.Product_Class_ID,
                                              }).FirstOrDefault<BCategory>(),
                                  User = (from u in db.User
                                          where u.User_ID == Pdt.User_ID
                                          select new BUser
                                          {
                                              ID = u.User_ID,
                                              Mall_Name = u.Mall_Name,
                                              Mall_ID = u.Mall_ID,
                                          }).FirstOrDefault<BUser>()
                              }).OrderBy(a=>a.ID).Skip((page-1)*pageSize).Take(pageSize).ToList<BProduct>();

                    
                    int[] products_ids=(from p in bProducts select p.ID).ToArray<int>();
                    var tmpChildren = from p in db.Product where products_ids.Contains(p.Parent_ID) select p;
                    List<Product> childrenProducts = tmpChildren.ToList<Product>();
                    int[] child_products_ids=(from p in childrenProducts select p.Product_ID).ToArray<int>();
                    List<Stock_Pile> stocks = (from s in db.Stock_Pile where (products_ids.Contains(s.Product_ID) || child_products_ids.Contains(s.Product_ID)) select s).ToList<Stock_Pile>();
                    foreach (BProduct product in bProducts)
                    {
                        int[] children=(from c in childrenProducts where c.Parent_ID==product.ID select c.Product_ID).ToArray<int>();
                        if (children != null && children.Length > 0)
                        {
                            product.Quantity = (from s in stocks where children.Contains(s.Product_ID) select s.Quantity).Sum();
                        }
                        else
                        {
                            product.Quantity = (from s in stocks where s.Product_ID==product.ID select s.Quantity).Sum();
                        }

                        if (product.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                        {
                            product.FromMainShop = true;
                        }
                        else if (child_ids != null && child_ids.Length > 0 && child_ids.Contains(product.Shop.Shop_ID))
                        {
                            product.FromChildShop = true;
                        }
                    }
                }
            }

            return bProducts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stocks"></param>
        /// <param name="messages"></param>
        public void UpdateProductsStocks(List<BStock> stocks, out List<string> messages)
        {
            messages = new List<string>();
            if (this.CurrentUserPermission.UPDATE_STOCKS == 0)
            {
                throw new KMJXCException("没有权限盘点库存数量");
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<BStock> needUpdatedStocks = new List<BStock>();

                foreach (BStock stock in stocks)
                {
                    if (stock.Product == null || stock.Product.ID<=0 || stock.StoreHouse==null || stock.StoreHouse.ID<=0 || stock.Batch==null || stock.Batch.ID<=0) 
                    {
                        continue;
                    }

                    needUpdatedStocks.Add(stock);
                }

                int[] products=(from stock in needUpdatedStocks select stock.Product.ID).ToArray<int>();  

                List<Stock_Pile> dbStocks=(from stock in db.Stock_Pile where products.Contains(stock.Product_ID) select stock).ToList<Stock_Pile>();
                foreach (BStock stock in needUpdatedStocks)
                {
                    Stock_Pile dbstock=(from sbsk in dbStocks where sbsk.Product_ID==stock.Product.ID && sbsk.StockHouse_ID==stock.StoreHouse.ID && sbsk.Batch_ID==stock.Batch.ID select sbsk).FirstOrDefault<Stock_Pile>();
                    if (dbstock != null)
                    {
                        dbstock.Quantity = stock.Quantity;
                    }
                }
                base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = this.Shop.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.UPDATE_STOCK }, Description = "" });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product_ids"></param>
        /// <param name="store_house_ids"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="paging"></param>
        /// <returns></returns>
        public List<BStock> SearchStocks(List<int> product_ids, List<int> store_house_ids,int page,int pageSize,out int total,bool paging=false)
        {
            List<BStock> stocks = new List<BStock>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                total = 0;
                if (page <= 0)
                {
                    page = 1;
                }

                if (pageSize <= 0)
                {
                    pageSize = 30;
                }
                var tmpProducts = from p in db.Product
                                  select p;

                if (product_ids != null && product_ids.Count > 0)
                {
                    int[] tmpProduct_ids = product_ids.ToArray<int>();
                    tmpProducts = tmpProducts.Where(p => tmpProduct_ids.Contains(p.Product_ID) || tmpProduct_ids.Contains(p.Parent_ID));
                }

                
                int[] all_product_ids=(from p in tmpProducts select p.Product_ID).ToArray<int>();
                var tmpStocks = from s in db.Stock_Pile
                                join p in db.Product on s.Product_ID equals p.Product_ID into LProduct
                                from l_p in LProduct.DefaultIfEmpty()
                                join h in db.Store_House on s.StockHouse_ID equals h.StoreHouse_ID into LHouse
                                from l_h in LHouse.DefaultIfEmpty()
                                join batch in db.Stock_Batch on s.Batch_ID equals batch.ID into LBatch
                                from l_batch in LBatch.DefaultIfEmpty()
                                select new BStock
                                {
                                    ID = s.StockPile_ID,
                                    Parent_Product_ID = l_p.Parent_ID > 0 ? l_p.Parent_ID : l_p.Product_ID,
                                    Product = new BProduct
                                    {
                                        Title = l_p.Name,
                                        ID = l_p.Product_ID
                                    },
                                    Quantity = s.Quantity,
                                    StoreHouse = new BStoreHouse { ID = l_h.StoreHouse_ID, Name = l_h.Title },
                                    Batch = l_batch != null ? new BStockBatch { ID=l_batch.ID,Name=l_batch.Name,Price=l_batch.Price } : new BStockBatch {ID=0,Name="",Price=0 }
                                };

                if (store_house_ids != null && store_house_ids.Count > 0)
                {
                    int[] tmp_house_ids = store_house_ids.ToArray<int>();
                    tmpStocks = tmpStocks.Where(s => tmp_house_ids.Contains(s.StoreHouse.ID));
                }

                if (all_product_ids != null && all_product_ids.Length > 0)
                {
                    tmpStocks = tmpStocks.Where(s=>all_product_ids.Contains(s.Product.ID));
                }

                List<BProductProperty> properties = (from pv in db.Product_Specifications
                                                     join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                     from l_prop in LProp.DefaultIfEmpty()
                                                     join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                     from l_propv in LPropv.DefaultIfEmpty()
                                                     where all_product_ids.Contains(pv.Product_ID)
                                                     select new BProductProperty
                                                     {
                                                         PID = pv.Product_Spec_ID,
                                                         PName = l_prop.Name,
                                                         ProductID = pv.Product_ID,
                                                         PValue = l_propv.Name,
                                                         PVID = pv.Product_Spec_Value_ID
                                                     }).ToList<BProductProperty>();

                tmpStocks = tmpStocks.OrderBy(s=>s.Product.ID);
                total = tmpStocks.Count();

                if (paging)
                {
                    stocks = tmpStocks.Skip((page - 1) * pageSize).Take(pageSize).ToList<BStock>();
                }
                else
                {
                    stocks = tmpStocks.ToList<BStock>();
                }

                foreach (BStock stock in stocks)
                {
                    if (stock.Product != null)
                    {
                        stock.Product.Properties=(from prop in properties where prop.ProductID == stock.Product.ID select prop).ToList<BProductProperty>();
                    }
                }
            }
            return stocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product_ids"></param>
        /// <param name="store_house_ids"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="paging"></param>
        /// <returns></returns>
        public List<BStock> SearchProductsStocks(List<int> product_ids, List<int> store_house_ids, int page, int pageSize, out int total, bool paging = false)
        {
            List<BStock> stocks = new List<BStock>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                total = 0;
                if (page <= 0)
                {
                    page = 1;
                }

                if (pageSize <= 0)
                {
                    pageSize = 30;
                }
                var tmpProducts = from p in db.Product
                                  select p;

                if (product_ids != null && product_ids.Count > 0)
                {
                    int[] tmpProduct_ids = product_ids.ToArray<int>();
                    tmpProducts = tmpProducts.Where(p => tmpProduct_ids.Contains(p.Product_ID) || tmpProduct_ids.Contains(p.Parent_ID));
                }


                int[] all_product_ids = (from p in tmpProducts select p.Product_ID).ToArray<int>();
                var tmpStocks = from s in db.Stock_Pile
                                join p in db.Product on s.Product_ID equals p.Product_ID into LProduct
                                from l_p in LProduct.DefaultIfEmpty()
                                join h in db.Store_House on s.StockHouse_ID equals h.StoreHouse_ID into LHouse
                                from l_h in LHouse.DefaultIfEmpty()
                                select new BStock
                                {
                                    ID = s.StockPile_ID,
                                    Parent_Product_ID = l_p.Parent_ID > 0 ? l_p.Parent_ID : l_p.Product_ID,
                                    Product = new BProduct
                                    {
                                        Title = l_p.Name,
                                        ID = l_p.Product_ID
                                    },
                                    Quantity = s.Quantity,
                                    StoreHouse = new BStoreHouse { ID = l_h.StoreHouse_ID, Name = l_h.Title }
                                };

                if (store_house_ids != null && store_house_ids.Count > 0)
                {
                    int[] tmp_house_ids = store_house_ids.ToArray<int>();
                    tmpStocks = tmpStocks.Where(s => tmp_house_ids.Contains(s.StoreHouse.ID));
                }

                if (all_product_ids != null && all_product_ids.Length > 0)
                {
                    tmpStocks = tmpStocks.Where(s => all_product_ids.Contains(s.Product.ID));
                }

                List<BProductProperty> properties = (from pv in db.Product_Specifications
                                                     join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                     from l_prop in LProp.DefaultIfEmpty()
                                                     join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                     from l_propv in LPropv.DefaultIfEmpty()
                                                     where all_product_ids.Contains(pv.Product_ID)
                                                     select new BProductProperty
                                                     {
                                                         PID = pv.Product_Spec_ID,
                                                         PName = l_prop.Name,
                                                         ProductID = pv.Product_ID,
                                                         PValue = l_propv.Name,
                                                         PVID = pv.Product_Spec_Value_ID
                                                     }).ToList<BProductProperty>();

                total = tmpStocks.Count();

                if (paging)
                {
                    stocks = tmpStocks.Skip((page - 1) * pageSize).Take(pageSize).ToList<BStock>();
                }
                else
                {
                    stocks = tmpStocks.ToList<BStock>();
                }

                foreach (BStock stock in stocks)
                {
                    if (stock.Product != null)
                    {
                        stock.Product.Properties = (from prop in properties where prop.ProductID == stock.Product.ID select prop).ToList<BProductProperty>();
                    }
                }
            }
            return stocks;
        }

        /// <summary>
        /// Perform stock analysis
        /// </summary>
        /// <param name="shop_id">Shop ID</param>
        /// <param name="includeChildren"></param>
        /// <param name="includeMain"></param>
        /// <returns></returns>
        public List<BStockAnalysis> StockAnalysis(int shop_id = 0,bool includeChildren=false,bool includeMain=false)
        {
            List<BStockAnalysis> analysis=new List<BStockAnalysis>();
            using(KuanMaiEntities db=new KuanMaiEntities())
            {
                var tmpCate = from cate in db.Product_Class
                              where cate.Parent_ID==0
                              select cate;

               
                if (shop_id > 0)
                {
                    tmpCate = tmpCate.Where(c => c.Shop_ID == shop_id);
                }
                else
                {
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        if (includeChildren)
                        {
                            int[] children = (from c in this.ChildShops select c.ID).ToArray<int>();
                            tmpCate = tmpCate.Where((c => children.Contains(c.Shop_ID) || c.Shop_ID == this.Shop.Shop_ID));
                        }
                        else
                        {
                            tmpCate = tmpCate.Where(c => c.Shop_ID == this.Shop.Shop_ID);
                        }
                    }
                    else
                    {
                        if (!includeMain)
                        {
                            tmpCate = tmpCate.Where(c => c.Shop_ID == this.Shop.Shop_ID);
                        }
                        else
                        {
                            tmpCate = tmpCate.Where(c => (c.Shop_ID == this.Shop.Shop_ID || c.Shop_ID==this.Main_Shop.Shop_ID));
                        }
                    }
                }

                List<BCategory> categories = (from c in tmpCate
                                              select new BCategory
                                              {
                                                  ID = c.Product_Class_ID,
                                                  Name = c.Name
                                              }).ToList<BCategory>();

                int[] cate_ids = (from c in categories select c.ID).ToArray<int>();

                List<Product_Class> childrenCate=(from c in db.Product_Class where cate_ids.Contains((int)c.Parent_ID) select c).ToList<Product_Class>();

                int [] childred_cate_ids=(from c in childrenCate select c.Product_Class_ID).ToArray<int>();
                List<Product> products=(from p in db.Product
                                        where (cate_ids.Contains(p.Product_Class_ID) || childred_cate_ids.Contains(p.Product_Class_ID))
                                        select p).ToList<Product>();

                int[] product_ids=(from p in products select p.Product_ID).ToArray<int>();

                long timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                long timeOneYearBefore = timeNow-24*3600*365;

                string[] sale_ids=(from s in db.Sale where s.Sale_Time>timeOneYearBefore && s.Sale_Time<timeNow select s.Mall_Trade_ID).ToArray<string>();

                List<Sale_Detail> saleDetails=(from s in db.Sale_Detail 
                                               where product_ids.Contains(s.Product_ID) && sale_ids.Contains(s.Mall_Trade_ID)
                                               && (s.Status1 != (int)SaleDetailStatus.BACK_STOCK && s.Status1 != (int)SaleDetailStatus.REFOUND_BEFORE_SEND && s.Status1 != (int)SaleDetailStatus.REFOUND_HANDLED && s.Status1 != (int)SaleDetailStatus.REFOUNDED_WAIT_HANDLE)
                                               select s).ToList<Sale_Detail>();

                double totalSalesAmount=(from s in saleDetails select (double)s.Amount).Sum();
                int totalProductCount = products.Count;
                foreach (BCategory category in categories)
                {
                    BStockAnalysis analy = new BStockAnalysis();
                    analy.Category = category;
                    int[] cates=(from c in childrenCate where c.Parent_ID==category.ID select c.Product_Class_ID).ToArray<int>();

                    List<Product> cProducts;
                    if (cates != null && cates.Length > 0)
                    {
                        cProducts = (from p in products where (p.Product_Class_ID == category.ID || cates.Contains(p.Product_Class_ID)) select p).ToList<Product>();
                    }
                    else
                    {
                        cProducts = (from p in products where p.Product_Class_ID == category.ID select p).ToList<Product>();
                    }

                    int[] pIds=(from p in cProducts select p.Product_ID).ToArray<int>();
                    if (pIds == null || pIds.Length <= 0)
                    {
                        analy.FiscalAmount = 0;
                        analy.FiscalAmountPercentage = 0;
                        analy.ProductCount = 0;
                        analy.ProductCountPercentage = 0;
                    }
                    else
                    {
                        analy.ProductCount = cProducts.Count;
                        analy.ProductCountPercentage =Math.Round((double) analy.ProductCount / totalProductCount,2);
                        analy.FiscalAmount = (from s in saleDetails where pIds.Contains(s.Product_ID) select (double)s.Amount).Sum();
                        analy.FiscalAmountPercentage = Math.Round(analy.FiscalAmount / totalSalesAmount,2);
                    }

                    analysis.Add(analy);
                }
            }
            return analysis;
        }

        /// <summary>
        /// Search stock batches
        /// </summary>
        /// <param name="shop_id">Shop</param>
        /// <param name="batchName">Keyword of batch name</param>
        /// <param name="productKeyWord">Keyword of product name</param>
        /// <param name="page">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="total">Total records</param>
        /// <returns>A list of BStockBatch</returns>
        public List<BStockBatch> SearchBatches(int shop_id,string batchName,string productKeyWord,int page,int pageSize,out int total)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            total=0;

            List<BStockBatch> batches=new List<BStockBatch>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from batch in db.Stock_Batch
                          select batch;
                if (shop_id > 0)
                {
                    tmp = tmp.Where(b => b.ShopID == shop_id);
                }
                else
                {
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        int[] childs = (from c in this.ChildShops select c.ID).ToArray<int>();
                        tmp = tmp.Where(b => (childs.Contains(b.ShopID) || b.ShopID == this.Shop.Shop_ID));
                    }
                    else
                    {
                        tmp = tmp.Where(b=>b.ShopID==this.Shop.Shop_ID);
                    }
                }

                if (!string.IsNullOrEmpty(batchName))
                {
                    tmp = tmp.Where(b=>b.Name.Contains(batchName));
                }

                if (!string.IsNullOrEmpty(productKeyWord))
                {
                    var products = from p in db.Product
                                   where p.Parent_ID == 0 && p.Name.Contains(productKeyWord)
                                   select p.Product_ID;

                    tmp = tmp.Where(b=>products.Contains(b.ProductID));
                }

                total = tmp.Count();
                if (total > 0)
                {
                    var x = from batch in tmp
                            join shop in db.Shop on batch.ShopID equals shop.Shop_ID into LShop
                            from l_shop in LShop.DefaultIfEmpty()
                            join user in db.User on batch.Created_By equals user.User_ID into LUser
                            from l_user in LUser.DefaultIfEmpty()
                            join product in db.Product on batch.ProductID equals product.Product_ID into LProduct
                            from l_product in LProduct.DefaultIfEmpty()
                            select new BStockBatch
                            {
                                ID = batch.ID,
                                Name = batch.Name,
                                Created = batch.Created,
                                Created_By = l_user != null ? new BUser { ID = batch.Created_By, Mall_ID = l_user.Mall_ID, Mall_Name = l_user.Mall_Name } : new BUser { ID = 0, Mall_ID = "", Mall_Name = ""},
                                Price = batch.Price,
                                Product = new BProduct { ID = batch.ProductID, ParentID = l_product.Parent_ID, Title = l_product.Name },
                                Shop = new BShop { ID=l_shop.Shop_ID,Title=l_shop.Name }
                            };

                    batches = (from b in x select b).OrderBy(b=>b.ID).ThenBy(b=>b.Product.ID).Skip((page-1)*pageSize).Take(pageSize).ToList<BStockBatch>();
                }
            }
            return batches;
        }
    }
}
