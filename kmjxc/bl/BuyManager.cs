using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class BuyManager:BBaseManager
    {
        public BuyManager(BUser user, int shop_id, Permission permission)
            : base(user, shop_id,permission)
        {
        }

        public BuyManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="user_ids"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BBuyOrder> SearchBuyOrders(int[] order_ids, int[] user_ids, int[] supplier_ids,int[] product_ids, int startTime, int endTime, int pageIndex, int pageSize, out int totalRecords) 
        {
            List<BBuyOrder> buyOrders = new List<BBuyOrder>();
            totalRecords = 0;
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var bo = from buyOrder in db.Buy_Order select buyOrder ;
                int[] spids=(from sp in this.ChildShops select sp.Shop_ID).ToArray<int>();
                if (spids != null && spids.Length > 0)
                {
                    bo = bo.Where(a=>a.Shop_ID==this.Shop.Shop_ID || a.Shop_ID==this.Main_Shop.Shop_ID || spids.Contains(a.Shop_ID));
                }

                if (user_ids !=null && user_ids.Length>0)
                {
                    bo = bo.Where(a=>user_ids.Contains(a.User_ID));
                }

                if (supplier_ids != null && supplier_ids.Length > 0)
                {
                    bo = bo.Where(a => supplier_ids.Contains(a.Supplier_ID));
                }

                if (product_ids != null && product_ids.Length > 0)
                {
                    int[] buyOrderIds=(from buyo in db.Buy_Order_Detail where product_ids.Contains(buyo.Product_ID) select buyo.Buy_Order_ID).Distinct<int>().ToArray<int>();
                    if (buyOrderIds != null && buyOrderIds.Length > 0)
                    {
                        bo = bo.Where(a => buyOrderIds.Contains(a.Buy_Order_ID));
                    }
                }
                if (order_ids != null && order_ids.Length > 0)
                {
                    bo = bo.Where(a => order_ids.Contains(a.Buy_Order_ID));
                }
                if (startTime > 0)
                {
                    bo = bo.Where(a=>a.Create_Date>=startTime);
                }

                if (endTime > 0)
                {
                    bo = bo.Where(a => a.Create_Date <= endTime);
                }
                totalRecords = bo.Count();
                if (totalRecords > 0)
                {
                    bo = bo.OrderBy(a => a.Create_Date).Skip((pageIndex-1)*pageSize).Take(pageSize);
                    buyOrders = (from border in bo
                                 select new BBuyOrder
                                     {
                                         ID = border.Buy_Order_ID,
                                         Created = border.Create_Date,
                                         Description = border.Description,
                                         EndTime = (long)border.End_Date,
                                         InsureTime = (long)border.Insure_Date,
                                         WriteTime=(long)border.Write_Date,                                         
                                         OrderUser = (from u in db.User
                                                      where u.User_ID == border.Order_User_ID
                                                      select new BUser
                                                      {
                                                          ID = u.User_ID,
                                                          Mall_ID = u.Mall_ID,
                                                          Mall_Name = u.Mall_Name,
                                                          Name = u.Name
                                                      }).FirstOrDefault<BUser>(),
                                         Status = (int)border.Status,
                                         Supplier = (from sp in db.Supplier where sp.Supplier_ID == border.Supplier_ID select sp).FirstOrDefault<Supplier>(),
                                         Created_By = (from u in db.User
                                                       where u.User_ID == border.User_ID
                                                       select new BUser
                                                       {
                                                           ID = u.User_ID,
                                                           Mall_ID = u.Mall_ID,
                                                           Mall_Name = u.Mall_Name,
                                                           Name = u.Name
                                                       }).FirstOrDefault<BUser>(),     
                                       
                                     }).ToList<BBuyOrder>();

                    foreach (BBuyOrder o in buyOrders)
                    {
                        var ds= from od in db.Buy_Order_Detail
                                     where od.Buy_Order_ID == o.ID
                                     select new BBuyOrderDetail
                                     {
                                         //BuyOrder = o,
                                         Price = od.Price,
                                         Quantity = od.Quantity,
                                         Status = (int)od.Status,
                                         Product = (from pdt in db.Product
                                                    where pdt.Product_ID == od.Product_ID
                                                    select new BProduct
                                                        {
                                                            ID = pdt.Product_ID,
                                                            Title = pdt.Name,
                                                        }).FirstOrDefault<BProduct>()
                                     };

                        List<BBuyOrderDetail> dss = ds.ToList<BBuyOrderDetail>();
                        o.Details = dss;
                    }
                }
            }

            return buyOrders;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="user_id"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BBuy> SearchBuys(int[] order_ids,int[] user_ids, int[] supplier_ids, int[] product_ids, int startTime, int endTime, int pageIndex, int pageSize, out int totalRecords)
        {
            List<KM.JXC.BL.Models.BBuy> verifications = new List<Models.BBuy>();
            totalRecords = 0;            

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var vbo = from vb in db.Buy 
                          where vb.Shop_ID == this.Shop.Shop_ID || vb.Shop_ID==this.Main_Shop.Shop_ID 
                          select vb;

                if (user_ids != null && user_ids.Length>0)
                {
                    vbo = vbo.Where(vb1 => user_ids.Contains(vb1.User_ID));
                }

                if (startTime > 0)
                {
                    vbo = vbo.Where(vb1 => vb1.Create_Date >= startTime);
                }

                if (order_ids != null && order_ids.Length > 0)
                {
                    int[] buyIds=(from bu in db.Buy where order_ids.Contains(bu.Buy_Order_ID) select bu.Buy_ID).ToArray<int>();
                    if (buyIds != null && buyIds.Length > 0)
                    {
                        vbo = vbo.Where(vb1 => buyIds.Contains(vb1.Buy_ID));
                    }
                }

                if (endTime > 0)
                {
                    vbo = vbo.Where(vb1 => vb1.Create_Date <= endTime);
                }
                totalRecords = vbo.Count();
                if (totalRecords > 0)
                {
                    vbo = vbo.OrderBy(a=>a.Create_Date).Skip((pageIndex-1)*pageSize).Take(pageSize);
                    var vboo = from vbb in vbo
                               select new BBuy
                               {
                                   Status=vbb.Status,
                                   Order =(from o in db.Buy_Order where o.Buy_Order_ID==vbb.Buy_Order_ID select new BBuyOrder{ ID=o.Buy_Order_ID}).FirstOrDefault<BBuyOrder>(),
                                   ComeDate = vbb.Come_Date,
                                   Description = vbb.Description,
                                   Created = vbb.Create_Date,
                                   ID = vbb.Buy_ID,
                                   User = (from u in db.User
                                           where u.User_ID == vbb.User_ID
                                           select new BUser
                                           {
                                               ID = u.User_ID,
                                               //EmployeeInfo = (from e in db.Employee where e.User_ID == u.User_ID select e).ToList<Employee>()[0],
                                               Mall_ID = u.Mall_ID,
                                               Mall_Name = u.Mall_Name,
                                               Parent_ID = (int)u.Parent_User_ID,
                                               Name = u.Name,
                                               Password = u.Password,
                                               //Type = (from t in db.Mall_Type where t.Mall_Type_ID == u.Mall_Type select t).ToList<Mall_Type>()[0]
                                           }).FirstOrDefault<BUser>(),
                                   Shop = (from sp in db.Shop where sp.Shop_ID == vbb.Shop_ID select new BShop { ID = sp.Shop_ID }).FirstOrDefault<BShop>()
                               };
                    verifications = vboo.ToList<BBuy>();
                    int[] orderIds;
                    orderIds = (from verify in verifications select verify.Order.ID).ToArray<int>();
                    if (orderIds != null && orderIds.Length > 0)
                    {
                        int total = 0;
                        List<BBuyOrder> orders = this.SearchBuyOrders(orderIds, null, null, null, 0, 0, 1, orderIds.Length, out total);
                        foreach (BBuy b in verifications)
                        {
                            b.Order=(from ods in orders where ods.ID==b.Order.ID select ods).FirstOrDefault<BBuyOrder>();
                        }
                    }
                }
            }

            return verifications;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buy_id"></param>
        /// <returns></returns>
        public List<BBuyDetail> GetBuyDetails(int buy_id)
        {
            List<Models.BBuyDetail> details = new List<BBuyDetail>();

            if (buy_id == 0)
            {
                throw new KMJXCException("验货单ID必须大于零");
            }

            BBuy buy = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var byo = from b in db.Buy
                          where b.Buy_ID == buy_id
                          select new BBuy
                          {
                              ComeDate = b.Come_Date,
                              Description = b.Description,
                              Created = b.Create_Date,
                              ID = b.Buy_ID,
                              User = (from u in db.User
                                      where u.User_ID == b.User_ID
                                      select new BUser
                                      {
                                          ID = u.User_ID,
                                          EmployeeInfo = (from e in db.Employee where e.User_ID == u.User_ID select e).ToList<Employee>()[0],
                                          Mall_ID = u.Mall_ID,
                                          Mall_Name = u.Mall_Name,
                                          Parent_ID = (int)u.Parent_User_ID,
                                          Name = u.Name,
                                          Password = u.Password,
                                          Type = (from t in db.Mall_Type where t.Mall_Type_ID == u.Mall_Type select t).ToList<Mall_Type>()[0]
                                      }).FirstOrDefault<BUser>(),
                              Shop = (from s in db.Shop
                                      where s.Shop_ID == b.Shop_ID
                                      select new BShop
                                          {
                                              ID = s.Shop_ID,
                                              Created = (int)s.Created,
                                              Description = s.Description,
                                              Title = s.Name,
                                              Mall_ID=s.Mall_Shop_ID,                                              
                                          }).FirstOrDefault<BShop>(),
                          };
                if (byo.ToList<BBuy>().Count == 1)
                {
                    buy = byo.ToList<BBuy>()[0];
                }

                if (buy == null)
                {
                    throw new KMJXCException("没有找到对应的验货单");
                }

                var bydo = from bb in db.Buy_Detail
                           where bb.Buy_ID == buy_id
                           select new BBuyDetail
                           {
                               Buy = buy,
                               Buy_Order_ID = bb.Buy_Order_ID,
                               CreateDate = bb.Create_Date,
                               Price = bb.Price,
                               Quantity=bb.Quantity,
                               Product = (from p in db.Product
                                          where p.Product_ID == bb.Product_ID
                                          select new BProduct
                                          {
                                              ID=bb.Product_ID
                                          }).ToList<BProduct>()[0],
                           };
            }

            return details;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buy"></param>
        /// <returns></returns>
        public bool CreateNewBuy(BBuy buy)
        {
            return this.CreateNewBuy(buy,buy.Details);
        }

        /// <summary>
        /// Create a buy which could contains details information 
        /// </summary>
        /// <param name="buy"></param>
        /// <param name="details">A list of Buy_Detail, it could be NULL</param>
        /// <returns>TRUE/FALSE</returns>
        public bool CreateNewBuy(BBuy buy, List<BBuyDetail> details)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单");
            }

            if (buy.Shop == null || buy.Shop.ID==0)
            {
                buy.Shop = new BShop() { ID = this.Shop.Shop_ID };
            }

            if (buy.User == null || buy.User.ID==0)
            {
                buy.User = new BUser() { ID = this.CurrentUser.ID };
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Buy dbBuy = new Buy();
                dbBuy.Buy_Order_ID = buy.Order.ID;
                dbBuy.Come_Date = buy.ComeDate;
                dbBuy.Create_Date = buy.Created;
                dbBuy.Description = buy.Description;
                dbBuy.Shop_ID = buy.Shop.ID;
                dbBuy.User_ID = buy.User.ID;
                dbBuy.Status = 0;
                db.Buy.Add(dbBuy);
                db.SaveChanges();
                int buy_id = (int)dbBuy.Buy_ID;
                if (buy_id <= 0)
                {
                    throw new KMJXCException("验货单创建失败");
                }

                if (details != null && details.Count>0)
                {
                    result = true;
                    result = result & this.CreateNewBuyDetails(dbBuy.Buy_ID, details);   
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buy"></param>
        /// <returns></returns>
        public bool VerifyBuyOrder(BBuy buy) 
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Buy dbBuy=(from dbbuy in db.Buy where dbbuy.Buy_Order_ID==buy.Order.ID select dbbuy).FirstOrDefault<Buy>();
                if (dbBuy == null)
                {
                    result = this.CreateNewBuy(buy, buy.Details);
                }
                else 
                {
                    result = this.CreateNewBuyDetails(dbBuy.Buy_ID, buy.Details);
                }

                //update buy order status
                var notVerified = from od in db.Buy_Order_Detail where od.Buy_Order_ID==buy.Order.ID && od.Status == 0 select od;
                if (notVerified.ToList<Buy_Order_Detail>().Count == 0) 
                {
                    Buy_Order order=(from o in db.Buy_Order where o.Buy_Order_ID==buy.Order.ID select o).FirstOrDefault<Buy_Order>();
                    if (order != null)
                    {
                        order.Status = 1;
                    }

                    db.SaveChanges();
                }

                result = true;
            }
            return result;
        }

        /// <summary>
        /// Create single buy detail
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public bool CreateNewBuyDetail(int buyId,BBuyDetail detail)
        {
            bool ret = false;

            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var d = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID && bdo.Product_ID == detail.Product.ID select bdo;
                if (d != null && d.ToList<Buy_Detail>().Count > 0)
                {
                    throw new KMJXCException("已经验过货");
                }


                if (buyId<=0)
                {
                    var d2 = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID select bdo;

                    List<Buy_Detail> existed = d2.ToList<Buy_Detail>();
                    if (existed.Count > 0)
                    {
                        buyId = existed[0].Buy_ID;
                    }
                }

                Buy_Detail dbDetail = new Buy_Detail();
                dbDetail.Buy_ID = buyId;
                dbDetail.Buy_Order_ID = detail.Buy_Order_ID;
                dbDetail.Create_Date = detail.CreateDate;
                dbDetail.Price = detail.Price;
                dbDetail.Product_ID = detail.Product.ID;
                dbDetail.Quantity = detail.Quantity;
                db.Buy_Detail.Add(dbDetail);

                Buy_Order_Detail boDetail = null;

                var d3 = from bod in db.Buy_Order_Detail where bod.Buy_Order_ID == detail.Buy_Order_ID && bod.Product_ID == detail.Product.ID select bod;
                if (d3.ToList<Buy_Order_Detail>().Count == 1)
                {
                    boDetail = d3.ToList<Buy_Order_Detail>()[0];
                }

                boDetail.Status = 1;   

                db.SaveChanges();

                //Verify buy order status
                this.VerifyBuyOrder((int)detail.Buy_Order_ID);

                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        public bool CreateNewBuyDetails(int buyId,List<BBuyDetail> details)
        {
            bool result = false;
            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单");
            }

            if (details == null)
            {
                throw new KMJXCException("输入错误");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                foreach (BBuyDetail detail in details)
                {
                    var d = from bdo in db.Buy_Detail where bdo.Buy_ID==buyId && bdo.Buy_Order_ID == detail.Buy_Order_ID && bdo.Product_ID == detail.Product.ID select bdo;
                    if (d != null && d.ToList<Buy_Detail>().Count > 0)
                    {
                        continue;
                    }

                    //if (buyId<=0)
                    //{
                    //    var d2 = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID select bdo;

                    //    List<Buy_Detail> existed = d2.ToList<Buy_Detail>();
                    //    if (existed.Count > 0)
                    //    {
                    //        buyId = existed[0].Buy_ID;
                    //    }
                    //}

                    Buy_Detail dbDetail = new Buy_Detail();
                    dbDetail.Create_Date = KM.JXC.Common.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbDetail.Buy_ID = buyId;
                    dbDetail.Buy_Order_ID = detail.Buy_Order_ID;
                    dbDetail.Create_Date = detail.CreateDate;
                    dbDetail.Price = detail.Price;
                    dbDetail.Product_ID = detail.Product.ID;
                    dbDetail.Quantity = detail.Quantity;
                    db.Buy_Detail.Add(dbDetail);

                    Buy_Order_Detail boDetail = null;

                    var d3 = from bod in db.Buy_Order_Detail where bod.Buy_Order_ID == detail.Buy_Order_ID && bod.Product_ID == detail.Product.ID select bod;
                    if (d3.ToList<Buy_Order_Detail>().Count == 1)
                    {
                        boDetail = d3.ToList<Buy_Order_Detail>()[0];
                    }

                    boDetail.Status = 1;   
                }

                db.SaveChanges();
            }
            return result;
        }
     
        /// <summary>
        /// Verify if all buy order details of one buy order have been verified
        /// </summary>
        /// <param name="buy_order_id"></param>
        private void VerifyBuyOrder(int buy_order_id)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int total_buy_order_details = db.Buy_Order_Detail.Count(bod=>bod.Buy_Order_ID==buy_order_id);
                int total_buy_details = db.Buy_Detail.Count(bd=>bd.Buy_Order_ID==buy_order_id);
                if (total_buy_details == total_buy_order_details)
                {
                    var bo = from bos in db.Buy_Order where bos.Buy_Order_ID == buy_order_id select bos;
                    Buy_Order buy_Order = (Buy_Order)bo;
                    buy_Order.Status = 1;
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buyOrder"></param>
        /// <returns></returns>
        public bool UpdateBuyOrder(BBuyOrder buy_Order)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities()) 
            {
                Buy_Order order = (from o in db.Buy_Order where o.Buy_Order_ID == buy_Order.ID select o).FirstOrDefault<Buy_Order>();
                if (order == null)
                {
                    throw new KMJXCException("订单不存在");
                }

                if (this.CurrentUserPermission.UPDATE_BUY_ORDER == 0)
                {
                    throw new KMJXCException("没有权限修改采购订单");
                }

                order.Description = buy_Order.Description;
                order.End_Date = buy_Order.EndTime;
                order.Insure_Date = buy_Order.InsureTime;
                if (buy_Order.OrderUser != null)
                {
                    order.Order_User_ID = buy_Order.OrderUser.ID;
                }
                else
                {
                    order.Order_User_ID = this.CurrentUser.ID;
                }

                if (buy_Order.Shop != null)
                {
                    order.Shop_ID = buy_Order.Shop.ID;
                }
                else
                {
                    order.Shop_ID = this.Shop.Shop_ID;
                }

                //order.Status = 0;
                order.Supplier_ID = buy_Order.Supplier.Supplier_ID;
                order.User_ID = this.CurrentUser.ID;
                order.Write_Date = buy_Order.WriteTime;              
                db.SaveChanges();
                result = true;
                if (result)
                {
                    result = this.DeleteBuyOrderDetails(buy_Order.ID);
                    if (result)
                    {
                        result=this.CreateNewBuyOrderDetails(buy_Order.ID, buy_Order.Details);
                    }
                }
            }
            return result;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buy_Order"></param>
        /// <returns></returns>
        public bool CreateNewBuyOrder(BBuyOrder buy_Order)
        {
            return this.CreateNewBuyOrder(buy_Order, buy_Order.Details);
        }

        /// <summary>
        /// Add new buy order
        /// </summary>
        /// <param name="buy_Order">Buy_Order object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool CreateNewBuyOrder(BBuyOrder buy_Order, List<BBuyOrderDetail> details)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限创建采购单合同");
            }

            if (buy_Order.Shop == null || buy_Order.Shop.ID<=0 || buy_Order.Supplier==null || buy_Order.Supplier.Supplier_ID==0)
            {
                throw new KMJXCException("Buy_Order 对象属性值不全，缺少店铺ID和供应商ID",ExceptionLevel.SYSTEM);
            }

            if (buy_Order.Supplier == null || buy_Order.Supplier.Supplier_ID == 0)
            {
                throw new KMJXCException("供应商不能为空");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Buy_Order order = new Buy_Order();

                order.Buy_Order_ID = buy_Order.ID;
                order.Create_Date = buy_Order.Created;
                order.Description = buy_Order.Description;
                order.End_Date = buy_Order.EndTime;
                order.Insure_Date = buy_Order.InsureTime;
                if (buy_Order.OrderUser != null)
                {
                    order.Order_User_ID = buy_Order.OrderUser.ID;
                }
                else
                {
                    order.Order_User_ID = this.CurrentUser.ID;
                }

                if (buy_Order.Shop != null)
                {
                    order.Shop_ID = buy_Order.Shop.ID;
                }
                else
                {
                    order.Shop_ID = this.Shop.Shop_ID;
                }

                order.Status = 0;
                order.Supplier_ID = buy_Order.Supplier.Supplier_ID;
                order.User_ID = this.CurrentUser.ID;
                order.Write_Date = buy_Order.WriteTime;

                db.Buy_Order.Add(order);
                db.SaveChanges();

                if (order.Buy_Order_ID > 0)
                {
                    result = true;
                    if (details != null && details.Count > 0)
                    {
                        result = result&this.CreateNewBuyOrderDetails(order.Buy_Order_ID, details);
                    }
                }
               
            }

            return result;
        }

        /// <summary>
        /// Delete buy order
        /// </summary>
        /// <param name="buy_order_id"></param>
        /// <returns></returns>
        public bool DeleteBuyOrder(int buy_order_id)
        {
            bool result = false;

            if (buy_order_id == 0)
            {
                throw new KMJXCException("采购单合同ID不能为空");
            }

            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单合同");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var od = from odo in db.Buy_Order_Detail where odo.Buy_Order_ID == buy_order_id select odo;
                if (od.ToList<Buy_Order_Detail>().Count > 0)
                {
                    throw new KMJXCException("采购单合同里包含产品详细订单，所以不能删除");
                }
                Buy_Order buy_Order = new Buy_Order() { Buy_Order_ID = buy_order_id };
                db.Buy_Order.Attach(buy_Order);
                db.Buy_Order.Remove(buy_Order);
                db.SaveChanges();
                result = true;
                
                
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public bool CreateNewBuyOrderDetail(int buyOrderId,BBuyOrderDetail detail)
        {
            if (this.CurrentUserPermission.ADD_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限创建采购单合同");
            }

            bool result = false;

            if (detail == null || detail.BuyOrder.ID == 0 || detail.Product.ID == 0 || detail.Quantity == 0)
            {
                throw new KMJXCException("采购单合同不能为空，产品不能为空，数量不能为零");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == detail.BuyOrder.ID select oo;
                Buy_Order order = (Buy_Order)o;
                if (order.Status != 0)
                {
                    throw new KMJXCException("此采购单已经验货或者验货未通过，所以不能继续添加");
                }

                Buy_Order_Detail dbOrderDetail = new Buy_Order_Detail();
                if (buyOrderId <= 0)
                {
                    dbOrderDetail.Buy_Order_ID = detail.BuyOrder.ID;
                }
                else
                {
                    dbOrderDetail.Buy_Order_ID = buyOrderId;
                }
                dbOrderDetail.Price = detail.Price;
                dbOrderDetail.Product_ID = detail.Product.ID;
                dbOrderDetail.Quantity = detail.Quantity;
                dbOrderDetail.Status = detail.Status;               
                db.Buy_Order_Detail.Add(dbOrderDetail);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="details"></param>
        /// <returns></returns>
        public bool CreateNewBuyOrderDetails(int buyOrderId, List<BBuyOrderDetail> details)
        {
            if (this.CurrentUserPermission.ADD_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限创建采购单合同");
            }
            bool result = false;

            if (details != null)
            {
                using (KuanMaiEntities db = new KuanMaiEntities())
                {
                    if (buyOrderId <= 0)
                    {
                        var o = from oo in db.Buy_Order where oo.Buy_Order_ID == details[0].BuyOrder.ID select oo;
                        Buy_Order order = (Buy_Order)o;
                        if (order.Status != 0)
                        {
                            throw new KMJXCException("此采购单已经验货或者验货未通过，不能继续添加采购合同产品信息");
                        }

                        buyOrderId = order.Buy_Order_ID;
                    }

                    foreach (BBuyOrderDetail detail in details)
                    {
                        Buy_Order_Detail dbOrderDetail = new Buy_Order_Detail();
                        dbOrderDetail.Buy_Order_ID = buyOrderId;
                        dbOrderDetail.Price = detail.Price;
                        dbOrderDetail.Product_ID = detail.Product.ID;
                        dbOrderDetail.Quantity = detail.Quantity;
                        dbOrderDetail.Status = detail.Status;
                        db.Buy_Order_Detail.Add(dbOrderDetail);
                    }

                    db.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Delete single buy order detail from buy order
        /// </summary>
        /// <param name="detail">Buy_Order_Detail object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool DeleteBuyOrderDetail(BBuyOrderDetail detail)
        {
            bool result = false;

            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单合同里的产品信息");
            }

            if (detail == null || detail.BuyOrder==null || detail.BuyOrder.ID == 0)
            {
                throw new KMJXCException("请选择采购单合同");
            }

            if (detail.Product == null || detail.Product.ID == 0)
            {
                throw new KMJXCException("请选择产品");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == detail.BuyOrder.ID select oo;
                Buy_Order order = (Buy_Order)o;
                if (order.Status == 1)
                {
                    throw new KMJXCException("此采购单已经验货通过，不能删除任何产品采购信息");
                }

                if (order.Status == 3)
                {
                    throw new KMJXCException("此采购单被冻结，不能删除任何产品采购信息");
                }

                Buy_Order_Detail dbOrderDetail = (from dbdetail in db.Buy_Order_Detail 
                                                  where dbdetail.Buy_Order_ID==detail.BuyOrder.ID && dbdetail.Product_ID==detail.Product.ID 
                                                  select dbdetail).FirstOrDefault<Buy_Order_Detail>();
                if (dbOrderDetail != null)
                {                   
                    db.Buy_Order_Detail.Remove(dbOrderDetail);
                    db.SaveChanges();
                }
               
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buyOrderID"></param>
        /// <returns></returns>
        public bool DeleteBuyOrderDetails(int buyOrderID)
        {
            bool result = false;
            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单产品采购信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {                
                Buy_Order order = (from oo in db.Buy_Order where oo.Buy_Order_ID == buyOrderID select oo).FirstOrDefault<Buy_Order>();

                if (order == null)
                {
                    throw new KMJXCException("采购订单不存在");
                }

                if (order.Status == 1)
                {
                    throw new KMJXCException("此采购单已经验货通过，不能删除任何产品采购信息");
                }

                List<Buy_Order_Detail> details=(from detail in db.Buy_Order_Detail where detail.Buy_Order_ID==buyOrderID select detail).ToList<Buy_Order_Detail>();
                foreach (Buy_Order_Detail d in details)
                {
                    db.Buy_Order_Detail.Remove(d);
                }

                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Delete multiple buy order details from one buy order
        /// </summary>
        /// <param name="details">Buy_Order_Detail list object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool DeleteBuyOrderDetails(List<BBuyOrderDetail> details)
        {
            bool result = false;
           
            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单产品采购信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == details[0].BuyOrder.ID select oo;
                Buy_Order order = (Buy_Order)o;

                if (order.Status == 1)
                {
                    throw new KMJXCException("此采购单已经验货通过，不能删除任何产品采购信息");
                }

                if (order.Status == 3)
                {
                    throw new KMJXCException("此采购单被冻结，不能删除任何产品采购信息");
                }

                foreach (BBuyOrderDetail detail in details)
                {
                    Buy_Order_Detail dbOrderDetail = (from dbdetail in db.Buy_Order_Detail
                                                      where dbdetail.Buy_Order_ID == detail.BuyOrder.ID && dbdetail.Product_ID == detail.Product.ID
                   
                                                      select dbdetail).FirstOrDefault<Buy_Order_Detail>();
                    if (dbOrderDetail != null)
                    {
                        db.Buy_Order_Detail.Remove(dbOrderDetail);
                    }
                }

                db.SaveChanges();
                result = true;        
            }

            return result;
        }
    }
}
