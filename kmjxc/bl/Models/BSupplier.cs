using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BSupplier:BModel
    {
        public int ID { get; set; }
        public long Created { get; set; }
        public BUser Created_By { get; set; }
        public BShop Shop { get; set; }              
        public string Name { get; set; }
        public string Address { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string ContactPerson { get; set; }
        public BArea Province { get; set; }
        public BArea City { get; set; }
        public bool Enable { get;set; }
        public string Remark { get; set; }
    }
}
