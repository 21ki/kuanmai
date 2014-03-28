using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BTrade
    {
        public int ID;
        public string Mall_Trade_ID;
        public List<BOrder> Orders { get; set; }
        public BUser Buyer { get; set; }
        public BUser Seller { get; set; }
        public double Amount { get; set; }
        public double Post_Fee { get; set; }
        public double Post_Fee_Actual { get; set; }
        public int SaleType { get; set; }
        public Shop Shop { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public long Synced { get; set; }
    }
}
