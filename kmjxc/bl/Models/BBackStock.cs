using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BBackStock:BModel
    {
        public long Created { get; set; }
        public BShop Shop { get; set; }       
        public BUser Created_By { get; set; }
        public string Description { get; set; }
        public int BackSaleID { get; set; }
        public BBackSale BackSale { get; set; }
        public List<BBackStockDetail> Details { get; set; }
        public long BackDateTime { get; set; }

        /// <summary>
        /// Internal used only
        /// </summary>
        public bool UpdateStock { get; set; }
    }
}
