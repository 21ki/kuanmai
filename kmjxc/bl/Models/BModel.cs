using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BModel
    {
        public int ID { get; set; }
        public bool FromMainShop { get; set; }
        public bool FromChildShop { get; set; }
        public long Created { get; set; }
    }
}
