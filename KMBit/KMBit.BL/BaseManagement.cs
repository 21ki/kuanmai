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
            if (user != null)
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
        public BaseManagement(string email)
        {
            this.CurrentLoginUser = this.GetUserInfo(email);
            this.InitializeLoggger();
        }

        protected virtual void InitializeLoggger()
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(this.GetType());
            }
        }

        public BUser GetUserInfo(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }
            BUser user = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                user = new BUser();
                user.User = (from u in db.Users where u.Id == userId select u).FirstOrDefault<Users>();

                Admin_Users au = (from ausr in db.Admin_Users where ausr.User_Id == userId select ausr).FirstOrDefault<Admin_Users>();
                if (au != null)
                {
                    user.IsSuperAdmin = au.IsSuperAdmin;
                    user.IsWebMaster = au.IsWebMaster;
                    user.IsAdmin = true;
                }
                if (!user.IsSuperAdmin)
                {
                    user.Permission = PermissionManagement.GetUserPermissions(userId);
                } else
                {
                    user.Permission = new Permissions();
                    System.Reflection.PropertyInfo[] fields = typeof(Permissions).GetProperties();
                    foreach (System.Reflection.PropertyInfo field in fields)
                    {
                        field.SetValue(user.Permission, true);
                    }
                }
            }
            return user;
        }

        public BUser GetUserInfo(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            BUser user = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                user = new BUser();
                user.User = (from u in db.Users where u.Email == email select u).FirstOrDefault<Users>();

                Admin_Users au = (from ausr in db.Admin_Users where ausr.User_Id == user.User.Id select ausr).FirstOrDefault<Admin_Users>();
                if (au != null)
                {
                    user.IsSuperAdmin = au.IsSuperAdmin;
                    user.IsWebMaster = au.IsWebMaster;
                    user.IsAdmin = true;
                }
                if (!user.IsSuperAdmin)
                {
                    user.Permission = PermissionManagement.GetUserPermissions(user.User.Id);
                }
                else
                {
                    user.Permission = new Permissions();
                    System.Reflection.FieldInfo[] fields = typeof(Permissions).GetFields();
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(user.Permission, 1);
                    }
                }
            }
            return user;
        }

        public List<Area> GetAreas(int parentId)
        {
            List<Area> areas = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var tmp = from a in db.Area select a;
                if (parentId > 0)
                {
                    tmp = tmp.Where(a => a.Upid == parentId);
                }
                else
                {
                    tmp = tmp.Where(a => a.Level == 1);
                }

                areas = tmp.OrderBy(a => a.Id).ToList<Area>();
            }
            return areas;
        }

        public List<Sp> GetSps()
        {
            List<Sp> spList = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                spList = (from s in db.Sp orderby s.Id select s).ToList<Sp>();
            }
            return spList;
        }

        public List<PayType> GetPayTypes()
        {
            List<PayType> types = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                types = (from t in db.PayType orderby t.Id select t).ToList<PayType>();
            }
            return types;
        }

        public List<User_type> GetUserTypes()
        {
            List<User_type> types = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                types = (from t in db.User_type orderby t.Id select t).ToList<User_type>();
            }
            return types;
        }

        protected void SyncObjectProperties(object o1, object o2)
        {
            if(o1==null || o2==null)
            {
                return;
            }

            if(o1.GetType().ToString()!=o2.GetType().ToString())
            {
                return;
            }

            System.Reflection.PropertyInfo[] properties = o1.GetType().GetProperties();
            if (properties == null || properties.Length == 0) {
                return;
            }
            foreach(System.Reflection.PropertyInfo property in properties)
            {
                property.SetValue(o1, property.GetValue(o2));
            }
        }

        public List<BTaocan> FindBTaocans()
        {
            List<BTaocan> taocans = new List<BTaocan>();

            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from t in db.Taocan
                            join sp in db.Sp on t.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            orderby t.Created_time descending
                            select new BTaocan
                            {
                                Taocan=t,
                                SP =llsp
                            };

                taocans = query.ToList<BTaocan>();
            }

            return taocans;
        }
    }
}
