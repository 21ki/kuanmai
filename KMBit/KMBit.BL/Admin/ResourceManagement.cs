using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;

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

        public List<BResource> FindResource(int resourceId,string resourceName)
        {
            List<BResource> resources = null;
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
                              CreatedBy=llcu,
                              UpdatedBy=lluu
                          };
                if(resourceId>0)
                {
                    tmp = tmp.Where(s=>s.Resource.Id==resourceId);
                }
                if(!string.IsNullOrEmpty(resourceName))
                {
                    tmp = tmp.Where(s=>s.Resource.Name.Contains(resourceName));
                }

                tmp.OrderBy(s => s.Resource.Created_time);

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
                resource.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                db.Resource.Attach(resource);               
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

            taocan.Enabled = true;
            taocan.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            taocan.Updated_time = taocan.Created_time;
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

        public List<BResourceTaocan> FindResourceTaocans(int sTaocanId,int resourceId,int spId)
        {
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

                sTaocans = tmp.ToList<BResourceTaocan>();
            }

            return sTaocans;
        }
    }
}
