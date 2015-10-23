using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
using log4net;
namespace KMBit.BL.Admin
{
    public class ResourceManagement : BaseManagement
    {
        public ResourceManagement(int userId) : base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public ResourceManagement(string email) : base(email)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public ResourceManagement(BUser user) : base(user)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public IQueryable<BResource> FindResourcesE(int resourceId, string resourceName, int spId)
        {
            using (chargebitEntities db = new chargebitEntities())
            {
                var tmp = from s in db.Resource
                          join sp in db.Sp on s.SP_Id equals sp.Id into lsp
                          from llsp in lsp.DefaultIfEmpty()
                          join pa in db.Area on s.Province_Id equals pa.Id into lpa
                          from llpa in lpa.DefaultIfEmpty()
                          join ca in db.Area on s.City_Id equals ca.Id into lca
                          from llca in lca.DefaultIfEmpty()
                          join cu in db.Users on s.CreatedBy equals cu.Id into lcu
                          from llcu in lcu.DefaultIfEmpty()
                          join uu in db.Users on s.UpdatedBy equals uu.Id into luu
                          from lluu in luu.DefaultIfEmpty()
                          select new BResource
                          {
                              Resource = s,
                              City = llca,
                              Province = llpa,
                              SP = llsp,
                              CreatedBy = llcu,
                              UpdatedBy = lluu
                          };
                if (spId > 0)
                {
                    tmp = tmp.Where(s => s.Resource.SP_Id == spId);
                }
                if (resourceId > 0)
                {
                    tmp = tmp.Where(s => s.Resource.Id == resourceId);
                }
                if (!string.IsNullOrEmpty(resourceName))
                {
                    tmp = tmp.Where(s => s.Resource.Name.Contains(resourceName));
                }
                tmp.OrderBy(s => s.Resource.Created_time);
                return tmp;
            }
        }

        public List<BResource> FindResources(int resourceId, string resourceName, int spId, out int total, int page = 1, int pageSize = 20, bool paging = false)
        {
            total = 0;
            List<BResource> resources = null;
            int skip = (page - 1) * pageSize;
            chargebitEntities db = new chargebitEntities();
            try
            {
                var tmp = from s in db.Resource
                          join sp in db.Sp on s.SP_Id equals sp.Id into lsp
                          from llsp in lsp.DefaultIfEmpty()
                          join pa in db.Area on s.Province_Id equals pa.Id into lpa
                          from llpa in lpa.DefaultIfEmpty()
                          join ca in db.Area on s.City_Id equals ca.Id into lca
                          from llca in lca.DefaultIfEmpty()
                          join cu in db.Users on s.CreatedBy equals cu.Id into lcu
                          from llcu in lcu.DefaultIfEmpty()
                          join uu in db.Users on s.UpdatedBy equals uu.Id into luu
                          from lluu in luu.DefaultIfEmpty()
                          select new BResource
                          {
                              Resource = s,
                              City = llca,
                              Province = llpa,
                              SP = llsp,
                              CreatedBy = llcu,
                              UpdatedBy = lluu
                          };
                if (spId > 0)
                {
                    tmp = tmp.Where(s => s.Resource.SP_Id == spId);
                }
                if (resourceId > 0)
                {
                    tmp = tmp.Where(s => s.Resource.Id == resourceId);
                }
                if (!string.IsNullOrEmpty(resourceName))
                {
                    tmp = tmp.Where(s => s.Resource.Name.Contains(resourceName));
                }

                total = tmp.Count();
                if (paging)
                {
                    tmp = tmp.OrderBy(s => s.Resource.Created_time).Skip(skip).Take(pageSize);
                }

                resources = tmp.ToList<BResource>();
            }catch(Exception ex)
            {

            }finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }

            return resources;
        }

        public bool CreateResource(Resource resource)
        {
            bool ret = false;

            if (!CurrentLoginUser.Permission.CREATE_RESOURCE)
            {
                throw new KMBitException("没有权限创建资源");
            }

            if (resource == null)
            {
                logger.Error("resource is NULL");
                throw new KMBitException("资源输入参数不正确");
            }

            if (string.IsNullOrEmpty(resource.Name))
            {
                logger.Error("resource name cannot be empty");
                throw new KMBitException("资源名称不能为空");
            }
            int total = 0;
            List<BResource> existResources = FindResources(0, resource.Name, 0, out total);
            if (existResources != null && existResources.Count > 0)
            {
                logger.Error(string.Format("Resource name:{0} is already existed", resource.Name));
                throw new KMBitException(string.Format("资源名称:{0} 已经存在", resource.Name));
            }

            resource.Enabled = true;
            resource.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            resource.Updated_time = resource.Created_time;
            using (chargebitEntities db = new chargebitEntities())
            {
                db.Resource.Add(resource);
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public bool UpdateResource(Resource resource)
        {
            if (!CurrentLoginUser.Permission.UPDATE_RESOURCE)
            {
                throw new KMBitException("没有权限更新资源");
            }
            bool ret = false;
            if (resource == null)
            {
                logger.Error("resource is NULL");
                throw new KMBitException("资源输入参数不正确");
            }
            if (resource.Id <= 0)
            {
                throw new KMBitException("资源输入参数不正确");
            }
            if (string.IsNullOrEmpty(resource.Name))
            {
                logger.Error("resource name cannot be empty");
                throw new KMBitException("资源名称不能为空");
            }
            using (chargebitEntities db = new chargebitEntities())
            {
                Resource dbResource = (from r in db.Resource where r.Id == resource.Id select r).FirstOrDefault<Resource>();
                //db.Resource.Attach(dbResource);  
                SyncObjectProperties(dbResource, resource);
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public bool UpdateResourceTaocan(Resource_taocan taocan)
        {
            bool ret = false;
            if (!CurrentLoginUser.Permission.UPDATE_RESOURCE_TAOCAN)
            {
                throw new KMBitException("没有权限更新资源套餐");
            }
            if (taocan == null || taocan.Id <= 0)
            {
                throw new KMBitException("输入数据不正确，不能更新套餐");
            }


            using (chargebitEntities db = new chargebitEntities())
            {
                Resource_taocan dbTaocan = (from t in db.Resource_taocan where t.Id == taocan.Id select t).FirstOrDefault<Resource_taocan>();
                if (dbTaocan == null)
                {
                    throw new KMBitException("套餐不存在不能更新");
                }
                SyncObjectProperties(dbTaocan, taocan);
                db.SaveChanges();
                ret = true;
            }
            return ret;
        }

        public bool CreateResourceTaocan(Resource_taocan taocan)
        {
            bool ret = false;
            if (!CurrentLoginUser.Permission.CREATE_RESOURCE_TAOCAN)
            {
                throw new KMBitException("没有权限创建资源套餐");
            }

            if (taocan.Quantity <= 0)
            {
                throw new KMBitException("套餐容量不能为零");
            }

            if (taocan.Resource_id <= 0)
            {
                throw new KMBitException("套餐资源信息不能为空");
            }
            int total = 0;
            List<BResource> resources = FindResources(taocan.Resource_id, null, 0, out total);
            if (total == 0)
            {
                throw new KMBitException("资源编号为:" + taocan.Resource_id + " 的资源不存在");
            }
            using (chargebitEntities db = new chargebitEntities())
            {
                Taocan ntaocan = (from t in db.Taocan where t.Sp_id == taocan.Sp_id && t.Quantity == taocan.Quantity select t).FirstOrDefault<Taocan>();
                Sp sp = (from s in db.Sp where s.Id == taocan.Sp_id select s).FirstOrDefault<Sp>();
                if (ntaocan == null)
                {
                    string taocanName = sp != null ? sp.Name + " " + taocan.Quantity.ToString() + "M" : "全网 " + taocan.Quantity.ToString() + "M";
                    ntaocan = new Taocan() { Created_time = taocan.Created_time, Description = taocanName, Name = taocanName, Sp_id = taocan.Sp_id, Quantity = taocan.Quantity, Updated_time = taocan.Updated_time };
                    db.Taocan.Add(ntaocan);
                    db.SaveChanges();
                }
                if (ntaocan.Id > 0)
                {
                    taocan.Taocan_id = ntaocan.Id;
                    db.Resource_taocan.Add(taocan);
                    db.SaveChanges();
                    ret = true;
                }
                else
                {
                    throw new KMBitException("套餐创建失败");
                }

            }
            return ret;
        }

        public List<BResourceTaocan> FindResourceTaocans(int sTaocanId, int resourceId, int spId, out int total, int page = 1, int pageSize = 25, bool paging = false)
        {
            total = 0;
            List<BResourceTaocan> sTaocans = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var tmp = from rta in db.Resource_taocan
                          join r in db.Resource on rta.Resource_id equals r.Id
                          join cu in db.Users on rta.CreatedBy equals cu.Id into lcu
                          from llcu in lcu.DefaultIfEmpty()
                          join uu in db.Users on rta.UpdatedBy equals uu.Id into luu
                          from lluu in luu.DefaultIfEmpty()
                          join city in db.Area on rta.Area_id equals city.Id into lcity
                          from llcity in lcity.DefaultIfEmpty()
                          join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                          from llsp in lsp.DefaultIfEmpty()
                          join tt in db.Taocan on rta.Taocan_id equals tt.Id
                         
                          select new BResourceTaocan
                          {
                              Taocan = rta,
                              Taocan2 = tt,
                              CreatedBy = llcu,
                              UpdatedBy = lluu,
                              Province = llcity,
                              SP = llsp,
                              Resource = new BResource() { Resource = r }
                          };

                if (sTaocanId > 0)
                {
                    tmp = tmp.Where(r => r.Taocan.Id == sTaocanId);
                }
                if (resourceId > 0)
                {
                    tmp = tmp.Where(r => r.Taocan.Resource_id == resourceId);
                }
                if (spId > 0)
                {
                    tmp = tmp.Where(r => r.Taocan.Sp_id == spId);
                }
                total = tmp.Count();
                if (paging)
                {
                    tmp = tmp.OrderBy(t => t.Taocan.Sp_id).ThenBy(t=>t.Taocan.Quantity).Skip((page - 1) * pageSize).Take(pageSize);
                }else
                {
                    tmp = tmp.OrderBy(t => t.Taocan.Sp_id).ThenBy(t => t.Taocan.Quantity);
                }

                sTaocans = tmp.ToList<BResourceTaocan>();
                foreach (BResourceTaocan t in sTaocans)
                {
                    if (t.SP == null)
                    {
                        t.SP = new Sp { Id = 0, Name = "全网" };
                    }

                    if (t.Province == null)
                    {
                        t.Province = new Area { Id = 0, Name = "全国" };
                    }
                }
            }

            return sTaocans;
        }

        public List<BResourceTaocan> FindEnabledResourceTaocansForAgent(int resourceId, int agencyId)
        {
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            using (chargebitEntities db = new chargebitEntities())
            {
                List<int> existedIds = (from au in db.Agent_route where au.User_id==agencyId && au.Resource_Id==resourceId select au.Resource_taocan_id).ToList<int>();
                var query = from rta in db.Resource_taocan                           
                            join r in db.Resource on rta.Resource_id equals r.Id into lr
                            from llr in lr.DefaultIfEmpty()
                            join cu in db.Users on rta.CreatedBy equals cu.Id into lcu
                            from llcu in lcu.DefaultIfEmpty()
                            join uu in db.Users on rta.UpdatedBy equals uu.Id into luu
                            from lluu in luu.DefaultIfEmpty()
                            join city in db.Area on rta.Area_id equals city.Id into lcity
                            from llcity in lcity.DefaultIfEmpty()
                            join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            join tt in db.Taocan on rta.Taocan_id equals tt.Id
                            where rta.Enabled == true && rta.Resource_id == resourceId && !existedIds.Contains(rta.Id)
                            select new BResourceTaocan
                            {
                                Taocan = rta,
                                Taocan2 = tt,
                                CreatedBy = llcu,
                                UpdatedBy = lluu,
                                Province = llcity,
                                SP = llsp,
                                Resource = new BResource() { Resource = llr }
                            };

                taocans = query.ToList<BResourceTaocan>();
            }
            return taocans;
        }

        public List<BResourceTaocan> FindResourceTaocans(int resourceId, int agencyId,bool availabled=true)
        {
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            using (chargebitEntities db = new chargebitEntities())
            {
                if (resourceId > 0 && agencyId > 0)
                {
                    var query = from rta in db.Resource_taocan
                                join au in db.Agent_route on rta.Id equals au.Resource_taocan_id
                                join r in db.Resource on rta.Resource_id equals r.Id
                                join cu in db.Users on rta.CreatedBy equals cu.Id into lcu
                                from llcu in lcu.DefaultIfEmpty()
                                join uu in db.Users on rta.UpdatedBy equals uu.Id into luu
                                from lluu in luu.DefaultIfEmpty()
                                join city in db.Area on rta.Area_id equals city.Id into lcity
                                from llcity in lcity.DefaultIfEmpty()
                                join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                                from llsp in lsp.DefaultIfEmpty()
                                join tt in db.Taocan on rta.Taocan_id equals tt.Id
                                where rta.Enabled == availabled && r.Id==resourceId
                                select new BResourceTaocan
                                {
                                    Taocan = rta,
                                    Taocan2 = tt,
                                    CreatedBy = llcu,
                                    UpdatedBy = lluu,
                                    Province = llcity,
                                    SP = llsp,
                                    Resource = new BResource() { Resource = r }
                                };

                    taocans = query.ToList<BResourceTaocan>();
                }else if(resourceId>0 && agencyId <= 0)
                {
                    int total = 0;
                    taocans= FindResourceTaocans(0, resourceId, 0, out total);
                }else if(resourceId<=0 && agencyId>0)
                {
                    var query = from rta in db.Resource_taocan
                                join au in db.Agent_route on rta.Id equals au.Resource_taocan_id
                                join r in db.Resource on rta.Resource_id equals r.Id
                                join cu in db.Users on rta.CreatedBy equals cu.Id into lcu
                                from llcu in lcu.DefaultIfEmpty()
                                join uu in db.Users on rta.UpdatedBy equals uu.Id into luu
                                from lluu in luu.DefaultIfEmpty()
                                join city in db.Area on rta.Area_id equals city.Id into lcity
                                from llcity in lcity.DefaultIfEmpty()
                                join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                                from llsp in lsp.DefaultIfEmpty()
                                join tt in db.Taocan on rta.Taocan_id equals tt.Id
                                where rta.Enabled == availabled && au.User_id==agencyId
                                select new BResourceTaocan
                                {
                                    Taocan = rta,
                                    Taocan2 = tt,
                                    CreatedBy = llcu,
                                    UpdatedBy = lluu,
                                    Province = llcity,
                                    SP = llsp,
                                    Resource = new BResource() { Resource = r }
                                };

                    taocans = query.ToList<BResourceTaocan>();
                }else
                {
                    int total = 0;
                    taocans = FindResourceTaocans(0, resourceId, 0, out total);
                }
            }

            return taocans;
        }

        public Resrouce_interface GetResrouceInterface(int resourceId)
        {
            if(resourceId<=0)
            {
                return null;
            }
            Resrouce_interface api = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                Resource resource = (from r in db.Resource where r.Id==resourceId select r).FirstOrDefault<Resource>();
                if(resource==null)
                {
                    throw new KMBitException(string.Format("编号为{0}的资源不存在",resourceId));
                }

                api = (from a in db.Resrouce_interface where a.Resource_id==resourceId select a).FirstOrDefault<Resrouce_interface>();
            }
                return api;
        }

        public bool UpdateResrouceInterface(Resrouce_interface api)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Resrouce_interface oapi = (from a in db.Resrouce_interface where a.Resource_id == api.Resource_id select a).FirstOrDefault<Resrouce_interface>();
                if(oapi==null)
                {
                    db.Resrouce_interface.Add(api);
                }else
                {
                    oapi.APIURL = api.APIURL;
                    oapi.CallBackUrl = api.CallBackUrl;
                    oapi.Username = api.Username;
                    oapi.Userpassword = api.Userpassword;
                    oapi.ProductApiUrl = api.ProductApiUrl;
                }

                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
