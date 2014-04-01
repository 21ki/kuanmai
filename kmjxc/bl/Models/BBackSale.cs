using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBackSale
    {
        public int ID { get; set; }
        public int Date { get; set; }
        public BTrade Trade { get; set; }
        public BUser CreatedBy { get; set; }
        public Store_House StoreHouse { get; set; }
        public string Description { get; set;}
        public BShop Shop { get; set; }
        public List<BBackSaleDetail> Details { get; set; }
    }
}
