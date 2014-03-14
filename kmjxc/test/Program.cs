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
            DateTime date = DateTime.Now;
            Console.WriteLine(date.ToLongTimeString());
            double time = DateTimeUtil.ConvertDateTimeToInt(date);
            Console.WriteLine(time);
        }
    }
}
