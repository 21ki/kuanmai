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
        /// <returns></returns>
        public BSyncTime GetSyncTime(int syncType=0)
        {
            BSyncTime time = null;
            using (KuanMaiEntities db = new KuanMaiEntities()) 
            {
                time = (from stime in db.Sale_SyncTime
                        join user in db.User on stime.SyncUser equals user.User_ID  into luser
                        from l_user in luser.DefaultIfEmpty()
                        where stime.ShopID == this.Shop.Shop_ID && stime.SyncType == syncType
                        orderby stime.LastSyncTime descending
                        select new BSyncTime
                        {                            
                            Last = stime.LastSyncTime,
                            LastTradeModifiedEndTime=stime.LastTradeModifiedEndTime,
                            LastTradeStartEndTime=stime.LastTradeStartEndTime,
                            SyncUser = new BUser
                            {
                                ID = l_user.User_ID,
                                Name = l_user.Name,
                                Mall_ID = l_user.Mall_ID,
                                Mall_Name = l_user.Mall_Name                               
                            }
                        }).FirstOrDefault<BSyncTime>();
            }
            return time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trades"></param>
        private void HandleBackTrades(List<BSale> trades)
        {
            if (trades == null || trades.Count==0)
            {
                return;
            }

            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                string[] sale_ids=(from trade in trades select trade.Sale_ID).ToArray<string>();
                List<Leave_Stock> leave_Stocks=(from ls in db.Leave_Stock where sale_ids.Contains(ls.Sale_ID) select ls).ToList<Leave_Stock>();
                int[] ls_ids=(from ls in leave_Stocks select ls.Leave_Stock_ID).ToArray<int>();
                List<Leave_Stock_Detail> leave_stock_Details=(from lsd in db.Leave_Stock_Detail where ls_ids.Contains(lsd.Leave_Stock_ID) select lsd).ToList<Leave_Stock_Detail>();
                List<Back_Sale> back_Sales=(from backSale in db.Back_Sale where sale_ids.Contains(backSale.Sale_ID) select backSale).ToList<Back_Sale>();
                int[] bs_ids=(from bs in back_Sales select bs.Back_Sale_ID).ToArray<int>();
                List<Back_Sale_Detail> back_Sale_Details=(from bsd in db.Back_Sale_Detail where bs_ids.Contains(bsd.Back_Sale_ID) select bsd).ToList<Back_Sale_Detail>();
                foreach (BSale trade in trades)
                {
                    Leave_Stock ls=(from leaveStock in leave_Stocks where leaveStock.Sale_ID==trade.Sale_ID select leaveStock).FirstOrDefault<Leave_Stock>();
                    Back_Sale bSale=(from b_Sale in back_Sales where b_Sale.Sale_ID==trade.Sale_ID select b_Sale).FirstOrDefault<Back_Sale>();
                    bool isNew = false;
                    double totalRefound = 0;
                    if (bSale == null)
                    {
                        isNew = true;                       
                        bSale = new Back_Sale();
                    }

                    bSale.Back_Date = trade.Synced;                    
                    bSale.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    bSale.Description = "";
                    bSale.Sale_ID = trade.Sale_ID;
                    bSale.Shop_ID = this.Shop.Shop_ID;
                    bSale.Status = 0;
                    bSale.User_ID = this.CurrentUser.ID;

                    if (isNew)
                    {
                        db.Back_Sale.Add(bSale);
                        db.SaveChanges();
                    }
                    if (trade.Orders != null)
                    {
                        foreach (BOrder order in trade.Orders)
                        {
                            //1- refound succeed
                            //0- is normal
                            if (order.Status1 != 1)
                            {
                                continue;
                            }

                            if (order.Product_ID <= 0)
                            {
                                continue;
                            }

                            Back_Sale_Detail dbSaleDetail = (from bsd in back_Sale_Details where bsd.Order_ID==order.Order_ID select bsd).FirstOrDefault<Back_Sale_Detail>();
                            if (dbSaleDetail == null)
                            {                                
                                dbSaleDetail = new Back_Sale_Detail();
                                dbSaleDetail.Order_ID = order.Order_ID;
                                dbSaleDetail.Back_Sale_ID = bSale.Back_Sale_ID;
                                dbSaleDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                                dbSaleDetail.Description = "同步商城订单时处理有退货的订单";
                                dbSaleDetail.Parent_Product_ID = order.Parent_Product_ID;
                                dbSaleDetail.Price = order.Price;
                                dbSaleDetail.Product_ID = order.Product_ID;
                                dbSaleDetail.Quantity = order.Quantity;
                                dbSaleDetail.Status = 0;
                                dbSaleDetail.Refound = order.Amount;
                                totalRefound += order.Amount;
                                db.Back_Sale_Detail.Add(dbSaleDetail);
                            }
                        }
                        bSale.Amount = totalRefound;  
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                db.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trades"></param>
        private void CreateLeaveStocks(List<BSale> trades)
        {
            if (trades == null)
            {
                return;
            }

            KuanMaiEntities db = new KuanMaiEntities();

            int[] csp_ids = (from child in this.ChildShops select child.Shop_ID).ToArray<int>();
            if (csp_ids == null)
            {
                csp_ids = new int[1];
            }

            List<Product> products = (from pdt in db.Product where (pdt.Shop_ID == this.Shop.Shop_ID || pdt.Shop_ID == this.Main_Shop.Shop_ID || csp_ids.Contains(pdt.Shop_ID)) && pdt.Parent_ID==0 select pdt).ToList<Product>();
            string[] sale_ids=(from sale in trades select sale.Sale_ID).ToArray<string>();
            List<Leave_Stock> cacheLeaveStocks=(from ls in db.Leave_Stock where sale_ids.Contains(ls.Sale_ID) select ls).ToList<Leave_Stock>();
            Store_House house=(from store in db.Store_House where store.Default==true select store).FirstOrDefault<Store_House>();
            List<Store_House> houses = (from store in db.Store_House select store).ToList<Store_House>();
            List<Stock_Pile> stockPiles=(from sp in db.Stock_Pile where sp.Shop_ID==this.Shop.Shop_ID || sp.Shop_ID==this.Main_Shop.Shop_ID || csp_ids.Contains(sp.Shop_ID) select sp).ToList<Stock_Pile>();
            List<Sale_Detail> tradeDetails=(from tradeDetail in db.Sale_Detail where sale_ids.Contains(tradeDetail.Mall_Trade_ID) select tradeDetail).ToList<Sale_Detail>();
            try
            {
                foreach (BSale trade in trades)
                {
                    bool isNew = false;
                    Leave_Stock dbStock = null;

                    dbStock=(from cstock in cacheLeaveStocks where cstock.Sale_ID==trade.Sale_ID select cstock).FirstOrDefault<Leave_Stock>();
                    if (dbStock == null)
                    {
                        isNew = true;
                        dbStock = new Leave_Stock();
                    }

                    dbStock.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbStock.Leave_Date = trade.Synced;
                    dbStock.Leave_Stock_ID = 0;
                    dbStock.Sale_ID = trade.Sale_ID;
                    dbStock.Shop_ID = this.Shop.Shop_ID;
                    dbStock.User_ID = this.CurrentUser.ID;
                    if (isNew)
                    {
                        db.Leave_Stock.Add(dbStock);
                        db.SaveChanges();
                    }

                    if (dbStock.Leave_Stock_ID <= 0)
                    {
                        throw new KMJXCException("出库单创建失败");
                    }

                    if (trade.Orders != null)
                    {
                        foreach (BOrder order in trade.Orders)
                        {
                            Sale_Detail order_detail = (from orderDetail in tradeDetails where orderDetail.Mall_Trade_ID == trade.Sale_ID && orderDetail.Mall_Order_ID == order.Order_ID select orderDetail).FirstOrDefault<Sale_Detail>();
                            if (order.Status1 != 0)
                            {
                                continue;
                            }

                            Leave_Stock_Detail dbDetail = new Leave_Stock_Detail();
                            dbDetail.Leave_Stock_ID = dbStock.Leave_Stock_ID;
                            dbDetail.Price = order.Price;
                            dbDetail.Quantity = order.Quantity;
                            dbDetail.Amount = order.Amount;
                            Stock_Pile stockPile = null;
                            if (house != null)
                            {
                                //order_detail.SyncResultMessage = "默认仓库：" + house.Title;
                                //create leave stock from default store house
                                stockPile = (from sp in stockPiles where sp.Product_ID == order.Product_ID && sp.StockHouse_ID == house.StoreHouse_ID && sp.Quantity >= order.Quantity select sp).FirstOrDefault<Stock_Pile>();                                                               
                            }

                            if (stockPile == null)
                            {
                                if (!string.IsNullOrEmpty(order_detail.SyncResultMessage))
                                {
                                    //order_detail.SyncResultMessage = "默认仓库：" + house.Title + "没有库存或者库存数量不够<br/>";
                                }
                                //get store house when it has the specific product
                                var tmpstockPile = from sp in stockPiles where sp.Product_ID == order.Product_ID && sp.Quantity >= order.Quantity select sp;
                                if (tmpstockPile.Count() > 0)
                                {
                                    stockPile = tmpstockPile.ToList<Stock_Pile>()[0];
                                    Store_House tmpHouse = (from h in houses where h.StoreHouse_ID == stockPile.StockHouse_ID select h).FirstOrDefault<Store_House>();

                                    order_detail.SyncResultMessage = "出库仓库："+tmpHouse.Title;
                                }
                                else
                                {
                                    //cannot leave stock, no stock pile
                                    order_detail.Status1 = 3;
                                    order_detail.SyncResultMessage = "没有足够的库存，不能出库";
                                }
                            }                            

                            //no stock cannot leave stock
                            if (stockPile != null)
                            {
                                dbDetail.Parent_Product_ID = order.Parent_Product_ID;
                                dbDetail.Product_ID = order.Product_ID;
                                dbDetail.StoreHouse_ID = stockPile.StockHouse_ID;
                                //Update stock
                                stockPile.Quantity = stockPile.Quantity - order.Quantity;
                                
                                //Update stock field in Product table
                                Product product=(from pdt in products where pdt.Product_ID==dbDetail.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                                if (product != null)
                                {
                                    product.Quantity = product.Quantity - order.Quantity;
                                }
                                dbDetail.Order_ID = order.Order_ID;
                                db.Leave_Stock_Detail.Add(dbDetail);
                            }  
                        }
                    }
                }

                db.SaveChanges();
            }
            catch (KMJXCException kex)
            {
                throw kex;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                db.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool SyncMallTrades(int startTime, int endTime, string status,int syncType)
        {
            bool result = false;
            IOTradeManager tradeManager = new TaobaoTradeManager(this.AccessToken, this.Shop.Mall_Type_ID);
            List<BSale> allSales = new List<BSale>();          
                      
            Sale_SyncTime syncTime = null; 
            KuanMaiEntities db = new KuanMaiEntities();
            syncTime = (from sync in db.Sale_SyncTime where sync.ShopID == this.Shop.Shop_ID && sync.SyncType == syncType orderby sync.LastSyncTime descending select sync).FirstOrDefault<Sale_SyncTime>();
            DateTime tNow = DateTime.Now;
            int timeNow = DateTimeUtil.ConvertDateTimeToInt(tNow);            
            int lastSyncModifiedTime = 0;
            lastSyncModifiedTime = DateTimeUtil.ConvertDateTimeToInt(tNow.AddDays(-1));
            if (syncTime != null)
            {
                lastSyncModifiedTime = syncTime.LastTradeModifiedEndTime;               
            }

            syncTime = new Sale_SyncTime();
            syncTime.ShopID = this.Shop.Shop_ID;            
            syncTime.SyncUser = this.CurrentUser.ID;
            syncTime.LastSyncTime = timeNow;
            
            
            
            syncTime.SyncType = syncType;
            db.Sale_SyncTime.Add(syncTime);

            
            //0 is normal sync trade by created time, 1 is increment sync trade by modified time
            if (syncType == 0)
            {
                syncTime.LastTradeStartEndTime = endTime + 1;
                syncTime.LastTradeModifiedEndTime = 0;
                DateTime? sDate = null;
                DateTime? eDate = null;

                if (startTime > 0)
                {
                    sDate = DateTimeUtil.ConvertToDateTime(startTime);
                }

                if (endTime > 0)
                {
                    eDate = DateTimeUtil.ConvertToDateTime(endTime); 
                }

                allSales = tradeManager.SyncMallTrades(sDate, eDate, status);
                this.HandleMallTrades(allSales);              
            }
            else if (syncType == 1)
            {
                syncTime.LastTradeStartEndTime = 0;
                syncTime.LastTradeModifiedEndTime = timeNow + 1;
                allSales = tradeManager.IncrementSyncMallTrades(lastSyncModifiedTime,timeNow, status);
                this.HandleMallTrades(allSales);
            }

            db.SaveChanges();
            
            return result;
        }

        private void HandleMallTrades(List<BSale> allSales)
        {
            if (allSales == null)
            {
                return;
            }

            List<BSale> newSales = new List<BSale>();
            List<BSale> backSales = new List<BSale>();

            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                int timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                var customers = from customer in db.Customer
                                where customer.Mall_Type_ID == this.Shop.Mall_Type_ID
                                select customer;
                List<Customer_Shop> shop_customers=(from shop_cus in db.Customer_Shop where shop_cus.Shop_ID==this.Shop.Shop_ID select shop_cus).ToList<Customer_Shop>();
                string[] sale_ids=(from sale in allSales select sale.Sale_ID).ToArray<string>();
                List<Sale> dbSales = (from sale in db.Sale where sale_ids.Contains(sale.Mall_Trade_ID) select sale).ToList<Sale>();
               
                //var dbSales = dbSaleObjs;
                List<Common_District> areas = (from area in db.Common_District select area).ToList<Common_District>(); ;
               
                foreach (BSale sale in allSales)
                {
                    Sale dbSale = new Sale();
                    dbSale.Amount = sale.Amount;
                    dbSale.Buyer_ID = 0;
                    dbSale.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbSale.Express_Cop = "";
                    dbSale.Mall_Trade_ID = sale.Sale_ID;
                    dbSale.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbSale.Post_Fee = sale.Post_Fee;
                    dbSale.Province_ID = 0;
                    dbSale.City_ID = 0;
                    dbSale.Sync_User = this.CurrentUser.ID;
                    dbSale.HasRefound = sale.HasRefound;
                    if (sale.Buyer != null)
                    {
                        if (sale.Buyer.Province != null)
                        {
                            sale.Buyer.Province = (from p in areas where p.name == sale.Buyer.Province.name select p).FirstOrDefault<Common_District>();
                            if (sale.Buyer.Province != null)
                            {
                                dbSale.Province_ID = sale.Buyer.Province.id;
                            }
                        }

                        if (sale.Buyer.City != null)
                        {
                            sale.Buyer.City = (from p in areas where p.name == sale.Buyer.City.name select p).FirstOrDefault<Common_District>();
                            if (sale.Buyer.City != null)
                            {
                                dbSale.City_ID = sale.Buyer.City.id;
                            }
                        }

                        if (!string.IsNullOrEmpty(sale.Buyer.Mall_ID))
                        {
                            Customer cus = (from custo in customers where custo.Mall_ID == sale.Buyer.Mall_ID select custo).FirstOrDefault<Customer>();
                            if (cus == null)
                            {
                                cus = new Customer();
                                cus.Address = sale.Buyer.Address;
                                cus.Name = sale.Buyer.Name;
                                cus.City_ID = dbSale.City_ID;
                                cus.Province_ID = dbSale.Province_ID;
                                cus.Mall_ID = sale.Buyer.Mall_ID;
                                cus.Mall_Type_ID = this.Shop.Mall_Type_ID;
                                cus.Phone = sale.Buyer.Phone;
                                db.Customer.Add(cus);
                                db.SaveChanges();

                                //refresh customer cache
                                //customers.Add(cus);

                                //add to shop customers
                                Customer_Shop cs = new Customer_Shop() { Shop_ID = this.Shop.Shop_ID, Customer_ID = cus.Customer_ID };
                                db.Customer_Shop.Add(cs);
                            }
                            else
                            {
                                //update customer info
                                cus.Address = sale.Buyer.Address;
                                cus.Name = sale.Buyer.Name;
                                cus.City_ID = dbSale.City_ID;
                                cus.Province_ID = dbSale.Province_ID;
                                cus.Mall_ID = sale.Buyer.Mall_ID;
                                cus.Mall_Type_ID = this.Shop.Mall_Type_ID;
                                cus.Phone = sale.Buyer.Phone;

                                Customer_Shop cuss=(from cusshop in shop_customers where cusshop.Customer_ID==cus.Customer_ID && cusshop.Shop_ID==this.Shop.Shop_ID select cusshop).FirstOrDefault<Customer_Shop>();
                                if (cuss == null)
                                {
                                    Customer_Shop cs = new Customer_Shop() { Shop_ID = this.Shop.Shop_ID, Customer_ID = cus.Customer_ID };
                                    db.Customer_Shop.Add(cs);
                                }
                            }

                            dbSale.Buyer_ID = cus.Customer_ID;
                        }
                    }
                    dbSale.Sale_Time = sale.SaleDateTime;
                    dbSale.Shop_ID = this.Shop.Shop_ID;
                    dbSale.Status = sale.Status;
                    dbSale.StockStatus = 0;
                    dbSale.Synced = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    Sale existed = null;
                    if (dbSales != null)
                    {
                        existed = (from s in dbSales where s.Mall_Trade_ID == dbSale.Mall_Trade_ID select s).FirstOrDefault<Sale>();
                    }
                    else
                    {
                        existed = (from s in db.Sale where s.Mall_Trade_ID == dbSale.Mall_Trade_ID select s).FirstOrDefault<Sale>();
                    }

                    if (sale.HasRefound)
                    {
                        backSales.Add(sale);
                    }

                    if (existed == null)
                    {
                        newSales.Add(sale);
                        db.Sale.Add(dbSale);
                        if (sale.Orders != null)
                        {
                            foreach (BOrder order in sale.Orders)
                            {
                                Sale_Detail sd = new Sale_Detail();
                                sd.Amount = order.Amount;
                                sd.Discount = order.Discount;
                                sd.Mall_Order_ID = order.Order_ID;
                                sd.Mall_Trade_ID = sale.Sale_ID;
                                sd.Parent_Product_ID = order.Parent_Product_ID;
                                sd.Product_ID = order.Product_ID;
                                sd.Price = order.Price;
                                sd.Quantity = order.Quantity;
                                sd.Status = order.Status;
                                if (string.IsNullOrEmpty(sd.Status))
                                {
                                    sd.Status = "0";
                                }
                                sd.ImageUrl = order.ImageUrl;
                                sd.Status1 = order.Status1;
                                sd.StockStatus = 0;
                                sd.Mall_PID = order.Mall_PID;
                                sd.Supplier_ID = 0;
                                db.Sale_Detail.Add(sd);
                            }
                        }
                    }
                    else
                    {                       
                        List<Sale_Detail> details = (from detail in db.Sale_Detail
                                                     where detail.Mall_Trade_ID == dbSale.Mall_Trade_ID
                                                     select detail).ToList<Sale_Detail>();
                        this.UpdateProperties(existed, dbSale);

                        foreach (BOrder order in sale.Orders)
                        {
                            Sale_Detail sd = (from ed in details where ed.Mall_Order_ID == order.Order_ID select ed).FirstOrDefault<Sale_Detail>();
                            bool isNew = false;
                            if (sd == null)
                            {
                                isNew = true;
                                sd = new Sale_Detail();
                            }

                            sd.Amount = order.Amount;
                            sd.Discount = order.Discount;
                            sd.Mall_Order_ID = order.Order_ID;
                            sd.Mall_Trade_ID = sale.Sale_ID;
                            sd.Parent_Product_ID = order.Parent_Product_ID;
                            sd.Product_ID = order.Product_ID;
                            sd.Price = order.Price;
                            sd.Quantity = order.Quantity;
                            sd.Status = order.Status;
                            if (string.IsNullOrEmpty(sd.Status))
                            {
                                sd.Status = "0";
                            }
                            sd.ImageUrl = order.ImageUrl;
                            sd.Status1 = order.Status1;
                            sd.StockStatus = 0;
                            sd.Mall_PID = order.Mall_PID;
                            sd.Supplier_ID = 0;

                            if (isNew)
                            {
                                db.Sale_Detail.Add(sd);
                            }
                        }
                    }
                }

                db.SaveChanges();
                this.CreateLeaveStocks(newSales);
                this.HandleBackTrades(backSales);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                db.Dispose();
            }
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
        public List<BSale> SearchSales(int[] pdtids,string productName, int[] customers, string customer_nick, int sSaleTime, int eSaleTime, int page, int pageSize, out int totalRecords)
        {
            List<BSale> sales = null;
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                var trades = from sale in db.Sale
                             where sale.Shop_ID == this.Shop.Shop_ID || sale.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(sale.Shop_ID)
                             select sale;

                if (pdtids != null && pdtids.Length > 0)
                {
                    string[] sale_ids = (from order in db.Sale_Detail
                                         where pdtids.Contains(order.Parent_Product_ID)
                                         select order.Mall_Trade_ID).Distinct<string>().ToArray<string>();
                    if (sale_ids != null && sale_ids.Length > 0)
                    {
                        trades = trades.Where(t => sale_ids.Contains(t.Mall_Trade_ID));
                    }
                }

                if (!string.IsNullOrEmpty(productName))
                {
                    string[] sale_ids = (from order in db.Sale_Detail
                                         join product in db.Product on order.Parent_Product_ID equals product.Product_ID
                                         where product.Parent_ID==0 && product.Name.Contains(productName)
                                         select order.Mall_Trade_ID).Distinct<string>().ToArray<string>();
                    if (sale_ids != null && sale_ids.Length > 0)
                    {
                        trades = trades.Where(t => sale_ids.Contains(t.Mall_Trade_ID));
                    }
                }

                if (customers != null && customers.Length > 0)
                {
                    trades = trades.Where(t => customers.Contains((int)t.Buyer_ID));
                }

                if(!string.IsNullOrEmpty(customer_nick))
                {
                    int buyer_id=(from customer in db.Customer where customer.Mall_ID==customer_nick select customer.Customer_ID).FirstOrDefault<int>();
                    trades = trades.Where(t => t.Buyer_ID==buyer_id);
                }

                if (sSaleTime > 0)
                {
                    trades = trades.Where(t => t.Sale_Time >= sSaleTime);
                }

                if (eSaleTime > 0)
                {
                    trades = trades.Where(t => t.Sale_Time <= eSaleTime);
                }

                var sObjs = from t in trades
                            join customer in db.Customer on t.Buyer_ID equals customer.Customer_ID
                            join shop in db.Shop on t.Shop_ID equals shop.Shop_ID
                            join user in db.User on t.Sync_User equals user.User_ID //into l_user
                            //from ll_user in l_user.DefaultIfEmpty()
                            //join province in db.Common_District on customer.Province_ID equals province.id
                            join city in db.Common_District on customer.City_ID equals city.id into l_city
                            from ll_city in l_city.DefaultIfEmpty()
                            select new BSale
                            {
                           
                                Created = (int)t.Created,
                                Synced = (int)t.Synced,
                                Modified = (int)t.Modified,
                                SaleDateTime = (int)t.Sale_Time,
                                Amount = t.Amount,
                                Buyer = new BCustomer
                                {
                                    ID = customer.Customer_ID,
                                    Name = customer.Name,
                                    Mall_ID = customer.Mall_ID,
                                    Mall_Name = customer.Mall_Name,
                                    Phone = customer.Phone,
                                    //Province = province,
                                    City = ll_city
                                },
                                Sale_ID = t.Mall_Trade_ID,
                                Post_Fee = (double)t.Post_Fee,
                                Status = t.Status,
                                Shop = new BShop
                                {
                                    ID = shop.Shop_ID,
                                    Title = shop.Name
                                },
                                SyncUser = new BUser
                                {
                                    ID = user.User_ID,
                                    Name = user.Name,
                                    Mall_ID = user.Mall_ID,
                                    Mall_Name = user.Mall_Name
                                },
                                HasRefound = (bool)t.HasRefound
                            };//).OrderByDescending(s => s.SaleDateTime).Skip((page - 1) * pageSize).Take(pageSize).ToList<BSale>();
                totalRecords = sObjs.Count();
                sales = sObjs.OrderByDescending(s => s.SaleDateTime).Skip((page - 1) * pageSize).Take(pageSize).ToList<BSale>();

                string[] bsale_ids = (from sale in sales select sale.Sale_ID).ToArray<string>();
                List<Sale_Detail> sale_details = (from sdetail in db.Sale_Detail where bsale_ids.Contains(sdetail.Mall_Trade_ID) select sdetail).ToList<Sale_Detail>();
                int[] product_ids = (from sd in sale_details select sd.Parent_Product_ID).ToArray<int>();
                int[] cproduct_ids = (from sd in sale_details select sd.Product_ID).Distinct<int>().ToArray<int>();
                List<Product> dbProducts = (from product in db.Product where product_ids.Contains(product.Product_ID) select product).ToList<Product>();
                List<BProductProperty> childs = (from prop in db.Product_Specifications
                             join ps in db.Product_Spec on prop.Product_Spec_ID equals ps.Product_Spec_ID
                             join psv in db.Product_Spec_Value on prop.Product_Spec_Value_ID equals psv.Product_Spec_Value_ID
                             where cproduct_ids.Contains(prop.Product_ID)
                             select new BProductProperty
                             {
                                 ProductID = prop.Product_ID,
                                 PID = prop.Product_Spec_ID,
                                 PName = ps.Name,
                                 PVID = prop.Product_Spec_Value_ID,
                                 PValue = psv.Name
                             }).ToList<BProductProperty>();
                foreach (BSale sale in sales)
                {
                    var os = from order in sale_details                             
                             where order.Mall_Trade_ID==sale.Sale_ID
                             select new BOrder
                             {
                                 Amount = (double)order.Amount,
                                 Order_ID = order.Mall_Order_ID,
                                 Quantity = order.Quantity,
                                 Price = order.Price,
                                 Parent_Product_ID = order.Parent_Product_ID,
                                 Product_ID = order.Product_ID,
                                 ImageUrl=order.ImageUrl,
                                 //Product = new BProduct
                                 //{
                                 //    ID = ll_product.Product_ID,
                                 //    Title = ll_product.Name
                                 //}
                                 Message=order.SyncResultMessage
                             };
                    sale.Orders = os.ToList<BOrder>();

                    foreach (BOrder order in sale.Orders)
                    {

                        order.Product = (from product in dbProducts
                                         where product.Product_ID == order.Parent_Product_ID
                                         select new BProduct
                                         {
                                             ID = product.Product_ID,                                             
                                             Title = product.Name
                                         }).FirstOrDefault<BProduct>();

                        if (order.Product != null)
                        {
                            order.Product.Properties = (from p in childs
                                                        where p.ProductID == order.Product_ID
                                                        select p).ToList<BProductProperty>();


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
        public List<BBackSaleDetail> SearchBackSaleDetails(string[] sale_ids, int[] user_ids, int stime, int etime, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            List<BBackSaleDetail> backSaleDetails = new List<BBackSaleDetail>();
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

                int[] backSaleIds = (from bs in dbBackSales select bs.Back_Sale_ID).ToArray<int>();

                var tmp = from bsd in db.Back_Sale_Detail
                          join bs in db.Back_Sale on bsd.Back_Sale_ID equals bs.Back_Sale_ID into lbs
                          from l_bs in lbs.DefaultIfEmpty()
                          join sale in db.Sale on l_bs.Sale_ID equals sale.Mall_Trade_ID into lsale
                          from l_sale in lsale.DefaultIfEmpty()
                          join customer in db.Customer on l_sale.Buyer_ID equals customer.Customer_ID into lcustomer
                          from l_customer in lcustomer.DefaultIfEmpty()
                          join product in db.Product on bsd.Parent_Product_ID equals product.Product_ID into lproduct
                          from l_product in lproduct.DefaultIfEmpty()
                          join shop in db.Shop on l_bs.Shop_ID equals shop.Shop_ID into lshop
                          from l_shop in lshop.DefaultIfEmpty()
                          join user in db.User on l_bs.User_ID equals user.User_ID into luser
                          from l_user in luser.DefaultIfEmpty()
                          where backSaleIds.Contains(bsd.Back_Sale_ID)
                          select new BBackSaleDetail
                          {
                              Order_ID=bsd.Order_ID,
                              Amount = bsd.Refound,
                              Created = (int)bsd.Created,
                              Description = bsd.Description,
                              BackSaleID = bsd.Back_Sale_ID,
                              ParentProductID = bsd.Parent_Product_ID,
                              Price = bsd.Price,
                              ProductID = bsd.Product_ID,
                              Quantity = bsd.Quantity,
                              Status = bsd.Status,
                              BackSale = new BBackSale
                              {
                                  Amount = l_bs.Amount,
                                  BackTime = l_bs.Back_Date,
                                  Created = l_bs.Created,
                                  Description = l_bs.Description,
                                  ID = l_bs.Back_Sale_ID,
                                  Sale = new BSale
                                  {
                                      Sale_ID = l_bs.Sale_ID,
                                      Buyer = new BCustomer
                                      {
                                          ID = l_customer.Customer_ID,
                                          Mall_ID = l_customer.Mall_ID,
                                          Mall_Name = l_customer.Mall_Name
                                      },
                                      Amount = l_sale.Amount
                                  },
                                  Shop = new BShop
                                  {
                                      Title = l_shop.Name,
                                      ID = l_shop.Shop_ID
                                  },
                                  Created_By = new BUser
                                  {
                                      ID = l_user.User_ID,
                                      Mall_ID = l_user.Mall_ID,
                                      Mall_Name = l_user.Mall_Name
                                  }
                              },
                              Product=new BProduct
                              {
                                 Title=l_product.Name
                              }
                          };
                tmp = tmp.OrderBy(b=>b.Status).OrderBy(b=>b.BackSaleID);

                totalRecords = tmp.Count();
                if (totalRecords > 0)
                {
                    backSaleDetails = tmp.Skip((pageIndex-1)*pageSize).Take(pageSize).ToList<BBackSaleDetail>();
                }
            }

            return backSaleDetails;
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
        public List<BBackSale> SearchBackSales(string[] sale_ids, int[] user_ids, int stime, int etime, int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
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

                var tmp = from sale in dbBackSales
                          orderby sale.Shop_ID ascending
                          join user in db.User on sale.User_ID equals user.User_ID
                          join mtype in db.Mall_Type on user.Mall_Type equals mtype.Mall_Type_ID
                          join order in db.Sale on sale.Sale_ID equals order.Mall_Trade_ID
                          join customer in db.Customer on order.Buyer_ID equals customer.Customer_ID
                          join shop in db.Shop on sale.Shop_ID equals shop.Shop_ID

                          select new BBackSale
                          {
                              BackTime = (int)sale.Back_Date,
                              Created = sale.Created,
                              Created_By = new BUser
                              {
                                  ID = user.User_ID,
                                  Mall_ID = user.Mall_ID,
                                  Mall_Name = user.Mall_Name,
                                  Created = (int)user.Created,
                                  Type = mtype
                              },
                            
                              Description = sale.Description,
                              ID = sale.Back_Sale_ID,
                              Amount=sale.Amount,
                              Sale = new BSale
                              {
                                  Amount = order.Amount,
                                  Buyer = new BCustomer
                                  {
                                      ID = customer.Customer_ID,
                                      Mall_ID = customer.Mall_ID,
                                      Mall_Name = customer.Mall_Name,
                                      Phone = customer.Phone,
                                      //Addres=customer.Address,
                                      Type = mtype,
                                     Address=customer.Address
                                  },
                                  Created = (int)order.Created,
                                  Sale_ID = order.Mall_Trade_ID,
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

                          };

                totalRecords = tmp.Count();
                backSales = tmp.OrderBy(a => a.ID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BBackSale>();

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
            if (backSale.Sale == null || string.IsNullOrEmpty(backSale.Sale.Sale_ID))
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
                dbbackSale.Sale_ID = backSale.Sale.Sale_ID;
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
                    //stockManager.CreateBackStock(dbbackSale.Back_Sale_ID, backSale.UpdateStock);
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backSaleID"></param>
        /// <param name="product_id"></param>
        /// <param name="status"></param>
        public void HandleBackSaleDetail(int backSaleID,string[] order_ids, int status)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (this.CurrentUserPermission.HANDLE_BACK_SALE == 0)
                {
                    throw new KMJXCException("没有处理退货单的权限");
                }

                var bsDetail = from bsd in db.Back_Sale_Detail
                               where bsd.Back_Sale_ID == backSaleID
                               select bsd;

                if (order_ids!=null)
                {
                    bsDetail = bsDetail.Where(b => order_ids.Contains(b.Order_ID));
                }

                Back_Sale_Detail detail = bsDetail.FirstOrDefault<Back_Sale_Detail>();
                if (detail == null)
                {
                    throw new KMJXCException("没有找到退货单信息");
                }

                if (status == 1)
                {
                    //back stock and update stock pile
                    stockManager.CreateBackStock(backSaleID, order_ids, status);
                }
                else if (status == 2)
                {
                    //no need to back stock, update wastage

                }
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
