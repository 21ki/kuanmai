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
using log4net;
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
        static ILog Logger = null;
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger = log4net.LogManager.GetLogger("Main...");
            if (args.Length==0)
            {
                Console.WriteLine("Please provide the command.");
                return;
            }

            string command = args[0];
            Logger.Info("command:"+command);
            switch (command)
            {
                case "syncpermissions":
                    PermissionManagement pgt = new PermissionManagement(3);
                    pgt.SyncPermissionsWithDB();
                    break;
                case "getstatus":                    
                    GetStatus();
                    break;
                default:
                    break;
            }
        }

        static void GetStatus()
        {
            ChargeBridge bridge = new ChargeBridge();
            bridge.SyncChargeStatus();
        }

        static void test()
        {
            ResourceManagement rmgt = new ResourceManagement(3);
            int total;
            List<BResource> resources = rmgt.FindResources(2,"xx",0,out total);
        }
    }
}
