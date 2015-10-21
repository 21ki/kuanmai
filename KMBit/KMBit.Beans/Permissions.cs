using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace KMBit.Beans
{
    public class AdminActionAttribute : System.Attribute
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public string ActionDescription { get; set; }
    }

    public class Permissions
    {
        [Display(Name = "新建用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "新建用户")]
        public bool CREATE_USER { get; set; }

        [Display(Name = "删除用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "删除用户")]
        public bool DELETE_USER { get; set; }

        [Display(Name = "禁用用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "禁用用户")]
        public bool DISABLE_USER { get; set; }

        [Display(Name = "启用用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "启用用户")]
        public bool ENABLE_USER { get; set; }

        [Display(Name = "更新用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "更新用户")]
        public bool UPDATE_USER { get; set; }

        [Display(Name = "更新用户密码")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "更新用户密码")]
        public bool UPDATE_USER_PASSWORD { get; set; }

        [Display(Name = "查询用户")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "查询用户")]
        public bool SEARCH_USER { get; set; }

        [Display(Name = "修改用户权限")]
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "修改用户权限")]
        public bool UPDATE_USER_PERMISSION { get; set; }

        [Display(Name = "查看资源")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "查看资源")]
        public bool VIEW_RESOURCE { get; set; }

        [Display(Name = "新建资源")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "新建资源")]
        public bool CREATE_RESOURCE { get; set; }

        [Display(Name = "更新资源")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "更新资源")]
        public bool UPDATE_RESOURCE { get; set; }

        [Display(Name = "配置资源API")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "配置资源API")]
        public bool CONFIGURE_RESOURCE { get; set; }

        [Display(Name = "删除资源")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "删除资源")]
        public bool DELETE_RESOURCE { get; set; }

        [Display(Name = "创建资源套餐")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "创建资源套餐")]
        public bool CREATE_RESOURCE_TAOCAN { get; set; }

        [Display(Name = "更新资源套餐")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "更新资源套餐")]
        public bool UPDATE_RESOURCE_TAOCAN { get; set; }

        [Display(Name = "删除资源套餐")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "删除资源套餐")]
        public bool DELETE_RESOURCE_TAOCAN { get; set; }

        [Display(Name = "导入资源套餐")]
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "导入资源套餐")]
        public bool IMPORT_RESOURCE_TAOCAN { get; set; }

        [Display(Name = "新建代理商路由")]
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "新建代理商路由")]
        public bool CREATE_USER_ROUTE { get; set; }

        [Display(Name = "删除代理商路由")]
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "删除代理商路由")]
        public bool DELETE_USER_ROUTE { get; set; }

        [Display(Name = "更新代理商路由")]
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "更新代理商路由")]
        public bool UPDATE_USER_ROUTE { get; set; }

        [Display(Name = "更新代理商费率")]
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "更新代理商费率")]
        public bool UPDATE_USER_FEE { get; set; }

        [Display(Name = "流量充值")]
        [AdminActionAttribute(ID = 4, CategoryName = "流量充值管理", ActionDescription = "流量充值")]
        public bool CHARGE_BYTE { get; set; }

        [Display(Name = "查询流量充值记录")]
        [AdminActionAttribute(ID = 4, CategoryName = "流量充值管理", ActionDescription = "查询流量充值记录")]
        public bool CHARGE_HISTORY { get; set; }
    }
}
