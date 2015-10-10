using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.DAL;
namespace KMBit.BL.Agent
{
    public class AgentManagement : BaseManagement
    {
        public AgentManagement(int userId):base(userId)
        {

        }
        public AgentManagement(BUser user) : base(user)
        {

        }    

        public List<BAgentRoute> FindTaocans(int routeId)
        {
            List<BAgentRoute> taocans = new List<BAgentRoute>();
            AgentAdminMenagement agentAdminMgt = new AgentAdminMenagement(this.CurrentLoginUser);
            int total = 0;
            taocans = agentAdminMgt.FindRoutes(routeId, CurrentLoginUser.User.Id, 0, 0, out total);
            return taocans;
        }

        public bool UpdateTaocanPrice(int routeId, float price,bool enable)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Agent_route route = (from r in db.Agent_route where r.Id == routeId select r).FirstOrDefault<Agent_route>();
                if (route == null)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐不存在", routeId.ToString()));
                }

                if(route.User_id!=CurrentLoginUser.User.Id)
                {
                    throw new KMBitException(string.Format("编号为{0}的套餐不属于你，不能修改套餐信息", routeId.ToString()));
                }

                route.Sale_price = price;
                route.Enabled = enable;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
