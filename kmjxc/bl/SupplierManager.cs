using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
namespace KM.JXC.BL
{
    public class SupplierManager:BBaseManager
    {        
        public SupplierManager(User user,int shop_id)
            : base(user, shop_id)
        {

        }

        public SupplierManager(User user)
            : base(user)
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
        public List<Supplier> GetShopSupplies(int shop_id,int pageIndex,int pageSize,out int totalRecord)
        {
            List<Supplier> supplies = new List<Supplier>();
            totalRecord = 0;
            int shop_Id = 0;
            if (shop_id > 0)
            {
                shop_Id = shop_id;
            }
            else
            {
                shop_Id = this.Shop_Id;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())            
            {                
                if (pageIndex == 0)
                {
                    pageIndex = 1;
                }
                totalRecord = db.Supplier.Count(sp => sp.Shop_ID == shop_Id);
                if (totalRecord > 0)
                {
                    var sps = from s in db.Supplier 
                              where s.Shop_ID == shop_Id 
                              orderby s.Supplier_ID ascending 
                              select s;
                    sps.Skip((pageIndex-1)*pageSize).Take(pageSize);
                    supplies = sps.ToList<Supplier>();
                }
            }
            
            return supplies;
        }

        /// <summary>
        /// Gets product suppliers, supports paging
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        public List<Supplier> GetProductSuppliers(int product_id, int pageIndex, int pageSize, out int totalRecord)
        {
            List<Supplier> supplies = new List<Supplier>();
            totalRecord = 0;           
           
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (pageIndex == 0)
                {
                    pageIndex = 1;
                }
                totalRecord = db.Product_Supplier.Count(ps => ps.Product_ID==product_id);
                if (totalRecord > 0)
                {
                    var sps = from s in db.Supplier
                              from sp in db.Product_Supplier                              
                              where sp.Product_ID==product_id && s.Supplier_ID==sp.Supplier_ID
                              orderby s.Supplier_ID ascending
                              select s;
                    sps.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    supplies = sps.ToList<Supplier>();
                }
            }

            return supplies;
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
                supplier.User_ID = this.CurrentUser.User_ID;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
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

            if (product_id == 0 || supplier_id == 0)
            {
                throw new KMJXCException("创建产品供应商时，产品和供应商都必须选择");
            }

            if (this.CurrentUserPermission.ADD_SUPPLIER == 0)
            {
                throw new KMJXCException("没有创建供应商的权限");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Product_Supplier ps = new Product_Supplier();
                ps.Product_ID = product_id;
                ps.Supplier_ID = supplier_id;
                db.Product_Supplier.Add(ps);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Update supplier information
        /// </summary>
        /// <param name="supplier">Supplier Object</param>
        /// <returns>TRUE/FALSE</returns>
        public bool UpdateSupplier(Supplier supplier)
        {
            bool result = false;

            if (supplier == null || supplier.Supplier_ID<=0)
            {
                return result;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Supplier.Attach(supplier);
                
                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
