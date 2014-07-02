using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models.Admin
{
    public class BSysUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public BSysUser Modified_By { get; set; }
        public string Password { get; set; }
        public Permission Permission { get; set; }
    }
}
