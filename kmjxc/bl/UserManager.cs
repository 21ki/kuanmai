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
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class UserManager:BaseManager
    { 
        public UserManager(User user):base(user)
        {
           
        }

        public UserManager(int user_id)
            : base(user_id)
        {

        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="user_Id"></param>
        /// <returns></returns>       
        public BUser GetUser(int user_Id)
        {
            BUser user = null;
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                var us = from u in db.User
                         where u.User_ID == user_Id
                         select new BUser
                             {
                                 ID = u.User_ID,
                                 EmployeeInfo = (from e in db.Employee where e.User_ID == u.User_ID select e).ToList<Employee>()[0],
                                 Mall_ID = u.Mall_ID,
                                 Mall_Name = u.Mall_Name,
                                 Mall_Parent_ID = u.Parent_Mall_ID,
                                 Mall_Parent_Name = u.Parent_Mall_Name,
                                 Parent_ID = (int)u.Parent_User_ID,
                                 Name = u.Name,
                                 Password = u.Password,
                                 Type = (from t in db.Mall_Type where t.Mall_Type_ID == u.Mall_Type select t).ToList<Mall_Type>()[0]
                             };
                user = us.ToList<BUser>()[0];

            }
            catch
            {
            }
            return user;
        }

        /// <summary>
        /// Get user by user object
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public BUser GetUser(User user)
        {
            return this.GetUser(user.User_ID);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User CreateNewUser(User user) {
           
            if (user == null) {
                throw new UserException("用户实体不能为空引用");
            }

            if (string.IsNullOrEmpty(user.Name)) {
                throw new UserException("用户名不能为空");
            }

            if (this.CurrentUserPermission.ADD_USER == 0)
            {
                throw new UserException("没有权限创建新用户");
            }
           
            KuanMaiEntities dba = new KuanMaiEntities();
            try
            {   
                if (GetUser(user) != null)
                    throw new UserException("用户名已经存在");

                dba.User.Add(user);
                dba.SaveChanges();
                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dba != null)
                {
                    dba.Dispose();
                }
            }            
        }

        /// <summary>
        /// Update user object
        /// </summary>
        /// <param name="newUser"></param>
        public void UpdateUser(User newUser)
        {
            if (this.CurrentUserPermission.UPDATE_USER == 0)
            {
                throw new UserException("没有权限创建新用户");
            }

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
