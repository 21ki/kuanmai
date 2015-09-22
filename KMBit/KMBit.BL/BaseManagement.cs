using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL.Admin;
namespace KMBit.BL
{
    public class BaseManagement
    {
        protected log4net.ILog logger;
        public BUser CurrentLoginUser { get; private set; }

        public BaseManagement(BUser user)
        {
            this.CurrentLoginUser = user;
            this.InitializeLoggger();
        }
        public BaseManagement(Users user)
        {
            if(user!=null)
            {
                this.CurrentLoginUser = this.GetUserInfo(user.Id);
            }
            this.InitializeLoggger();
        }
        public BaseManagement(int userId)
        {
            this.CurrentLoginUser = this.GetUserInfo(userId);
            this.InitializeLoggger();
        }

        protected virtual void InitializeLoggger()
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(this.GetType());
            }
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
                if (!user.IsSuperAdmin)
                {
                    user.Permission = PermissionManagement.GetUserPermissions(userId);
                }else
                {
                    user.Permission = new Permissions();
                    System.Reflection.FieldInfo[] fields = typeof(Permissions).GetFields();
                    foreach(System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(user.Permission, 1);
                    }
                }                
            }
            return user;
        }
    }
}
