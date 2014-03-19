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

        public bool CreateSupplier(Supplier supplier)
        {
            bool result = false;

            return result;
        }
    }
}
