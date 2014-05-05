using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BSupplier
    {
        public int ID { get; set; }
        public int Created { get; set; }
        public BUser Created_By { get; set; }
        public BShop Shop { get; set; }
        public bool IsParent { get; set; }       
        public string Name { get; set; }
        public string Address { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string ContactPerson { get; set; }
        public Common_District Province { get; set; }
        public Common_District City { get; set; }
        public bool Enable { get;set; }
        public string Remark { get; set; }
    }
}
