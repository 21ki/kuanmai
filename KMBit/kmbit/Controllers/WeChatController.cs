using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.DAL;
using KMBit.Models;
using KMBit.Util;
using KMBit.Controllers.api;
using WeChat.Adapter.Responses;
using WeChat.Adapter.Requests;
using WeChat.Adapter;
namespace KMBit.Controllers
{
    public class WeChatController : Controller
    {
        // GET: WeChat
        public ActionResult Index()
        {
            WeChatChargeModel model = new WeChatChargeModel();
            //get weichat account openid from redirect URL parameters
            if (!string.IsNullOrEmpty(Request["openid"]))
            {
                model.OpenId = Request["openid"].ToString();
                Session["wechat_openid"] = model.OpenId;
            }
            else
            {
                model.OpenId = Session["wechat_openid"]!=null? Session["wechat_openid"].ToString():"";
            }
            ViewBag.nancestr = Guid.NewGuid().ToString();
            ViewBag.appid = Guid.NewGuid().ToString();
            return View(model);
        }

        [HttpPost]
        public JsonResult PreCharge(WeChatChargeModel model)
        {
            ApiMessage message = new ApiMessage();
            if (ModelState.IsValid)
            {
                //ChargeBridge cb = new ChargeBridge();
                ChargeOrder order = new ChargeOrder()
                {
                    ChargeType = 0,
                    AgencyId = 0,
                    Id = 0,
                    Province = model.Province,
                    City = model.City,
                    MobileSP = model.SPName,
                    MobileNumber = model.Mobile,
                    OutId = "",
                    ResourceId = 0,
                    ResourceTaocanId = model.ResourceTaocanId,
                    RouteId = 0,
                    CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                    Payed = false,
                    OpenId = model.OpenId,
                    OpenAccountType = 1
                };
                //
                OrderManagement orderMgt = new OrderManagement();
                ResourceManagement resourceMgr = new ResourceManagement(0);
                order = orderMgt.GenerateOrder(order);
                int total = 0;
                List<BResourceTaocan> taocans = resourceMgr.FindResourceTaocans(order.ResourceTaocanId, 0, 0, out total);
                if (taocans == null || taocans.Count == 0)
                {
                    message.Message = "当前套餐不可用";
                    message.Status = "ERROR";
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                BResourceTaocan taocan = taocans[0];
                message.Status = "OK";
                message.Message = "预充值订单已经生成";
                message.Item = null;
                //
                string prepayId = WeChatPaymentWrapper.GetPrepayId(new WeChatPayConfig(), order.PaymentId.ToString(), "12.44.55.66", (int)taocan.Taocan.Sale_price * 100, TradeType.JSAPI);
                WeChatOrder weOrder = new WeChatOrder();
                weOrder.Order = new ChargeOrder { Id = order.Id, Payed = order.Payed, PaymentId = order.PaymentId, MobileNumber = order.MobileNumber, MobileSP = order.MobileSP, Province = order.Province };
                weOrder.PrepayId = prepayId;
                weOrder.PaySign = "";
                message.Item = weOrder;
              
                
                WeChatPayConfig config = XMLUtil.DeserializeXML<WeChatPayConfig>(System.IO.Path.Combine(Request.PhysicalApplicationPath, "Config\\WeChatPayConfig.xml"));
                AccessToken token = Session["wechat_access_token"]!=null ? (AccessToken)Session["wechat_access_token"]:null;
                token = WeChatPaymentWrapper.GetWeChatToken(config, token);
                JSAPITicket ticket = Session["wechat_jsapi_ticket"] != null ? (JSAPITicket)Session["wechat_jsapi_ticket"] : null;
                ticket = WeChatPaymentWrapper.GetJSAPITicket(config, token, ticket);
                if(Session["wechat_access_token"]==null)
                {
                    Session["wechat_access_token"] = token;
                }
                if(Session["wechat_jsapi_ticket"]==null)
                {
                    Session["wechat_jsapi_ticket"] = ticket;
                }
                
            }
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}