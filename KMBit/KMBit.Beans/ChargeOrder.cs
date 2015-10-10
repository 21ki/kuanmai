using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class ChargeOrder
    {
        public int Id { get; set; }
        public string MobileNumber { get; set; }
        public int ResourceId { get; set; }
        public int ResourceTaocanId { get; set; }
    }

    public class AgencyChargeOrder:ChargeOrder
    {
        public int AgencyId { get; set; }
        public int RouteId { get; set; }
    }
}
