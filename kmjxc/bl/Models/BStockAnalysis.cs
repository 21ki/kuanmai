using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BStockAnalysis
    {
        public BCategory Category { get; set; }
        public int ProductCount { get; set; }
        public double ProductCountPercentage { get; set; }
        public double FiscalAmount { get; set; }
        public double FiscalAmountPercentage { get; set; }
    }
}
