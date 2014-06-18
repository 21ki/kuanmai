using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
namespace KM.JXC.Web.Controllers.api
{
    [Authorize]
    public class CategoriesController : ApiController
    {
        [HttpPost]
        public PQGridData GetCategories()
        {
            PQGridData data = new PQGridData();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);

           
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int id = 0;
            if (!string.IsNullOrEmpty(request["parent_id"]))
            {
                id = int.Parse(request["parent_id"].ToString());
            }

            List<BCategory> categories = new List<BCategory>();
            categories = cateMgr.GetCategories(id);
            data.curPage = 1;
            data.totalRecords = categories.Count;
            data.data = categories;
            return data;
        }

        [HttpGet]
        public IEnumerable<BCategory> GetOnlineCategories()
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BCategory> cates = cateMgr.GetOnlineCategories();
            List<BProperty> props = cateMgr.GetOnlineProperties();
            return cates;
        }

        public BCategory Get(int id)
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            BCategory cate = cateMgr.GetCategory(id);
            
            return cate;
        }

        public List<BProperty> GetProperties(int categoryId)
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            List<BProperty> properties = cateMgr.GetProperties(categoryId);
            return properties;
        }

        [HttpPost]
        public PQGridData GetPropertyValues()
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            PQGridData data = new PQGridData();
            BUser user = userMgr.CurrentUser;            
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int property_id = 0;
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int.TryParse(request["pid"], out property_id);
            data.data = cateMgr.GetPropertyValues(property_id);
            return data;
        }

        [HttpPost]
        public PQGridData GetPropertiesT()
        {
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            ApiMessage message = new ApiMessage();
            BUser user = userMgr.CurrentUser;            
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int categoryId = 0;
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int.TryParse(request["cid"], out categoryId);
            string sortBy = "";
            string dir = "";
            if (request["sortBy"] != null)
            {
                sortBy = request["sortBy"];
            }
            if (request["dir"] != null)
            {
                sortBy = request["dir"];
            }
            List<BProperty> properties = cateMgr.GetProperties(categoryId,0,sortBy,dir);
            PQGridData grid = new PQGridData();
            grid.curPage = 1;
            grid.totalRecords = properties.Count;
            grid.data = properties;
            message.Item = grid;
            message.Status="ok";
            return grid;
        }

        [HttpPost]
        public BProperty GetProperty()
        {
            BProperty property = new BProperty();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int propId = 0;
            int.TryParse(request["prop_id"],out propId);
            List<BProperty> properties = cateMgr.GetProperties(0,propId);
            if (properties.Count == 1)
            {
                property = properties[0];
            }

            return property;
        }

        [HttpPost]
        public ApiMessage CreateProperty()
        {
            BProperty property = new BProperty();
            ApiMessage message = new ApiMessage();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string categoryId = request["category_id"];
            string propName = request["prop_name"];
            string propValue = request["prop_value"];
            List<string> propValues = new List<string>();
            if (!string.IsNullOrEmpty(propValue))
            {
                string[] vs = propValue.Split(',');
                if (vs != null && vs.Length > 0)
                {
                    for (int i = 0; i < vs.Length; i++)
                    {
                        propValues.Add(vs[i]);
                    }
                }
            }

            try
            {
                property = cateMgr.CreateProperty(int.Parse(categoryId), propName, propValues);
                message.Item = property;
                message.Status = "ok";
            }
            catch (KM.JXC.Common.KMException.KMJXCException ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
                message.Item = null;
            }
            catch (Exception nex)
            {

            }
           
            return message;
        }

        [HttpPost]
        public ApiMessage AddNewPropValue()
        {
            ApiMessage message = new ApiMessage();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string propId = request["prop_id"];
            string propValues = request["prop_values"];
            if (!string.IsNullOrEmpty(propValues))
            {
                string[] values = propValues.Split(',');
                List<string> vs = new List<string>();

                if (values != null && values.Length > 0)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        vs.Add(values[i]);
                    }
                }

                try
                {
                    if (cateMgr.AddNewPropValue(int.Parse(propId), vs))
                    {
                        message.Status = "ok";
                    }
                    else
                    {
                        message.Status = "failed";
                    }
                }
                catch (KM.JXC.Common.KMException.KMJXCException ex)
                {
                    message.Status = "failed";
                    message.Message = ex.Message;
                }
            }
            return message;
        }

        [HttpPost]
        public ApiMessage Add()
        {           
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop cShop = userMgr.Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, cShop, userMgr.CurrentUserPermission);

            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            ApiMessage message = new ApiMessage();
            try
            {
                BCategory cate = new BCategory();
                cate.Name = request["name"].ToString();
                cate.ID = 0;
                cate.Mall_ID = "";
                cate.Mall_PID = "";
                cate.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                cate.Parent = new BCategory() { ID = int.Parse(request["parent_id"].ToString()) };
                cateMgr.CreateCategory(cate);
                message.Status = "ok";
                message.Message = "分类创建成功";
                message.Item = cate;
            }
            catch (KM.JXC.Common.KMException.KMJXCException ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "分类创建失败";
            }

            return message;
        }

        public void Post([FromBody]string value)
        {

        }

        [HttpPost]
        public ApiMessage DisableCategory()
        {
            ApiMessage message = new ApiMessage() { Status = "failed", Message = "" };
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);

            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            int category_id = 0;
            int.TryParse(request["cid"],out category_id);
            try
            {
                if (cateMgr.DisableCategory(category_id))
                {
                    message.Status = "ok";
                }
                else
                {
                    message.Message = "操作失败";
                }
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {

            }
            return message;
        }
    }
}