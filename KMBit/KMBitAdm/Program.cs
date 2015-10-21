using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.Beans;
using KMBit.BL.MobileLocator;
namespace KMBitAdm
{
    public interface Itest
    {
        void Print(string message);
    }
    public class P1:Itest
    {
        public void Print(string message)
        {
            Console.WriteLine(message);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length==0)
            {
                Console.WriteLine("Please provide the command.");
                return;
            }

            string command = args[0];

            switch(command)
            {
                case "syncpermissions":
                    PermissionManagement pgt = new PermissionManagement(3);
                    pgt.SyncPermissionsWithDB();
                    break;
                default:
                    break;
            }
        }

        static void test()
        {
            ResourceManagement rmgt = new ResourceManagement(3);
            int total;
            List<BResource> resources = rmgt.FindResources(2,"xx",0,out total);
        }
    }
}
