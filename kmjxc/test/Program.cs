using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.Common.Util;
namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            DateTime date = new DateTime(2050, 1, 1);
            long time = DateTimeUtil.ConvertDateTimeToInt(date);
            Console.WriteLine(time);
        }
    }
}
