using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BEnterStockDetail
    {
        public BEnterStock EnterStock { get; set; }
        public BProduct Product { get; set; }
        public long Quantity { get; set; }
        public double Price { get; set; }
        public bool Invoiced { get; set; }
        public string InvoiceNumber { get; set; }
        public double InvoiceAmount { get; set; }
        public int Created { get; set; }
    }
}
