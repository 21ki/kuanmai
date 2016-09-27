using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter
{
    public class JSAPITicket
    {
        public string Ticket { get; set; }
        public int ExipresIn { get; set; }
        public DateTime ExpiresTime { get; set; }
    }
}
