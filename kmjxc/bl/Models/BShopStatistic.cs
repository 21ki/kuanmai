using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BShopStatistic
    {
        public BShop Shop { get; set; }
        public int LeaveStock { get; set; }
        public int BuyOrder { get; set; }
        public int BuyOrderUnhandled { get; set; }
        public int Buy { get; set; }
        public int BuyUnhandled { get; set; }
        public int BackSale { get; set; }
        public int BackSaleUnhandled { get; set; }
        public int BackStock { get; set; }
        public int BackStockUnhandled { get; set; }
        public int Trade { get; set; }
        public int Account { get; set; }
        public int ChildShop { get; set; }
    }
}
