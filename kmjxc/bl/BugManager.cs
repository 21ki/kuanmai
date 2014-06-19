using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.BL.Models;
using KM.JXC.BL.Models.Admin;
using KM.JXC.DBA;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;
namespace KM.JXC.BL
{
    public class BugManager
    {
        private BUser currentUser = null;

        public BUser CurrentUser
        {
            get { return currentUser; }
        }

        public BugManager(BUser user)
        {
            this.currentUser = user;
        }

        public BugManager(int user_id)
        {
            GetUser(user_id);
        }

        private void GetUser(int uid)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                this.currentUser = (from user in db.User 
                                    where user.User_ID == uid 
                                    select new BUser
                                    {
                                        ID=user.User_ID,
                                        Mall_Name=user.Mall_Name,
                                        Name=user.Name
                                    }).FirstOrDefault<BUser>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bug"></param>
        /// <returns></returns>
        public bool CreateNewBug(BBug bug)
        {
            if (bug.Created_By == null)
            {
                throw new KMJXCException("创建Bug时必须有创建人");
            }
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Bug dbBug = new Bug();
                dbBug.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                if (bug.Created_By != null)
                {
                    dbBug.Created_By = bug.Created_By.ID;
                }
                dbBug.Description = bug.Description;
                if (bug.Feature != null)
                {
                    dbBug.Feature = bug.Feature.ID;
                }
                dbBug.Function = 0;
                dbBug.Modified = dbBug.Created;
                dbBug.Modified_By = dbBug.Created_By;
                dbBug.Resolved = 0;
                dbBug.Resolved_By = 0;
                dbBug.Status = 1;
                dbBug.Title = bug.Title;
                db.Bug.Add(dbBug);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bug"></param>
        /// <returns></returns>
        public bool UpdateBug(BBug bug)
        {
            if (bug.Modified_By == null)
            {
                throw new KMJXCException("更新Bug时必须有更新人");
            }
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Bug existed=(from b in db.Bug where b.ID==bug.ID select b).FirstOrDefault<Bug>();
                if (existed == null)
                {
                    throw new KMJXCException("所需更新的Bug信息不存在"); 
                }

                if (bug.Status != null)
                {
                    existed.Status = bug.Status.ID;                  
                }

                if (bug.Modified_By != null)
                {
                    existed.Modified_By = bug.Modified_By.ID;
                    existed.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                }
                if (!string.IsNullOrEmpty(bug.Title))
                {
                    existed.Title = bug.Title;
                }

                if (!string.IsNullOrEmpty(bug.Description))
                {
                    existed.Description = bug.Description;
                }
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bugId"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public bool ResolveBug(int bugId,int user_id)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Bug existed = (from b in db.Bug where b.ID == bugId select b).FirstOrDefault<Bug>();
                if (existed == null)
                {
                    throw new KMJXCException("所需更新的Bug信息不存在");
                }

                existed.Status = 6;
                existed.Resolved = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                existed.Resolved_By = user_id;
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bug_id"></param>
        /// <returns></returns>
        public BBug GetBugInfo(int bug_id)
        {
            BBug bug = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from b in db.Bug
                          join feature in db.Bug_Feature on b.Feature equals feature.ID into LFeature
                          from l_feature in LFeature.DefaultIfEmpty()
                          join user in db.User on b.Created_By equals user.User_ID into lCreatedBy
                          from createdBy in lCreatedBy.DefaultIfEmpty()
                          join user_r in db.User on b.Resolved_By equals user_r.User_ID into LResolved
                          from resolved in LResolved.DefaultIfEmpty()
                          join status in db.Bug_Status on b.Status equals status.ID into LStatus
                          from l_status in LStatus.DefaultIfEmpty()
                          where b.ID==bug_id
                          select new BBug
                          {
                              Created = b.Created,
                              Created_By = new BUser
                              {
                                  ID = createdBy.User_ID,
                                  Name = createdBy.Name,
                                  Mall_Name = createdBy.Mall_Name
                              },
                              Description = b.Description,
                              Title = b.Title,
                              Status = new BBugStatus
                              {
                                  ID = l_status.ID,
                                  Name = l_status.Status
                              },
                              ID = b.ID,
                              Feature = new BBugFeature
                              {
                                  ID = l_feature.ID,
                                  Name = l_feature.Description
                              },
                              Modified = (long)b.Modified,
                              Resolved_By = resolved != null ? new BUser
                              {
                                  ID = resolved.User_ID,
                                  Name = resolved.Name,
                                  Mall_Name = resolved.Mall_Name
                              } : new BUser
                              {
                                  ID = 0,
                                  Name = "",
                                  Mall_Name = ""
                              },
                          };

                bug = tmp.FirstOrDefault<BBug>();
                if (bug != null)
                {
                    var tmpRes = from bs in db.Bug_Response
                                 join user in db.User on bs.Create_By equals user.User_ID into LUser
                                 from l_user in LUser.DefaultIfEmpty()
                                 where bs.BugID==bug_id
                                 select new BBugResponse
                                 {
                                     Created = bs.Created,
                                     Created_By = l_user != null ? new BUser
                                     {
                                         ID = l_user.User_ID,
                                         Name = l_user.Name,
                                         Mall_Name = l_user.Mall_Name
                                     } : new BUser 
                                     {
                                         ID = 0,
                                         Name = "",
                                         Mall_Name = ""
                                     },
                                     Description = bs.Description,
                                     ID = bs.ID
                                 };

                    bug.Responses = tmpRes.ToList<BBugResponse>();
                }
            }
            return bug;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void CreateNewResponse(int bugId,string response)
        {
            if (response == null)
            {
                throw new KMJXCException("问题创建失败");
            }

            if (bugId <= 0)
            {
                throw new KMJXCException("问题创建失败");
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Bug_Response resp = new Bug_Response();

                resp.BugID = bugId;
                resp.Create_By = this.currentUser.ID;
                resp.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                resp.Description = response;

                db.Bug_Response.Add(resp);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="feature_id"></param>
        /// <param name="status_id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<BBug> SearchBugs(int user_id,int feature_id,int status_id,int page,int pageSize,out int total)
        {
            total = 0;
            List<BBug> bugs = null;
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize < 0)
            {
                pageSize = 20;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from bug in db.Bug
                          select bug;

                if (user_id > 0)
                {
                    tmp = tmp.Where(b => b.Created_By == user_id);
                }

                if (feature_id > 0)
                {
                    tmp = tmp.Where(b => b.Feature == feature_id);
                }

                if (status_id > 0)
                {
                    tmp = tmp.Where(b => b.Status == status_id);
                }

                var tmpBugs = from bug in tmp
                              join user in db.User on bug.Created_By equals user.User_ID into LUser
                              from createdBy in LUser.DefaultIfEmpty()
                              join feature in db.Bug_Feature on bug.Feature equals feature.ID into LFeature
                              from l_feature in LFeature.DefaultIfEmpty()
                              join user1 in db.User on bug.Resolved_By equals user1.User_ID into LUser1
                              from resolved in LUser1.DefaultIfEmpty()
                              join status in db.Bug_Status on bug.Status equals status.ID into LStatus
                              from l_status in LStatus.DefaultIfEmpty()
                              select new BBug
                              {
                                  Created = bug.Created,
                                  Description = bug.Description,
                                  Title = bug.Title,
                                  ID = bug.ID,
                                  Modified = (long)bug.Modified,
                                  Created_By = new BUser
                                  {
                                      ID = createdBy.User_ID,
                                      Name = createdBy.Name,
                                      Mall_Name = createdBy.Mall_Name
                                  },
                                  Feature = new BBugFeature
                                  {
                                     ID=l_feature.ID,
                                     Name=l_feature.Description
                                  },
                                  Resolved_By = resolved != null ? new BUser
                                  {
                                      ID = resolved.User_ID,
                                      Name = resolved.Name,
                                      Mall_Name = resolved.Mall_Name
                                  } : new BUser
                                  {
                                      ID = 0,
                                      Name = "",
                                      Mall_Name = ""
                                  },
                                  Status = new BBugStatus
                                  {
                                      ID = l_status.ID,
                                      Name = l_status.Status
                                  },
                                  Resolved=bug.Resolved!=null?(long)bug.Resolved:0
                              };

                total = tmpBugs.Count();
                bugs = tmpBugs.OrderBy(b=>b.ID).Skip((page-1)*pageSize).Take(pageSize).ToList<BBug>();
            }
            return bugs;
        }

        public List<BBugStatus> GetBugStatuses()
        {
            List<BBugStatus> statuses = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                statuses = (from status in db.Bug_Status 
                            select new BBugStatus 
                            {
                                 ID=status.ID,
                                 Name=status.Status
                            }).ToList<BBugStatus>();
            }
            return statuses;
        }

        public List<BBugFeature> GetBugFeatures()
        {
            List<BBugFeature> features = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                features = (from status in db.Bug_Feature
                            select new BBugFeature
                            {
                                ID = status.ID,
                                Name = status.Description
                            }).ToList<BBugFeature>();
            }
            return features;
        }
    }
}
