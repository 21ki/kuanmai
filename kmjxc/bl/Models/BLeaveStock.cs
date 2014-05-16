using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BLeaveStock:BModel
    {
        public int ID { get; set; }
        public List<BLeaveStockDetail> Details { get; set; }
        public BSale Sale { get; set; }        
        public BShop Shop { get; set; }
        public BUser Created_By { get; set; }
        public long LeaveDate { get; set; }
        public long Created { get; set; } 
        public int Status { get; set; }
        /// <summary>
        /// This field only used for create leave stock
        /// </summary>
        public bool UpdateStock { get; set; }
    }
}
