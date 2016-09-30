using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans.API
{
    public class AgentProduct
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public int SP { get; set; }
        public string SPName { get; set; }
        public float ClientDiscount { get; set; }
        public float PlatformSalePrice { get; set; }
        public string RestrictProvince { get; set; }
        public float ClientSalePrice { get; set; }
    }
}
