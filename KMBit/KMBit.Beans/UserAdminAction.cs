using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class UserAdminAction
    {
        public Admin_Categories Category { get; set; }
        public Admin_Actions Action { get; set; }
    }
}
