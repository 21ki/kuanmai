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
    public class Permission
    {
        //User
        public int DELETE_USER = 0;
        public int DISABLE_USER = 0;
        public int VIEW_USER_PERMISSION = 0;
        public int GRANT_USER_PERMISSION = 0;
        public int UPDATE_USER;        
        public int MAPP_USER_EMPLOYEE;
        public int SYNC_MALLUSER;
        public int ADD_USER;

        //Employee
        public int VIEW_EMPLOYEE = 0;
        public int UPDATE_EMPLOYEE = 0;
        public int DELETE_EMPLOYEE = 0;
        public int CREATE_EMPLOYEE;

        //Store
        public int ADD_STORE_HOUSE = 0;
        public int UPDATE_STORE_HOUSE = 0;
        public int DELETE_STORE_HOUSE = 0;

        //Sale Order
        public int ADD_BACK_SALE = 0;
        public int SYNC_ORDER = 0;
        public int VIEW_ORDER_SUMMARY = 0;
        public int VIEW_ORDER_DETAIL = 0;
        public int DELETE_ORDER = 0;
        public int DISABLE_ORDER = 0;
        public int BACK_STORE_ORDER = 0;
        public int WAST_ORDER = 0;

        //Supplier
        public int ADD_SUPPLIER = 0;
        public int UPDATE_SUPPLIER = 0;
        public int VIEW_SUPPLIER_SUMMARY = 0;
        public int VIEW_SUPPLIER_DETAIL = 0;

        //Customer
        public int VIEW_CUSTOMER = 0;
        public int DELETE_CUSTOMER = 0;
        public int UPDATE_CUSTOMER = 0;

        //Buy
        public int VIEW_BUY = 0;
        public int ADD_BUY = 0;        
        public int VIEW_BUY_ORDER;
        public int ADD_BUY_ORDER = 0;
        public int UPDATE_BUY_ORDER = 0;
        public int DELETE_BUY_ORDER = 0;//only this order doesn't has buy order detail could be deleted
        public int DELETE_BUY = 0;//only this buy doesn't has buy detail could be deleted       

        //Product
        public int VIEW_PRODUCT = 0;
        public int ADD_PRODUCT = 0;
        public int UPDATE_PRODUCT = 0;
        public int DELETE_PRODUCT = 0;
        public int ADD_PRODUCT_CLASS = 0;
        public int UPDATE_PRODUCT_CLASS = 0;
        public int DELETE_PRODUCT_CLASS = 0;
        public int ADD_PRODUCT_UNIT = 0;
        public int UPDATE_PRODUCT_UNIT = 0;

        //Stock
        public int ADD_ENTER_STOCK = 0;
        public int ADD_LEAVE_STOCK = 0;
        public int ADD_BACK_STOCK = 0;

        //Category and Property
        public int ADD_CATEGORY = 0;
        public int UPDATE_CATEGORY = 0;
        public int ADD_PORPERTY = 0;
        public int UPDATE_PORPERTY = 0;
        public int DISABLE_CATEGORY = 0;
    }

    public class PermissionManager
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

        /// <summary>
        /// Verify user is super admin or site admin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Admin_Super GetSuperAdmin(User user)
        {
            Admin_Super super_admin=new Admin_Super();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var super = from s in db.Admin_Super where s.user_id == user.User_ID select s;
                if (super != null && super.ToList<Admin_Super>().Count>0)
                {
                    super_admin = super.ToList<Admin_Super>()[0];
                }
            }
            
             return super_admin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        public void CreateAdminRole(Admin_Role adminRole)
        {
            if (adminRole == null || string.IsNullOrEmpty(adminRole.role_name))
            {
                throw new KMJXCException("输入信息有错误");
            }
            if (adminRole.shop_id <= 0)
            {
                throw new KMJXCException("创建权限角色时必须制定店铺");
            }

            Admin_Role role = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                role = (from adminrole in db.Admin_Role
                        where adminrole.role_name == adminRole.role_name && adminrole.shop_id == adminRole.shop_id 
                                   select adminrole).FirstOrDefault<Admin_Role>();

                if (role != null && role.id > 0)
                {
                    throw new KMJXCException("名称为" + adminRole.role_name + "的管理角色已经存在");
                }
                
                db.Admin_Role.Add(adminRole);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GrantRoleActions(int roleId,int[] actions)
        {
            if (roleId <= 0)
            {
                throw new KMJXCException("请选择角色");
            }
            List<Admin_Action> adminActions = new List<Admin_Action>();
            if (actions!=null && actions.Length > 0)
            {
                for (int i = 0; i < actions.Length; i++)
                {                   
                    adminActions.Add(new Admin_Action() { id=actions[i] });
                }
            }

            this.GrantRoleActions(new Admin_Role() { id=roleId},adminActions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <param name="actions"></param>
        public void GrantRoleActions(Admin_Role role, List<Admin_Action> actions)
        {
            if (role == null)
            {
                throw new KMJXCException("请选择角色");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Admin_Role existing = (from roles in db.Admin_Role where roles.id == role.id select roles).FirstOrDefault<Admin_Role>();
                if (existing == null)
                {
                    throw new KMJXCException(role.role_name+" 不存在");
                }

                List<Admin_Action> allActions=(from a in db.Admin_Action select a).ToList<Admin_Action>();

                List<Admin_Role_Action> roleActions = (from ara in db.Admin_Role_Action where ara.role_id == existing.id select ara).ToList<Admin_Role_Action>();

                //remove existed role actions
                if (roleActions != null && roleActions.Count > 0)
                {
                    foreach (Admin_Role_Action ara in roleActions)
                    {
                        db.Admin_Role_Action.Remove(ara);
                    }

                    db.SaveChanges();
                }

                //add role actions
                if (actions != null)
                {
                    foreach (Admin_Action action in actions)
                    {
                        //verify if the action is existed
                        Admin_Action existed = (from al in allActions where al.id == action.id select al).FirstOrDefault<Admin_Action>();
                        if (existed != null)
                        {
                            Admin_Role_Action ara = new Admin_Role_Action();
                            ara.role_id = existing.id;
                            ara.action_id = action.id;
                            db.Admin_Role_Action.Add(ara);
                        }
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}
