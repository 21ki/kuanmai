using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{    
    public class BBuyOrder:BModel
    {
        public int ID { get; set; }
        public Supplier Supplier { get; set; }
        public BUser Created_By { get; set; }
        public long WriteTime { get; set; }
        public long InsureTime { get; set; }
        public long EndTime { get; set; }
        public long Created { get; set; }
        public BShop Shop { get; set; }
        public BUser OrderUser { get; set; }
        public int Status { get; set; }
        public List<BBuyOrderDetail> Details { get; set; }
        public string Description { get; set; }
    }
}
