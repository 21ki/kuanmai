using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
namespace WeChat.Adapter.Responses
{
    public class PreOrderResponse:BaseResponse
    {       
        public string prepay_id { get; set; }
        public string trade_type { get; set; }
    }
}
