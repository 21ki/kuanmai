using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.BL
{
    public class AdminActionAttribute : System.Attribute
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public string ActionDescription { get; set; }
    }

    public class Permissions
    {
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "新建用户")]
        public int CREATE_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "删除用户")]
        public int DELETE_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "禁用用户")]
        public int DISABLE_USER = 0;
        [AdminActionAttribute(ID = 1, CategoryName = "用户管理", ActionDescription = "更新用户")]
        public int UPDATE_USER;

        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "新建资源")]
        public int CREATE_RESOURCE;
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "更新资源")]
        public int UPDATE_RESOURCE;
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "删除资源")]
        public int DELETE_RESOURCE;
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "创建资源套餐")]
        public int CREATE_RESOURCE_TAOCAN;
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "更新资源套餐")]
        public int UPDATE_RESOURCE_TAOCAN;
        [AdminActionAttribute(ID = 2, CategoryName = "资源管理", ActionDescription = "删除资源套餐")]
        public int DELETE_RESOURCE_TAOCAN;

        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "新建代理商路由")]
        public int CREATE_USER_ROUTE = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "删除代理商路由")]
        public int DELETE_USER_ROUTE = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "更新代理商路由")]
        public int UPDATE_USER_ROUTE = 0;
        [AdminActionAttribute(ID = 3, CategoryName = "代理商路由管理", ActionDescription = "更新代理商费率")]
        public int UPDATE_USER_FEE = 0;

    }
}
