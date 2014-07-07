using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data.Entity;
using System.Data.Entity.Validation;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
using KM.JXC.Common.Util;
namespace KM.JXC.BL
{
    public class UserActionLogManager
    {
        public BUser CurrentUser { get; private set; }

        public UserActionLogManager(BUser user)
        {
            this.CurrentUser = user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void CreateActionLog(BUserActionLog action)
        {
            if (action == null || action.Shop==null)
            {
                return;
            }
            KuanMaiEntities db = null;
            try
            {
                db = new KuanMaiEntities();
                User_Action_Log log = new User_Action_Log();

                log.Action = action.Action.Action_ID;

                log.Description = action.Description;
                if (action.User != null && action.User.ID > 0)
                {
                    log.User_ID = action.User.ID;
                }
                else
                {
                    log.User_ID = this.CurrentUser.ID;
                }

                log.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                if (string.IsNullOrEmpty(log.Description))
                {
                    log.Description = "";
                }
                log.Shop_ID = action.Shop.ID;
                db.User_Action_Log.Add(log);
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

            }
            catch (Exception ex)
            {

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
        /// <param name="user_id"></param>
        /// <param name="action"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<BUserActionLog> SearchUserActionLog(int user_id, int action, long date1,long date2, int page, int pageSize, out int total)
        {
            total = 0;
            List<BUserActionLog> actions = new List<BUserActionLog>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from log in db.User_Action_Log
                          select log;

                if (user_id > 0)
                {
                    tmp = tmp.Where(l => l.User_ID == user_id);
                }

                if (action > 0)
                {
                    tmp = tmp.Where(l => l.Action == action);
                }

                if (page <= 0)
                {
                    page = 1;
                }

                if (pageSize <= 0)
                {
                    pageSize = 30;
                }

                if (date1 > 0)
                {
                    tmp = tmp.Where(l => l.Created >= date1);
                }

                if (date2 > 0)
                {
                    tmp = tmp.Where(l => l.Created <= date2);
                }

                total = tmp.Count();
                actions = (from log in tmp
                           join ac in db.User_Action on log.Action equals ac.Action_ID into LAction
                           from l_ac in LAction.DefaultIfEmpty()
                           join user in db.User on log.User_ID equals user.User_ID into LUser
                           from l_user in LUser.DefaultIfEmpty()
                           join shop in db.Shop on log.Shop_ID equals shop.Shop_ID into LShop
                           from l_shop in LShop.DefaultIfEmpty()
                           select new BUserActionLog
                           {
                               Action = new BUserAction
                               {
                                   Action_Desc = l_ac.Action_Description,
                                   Action_ID = log.Action,
                                   Action_Name = l_ac.Action_Name,
                                   Created = l_ac.Created,
                                   ID = l_ac.ID
                               },
                               Created = log.Created,
                               Description = log.Description,
                               ID = log.ID,
                               User = new BUser
                               {
                                   ID = log.User_ID,
                                   Name = l_user.Name,
                                   Mall_ID = l_user.Mall_ID,
                                   Mall_Name = l_user.Mall_Name
                               },
                               Shop = l_shop != null ?
                               new BShop
                               {
                                   ID = log.Shop_ID,
                                   Title = l_shop.Name
                               } : new BShop
                               {
                                   ID = 0,
                                   Title = ""
                               }
                           }).OrderByDescending(a => a.Created).Skip((page - 1) * pageSize).Take(pageSize).ToList<BUserActionLog>();
            }

            return actions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BUserAction> GetActions()
        {
            List<BUserAction> actions = new List<BUserAction>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                actions = (from ac in db.User_Action orderby ac.Action_Description select new BUserAction
                {
                     Action_Desc=ac.Action_Description,
                     Action_ID=ac.Action_ID,
                     Created=ac.Created
                }).ToList<BUserAction>();
            }
            return actions;
        }
    }
}
