using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BStockBatch:BModel
    {
        public long ID { get; set; }
        public BProduct Product { get; set; }
        public BShop Shop { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}
