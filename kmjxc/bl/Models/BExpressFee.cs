using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BExpressFee
    {
        public int ID { get; set; }
        public BExpress Express { get; set; }
        public BShop Shop { get; set; }
        public BArea Province { get; set; }
        public BArea City { get; set; }
        public BStoreHouse StoreHouse { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public BUser Created_By { get; set; }
        public BUser Modified_By { get; set; }
        public Double Fee { get; set; }
    }
}
