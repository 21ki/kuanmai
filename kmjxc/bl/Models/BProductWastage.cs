using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BProductWastage:BModel
    {
        public BProduct Product { get; set; }
        public int Quantity { get; set; }
    }
}
