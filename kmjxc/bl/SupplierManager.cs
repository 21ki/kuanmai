using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
namespace KM.JXC.BL
{
    public class SupplierManager:BaseManager
    {        
        public SupplierManager(User user,int shop_id)
            : base(user, shop_id)
        {

        }

        public SupplierManager(User user)
            : base(user)
        {
        }

        public List<Supplier> GetSupplies(int shop_id,int pageIndex,int pageSize,out int totalRecord)
        {
            List<Supplier> supplies = new List<Supplier>();
            totalRecord = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                totalRecord = db.Supplier.Count();
            }

            return supplies;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="supplier_id"></param>
        /// <returns></returns>
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
    }
}
