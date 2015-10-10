using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.BL.Charge
{
    public class ChargeConstant
    {
        public const string SUCCEED_CHARGE = "充值成功";
        public const string AGENT_NOT_ENOUGH_MONEY = "代理商账户余额不足，不能充值，请先到平台充值缴费";
        public const string AGENT_NOT_ENOUGH_CREDIT = "后付款代理商信用额度不足，不能充值，请先到平台充值缴费";
        public const string AGENT_WRONG_NAME = "代理商账户不存在";
        public const string AGENT_WRONG_PASSWORD = "代理商账户密码错误";
        public const string RESOURCE_NOT_ENOUGH_MONEY = "平台落地账户余额不足，不能充值，请立即联系平台管理员";
        public const string MOBILE_SP_NOT_MATCH = "不能充值此号码所对应的运营商的手机流量";
        public const string MOBILE_CITY_NOT_MATCH = "此号码的归属地和所选套餐归属地不符，不能充值";
    }
}
