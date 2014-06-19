using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;

namespace KM.JXC.BL
{
    public class PermissionManagement : BBaseManager
    {
        private PermissionManager PermissionManager;
        public PermissionManagement(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            PermissionManager = new PermissionManager(shop.Shop_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="shop_id"></param>
        public BAdminRole CreateRole(string name,string desc,int[] actions,int shop_id=0)
        {
            BAdminRole badminRole = null;
            if (this.CurrentUserPermission.ADD_ADMIN_ROLE == 0)
            {
                throw new KMJXCException("没有权限创建权限分组");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int shop=this.Shop.Shop_ID;
                if (shop_id > 0)
                {
                    shop = shop_id;
                }
                Admin_Role admin_role=(from role in db.Admin_Role where role.shop_id==shop && role.role_name==name select role).FirstOrDefault<Admin_Role>();
                if (admin_role != null)
                {
                    throw new KMJXCException("名为 " + name + " 的分组已经存在");
                }

                admin_role = new Admin_Role();
                admin_role.role_name = name;
                admin_role.shop_id = shop;
                admin_role.enabled = true;
                admin_role.description = desc;
                admin_role.create_uid = this.CurrentUser.ID;
                admin_role.create_date = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                admin_role.system_role = false;
                db.Admin_Role.Add(admin_role);
                db.SaveChanges();
                if (actions != null)
                {
                    this.UpdateRoleActions(admin_role.id,actions);
                }

                badminRole = new BAdminRole() { ID = admin_role.id, Name = admin_role.role_name, Description = admin_role.description };
                return badminRole;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BAdminRole> GetAdminRoles(int shop_id=0,bool enabled=true)
        {
            List<BAdminRole> roles = new List<BAdminRole>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int shop=this.Shop.Shop_ID;
                if(shop_id>0)
                {
                    shop=shop_id;
                }
                roles = (from role in db.Admin_Role
                         where role.shop_id == shop && role.enabled == enabled
                         select new BAdminRole
                         {
                             ID = role.id,
                             Name = role.role_name,
                             Description=role.description
                         }).ToList<BAdminRole>();
            }
            return roles;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role_id"></param>
        /// <param name="actions"></param>
        public void UpdateRoleActions(int role_id,int[] actions)
        {
            if (this.CurrentUserPermission.UPDATE_ROLE_ACTION == 0)
            {
                throw new KMJXCException("没有权限执行此操作");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Admin_Role role=(from r in db.Admin_Role where r.id==role_id select r).FirstOrDefault<Admin_Role>();
                if (role == null)
                {
                    throw new KMJXCException("此权限分组不存在，不能添加任何权限");
                }

                List<Admin_Role_Action> role_actions=(from ra in db.Admin_Role_Action 
                                                      where ra.role_id==role_id 
                                                      select ra).ToList<Admin_Role_Action>();

                List<Admin_Action> all_actions=(from action in db.Admin_Action select action).ToList<Admin_Action>();

                //add new
                foreach (int action in actions)
                {
                    Admin_Action dbAction=(from dba in all_actions where dba.id==action select dba).FirstOrDefault<Admin_Action>();
                    //No action
                    if (dbAction == null)
                    {
                        continue;
                    }

                    Admin_Role_Action dbRoleAction=(from dbra in role_actions where dbra.role_id==role_id && dbra.action_id==action select dbra).FirstOrDefault<Admin_Role_Action>();
                    //the role already has the action
                    if (dbRoleAction != null)
                    {
                        continue;
                    }

                    dbRoleAction = new Admin_Role_Action();
                    dbRoleAction.action_id = action;
                    dbRoleAction.role_id = role_id;
                    db.Admin_Role_Action.Add(dbRoleAction);
                }

                //remove deleted
                foreach (Admin_Role_Action action in role_actions)
                {
                    if (!actions.Contains(action.action_id))
                    {
                        db.Admin_Role_Action.Remove(action);
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get all actions by category
        /// </summary>
        /// <param name="role_id"></param>
        /// <returns></returns>
        public List<BAdminCategoryAction> GetActionsByCategory(int role_id=0)
        {
            List<BAdminCategoryAction> actions = new List<BAdminCategoryAction>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Admin_Action> roleActions = new List<Admin_Action>();
                if (role_id > 0)
                {
                    var tmp = from roleA in db.Admin_Role_Action
                              where roleA.role_id == role_id
                              join action in db.Admin_Action on roleA.action_id equals action.id into lAction
                              from l_action in lAction.DefaultIfEmpty()
                              select l_action;

                    roleActions = (from action in tmp
                                  where action.enable == true
                                  select action).ToList<Admin_Action>();
                }
                List<BAdminCategory> categories = (from category in db.Admin_Category
                                                   select new BAdminCategory
                                                   {
                                                       ID = category.ID,
                                                       Created = category.Created,
                                                       Name = category.Name
                                                   }).ToList<BAdminCategory>();

                foreach (BAdminCategory category in categories)
                {
                    BAdminCategoryAction cateAction = new BAdminCategoryAction();
                    cateAction.Category = category;

                    cateAction.Actions = (from action in db.Admin_Action
                                          where action.category_id == category.ID && action.enable==true
                                          select new BAdminAction
                                          {
                                              ActionName = action.action_name,
                                              Description = action.action_description,
                                              ID = action.id,
                                              Enabled = action.enable
                                          }).ToList<BAdminAction>();

                    foreach (BAdminAction bAction in cateAction.Actions)
                    {
                        Admin_Action dbAction = (from action in roleActions where action.id == bAction.ID select action).FirstOrDefault<Admin_Action>();
                        if (dbAction != null)
                        {
                            bAction.HasPermission = true;
                        }
                    }

                    actions.Add(cateAction);
                }
            }
            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roles"></param>
        /// <param name="user_id"></param>
        public void UpdateUserRoles(int[] roles,int user_id)
        {
            if (this.CurrentUserPermission.UPDATE_USER_PERMISSION == 0)
            {
                throw new KMJXCException("没有权限执行此操作");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] userRoles=(from role in db.Admin_User_Role where role.user_id==user_id select role.role_id ).ToArray<int>();

                foreach (int role in roles)
                {
                    if (!userRoles.Contains(role))
                    {
                        Admin_User_Role uRole = new Admin_User_Role() { role_id=role, user_id=user_id };
                        db.Admin_User_Role.Add(uRole);
                    }
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BAdminRole> GetUserAdminRoles(int user_id,int shop_id=0)
        {
            if (this.CurrentUserPermission.VIEW_USER_PERMISSION == 0)
            {
                throw new KMJXCException("没有权限查看用户权限");
            }

            int shopId=this.Shop.Shop_ID;
            if(shop_id>0)
            {
                shopId=shop_id;
            }

            List<BAdminRole> roles = new List<BAdminRole>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Admin_Action> allActions=(from action in db.Admin_Action select action).ToList<Admin_Action>();
                int[] role_ids=(from role in db.Admin_Role where role.shop_id==shopId && role.enabled==true select role.id).ToArray<int>();
                List<Admin_Role_Action> role_Actions=(from roleAction in db.Admin_Role_Action where role_ids.Contains(roleAction.role_id) select roleAction).ToList<Admin_Role_Action>();
                var tmp = from user_role in db.Admin_User_Role
                          where user_role.user_id == user_id
                          join role in db.Admin_Role on user_role.role_id equals role.id into lRole
                          from l_role in lRole.DefaultIfEmpty()
                          join shop in db.Shop on l_role.shop_id equals shop.Shop_ID into lShop
                          from l_shop in lShop.DefaultIfEmpty()
                          select new BAdminRole
                          {
                              ID = user_role.role_id,
                              Name = l_role.role_name,
                              Description=l_role.description,
                              Shop = new BShop
                              {
                                   ID=l_shop.Shop_ID,
                                   Title=l_shop.Name
                              }
                          };

                roles = tmp.ToList<BAdminRole>();

                foreach (BAdminRole role in roles)
                {
                    var actions = from action in role_Actions
                                  where action.role_id == role.ID
                                  join action1 in allActions on action.action_id equals action1.id into lAction1
                                  from l_action1 in lAction1.DefaultIfEmpty()
                                  select new BAdminAction
                                  {
                                      ActionName = l_action1.action_name,
                                      Description = l_action1.action_description,
                                      ID = action.action_id
                                  };

                    role.Actions = actions.ToList<BAdminAction>();
                }
            }
            return roles;
        }

        public void SetAdminRoleStatus(int role_id, bool status)
        {
            if (this.CurrentUserPermission.UPDATE_ADMIN_ROLE == 0)
            {
                throw new KMJXCException("没有权限更新分组状态");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Admin_Role role=(from r in db.Admin_Role where r.id==role_id select r).FirstOrDefault<Admin_Role>();
                if(role==null)
                {
                    throw new KMJXCException("分组信息不存在");
                }

                role.enabled=status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Sync actions with Permission object
        /// </summary>
        public static void SyncPermissionWithAction()
        {
            Type permission = typeof(Permission);
            FieldInfo[] fields = permission.GetFields();
            if (fields == null || fields.Length <= 0)
            {
                return;
            }

            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                List<AdminActionAttribute> cates=new List<AdminActionAttribute>();
                List<Admin_Action> allActions=(from action in db.Admin_Action select action).ToList<Admin_Action>();
                List<Admin_Category> allCates=(from cate in db.Admin_Category select cate).ToList<Admin_Category>();
                foreach (FieldInfo field in fields)
                {
                    AdminActionAttribute attr = field.GetCustomAttribute<AdminActionAttribute>(); 
                    Admin_Action action = (from a in allActions where a.action_name == field.Name select a).FirstOrDefault<Admin_Action>();
                    if (action == null)
                    {
                        action = new Admin_Action();
                        action.action_name = field.Name;
                        //action.action_description = field.Name;
                        action.enable = true;
                                             

                        db.Admin_Action.Add(action);
                    }

                    if (attr != null)
                    {
                        action.category_id = attr.ID;
                        action.action_description = attr.ActionDescription;
                        AdminActionAttribute existed = (from pcate in cates where pcate.ID == attr.ID select pcate).FirstOrDefault<AdminActionAttribute>();
                        if (existed == null)
                        {
                            cates.Add(attr);
                        }
                    }
                }
                db.SaveChanges();

                foreach (Admin_Action action in allActions)
                {
                    bool found = false;

                    foreach (FieldInfo field in fields)
                    {
                        if (action.action_name == field.Name)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        action.enable = false;
                        //db.Admin_Action.Remove(action);
                    }
                }

                db.SaveChanges();

                //category
                foreach (AdminActionAttribute pcate in cates)
                {
                    Admin_Category dbCate=(from c in allCates where c.ID==pcate.ID select c).FirstOrDefault<Admin_Category>();
                    if (dbCate == null)
                    {
                        dbCate = new Admin_Category();
                        dbCate.ID = pcate.ID;
                        dbCate.Name = pcate.CategoryName;
                        dbCate.System_category = pcate.IsSystem;
                        dbCate.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        db.Admin_Category.Add(dbCate);                        
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                db.Dispose();
            }
        }
    }
}
