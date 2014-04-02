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
    public class UserManager:BBaseManager
    {
        public IOUserManager MallUserManager = null;

        public UserManager(BUser user, Permission permission)
            : base(user,permission)
        {
            
        }

        public UserManager(int user_id, Permission permission)
            : base(user_id,permission)
        {
            
        }

        public UserManager(int user_id, IOUserManager manager, Permission permission)
            : base(user_id,permission)
        {
            this.MallUserManager = manager;
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
                                 EmployeeInfo = (from e in db.Employee where e.User_ID == u.User_ID select e).FirstOrDefault<Employee>(),
                                 Mall_ID = u.Mall_ID,
                                 Mall_Name = u.Mall_Name,                                
                                 Name = u.Name,
                                 Password = u.Password,
                                 Parent_ID=(int)u.Parent_User_ID,
                                 Type = (from t in db.Mall_Type where t.Mall_Type_ID == u.Mall_Type select t).FirstOrDefault<Mall_Type>(),                                 
                             };
                user = us.FirstOrDefault<BUser>();
                if (user.Parent_ID > 0)
                {
                    user.Parent = this.GetUser(user.Parent_ID);
                }
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
        public BUser GetUser(BUser user)
        {
            return this.GetUser(user.ID);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public BUser CreateNewUser(BUser user) {
           
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

                User dbUser = new User();
                dbUser.User_ID = user.ID;
                dbUser.Mall_ID = user.Mall_ID;
                dbUser.Mall_Name = user.Mall_Name;
                dbUser.Name = user.Name;
                dbUser.Mall_Type = user.Type.Mall_Type_ID;
                if (user.Parent != null)
                {
                    dbUser.Parent_Mall_ID = user.Parent.Mall_ID;
                    dbUser.Parent_Mall_Name = user.Parent.Mall_Name;
                    dbUser.Parent_User_ID = user.Parent.ID;
                }
                dba.User.Add(dbUser);
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
        public void UpdateUser(BUser user)
        {
            if (this.CurrentUserPermission.UPDATE_USER == 0)
            {
                throw new UserException("没有权限创建新用户");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var old = from ou in db.User where ou.User_ID == user.ID select ou;

                User dbUser = old.FirstOrDefault<User>();
                if (dbUser != null)
                {
                    dbUser.User_ID = user.ID;
                    dbUser.Mall_ID = user.Mall_ID;
                    dbUser.Mall_Name = user.Mall_Name;
                    dbUser.Name = user.Name;
                    dbUser.Password = user.Password;
                    //dbUser.Mall_Type = user.Type.Mall_Type_ID;
                    if (user.Parent != null)
                    {
                        dbUser.Parent_Mall_ID = user.Parent.Mall_ID;
                        dbUser.Parent_Mall_Name = user.Parent.Mall_Name;
                        dbUser.Parent_User_ID = user.Parent.ID;
                    }

                    Employee employee = (from em in db.Employee where em.User_ID == dbUser.User_ID select em).FirstOrDefault<Employee>();
                    if (employee != null)
                    {
                        base.UpdateProperties(employee, user.EmployeeInfo);
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public bool SyncShopSubUsers(int shop_id)
        {
            bool result = false;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                BUser dbUser = (from us in db.User
                                from sp in db.Shop
                                where us.User_ID == sp.User_ID && sp.Shop_ID == shop_id
                                select new BUser
                                {
                                    ID = us.User_ID,
                                    //EmployeeInfo = (from e in db.Employee where e.User_ID == us.User_ID select e).FirstOrDefault<Employee>(),
                                    Mall_ID = us.Mall_ID,
                                    Mall_Name = us.Mall_Name,
                                    Type = (from type in db.Mall_Type where type.Mall_Type_ID == us.Mall_Type select type).FirstOrDefault<Mall_Type>(),
                                    Parent_ID = (int)us.Parent_User_ID,
                                    Parent = null,
                                    Name = us.Name,
                                    Password = us.Password
                                }).FirstOrDefault<BUser>();

                if (dbUser == null)
                {
                    throw new KMJXCException("没有找到对应店铺的卖家信息");
                }

                List<BUser> subUsers = this.MallUserManager.GetSubUsers(dbUser);
                List<BUser> existedUsers = (from us in db.User
                                            from sp in db.Shop_User
                                            where us.User_ID == sp.User_ID && sp.Shop_ID == shop_id
                                            select new BUser
                                            {
                                                ID = us.User_ID,
                                                //EmployeeInfo = (from e in db.Employee where e.User_ID == us.User_ID select e).FirstOrDefault<Employee>(),
                                                Mall_ID = us.Mall_ID,
                                                Mall_Name = us.Mall_Name,
                                                Type = (from type in db.Mall_Type where type.Mall_Type_ID == us.Mall_Type select type).FirstOrDefault<Mall_Type>(),
                                                Parent_ID = (int)us.Parent_User_ID,
                                                Parent = null,
                                                Name = us.Name,
                                                Password = us.Password
                                            }).ToList<BUser>();


                foreach (BUser user in subUsers)
                {
                    bool found = false;
                    foreach (BUser eUser in existedUsers)
                    {
                        if (user.Mall_ID == eUser.Mall_ID && user.Mall_Name == eUser.Mall_Name)
                        {
                            //Update user
                            found = true;
                            eUser.EmployeeInfo = user.EmployeeInfo;
                            eUser.Name = user.Name;
                            break;
                        }
                    }

                    if (!found)
                    {
                        //add new sub user
                        User dbUser1 = new User();
                        //dbUser1.User_ID = user.ID;
                        dbUser1.Mall_ID = user.Mall_ID;
                        dbUser1.Mall_Name = user.Mall_Name;
                        dbUser1.Name = user.Name;
                        dbUser1.Mall_Type = user.Type.Mall_Type_ID;
                        if (user.Parent != null)
                        {
                            dbUser1.Parent_Mall_ID = user.Parent.Mall_ID;
                            dbUser1.Parent_Mall_Name = user.Parent.Mall_Name;
                            dbUser1.Parent_User_ID = user.Parent.ID;
                        }
                        db.User.Add(dbUser1);
                        db.SaveChanges();

                        if (user.EmployeeInfo != null && dbUser1.User_ID>0)
                        {
                            db.Employee.Add(user.EmployeeInfo);
                        }

                        Shop_User sp = new Shop_User();
                        sp.Shop_ID = shop_id;
                        sp.User_ID = dbUser1.User_ID;
                        db.Shop_User.Add(sp);
                    }
                }

                db.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool UpdateEmployeeInfo(Employee employee)
        {
            bool result = false;
            if (this.CurrentUserPermission.UPDATE_EMPLOYEE == 0)
            {
                throw new KMJXCException("没有权限更新员工信息");
            }
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                User user = (from u in db.User where u.User_ID == employee.User_ID select u).FirstOrDefault<User>();
                if (user != null)
                {
                    Employee existing = (from e in db.Employee where e.User_ID == employee.User_ID select e).FirstOrDefault<Employee>();
                    if (existing == null)
                    {
                        db.Employee.Add(employee);
                    }
                    else
                    {
                        this.UpdateProperties(existing, employee);
                    }
                    result = true;
                }
            }
            catch
            {

            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public Employee GetEmployInfo(int user_id)
        {
            Employee e = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                e = (from em in db.Employee where em.User_ID == user_id select em).FirstOrDefault<Employee>();
            }
            return e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BUser> GetUsers(string department,string duty,int pageInde,int pageSize,out int total)
        {
            List<BUser> users = new List<BUser>();
            total = 0;
            return users;
        }
    }
}
