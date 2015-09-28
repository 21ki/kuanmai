using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using KMBit.Beans;
using KMBit.BL;
using KMBit.DAL;
namespace KMBit.Controllers.api
{
    public class GeneralController : BaseApiController
    {
        // POST: General
        [HttpPost]
        [AcceptVerbs("POST")]
        public ApiMessage GetAreas()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage() { Status="ERROR", Message="",Item=null};
            try
            {
                BaseManagement baseMgt = new BL.BaseManagement(User.Identity.Name);
                int parentId = 0;
                int.TryParse(this.request["parentId"], out parentId);
                List<Area> areas = baseMgt.GetAreas(parentId);
                message.Status = "OK";
                message.Message = "成功获取地区数据，请使用JSON Item节点数据";
                message.Item = areas;
            }catch
            { }

            return message;
        }

        // GET: General
        [HttpGet]
        [AcceptVerbs("GET")]
        public ApiMessage GetSPs()
        {
            ApiMessage message = new ApiMessage() { Status = "ERROR", Message = "", Item = null };
            try
            {
                BaseManagement baseMgt = new BL.BaseManagement(User.Identity.Name);
                List<Sp> sps = baseMgt.GetSps();
                message.Status = "OK";
                message.Message = "成功获取运营商类型数据，请使用JSON Item节点数据";
                message.Item = sps;
            }
            catch
            { }

            return message;
        }

        // GET: General
        [HttpGet]
        [AcceptVerbs("GET")]
        public ApiMessage GetUserTypes()
        {
            ApiMessage message = new ApiMessage() { Status = "ERROR", Message = "获取用户类型数据失败", Item = null };
            try
            {
                BaseManagement baseMgt = new BL.BaseManagement(User.Identity.Name);
                List<User_type> sps = baseMgt.GetUserTypes();
                message.Status = "OK";
                message.Message = "成功获取用户类型数据，请使用JSON Item节点数据";
                message.Item = sps;
            }
            catch
            { }

            return message;
        }

        // GET: General
        [HttpGet]
        [AcceptVerbs("GET")]
        public ApiMessage GetPayTypes()
        {
            ApiMessage message = new ApiMessage() { Status = "ERROR", Message = "获取支付方式数据失败", Item = null };
            try
            {
                BaseManagement baseMgt = new BL.BaseManagement(User.Identity.Name);
                List<PayType> sps = baseMgt.GetPayTypes();
                message.Status = "OK";
                message.Message = "成功获取支付方式数据，请使用JSON Item节点数据";
                message.Item = sps;
            }
            catch
            { }

            return message;
        }

        [HttpPost]
        [AcceptVerbs("POST")]
        public ApiMessage GetResourceTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage() { Status = "ERROR", Message = "获取资源套餐数据失败", Item = null };
            try
            {
                int resourceId = 0;
                if (this.request["resourceId"] != null)
                {
                    int.TryParse(this.request["resourceId"], out resourceId);
                }

                if (resourceId > 0)
                {
                    KMBit.BL.Admin.ResourceManagement resourceMgt = new BL.Admin.ResourceManagement(User.Identity.Name);
                    int total = 0;
                    List<BResourceTaocan> taocans = resourceMgt.FindResourceTaocans(0, resourceId, 0, out total);
                    message.Status = "OK";
                    message.Message = "成功获取用户类型数据，请使用JSON Item节点数据";
                    message.Item = (from t in taocans select new { Id = t.Taocan.Id, Name = t.SP.Name + " " + t.City.Name + " " + t.Taocan.Quantity + "M" });
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