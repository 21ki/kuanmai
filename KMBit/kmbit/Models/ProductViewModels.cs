using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using KMBit.Beans;
namespace KMBit.Models
{
    public class ChargeModel
    {
        [Required(ErrorMessage ="请输入正确的手机号码")]
        [Display(Name = "手机号码")]
        public string Mobile { get; set; }

        [Required]
        [Display(Name = "运营商")]
        public string SPName { get; set; }

        [Display(Name = "省份")]
        public string Province { get; set; }

        [Display(Name = "城市")]
        public string City { get; set; }
        
        [Display(Name = "落地套餐编号")]
        public int ResourceTaocanId { get; set; }

        public int Operator { get; set; }

        public PayType PayType { get; set; }
    }

    public class WeChatChargeModel: ChargeModel
    {
        public string OpenId { get; set; }
        public string timestamp { get; set; }
        public string nancestr { get; set; }
        public string appid { get; set; }
        public string signature { get; set; }
        public string prepay_id { get; set; }
        public string paySign { get; set; }
    }
    public class AgentChargeModel:ChargeModel
    {
        [Display(Name = "路由编号")]
        public int RouteId { get; set; }
    }
}