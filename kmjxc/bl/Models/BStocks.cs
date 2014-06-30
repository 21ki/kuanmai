using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BStock:BModel
    {        
        public BProduct Product { get; set; }
        public int Parent_Product_ID { get; set; }
        public BStoreHouse StoreHouse { get; set; }
        public int Quantity { get; set; }
    }
}
