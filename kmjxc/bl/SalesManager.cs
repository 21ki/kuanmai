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
        /// The back sales won't be created while the sales don't has corresponding leave stocks
        /// Example:Customers refounded before expressed
        /// </summary>
        /// <param name="trades"></param>
        private void HandleBackTrades(List<BSale> trades, Shop shop)
        {
            if (trades == null || trades.Count == 0 || shop==null)
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
                    //no leave stock no need to create back sale
                    if (ls == null) 
                    {
                        continue;
                    }

                    Back_Sale bSale=(from b_Sale in back_Sales where b_Sale.Sale_ID==trade.Sale_ID select b_Sale).FirstOrDefault<Back_Sale>();
                    bool isNewBackSale = false;
                    double totalRefound = 0;
                    if (bSale == null)
                    {
                        isNewBackSale = true;                       
                        bSale = new Back_Sale();
                    }

                    bSale.Back_Date = trade.Synced;                    
                    bSale.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    bSale.Description = "";
                    bSale.Sale_ID = trade.Sale_ID;
                    bSale.Shop_ID = shop.Shop_ID;
                    bSale.Status = 0;
                    bSale.User_ID = this.CurrentUser.ID;

                    if (isNewBackSale)
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
                            Leave_Stock_Detail dbLeaveStockDetail=(from dblsd in leave_stock_Details where dblsd.Order_ID==order.Order_ID select dblsd).FirstOrDefault<Leave_Stock_Detail>();
                            //no need to create back sale detail while no leave stock detail
                            if (dbSaleDetail == null && dbLeaveStockDetail!=null)
                            {                                
                                dbSaleDetail = new Back_Sale_Detail();
                                dbSaleDetail.Order_ID = order.Order_ID;
                                dbSaleDetail.Back_Sale_ID = bSale.Back_Sale_ID;
                                dbSaleDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                                dbSaleDetail.Description = "同步商城订单时处理有退货的订单(有出库单详细信息)";
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
        /// The leave stocks won't be created while orders products are not mapped with local products
        /// </summary>
        /// <param name="trades"></param>
        private void CreateLeaveStocks(List<BSale> trades,Shop shop)
        {
            if (trades == null)
            {
                return;
            }
            if (shop == null)
            {
                return;
            }
            KuanMaiEntities db = new KuanMaiEntities();

            int[] csp_ids = (from child in this.DBChildShops select child.Shop_ID).ToArray<int>();
            if (csp_ids == null)
            {
                csp_ids = new int[1];
            }

            List<Product> allproducts = (from pdt in db.Product where (pdt.Shop_ID == shop.Shop_ID || pdt.Shop_ID == this.Main_Shop.Shop_ID || csp_ids.Contains(pdt.Shop_ID)) select pdt).ToList<Product>();
            string[] sale_ids=(from sale in trades select sale.Sale_ID).ToArray<string>();
            List<Leave_Stock> cacheLeaveStocks=(from ls in db.Leave_Stock where sale_ids.Contains(ls.Sale_ID) select ls).ToList<Leave_Stock>();
            Store_House house=(from store in db.Store_House where store.Default==true select store).FirstOrDefault<Store_House>();
            List<Store_House> houses = (from store in db.Store_House select store).ToList<Store_House>();
            List<Stock_Pile> stockPiles = (from sp in db.Stock_Pile where sp.Shop_ID == shop.Shop_ID || sp.Shop_ID == this.Main_Shop.Shop_ID || csp_ids.Contains(sp.Shop_ID) select sp).ToList<Stock_Pile>();
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
                    dbStock.Shop_ID = shop.Shop_ID;
                    dbStock.User_ID = this.CurrentUser.ID;  

                    if (trade.Orders != null)
                    {
                        List<Leave_Stock_Detail> dbLDetails = new List<Leave_Stock_Detail>();
                        #region handle sale orders
                        foreach (BOrder order in trade.Orders)
                        {
                            Leave_Stock_Detail dbDetail = new Leave_Stock_Detail();
                            int stockPileProductId = 0;

                            Sale_Detail order_detail = (from orderDetail in tradeDetails where orderDetail.Mall_Trade_ID == trade.Sale_ID && orderDetail.Mall_Order_ID == order.Order_ID select orderDetail).FirstOrDefault<Sale_Detail>();
                            if (order_detail == null)
                            {
                                continue;
                            }

                            order_detail.SyncResultMessage = "";
                            if (order.Status1 != 0)
                            {
                                order_detail.Status1 = (int)SaleDetailStatus.REFOUND_BEFORE_SEND;
                                order_detail.SyncResultMessage = "宝贝在发货前退货，不需要出库";
                                db.SaveChanges();
                                continue;
                            }
                            
                            Product parentPdt = (from pdt in allproducts where pdt.Product_ID == order.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                            Product childPdt = (from pdt in allproducts where pdt.Product_ID == order.Product_ID select pdt).FirstOrDefault<Product>();
                            order.Product_ID = 0;
                            order.Parent_Product_ID = 0;
                            if (parentPdt != null)
                            {                                
                                order.Parent_Product_ID = parentPdt.Product_ID;                              
                                if (childPdt != null)
                                {
                                    order.Product_ID = childPdt.Product_ID;
                                }
                            }
                            else
                            {
                                if (childPdt != null)
                                {
                                    order.Parent_Product_ID = childPdt.Parent_ID;
                                }
                            }

                            //no need to create leave stock while the mall product is not mapped with local product
                            if (order.Product_ID == 0 && order.Parent_Product_ID == 0)
                            {
                                order_detail.Status1 = (int)SaleDetailStatus.NOT_CONNECTED;
                                order_detail.SyncResultMessage = "宝贝未关联，不能出库";
                                db.SaveChanges();
                                continue;
                            }

                            stockPileProductId = order.Product_ID;
                            if (stockPileProductId == 0 && string.IsNullOrEmpty(order.Mall_SkuID) && order.Parent_Product_ID>0)
                            {
                                stockPileProductId = order.Parent_Product_ID;
                            }
                            
                            dbDetail.Leave_Stock_ID = dbStock.Leave_Stock_ID;
                            dbDetail.Price = order.Price;
                            dbDetail.Quantity = order.Quantity;
                            dbDetail.Amount = order.Amount;
                            Stock_Pile stockPile = null;
                            if (house != null)
                            {
                                //order_detail.SyncResultMessage = "默认仓库：" + house.Title;
                                //create leave stock from default store house
                                stockPile = (from sp in stockPiles where sp.Product_ID == stockPileProductId && sp.StockHouse_ID == house.StoreHouse_ID && sp.Quantity >= order.Quantity select sp).FirstOrDefault<Stock_Pile>();                                                               
                            }

                            if (stockPile == null)
                            {
                                if (!string.IsNullOrEmpty(order_detail.SyncResultMessage))
                                {
                                    //order_detail.SyncResultMessage = "默认仓库：" + house.Title + "没有库存或者库存数量不够<br/>";
                                }
                                //get store house when it has the specific product
                                var tmpstockPile = from sp in stockPiles where sp.Product_ID == stockPileProductId && sp.Quantity >= order.Quantity select sp;
                                if (tmpstockPile.Count() > 0)
                                {
                                    stockPile = tmpstockPile.ToList<Stock_Pile>()[0];
                                    Store_House tmpHouse = (from h in houses where h.StoreHouse_ID == stockPile.StockHouse_ID select h).FirstOrDefault<Store_House>();
                                    order_detail.Status1 = (int)SaleDetailStatus.LEAVED_STOCK;
                                    order_detail.SyncResultMessage = "出库仓库："+tmpHouse.Title;
                                }
                                else
                                {
                                    //cannot leave stock, no stock pile
                                    order_detail.Status1 = (int)SaleDetailStatus.NO_ENOUGH_STOCK;
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
                                Product product=(from pdt in allproducts where pdt.Product_ID==dbDetail.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                                if (product != null)
                                {
                                    product.Quantity = product.Quantity - order.Quantity;
                                }
                                dbDetail.Order_ID = order.Order_ID;
                                dbLDetails.Add(dbDetail);
                                //db.Leave_Stock_Detail.Add(dbDetail);
                            }
                        }
                        #endregion

                        if (isNew && dbLDetails.Count>0)
                        {
                            db.Leave_Stock.Add(dbStock);
                            db.SaveChanges();
                            foreach (Leave_Stock_Detail d in dbLDetails)
                            {
                                if (d.Leave_Stock_ID == 0)
                                {
                                    d.Leave_Stock_ID = dbStock.Leave_Stock_ID;
                                }

                                db.Leave_Stock_Detail.Add(d);
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
                throw ex;
            }
            finally
            {
                db.Dispose();
            }
        }

        public void SyncMallTrades(int startTime, int endTime, string status, int syncType, int shop_id = 0)
        {
            if (shop_id == 0)
            {
                this.SyncMallTrades(startTime, endTime, status, syncType, this.Shop);
                foreach (Shop s in this.DBChildShops)
                {
                    this.SyncMallTrades(startTime, endTime, status, syncType, s);
                }
            }
            else
            {
                Shop tmpShop = null;
                int[] child_shop_ids=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (this.Shop.Shop_ID != shop_id && !child_shop_ids.Contains(shop_id))
                {
                    throw new KMJXCException("不能同步他人店铺的订单，不要再次尝试此操作");
                }

                if (this.Main_Shop.Shop_ID == shop_id && this.CurrentUser.Shop.ID != shop_id)
                {
                    throw new KMJXCException("没有权限同步主店铺订单");
                }
                else if (this.Main_Shop.Shop_ID == shop_id && this.CurrentUser.Shop.ID == shop_id)
                {
                    tmpShop = this.Main_Shop;
                }
                else if(this.Main_Shop.Shop_ID!=shop_id)
                {
                    if (shop_id == this.Shop.Shop_ID)
                    {
                        tmpShop = this.Shop;
                    }
                    else
                    {
                        tmpShop = (from s in this.DBChildShops where s.Shop_ID == shop_id select s).FirstOrDefault<Shop>();
                    }
                }

                this.SyncMallTrades(startTime, endTime, status, syncType, tmpShop);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool SyncMallTrades(int startTime, int endTime, string status,int syncType,Shop shop)
        {
            bool result = false;
            try
            {
                KuanMaiEntities db = new KuanMaiEntities();
                IOTradeManager tradeManager = null;
                Access_Token token = this.AccessToken;
                Shop desshop = this.Shop;
                if (shop != null)
                {
                    desshop = shop;
                    token = (from t in db.Access_Token
                             from u in db.User
                             where t.User_ID == u.User_ID && u.User_ID == shop.User_ID
                             select t).FirstOrDefault<Access_Token>();

                    if (token == null)
                    {
                        throw new KMJXCException("店铺 "+shop.Name+" 的授权已经过期，请使用此店铺掌柜账号登录一次系统");
                    }

                    if (IsTokenExpired(token))
                    {
                        throw new KMJXCException("店铺 " + shop.Name + " 的授权已经过期，请使用此店铺掌柜账号登录一次系统");
                    }
                }

                tradeManager = new TaobaoTradeManager(token, desshop.Mall_Type_ID);
                List<BSale> allSales = new List<BSale>();

                Sale_SyncTime syncTime = null;
                
                syncTime = (from sync in db.Sale_SyncTime where sync.ShopID == desshop.Shop_ID && sync.SyncType == syncType orderby sync.LastSyncTime descending select sync).FirstOrDefault<Sale_SyncTime>();
                DateTime tNow = DateTime.Now;
                long timeNow = DateTimeUtil.ConvertDateTimeToInt(tNow);
                long lastSyncModifiedTime = 0;
                lastSyncModifiedTime = DateTimeUtil.ConvertDateTimeToInt(tNow.AddDays(-1));
                if (syncTime != null)
                {
                    lastSyncModifiedTime = syncTime.LastTradeModifiedEndTime;
                }

                syncTime = new Sale_SyncTime();
                syncTime.ShopID = desshop.Shop_ID;
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
                }
                else if (syncType == 1)
                {
                    syncTime.LastTradeStartEndTime = 0;
                    syncTime.LastTradeModifiedEndTime = timeNow + 1;
                    allSales = tradeManager.IncrementSyncMallTrades(lastSyncModifiedTime, timeNow, status);
                }

                List<BSale> sales = new List<BSale>();

                foreach (BSale s in allSales)
                {
                    List<BSale> existed = (from ss in sales where ss.Sale_ID == s.Sale_ID select ss).ToList<BSale>();
                    if (existed == null || existed.Count == 0)
                    {
                        sales.Add(s);
                    }
                }

                this.HandleMallTrades(sales, desshop);
                db.SaveChanges();
            }
            catch (KMJXCTaobaoException mex)
            {
                throw mex;
            }
            catch (KMJXCMallException mex)
            {
                throw mex;
            }
            catch (KMJXCException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return result;
        }

        /// <summary>
        /// Handle sales synchronized from mall including creating leave stocks and handle refounded trades which already synchronized before.
        /// </summary>
        /// <param name="allSales"></param>
        private void HandleMallTrades(List<BSale> allSales,Shop shop)
        {
            if (allSales == null)
            {
                return;
            }

            List<BSale> newSales = new List<BSale>();
            List<BSale> backSales = new List<BSale>();
            List<Product> allProducts = null;
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                int[] child_shops=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                allProducts = (from p in db.Product
                               where p.Shop_ID == shop.Shop_ID || child_shops.Contains(p.Shop_ID) || p.Shop_ID == this.Main_Shop.Shop_ID
                               select p).ToList<Product>();

                var tmpExp = from expsp in db.Express_Shop
                             join exp in db.Express on expsp.Express_ID equals exp.Express_ID into lExp
                             from l_exp in lExp
                             where expsp.Shop_ID == shop.Shop_ID && expsp.IsDefault == 1
                             select l_exp;
                Express defaultExp = tmpExp.FirstOrDefault<Express>();
                List<Express_Fee> expFees = new List<Express_Fee>();
                if (defaultExp != null)
                {
                    expFees = (from fee in db.Express_Fee
                               where fee.Express_ID == defaultExp.Express_ID && fee.Shop_ID == this.Shop.Shop_ID
                               select fee).ToList<Express_Fee>();
                }

                long timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                var customers = from customer in db.Customer
                                where customer.Mall_Type_ID == shop.Mall_Type_ID
                                select customer;
                List<Customer_Shop> shop_customers = (from shop_cus in db.Customer_Shop where shop_cus.Shop_ID == shop.Shop_ID select shop_cus).ToList<Customer_Shop>();
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
                    if (defaultExp != null)
                    {
                        dbSale.Express_ID = defaultExp.Express_ID;
                        dbSale.Express_Cop = defaultExp.Name;
                    }
                    List<Express_Fee> tmpFees = new List<Express_Fee>();
                    if (sale.Buyer != null)
                    {                        
                        if (sale.Buyer.Province != null)
                        {
                            sale.Buyer.Province = (from p in areas where p.name == sale.Buyer.Province.name select p).FirstOrDefault<Common_District>();
                            if (sale.Buyer.Province != null)
                            {
                                dbSale.Province_ID = sale.Buyer.Province.id;
                                tmpFees=(from fee in expFees where fee.Province_ID ==  dbSale.Province_ID select fee).ToList<Express_Fee>();
                            }
                        }

                        if (sale.Buyer.City != null)
                        {
                            sale.Buyer.City = (from p in areas where p.name == sale.Buyer.City.name select p).FirstOrDefault<Common_District>();
                            if (sale.Buyer.City != null)
                            {
                                dbSale.City_ID = sale.Buyer.City.id;
                                tmpFees = (from fee in tmpFees where fee.City_ID == dbSale.City_ID select fee).ToList<Express_Fee>();
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
                                Customer_Shop cs = new Customer_Shop() { Shop_ID = shop.Shop_ID, Customer_ID = cus.Customer_ID };
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

                                Customer_Shop cuss = (from cusshop in shop_customers where cusshop.Customer_ID == cus.Customer_ID && cusshop.Shop_ID == shop.Shop_ID select cusshop).FirstOrDefault<Customer_Shop>();
                                if (cuss == null)
                                {
                                    Customer_Shop cs = new Customer_Shop() { Shop_ID = shop.Shop_ID, Customer_ID = cus.Customer_ID };
                                    db.Customer_Shop.Add(cs);
                                }
                            }

                            dbSale.Buyer_ID = cus.Customer_ID;
                        }
                    }
                    if (tmpFees != null && tmpFees.Count > 0)
                    {
                        dbSale.Post_Fee1 = tmpFees[0].Fee;
                    }

                    dbSale.Sale_Time = sale.SaleDateTime;
                    dbSale.Shop_ID = shop.Shop_ID;
                    dbSale.Status = sale.Status;
                    dbSale.StockStatus = 0;
                    dbSale.Synced = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    Sale existed = null;
                    if (dbSales != null && dbSales.Count>0)
                    {
                        existed = (from s in dbSales where s.Mall_Trade_ID == dbSale.Mall_Trade_ID select s).FirstOrDefault<Sale>();
                    }
                    //else
                    //{
                    //    existed = (from s in db.Sale where s.Mall_Trade_ID == dbSale.Mall_Trade_ID select s).FirstOrDefault<Sale>();
                    //}

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
                                sd.Refound = order.Refound;
                                sd.Parent_Product_ID = 0;
                                sd.Product_ID = 0;

                                Product parentPdt = (from pdt in allProducts where pdt.Product_ID == order.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                                Product childPdt = (from pdt in allProducts where pdt.Product_ID == order.Product_ID select pdt).FirstOrDefault<Product>();

                                if (parentPdt != null)
                                {
                                    sd.Parent_Product_ID = parentPdt.Product_ID;
                                }
                                else
                                {
                                    if (childPdt != null)
                                    {
                                        sd.Parent_Product_ID = childPdt.Parent_ID;
                                    }
                                }

                                if (childPdt != null)
                                {
                                    sd.Product_ID = childPdt.Product_ID;
                                }

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
                                sd.Mall_SkuID = order.Mall_SkuID;
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

                            Product parentPdt = (from pdt in allProducts where pdt.Product_ID == order.Parent_Product_ID select pdt).FirstOrDefault<Product>();
                            Product childPdt = (from pdt in allProducts where pdt.Product_ID == order.Product_ID select pdt).FirstOrDefault<Product>();

                            if (parentPdt != null)
                            {
                                sd.Parent_Product_ID = parentPdt.Product_ID;
                            }
                            else
                            {
                                if (childPdt != null)
                                {
                                    sd.Parent_Product_ID = childPdt.Parent_ID;
                                }
                            }

                            if (childPdt != null)
                            {
                                sd.Product_ID = childPdt.Product_ID;
                            }

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
                this.CreateLeaveStocks(newSales, shop);
                this.HandleBackTrades(backSales,shop);
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
        public List<BSale> SearchSales(int[] pdtids, string productName,string[] trade_num,string[] trade_status, int[] customers, string customer_nick, long sSaleTime, long eSaleTime, int page, int pageSize, out int totalRecords, int shop_id = 0)
        {
            List<BSale> sales = null;
            totalRecords = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                var trades = from sale in db.Sale                             
                             select sale;

                if (shop_id == 0)
                {
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        trades = trades.Where(t => (t.Shop_ID == this.Shop.Shop_ID || cspids.Contains(t.Shop_ID)));
                    }
                    else
                    {
                        trades = trades.Where(t => t.Shop_ID == this.Shop.Shop_ID);
                    }
                }
                else
                {
                    trades = trades.Where(t => t.Shop_ID ==shop_id);
                }

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

                if (trade_num != null && trade_num.Length > 0)
                {
                    trades = trades.Where(t =>trade_num.Contains(t.Mall_Trade_ID));
                }

                if (trade_status != null && trade_status.Length > 0)
                {
                    trades = trades.Where(t => trade_status.Contains(t.Status));
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
                string[] mallProduct_ids = (from sd in sale_details select sd.Mall_PID).ToArray<string>();
                int[] cproduct_ids = (from sd in sale_details select sd.Product_ID).Distinct<int>().ToArray<int>();
                List<Product> dbProducts = (from product in db.Product where product_ids.Contains(product.Product_ID) select product).ToList<Product>();
                List<Mall_Product> dbMallProducts=(from p in db.Mall_Product where mallProduct_ids.Contains(p.Mall_ID) select p).ToList<Mall_Product>();
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
                                 Mall_PID=order.Mall_PID,
                                 //Product = new BProduct
                                 //{
                                 //    ID = ll_product.Product_ID,
                                 //    Title = ll_product.Name
                                 //}
                                 Message=order.SyncResultMessage,
                                 Refound=order.Refound
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

                        order.MallProduct = (from p in dbMallProducts
                                             where p.Mall_ID == order.Mall_PID
                                             select new BMallProduct
                                             {
                                                 ID = p.Mall_ID,
                                                 Title = p.Title
                                             }).FirstOrDefault<BMallProduct>();

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
        public List<BBackSaleDetail> SearchBackSaleDetails(string[] sale_ids, int[] user_ids, int? status, long stime, long etime, int pageIndex, int pageSize, out int totalRecords)
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
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
               
                var dbBackSales = from sale in db.Back_Sale
                                  //where sale.Shop_ID == this.Shop.Shop_ID || sale.Shop_ID == this.Main_Shop.Shop_ID || cspids.Contains(sale.Shop_ID)
                                  select sale;

                if (cspids != null && cspids.Length > 0)
                {
                    dbBackSales = dbBackSales.Where(bs => (bs.Shop_ID == this.Shop.Shop_ID || cspids.Contains(bs.Shop_ID)));
                }
                else
                {
                    dbBackSales = dbBackSales.Where(bs => (bs.Shop_ID == this.Shop.Shop_ID));
                }

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

                if (status != null)
                {
                    tmp = tmp.Where(t=>t.Status==status);
                }
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
        public List<BBackSale> SearchBackSales(string[] sale_ids, int[] user_ids, long stime, long etime, int pageIndex, int pageSize, out int totalRecords)
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
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
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
                                  Type = new BMallType { ID = mtype.Mall_Type_ID, Name = mtype.Name, Description = mtype.Description }
                              },

                              Description = sale.Description,
                              ID = sale.Back_Sale_ID,
                              Amount = sale.Amount,
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
                                      Type = new BMallType {  ID=mtype.Mall_Type_ID,Name=mtype.Name,Description=mtype.Description},
                                      Address = customer.Address
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
        /// <param name="user_id"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="syncType"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BSaleSyncLog> SearchTradeSyncLog(int user_id,int startTime,int endTime,int? syncType,int page,int pageSize,out int totalRecords)
        {
            List<BSaleSyncLog> log = new List<BSaleSyncLog>();
            if(page<=0)
            {
                page=1;
            }
            if(pageSize<=0)
            {
                pageSize=20;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                totalRecords = 0;
                int[] child_shop = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                var tmp = from sync in db.Sale_SyncTime
                          //where sync.ShopID == this.Shop.Shop_ID || sync.ShopID == this.Main_Shop.Shop_ID || child_shop.Contains(sync.ShopID)
                          select sync;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    tmp = tmp.Where(l => (l.ShopID == this.Shop.Shop_ID || child_shop.Contains(l.ShopID)));
                }
                else
                {
                    tmp = tmp.Where(l => (l.ShopID == this.Shop.Shop_ID));
                }

                if (user_id > 0)
                {
                    tmp = tmp.Where(t => t.SyncUser == user_id);
                }

                if (startTime > 0)
                {
                    tmp = tmp.Where(t => t.LastSyncTime >= startTime);
                }

                if (endTime > 0)
                {
                    tmp = tmp.Where(t => t.LastSyncTime <= endTime);
                }

                if (syncType != null)
                {
                    tmp = tmp.Where(t => t.SyncType == syncType);
                }

                var tmpSyncLog = from sync in tmp
                                 join user in db.User on sync.SyncUser equals user.User_ID into lUser
                                 from l_user in lUser.DefaultIfEmpty()
                                 join shop in db.Shop on sync.ShopID equals shop.Shop_ID into lShop
                                 from l_shop in lShop.DefaultIfEmpty()
                                 select new BSaleSyncLog
                                 {
                                     Type=sync.SyncType,
                                     LastModifiedEndTime = sync.LastTradeModifiedEndTime,
                                     LastStartEndTime = sync.LastTradeStartEndTime,
                                     LastSyncTime = sync.LastSyncTime,
                                     Shop = new BShop
                                     {
                                         ID=l_shop.Shop_ID,
                                         Title=l_shop.Name
                                     },
                                     User = new BUser
                                     {
                                         ID=l_user.User_ID,
                                         Mall_Name=l_user.Mall_Name,
                                         Mall_ID=l_user.Mall_ID
                                     }
                                 };
                totalRecords = tmpSyncLog.Count();

                log = tmpSyncLog.OrderByDescending(a => a.LastSyncTime).Skip((page - 1) * pageSize).Take(pageSize).ToList<BSaleSyncLog>();
            }

            return log;
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
                int[] cspids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
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
        public void HandleBackSaleDetail_BackStock(int backSaleID,List<BOrder> borders, int status)
        {
            if (this.CurrentUserPermission.HANDLE_BACK_SALE == 0)
            {
                throw new KMJXCException("没有处理退货单的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (backSaleID <= 0 || borders == null || borders.Count <= 0)
                {
                    throw new KMJXCException("没有找到退货单信息");
                }

                stockManager.CreateBackStock(backSaleID, borders, status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void HandleBackSaleDetail_PartialWaste(int backSaleID, List<BOrder> orders, int status)
        {
            if (this.CurrentUserPermission.HANDLE_BACK_SALE == 0)
            {
                throw new KMJXCException("没有处理退货单的权限");
            }

            if (backSaleID == 0 || orders == null || orders.Count <= 0)
            {
                throw new KMJXCException("退货单信息有错误");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                string[] order_id=(from o in orders select o.Order_ID).ToArray<string>();
                List<Back_Sale_Detail> details = (from bsd in db.Back_Sale_Detail
                                                  where bsd.Back_Sale_ID == backSaleID && order_id.Contains(bsd.Order_ID)
                                                  select bsd).ToList<Back_Sale_Detail>();

                this.HandleWastageBackSale_TotalWaste(backSaleID,orders,status);

                List<BOrder> bkOrders = new List<BOrder>();
                foreach (BOrder o in orders)
                {
                    Back_Sale_Detail detail=(from d in details where d.Order_ID==o.Order_ID select d).FirstOrDefault<Back_Sale_Detail>();
                    if (detail.Quantity - o.Quantity > 0)
                    {
                        bkOrders.Add(new BOrder() { Order_ID = o.Order_ID, Quantity = detail.Quantity - o.Quantity });
                    }
                }

                this.HandleBackSaleDetail_BackStock(backSaleID, bkOrders, status);
            }
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backSaleId"></param>
        /// <param name="order_id"></param>
        /// <param name="status"></param>
        public void HandleWastageBackSale_TotalWaste(int backSaleId,List<BOrder> borders,int status)
        {
            if (this.CurrentUserPermission.HANDLE_BACK_SALE == 0)
            {
                throw new KMJXCException("没有处理退货单的权限");
            }

            string[] order_id = null;

            if (backSaleId <= 0 || borders==null || borders.Count==0)
            {
                throw new KMJXCException("没有退货单信息");
            }

            order_id=(from o in borders select o.Order_ID).ToArray<string>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Back_Sale backSake=(from bs in db.Back_Sale where bs.Back_Sale_ID==backSaleId select bs).FirstOrDefault<Back_Sale>();
                if (backSake == null)
                {
                    throw new KMJXCException("编号为:"+backSaleId+" 的退货单信息不存在");
                }

                List<Back_Sale_Detail> details=(from bsd in db.Back_Sale_Detail where bsd.Back_Sale_ID==backSaleId && order_id.Contains(bsd.Order_ID) select bsd).ToList<Back_Sale_Detail>();

                int[] child_shop=(from child in this.DBChildShops select child.Shop_ID).ToArray<int>();

                List<Stock_Waste> wastage=(from waste in db.Stock_Waste 
                                           where waste.Shop_ID==this.Shop.Shop_ID || waste.Shop_ID==this.Main_Shop.Shop_ID || child_shop.Contains(waste.Shop_ID) 
                                           select waste).ToList<Stock_Waste>();

                Leave_Stock leave_Stock = (from ls in db.Leave_Stock where ls.Sale_ID == backSake.Sale_ID select ls).FirstOrDefault<Leave_Stock>();

                //no leave no need to set to wastage
                if (leave_Stock == null)
                {
                    return;
                }

                List<Leave_Stock_Detail> leaveDetails = (from ld in db.Leave_Stock_Detail where ld.Leave_Stock_ID == leave_Stock.Leave_Stock_ID select ld).ToList<Leave_Stock_Detail>();

                foreach (Back_Sale_Detail detail in details)
                {
                    Leave_Stock_Detail leaveDetail=(from ld in leaveDetails where ld.Order_ID==detail.Order_ID && ld.Product_ID==detail.Product_ID select ld).FirstOrDefault<Leave_Stock_Detail>();
                    if (leaveDetail == null)
                    {
                        continue;
                    }

                    int q=(from o in borders where o.Order_ID == detail.Order_ID select o.Quantity).FirstOrDefault<int>();
                    Stock_Waste w = (from waste in wastage where waste.Product_ID == detail.Product_ID select waste).FirstOrDefault<Stock_Waste>();
                    if (w == null)
                    {
                        w = new Stock_Waste();
                        w.Parent_ProductID = detail.Parent_Product_ID;
                        w.Price = detail.Price;
                        w.Product_ID = w.Product_ID;
                        w.Quantity = q;
                        w.Shop_ID = this.Shop.Shop_ID;
                        db.Stock_Waste.Add(w);
                    }
                    else
                    {
                        w.Quantity += q;
                    }

                    detail.Status = status;
                    leaveDetail.Status = status;
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
