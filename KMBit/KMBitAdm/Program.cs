using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.BL;
using KMBit.Beans;
namespace KMBitAdm
{
    class Program
    {
        static void Main(string[] args)
        {
            test();
            return;

            if (args.Length==0)
            {
                Console.WriteLine("Please provide the command.");
                return;
            }

            string command = args[0];

            switch(command)
            {
                case "syncpermissions":
                    //PermissionManagement.SyncPermissionsWithDB();
                    break;
                default:
                    break;
            }
        }

        static void test()
        {
            ResourceManagement rmgt = new ResourceManagement(3);
            List<BResource> resources = rmgt.FindResource(2,"xx");
        }
    }
}
