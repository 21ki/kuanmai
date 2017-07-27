using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using KMBit.Beans;
using KMBit.BL;
namespace KMBit.ChargeProcess
{
    class Program
    {
        public static DateTime last = DateTime.Now;
        public static bool initialized = true;
        static void Main(string[] args)
        {
            Console.WriteLine("Order Processing service is started");
            OrdersProcesser.ProcessOrders();
            //OrdersProcesser.ProcessAgentAccountChargePaymentsNew();
            //return;
            while (true)
            {
                ProcessOrders();
            } 
        }

        static void ProcessOrders()
        {
            //OrdersProcesser.ProcessOrders();
            Console.WriteLine("Six threads will be started in every 8 seconds to process orders to gateway...");
            for (int i = 0; i <= 5; i++)
            {
                Thread t = new Thread(new ThreadStart(OrdersProcesser.ProcessOrders));
                t.Name = "OrderProcesser" + i;
                t.Start();
                Console.WriteLine("Thread " + t.Name + " is started.");
            }
            OrdersProcesser.ProcessAgentAccountChargePaymentsNew();
            Thread.Sleep(1 * 8 * 1000);            
        }

        static void Process()
        {
            Thread thread = new Thread(OrdersProcesser.ProcessOrders);
            thread.Start();
        }
    }
}
