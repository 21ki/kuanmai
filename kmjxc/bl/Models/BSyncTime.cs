using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BSyncTime
    {
        public long LastTradeStartEndTime { get; set; }
        public long LastTradeModifiedEndTime { get; set; }
        public long Last { get; set; }
        public BUser SyncUser { get; set; }
    }
}
