using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL
{
    public class AgentManagement : BaseManagement
    {
        public AgentManagement(int userId):base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(AgentManagement));
            }
        }

        public bool CreateNewAgent(Users user)
        {
            bool ret = false;
            //var result = await UserManager.CreateAsync(user, model.Password);
            return ret;
        }
    }
}
