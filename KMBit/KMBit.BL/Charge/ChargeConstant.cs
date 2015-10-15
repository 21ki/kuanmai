using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.BL.Charge
{
    public class ChargeConstant
    {
        public const string CHARGING = "充值中";
        public const string SUCCEED_CHARGE = "充值成功";
        public const string AGENT_NOT_ENOUGH_MONEY = "代理商账户余额不足，不能充值，请先到平台充值缴费";
        public const string AGENT_NOT_ENOUGH_CREDIT = "后付款代理商信用额度不足，不能充值，请先到平台充值缴费";
        public const string AGENT_NOT_BIND_IP = "代理商没有绑定IP";
        public const string AGENT_IP_NOT_MATCH = "代理商当前使用的IP和绑定的IP不一致，不能充值";
        public const string AGENT_WRONG_NAME = "代理商账户不存在";
        public const string AGENT_WRONG_PASSWORD = "代理商账户密码错误";
        public const string AGENT_RUOTE_DISABLED = "代理商套餐被冻结，目前不能用此套餐充值";
        public const string RESOURCE_NOT_ENOUGH_MONEY = "平台落地账户余额不足，不能充值，请立即联系平台管理员";
        public const string MOBILE_SP_NOT_MATCH = "不能充值此号码所对应的运营商的手机流量";
        public const string MOBILE_CITY_NOT_MATCH = "此号码的归属地和所选套餐归属地不符，不能充值";
        public const string RESOURCE_PRODUCT_NOT_EXIST = "落地资源没有此套餐产品";
        public const string RESOURCE_INTERFACE_NOT_CONFIGURED = "落地资源接口没有在Resource_Inteface里配置";
        public const string RESOURCE_INTERFACE_APIURL_EMPTY = "落地资源接口URL为空，请在Resource_Inteface表里配置";
        public const string RESOURCE_TAOCAN_DISABLED = "此套餐被冻结，目前不能使用此套餐充值流量，请选择其他套餐";
        public const string RESOURCE_TAOCAN_NO_PDTID = "落地资源套餐产品编码不存在（经销商平台里产品唯一识别码）";
        public const string RESOURCE_DISABLED = "落地资源被冻结，不能使用，请联系平台管理员";
    }
}
