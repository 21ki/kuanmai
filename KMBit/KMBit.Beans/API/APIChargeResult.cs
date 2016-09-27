using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans.API
{
    public class APIChargeResult
    {
        public int Status { get; set; }
        public int OrderId { get; set; }
        public string Message { get; set; }
    }
}
