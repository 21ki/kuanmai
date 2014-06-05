using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;

namespace KM.JXC.Web.Controllers.api
{
    public class PermissionController : BaseApiController
    {
        [HttpPost]
        public ApiMessage GetAdminRoles()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            List<BAdminRole> roles = new List<BAdminRole>();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            PermissionManagement permissionMgt = new PermissionManagement(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            bool enabled = true;
            if (request["enabled"] != null && request["enabled"] == "0")
            {
                enabled = false;
            }
            int shop = 0;
            int.TryParse(request["shop"],out shop);
            try
            {
                roles = permissionMgt.GetAdminRoles(shop, enabled);
                message.Item = roles;
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public ApiMessage GetAdminActions()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            List<BAdminCategoryAction> actions = new List<BAdminCategoryAction>();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            PermissionManagement permissionMgt = new PermissionManagement(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int role_id = 0;
            int.TryParse(request["role"],out role_id);
            try
            {
                actions = permissionMgt.GetActionsByCategory(role_id);
                message.Item = actions;
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public ApiMessage UpdateUserRoles()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };            
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            PermissionManagement permissionMgt = new PermissionManagement(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int[] role_id = null;
            int uid = 0;
            
            int.TryParse(request["user"], out uid);
            try
            {
                role_id = this.ConvertToIntArrar(request["roles"]);
                if (role_id != null && role_id.Length > 0)
                {
                    permissionMgt.UpdateUserRoles(role_id, uid);
                }
                else
                {
                    message.Status = "failed";
                    message.Message = "请选择正确的权限分组";
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public ApiMessage UpdateRoleActions()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };            
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            PermissionManagement permissionMgt = new PermissionManagement(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            
            int role_id = 0;
            int[] actions = null;
            int.TryParse(request["role"], out role_id);
            try
            {
                actions = this.ConvertToIntArrar(request["actions"]);
                if (actions != null)
                {
                    permissionMgt.UpdateRoleActions(role_id, actions);
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }

        [HttpPost]
        public ApiMessage CreateRole()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            List<BAdminRole> roles = new List<BAdminRole>();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            PermissionManagement permissionMgt = new PermissionManagement(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            string role_name = request["role"];
            string desc=request["desc"];
            int shop_id = 0;
            int.TryParse(request["shop_id"],out shop_id);
            try
            {
                int[] actions = this.ConvertToIntArrar(request["actions"]);
                if (string.IsNullOrEmpty(role_name))
                {
                    message.Status = "failed";
                    message.Message = "分组名字不能为空";
                    return message;
                }
                BAdminRole role=permissionMgt.CreateRole(role_name, desc,actions, shop_id);
                message.Item = role;

            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch
            {
            }
            finally
            {

            }
            return message;
        }
    }
}