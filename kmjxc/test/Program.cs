using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL;
using KM.JXC.Common.Util;
namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            PermissionTest pt = new PermissionTest();
            pt.SyncPermissionsWithActions();
        }
    }
}
