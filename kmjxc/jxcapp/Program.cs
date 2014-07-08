using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.BL.Models;
using KM.JXC.BL;
namespace KM.JXC.jxcapp
{
    class Program
    {
        static void Main(string[] args)
        {
            PermissionManagement.SyncPermissionWithAction();
            PermissionManagement.SyncUserAction();
        }
    }
}
