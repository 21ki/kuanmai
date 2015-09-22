using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL.Agent
{
    public class AgentMenagement:BaseManagement
    {
        public AgentMenagement(int userId):base(userId)
        {
           
        }
        public AgentMenagement(BUser user) : base(user)
        {

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
