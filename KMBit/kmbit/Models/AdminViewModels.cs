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
       
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "AppKey")]
        public string AppKey { get; set; }

        [Display(Name = "AppSecret")]
        public string AppSecret { get; set; }

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

       
        public string Package { get; set; }

        [Display(Name = "运营商")]
        public int? SP { get; set; }

        [Required]
        [Display(Name = "流量大小")]
        [Range(1, int.MaxValue, ErrorMessage = "流量大小必须为整数，单位为M")]
        public int Quantity { get; set; }

        [Display(Name = "流量归属省份")]
        public int? Province { get; set; }

        [Display(Name = "流量归属城市")]
        public int? City { get; set; }

        [Display(Name = "号码归属省份")]
        public int? NumberProvince { get; set; }

        [Display(Name = "号码归属城市")]
        public int? NumberCity { get; set; }

        [Display(Name = "售价")]
        [Required]
        [Range(1, float.MaxValue, ErrorMessage = "售价必须为数字，可带小数")]
        
        public float SalePrice { get; set; }

        [Display(Name = "原价")]
        [Required]
        [Range(1, float.MaxValue, ErrorMessage = "代理价必须为数字，可带小数")]
        public float PurchasePrice { get; set; }

        [Display(Name = "使用折扣")]
        [Required]        
        public bool EnabledDiscount { get; set; }

        [Display(Name = "代理折扣")]
        [Range(0.1, 1, ErrorMessage = "代理折扣必须在0-1之间的数字可带小数")]
        public float Discount { get; set; }

        [Display(Name = "是否启用")]
        [Required]
        public bool Enabled { get; set; }

        [Display(Name = "资源包编号")]
        [Required(ErrorMessage ="落地资源流量包编号必须填写")]
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

    public class AdminChargeAccountModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "代理商名称不能为空")]
        [Display(Name = "代理商")]
        public string Name { get; set; }
        [Display(Name = "账户余额")]
        public float Remaining { get; set; }

        [Required(ErrorMessage = "充值金额不能为空")]
        [Display(Name = "充值金额")]
        [Range(0,20000,ErrorMessage ="充值金额必须在0-20000之间")]
        public float Amount { get; set; }
    }
    public class CreateAgencyModel
    {        
        public int Id { get; set; }
       
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage ="代理商名称不能为空")]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "邮件不能为空")]
        [DataType(DataType.EmailAddress,ErrorMessage ="邮件地址格式不正确")]
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

        [Required(ErrorMessage = "代理商电话不能为空")]
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

        [Required(ErrorMessage = "请指定代理商")]
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

    public class CreateCustomerModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="客户名称不能为空")]
        [Display(Name = "客户名称")]
        public string Name { get; set; }
       
        [Display(Name = "公众号类型")]
        public int? OpenType { get; set; }
        
        [Display(Name = "公众号")]
        public string OpenAccount { get; set; }

        [Required(ErrorMessage = "联系人不能为空")]
        [Display(Name = "联系人")]
        public string ContactPeople { get; set; }

        [Required(ErrorMessage = "联系电话不能为空")]
        [Display(Name = "手机号码")]
        [StringLength(13, ErrorMessage = "最多只能13个字")]
        public string ContactPhone { get; set; }

        [Display(Name = "联系地址")]
        [StringLength(45, ErrorMessage = "最多只能45个字")]
        public string ContactAddress { get; set; }

        [Display(Name = "电子邮件")]
        [DataType(DataType.EmailAddress,ErrorMessage ="邮箱地址无效")]
        [StringLength(45, ErrorMessage = "最多只能45个字")]
        public string ContactEmail { get; set; }

        [Display(Name = "备注")]
        [StringLength(200,ErrorMessage ="描述最多只能200个字")]
        public string Description { get; set; }

        [Display(Name = "额度")]
        public float CreditAmount { get; set; }

        [Display(Name = "余额")]
        public float Amount { get; set; }
    }

    public class CustomerRechargeModel
    {
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        [Required]
        [Display(Name = "当前余额")]
        public float CurrentAmount { get; set; }

        [Required]
        [Display(Name = "充值金额")]
        [Range(1,float.MaxValue,ErrorMessage ="充值金额必须是大于1的数字")]
        public float ChargeAmount { get; set; }
    }

    public class CustomerActivityModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "活动名称不能为空")]
        [Display(Name = "活动名称")]
        public string Name { get; set; }

        [Required(ErrorMessage = "扫码类型不能为空")]
        [Display(Name = "扫码类型")]
        public int ScanType { get; set; }

        [Display(Name = "开始日期")]
        public string StartTime { get; set; }

        [Display(Name = "结束日期")]
        public string ExpiredTime { get; set; }

        [StringLength(300, ErrorMessage = "描述最大只能300个字")]
        [Display(Name = "备注")]
        public string Description { get; set; }
        
        [Display(Name = "是否启用")]
        public bool Enable { get; set; }
    }

    public class ActivityTaocanModel
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required(ErrorMessage ="套餐必须选择")]
        [Display(Name = "套餐")]
        public int RouteId { get; set; }

        [Required(ErrorMessage ="数量不能为空，并且只能是数字")]
        [Display(Name = "数量")]
        [Range(1,int.MaxValue,ErrorMessage ="数量必须大于1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "价格不能为空，并且只能是数字")]
        [Display(Name = "价格")]
        [Range(0.1, int.MaxValue, ErrorMessage = "价格必须大于0，推荐大于您的代理成本价")]
        public float Price { get; set; }
    }
}