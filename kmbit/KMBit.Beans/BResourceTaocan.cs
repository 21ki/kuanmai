using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BResourceTaocan
    {
        public Resource_taocan Taocan { get; set; }
        public Sp SP { get; set; }       
        public Area City { get; set; }
        public Users CreatedBy { get; set; }
        public Users UpdatedBy { get; set; }
    }
}
