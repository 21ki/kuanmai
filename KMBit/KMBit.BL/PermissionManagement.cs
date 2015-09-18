using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using log4net;
using KMBit.DAL;

namespace KMBit.BL
{
    public class PermissionManagement
    {        
        public static void SyncPermissionsWithDB()
        {
            log4net.ILog logger = log4net.LogManager.GetLogger(typeof(PermissionManagement));
            KMBit.DAL.chargebitEntities db = new chargebitEntities();
            try
            {
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
                    Admin_Actions action = (from a in allActions where a.action_name == field.Name select a).FirstOrDefault<Admin_Action>();
                    if (action == null)
                    {
                        action = new Admin_Action();
                        action.action_name = field.Name;
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
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }
    }
}
