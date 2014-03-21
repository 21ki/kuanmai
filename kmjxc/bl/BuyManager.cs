using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
namespace KM.JXC.BL
{
    public class BuyManager:BaseManager
    {
        public BuyManager(User user, int shop_id)
            : base(user, shop_id)
        {
        }

        public BuyManager(User user)
            : base(user)
        {
        }

        

        /// <summary>
        /// Create a buy which could contains details information 
        /// </summary>
        /// <param name="buy"></param>
        /// <param name="details">A list of Buy_Detail, it could be NULL</param>
        /// <returns>TRUE/FALSE</returns>
        public bool CreateNewBuy(Buy buy,List<Buy_Detail> details)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单");
            }

            if (buy.Shop_ID == 0)
            {
                throw new KMJXCException("店铺信息丢失，不能创建验货单");
            }

            if (buy.User_ID == 0)
            {
                buy.User_ID = this.CurrentUser.User_ID;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Buy.Add(buy);
                db.SaveChanges();
                int buy_id = (int)buy.Buy_ID;
                if (details != null)
                {
                    foreach (Buy_Detail detail in details)
                    {
                        db.Buy_Detail.Add(detail);

                        Buy_Order_Detail boDetail = null;

                        var d3 = from bod in db.Buy_Order_Detail where bod.Buy_Order_ID == detail.Buy_Order_ID && bod.Product_ID == detail.Product_ID select bod;
                        if (d3.ToList<Buy_Order_Detail>().Count == 1)
                        {
                            boDetail = d3.ToList<Buy_Order_Detail>()[0];
                        }

                        boDetail.Status = 1;   
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
        public bool CreateNewBuyDetail(Buy_Detail detail)
        {
            bool ret = false;

            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var d = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID && bdo.Product_ID == detail.Product_ID select bdo;
                if (d != null && d.ToList<Buy_Detail>().Count > 0)
                {
                    throw new KMJXCException("已经验过货");
                }

                var d2 = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID select bdo;

                List<Buy_Detail> existed = d2.ToList<Buy_Detail>();
                if (existed.Count > 0)
                {
                    if (detail.Buy_ID != existed[0].Buy_ID)
                    {
                        detail.Buy_ID = existed[0].Buy_ID;
                    }
                }
                db.Buy_Detail.Add(detail);

                Buy_Order_Detail boDetail = null;

                var d3 = from bod in db.Buy_Order_Detail where bod.Buy_Order_ID == detail.Buy_Order_ID && bod.Product_ID == detail.Product_ID select bod;
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

        private void UpdateBuyOrderDetail(Buy_Order_Detail detail)
        {

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
        /// <param name="details"></param>
        /// <returns></returns>
        public bool CreateNewBuyDetails(List<Buy_Detail> details)
        {
            bool ret = false;

            if (this.CurrentUserPermission.ADD_BUY == 0)
            {
                throw new KMJXCException("没有权限创建验货单信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {               
                foreach (Buy_Detail detail in details)
                {
                    var d = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID && bdo.Product_ID == detail.Product_ID select bdo;
                    if (d != null && d.ToList<Buy_Detail>().Count > 0)
                    {
                        continue;
                    }

                    var d2 = from bdo in db.Buy_Detail where bdo.Buy_Order_ID == detail.Buy_Order_ID select bdo;

                    List<Buy_Detail> existed = d2.ToList<Buy_Detail>();
                    if (existed.Count > 0)
                    {
                        if (detail.Buy_ID != existed[0].Buy_ID)
                        {
                            detail.Buy_ID = existed[0].Buy_ID;
                        }
                    }
                    db.Buy_Detail.Add(detail);

                    Buy_Order_Detail boDetail = null;

                    var d3 = from bod in db.Buy_Order_Detail where bod.Buy_Order_ID == detail.Buy_Order_ID && bod.Product_ID == detail.Product_ID select bod;
                    if (d3.ToList<Buy_Order_Detail>().Count == 1)
                    {
                        boDetail = d3.ToList<Buy_Order_Detail>()[0];
                    }

                    boDetail.Status = 1;                    
                }


                if (details.Count > 0)
                {
                    this.VerifyBuyOrder((int)details[0].Buy_Order_ID);
                }
                db.SaveChanges();
            }
            return ret;
        }

        /// <summary>
        /// Add new buy order
        /// </summary>
        /// <param name="buy_Order">Buy_Order object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool CreateNewBuyOrder(Buy_Order buy_Order)
        {
            bool result = false;

            if (buy_Order == null || buy_Order.Shop_ID<=0 || buy_Order.Supplier_ID<=0)
            {
                throw new KMJXCException("Buy_Order 对象属性值不全，缺少店铺ID和供应商ID",ExceptionLevel.SYSTEM);
            }

            if (this.CurrentUserPermission.ADD_BUY_ORDER==0)
            {
                throw new KMJXCException("没有权限创建采购单合同");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Buy_Order.Add(buy_Order);
                db.SaveChanges();
                result = true;
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
        public bool CreateNewBuyOrderDetail(Buy_Order_Detail detail)
        {
            bool result = false;

            if (detail == null || detail.Buy_Order_ID == 0 || detail.Product_ID == 0 || detail.Quantity == 0)
            {
                throw new KMJXCException("采购单合同不能为空，产品不能为空，数量不能为零");
            }

            if (this.CurrentUserPermission.ADD_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单合同以及采购单合同详细信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == detail.Buy_Order_ID select oo;
                Buy_Order order = (Buy_Order)o;
                if (order.Status != 0)
                {
                    throw new KMJXCException("此采购单已经验货或者验货未通过，所以不能继续添加");
                }
                db.Buy_Order_Detail.Add(detail);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Delete single buy order detail from buy order
        /// </summary>
        /// <param name="detail">Buy_Order_Detail object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool DeleteBuyOrderDetail(Buy_Order_Detail detail)
        {
            bool result = false;

            if (detail == null || detail.Buy_Order_ID == 0)
            {
                throw new KMJXCException("请选择采购单合同");
            }

            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单合同");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == detail.Buy_Order_ID select oo;
                Buy_Order order = (Buy_Order)o;
                if (order.Status == 1)
                {
                    throw new KMJXCException("此采购单已经验货通过，不能删除任何产品采购信息");
                }

                if (order.Status == 3)
                {
                    throw new KMJXCException("此采购单被冻结，不能删除任何产品采购信息");
                }

                db.Buy_Order_Detail.Attach(detail);
                db.Buy_Order_Detail.Remove(detail);
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
        public bool DeleteBuyOrderDetails(List<Buy_Order_Detail> details)
        {
            bool result = false;
           
            if (this.CurrentUserPermission.DELETE_BUY_ORDER == 0)
            {
                throw new KMJXCException("没有权限删除采购单产品采购信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var o = from oo in db.Buy_Order where oo.Buy_Order_ID == details[0].Buy_Order_ID select oo;
                Buy_Order order = (Buy_Order)o;

                if (order.Status == 1)
                {
                    throw new KMJXCException("此采购单已经验货通过，不能删除任何产品采购信息");
                }

                if (order.Status == 3)
                {
                    throw new KMJXCException("此采购单被冻结，不能删除任何产品采购信息");
                }

                foreach (Buy_Order_Detail detail in details)
                {
                    db.Buy_Order_Detail.Attach(detail);
                    db.Buy_Order_Detail.Remove(detail);
                }

                db.SaveChanges();
                result = true;        
            }

            return result;
        }
    }
}
