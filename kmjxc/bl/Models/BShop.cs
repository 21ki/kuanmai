using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BShop:BModel
    {
        public string Mall_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<BShop> Chindren { get; set; }
        public BShop Parent { get; set; }
        public Mall_Type Type { get; set; }
        public long Created { get; set; }
        public long Synced { get; set; }
        public BUser Created_By { get; set; }
        public string Mobile { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }
}
