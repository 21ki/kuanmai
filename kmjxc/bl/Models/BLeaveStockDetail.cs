using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BLeaveStockDetail
    {
        public BStoreHouse StoreHouse { get; set; }
        public int ProductID { get; set; }
        public BProduct Product { get; set; }
        public int Parent_ProductID { get; set; }
        public BProduct ParentProduct { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
