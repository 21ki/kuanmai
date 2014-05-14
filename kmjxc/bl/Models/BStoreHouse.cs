using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;

namespace KM.JXC.BL.Models
{
    public class BStoreHouse:BModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Created { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public BUser Guard { get; set; }
        public BUser Created_By { get; set; }
        public BShop Shop { get; set; }
        public bool IsDefault { get; set; }
    }
}
