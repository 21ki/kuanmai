using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.Util;
using KMBit.BL.Charge;
using log4net;
namespace KMBit.BL
{
    public class OrdersProcesser
    {        
        public static void ProcessOrders()
        {
            ILog logger = KMLogger.GetLogger();
            logger.Info("Going to process orders...");
            chargebitEntities db = null;
            try
            {
                db = new chargebitEntities();
                List<Charge_Order> orders = (from o in db.Charge_Order where (o.Payed==true && o.Charge_type==0 && o.Status==10) || (o.Charge_type==1 && o.Status!=3 && o.Status!=2) || (o.Charge_type == 2 && o.Status != 3 && o.Status != 2)  orderby o.Charge_type ascending select o).ToList<Charge_Order>();
                logger.Info(string.Format("Get {0} unprocessed orders", orders.Count));
                if (orders.Count==0)
                {
                    return;
                }
                
                List<Charge_Order> frontEndOrders = (from o in orders where o.Payed == true && o.Charge_type == 0 && o.Status == 10 select o).ToList<Charge_Order>();
                List<Charge_Order> agentOrders = (from o in orders where o.Charge_type == 1 && o.Status != 3 && o.Status != 2 select o).ToList<Charge_Order>();
                List<Charge_Order> backendOrders = (from o in orders where o.Charge_type == 2 && o.Status != 3 && o.Status != 2 select o).ToList<Charge_Order>();
                logger.Info(string.Format("{0} unprocessed frontend orders", frontEndOrders.Count));
                logger.Info(string.Format("{0} unprocessed agent orders", agentOrders.Count));
                logger.Info(string.Format("{0} unprocessed backend orders", backendOrders.Count));
                ChargeResult result = null;
                logger.Info("");
                foreach (Charge_Order corder in orders)
                {
                    logger.Info(string.Format("Processing order Id-{0}, Phone-{1}, Agent-{2}, ChargeType-{3}", corder.Id.ToString(),corder.Phone_number,corder.Agent_Id,corder.Charge_type));
                    ChargeOrder order = new ChargeOrder()
                    {
                        Payed = corder.Payed,
                        ChargeType = corder.Charge_type,
                        AgencyId = corder.Agent_Id,
                        OperateUserId = corder.Operate_User,
                        Id = corder.Id,
                        Province = corder.Province,
                        City = corder.City,
                        MobileSP = corder.MobileSP,
                        MobileNumber = corder.Phone_number,
                        OutId = "",
                        ResourceId = 0,
                        ResourceTaocanId = corder.Resource_taocan_id,
                        RouteId = corder.RuoteId,
                        CreatedTime = corder.Created_time
                    };
                    ChargeBridge cb = new ChargeBridge(logger);
                    result = cb.Charge(order);
                    logger.Info(string.Format("{0} - {1}",result.Status,result.Message));
                    logger.Info("");
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
        }
    }
}
