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
        public string Price { get; set; }
        public string Title { get; set; }
        public long Quantity { get; set; }
        public string OuterID { get; set; }
        public string PicUrl { get; set; }
        public List<BMallSku> Skus { get; set; }
        public int Created { get; set; }
        public int Modified { get; set; }
        public Shop Shop { get; set; }
        public BMallProduct()
        {
            this.Skus = new List<BMallSku>();
        }
    }

    public class BMallSku
    {
        public string MallProduct_ID { get; set; }
        public string SkuID { get; set; }
        public string PropertiesName { get; set; }
        public long Quantity { get; set; }
        public string OuterID { get; set; }
        public string Properities { get; set; }
        public double Price { get; set; }
    }
}
