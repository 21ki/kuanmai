using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BUser:BModel
    {
        public int Parent_ID { get; set; }
        public BMallType Type { get; set; }
        public string Mall_Name { get; set; }
        public string Mall_ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public BEmployee EmployeeInfo { get; set; }
        public BUser Parent { get; set; }
        public long Created { get; set; }
        public BShop Shop { get; set; }
        public bool IsSystemUser { get; set; }
        public string NickName { get; set; }
    }
}
