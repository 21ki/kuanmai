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
            UserManager userManager = new UserManager(1, null);
            User u1 = new User { Mall_Name = "test"+Guid.NewGuid().ToString(), Mall_ID = "2345423535", Mall_Type = 1, Name = "xxxx", Password = "f3435435", Parent_User_ID = 0 };
            u1 = userManager.CreateNewUser(u1);
            User u2 = u1;
            u2.Name="fuck you";

            userManager.UpdateUser(u2);
        }
    }
}
