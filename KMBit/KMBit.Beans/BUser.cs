using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;

namespace KMBit.Beans
{
    public class BUser
    {
        public Users User;
        public Permissions Permission { get; set; }
        public bool IsWebMaster { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsAdmin { get; set; }
        public Area Province { get; set; }
        public Area City { get; set; }
        public KMBit.DAL.PayType PayType { get; set; }
        public User_type UserType { get; set; }
        public Users CreatedBy { get; set; }
    }
}
