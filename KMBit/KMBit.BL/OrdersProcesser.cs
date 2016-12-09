using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.Util;
using KMBit.BL.Charge;
using log4net;
using WeChat.Adapter.Requests;
namespace KMBit.BL
{
    public class OrdersProcesser
    {      
        static ILog logger = KMLogger.GetLogger();
        static int lastOrderId = 0;
        static object o = new object();
        static ChargeBridge cb = new ChargeBridge(logger);
        public static void ProcessOrders()
        {            
            logger.Info("Going to process orders...");
            chargebitEntities db = null;
            try
            {                
                List<Charge_Order> orders = null;
                db = new chargebitEntities();
                lock (o)
                {
                    logger.Info("Last Max Order Id:" + lastOrderId);
                    orders = (from o in db.Charge_Order where o.Id > lastOrderId && ((o.Payed == true && o.Charge_type == 0 && o.Status == 10) || (o.Charge_type == 1 && o.Status != 3 && o.Status != 2) || (o.Charge_type == 2 && o.Status != 3 && o.Status != 2)) orderby o.Charge_type ascending orderby o.Id ascending select o).ToList<Charge_Order>();
                    logger.Info(string.Format("Get {0} unprocessed orders", orders.Count));
                    if (orders.Count == 0)
                    {
                        logger.Info("No unprocessed orders, thread will exit!");
                        return;
                    }

                    lastOrderId = (from o in orders select o.Id).Max();
                    logger.Info("Max Order Id updated to:"+lastOrderId);
                }

                List<int> agentIds = (from o in orders where o.Agent_Id > 0 select o.Agent_Id).ToList<int>();
                List<Users> agents = new List<Users>();
                if(agentIds!=null && agentIds.Count>0)
                {
                    int[] ids = agentIds.ToArray<int>();
                    agents = (from u in db.Users where ids.Contains(u.Id) select u).ToList<Users>();
                }
                List<Charge_Order> frontEndOrders = (from o in orders where o.Payed == true && o.Charge_type == 0 && o.Status == 10 select o).ToList<Charge_Order>();
                List<Charge_Order> agentOrders = (from o in orders where o.Charge_type == 1 && o.Status != 3 && o.Status != 2 select o).ToList<Charge_Order>();
                List<Charge_Order> backendOrders = (from o in orders where o.Charge_type == 2 && o.Status != 3 && o.Status != 2 select o).ToList<Charge_Order>();
                //logger.Info(string.Format("{0} unprocessed frontend orders", frontEndOrders.Count));
                //logger.Info(string.Format("{0} unprocessed agent orders", agentOrders.Count));
                //logger.Info(string.Format("{0} unprocessed backend orders", backendOrders.Count));
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
                        OutOrderId = "",
                        ResourceId = 0,
                        ResourceTaocanId = corder.Resource_taocan_id,
                        RouteId = corder.RuoteId,
                        CreatedTime = corder.Created_time
                    };

                    result = cb.Charge(order);
                    Users agent = (from u in agents where corder.Agent_Id>0 && corder.Agent_Id==u.Id select u).FirstOrDefault<Users>();
                    logger.Info("Order status - " +order.Status);
                    if(order.Status==1)
                    {
                        logger.Info("Sync status process will do the callback request.");
                    }
                    if(!string.IsNullOrEmpty(corder.CallBackUrl) && corder.Agent_Id>0 && agent!=null && order.Status!=1)
                    {
                        //send back the status to agent system
                        NameValueCollection col = new NameValueCollection();
                        SortedDictionary<string, string> param = new SortedDictionary<string, string>();
                        param.Add("OrderId", corder.Id.ToString());
                        param.Add("ClientOrderId", corder.Client_Order_Id!=null?corder.Client_Order_Id.ToString():"");
                        param.Add("Message", result.Message);
                        param.Add("Status", result.Status.ToString());
                        string querystr = "";
                        foreach(KeyValuePair<string,string> p in param)
                        {
                            col.Add(p.Key, p.Value != null ? p.Value : "");
                            if (querystr=="")
                            {
                                querystr = p.Key + "=" + (p.Value != null ? p.Value : "");
                            }
                            else
                            {
                                querystr +="&"+ p.Key + "=" + (p.Value != null ? p.Value : "");
                            }
                        }
                        logger.Info(string.Format("Post data to callback url - {0}",corder.CallBackUrl));
                        logger.Info(string.Format("Data - {0}",querystr));                       
                        querystr += "&key=" + agent.SecurityStamp;
                        string sign = UrlSignUtil.GetMD5(querystr);
                        col.Add("Sign",sign);
                        //logger.Info("sign=" + sign);
                        HttpSercice.PostHttpRequest(corder.CallBackUrl, col, WeChat.Adapter.Requests.RequestType.POST, null);
                    }
                    logger.Info(string.Format("Order ID:{2} - {0} - {1}",result.Status,result.Message,corder.Id));
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

        public static void ProcessAgentAccountChargePaymentsNew()
        {
            ChargeService.ProcessAgentAccountChargePayments();
        }

        public static void SyncStatus()
        {
            cb.SyncChargeStatus();
        }
        public static void ProcessAgentAccountChargePayments()
        {
            logger.Info("ProcessAgentAccountChargePayments...");
            chargebitEntities db = null ;
            lock(ChargeService.o)
            {
                try
                {
                    db = new chargebitEntities();
                    List<Payment_history> payments = (from p in db.Payment_history where p.ChargeOrderId==0 && p.PayType != 0 && p.Status == 1 orderby p.PayType ascending select p).ToList<Payment_history>();
                    if (payments.Count > 0)
                    {
                        List<int> userIds = (from p in payments select p.User_id).ToList<int>();
                        List<Users> agents = (from u in db.Users where userIds.Contains(u.Id) select u).ToList<Users>();
                        foreach (Payment_history payment in payments)
                        {
                            logger.Info(string.Format("Payment Id - {0}, Agent Id - {1}, Amount - {2}, OperUser - {3}",payment.Id,payment.User_id,payment.Amount.ToString("0.00"),payment.OperUserId));
                            Users user = (from u in agents where u.Id == payment.User_id select u).FirstOrDefault<Users>();
                            if (user != null)
                            {
                                logger.Info(string.Format("Agent Name = {0}",user.Name));
                                user.Remaining_amount = user.Remaining_amount + payment.Amount;
                                payment.Status = 2;
                                db.SaveChanges();
                            }
                           
                            logger.Info("Done.");
                        }
                    }
                    else
                    {
                        logger.Info("No agent account charge payments need to be handled.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    if (db != null)
                    {
                        db.Dispose();
                    }
                }
            }
            logger.Info("Leaving ProcessAgentAccountChargePayments...");
        }
    }
}
