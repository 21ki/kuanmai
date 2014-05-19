using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BProduct:BModel
    {
        public int ID { get; set; }
        public BUser User { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BCategory Category { get; set; }
        public Product_Unit Unit { get; set; }
        public decimal Price { get; set; }
        public long CreateTime { get; set; }
        public Shop Shop { get; set; }
        public BShop BShop { get; set; }
        public BProduct Parent { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public string Code { get; set; }
      
        /// <summary>
        /// this property used for child product
        /// </summary>
        public List<BProductProperty> Properties { get; set; }
        public List<BProduct> Children { get; set; }
        public int Quantity { get; set; }
        public List<Image> Images { get; set; }
        public string FileRootPath { get; set; }
    }
}
