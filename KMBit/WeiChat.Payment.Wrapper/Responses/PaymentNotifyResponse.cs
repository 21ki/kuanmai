using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter.Responses
{
    public class PaymentNotifyResponse : BaseResponse
    {
        public TradeType trade_type { get; set; }
        public string bank_type { get; set; }
        public int total_fee { get; set; }
        public int settlement_total_fee { get; set; }
        public string fee_type { get; set; }
        public int cash_fee { get; set; }
        public string cash_fee_type { get; set; }
        public int coupon_fee { get; set; }
        public int coupon_count { get; set; }
        public string transaction_id { get; set; }
        public string out_trade_no { get; set; }
        public string time_end { get; set; }
        public string attach { get; set; }
        public string sub_mch_id { get; set; }
        public string is_subscribe { get; set; }
    }
}
