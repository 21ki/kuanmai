using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BUser
    {
        public int ID { get; set; }
        public int Parent_ID { get; set; }
        public Mall_Type Type { get; set; }
        public string Mall_Name { get; set; }
        public string Mall_ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public Employee EmployeeInfo { get; set; }
        public BUser Parent { get; set; }
    }
}
