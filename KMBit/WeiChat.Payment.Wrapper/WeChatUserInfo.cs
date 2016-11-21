using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter
{
    public class WeChatUserInfo
    {
        public string UnionId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public int Gendar { get; set; }
        public string AvtorUrl { get; set; }
    }
}
