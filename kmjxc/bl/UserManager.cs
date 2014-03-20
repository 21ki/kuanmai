using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Validation;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
namespace KM.JXC.BL
{
    public class UserManager:BaseManager
    {    
        public Access_Token CurrentToken { get; set; }
        
        public UserManager(User user,Access_Token token):base(user)
        {
           
        }

        public UserManager(int user_id, Access_Token token)
            : base(user_id)
        {

        }

        public UserManager()
            : base()
        {

        }

        public User GetUser(int user_Id)
        {
            User user = null;
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                var us = from u in db.User where u.User_ID == user_Id select u;
                if (us != null && us.ToList<User>().Count > 0)
                {
                    user = us.ToList<User>()[0];
                }
            }
            catch (DbEntityValidationException dbex)
            {

            }
            catch (Exception ex)
            {

            }

            return user;
        }

        public User GetUser(User user) {           

            KuanMaiEntities dba = new KuanMaiEntities();

            var obj = from p in dba.User where p.Mall_Name == user.Mall_Name && p.Mall_Type == user.Mall_Type select p;

            if (obj != null && obj.ToList<User>().Count > 0)
            {
                return obj.ToList<User>()[0];
            }

            return null;
        }

        public User CreateNewUser(User user) {
           
            if (user == null) {
                throw new UserException("用户实体不能为空引用");
            }

            if (string.IsNullOrEmpty(user.Name)) {
                throw new UserException("用户名不能为空");
            }

            try
            {
                KuanMaiEntities dba = new KuanMaiEntities();

                if (GetUser(user) != null)
                    throw new UserException("用户名已经存在");

                dba.User.Add(user);
                dba.SaveChanges();
                return GetUser(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }            
        }

        public void UpdateUser(User newUser)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var old = from ou in db.User where ou.User_ID == newUser.User_ID select ou;

                User oldUser = old.ToList<User>()[0];
                this.UpdateProperties(oldUser, newUser);
                db.SaveChanges();
            }
        }
    }
}
