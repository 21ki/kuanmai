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
            OrdersProcesser.ProcessOrders();
            OrdersProcesser.ProcessAgentAccountChargePayments();
            initialized = false;
            Thread.Sleep(5 * 60 * 1000);

            //if (initialized)
            //{
            //    OrdersProcesser.ProcessOrders();
            //    OrdersProcesser.ProcessAgentAccountChargePayments();
            //    initialized = false;
            //    //last = DateTime.Now.AddMinutes(5);
            //    Thread.Sleep(5*60*1000);
            //}
            //else
            //{
            //    if(last<=DateTime.Now)
            //    {
            //        OrdersProcesser.ProcessOrders();
            //        OrdersProcesser.ProcessAgentAccountChargePayments();
            //        last = DateTime.Now.AddMinutes(5);
            //        //Thread.Sleep(5 * 60 * 1000);
            //    }
            //}
        }

        static void Process()
        {
            Thread thread = new Thread(OrdersProcesser.ProcessOrders);
            thread.Start();
        }
    }
}
