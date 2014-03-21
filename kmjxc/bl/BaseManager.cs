using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using System.Data.Entity;

namespace KM.JXC.BL
{
    public class BaseManager
    {
        protected User CurrentUser{get;set;}
        protected int Shop_Id { get; set; }
        protected Permission CurrentUserPermission = new Permission();
        protected PermissionManager permissionManager;

        public BaseManager(User user,int shop_id)
        {
            this.CurrentUser = user;
            this.Shop_Id = shop_id;
            permissionManager = new PermissionManager(shop_id);
            this.GetUserPermission();
        }

        public BaseManager(int user_id, int shop_id)
        {
            GetUserById(user_id);
            this.Shop_Id = shop_id;
            this.GetUserPermission();
        }

        public BaseManager(int user_id)
        {
            GetUserById(user_id);
            this.GetUserPermission();
        }

        public BaseManager(User user)
        {
            this.CurrentUser = user;           
            this.GetUserPermission();
        }

        public BaseManager()
        {
        }

        private void GetUserById(int user_id)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var u = from us in db.User where us.User_ID == user_id select us;
                if (u.ToList<User>().Count == 1)
                {
                    this.CurrentUser = u.ToList<User>()[0];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetUserPermission()
        {
            if (this.CurrentUser == null || this.CurrentUser.User_ID <= 0)
            {
                return;
                //throw new KMJXCException("调用 " + this.GetType() + " 没有传入当前登录用户对象", ExceptionLevel.SYSTEM);
            }

            CurrentUserPermission = this.permissionManager.GetUserPermission(this.CurrentUser);
        }

        protected void UpdateProperties(object oldObj,object newObj)
        {
            if (oldObj.GetType().ToString() != newObj.GetType().ToString())
            {
                throw new KMJXCException("更新model数据时两个对象的类型必须相同",ExceptionLevel.SYSTEM);
            }

            Type type = oldObj.GetType();

            
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                property.SetValue(oldObj, property.GetValue(newObj));
            }
        }
    }
}
