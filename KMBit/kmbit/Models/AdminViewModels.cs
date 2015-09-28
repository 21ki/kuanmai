using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace KMBit.Models
{
    public class ResourceTaocanModel
    {
        [Display(Name = "编号")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "资源编号")]
        public int ResoucedId { get; set; }

        [Display(Name = "运营商")]
        public int? SP { get; set; }

        [Required]
        [Display(Name = "流量大小")]
        [Range(1, int.MaxValue, ErrorMessage = "流量大小必须为整数，单位为M")]
        public int Quantity { get; set; }

        [Display(Name = "省份")]
        public int? Province { get; set; }

        [Display(Name = "城市")]
        public int? City { get; set; }

        [Display(Name = "售价")]
        [Required]
        [Range(1, float.MaxValue, ErrorMessage = "售价必须为数字，可带小数")]
        public float SalePrice { get; set; }

        [Display(Name = "成本价")]
        [Required]
        [Range(1, float.MaxValue, ErrorMessage = "成本价必须为数字，可带小数")]
        public float PurchasePrice { get; set; }

        [Display(Name = "是否启用")]
        [Required]
        public bool Enabled { get; set; }
    }
    public class ResourceModel
    {
        [Display(Name = "编号")]
        public int Id { get; set; }
            
        [Display(Name = "运营商")]
        [Required]
        public int SP { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "名称")]
        public string Name { get; set; }
       
        [DataType(DataType.MultilineText)]
        [Display(Name = "备注")]
        public string Description { get; set; }

        [Required]        
        [Display(Name = "省份")]
        public int Province { get; set; }

        [Required]
        [Display(Name = "城市")]
        public int City { get; set; }
        
        [DataType(DataType.Text)]
        [Display(Name = "地址")]
        public string Address { get; set; }
        
        [DataType(DataType.EmailAddress)]
        [Display(Name = "邮箱")]
        public string Email { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "联系电话")]
        public string Contact { get; set; }

        [Display(Name = "是否启用")]
        public bool Enabled { get; set; }   
    }

    public class CreateAgencyModel
    {        
        public int Id { get; set; }
       
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "邮箱")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "类型")]
        public int Type { get; set; }

        [Required]
        [Display(Name = "付款类型")]
        public int PayType { get; set; }

        [Display(Name = "省份")]
        public int? Province { get; set; }

        [Display(Name = "城市")]
        public int? City { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "电话")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "启用")]
        public bool Enabled { get; set; }

        [Display(Name = "备注")]
        public string Description { get; set; }
    }

    public class CreateAgentRouteModel
    {
       
        [Display(Name = "路由编号")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "代理商")]
        public int AgencyId { get; set; }

        [Required]
        [Display(Name = "资源")]
        public int ResourceId { get; set; }

        [Required]
        [Display(Name = "资源套餐")]
        public int[] ResouceTaocans { get; set; }
       
        [Display(Name = "零售价")]
        [Range(0.1,float.MaxValue,ErrorMessage ="零售价必须大于0")]
        public float SalePrice { get; set; }

        [Display(Name = "启用")]
        public bool Enabled { get; set; }
    }
}