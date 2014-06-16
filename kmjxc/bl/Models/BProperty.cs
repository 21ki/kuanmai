using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BProperty:BModel
    {
        public string Name { get; set; }
        public long Created { get; set; }
        public BUser Created_By { get; set; }
        public BShop Shop { get; set; }
        public string MID { get; set; }
        public List<Product_Spec_Value> Values { get; set; }
        public int CategoryId { get; set; }
        public Product_Class Category { get; set; }        
    }
}
