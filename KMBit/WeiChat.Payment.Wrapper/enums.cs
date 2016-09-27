using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter
{
    public enum ResuleState {
        SUCCESS,
        FAIL,
        NONE
    }
    public enum TradeState {
        SUCCESS,
        REFUND,
        NOTPAY,
        CLOSED,
        REVOKED,
        USERPAYING,
        PAYERROR,
        NONE
    }
    public enum TradeType
    {
        JSAPI,
        NATIVE,
        APP,
        MICROPAY,
        NONE
    }
}
