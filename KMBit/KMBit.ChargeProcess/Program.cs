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
            while(true)
            {
                ProcessOrders();
            } 
        }

        static void ProcessOrders()
        {
            if(initialized)
            {
                OrdersProcesser.ProcessOrders();
                OrdersProcesser.ProcessAgentAccountChargePayments();
                initialized = false;
                last = DateTime.Now.AddMinutes(5);
            }
            else
            {
                if(last<=DateTime.Now)
                {
                    OrdersProcesser.ProcessOrders();
                    OrdersProcesser.ProcessAgentAccountChargePayments();
                    last = DateTime.Now.AddMinutes(5);
                }
            }
        }

        static void Process()
        {
            Thread thread = new Thread(OrdersProcesser.ProcessOrders);
            thread.Start();
        }
    }
}
