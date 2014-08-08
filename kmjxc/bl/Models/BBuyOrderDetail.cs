using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BBuyOrderDetail
    {
        public BBuyOrder BuyOrder { get; set; }
        public BProduct Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        public int Parent_Product_ID { get; set; }
        public long BuyDate { get; set; }
    }
}
