using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BAddChildRequest:BModel
    {
        public BShop Parent { get; set; }
        public BShop Child { get; set; }
        public int Created { get; set; }
        public BUser Created_By { get; set; }
        public int Modified { get; set; }
        public BUser Modified_By { get; set; }
        public int Status { get; set; }
    }
}
