﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL.Admin;
namespace KMBit.BL
{
    public class BaseManagement
    {
        protected log4net.ILog logger;
        public BUser CurrentLoginUser { get; private set; }

        public BaseManagement()
        {
            this.InitializeLoggger();
        }
        public BaseManagement(BUser user)
        {
            this.CurrentLoginUser = user;
            this.InitializeLoggger();
        }
        public BaseManagement(Users user)
        {
            if (user != null)
            {
                this.CurrentLoginUser = this.GetUserInfo(user.Id);
            }
            this.InitializeLoggger();
        }
        public BaseManagement(int userId)
        {
            this.CurrentLoginUser = this.GetUserInfo(userId);
            this.InitializeLoggger();
        }
        public BaseManagement(string email)
        {
            this.CurrentLoginUser = this.GetUserInfo(email);
            this.InitializeLoggger();
        }

        protected virtual void InitializeLoggger()
        {
            if (this.logger == null)
            {
                this.logger = KMLogger.GetLogger();
            }
        }

        public BUser GetUserInfo(int userId)
        {
            if (userId <= 0)
            {
                return null;
            }
            BUser user = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                user = new BUser();
                user.User = (from u in db.Users where u.Id == userId select u).FirstOrDefault<Users>();

                Admin_Users au = (from ausr in db.Admin_Users where ausr.User_Id == userId select ausr).FirstOrDefault<Admin_Users>();
                if (au != null)
                {
                    user.IsSuperAdmin = au.IsSuperAdmin;
                    user.IsWebMaster = au.IsWebMaster;
                    user.IsAdmin = true;
                }
                if (!user.IsSuperAdmin)
                {
                    user.Permission = PermissionManagement.GetUserPermissions(userId);
                } else
                {
                    user.Permission = new Permissions();
                    System.Reflection.PropertyInfo[] fields = typeof(Permissions).GetProperties();
                    foreach (System.Reflection.PropertyInfo field in fields)
                    {
                        field.SetValue(user.Permission, true);
                    }
                }
            }
            return user;
        }

        public BUser GetUserInfo(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            BUser user = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                user = new BUser();
                user.User = (from u in db.Users where u.Email == email select u).FirstOrDefault<Users>();

                Admin_Users au = (from ausr in db.Admin_Users where ausr.User_Id == user.User.Id select ausr).FirstOrDefault<Admin_Users>();
                if (au != null)
                {
                    user.IsSuperAdmin = au.IsSuperAdmin;
                    user.IsWebMaster = au.IsWebMaster;
                    user.IsAdmin = true;
                }
                if (!user.IsSuperAdmin)
                {
                    user.Permission = PermissionManagement.GetUserPermissions(user.User.Id);
                }
                else
                {
                    user.Permission = new Permissions();
                    System.Reflection.FieldInfo[] fields = typeof(Permissions).GetFields();
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(user.Permission, 1);
                    }
                }
            }
            return user;
        }

        public List<Area> GetAreas(int parentId)
        {
            List<Area> areas = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var tmp = from a in db.Area select a;
                if (parentId > 0)
                {
                    tmp = tmp.Where(a => a.Upid == parentId);
                }
                else
                {
                    tmp = tmp.Where(a => a.Level == 1);
                }

                areas = tmp.OrderBy(a => a.Id).ToList<Area>();
            }
            return areas;
        }

        public List<Sp> GetSps()
        {
            List<Sp> spList = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                spList = (from s in db.Sp orderby s.Id select s).ToList<Sp>();
            }
            return spList;
        }

        public List<DAL.PayType> GetPayTypes()
        {
            List<DAL.PayType> types = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                types = (from t in db.PayType orderby t.Id select t).ToList();
            }
            return types;
        }

        public List<User_type> GetUserTypes()
        {
            List<User_type> types = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                types = (from t in db.User_type orderby t.Id select t).ToList<User_type>();
            }
            return types;
        }

        protected void SyncObjectProperties(object o1, object o2)
        {
            if(o1==null || o2==null)
            {
                return;
            }

            if(o1.GetType().ToString()!=o2.GetType().ToString())
            {
                return;
            }

            System.Reflection.PropertyInfo[] properties = o1.GetType().GetProperties();
            if (properties == null || properties.Length == 0) {
                return;
            }
            foreach(System.Reflection.PropertyInfo property in properties)
            {
                property.SetValue(o1, property.GetValue(o2));
            }
        }

        public List<BTaocan> FindBTaocans()
        {
            List<BTaocan> taocans = new List<BTaocan>();

            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from t in db.Taocan
                            join sp in db.Sp on t.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            orderby t.Created_time descending
                            select new BTaocan
                            {
                                Taocan=t,
                                SP =llsp
                            };

                taocans = query.ToList<BTaocan>();
            }

            return taocans;
        }

        /// <summary>
        /// Return the available packages for the gaving mobile phone number
        /// </summary>
        /// <param name="spName">SP Name</param>
        /// <param name="province">The Province the mobile number belongs to</param>
        /// <param name="scope">Global or local bit</param>
        /// <returns></returns>
        public List<BResourceTaocan> SearchResourceTaocans(string spName, string province,BitScope scope)
        {
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            using (chargebitEntities db = new chargebitEntities())
            {
                int spId = 0;
                if(!string.IsNullOrEmpty(spName))
                {
                    spId = (from s in db.Sp where s.Name==spName select s.Id).FirstOrDefault<int>();
                }
                int provinceId = 0;
                if(!string.IsNullOrEmpty(province))
                {
                    provinceId = (from p in db.Area where p.Name.Contains(province) select p.Id).FirstOrDefault<int>();
                }

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
                          where rta.Enabled==true && rta.Sale_price>0 && r.Enabled==true
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

                if(spId>0)
                {
                    tmp = tmp.Where(t => t.Taocan.Sp_id == spId || t.Taocan.Sp_id == 0);
                }else
                {
                    tmp = tmp.Where(t => t.Taocan.Sp_id == 0);
                }

                //全国还是本地流量
                if(scope== BitScope.Local)
                {
                    tmp = tmp.Where(t => t.Taocan.Area_id == provinceId);
                }
                else
                {
                    tmp = tmp.Where(t => t.Taocan.Area_id == 0);
                }

                //限制号码归属地
                if (provinceId > 0)
                {
                    tmp = tmp.Where(t => (t.Taocan.NumberProvinceId == provinceId || t.Taocan.NumberProvinceId==0));
                }

                List<BResourceTaocan> tmpTaocans = tmp.OrderBy(t=>t.Taocan.Quantity).ToList<BResourceTaocan>();
                List<int> ts = (from t in tmpTaocans select t.Taocan.Quantity).Distinct<int>().ToList<int>();
                List<BResourceTaocan> globalTaocans = (from t in tmpTaocans where t.Taocan.Area_id == 0 select t).ToList<BResourceTaocan>();
                List<BResourceTaocan> localTaocans = (from t in tmpTaocans where t.Taocan.Area_id > 0 select t).ToList<BResourceTaocan>();
                foreach (int t in ts)
                {
                    BResourceTaocan st = (from tc in globalTaocans where tc.Taocan.Quantity==t orderby tc.Taocan.Resource_Discount ascending select tc).FirstOrDefault<BResourceTaocan>();
                    BResourceTaocan st2 = (from tc in localTaocans where tc.Taocan.Quantity == t orderby tc.Taocan.Resource_Discount ascending select tc).FirstOrDefault<BResourceTaocan>();
                    if (st != null)
                    {
                        taocans.Add(st);
                    }
                    if (st2 != null)
                    {
                        taocans.Add(st2);
                    }
                }
            }
            return taocans;
        }
    }
}
