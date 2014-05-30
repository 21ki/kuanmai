using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BShopExpress:BModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public BShop Shop { get; set; }
        public int Created { get; set; }
        public BUser Created_By { get; set; }
        public BUser Modified_By { get; set; }
        public int Modified { get; set; }
        public List<BExpressFee> Fees { get; set; }
        public bool IsDefault { get; set; }
    }
}
