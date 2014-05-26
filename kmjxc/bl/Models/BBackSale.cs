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
        public int BackTime { get; set; }
        public int Created { get; set; }
        public BSale Sale { get; set; }
        public BUser CreatedBy { get; set; }        
        public string Description { get; set;}
        public BShop Shop { get; set; }
        public List<BBackSaleDetail> Details { get; set; }
        public double Amount { get; set; }
        public BCustomer Buyer { get; set; }
        //Internal used only
        public bool GenerateBackStock { get; set; }
        public bool UpdateStock { get; set; }
    }
}
