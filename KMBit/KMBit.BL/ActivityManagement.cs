using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.Util;
using KMBit.DAL;
using KMBit.BL.Agent;
namespace KMBit.BL
{
    public class ActivityManagement:BaseManagement
    {
        public ActivityManagement(int userId):base(userId)
        { }

        public ActivityManagement(string email):base(email)
        { }

        public ActivityManagement(BUser user):base(user)
        {

        }

        public Marketing_Activities CreateNewActivity(Marketing_Activities activity,bool createOrders)
        {            
            if(activity==null)
            {
                throw new KMBitException("参数输入错误");
            }
            if(activity.AgentId!=CurrentLoginUser.User.Id)
            {
                throw new KMBitException("代理商用户不能为别的代理商用户的客户创建活动");
            }
            if (activity.RuoteId <= 0)
            {
                throw new KMBitException("套餐不能为空");
            }
            using (chargebitEntities db = new chargebitEntities())
            {
                Customer customer = (from c in db.Customer where c.Id == activity.CustomerId select c).FirstOrDefault<Customer>();
                if(customer==null)
                {
                    throw new KMBitException(string.Format("编号为:{0}的客户不存在",activity.CustomerId));
                }
                if (customer.RemainingAmount < activity.UnitPrice * activity.Quantity)
                {
                    throw new KMBitException(string.Format("客户 {0} 余额不足", customer.Name));
                }
                activity.CreatedTime = activity.CreatedTime>0? activity.CreatedTime: DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                Agent_route route = (from ro in db.Agent_route where ro.Id== activity.RuoteId select ro).FirstOrDefault<Agent_route>();
                if(route==null)
                {
                    throw new KMBitException("套餐不存在");
                }
                activity.ResourceId = route.Resource_Id;
                activity.ResourceTaocanId = route.Resource_taocan_id;
                db.Marketing_Activities.Add(activity);
                db.SaveChanges();
                if(activity.Id>0 && createOrders)
                {
                    CreateActivityOrders(activity);
                }
            }

            return activity;
        }

        public bool CreateActivityOrders(int activityId)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Marketing_Activities activity = (from a in db.Marketing_Activities where a.Id == activityId select a).FirstOrDefault<Marketing_Activities>();
                if (activity != null)
                {
                    result = CreateActivityOrders(activity);
                }
            }
            return result;
        }

        public bool CreateActivityOrders(Marketing_Activities activity)
        {
            bool result = false;         

            using (chargebitEntities db = new chargebitEntities())
            {
                //db.Configuration.AutoDetectChangesEnabled = false;
                Customer customer = (from cus in db.Customer where cus.Id==activity.CustomerId select cus).FirstOrDefault<Customer>();
                Agent_route route = (from r in db.Agent_route where r.Id==activity.RuoteId select r).FirstOrDefault<Agent_route>();
                Resource_taocan taocan = (from r in db.Resource_taocan where r.Id == activity.ResourceTaocanId select r).FirstOrDefault<Resource_taocan>();
                if (route == null)
                {
                    throw new KMBitException(string.Format("编号为:{0}的路由不存在", activity.RuoteId));
                }
                if (customer == null)
                {
                    throw new KMBitException(string.Format("编号为:{0}的客户不存在", activity.CustomerId));
                }
                if (taocan == null)
                {
                    throw new KMBitException(string.Format("编号为:{0}的套餐不存在", activity.ResourceTaocanId));
                }
                if(customer.RemainingAmount<activity.UnitPrice*activity.Quantity)
                {
                    throw new KMBitException(string.Format("客户 {0} 余额不足", customer.Name));
                }
                //Create marketing orders
                for (int i = 0; i < activity.Quantity; i++)
                {
                    Marketing_Orders order = new Marketing_Orders()
                    {
                        ActivityId = activity.Id,
                        AgentPurchasePrice = route.Discount * taocan.Sale_price,
                        AgentSalePrice = activity.UnitPrice,
                        CreatedTime = activity.CreatedTime,
                        ExpiredTime = activity.ExpiredTime,
                        MacAddress = "",
                        PhoneNumber = "",
                        Used = false,
                        UsedTime = 0,
                        PlatformPurchasePrice = taocan.Purchase_price,
                        PlatformSalePrice = taocan.Sale_price,
                        StartTime = activity.StartedTime
                    };

                    db.Marketing_Orders.Add(order);
                }

                db.SaveChanges();
                customer.RemainingAmount -= activity.UnitPrice * activity.Quantity;
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        public List<BActivity> FindActivities(int agentId,int customerId,out int total,bool paging,int page,int pageSize)
        {
            total = 0;
            List<BActivity> activities = new List<BActivity>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from av in db.Marketing_Activities
                            join cs in db.Customer on av.CustomerId equals cs.Id
                            join ag in db.Users on av.AgentId equals ag.Id into lag
                            join ruote in db.Agent_route on av.RuoteId equals ruote.Id
                            join resouce in db.Resource on av.ResourceId equals resouce.Id
                            join resouceTaocan in db.Resource_taocan on av.ResourceTaocanId equals resouceTaocan.Id                           
                            from llag in lag.DefaultIfEmpty()                                             
                            select new BActivity
                            {
                                 Activity=av,
                                 Customer=new BCustomer()
                                 {
                                     Name= cs.Name,
                                     Id=av.CustomerId
                                 },
                                 Ruote=new BAgentRoute() { Route=ruote, Taocan=new BResourceTaocan() { Taocan=resouceTaocan } },
                                 UsedCount=(from ao in db.Marketing_Orders where av.Id == ao.ActivityId && ao.Used==true select ao.Id).Count()
                            };

                if(agentId>0)
                {
                    query = query.Where(o=>o.Activity.AgentId==agentId);
                }
                if(customerId>0)
                {
                    query = query.Where(o => o.Activity.CustomerId == customerId);
                }
                query = query.OrderBy(o=>o.Customer.Name).OrderByDescending(o=>o.Activity.CreatedTime);
                if(paging)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 20;
                    query = query.Skip((page-1)*pageSize).Take(pageSize);
                }
                activities = query.ToList<BActivity>();
            }
            return activities;
        }
    }
}
