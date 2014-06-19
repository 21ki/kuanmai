using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models.Admin
{
    public class BAdminBase
    {
        public int ID { get; set; }
        public long Created { get; set; }
        public BUser Created_By { get; set; }
        public long Modified { get; set; }
        public BUser Modified_By { get; set; }
    }
}
