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

        public ChargeOrder GenerateOrder(ChargeOrder order)
        {            
            if (order.ResourceTaocanId<=0 && order.RouteId<=0)
            {
                throw new KMBitException("充值前请先选择套餐");
            }
            chargebitEntities db = null;
            try           
            {
                db = new chargebitEntities();
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

                if(order.AgencyId>0 && order.RouteId>0)
                {
                    Agent_route route = (from r in db.Agent_route where r.Id== order.RouteId select r).FirstOrDefault<Agent_route>();
                    if(route==null)
                    {
                        throw new KMBitException("代理商路由不存在");
                    }

                    if(route.User_id!=order.AgencyId)
                    {
                        throw new KMBitException("当前代理商没有此路由");
                    }
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
                history.Charge_type = order.AgencyId>0?1:0;
                db.Charge_Order.Add(history);
                db.SaveChanges();
                order.Id = history.Id;
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

        public List<BOrder> FindOrders(int orderId,int agencyId, int resourceId, int resourceTaocanId, int routeId, string spName, string mobilePhone, List<int> status, long startTime, long endTime,out int total, int pageSize = 50, int page = 1, bool paging = false)
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
                            select new BOrder
                            {
                                AgentEmail = llagency != null ? llagency.Email : null,
                                AgentId = o.Agent_Id,
                                AgentRouteId = o.RuoteId,
                                CompletedTime = o.Completed_Time,
                                CreatedTime = o.Created_time,
                                Id = o.Id,
                                Message = o.Message,
                                MobilePhone = o.Phone_number,
                                MobileSP = o.MobileSP,
                                Operator = o.Operate_User,
                                OperatorEmail = llopr != null ? llopr.Email : null,
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
                                TaocanName = "",
                                Refound=o.Refound,
                                Revenue=o.Revenue
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
                if(status!=null && status.Count>0)
                {
                    query = query.Where(o=>status.ToArray<int>().Contains(o.Status));
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
                foreach(BOrder o in orders)
                {
                    
                    switch(o.Status)
                    {
                        case 0:
                            o.StatusText = "等待处理";
                            break;
                        case 1:
                            o.StatusText = "正在充值";
                            break;
                        case 2:
                            o.StatusText = "充值成功";
                            break;
                        case 3:
                            o.StatusText = "充值失败";
                            break;
                        default:
                            break;
                    }
                }
            }
            return orders;
        }
    }
}
