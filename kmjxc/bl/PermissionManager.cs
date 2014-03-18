using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
namespace KM.JXC.BL
{
    public struct Permissions
    {
        //User
        public int DELETE_USER = 0;
        public int DISABLE_USER = 0;
        public int VIEW_USER_PERMISSION = 0;
        public int GRANT_USER_PERMISSION = 0;

        //Store
        public int ADD_STORE_HOUSE = 0;
        public int UPDATE_STORE_HOUSE = 0;
        public int DELETE_STORE_HOUSE = 0;

        //Sale Order
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

        //Employee
        public int VIEW_EMPLOYEE = 0;
        public int UPDATE_EMPLOYEE = 0;
        public int DELETE_EMPLOYEE = 0;

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
        public int ADD_STOCK = 0;
        public int LEAVE_STOCK = 0;
        public int BACK_STOCK = 0;
    }

    public class PermissionManager
    {
        public PermissionManager()
        {
        }

        /// <summary>
        /// Get all actions
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Admin_Action> GetActions(User user)
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
                    action.HadPermission = true;
                }
            }

            return all_actions;
        }

        /// <summary>
        /// Get user actions
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Admin_Action> GetUserActions(User user)
        {
            List<Admin_Action> actions = new List<Admin_Action>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var ps = from a in db.Admin_Action
                         from c in db.Admin_Role_Action
                         from d in db.Admin_User_Role
                         where a.id == c.action_id && c.role_id == d.role_id && d.user_id == user.User_ID
                         select a;
                if (ps != null)
                {
                    actions = ps.ToList<Admin_Action>();
                }
            }

            return actions;
        }

        /// <summary>
        /// Get user permission
        /// </summary>
        /// <param name="user">User object, must has User_ID field</param>
        /// <returns>Permissions Object</returns>
        public Permissions GetUserPermission(User user)
        {
            Permissions permissions = new Permissions();
            List<Admin_Action> actions = this.GetUserActions(user); 
            Type type = typeof(Permissions);

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
    }
}
