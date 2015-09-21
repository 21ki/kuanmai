using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.BL
{
    public class BaseManagement
    {
        protected log4net.ILog logger;
        public BUser CurrentLoginUser { get; private set; }

        public BaseManagement(BUser user)
        {
            this.CurrentLoginUser = user;
        }
        public BaseManagement(Users user)
        {
            if(user!=null)
            {
                this.CurrentLoginUser = this.GetUserInfo(user.Id);
            }
        }
        public BaseManagement(int userId)
        {
            this.CurrentLoginUser = this.GetUserInfo(userId);
        }

        protected BUser GetUserInfo(int userId)
        {
            BUser user = new BUser();
            using (chargebitEntities db = new chargebitEntities())
            {
                user.User = (from u in db.Users where u.Id == userId select u).FirstOrDefault<Users>();

                Admin_Users au = (from ausr in db.Admin_Users where ausr.User_Id == userId select ausr).FirstOrDefault<Admin_Users>();
                if (au != null)
                {
                    user.IsSuperAdmin = au.IsSuperAdmin;
                    user.IsWebMaster = au.IsWebMaster;
                }

                user.Permission = PermissionManagement.GetUserPermissions(userId);
            }
            return user;
        }
    }
}
