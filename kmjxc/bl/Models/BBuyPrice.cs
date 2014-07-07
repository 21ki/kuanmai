using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BBuyPrice:BModel
    {
        public BUser User { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public BShop Shop { get; set; }
        public List<BBuyPriceDetail> Details { get; set; }
    }
}
