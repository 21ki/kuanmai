using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.Common.Util;
using KM.JXC.BL.Admin;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Models;
using KM.JXC.BL.Models.Admin;
namespace KM.JXC.BL.Admin
{
    public class SystemAdmin
    {
        public BSysUser CurrentUser { get; private set; }

        public SystemAdmin(BSysUser user) 
        {
            if (user == null)
            {
                throw new KMJXCException("请先用系统管理账户登录后台管理系统");
            }
            this.CurrentUser = user;
        }

        public SystemAdmin(int user_id)
        {
            if (user_id == 0)
            {
                throw new KMJXCException("请先用系统管理账户登录后台管理系统");
            }

            this.InitializeUser(user_id);
        }

        private void InitializeUser(int uid) 
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                this.CurrentUser = (from u in db.User
                                    where u.User_ID == uid && u.IsSystemUser == true
                                    select new BSysUser 
                                    {
                                         ID=u.User_ID,
                                         Name=u.Name,
                                         Created=(long)u.Created,
                                         Modified=(long)u.Modified,
                                         NickName=u.NickName
                                    }).FirstOrDefault<BSysUser>();


                PermissionManager pManager = new PermissionManager();

                Admin_Super adminUser=(from a in db.Admin_Super where a.user_id==this.CurrentUser.ID select a).FirstOrDefault<Admin_Super>();
                if (adminUser != null)
                {
                    this.CurrentUser.Permission = pManager.GetAllPermission();
                }
                else 
                {
                    this.CurrentUser.Permission = pManager.GetUserPermission(new BUser { ID = this.CurrentUser.ID });
                }                
            }
        }

        public static SystemAdmin Login(string name, string password)
        {
            SystemAdmin adminInstance = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                User user = (from u in db.User where u.IsSystemUser == true && u.Name == name select u).FirstOrDefault<User>();
                if (user == null)
                {
                    throw new KMJXCException("用户名不存在");
                }

                string md5Password = Encrypt.MD5(password);

                if (md5Password != user.Password)
                {
                    throw new KMJXCException("密码不正确");
                }

                adminInstance = new SystemAdmin(user.User_ID);
            }

            return adminInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void SetCorpInfo(Corp_Info info)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Corp_Info first=(from ci in db.Corp_Info orderby ci.ID ascending select ci).FirstOrDefault<Corp_Info>();
                List<Corp_Info> currents=(from ci in db.Corp_Info where ci.IsCurrent==true select ci).ToList<Corp_Info>();
                foreach (Corp_Info ci in currents)
                {
                    ci.IsCurrent = false;
                }
                info.IsCurrent = true;
                info.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                info.Modified_By=this.CurrentUser.ID;
                if(first!=null)
                {
                    info.Created=first.Created;
                    info.Created_By=first.Created_By;
                }
                db.Corp_Info.Add(info);
                db.SaveChanges();
            }
        }

        public bool VerifyPassword(string password,int uid=0)
        {
            bool result = false;
            int user_id = this.CurrentUser.ID;
            if (uid > 0)
            {
                user_id = uid;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                User user=(from u in db.User where u.User_ID==user_id && u.IsSystemUser==true select u).FirstOrDefault<User>();
                if (user == null)
                {
                    throw new KMJXCException("用户不存在");
                }

                if (Encrypt.MD5(password) == user.Password)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool UpdatePassword(string password, int uid = 0)
        {
            bool result = false;
            int user_id = this.CurrentUser.ID;
            if (uid > 0)
            {
                user_id = uid;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                User user = (from u in db.User where u.User_ID == user_id && u.IsSystemUser == true select u).FirstOrDefault<User>();
                if (user == null)
                {
                    throw new KMJXCException("用户不存在");
                }

                user.Password = Encrypt.MD5(password);
                db.SaveChanges();
                result = true;
            }

            return result;
        }
    }
}
