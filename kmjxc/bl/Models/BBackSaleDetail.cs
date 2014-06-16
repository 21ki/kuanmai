using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBackSaleDetail
    {
        public int BackSaleID { get; set; }
        public BProduct Product { get; set; }
        public int ProductID { get; set; }
        public int ParentProductID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public long Created { get; set; }
        public BBackSale BackSale { get; set; }
        public double Amount { get; set; }
        public string Order_ID { get; set; }
    }
}
