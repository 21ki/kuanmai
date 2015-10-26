using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BCustomer:Customer
    {
        public Users Agent { get; set; }
    }
}
