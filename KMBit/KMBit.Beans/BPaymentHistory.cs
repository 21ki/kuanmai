using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BPaymentHistory : Payment_history
    {
        public string UserName { get; set; }
        public string OprUser { get; set; }
        public string PayTypeText { get; set; }
        public string TranfserTypeText { get; set; }
        public string StatusText { get; set; }
    }
}
