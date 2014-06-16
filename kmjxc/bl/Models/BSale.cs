using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BSale
    {        
        public string Sale_ID;
        public List<BOrder> Orders { get; set; }
        public BCustomer Buyer { get; set; }
        public BUser Seller { get; set; }
        public double Amount { get; set; }
        public double Post_Fee { get; set; }
        public double Post_Fee_Actual { get; set; }
        public int SaleType { get; set; }
        public BShop Shop { get; set; }
        public long SaleDateTime { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public long Synced { get; set; }
        public int StockStatus { get; set; }
        public string Status { get; set; }
        public string StatusText { get; set; }
        public Common_District Province { get; set; }
        public Common_District City { get; set; }
        public BUser SyncUser { get; set; }
        public bool HasRefound { get; set; }
    }
}
