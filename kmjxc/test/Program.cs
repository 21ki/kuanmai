using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            KuanMaiEntities dba = new KuanMaiEntities();
            User u = new User();
            u.Name = "flysocket";
            u.Password = "test";
            u.Mall_ID = "12";
            u.Mall_Name = "kuanmai店";
            u.Mall_Type = 1;
            dba.User.Add(u);
            dba.SaveChanges();
        }
    }
}
