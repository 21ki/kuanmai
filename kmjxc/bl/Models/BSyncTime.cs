using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BSyncTime
    {
        public int First { get; set; }
        public int Last { get; set; }
        public BUser SyncUser { get; set; }
    }
}
