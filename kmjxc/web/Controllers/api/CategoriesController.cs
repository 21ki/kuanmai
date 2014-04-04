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
        public IEnumerable<BCategory> GetCategories()
        {   
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

            if (id > 0)
            {
                categories = cateMgr.GetCategories(id);
            }
            return categories;
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

        [HttpPost]
        public ApiMessage Add()
        {           
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            Shop MainShop = userMgr.Main_Shop;
            ShopCategoryManager cateMgr = new ShopCategoryManager(userMgr.CurrentUser, MainShop, userMgr.CurrentUserPermission);

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
    }
}