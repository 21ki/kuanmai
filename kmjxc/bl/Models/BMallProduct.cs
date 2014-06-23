using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BMallProduct
    {
        public string ID { get; set; }
        public double Price { get; set; }
        public string Title { get; set; }
        public long Quantity { get; set; }
        public int OuterID { get; set; }
        public string PicUrl { get; set; }
        public List<BMallSku> Skus { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public BShop Shop { get; set; }
        public BMallProduct()
        {
            this.Skus = new List<BMallSku>();
        }
        public string Description { get; set; }
        public BProduct Product { get; set; }
        public long Synced { get; set; }
        public long FirstSync { get; set; }
        public bool HasProductCreated { get; set; }
        public string Code { get; set; }
    }

    public class BMallSku
    {
        public string MallProduct_ID { get; set; }
        public string SkuID { get; set; }
        public string PropertiesName { get; set; }
        public long Quantity { get; set; }
        public int OuterID { get; set; }
        public string Properities { get; set; }
        public double Price { get; set; }
        public BProduct Product { get; set; }
        public string Code { get; set; }
    }
}
