using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BCategory
    {
        public int ID { get; set; }
        public string Mall_ID { get; set; }
        public string Mall_PID { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public long Created { get; set; }       
        public List<BCategory> Children { get; set; }
        public BCategory Parent { get; set; }
        public bool Enabled { get; set; }
        public BUser Created_By { get; set; }
        public bool IsMainShopProp { get; set; }
        public BShop Shop { get; set; }
    }
}
