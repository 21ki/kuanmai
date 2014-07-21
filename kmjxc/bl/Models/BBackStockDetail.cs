using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BBackStockDetail
    {
        public BBackStock BackStock { get; set; }
        public BProduct Product { get; set; }
        public int ProductID { get; set; }
        public int ParentProductID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public BStoreHouse StoreHouse { get; set; }
        public int Status { get; set; }
        public BStockBatch Batch { get; set; }
    }
}
