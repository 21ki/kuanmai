using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BMallSync:BModel
    {
        public BUser User { get; set; }
        public BShop Shop { get; set; }
        public long SyncTime { get; set; }

        //0-sync products
        public int SyncType { get; set; }
    }
}
