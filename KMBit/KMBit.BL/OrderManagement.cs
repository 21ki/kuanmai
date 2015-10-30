using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Util;
using KMBit.Beans;
using KMBit.BL.Charge;
namespace KMBit.BL
{
    public class OrderManagement:BaseManagement
    {
        public OrderManagement():base()
        {

        }

        public OrderManagement(int userId) : base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(OrderManagement));
            }
        }

        public OrderManagement(string email) : base(email)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(OrderManagement));
            }
        }

        public OrderManagement(BUser user) : base(user)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(OrderManagement));
            }
        }

        /// <summary>
        /// This method is only applied to platform direct charge
        /// </summary>
        /// <param name="orderId"></param>
        public ChargeResult ProcessOrderAfterPaid(int paymentId,string tradeNo,string buyerAccount)
        {
            ChargeResult result = new ChargeResult();
            using (chargebitEntities db = new chargebitEntities())
            {
                Payment_history payment = (from p in db.Payment_history where p.Id==paymentId select p).FirstOrDefault<Payment_history>();
                if(payment==null)
                {
                    result.Status = ChargeStatus.FAILED;
                    result.Message = string.Format("编号为:{0}的支付编号不存在", paymentId);
                    return result;
                }
                payment.Pay_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                payment.PaymentAccount = buyerAccount!=null?buyerAccount:"";
                payment.PaymentTradeId = tradeNo != null ? tradeNo : "";
                db.SaveChanges();
                //前台用户直充网络支付成功之后，提交订单到资源充值
                if (payment.PayType==0)
                {
                    if(payment.ChargeOrderId<=0)
                    {
                        result.Status = ChargeStatus.FAILED;
                        result.Message = string.Format("编号为:{0}的支付编号没有相关充值订单", paymentId);
                        return result;
                    }

                    Charge_Order corder = (from o in db.Charge_Order where o.Id==payment.ChargeOrderId select o).FirstOrDefault<Charge_Order>();
                    corder.Payed = true;
                    db.SaveChanges();

                    ChargeOrder order = new ChargeOrder()
                    { Payed=true, ChargeType = corder.Charge_type, AgencyId = corder.Agent_Id, Id = corder.Id, Province = corder.Province, City = corder.City, MobileSP = corder.MobileSP, MobileNumber = corder.Phone_number, OutId = "", ResourceId = 0, ResourceTaocanId = corder.Resource_taocan_id, RouteId = corder.RuoteId, CreatedTime = corder.Created_time };
                    ChargeBridge cb = new ChargeBridge();
                    result = cb.Charge(order);                   
                }
            }

            return result;
        }

        public ChargeOrder GenerateOrder(ChargeOrder order)
        {            
            if(order==null)
            {
                throw new KMBitException("订单对象不能为NULL");
            }
            if(string.IsNullOrEmpty(order.MobileNumber))
            {
                throw new KMBitException("充值的手机号码不能为空");
            }
            chargebitEntities db = null;
            try           
            {
                db = new chargebitEntities();
                Marketing_Orders mOrder = null;
                Marketing_Activity_Taocan mTaocan = null;

                if (order.IsMarket && order.MarketOrderId>0)
                {   
                    //if (string.IsNullOrEmpty(order.MacAddress))
                    //{
                    //    throw new KMBitException("活动充值时必须获取终端的MAC地址");
                    //}
                    mOrder = (from o in db.Marketing_Orders where o.Id == order.MarketOrderId select o).FirstOrDefault<Marketing_Orders>();
                    if (mOrder == null)
                    {
                        throw new KMBitException(string.Format("编号为{0}的活动充值记录不存在", order.MarketOrderId));
                    }
                    if(mOrder.Used || mOrder.Sent)
                    {
                        throw new KMBitException("已经只用过，不能重复使用");
                    }
                    mTaocan = (from m in db.Marketing_Activity_Taocan where m.Id == mOrder.ActivityTaocanId select m).FirstOrDefault<Marketing_Activity_Taocan>();
                    if (mTaocan == null)
                    {
                        throw new KMBitException(string.Format("编号为{0}的活动充值记录的套餐信息不存在", order.MarketOrderId));
                    }
                    //check if the device or the number is already charged
                    int count = (from mo in db.Marketing_Orders where mo.PhoneNumber == order.MobileNumber && mo.ActivityId == mTaocan.ActivityId select mo.Id).Count();
                    if (count > 0)
                    {
                        throw new KMBitException("同一次营销活动每个手机每个号码只能扫描使用一次");
                    }

                    order.ResourceId = mTaocan.ResourceId;
                    order.ResourceTaocanId = mTaocan.ResourceTaocanId;
                    order.RouteId = mTaocan.RouteId;
                    if (order.AgencyId == 0)
                    {
                        order.AgencyId = (from a in db.Marketing_Activities
                                          where a.Id == mTaocan.ActivityId
                                          select a.AgentId
                                          ).FirstOrDefault<int>();
                    }
                    
                }
                Resource_taocan taocan = null;
                if(order.ResourceTaocanId<=0 && order.RouteId>0)
                {
                    var query = from ta in db.Agent_route 
                                join tc in db.Resource_taocan on ta.Resource_taocan_id equals tc.Id
                                where ta.Id == order.RouteId
                                select tc;
                    taocan = query.FirstOrDefault<Resource_taocan>();                    
                }else if(order.ResourceTaocanId>0)
                {
                    taocan = (from tc in db.Resource_taocan where tc.Id == order.ResourceTaocanId select tc).FirstOrDefault<Resource_taocan>();
                }                
                if (!taocan.Enabled)
                {
                    throw new KMBitException(ChargeConstant.RESOURCE_TAOCAN_DISABLED);
                }
                if(taocan==null)
                {
                    throw new KMBitException("套餐信息不存在，不能充值");
                }
                order.ResourceTaocanId = taocan.Id;
                KMBit.DAL.Resource resource = (from ri in db.Resource
                                               join tr in db.Resource_taocan on ri.Id equals tr.Resource_id
                                               where tr.Id == order.ResourceTaocanId
                                               select ri).FirstOrDefault<Resource>();

                if (resource == null)
                {
                    throw new KMBitException("落地资源不存在");
                }
                if (!resource.Enabled)
                {
                    throw new KMBitException(ChargeConstant.RESOURCE_DISABLED);
                }
                Agent_route route = null;
                if (order.AgencyId>0 && order.RouteId>0)
                {
                    route = (from r in db.Agent_route where r.Id== order.RouteId select r).FirstOrDefault<Agent_route>();
                    if(route==null)
                    {
                        throw new KMBitException("代理商路由不存在");
                    }

                    if(route.User_id!=order.AgencyId)
                    {
                        throw new KMBitException("当前代理商没有此路由");
                    }
                }
                if(!route.Enabled)
                {
                    throw new KMBitException(string.Format("编号为{0}代理商路由已被管理员停用",route.Id));
                }

                Charge_Order history = new Charge_Order();                
                history.Agent_Id = order.AgencyId;               
                history.Completed_Time = 0;
                history.Created_time = order.CreatedTime > 0 ? order.CreatedTime : DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                history.Operate_User = order.OperateUserId;
                history.Out_Order_Id = "";
                history.Payed = order.Payed;
                history.Phone_number = order.MobileNumber;
                history.MobileSP = order.MobileSP;
                history.Province = order.Province;
                history.City = order.City;
                history.Resource_id = resource.Id;
                history.Resource_taocan_id = order.ResourceTaocanId;
                history.RuoteId = order.RouteId;
                history.Status = 11;

                history.Platform_Sale_Price = taocan.Sale_price;
                if (taocan.EnableDiscount)
                {
                    history.Platform_Cost_Price = taocan.Purchase_price * taocan.Resource_Discount;
                }else
                {
                    history.Platform_Cost_Price = taocan.Purchase_price;
                }

                if (order.IsMarket && order.MarketOrderId>0)
                {
                    //营销充值
                    history.Charge_type = 1;
                    history.MarketOrderId = order.MarketOrderId;                   
                    history.Sale_price = mTaocan.Price;
                    history.Purchase_price = history.Platform_Sale_Price * route.Discount;
                    history.Payed = true;
                }
                else
                {
                    if (order.AgencyId > 0)
                    {
                        //代理商充值
                        history.Charge_type = 1;                      
                        history.Purchase_price = taocan.Sale_price * route.Discount;
                        history.Sale_price = route.Sale_price;
                        history.Revenue = history.Purchase_price - history.Platform_Cost_Price;
                    }
                    else if (order.OperateUserId > 0)
                    {
                        //管理员后台充值
                        history.Charge_type = 2;                       
                        history.Purchase_price = taocan.Sale_price;
                        history.Sale_price = taocan.Sale_price;
                        history.Revenue = history.Platform_Sale_Price - history.Platform_Cost_Price;
                    }
                    else
                    {
                        //前台用户直冲
                        history.Charge_type = 0;                      
                        history.Purchase_price = taocan.Sale_price;
                        history.Sale_price = taocan.Sale_price;
                        history.Revenue = history.Platform_Sale_Price - history.Platform_Cost_Price;
                    }
                }                
                
                db.Charge_Order.Add(history);
                db.SaveChanges();
                order.Id = history.Id;

                //Create Payment record for direct charge
                if(history.Charge_type==0)
                {
                    Payment_history payment = new Payment_history() { PaymentAccount="", Amount=taocan.Sale_price, ChargeOrderId=history.Id, CreatedTime=DateTimeUtil.ConvertDateTimeToInt(DateTime.Now), PayType=0, Tranfser_Type=1 };
                    db.Payment_history.Add(payment);
                    db.SaveChanges();
                    order.PaymentId = payment.Id;
                }

            }catch(Exception ex)
            {
                throw new KMBitException(ex.Message);
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }

            return order;
        }

        public bool UpdatePaymentStatus(string mobile,int orderId, bool payed)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Charge_Order cOrder = (from o in db.Charge_Order where o.Id==orderId && o.Phone_number==mobile select o).FirstOrDefault<Charge_Order>();
                if(cOrder!=null)
                {
                    cOrder.Payed = payed;
                }else
                {
                    throw new KMBitException(string.Format("编号为{0}的充值记录不存在",orderId));
                }

                db.SaveChanges();
                result = true;
            }

            return result;
        }

        public List<ReportTemplate> SearchAgentReport(int agencyId,long startTime, long endTime)
        {
            List<ReportTemplate> reportList = new List<ReportTemplate>();
            int total = 0;
            if(agencyId==0)
            {
                agencyId = CurrentLoginUser.User.Id;
            }
            List<BOrder> orders = FindOrders(0, agencyId, 0, 0, 0, null, null, null, startTime, endTime, out total);
            var rp = from order in orders
                     group order by order.AgentRouteId into gOrders
                     orderby gOrders.Key
                     select new
                     {
                         ResourceId = gOrders.Key,
                         ResourceName = gOrders.FirstOrDefault<BOrder>().TaocanName,
                         SalesAmount = gOrders.Sum(o => o.SalePrice),
                         CostAmount = gOrders.Sum(o => o.PurchasePrice),
                         Revenue = gOrders.Sum(o => o.SalePrice)- gOrders.Sum(o => o.PurchasePrice)
                     };

            foreach (var rpt in rp)
            {
                ReportTemplate temp = new ReportTemplate() { CostAmount = rpt.CostAmount, Id = rpt.ResourceId, Name = rpt.ResourceName, Revenue = rpt.Revenue, SalesAmount = rpt.SalesAmount };
                reportList.Add(temp);
            }

            return reportList;
        }

        public ChartReport SearchResourceAndAgentReport(long startTime, long endTime)
        {
            ChartReport report = new ChartReport();
            int total = 0;
            List<BOrder> orders = FindOrders(0, 0, 0, 0, 0, null, null, null, startTime, endTime, out total);
            List<ReportTemplate> resourceReport = new List<ReportTemplate>();
            List<ReportTemplate> agentReport = new List<ReportTemplate>();
            var rp = from order in orders
                     group order by order.ResourceId into gOrders
                     orderby gOrders.Key
                     select new
                     {
                         ResourceId = gOrders.Key,
                         ResourceName = gOrders.FirstOrDefault<BOrder>().ReseouceName,
                         SalesAmount = gOrders.Sum(o => o.PurchasePrice),
                         CostAmount = gOrders.Sum(o => o.PlatformCostPrice),
                         Revenue = gOrders.Sum(o => o.Revenue)
                     };
            foreach (var rpt in rp)
            {
                ReportTemplate temp = new ReportTemplate() { CostAmount = rpt.CostAmount, Id = rpt.ResourceId, Name = rpt.ResourceName, Revenue = rpt.Revenue, SalesAmount = rpt.SalesAmount };
                resourceReport.Add(temp);
            }
            report.ResourceReport = resourceReport;
            var rp2 = from order in orders
                      where order.AgentId > 0 && order.Operator==0
                      group order by order.AgentId into gOrders
                      orderby gOrders.Key
                      select new
                      {
                          AgentId = gOrders.Key,
                          AgentName = gOrders.FirstOrDefault<BOrder>().AgentName,
                          SalesAmount = gOrders.Sum(o => o.PurchasePrice),
                          CostAmount = gOrders.Sum(o => o.PlatformCostPrice),
                          Revenue = gOrders.Sum(o => o.Revenue)
                      };

            foreach (var rpt in rp2)
            {
                ReportTemplate temp = new ReportTemplate() { CostAmount = rpt.CostAmount, Id = rpt.AgentId, Name = rpt.AgentName, Revenue = rpt.Revenue, SalesAmount = rpt.SalesAmount };
                agentReport.Add(temp);
            }

            var rp3 = from order in orders
                      where order.AgentId == 0
                      group order by order.ChargeType into gOrders
                      orderby gOrders.Key
                      select new
                      {
                          Type = gOrders.Key,
                          Name = gOrders.FirstOrDefault<BOrder>().Operator==0?"前台直冲":"后台直冲",
                          SalesAmount = gOrders.Sum(o => o.PurchasePrice),
                          CostAmount = gOrders.Sum(o => o.PlatformCostPrice),
                          Revenue = gOrders.Sum(o => o.Revenue)
                      };

            foreach (var rpt in rp3)
            {
                ReportTemplate temp = new ReportTemplate() { CostAmount = rpt.CostAmount, Id = rpt.Type, Name = rpt.Name, Revenue = rpt.Revenue, SalesAmount = rpt.SalesAmount };
                agentReport.Add(temp);
            }
            report.UserReport = agentReport;
            return report;
        }

        public List<BOrder> FindOrders(int orderId,int agencyId, int resourceId, int resourceTaocanId, int routeId, string spName, string mobilePhone, int[] status, long startTime, long endTime,out int total, int pageSize = 50, int page = 1, bool paging = false)
        {
            total = 0;
            List<BOrder> orders = new List<BOrder>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from o in db.Charge_Order
                            join r in db.Resource on o.Resource_id equals r.Id
                            join t in db.Resource_taocan on o.Resource_taocan_id equals t.Id
                            join route in db.Agent_route on o.RuoteId equals route.Id into lroute
                            from llroute in lroute.DefaultIfEmpty()
                            join agency in db.Users on o.Agent_Id equals agency.Id into lagency
                            from llagency in lagency.DefaultIfEmpty()
                            join opr in db.Users on o.Operate_User equals opr.Id into lopr
                            from llopr in lopr.DefaultIfEmpty()
                            join tcc in db.Taocan on t.Taocan_id equals tcc.Id into ltcc
                            from lltcc in ltcc.DefaultIfEmpty()
                            select new BOrder
                            {
                                AgentName = llagency != null ? llagency.Name : null,
                                AgentId = o.Agent_Id,
                                AgentRouteId = o.RuoteId,
                                CompletedTime = o.Completed_Time,
                                CreatedTime = o.Created_time,
                                Id = o.Id,
                                Message = o.Message,
                                MobilePhone = o.Phone_number,
                                MobileSP = o.MobileSP,
                                Operator = o.Operate_User,
                                OperatorName = llopr != null ? llopr.Name : null,
                                Payed = o.Payed,
                                PlatformCostPrice = o.Platform_Cost_Price,
                                PlatformSalePrice = o.Platform_Sale_Price,
                                ProceedTime = o.Process_time,
                                PurchasePrice = o.Purchase_price,
                                SalePrice = o.Sale_price,
                                Quantity = t.Quantity,
                                ResourceId = o.Resource_id,
                                ReseouceName = r.Name,
                                ResourceTaocanId = o.Resource_taocan_id,
                                Status = o.Status,                                
                                TaocanName = lltcc!=null? lltcc.Name:"",
                                Refound=o.Refound,
                                Revenue=o.Revenue,
                                ChargeType= o.Charge_type
                            };

                if(orderId>0)
                {
                    query = query.Where(o => o.Id == orderId);
                }
                if(agencyId>0)
                {
                    query = query.Where(o=>o.AgentId==agencyId);
                }
                if(resourceId>0)
                {
                    query = query.Where(o => o.ResourceId == resourceId);
                }
                if (resourceTaocanId > 0)
                {
                    query = query.Where(o => o.ResourceTaocanId == resourceTaocanId);
                }
                if (routeId > 0)
                {
                    query = query.Where(o => o.AgentRouteId == routeId);
                }
                if(!string.IsNullOrEmpty(spName))
                {
                    query = query.Where(o=>o.MobileSP.Contains(spName));
                }
                if(!string.IsNullOrEmpty(mobilePhone))
                {
                    query = query.Where(o => o.MobilePhone==mobilePhone);
                }
                if(status!=null && status.Length>0)
                {
                    if(status.Length>1 ||(status.Length==1 && status[0]!=0))
                    {
                        query = query.Where(o => status.Contains(o.Status));
                    }                    
                }
                if(startTime>0)
                {
                    query = query.Where(o=>o.CreatedTime>=startTime);
                }
                if (endTime > 0)
                {
                    query = query.Where(o => o.CreatedTime <= endTime);
                }
                query = query.OrderByDescending(o=>o.CreatedTime);
                total = query.Count();
                if(paging)
                {
                    if (pageSize <= 0) { pageSize = 50; }
                    if (page <= 0) { page = 1; }
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                orders = query.ToList<BOrder>();
                List<DictionaryTemplate> statusList = StaticDictionary.GetChargeStatusList();
                List<DictionaryTemplate> chargeTypeList = StaticDictionary.GetChargeTypeList();
                foreach (BOrder o in orders)
                {
                    o.StatusText = (from s in statusList where o.Status == s.Id select s.Value).FirstOrDefault<string>();
                    o.ChargeTypeText = (from s in chargeTypeList where o.ChargeType == s.Id select s.Value).FirstOrDefault<string>();
                }
            }
            return orders;
        }
    }
}
