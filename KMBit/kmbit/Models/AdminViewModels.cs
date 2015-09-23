using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace KMBit.Models
{
    public class CreateResourceViewModel
    {
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
        
        [Display(Name = "运营商")]
        public int SP { get; set; }
       
        //[Display(Name = "状态")]
        //public int Enabled { get; set; }

        //[Display(Name = "创建时间")]
        //public int CreateTime { get; set; }

        //[Display(Name = "更新时间")]
        //public int UpdateTime { get; set; }
    }
}