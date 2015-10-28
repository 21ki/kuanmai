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

        public Marketing_Activities CreateNewActivity(Marketing_Activities activity)
        {            
            if(activity==null)
            {
                throw new KMBitException("参数输入错误");
            }
            if(activity.AgentId!=CurrentLoginUser.User.Id)
            {
                throw new KMBitException("代理商用户不能为别的代理商用户的客户创建活动");
            }           
            using (chargebitEntities db = new chargebitEntities())
            {
                Customer customer = (from c in db.Customer where c.Id == activity.CustomerId select c).FirstOrDefault<Customer>();
                if(customer==null)
                {
                    throw new KMBitException(string.Format("编号为:{0}的客户不存在",activity.CustomerId));
                }
               
                db.Marketing_Activities.Add(activity);
                db.SaveChanges();               
            }

            return activity;
        }      

        public List<BActivity> FindActivities(int activityId,int agentId,int customerId,out int total,bool paging=false,int page=1,int pageSize=30)
        {
            total = 0;
            List<BActivity> activities = new List<BActivity>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from av in db.Marketing_Activities
                            join cs in db.Customer on av.CustomerId equals cs.Id
                            join ag in db.Users on av.AgentId equals ag.Id into lag                     
                            select new BActivity
                            {
                                 Activity=av,
                                 Customer=new BCustomer()
                                 {
                                     Name= cs.Name,
                                     Id=av.CustomerId
                                 }
                            };
                if(activityId>0)
                {
                    query = query.Where(o => o.Activity.Id == activityId);
                }
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

        public bool CreateActivityTaocan(Marketing_Activity_Taocan taocan)
        {
            bool result = false;
            if(taocan==null)
            {
                throw new KMBitException("输入不正确");
            }
            if(taocan.ActivityId<=0)
            {
                throw new KMBitException("活动信息丢失");
            }
            if(taocan.RouteId<=0)
            {
                throw new KMBitException("套餐必须选择");
            }
            if(taocan.Quantity<=0)
            {
                throw new KMBitException("数量不能小于0");
            }
            if (taocan.Price <= 0)
            {
                throw new KMBitException("价格不能小于0");
            }
            chargebitEntities db = new chargebitEntities();            
            try
            {
                Marketing_Activities activity = (from a in db.Marketing_Activities where a.Id == taocan.ActivityId select a).FirstOrDefault<Marketing_Activities>();
                if(activity==null)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不存在",taocan.ActivityId));
                }
                Customer customer = (from c in db.Customer where c.Id == activity.CustomerId select c).FirstOrDefault<Customer>();
                if(customer==null)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不属于任何客户", taocan.ActivityId));
                }
                if(customer.AgentId!=CurrentLoginUser.User.Id)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不属您的客户", taocan.ActivityId));
                }

                List<BAgentRoute> routes = new List<BAgentRoute>();
                var query = from r in db.Agent_route
                            join u in db.Users on r.CreatedBy equals u.Id
                            join uu in db.Users on r.UpdatedBy equals uu.Id into luu
                            from lluu in luu.DefaultIfEmpty()
                            join re in db.Resource on r.Resource_Id equals re.Id
                            join tc in db.Resource_taocan on r.Resource_taocan_id equals tc.Id
                            join city in db.Area on tc.Area_id equals city.Id into lcity
                            from llcity in lcity.DefaultIfEmpty()
                            join sp in db.Sp on tc.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            join tcc in db.Taocan on tc.Taocan_id equals tcc.Id into ltc
                            from lltc in ltc.DefaultIfEmpty()
                            where r.Id==taocan.RouteId
                            select new BAgentRoute
                            {
                                Route = r,
                                CreatedBy = u,
                                UpdatedBy = lluu,
                                Taocan = new BResourceTaocan
                                {
                                    Taocan = tc,
                                    Resource = new BResource { Resource = re },
                                    Province = llcity,
                                    SP = llsp,
                                    Taocan2 = lltc
                                }
                            };
                routes = query.ToList<BAgentRoute>();
                if(routes.Count==0)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐不存在", taocan.RouteId));
                }
                BAgentRoute route = routes[0];
                if(!route.Route.Enabled)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐已经被管理员停用", taocan.RouteId));
                }
                if(!route.Taocan.Taocan.Enabled)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐的落地套餐被管理员停用", taocan.RouteId));
                }
                if(!route.Taocan.Resource.Resource.Enabled)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐的落地被管理员停用", taocan.RouteId));
                }
                taocan.CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                taocan.ResourceId = route.Taocan.Resource.Resource.Id;
                taocan.ResourceTaocanId = route.Taocan.Taocan.Id;
                taocan.Generated = true;
                db.Marketing_Activity_Taocan.Add(taocan);
                db.SaveChanges();
                if(taocan.Id>0)
                {
                    for(int i=0;i<taocan.Quantity;i++)
                    {
                        Marketing_Orders order = new Marketing_Orders()
                        {
                            ActivityId = taocan.ActivityId,
                            ActivityTaocanId = taocan.Id,
                            AgentPurchasePrice = 0,
                            AgentSalePrice = 0,
                            CreatedTime = taocan.CreatedTime,
                            ExpiredTime = activity.ExpiredTime,
                            PlatformPurchasePrice = 0,
                            PlatformSalePrice = route.Taocan.Taocan.Sale_price,
                            Sent = false,
                            Used = false,
                            StartTime = activity.StartedTime,
                            UsedTime = 0
                        };

                        if(route.Taocan.Taocan.EnableDiscount)
                        {
                            order.PlatformPurchasePrice = route.Taocan.Taocan.Purchase_price * route.Taocan.Taocan.Resource_Discount;
                        }

                        order.AgentPurchasePrice = order.PlatformSalePrice * route.Route.Discount;
                        order.AgentSalePrice = taocan.Price;

                        db.Marketing_Orders.Add(order);
                    }

                    db.SaveChanges();
                    result = true;
                }
            }catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                
                if (db!=null)
                {
                    db.Dispose();
                }
            }
            return result;
        }

        public List<BActivityTaocan> FindActivityTaocans(int actityId,int customerId, int agentId)
        {
            List<BActivityTaocan> taocans = new List<BActivityTaocan>();
            using (chargebitEntities db = new chargebitEntities())
            {
                Marketing_Activities activity = (from a in db.Marketing_Activities where a.Id == actityId select a).FirstOrDefault<Marketing_Activities>();
                if (activity == null)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不存在",actityId));
                }
                if(agentId==0)
                {
                    agentId = CurrentLoginUser.User.Id;
                }
                if(agentId!=activity.AgentId)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不是您客户的活动", actityId));
                }
                if(activity.CustomerId!=customerId)
                {
                    throw new KMBitException(string.Format("编号为{0}的活动不是编号为{1}的客户的", actityId,customerId));
                }
                var query = from at in db.Marketing_Activity_Taocan
                            join route in db.Agent_route on at.RouteId equals route.Id
                            join taocan in db.Resource_taocan on route.Resource_taocan_id equals taocan.Id
                            join taocan2 in db.Taocan on taocan.Taocan_id equals taocan2.Id
                            select new BActivityTaocan
                            {
                                ATaocan = at,
                                Route = new BAgentRoute() { Route = route, Taocan = new BResourceTaocan() { Taocan = taocan, Taocan2 = taocan2 } },
                                UsedCount = (from ao in db.Marketing_Orders where ao.ActivityId== activity.Id && ao.Used==true select ao.Id).Count(),
                                SentOutCount = (from ao in db.Marketing_Orders where ao.ActivityId == activity.Id && ao.Sent == true select ao.Id).Count(),
                            };


                taocans = query.ToList<BActivityTaocan>();                
            }
            return taocans;
        }

        public List<BAgentRoute> FindAvailableAgentRoutes(int agentId=0,int activityId=0)
        {
            agentId = agentId > 0 ? agentId : CurrentLoginUser.User.Id;
            AgentManagement agentMgr = new AgentManagement(CurrentLoginUser);
            List<BAgentRoute> routes = agentMgr.FindTaocans(0, true);
            List<BAgentRoute> tmp = new List<BAgentRoute>();
            if(activityId==0)
            {
                tmp = routes;
            }else
            {
                using (chargebitEntities db = new chargebitEntities())
                {
                    List<Marketing_Activity_Taocan> taocans = (from t in db.Marketing_Activity_Taocan where t.ActivityId==activityId select t).ToList<Marketing_Activity_Taocan>();
                    foreach(BAgentRoute route in routes)
                    {
                        Marketing_Activity_Taocan t = (from tc in taocans where tc.RouteId == route.Route.Id select tc).FirstOrDefault<Marketing_Activity_Taocan>();
                        if(t==null)
                        {
                            tmp.Add(route);
                        }
                    }
                }
            }           
            return tmp;
        }
    }
}
