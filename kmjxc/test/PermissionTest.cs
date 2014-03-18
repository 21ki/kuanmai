using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.BL;

namespace test
{
    public class PermissionTest
    {
        public PermissionManager pm = new PermissionManager();

        public void SyncPermissionsWithActions()
        {
            pm.SyncPermissionWithAction();
        }
    }
}
