using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BBuyPriceDetailProduct
    {
        public BProduct Product { get; set; }
        public List<BSupplier> Suppliers { get; set; }
        public List<BBuyPriceDetail> Details { get; set; }
    }
}
