using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.BL.Models;
using KM.JXC.DBA;
namespace KM.JXC.BL
{
    public class UserManagement
    {
        public BUser GetUserInfo(int user_id)
        {
            BUser user = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                user = (from u in db.User
                        where u.User_ID == user_id
                        select new BUser
                            {
                                ID=u.User_ID,
                                Name=u.Name,
                                Mall_ID=u.Mall_ID,
                                Mall_Name=u.Mall_Name,
                                IsSystemUser=u.IsSystemUser,
                                NickName=u.NickName
                            }).FirstOrDefault<BUser>();
            }
            return user;
        }
    }
}
