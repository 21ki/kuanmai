using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models.Admin
{
    public class BSysUser:BUser
    {      
        public BSysUser Modified_By { get; set; }
        public Permission Permission { get; set; }       
    }
}
