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
        public void CreateRole(string name,string desc,int shop_id=0)
        {
            if (this.CurrentUserPermission.ADD_ADMIN_ROLE == 0)
            {
                throw new KMJXCException("没有权限创建权限分组");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int shop=this.Shop.Shop_ID;
                if(shop_id>0)
                {
                    shop=shop_id;
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
                db.Admin_Role.Add(admin_role);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<Admin_Role> GetAdminRoles(int shop_id=0)
        {
            List<Admin_Role> roles = new List<Admin_Role>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int shop=this.Shop.Shop_ID;
                if(shop_id>0)
                {
                    shop=shop_id;
                }
                roles=(from role in db.Admin_Role where role.shop_id==shop select role).ToList<Admin_Role>();
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

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BAdminCategoryAction> GetActionsByCategory()
        {
            List<BAdminCategoryAction> actions = new List<BAdminCategoryAction>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
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
                                          where action.category_id == category.ID
                                          select new BAdminAction
                                          {
                                              Action = action.action_name,
                                              Description = action.action_description,
                                              ID = action.id,
                                              Enabled = action.enable
                                          }).ToList<BAdminAction>();

                    actions.Add(cateAction);
                }
            }
            return actions;
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
                List<PCategory> cates=new List<PCategory>();
                List<Admin_Action> allActions=(from action in db.Admin_Action select action).ToList<Admin_Action>();
                List<Admin_Category> allCates=(from cate in db.Admin_Category select cate).ToList<Admin_Category>();
                foreach (FieldInfo field in fields)
                {
                    PCategory attr = field.GetCustomAttribute<PCategory>(); 
                    Admin_Action action = (from a in allActions where a.action_name == field.Name select a).FirstOrDefault<Admin_Action>();
                    if (action == null)
                    {
                        action = new Admin_Action();
                        action.action_name = field.Name;
                        action.action_description = field.Name;
                        action.enable = true;
                                             

                        db.Admin_Action.Add(action);
                    }

                    if (attr != null)
                    {
                        action.category_id = attr.ID;
                        PCategory existed = (from pcate in cates where pcate.ID == attr.ID select pcate).FirstOrDefault<PCategory>();
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
                        db.Admin_Action.Remove(action);
                    }
                }

                db.SaveChanges();

                //category
                foreach (PCategory pcate in cates)
                {
                    Admin_Category dbCate=(from c in allCates where c.ID==pcate.ID select c).FirstOrDefault<Admin_Category>();
                    if (dbCate == null)
                    {
                        dbCate = new Admin_Category();
                        dbCate.ID = pcate.ID;
                        dbCate.Name = pcate.Name;
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
