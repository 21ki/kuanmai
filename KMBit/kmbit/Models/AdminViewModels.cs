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
        public int Quantity { get; set; }

        [Display(Name = "省份")]
        public int? Province { get; set; }

        [Display(Name = "城市")]
        public int? City { get; set; }

        [Display(Name = "售价")]
        [DataType(DataType.Currency)]
        public float SalePrice { get; set; }

        [Display(Name = "成本价")]
        [DataType(DataType.Currency)]
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
}