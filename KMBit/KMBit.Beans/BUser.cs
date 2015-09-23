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
        public Permissions Permission;
        public bool IsWebMaster;
        public bool IsSuperAdmin;
        public bool IsAdmin;
    }
}
