using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace KMBit.Beans
{
    public class BOrder
    {
        [Display(Name ="编号")]
        public int Id { get; set; }
        [Display(Name = "代理商编号")]
        public int AgentId { get; set; }
        [Display(Name = "代理商")]
        public string AgentName { get; set; }
        [Display(Name = "资源编号")]
        public int ResourceId { get; set; }
        [Display(Name = "资源名称")]
        public string ReseouceName { get; set; }
        [Display(Name = "资源套餐编号")]
        public int ResourceTaocanId { get; set; }
        [Display(Name = "代理商路由编号")]
        public int AgentRouteId { get; set; }
        public string TaocanName { get; set; }
        [Display(Name = "手机归属")]
        public string MobileSP { get; set; }
        [Display(Name = "手机号码")]
        public string MobilePhone { get; set; }
        [Display(Name = "流量")]
        public int Quantity { get; set; }
        [Display(Name = "订单时间")]
        public long CreatedTime { get; set; }
        [Display(Name = "处理时间")]
        public long ProceedTime { get; set; }
        [Display(Name = "充值时间")]
        public long CompletedTime { get; set; }
        public int Status { get; set; }
        [Display(Name = "充值状态")]
        public string StatusText { get; set; }

        public int ChargeType { get; set; }
        [Display(Name = "充值类型")]
        public string ChargeTypeText { get; set; }

        [Display(Name = "充值消息")]
        public string Message { get; set; }
        public int Operator { get; set; }
        [Display(Name = "操作员用户")]
        public string OperatorName { get; set; }
        [Display(Name = "代理售价")]
        public float SalePrice { get; set; }
        [Display(Name = "代理价格")]
        public float PurchasePrice { get; set; }
        [Display(Name = "成本价")]
        public float PlatformCostPrice { get; set; }
        [Display(Name = "售价")]
        public float PlatformSalePrice { get; set; }
        [Display(Name = "已支付")]
        public bool Payed { get; set; }

        [Display(Name = "已退款")]
        public bool Refound { get; set; }

        [Display(Name = "收益")]
        public float Revenue { get; set; }
    }
}
