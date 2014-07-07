using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BUserAction:BModel
    {
        public int Action_ID { get; set; }
        public string Action_Name { get; set; }
        public string Action_Desc { get; set; }
    }
}
