using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
namespace KMBit.Beans
{
    public class BActivityOrder
    {
        public Marketing_Orders Order { get; set; }
        public string ActivityName { get; set; }
        public string TaocanName { get; set; }
    }
}
