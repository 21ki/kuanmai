using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter.Responses
{
    public class JSAPITicketResponse:BaseResponse
    {
        public JSAPITicket Ticket { get; set; }       
    }
}
