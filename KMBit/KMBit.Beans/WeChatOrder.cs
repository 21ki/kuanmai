using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class WeChatOrder
    {
        public ChargeOrder Order { get; set; }
        public string PrepayId { get; set; }
        public string PaySign { get; set; }
    }
}
