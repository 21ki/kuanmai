using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BSaleSyncLog
    {
        public BShop Shop { get; set; }
        public int LastSyncTime { get; set; }
        public BUser User { get; set; }
        public int LastStartEndTime { get; set; }
        public int LastModifiedEndTime { get; set; }
        public int Type { get; set; }
    }
}
