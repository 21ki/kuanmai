using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;

namespace KM.JXC.BL.Models
{
    public class BEnterStock
    {
        public int ID { get; set; }

        public Shop Shop { get; set; }

        public BUser User { get; set; }

        public int BuyID { get; set; }        

        public int EnterTime { get; set; }

        public List<BEnterStockDetail> Details { get; set; }
    }
}
