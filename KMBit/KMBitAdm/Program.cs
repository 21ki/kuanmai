using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.BL;
namespace KMBitAdm
{
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
                    //PermissionManagement.SyncPermissionsWithDB();
                    break;
                default:
                    break;
            }
        }
    }
}
