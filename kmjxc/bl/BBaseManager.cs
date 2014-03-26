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
using KM.JXC.BL.Models;

namespace KM.JXC.BL
{
    public class BBaseManager
    {
        public Shop Shop { get; private set; }
        public User CurrentUser { get; private set; }
        public int Shop_Id { get; private set; }
        public Permission CurrentUserPermission = new Permission();
        public PermissionManager permissionManager;

        public BBaseManager(User user,int shop_id)
        {
            this.CurrentUser = user;
            this.Shop_Id = shop_id;
            if (this.Shop_Id == 0)
            {
                this.FindUserShop();
            }
            permissionManager = new PermissionManager(shop_id);
            this.GetUserPermission();
        }

        public BBaseManager(int user_id, int shop_id)
        {
            GetUserById(user_id);
            this.Shop_Id = shop_id;
            if (this.Shop_Id == 0)
            {
                this.FindUserShop();
            }
            this.GetUserPermission();
        }

        public BBaseManager(int user_id)
        {
            GetUserById(user_id);
            this.GetUserPermission();
            this.FindUserShop();
        }

        public BBaseManager(User user)
        {
            this.CurrentUser = user;           
            this.GetUserPermission();
            this.FindUserShop();
        }

        public BBaseManager()
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

        /// <summary>
        /// Find shop for current login user
        /// </summary>
        private void FindUserShop()
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Shop shop = (from s in db.Shop where s.User_ID == this.CurrentUser.User_ID select s).FirstOrDefault<Shop>();
                if (shop == null)
                {
                    shop = (from s in db.Shop
                            from sp in db.Shop_User
                            where s.Shop_ID == sp.Shop_ID && sp.User_ID == this.CurrentUser.User_ID
                            select s).FirstOrDefault<Shop>();

                    if (shop == null)
                    {
                        throw new KMJXCException("你不是店铺掌柜，也不是任何店铺的子账户");
                    }

                    
                }
                
                this.Shop_Id = shop.Shop_ID;
                this.Shop = shop;
            }
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
