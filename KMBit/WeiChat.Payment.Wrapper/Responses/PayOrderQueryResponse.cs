using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
namespace WeChat.Adapter.Responses
{
    public class PayOrderQueryResponse : BaseResponse
    {
        public string device_info { get; set; }
        public string openid { get; set; }
        public string is_subscribe { get; set; }
        public TradeType trade_type { get; set; }
        public string bank_type { get; set; }
        public int total_fee { get; set; }
        public string fee_type { get; set; }
        public string transaction_id { get; set; }
        public string out_trade_no { get; set; }
        public string attach { get; set; }
        public string time_end { get; set; }
        public TradeState trade_state { get; set; }
    }
}
