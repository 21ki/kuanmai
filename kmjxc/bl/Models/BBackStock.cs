using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBackStock
    {
        public int ID { get; set; }
        public int Created { get; set; }
        public BShop Shop { get; set; }
        public Store_House StoreHouse { get; set; }
        public BUser CreatedBy { get; set; }
        public string Description { get; set; }
        public int BackSaleID { get; set; }
        public BBackSale BackSale { get; set; }
        public List<BBackStockDetail> Details { get; set; }
    }
}
