using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BProductSupplier
    {
        public BSupplier Supplier { get; set; }
        public BProduct Product { get; set; }
    }
}
