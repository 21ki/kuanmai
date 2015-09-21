using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using log4net;
using KMBit.DAL;
using KMBit.Beans;
namespace KMBit.BL
{
    public class PermissionManagement:BaseManagement
    { 
        public PermissionManagement(int userId):base(userId)
        {
            if(this.logger==null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(PermissionManagement));
            }            
        }

        /// <summary>
        /// Sync database user permission actions with the definitions of Permissions object
        /// </summary>
        public void SyncPermissionsWithDB()
        {
            if(logger==null)
            {
                logger = log4net.LogManager.GetLogger(typeof(PermissionManagement));
            }
            
            KMBit.DAL.chargebitEntities db = null;            
            try
            {
                db = new chargebitEntities();
                db.Configuration.AutoDetectChangesEnabled = false;
                List<AdminActionAttribute> cates = new List<AdminActionAttribute>();
                List<Admin_Actions> allActions = (from action in db.Admin_Actions select action).ToList<Admin_Actions>();
                List<Admin_Categories> allCates = (from cate in db.Admin_Categories select cate).ToList<Admin_Categories>();

                Type permission = typeof(Permissions);
                FieldInfo[] fields = permission.GetFields();
                if (fields == null || fields.Length <= 0)
                {
                    return;
                }

                foreach (FieldInfo field in fields)
                {
                    AdminActionAttribute attr = field.GetCustomAttribute<AdminActionAttribute>();
                    Admin_Actions action = (from a in allActions where a.Name == field.Name select a).FirstOrDefault<Admin_Actions>();
                    if (action == null)
                    {
                        action = new Admin_Actions();
                        action.Name = field.Name;
                        action.Enabled = true;
                        db.Admin_Actions.Add(action);
                    }

                    if (attr != null)
                    {
                        action.Category = attr.ID;
                        action.Description = attr.ActionDescription;
                    }
                    List<Admin_Categories> categories = (from cate in allCates where cate.Id == attr.ID select cate).ToList<Admin_Categories>();
                    if (categories == null || categories.Count == 0)
                    {
                        Admin_Categories newCate = new Admin_Categories() { Id = attr.ID, Name = attr.CategoryName };
                        db.Admin_Categories.Add(newCate);
                        allCates.Add(newCate);
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public bool GrantUserPermissions(int userId,List<Admin_Actions> actions)
        {
            bool ret = false;
            using (chargebitEntities db = new chargebitEntities())
            {                
                if (actions != null && actions.Count>0)
                {
                    db.Database.ExecuteSqlCommand("delete from Admin_Users_Actions where User_Id=" + userId.ToString());
                    foreach(Admin_Actions action in actions)
                    {
                        Admin_Users_Actions uaction = new Admin_Users_Actions() { Action_Id = action.Id,User_Id=userId };
                        db.Admin_Users_Actions.Add(uaction);
                    }
                    db.SaveChanges();
                    ret = true;
                }               
            }
            return ret;
        }

        /// <summary>
        /// Gets user actions list
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="categoryId"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public List<UserAdminAction> GetUserPermissionActions(int userId, int categoryId, string categoryName)
        {
            List<UserAdminAction> actions = new List<UserAdminAction>();
            using (chargebitEntities db = new chargebitEntities())
            {
                List<Admin_Categories> allcates = (from cate in db.Admin_Categories orderby cate.Id select cate).ToList<Admin_Categories>();
                var userActions = from ua in db.Admin_Users_Actions
                                  join action in db.Admin_Actions on ua.Action_Id equals action.Id into laction
                                  from uaction in laction.DefaultIfEmpty()
                                  join cate in db.Admin_Categories on uaction.Category equals cate.Id into lcate
                                  from ucate in lcate.DefaultIfEmpty()
                                  select new
                                  {
                                      userId = ua.User_Id,
                                      categoryId = uaction.Id,
                                      categoryName = ucate.Name,
                                      actionId = ua.Action_Id,
                                      actionName = uaction.Name,
                                      actionDesc = uaction.Description
                                  };
                if (userId > 0)
                {
                    userActions = userActions.Where(u => u.userId == userId);
                }

                if (categoryId > 0)
                {
                    userActions = userActions.Where(u => u.categoryId == categoryId);
                }

                if(!string.IsNullOrEmpty(categoryName))
                {
                    userActions = userActions.Where(u => u.categoryName == categoryName);
                }

                foreach (var uaction in userActions)
                {
                    UserAdminAction uaa = new UserAdminAction();
                    uaa.Action = new Admin_Actions() { Id = uaction.actionId, Name = uaction.actionName, Description = uaction.actionDesc, Enabled=true };
                    uaa.Category = new Admin_Categories() { Id= uaction.categoryId, Name= uaction.categoryName };
                    actions.Add(uaa);
                }

            }

            return actions;
        }

        /// <summary>
        /// Gets permission categories
        /// </summary>
        /// <param name="id">Query by category Id</param>
        /// <param name="name">Query by category name</param>
        /// <returns>A list of PermissionCategory</returns>
        public List<PermissionCategory> GetPermissionCategories(int id, string name)
        {
            List<PermissionCategory> categories = new List<PermissionCategory>();

            using (chargebitEntities db = new chargebitEntities())
            {
                var cates = from cate in db.Admin_Categories select new PermissionCategory { CategoryDescription=cate.Description,CategoryId=cate.Id, CategoryName=cate.Name };
                if(id>0)
                {
                    cates = cates.Where(c => c.CategoryId == id);
                }
                if(!string.IsNullOrEmpty(name))
                {
                    cates = cates.Where(c => c.CategoryName == name);
                }
                categories = cates.ToList<PermissionCategory>();
            }

            return categories;
        }

        /// <summary>
        /// Gets single user permissions object
        /// </summary>
        /// <param name="userId">User Id of user</param>
        /// <returns>Instance of Permissions object</returns>
        public static Permissions GetUserPermissions(int userId)
        {
            Permissions permissions = new Permissions();
            FieldInfo[] fields = permissions.GetType().GetFields();
            KMBit.DAL.chargebitEntities db = null;
            try
            {
                db = new chargebitEntities();
                List<Admin_Actions> actions = (from a in db.Admin_Actions select a).ToList<Admin_Actions>();
                List<Admin_Users_Actions> userActions = (from ua in db.Admin_Users_Actions where ua.User_Id == userId select ua).ToList<Admin_Users_Actions>();
                if (userActions != null && userActions.Count > 0)
                {
                    foreach (Admin_Users_Actions ua in userActions)
                    {
                        Admin_Actions action = (from a in actions where a.Id == ua.Action_Id select a).FirstOrDefault<Admin_Actions>();
                        if (action != null)
                        {
                            foreach (FieldInfo f in fields)
                            {
                                if (f.Name == action.Name)
                                {
                                    f.SetValue(permissions, 1);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
            return permissions;
        }
    }
}
