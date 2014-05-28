using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BSyncTime
    {
        public int LastTradeStartEndTime { get; set; }
        public int LastTradeModifiedEndTime { get; set; }
        public int Last { get; set; }
        public BUser SyncUser { get; set; }
    }
}
