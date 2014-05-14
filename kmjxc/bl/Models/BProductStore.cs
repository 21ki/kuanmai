using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BProductStore
    {
        public int ProductID { get; set; }
        public string ProductTitle { get; set; }
        public int Quantity { get; set; }
        public int TotalQuantity { get; set; }
        public List<BProductStoreProperty> StoreProperties { get; set; }
    }
}
