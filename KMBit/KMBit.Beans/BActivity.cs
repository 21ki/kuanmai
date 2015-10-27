using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;

namespace KMBit.Beans
{
    public class BActivity
    {
        public Marketing_Activities Activity { get; set; }
        public BAgentRoute Ruote { get; set; }
        public BCustomer Customer { get; set; }
        public int UsedCount { get; set; }
    }
}
