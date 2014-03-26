using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

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
        public StockManager(User user,int shop_id)
            : base(user, shop_id)
        {
        }

        public StockManager(User user)
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
                              Shop = (from s in db.Shop where s.Shop_ID == o2.Shop_ID select s).FirstOrDefault<Shop>(),
                              User = (from u in db.User
                                      where u.User_ID == o2.User_ID
                                      select new BUser
                                      { 
                                         ID=u.User_ID,
                                         EmployeeInfo=(from e in db.Employee where e.User_ID==u.User_ID select e).ToList<Employee>()[0],
                                         Mall_ID=u.Mall_ID,
                                         Mall_Name=u.Mall_Name,
                                         Parent_ID = (int)u.Parent_User_ID,
                                         Name=u.Name,
                                         Password=u.Password,
                                         Type = (from t in db.Mall_Type where t.Mall_Type_ID==u.Mall_Type select t).ToList<Mall_Type>()[0]
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

                                          }).ToList<BProduct>()[0],
                               Quantity = sddd.Quantity,
                               Price = (double)sddd.Price,
                               DateTime = (int)sddd.Create_Date,
                               Invoiced = (bool)sddd.Have_Invoice,
                               InvoiceAmount = (double)sddd.Invoice_Amount,
                               InvoiceNumber = sddd.Invoice_Num
                           };
            }
            
            return details;
        }

        /// <summary>
        /// Add new enter stock record
        /// </summary>
        /// <param name="stock">Instance of Enter_Stock object</param>
        /// <returns></returns>
        public bool EnterStock(Enter_Stock stock)
        {
            bool result = false;
            if (stock == null)
            {
                return result;
            }

            if (stock.Buy_ID <= 0) {
                throw new KMJXCException("入库单未包含任何采购单信息");
            }

            if (stock.Shop_ID <= 0)
            {
                throw new KMJXCException("入库单未包含店铺信息");
            }

            if (stock.StoreHouse_ID <= 0)
            {
                throw new KMJXCException("入库单未包含任何仓库信息");
            }

            if (stock.User_ID <= 0)
            {
                stock.User_ID = this.CurrentUser.User_ID;
            }

            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Enter_Stock.Add(stock);
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Add multiple stock detail records
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool EnterStockDetails(List<Enter_Stock_Detail> details)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }
            KuanMaiEntities db = new KuanMaiEntities();
            foreach (Enter_Stock_Detail detail in details)
            {
                if (detail.Enter_Stock_ID > 0 && detail.Quantity > 0 && detail.Product_ID > 0)
                {
                    db.Enter_Stock_Detail.Add(detail);
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch
            {
                throw new KMJXCException("未知错误");
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
        public bool EnterStockDetail(Enter_Stock_Detail detail)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_ENTER_STOCK == 0)
            {
                throw new KMJXCException("没有新增入库单的权限");
            }

            if (detail.Enter_Stock_ID == 0)
            {
                throw new KMJXCException("必须指定入库单");
            }

            if (detail.Product_ID == 0)
            {
                throw new KMJXCException("必须指定商品");
            }

            if (detail.Quantity == 0)
            {
                throw new KMJXCException("数量必须大于零");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Enter_Stock_Detail.Add(detail);
                db.SaveChanges();
                result = true;
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

                    var sp = from spl in db.Stock_Pile where spl.Product_ID == product_id select spl;
                    Stock_Pile stock_Pile = null;
                    if (sp != null && sp.ToList<Stock_Pile>().Count>0)
                    {
                        stock_Pile = sp.ToList<Stock_Pile>()[0];
                    }

                    if (stock_Pile != null)
                    {
                        stock_Pile.Quantity = stock_Pile.Quantity - quantity;
                        stock_Pile.LastLeave_Time = lstock.Leave_Date;
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
    }
}
