using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BOrder
    {
        public string Sale_ID { get; set; }
        public string Order_ID { get; set; }
        public int Product_ID { get; set; }
        public string Mall_PID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }
        public string Status { get; set; }
        public double Amount { get; set; }
        public int Supplier_ID { get; set; }
        public int StockStatus { get; set; }
        public int Parent_Product_ID { get; set; }
        public BProduct Product { get; set; }
    }
}
