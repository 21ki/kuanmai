using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBuy:BModel
    {
        public int ID { get; set; }
        public long ComeDate { get; set; }
        public long Created { get; set; }
        public string Description { get; set; }
        public BUser User { get; set; }
        public BShop Shop { get; set; }
        public List<BBuyDetail> Details { get; set; }
        public BBuyOrder Order { get; set; }
        public int Status { get; set; }
    }
}
