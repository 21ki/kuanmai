using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using KMBit.Beans;
namespace KMBit.Models
{

    public class BigOrderSearchModel
    {
        public OrderSearchModel SearchModel { get;set;}
        public KMBit.Grids.KMGrid<BOrder> OrderGrid { get; set; }
    }

    public class OrderSearchModel
    {
        [Display(Name = "订单号")]
        public int? OrderId { get; set; }

        [Display(Name = "代理商")]
        public int? AgencyId { get; set; }

        [Display(Name = "资源")]
        public int? ResourceId { get; set; }

        [Display(Name = "套餐")]
        public int? ResourceTaocanId { get; set; }
        public int? RuoteId { get; set; }

        [Display(Name = "开始日期")]
        public string StartTime { get; set; }

        [Display(Name = "结束日期")]
        public string EndTime { get; set; }

        [Display(Name = "手机号码")]
        public string MobileNumber { get; set; }
        public string SPName { get; set; }
        public int Page { get; set; }

        [Display(Name = "状态")]
        public int[] Status { get; set; }
    }
    

    public class ResourceConfigModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "资源编号")]
        public int ResoucedId { get; set; }

        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "充值接口URL")]
        public string ApiUrl { get; set; }

        [Display(Name = "产品接口URL")]
        public string ProductFetchUrl { get; set; }

        [Required]
        [Display(Name = "平台接口名称")]
        public string InterfaceName { get; set; }

        [Required]
        [Display(Name = "平台接口DLL名称")]
        public string InterfaceAssemblyName { get; set; }

        [Required]
        [Display(Name = "回调URL")]
        public string CallBack { get; set; }       
    }

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

        public string Serial { get; set; }
    }
    public class ResourceModel
    {       
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
        
        [Display(Name = "地址")]
        public string Address { get; set; }
        
        [DataType(DataType.EmailAddress)]
        [Display(Name = "邮箱")]
        public string Email { get; set; }        
        
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
        public int Province { get; set; }
      
        [Display(Name = "城市")]
        public int City { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "电话")]
        public string Phone { get; set; }
       
        [Display(Name = "启用")]
        public bool Enabled { get; set; }

        [Display(Name = "备注")]
        public string Description { get; set; }
    }

    public class CreateAgentRouteModel
    {   
        public int Id { get; set; }

        [Required(ErrorMessage = "请制定代理商")]
        [Display(Name = "代理商")]
        public int AgencyId { get; set; }

        [Required(ErrorMessage ="请选择落地资源")]
        [Display(Name = "资源")]
        public int ResourceId { get; set; }

        [Required(ErrorMessage = "落地资源套餐必须至少选择一个")]
        [Display(Name = "资源套餐")]
        public int[] ResouceTaocans { get; set; }   

        [Required(ErrorMessage = "折扣必须填写，并且必须是0-1之间的小数")]
        [Display(Name = "折扣")]
        [Range(0.5, 1, ErrorMessage = "折扣必须在0.5-1之间")]
        public float Discount { get; set; }

        [Display(Name = "启用")]
        public bool Enabled { get; set; }
    }

    public class EditAgentRouteModel:CreateAgentRouteModel
    {
        [Required(ErrorMessage = "售价不能为空")]
        [Display(Name = "零售价")]
        [Range(0, float.MaxValue, ErrorMessage = "售价不能为空必须为数字，可以有小数点")]
        public float SalePrice { get; set; }
    }

    public class CreateAdminModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string Name { get; set; }

        [Required(ErrorMessage ="邮箱不能为空，并且邮箱不能重复，一个邮箱只能使用一次")]
        [DataType(DataType.EmailAddress, ErrorMessage="邮箱格式不正确")]
        [Display(Name = "邮箱")]
        public string Email { get; set; }
    }
}