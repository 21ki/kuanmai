using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBuyDetail
    {       
        public BBuy Buy { get; set; }
        public BUser User { get; set; }
        public BProduct Product { get; set; }
        public int ProductId { get; set; }
        public int Buy_Order_ID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public long CreateDate { get; set; }
    }
}
