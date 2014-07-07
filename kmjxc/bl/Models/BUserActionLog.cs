using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BUserActionLog:BModel
    {
        public long ID{get;set;}
        public BUser User { get; set; }
        public BUserAction Action { get; set; }
        public string Description { get; set; }
        public BShop Shop { get; set; }
    }
}
