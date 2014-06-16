using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BCustomer:BUser
    {
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public Common_District City { get; set; }
        public Common_District Province { get; set; }
        public long Created { get; set; }
    }
}
