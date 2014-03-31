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
        public Shop Main_Shop { get; private set; }
        public BUser CurrentUser { get; private set; }
        public BUser MainUser { get; private set; }
        public int Shop_Id { get; private set; }
        public int Main_Shop_Id { get; private set; }
        public Permission CurrentUserPermission = new Permission();
        public PermissionManager permissionManager;

        public BBaseManager(BUser user,int shop_id)
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

        public BBaseManager(BUser user)
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
                this.CurrentUser = (from us in db.User where us.User_ID == user_id 
                                    select new BUser
                                    {
                                         EmployeeInfo=(from employee in db.Employee where employee.User_ID==us.User_ID select employee).FirstOrDefault<Employee>(),
                                         ID=us.User_ID,
                                         Mall_ID=us.Mall_ID,
                                         Mall_Name=us.Mall_Name,
                                         Name=us.Name,
                                         Parent=null,
                                         Parent_ID=(int)us.Parent_User_ID,
                                         Password=us.Password,
                                         Type= (from mtype in db.Mall_Type where mtype.Mall_Type_ID==us.Mall_Type select mtype).FirstOrDefault<Mall_Type>()
                                    }).FirstOrDefault<BUser>();
                if (this.CurrentUser != null && this.CurrentUser.Parent_ID > 0 && !string.IsNullOrEmpty(this.CurrentUser.Parent.Mall_ID))
                {
                    this.MainUser = (from us in db.User
                                     where us.User_ID == this.CurrentUser.Parent_ID
                                     select new BUser
                                     {
                                         EmployeeInfo = (from employee in db.Employee where employee.User_ID == us.User_ID select employee).FirstOrDefault<Employee>(),
                                         ID = us.User_ID,
                                         Mall_ID = us.Mall_ID,
                                         Mall_Name = us.Mall_Name,
                                         Name = us.Name,
                                         Parent = null,
                                         Parent_ID = (int)us.Parent_User_ID,
                                         Password = us.Password,
                                         Type = (from mtype in db.Mall_Type where mtype.Mall_Type_ID == us.Mall_Type select mtype).FirstOrDefault<Mall_Type>()
                                     }).FirstOrDefault<BUser>();
                }
                else
                {
                    this.MainUser = this.CurrentUser;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetUserPermission()
        {
            if (this.CurrentUser == null || this.CurrentUser.ID <= 0)
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
                Shop shop = (from s in db.Shop where s.User_ID == this.MainUser.ID select s).FirstOrDefault<Shop>();
                if (shop == null)
                {
                    shop = (from s in db.Shop
                            from sp in db.Shop_User
                            where s.Shop_ID == sp.Shop_ID && sp.User_ID == this.CurrentUser.ID
                            select s).FirstOrDefault<Shop>();

                    if (shop == null)
                    {
                        throw new KMJXCException("你不是店铺掌柜，也不是任何店铺的子账户");
                    }
                }
                
                this.Shop_Id = shop.Shop_ID;
                this.Shop = shop;
                if (shop.Parent_Shop_ID > 0)
                {
                    this.Main_Shop = (from s in db.Shop where s.Shop_ID == shop.Parent_Shop_ID select s).FirstOrDefault<Shop>();
                }
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
