using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class ChargeBaseOrder
    {
        public int Id { get; set; }
        public string MobileNumber { get; set; }
        public int ResourceId { get; set; }
        public int ResourceTaocanId { get; set; }
        public string OutId { get; set; }
        public int OperateUserId { get; set; }
        public long CreatedTime { get; set; }

        public bool Payed { get; set; }
    }

    public class ChargeOrder: ChargeBaseOrder
    {
        public int AgencyId { get; set; }
        public int RouteId { get; set; }
    }
}
