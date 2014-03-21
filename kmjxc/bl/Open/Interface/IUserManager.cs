using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Open.Interface
{
    public interface IUserManager
    {
        User GetUser(string mall_id,string mall_name);
        User GetSubUser(string mall_id, string mall_name);
        List<User> GetSubUsers(string user_id);
    }
}
