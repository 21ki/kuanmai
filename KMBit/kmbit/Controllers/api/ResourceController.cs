using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL.Admin;
using KMBit.BL;
namespace KMBit.Controllers.api
{
    public class ResourceController : BaseApiController
    {
        [HttpPost]
        [AcceptVerbs("POST")]
        public ApiMessage GetResourceTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage() { Status = "ERROR", Message = "获取资源套餐数据失败", Item = null };
            try
            {
                int agencyId = 0;
                int resourceId = 0;
                if (this.request["resourceId"] != null)
                {
                    int.TryParse(this.request["resourceId"], out resourceId);
                }
                if (this.request["agencyId"] != null)
                {
                    int.TryParse(this.request["agencyId"], out agencyId);
                }

                if (resourceId > 0)
                {
                    KMBit.BL.Admin.ResourceManagement resourceMgt = new BL.Admin.ResourceManagement(User.Identity.Name);
                    int total = 0;
                    List<BResourceTaocan> taocans = resourceMgt.FindResourceTaocans(resourceId, agencyId);
                    message.Status = "OK";
                    message.Message = "成功获取用户类型数据，请使用JSON Item节点数据";
                    message.Item = taocans;
                }
                else
                {
                    message.Message = "资源编号必须是大于零的整数";
                }

            }
            catch
            { }

            return message;
        }
    }
}
