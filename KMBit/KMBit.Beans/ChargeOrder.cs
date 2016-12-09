using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public enum PayType {
        NONE,
        WEIXIN,
        ALIPAY,
        NETBANK
    }
    public class ChargeBaseOrder
    {
        public ChargeBaseOrder()
        {
           
        }
        public int Id { get; set; }
        public string MobileSP { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string MobileNumber { get; set; }
        public int ResourceId { get; set; }
        public int ResourceTaocanId { get; set; }
        public string OutOrderId { get; set; }//落地平台的订单ID
        public int OperateUserId { get; set; }
        public long CreatedTime { get; set; }
        public int ChargeType { get; set; }
        public bool Payed { get; set; }
        public int PaymentId { get; set; }

        public int MarketOrderId { get; set; }
        public bool IsMarket { get; set; }

        public string MacAddress { get; set; }

        public string CallbackUrl { get; set; }
    }

    public class ChargeOrder: ChargeBaseOrder
    {
        public int AgencyId { get; set; }
        public int RouteId { get; set; }
        public string OpenId { get; set; }
        public int OpenAccountType { get; set; }
        public string ClientOrderId { get; set; }//客户系统的订单ID
        public int Status { get; set; }
    }
}
