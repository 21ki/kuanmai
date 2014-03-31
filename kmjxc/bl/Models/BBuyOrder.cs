using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{    
    public class BBuyOrder
    {
        public int ID { get; set; }
        public Supplier Supplier { get; set; }
        public BUser User { get; set; }
        public int WriteTime { get; set; }
        public int InsureTime { get; set; }
        public int EndTime { get; set; }
        public int Created { get; set; }
        public Shop Shop { get; set; }
        public BUser OrderUser { get; set; }
        public int Status { get; set; }
        public List<BBuyOrderDetail> Details { get; set; }
        public string Description { get; set; }
    }
}
