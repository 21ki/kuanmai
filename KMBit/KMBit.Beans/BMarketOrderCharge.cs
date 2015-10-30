using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class BMarketOrderCharge
    {
        public int AgentId { get; set; }
        public int CustomerId { get; set; }
        public int ActivityId { get; set; }
        public int ActivityOrderId { get; set; }
        public string Phone { get; set; }
        public string SPName { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string MacAddress { get; set; }
    }
}
