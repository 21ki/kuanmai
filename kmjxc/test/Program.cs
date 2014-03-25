using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.Common.Util;
namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            
            StockManager stockManager = new StockManager(new User { User_ID = 3 }, 1);
            int totalRecords = 0;
            List<BEnterStock> stocks = stockManager.GetEnterStocks(0,0, 0, 0, 0, 1, 30, out totalRecords);
            
        }
    }
}
