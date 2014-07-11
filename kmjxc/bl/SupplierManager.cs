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

                int[] cshops =null;
                if (this.DBChildShops != null && this.DBChildShops.Count > 0)
                {
                    cshops = (from shop in this.DBChildShops select shop.Shop_ID).ToArray<int>();
                    spps = spps.Where(a => a.Shop_ID == this.Shop.Shop_ID || a.Shop_ID == this.Main_Shop.Shop_ID || cshops.Contains(a.Shop_ID));
                }
                else
                {
                    spps = spps.Where(a => a.Shop_ID == this.Shop.Shop_ID || a.Shop_ID == this.Main_Shop.Shop_ID);
                }

                totalRecord = spps.Count();
              
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
                                                 ID = supplier.City_ID,
                                                 Name = c.name
                                             }).FirstOrDefault<BArea>(),
                                 Province = (from c in db.Common_District
                                             where c.id == supplier.Province_ID
                                             select new BArea
                                             {
                                                 ID = supplier.Province_ID,
                                                 Name = c.name
                                             }).FirstOrDefault<BArea>(),
                                 Created_By = (from u in db.User
                                               where u.User_ID == supplier.User_ID
                                               select new BUser
                                                   {
                                                       ID = supplier.User_ID,
                                                       Name = u.Name,
                                                       Mall_ID = u.Mall_ID,
                                                       Mall_Name = u.Mall_Name
                                                   }).FirstOrDefault<BUser>(),
                                 Shop = (from sp in db.Shop
                                         where sp.Shop_ID == supplier.Shop_ID
                                         select new
                                             BShop
                                             {
                                                 ID=supplier.Shop_ID,
                                                 Title=sp.Name,
                                                 Description=sp.Description
                                             }).FirstOrDefault<BShop>()
                             };

                ss = ss.OrderBy(a => a.Shop.ID).OrderBy(a => a.ID);
                ss = ss.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                suppliers = ss.ToList<BSupplier>();
                List<int> cspids = (from csp in this.DBChildShops select csp.Shop_ID).ToList<int>();
                foreach (BSupplier sp in suppliers) 
                {
                    if (sp.Shop.ID == this.Main_Shop.Shop_ID)
                    {
                        sp.FromMainShop = true;
                    }
                    else if (cshops != null && cshops.Length > 0)
                    {
                        if (cshops.Contains(sp.Shop.ID))
                        {
                            sp.FromChildShop = true;
                        }
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
        /// Update supplier products
        /// </summary>
        /// <param name="product_ids"></param>
        /// <param name="supplier_id"></param>
        public void UpdateSupplierProducts(int[] product_ids, int supplier_id)
        {
            if (supplier_id <= 0)
            {
                throw new KMJXCException("更新供应商产品时必须输入供应商编号");
            }

            if (this.CurrentUserPermission.UPDATE_SUPPLIER_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限更新供应商产品");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Supplier supplier = (from s in db.Supplier where s.Supplier_ID == supplier_id select s).FirstOrDefault<Supplier>();
                if (supplier == null)
                {
                    throw new KMJXCException("编号为:" + supplier_id + " 的供应商信息不存在");
                }

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    int[] child_shops = (from c in this.ChildShops select c.ID).ToArray<int>();
                    if (this.Shop.Shop_ID != supplier.Shop_ID && !child_shops.Contains(supplier.Shop_ID))
                    {
                        throw new KMJXCException("不能操作其他店铺的供应商，只能使用主店铺和子店铺的供应商");
                    }
                }
                else
                {
                    if (supplier.Shop_ID != this.Shop.Shop_ID && supplier.Shop_ID != this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能操作其他店铺的供应商，只能使用主店铺和子店铺的供应商");
                    }
                }

                List<Product_Supplier> products = (from s in db.Product_Supplier where s.Supplier_ID == supplier_id select s).ToList<Product_Supplier>();
                if(product_ids==null || product_ids.Length<=0)
                {
                    foreach (Product_Supplier p in products)
                    {
                        p.Enabled = false;
                    }

                    db.SaveChanges();
                }else
                {
                    for (int i = 0; i < product_ids.Length; i++)
                    {
                        Product_Supplier ps=(from s in products where s.Product_ID==product_ids[i] select s).FirstOrDefault<Product_Supplier>();
                        if (ps == null)
                        {
                            ps = new Product_Supplier();
                            ps.Supplier_ID = supplier_id;
                            ps.Product_ID = product_ids[i];
                            ps.Enabled = true;
                            ps.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            ps.Created_By = this.CurrentUser.ID;
                            db.Product_Supplier.Add(ps);
                        }
                        else
                        {
                            ps.Enabled = true;
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Remove supplier products
        /// </summary>
        /// <param name="product_ids"></param>
        /// <param name="supplier_id"></param>
        public void RemoveSupplierProducts(int[] product_ids, int supplier_id)
        {
            if (supplier_id <= 0)
            {
                throw new KMJXCException("更新供应商产品时必须输入供应商编号");
            }

            if (this.CurrentUserPermission.UPDATE_SUPPLIER_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限移除供应商产品");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (product_ids == null || product_ids.Length <= 0)
                {
                    return;
                }

                Supplier supplier=(from s in db.Supplier where s.Supplier_ID==supplier_id select s).FirstOrDefault<Supplier>();
                if (supplier == null)
                {
                    throw new KMJXCException("编号为:"+supplier_id+" 的供应商信息不存在");
                }

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    int[] child_shops = (from c in this.ChildShops select c.ID).ToArray<int>();
                    if (this.Shop.Shop_ID != supplier.Shop_ID && !child_shops.Contains(supplier.Shop_ID))
                    {
                        throw new KMJXCException("不能操作其他店铺的供应商，只能使用主店铺和子店铺的供应商");
                    }
                }
                else
                {
                    if (supplier.Shop_ID != this.Shop.Shop_ID && supplier.Shop_ID != this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能操作其他店铺的供应商，只能使用主店铺和子店铺的供应商");
                    }
                }

                List<Product_Supplier> products = (from s in db.Product_Supplier where s.Supplier_ID == supplier_id && product_ids.Contains(s.Product_ID) select s).ToList<Product_Supplier>();
                foreach (Product_Supplier ps in products)
                {
                    ps.Enabled = false;
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get supplier detail information include products
        /// </summary>
        /// <param name="supplier_id">supplier id</param>
        /// <returns>Instance of BSupplier object</returns>
        public BSupplier GetSupplierFullInfo(int supplier_id,bool getProduct=false)
        {
            BSupplier supplier = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                supplier = (from s in db.Supplier
                            join user in db.User on s.User_ID equals user.User_ID into LUser
                            from l_user in LUser.DefaultIfEmpty()
                            join shop in db.Shop on s.Shop_ID equals shop.Shop_ID into LShop
                            from l_shop in LShop.DefaultIfEmpty()
                            join city in db.Common_District on s.City_ID equals city.id into LCity
                            from l_city in LCity.DefaultIfEmpty()
                            join province in db.Common_District on s.Province_ID equals province.id into LProvince
                            from l_province in LProvince.DefaultIfEmpty()
                            where s.Supplier_ID == supplier_id
                            select new BSupplier
                            {
                                ID = s.Supplier_ID,
                                Name = s.Name,
                                Address = s.Address,
                                City = l_city != null ? new BArea { ID = l_city.id, Name = l_city.name } : new BArea { ID = 0, Name = "" },
                                ContactPerson = s.Contact_Person,
                                Created = s.Create_Time,
                                Created_By = l_user != null ? new BUser
                                {
                                    ID = l_user.User_ID,
                                    Name = l_user.Name,
                                    Mall_ID = l_user.Mall_ID,
                                    Mall_Name = l_user.Mall_Name
                                } : new BUser
                                {
                                    ID = 0,
                                    Name = "",
                                    Mall_ID = "",
                                    Mall_Name = ""
                                },
                                Enable = (bool)s.Enabled,
                                Fax = s.Fax,
                                Phone = s.Phone,
                                Province = l_province != null ? new BArea { ID = l_province.id, Name = l_province.name } : new BArea { ID = 0, Name = "" },
                                Remark = s.Remark,
                                Shop = l_shop != null ? new BShop
                                {
                                    ID = l_shop.Shop_ID,
                                    Title = l_shop.Name
                                } : new BShop
                                {
                                    ID = 0,
                                    Title = ""
                                }

                            }).FirstOrDefault<BSupplier>();

                if (supplier != null && getProduct)
                {
                    List<BProduct> products = null;
                    var productIds = from s in db.Product_Supplier
                                     where s.Supplier_ID == supplier.ID && s.Enabled==true
                                     select s.Product_ID;

                    products = (from p in db.Product 
                                where p.Parent_ID == 0 && productIds.Contains(p.Product_ID) 
                                select new BProduct 
                                {
                                    ID=p.Product_ID,
                                    Title=p.Name
                                }).ToList<BProduct>();

                    supplier.Products = products;
                }
            }
            return supplier;
        }

        /// <summary>
        /// Update supplier basic information
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public bool UpdateSupplier(Supplier supplier)
        {
            bool result = false;

            if (this.CurrentUserPermission.UPDATE_SUPPLIER == 0)
            {
                throw new KMJXCException("没有权限更新供应商");
            }

            if (string.IsNullOrEmpty(supplier.Name))
            {
                throw new KMJXCException("供应商名称不能为空");
            }
            KuanMaiEntities db = null;

            try
            {
                
                db = new KuanMaiEntities();
                int[] child_shops = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                Supplier existed = (from supp in db.Supplier where supp.Supplier_ID == supplier.Supplier_ID select supp).FirstOrDefault<Supplier>();
                if (existed == null)
                {
                    throw new KMJXCException("要修改的供应商不存在");
                }
                var obj = (from sp in db.Supplier where (child_shops.Contains(sp.Shop_ID) || sp.Shop_ID == this.Shop.Shop_ID || sp.Shop_ID == this.Main_Shop.Shop_ID) && supplier.Name.Contains(sp.Name) && sp.Supplier_ID!=existed.Supplier_ID select sp);
                
                if (obj.ToList<Supplier>().Count > 0)
                {
                    throw new KMJXCException("供应商名称已经存在,请换个名字");
                }

                if (this.Shop.Shop_ID != this.Main_Shop.Shop_ID)
                {
                    if (existed.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        if (this.CurrentUser.Shop.ID != existed.Shop_ID)
                        {
                            throw new KMJXCException("您不能修改主店铺供应商");
                        }
                    }
                    else if (existed.Shop_ID!=this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能其他主店铺供应商");
                    }
                   
                }
                else
                {
                    if (existed.Shop_ID != this.Main_Shop.Shop_ID && !child_shops.Contains(existed.Shop_ID))
                    {
                        throw new KMJXCException("您不能修改其他店铺的供应商，只能修改主店铺或者子店铺供应商");
                    }
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

            if (this.CurrentUserPermission.ADD_SUPPLIER == 0)
            {
                throw new KMJXCException("没有权限添加新供应商");
            }

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
