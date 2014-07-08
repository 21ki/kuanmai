using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.Entity.Validation;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class AdminActionAttribute : System.Attribute
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public string ActionDescription { get; set; }
        public bool IsSystem { get; set; }
    }

    public class Permission
    {
        //User 
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription="删除用户")]
        public int DELETE_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "禁用用户")]
        public int DISABLE_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "更新用户")]
        public int UPDATE_USER;       
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "同步商城用户")]
        public int SYNC_MALLUSER;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "添加用户")]
        public int ADD_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "查看员工信息")]
        public int VIEW_EMPLOYEE = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "更新员工信息")]
        public int UPDATE_EMPLOYEE = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "删除员工信息")]
        public int DELETE_EMPLOYEE = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "创建员工信息")]
        public int CREATE_EMPLOYEE;

        //Sale Order  
        [AdminActionAttribute(ID = 6, CategoryName = "订单管理",ActionDescription = "同步订单")]
        public int SYNC_ORDER = 0;
        [AdminActionAttribute(ID = 6, CategoryName = "订单管理", ActionDescription = "处理退货订单")]
        public int HANDLE_BACK_SALE = 0;
        //Customer
        [AdminActionAttribute(ID = 6, CategoryName = "订单管理", ActionDescription = "查看客户信息")]
        public int VIEW_CUSTOMER = 0;
        [AdminActionAttribute(ID = 6, CategoryName = "订单管理", ActionDescription = "删除客户")]
        public int DELETE_CUSTOMER = 0;
        [AdminActionAttribute(ID = 6, CategoryName = "订单管理", ActionDescription = "更新客户信息")]
        public int UPDATE_CUSTOMER = 0;

        //Buy

        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "创建采购询价单")]
        public int CREATE_BUY_PRICE = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "修改采购询价单")]
        public int UPDATE_BUY_PRICE = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "查看采购询价单")]
        public int VIEW_BUY_PRICE = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "查看验货单")]
        public int VIEW_BUY = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "添加验货单")]
        public int ADD_BUY = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "查看采购订单")]
        public int VIEW_BUY_ORDER;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "添加采购订单")]
        public int ADD_BUY_ORDER = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "修改采购订单")]
        public int UPDATE_BUY_ORDER = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "删除采购订单")]
        public int DELETE_BUY_ORDER = 0;//only this order doesn't has buy order detail could be deleted
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "删除验货单")]
        public int DELETE_BUY = 0;//only this buy doesn't has buy detail could be deleted       
        //Supplier
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "添加供应商")]
        public int ADD_SUPPLIER = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "更新供应商")]
        public int UPDATE_SUPPLIER = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "查看供应商信息")]
        public int VIEW_SUPPLIER_SUMMARY = 0;
        [AdminActionAttribute(ID = 4, CategoryName = "采购管理", ActionDescription = "查看供应商详细信息")]
        public int VIEW_SUPPLIER_DETAIL = 0;

        //Product
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "添加产品")]
        public int ADD_PRODUCT = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "更新产品")]
        public int UPDATE_PRODUCT = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "删除产品")]
        public int DELETE_PRODUCT = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "添加类目")]
        public int ADD_PRODUCT_CLASS = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "更新类目")]
        public int UPDATE_PRODUCT_CLASS = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "删除类目")]
        public int DELETE_PRODUCT_CLASS = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "添加产品单位")]
        public int ADD_PRODUCT_UNIT = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "更新产品单位")]
        public int UPDATE_PRODUCT_UNIT = 0;
        //Category and Property
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "添加类目")]
        public int ADD_CATEGORY = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "更新类目")]
        public int UPDATE_CATEGORY = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "添加属性")]
        public int ADD_PORPERTY = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "更新属性")]
        public int UPDATE_PORPERTY = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "产品管理", ActionDescription = "禁用类目")]
        public int DISABLE_CATEGORY = 0;

        //Stock
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "添加入库单")]
        public int ADD_ENTER_STOCK = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "添加出库单")]
        public int ADD_LEAVE_STOCK = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "添加退库单")]
        public int ADD_BACK_STOCK = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "更新入库单到库存")]
        public int UPDATE_ENTERSTOCK_TO_PRODUCT_STOCK = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "添加仓库")]
        public int ADD_STORE_HOUSE = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "更新仓库")]
        public int UPDATE_STORE_HOUSE = 0;
        [AdminActionAttribute(ID = 2, CategoryName = "库存管理", ActionDescription = "删除仓库")]
        public int DELETE_STORE_HOUSE = 0;



        //Express
        [AdminActionAttribute(ID = 6, CategoryName = "店铺管理", ActionDescription = "添加快递")]
        public int ADD_SHOP_EXPRESS = 0;
        [AdminActionAttribute(ID = 6, CategoryName = "店铺管理", ActionDescription = "更新快递")]
        public int UPDATE_SHOP_EXPRESS = 0;
        [AdminActionAttribute(ID = 6, CategoryName = "店铺管理", ActionDescription = "查看日志")]
        public int VIEW_USER_LOG = 0;

        //User Permission
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "查看权限管理")]
        public int HAS_PERMISSION_ADMIN = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "添加管理分组")]
        public int ADD_ADMIN_ROLE = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "更新管理分组")]
        public int UPDATE_ADMIN_ROLE = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "删除管理分组")]
        public int DELETE_ADMIN_ROLE = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "查看管理分组")]
        public int VIEW_ADMIN_ROLE = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "查看用户权限")]
        public int VIEW_USER_PERMISSION = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "更新用户权限")]
        public int UPDATE_USER_PERMISSION = 0;
        [AdminActionAttribute(ID = 5, CategoryName = "权限管理", ActionDescription = "更新管理分组权限")]
        public int UPDATE_ROLE_ACTION = 0;
        //Report

    }

    internal class PermissionManager
    {
        public int Shop_Id { get; set; }       

        public PermissionManager(int shop_id)
        {
            this.Shop_Id = shop_id;
        }

        public PermissionManager()
        {

        }

        /// <summary>
        /// Get all actions
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Admin_Action> GetActions(BUser user)
        {
            List<Admin_Action> actions = new List<Admin_Action>();
            List<Admin_Action> all_actions = new List<Admin_Action>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (user != null)
                {
                    actions = this.GetUserActions(user);
                }

                var ps = from a in db.Admin_Action select a;
                all_actions = ps.ToList<Admin_Action>();
            }

            foreach (Admin_Action action in all_actions)
            {
                bool found = false;

                foreach (Admin_Action uaction in actions)
                {
                    if (action.action_name == uaction.action_name && action.id == uaction.id)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    action.enable = true;
                }
                else
                {
                    action.enable = false;
                }
            }

            return all_actions;
        }

        /// <summary>
        /// Get user actions
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Admin_Action> GetUserActions(BUser user)
        {            
            List<Admin_Action> actions = new List<Admin_Action>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {

                var ps = from a in db.Admin_Action   
                         join c in db.Admin_Role_Action on a.id equals c.action_id
                         join d in db.Admin_User_Role on c.role_id equals d.role_id
                         join b in db.Admin_Role on d.role_id equals b.id
                         where d.user_id==user.ID && b.enabled==true
                         orderby a.id ascending
                         select a;               
                actions = ps.ToList<Admin_Action>();              
                
            }            

            return actions;
        }

        /// <summary>
        /// Get user permission
        /// </summary>
        /// <param name="user">User object, must has User_ID field</param>
        /// <returns>Permissions Object</returns>
        public Permission GetUserPermission(BUser user)
        {
            Permission permissions = new Permission();
            List<Admin_Action> actions = this.GetUserActions(user); 
            Type type = typeof(Permission);

            foreach (Admin_Action action in actions)
            {
                FieldInfo field = type.GetField(action.action_name);
                if (field != null)
                {
                    field.SetValue(permissions, 1);
                }
            }

            return permissions;
        }

        /// <summary>
        /// Sync actions with Permission object
        /// </summary>
        public void SyncPermissionWithAction()
        {
            Type permission=typeof(Permission);
            FieldInfo[] fields = permission.GetFields();
            if (fields == null || fields.Length<=0)
            {
                return;
            }

            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                foreach (FieldInfo field in fields)
                {
                    var action = from a in db.Admin_Action where a.action_name == field.Name select a;
                    if (action == null || action.ToList<Admin_Action>().Count == 0)
                    {
                        Admin_Action new_action = new Admin_Action();
                        new_action.action_name = field.Name;
                        new_action.action_description = field.Name;
                        new_action.enable = true;
                        db.Admin_Action.Add(new_action);
                    }
                }
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

            }
            finally
            {
                db.Dispose();
            }
        }

        /// <summary>
        /// Update actions
        /// </summary>
        /// <param name="actions"></param>
        public void UpdateAction(List<Admin_Action> actions)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                foreach (Admin_Action action in actions)
                {
                    if (action.id > 0)
                    {
                        db.Admin_Action.Attach(action);
                    }
                }

                db.SaveChanges();
            }
        }

        public Permission GetAllPermission() 
        {
            Permission p = new Permission();
            //shop owner has full permissions
            Type permission = typeof(Permission);
            FieldInfo[] fields = permission.GetFields();
            foreach (FieldInfo field in fields)
            {
                field.SetValue(p, 1);
            }

            return p;
        }
    }
}
