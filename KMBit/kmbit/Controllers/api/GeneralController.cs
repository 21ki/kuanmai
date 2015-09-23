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
    }
}