using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Models;
namespace KM.JXC.BL.Open.Interface
{
    public interface IOUserManager
    {
        BUser GetUser(string mall_id, string mall_name);
        BUser GetSubUser(string mall_id, string mall_name);
        BUser GetSubUserFullInfo(string mall_name);
        List<BUser> GetSubUsers(BUser mainUser);
    }
}
