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
        public string Name { get; set; }
        public long Created { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public BUser Guard { get; set; }
        public BUser Created_By { get; set; }
        public BShop Shop { get; set; }
        public bool IsDefault { get; set; }
    }
}
