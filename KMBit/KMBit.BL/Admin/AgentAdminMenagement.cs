using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL.Admin
{
    public class AgentAdminMenagement:BaseManagement
    {
        public AgentAdminMenagement(int userId):base(userId)
        {
           
        }
        public AgentAdminMenagement(BUser user) : base(user)
        {

        }

        public bool CreateAgentRuote()
        {
            bool ret = false;
            return ret;
        }
        public async Task<bool> CreateAgency(Users dbUser)
        {
            if(CurrentLoginUser.Permission.CREATE_USER==0)
            {
                throw new KMBitException("没有权限创建代理商");
            }
            int total = 0;
            List<BUser> existedUsers = FindAgencies(0, dbUser.Email, null, 0, 0,out total);
            if(existedUsers.Count>0)
            {
                throw new KMBitException(string.Format("Email:{0}已经注册过"));
            }

            bool ret = false;
            ApplicationUserManager manager = new ApplicationUserManager(new ApplicationUserStore(new chargebitEntities()));
            ApplicationUser appUser = new ApplicationUser();
            appUser.Address = dbUser.Address;
            appUser.AccessFailedCount = 0;
            appUser.City_id = dbUser.City_id;
            appUser.CreatedBy = dbUser.CreatedBy;
            appUser.Credit_amount = dbUser.Credit_amount;
            appUser.Description = dbUser.Description;
            appUser.Email = dbUser.Email;
            appUser.UserName = dbUser.Email;
            appUser.Name = dbUser.Name;
            appUser.PasswordHash = dbUser.PasswordHash;
            appUser.Pay_type = dbUser.Pay_type;
            appUser.PhoneNumber = dbUser.PhoneNumber;
            appUser.Province_id = dbUser.Province_id;
            appUser.Regtime = dbUser.Regtime;
            appUser.Enabled = dbUser.Enabled;
            if(appUser.Regtime==0)
            {
                appUser.Regtime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            }
            appUser.Type = dbUser.Type;
            appUser.Update_time = appUser.Regtime;
            appUser.UserName = dbUser.Email;
            var result = await manager.CreateAsync(appUser, dbUser.PasswordHash);
            if(result.Succeeded)
            {
                ret = true;
            }
            return ret;
        }

        public bool UpdateAgency(Users dbUser)
        {
            if(CurrentLoginUser.Permission.UPDATE_USER==0)
            {
                throw new KMBitException("没有权限更新用户信息");
            }
            if(dbUser.Id<=0)
            {
                throw new KMBitException("用户编号不能为空");
            }
            bool ret = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Users user = (from u in db.Users where u.Id==dbUser.Id select u).FirstOrDefault<Users>();
                if(user==null)
                {
                    throw new KMBitException(string.Format("用户编号为{0}的用户不存在",dbUser.Id.ToString()));
                }

                user.Address = dbUser.Address;
                user.City_id = dbUser.City_id;
                user.Province_id = dbUser.Province_id;
                user.PhoneNumber = dbUser.PhoneNumber;
                user.Pay_type = dbUser.Pay_type;
                user.Description = dbUser.Description;
                user.Credit_amount = dbUser.Credit_amount;
                //user.Remaining_amount = dbUser.Remaining_amount;
                user.Update_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public List<BUser> FindAgencies(int userId, string email, string name, int provinceId, int cityId, out int total,int page = 1, int pageSize = 30)
        {
            total = 0;
            List<BUser> agencies = new List<BUser>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from u in db.Users
                            join paytype in db.PayType on u.Pay_type equals paytype.Id
                            join usertype in db.User_type on u.Type equals usertype.Id
                            join cu in db.Users on u.CreatedBy equals cu.Id into lcu
                            from llcu in lcu.DefaultIfEmpty()
                            join city in db.Area on u.City_id equals city.Id into lcity
                            from llcity in lcity.DefaultIfEmpty()
                            join province in db.Area on u.Province_id equals province.Id into lprovince
                            from llprovince in lcity.DefaultIfEmpty()
                            where u.Type != 1
                            select new BUser
                            {
                                User = u,
                                CreatedBy = llcu,
                                City = llcity,
                                Province =llprovince,
                                PayType=paytype,
                                UserType=usertype
                            };

                if(userId>0)
                {
                    query = query.Where(q => q.User.Id == userId);
                }
                if(!string.IsNullOrEmpty(email))
                {
                    query = query.Where(q=>q.User.Email==email);
                }
                if(cityId>0)
                {
                    query = query.Where(q=>q.City.Id==cityId);
                }
                if (provinceId > 0)
                {
                    query = query.Where(q => q.Province.Id == provinceId);
                }

                total = query.Count();
                page = page <= 0 ? 1 : page;
                query = query.OrderByDescending(u => u.User.Regtime).Skip((page - 1) * pageSize).Take(pageSize);
                agencies = query.ToList<BUser>();
            }
            return agencies;
        }

        public bool UpdateRoutePrice(int route_id, float price)
        {
            bool ret = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Agent_route route = (from r in db.Agent_route where r.Id==route_id select r).FirstOrDefault<Agent_route>();
                if(route==null)
                {
                    throw new KMBitException("此路由套餐不存在");
                }

                route.Sale_price = price;
                db.SaveChanges();
                ret = true;
            }
            return ret;
        }

        public List<BAgentRoute> FindRoutes(int agencyId, int resourceId, int resourceTaocanId, out int total, int page = 1, int pageSize = 30, bool paging = false)
        {
            total = 0;
            List<BAgentRoute> routes = new List<BAgentRoute>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from r in db.Agent_route
                            join u in db.Users on r.CreatedBy equals u.Id
                            join uu in db.Users on r.UpdatedBy equals uu.Id
                            join re in db.Resource on r.Resource_Id equals re.Id
                            join tc in db.Resource_taocan on r.Resource_taocan_id equals tc.Id
                            join city in db.Area on tc.Area_id equals city.Id into lcity
                            from llcity in lcity.DefaultIfEmpty()
                            join sp in db.Sp on tc.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            select new BAgentRoute
                            {
                                Route = r,
                                CreatedBy = u,
                                UpdatedBy = uu,
                                Taocan = new BResourceTaocan
                                {
                                    Taocan = tc,
                                    Resource = new BResource { Resource = re },
                                    City = llcity,
                                    SP = llsp
                                }
                            };

                if(agencyId>0)
                {
                    query = query.Where(o => o.Route.User_id == agencyId);
                }

                if(resourceId>0)
                {
                    query = query.Where(o=>o.Route.Resource_Id==resourceId);
                }
                if (resourceTaocanId > 0)
                {
                    query = query.Where(o => o.Route.Resource_taocan_id == resourceTaocanId);
                }

                total = query.Count();
                if(paging)
                {
                    page = page > 0 ? page : 1;
                }

                query = query.OrderBy(o => o.Route.Id);
                routes = query.Skip((page - 1) * pageSize).Take(pageSize).ToList<BAgentRoute>();
            }
            return routes;
        }

        public bool CreateRoute(Agent_route route)
        {
            bool ret = false;

            return ret;
        }

        public List<BAgentRoute> GetRoutes()
        {
            List<BAgentRoute> routes=null;
            int userId = CurrentLoginUser.User.Id;
            using (chargebitEntities db = new chargebitEntities())
            {
                var tmp = from au in db.Agent_route
                          join rta in db.Resource_taocan on au.Resource_taocan_id equals rta.Id
                          join cu in db.Users on au.CreatedBy equals cu.Id into lcu
                          from llcu in lcu.DefaultIfEmpty()
                          join uu in db.Users on au.UpdatedBy equals uu.Id into luu
                          from lluu in luu.DefaultIfEmpty()
                          join r in db.Resource on rta.Resource_id equals r.Id                         
                          join city in db.Area on rta.Area_id equals city.Id into lcity
                          from llcity in lcity.DefaultIfEmpty()
                          join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                          from llsp in lsp.DefaultIfEmpty()
                          where au.User_id==userId
                          select new BAgentRoute
                          {
                              CreatedBy = llcu,
                              UpdatedBy = lluu,
                              Route = au,
                              Taocan = new BResourceTaocan
                              {
                                  Taocan = rta,                                
                                  City = llcity,
                                  SP = llsp,
                                  Resource = new BResource() { Resource = r }
                              } 
                          };

                routes = tmp.ToList<BAgentRoute>();

            }

            return routes;
        }
    }
}
