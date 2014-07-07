using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BBuyPriceDetail
    {
        public BBuyPrice BuyPrice { get; set; }
        public long Created { get; set; }
        public double Price { get; set; }
        public BUser User { get; set; }
        public BSupplier Supplier { get; set; }
        public BProduct Product { get; set; }
        public string Desc { get; set; }
    }
}
