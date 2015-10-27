using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BCustomerReChargeHistory
    {
        public Customer_Recharge History { get; set; }
        public Customer Customer { get; set; }
        public Users User { get; set; }
    }
}
