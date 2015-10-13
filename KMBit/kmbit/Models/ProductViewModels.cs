using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace KMBit.Models
{
    public class DirectChargeModel
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

        [Required]
        [Display(Name = "落地套餐编号")]
        public int ResourceTaocanId { get; set; }
    }
}