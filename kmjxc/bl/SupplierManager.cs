using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;

namespace KM.JXC.BL
{
    public class SupplierManager:BBaseManager
    {
        public SupplierManager(BUser user, int shop_id, Permission permission)
            : base(user, shop_id,permission)
        {

        }

        public SupplierManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            
        }

        /// <summary>
        /// Gets shop suppliers, supports paging
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        public List<BSupplier> SearchSupplies(int product_id, int pageIndex, int pageSize, out int totalRecord)
        {
            List<BSupplier> suppliers = new List<BSupplier>();
            totalRecord = 0;
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                int[] sids = null;
                if (product_id > 0)
                {
                    sids = (from ps in db.Product_Supplier where ps.Product_ID == product_id select ps.Supplier_ID).ToArray<int>();
                }
                var spps = from sp in db.Supplier
                           //where sp.Shop_ID==this.Shop.Shop_ID || sp.Shop_ID==this.Main_Shop.Shop_ID
                           select sp;
                if (sids != null && sids.Length > 0)
                {
                    spps = spps.Where(a => sids.Contains(a.Supplier_ID));
                }

                if (this.ChildShops != null && this.ChildShops.Count > 0)
                {
                    int[] cshops = (from shop in this.ChildShops select shop.Shop_ID).ToArray<int>();
                    spps = spps.Where(a => a.Shop_ID == this.Shop.Shop_ID || a.Shop_ID == this.Main_Shop.Shop_ID || cshops.Contains(a.Shop_ID));
                }
                else
                {
                    spps = spps.Where(a => a.Shop_ID == this.Shop.Shop_ID || a.Shop_ID == this.Main_Shop.Shop_ID);
                }

                totalRecord = spps.Count();
                spps = spps.OrderBy(a => a.Shop_ID).OrderBy(a => a.Supplier_ID);
                spps = spps.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                var ss = from supplier in spps
                         select new BSupplier
                             {
                                 ID = supplier.Supplier_ID,
                                 Created = supplier.Create_Time,
                                 Address = supplier.Address,
                                 ContactPerson = supplier.Contact_Person,
                                 Enable = (bool)supplier.Enabled,
                                 Name = supplier.Name,
                                 Remark = supplier.Remark,
                                 Fax = supplier.Fax,
                                 Phone = supplier.Phone,
                                 PostalCode = supplier.PostalCode,
                                 City = (from c in db.Common_District
                                         where c.id == supplier.City_ID
                                         select new BArea
                                             {
                                                 ID = c.id,
                                                 Name = c.name
                                             }).FirstOrDefault<BArea>(),
                                 Province = (from c in db.Common_District
                                             where c.id == supplier.Province_ID
                                             select new BArea
                                             {
                                                 ID = c.id,
                                                 Name = c.name
                                             }).FirstOrDefault<BArea>(),
                                 Created_By = (from u in db.User
                                               where u.User_ID == supplier.User_ID
                                               select new BUser
                                                   {
                                                       ID = u.User_ID,
                                                       Name = u.Name,
                                                       Mall_ID = u.Mall_ID,
                                                       Mall_Name = u.Mall_Name
                                                   }).FirstOrDefault<BUser>(),
                                 Shop = (from sp in db.Shop
                                         where sp.Shop_ID == supplier.Shop_ID
                                         select new
                                             BShop
                                             {
                                                 ID=sp.Shop_ID,
                                                 Title=sp.Name,
                                                 Description=sp.Description
                                             }).FirstOrDefault<BShop>()
                             };

                suppliers = ss.ToList<BSupplier>();
                List<int> cspids = (from csp in this.ChildShops select csp.Shop_ID).ToList<int>();
                foreach (BSupplier sp in suppliers) 
                {
                    if (sp.Shop.ID != this.Shop.Shop_ID)
                    {
                        if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                        {
                            if (cspids.Contains(sp.Shop.ID))
                            {
                                sp.FromChildShop = true;
                            }
                        }
                        else
                        {
                            sp.FromMainShop = true;
                        };
                    }                   
                }
            }
            catch (Exception ex)
            {

            }
            return suppliers;
        }

        /// <summary>
        /// Gets product suppliers, supports paging
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        public List<BSupplier> GetProductSuppliers(int product_id, int pageIndex, int pageSize, out int totalRecord)
        {
            totalRecord = 0;
            return this.SearchSupplies(product_id,pageIndex,pageSize,out totalRecord);
        }

        /// <summary>
        /// Get supplier detail information
        /// </summary>
        /// <param name="supplier_id">Supplier ID</param>
        /// <returns>TRUE/FALSE</returns>
        public Supplier GetSupplierDetail(int supplier_id)
        {
            Supplier supplier = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var s = from sp in db.Supplier where sp.Supplier_ID == supplier_id select sp;

                if(s!=null && s.ToList<Supplier>().Count>0)
                {
                    supplier=s.ToList<Supplier>()[0];
                }
            }

            return supplier;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public bool UpdateSupplier(Supplier supplier)
        {
            bool result = false;
            if (string.IsNullOrEmpty(supplier.Name))
            {
                throw new KMJXCException("供应商名称不能为空");
            }
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                Supplier existed = (from supp in db.Supplier where supp.Supplier_ID == supplier.Supplier_ID select supp).FirstOrDefault<Supplier>();
                if (existed == null)
                {
                    throw new KMJXCException("要修改的供应商不存在");
                }
                var obj = (from sp in db.Supplier where (sp.Shop_ID == this.Shop.Shop_ID || sp.Shop_ID == this.Main_Shop.Shop_ID) && supplier.Name.Contains(sp.Name) && sp.Supplier_ID!=existed.Supplier_ID select sp);
                if (obj.ToList<Supplier>().Count > 0)
                {
                    throw new KMJXCException("供应商名称已经存在");
                }

                supplier.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                supplier.Modified_By = this.CurrentUser.ID;
                this.UpdateProperties(existed, supplier);
                db.SaveChanges();
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Create new supplier
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public bool CreateSupplier(Supplier supplier)
        {
            bool result = false;
            
            if (string.IsNullOrEmpty(supplier.Name))
            {
                throw new KMJXCException("供应商名称不能为空");
            }

            if (supplier.User_ID == 0 && this.CurrentUser!=null)
            {
                supplier.User_ID = this.CurrentUser.ID;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var obj = (from sp in db.Supplier where (sp.Shop_ID == this.Shop.Shop_ID || sp.Shop_ID == this.Main_Shop.Shop_ID) && supplier.Name.Contains(sp.Name) select sp);
                if (obj.ToList<Supplier>().Count > 0)
                {
                    throw new KMJXCException("供应商名称已经存在");
                }
                db.Supplier.Add(supplier);
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Set product supplier
        /// </summary>
        /// <param name="product_id">Product ID</param>
        /// <param name="supplier_id">Supplier ID</param>
        /// <returns>TRUE/FALSE</returns>
        public bool SetProductSupplier(int product_id, int supplier_id)
        {
            bool result = false;

            if (this.CurrentUserPermission.ADD_SUPPLIER == 0)
            {
                throw new KMJXCException("没有创建供应商的权限");
            }

            if (product_id == 0 || supplier_id == 0)
            {
                throw new KMJXCException("创建产品供应商时，产品和供应商都必须选择");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Product_Supplier pps = (from p in db.Product_Supplier where p.Product_ID == product_id && p.Supplier_ID == supplier_id select p).FirstOrDefault<Product_Supplier>();
                if (pps != null)
                {
                    throw new KMJXCException("");
                }
                Product_Supplier ps = new Product_Supplier();
                ps.Product_ID = product_id;
                ps.Supplier_ID = supplier_id;
                db.Product_Supplier.Add(ps);
                db.SaveChanges();
                result = true;
            }
            return result;
        }        
    }
}
