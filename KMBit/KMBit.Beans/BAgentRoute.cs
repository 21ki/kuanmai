using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BAgentRoute
    {
        public Agent_route Route { get; set; }
        public BResourceTaocan Taocan { get; set; }
        public Users CreatedBy { get; set; }
        public Users UpdatedBy { get; set; }
    }
}
