using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;

namespace KM.JXC.BL.Models
{
    public class BEnterStock
    {
        public int ID { get; set; }
        public BShop Shop { get; set; }
        public BUser User { get; set; }
        public int BuyID { get; set; }        
        public BBuy Buy{get;set;}
        public int EnterTime { get; set; }
        public List<BEnterStockDetail> Details { get; set; }
        public Store_House StoreHouse { get; set; }

        /// <summary>
        /// This field only used for create enter stock
        /// </summary>
        public bool UpdateStock { get; set; }
    }
}
