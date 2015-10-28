using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BActivityTaocan
    {
        public Marketing_Activity_Taocan ATaocan { get; set; }
        public BAgentRoute Route { get; set; }
        public int UsedCount { get; set; }
        public int SentOutCount { get; set; }
    }
}
