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
using WeChat.Adapter.Authorization;
using log4net;

namespace KMBit.Controllers
{
    public class WeChatTestController : Controller
    {
        ILog logger = KMLogger.GetLogger();
        // GET: WeChat
        public ActionResult Index()
        {
            logger.Info(this.GetType().FullName + " - Index....");
            WeChatChargeModel model = new WeChatChargeModel();
            //get weichat account openid from redirect URL parameters
            if (!string.IsNullOrEmpty(Request["openid"]))
            {
                model.OpenId = Request["openid"].ToString();
                Session["wechat_openid"] = model.OpenId;
            }
            else
            {
                if (Session["wechat_openid"] != null)
                {
                    logger.Info("already has wechat openid stored.");
                    model.OpenId = Session["wechat_openid"] != null ? Session["wechat_openid"].ToString() : "";
                }
                else
                {
                    string code = Request["code"];
                    if (!string.IsNullOrEmpty(code))
                    {
                        logger.Info("new request from wechat public account, code is " + code);
                        AccessToken weChatAccessToken = AuthHelper.GetAccessToken(PersistentValueManager.config, code);
                        if (weChatAccessToken != null)
                        {
                            logger.Info("get wechat account openid by code " + code);
                            model.OpenId = weChatAccessToken.OpenId;
                            Session["wechat_openid"] = model.OpenId;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.OpenId))
            {
                logger.Info("Wechat OpenId:" + model.OpenId);
            }
            else
            {
                logger.Error("Wechat OpenId is empty, cannot processing any more.");
            }
            model.nancestr = Guid.NewGuid().ToString().Replace("-", "");
            if (model.nancestr.Length > 32)
            {
                model.nancestr = model.nancestr.Substring(0, 32);
            }
            model.appid = PersistentValueManager.config.APPID;
            model.timestamp = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now).ToString();
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            JSAPITicket ticket = PersistentValueManager.GetWeChatJsApiTicket();
            if (ticket != null)
            {
                param.Add("jsapi_ticket", ticket.Ticket);
                logger.Info("jsapi_ticket:" + ticket.Ticket);
            }
            //param.Add("appId", model.appid);
            param.Add("timestamp", model.timestamp);
            param.Add("noncestr", model.nancestr);
            param.Add("url", Request.Url.AbsoluteUri.ToString());
            logger.Info(Request.Url.AbsoluteUri.ToString());
            //param.Add("jsApiList", "[chooseWXPay]");
            string sign = UrlSignUtil.SHA1_Hash(param);
            model.signature = sign;
            logger.Info("Done Index....");
            return View(model);
        }

        [HttpPost]
        public JsonResult PreCharge(WeChatChargeModel model)
        {
           
            logger.Info("WeChatController.PreCharge......................................................");
            ApiMessage message = new ApiMessage();
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.OpenId))
                {
                    message.Status = "ERROR";
                    message.Message = "请从公众号菜单打开此页面";
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                
                ChargeOrder order = new ChargeOrder()
                {
                    ChargeType = 0,
                    AgencyId = 0,
                    Id = 0,
                    Province = model.Province,
                    City = model.City,
                    MobileSP = model.SPName,
                    MobileNumber = model.Mobile,
                    OutOrderId = "",
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
                logger.Info(string.Format("Order is generated, Id - {0}, mobile - {1}", order.Id, order.MobileNumber));
                BResourceTaocan taocan = taocans[0];
                message.Status = "OK";
                message.Message = "预充值订单已经生成";
                message.Item = null;
                //
                string ip = Request.ServerVariables["REMOTE_ADDR"];
                if (ip != null && ip.IndexOf("::") > -1)
                {
                    ip = "127.0.0.1";
                }
                string prepayId = WeChatPaymentWrapper.GetPrepayId(PersistentValueManager.config, Session["wechat_openid"] != null ? Session["wechat_openid"].ToString() : "", order.PaymentId.ToString(), "TEST WECHATPAY", ip, (int)taocan.Taocan.Sale_price * 100, TradeType.JSAPI);
                logger.Info(string.Format("Prepay Id - {0}", prepayId));
                WeChatOrder weOrder = new WeChatOrder();
                weOrder.Order = new ChargeOrder { Id = order.Id, Payed = order.Payed, PaymentId = order.PaymentId, MobileNumber = order.MobileNumber, MobileSP = order.MobileSP, Province = order.Province };
                weOrder.PrepayId = prepayId;
                weOrder.PaySign = "";
                message.Item = weOrder;

                AccessToken token = PersistentValueManager.GetWeChatAccessToken();
                JSAPITicket ticket = PersistentValueManager.GetWeChatJsApiTicket();
                SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
                parameters.Add("appId", PersistentValueManager.config.APPID);
                parameters.Add("timeStamp", model.timestamp);
                parameters.Add("nonceStr", model.nancestr);
                parameters.Add("package", "prepay_id=" + prepayId);
                parameters.Add("signType", "MD5");

                logger.Info(string.Format("timeStamp:{0}", model.timestamp));
                logger.Info(string.Format("nonceStr:{0}", model.nancestr));
                logger.Info(string.Format("package:{0}", "prepay_id=" + prepayId));

                string querystr = null;
                foreach (KeyValuePair<string, string> para in parameters)
                {
                    if (querystr == null)
                    {
                        querystr = para.Key + "=" + para.Value;
                    }
                    else
                    {
                        querystr += "&" + para.Key + "=" + para.Value;
                    }
                }
                querystr += "&key=" + PersistentValueManager.config.ShopSecret;
                logger.Info(querystr);
                string sign = UrlSignUtil.GetMD5(querystr);
                model.paySign = sign.ToUpper();
                model.prepay_id = prepayId;
                logger.Info(string.Format("paySign:{0}", sign.ToUpper()));
                message.Item = model;
            }
            logger.Info("Done.");
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}