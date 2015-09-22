using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BResource
    {
        public Resource Resource { get; set; }
        public Area Province { get; set; }
        public Area City { get; set; }
        public Sp SP { get; set; }
        public Users CreatedBy { get; set; }
        public Users UpdatedBy { get; set; }
    }
}
