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
    public class StockManager:BBaseManager
    {
        public StockManager(BUser user,int shop_id)
            : base(user, shop_id)
        {
        }

        public StockManager(BUser user)
            : base(user)
        {
        }

        /// <summary>
        /// Get Enter stocks
        /// </summary>
        /// <param name="user_id">who create the enter stock</param>
        /// <param name="startTime">date time range for enter date</param>
        /// <param name="endTime">date time range for enter date</param>
        /// <param name="storeHouseId">store house</param>
        /// <param name="pageIndex">page</param>
        /// <param name="pageSize">page size</param>
        /// <param name="totalRecords">total records fitting the input conditions</param>
        /// <returns>a list of enter stock</returns>
        public List<BEnterStock> GetEnterStocks(int enter_stock_id,int user_id,int startTime,int endTime,int storeHouseId, int pageIndex,int pageSize,out int totalRecords)
        {            
            List<BEnterStock> stocks = new List<BEnterStock>();
            totalRecords = 0;
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var os = from o in db.Enter_Stock
                         select o;  
                os=os.Where(o1=>o1.Shop_ID==this.Shop_Id);

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
                    os=os.Where(o11=>o11.Enter_Date>startTime);
                }

                if (endTime > 0)
                {
                    os = os.Where(o11 => o11.Enter_Date < endTime);
                }

                totalRecords = os.Count();
                var oos = from o2 in os
                          orderby o2.Enter_Date descending
                          select new BEnterStock()
                          {
                              ID = (int)o2.Enter_Stock_ID,
                              Shop = (from sp in db.Shop
                                      where sp.Shop_ID == o2.Shop_ID
                                      select new BShop
                                      {
                                          Chindren = null,
                                          Created = (int)sp.Created,
                                          Description = sp.Description,
                                          ID = sp.Shop_ID,
                                          Mall_ID = sp.Mall_Shop_ID,
                                          Parent = null,
                                          Synced = (int)sp.Synced,
                                          Title = sp.Name,
                                          Type = null,

                                      }).FirstOrDefault<BShop>(),
                              User = (from u in db.User
                                      where u.User_ID == o2.User_ID
                                      select new BUser
                                      {
                                          ID = u.User_ID,
                                          //EmployeeInfo=(from e in db.Employee where e.User_ID==u.User_ID select e).ToList<Employee>()[0],
                                          Mall_ID = u.Mall_ID,
                                          Mall_Name = u.Mall_Name,
                                          Parent_ID = (int)u.Parent_User_ID,
                                          Name = u.Name,
                                          Password = u.Password,
                                          //Type = (from t in db.Mall_Type where t.Mall_Type_ID==u.Mall_Type select t).ToList<Mall_Type>()[0]
                                      }).FirstOrDefault<BUser>(),
                              BuyID = (int)o2.Buy_ID,
                              EnterTime = (int)o2.Enter_Date
                          };
                                
                oos.Skip((pageIndex-1)*pageSize).Take(pageSize);

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
            List<BEnterStock> stocks = this.GetEnterStocks(enter_stock_id, 0, 0, 0, 0, 1, 1, out stockCount);
            if (stockCount == 0)
            {
                throw new KMJXCException("此入库单不存在");
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
        /// Add new enter stock record
        /// </summary>
        /// <param name="stock">Instance of Enter_Stock object</param>
        /// <returns></returns>
        public bool EnterStock(BEnterStock stock)
        {
            bool result = false;
            if (stock == null)
            {
                return result;
            }

            if (stock.BuyID <= 0) {
                throw new KMJXCException("入库单未包含任何采购单信息");
            }

            if (stock.Shop==null)
            {
                stock.Shop = new BShop() { ID = this.Shop_Id, Title=this.Shop.Name };
            }

            if (stock.StoreHouse ==null)
            {
                throw new KMJXCException("入库单未包含任何仓库信息");
            }

            if (stock.User == null)
            {
                stock.User = this.CurrentUser;
            }

            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Enter_Stock dbStock = new Enter_Stock();

                dbStock.Buy_ID = stock.BuyID;
                dbStock.Enter_Date = stock.EnterTime;
                dbStock.Enter_Stock_ID = 0;
                dbStock.Shop_ID = stock.Shop.ID;
                dbStock.StoreHouse_ID = stock.StoreHouse.StoreHouse_ID;
                dbStock.User_ID = stock.User.ID;
                db.Enter_Stock.Add(dbStock);
                db.SaveChanges();
                if (dbStock.Enter_Stock_ID <= 0)
                {
                    throw new KMJXCException("入库单创建失败");
                }

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Add multiple stock detail records
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool EnterStockDetails(int stockId,List<BEnterStockDetail> details)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单产品信息的权限");
            }

            if (stockId <= 0)
            {
                throw new KMJXCException("");
            }

            if (details == null)
            {
                throw new KMJXCException("输入错误",ExceptionLevel.SYSTEM);
            }            

            KuanMaiEntities db = new KuanMaiEntities();
            List<Stock_Pile> stockPiles = (from sp in db.Stock_Pile where sp.Shop_ID == this.Shop.Shop_ID select sp).ToList<Stock_Pile>();
            Enter_Stock stock = (from st in db.Enter_Stock where st.Enter_Stock_ID == stockId select st).FirstOrDefault<Enter_Stock>();
            if (stock == null)
            {
                throw new KMJXCException("");
            }
            foreach (BEnterStockDetail detail in details)
            {
                Enter_Stock_Detail dbDetail = new Enter_Stock_Detail();
                dbDetail.Create_Date = detail.Created;
                dbDetail.Enter_Stock_ID = stockId;
                dbDetail.Have_Invoice = detail.Invoiced;
                dbDetail.Invoice_Amount = decimal.Parse(detail.InvoiceAmount.ToString("0.00"));
                dbDetail.Invoice_Num = detail.InvoiceNumber;
                dbDetail.Price = decimal.Parse(detail.Price.ToString("0.00"));
                dbDetail.Product_ID = detail.Product.ID;
                dbDetail.Quantity = (int)detail.Quantity;
                db.Enter_Stock_Detail.Add(dbDetail);   
             
                //update stock pile
                Stock_Pile stockPile = (from sp in stockPiles where sp.Product_ID == dbDetail.Product_ID && sp.StockHouse_ID == stock.StoreHouse_ID select sp).FirstOrDefault<Stock_Pile>();
                if (stockPile != null)
                {
                    stockPile.Quantity = stockPile.Quantity + dbDetail.Quantity;
                    stockPile.Price = dbDetail.Price;
                    if (stockPile.First_Enter_Time == 0)
                    {
                        stockPile.First_Enter_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    }
                }
            }

            db.SaveChanges();

            try
            {
                db.SaveChanges();
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
                detail.EnterStock = new BEnterStock() { ID = stockId, StoreHouse = new Store_House() { StoreHouse_ID = dbStock.StoreHouse_ID } };
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
                dbDetail.Invoice_Amount = decimal.Parse(detail.InvoiceAmount.ToString("0.00"));
                dbDetail.Invoice_Num = detail.InvoiceNumber;
                dbDetail.Price = decimal.Parse(detail.Price.ToString("0.00"));
                dbDetail.Product_ID = detail.Product.ID;
                dbDetail.Quantity = (int)detail.Quantity;
                db.Enter_Stock_Detail.Add(dbDetail);
                db.SaveChanges();

                //update stock pile
                Stock_Pile stockPile = (from sp in db.Stock_Pile where sp.Product_ID == dbDetail.Product_ID && sp.StockHouse_ID==detail.EnterStock.StoreHouse.StoreHouse_ID select sp).FirstOrDefault<Stock_Pile>();
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
        public bool LeaveStock(Leave_Stock lstock)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_LEAVE_STOCK == 0)
            {
                throw new KMJXCException("没有权限出库");
            }

            if (lstock.Sale_ID == 0)
            {
                throw new KMJXCException("必须选择订单出库");
            }

            if (lstock.Shop_ID == 0)
            {
                throw new KMJXCException("必须选择店铺");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Leave_Stock.Add(lstock);

                //update stock pile
                var os = from o in db.Sale_Detail where o.Sale_ID == lstock.Sale_ID select o;
                List<Sale_Detail> orders = os.ToList<Sale_Detail>();
                foreach (Sale_Detail order in orders)
                {
                    int product_id = (int)order.Product_ID;
                    int quantity = (int)order.Quantity;

                    var sp = from spl in db.Stock_Pile where spl.Product_ID == product_id && spl.StockHouse_ID == lstock.StoreHouse_ID select spl;
                    Stock_Pile stock_Pile = null;
                    if (sp != null && sp.ToList<Stock_Pile>().Count>0)
                    {
                        stock_Pile = sp.ToList<Stock_Pile>()[0];
                    }

                    if (stock_Pile != null)
                    {
                        stock_Pile.Quantity = stock_Pile.Quantity - quantity;
                        stock_Pile.LastLeave_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    }
                }

                if (orders.Count > 0)
                {
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Batch leave stocks
        /// </summary>
        /// <param name="stocks"></param>
        public void LeaveStocks(List<Leave_Stock> stocks)
        {
            foreach (Leave_Stock stock in stocks)
            {
                this.LeaveStock(stock);
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

            if (stockPile.StockHouse_ID <= 0)
            {
                throw new KMJXCException("");
            }

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
        public void CreateStoreHouse(Store_House house)
        {
            if (this.CurrentUserPermission.ADD_STORE_HOUSE == 0)
            {
                throw new KMJXCException("没有创建仓库的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int existing = (from h in db.Store_House where h.Title == house.Title select h).Count();
                if (existing > 0)
                {
                    throw new KMJXCException("此仓库名字已经存在");
                }
                db.Store_House.Add(house);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Store_House> GetStoreHouses()
        {
            List<Store_House> houses = new List<Store_House>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                houses = (from house in db.Store_House where house.Shop_ID == this.Main_Shop_Id select house).ToList<Store_House>();
            }

            return houses;
        }
    }
}
