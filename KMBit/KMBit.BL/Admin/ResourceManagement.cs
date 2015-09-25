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
    public class ResourceManagement:BaseManagement
    {
        public ResourceManagement(int userId):base(userId)
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

        public List<BResource> FindResources(int resourceId, string resourceName, int spId, out int total,int page=1,int pageSize=20)
        {
            total = 0;
            List<BResource> resources = null;
            int skip = (page - 1) * pageSize;
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
               
                total = tmp.Count();
                tmp = tmp.OrderBy(s => s.Resource.Created_time).Skip(skip).Take(pageSize);
                resources = tmp.ToList<BResource>();
            }

            return resources;
        }

        public bool CreateResource(Resource resource)
        {
            bool ret = false;

            if(CurrentLoginUser.Permission.CREATE_RESOURCE==0)
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
            List<BResource> existResources = FindResources(0, resource.Name,0,out total);
            if(existResources!=null && existResources.Count>0)
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
            if (CurrentLoginUser.Permission.UPDATE_RESOURCE == 0)
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
                Resource dbResource = (from r in db.Resource where r.Id==resource.Id select r).FirstOrDefault<Resource>();
                //db.Resource.Attach(dbResource);  
                SyncObjectProperties(dbResource, resource);             
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public bool CreateResourceTaocan(Resource_taocan taocan)
        {
            bool ret = false;
            if (CurrentLoginUser.Permission.CREATE_RESOURCE_TAOCAN == 0)
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
            if(total==0)
            {
                throw new KMBitException("资源编号为:"+taocan.Resource_id+" 的资源不存在");
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

        public List<BResourceTaocan> FindResourceTaocans(int sTaocanId,int resourceId,int spId,out int total,int page=1,int pageSize=25)
        {
            total = 0;
            List<BResourceTaocan> sTaocans=null;
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
                          select new BResourceTaocan
                          {
                              Taocan = rta,
                              CreatedBy = llcu,
                              UpdatedBy = lluu,
                              City = llcity,
                              SP = llsp,
                              Resource=new BResource() { Resource=r }
                          };

                if(sTaocanId>0)
                {
                    tmp = tmp.Where(r => r.Taocan.Id == sTaocanId);
                }
                if(resourceId>0)
                {
                    tmp = tmp.Where(r => r.Taocan.Resource_id == resourceId);
                }
                if (spId > 0)
                {
                    tmp = tmp.Where(r => r.Taocan.Sp_id == spId);
                }
                total = tmp.Count();
                tmp = tmp.OrderBy(t => t.Taocan.Id).Skip((page - 1) * pageSize).Take(pageSize);
                sTaocans = tmp.ToList<BResourceTaocan>();
                foreach(BResourceTaocan t in sTaocans)
                {
                    if(t.SP==null)
                    {
                        t.SP = new Sp { Id=0,Name="全网" };                        
                    }

                    if (t.City == null)
                    {
                        t.City = new Area { Id = 0, Name = "全国" };
                    }
                }
            }

            return sTaocans;
        }
    }
}
