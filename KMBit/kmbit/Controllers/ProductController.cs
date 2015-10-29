using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.Beans;
using KMBit.Models;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.Util;
using KMBit.BL.PayAPI.AliPay;
using KMBit.BL.Admin;
using KMBit.Filters;
namespace KMBit.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        [HttpGet]
        public ActionResult Charge(string message)
        {
            ViewBag.Message = message!=null?message: "";
            return View();
        }

        [HttpGet]
        public ActionResult Case(string message)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Charge(ChargeModel model)
        {
            if (ModelState.IsValid)
            {
                //ChargeBridge cb = new ChargeBridge();
                ChargeOrder order = new ChargeOrder()
                { ChargeType = 0, AgencyId = 0, Id = 0, Province = model.Province, City = model.City, MobileSP = model.SPName, MobileNumber = model.Mobile, OutId = "", ResourceId = 0, ResourceTaocanId = model.ResourceTaocanId, RouteId = 0, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                //
                OrderManagement orderMgt = new OrderManagement();
                ResourceManagement resourceMgr = new ResourceManagement(0);
                order = orderMgt.GenerateOrder(order);
                int total = 0;
                List<BResourceTaocan> taocans = resourceMgr.FindResourceTaocans(order.ResourceTaocanId, 0, 0, out total);
                if (taocans == null || taocans.Count == 0)
                {
                    ViewBag.Message = "当前套餐不可用";
                    return View();
                }
                BResourceTaocan taocan = taocans[0];
                //Redirct to the payment page.
                //TBD
                //After the payment is done then process below steps
                AlipayConfig config = new AlipayConfig(System.IO.Path.Combine(Request.PhysicalApplicationPath, "Config\\AliPayConfig.xml"));
                Submit submit = new Submit(config);

                SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                sParaTemp.Add("partner", config.Partner);
                sParaTemp.Add("seller_email", "372864472@qq.com");
                sParaTemp.Add("_input_charset", config.Input_charset.ToLower());
                sParaTemp.Add("service", "create_direct_pay_by_user");                
                sParaTemp.Add("payment_type", "1");
                sParaTemp.Add("notify_url", config.Notify_Url);
                sParaTemp.Add("return_url", config.Return_Url);
                sParaTemp.Add("out_trade_no", order.PaymentId.ToString());
                sParaTemp.Add("subject", string.Format("{0}M", taocan.Taocan.Quantity));
                sParaTemp.Add("total_fee", taocan.Taocan.Sale_price.ToString("0.00"));
                sParaTemp.Add("body", string.Format("{0}M", taocan.Taocan.Quantity));
                sParaTemp.Add("show_url", "");
                sParaTemp.Add("seller_id",config.Partner);
                //sParaTemp.Add("anti_phishing_key", "");
                //sParaTemp.Add("exter_invoke_ip", "");

                //建立请求
                string sHtmlText = submit.BuildRequest(sParaTemp, "get", "确认");
                //Response.Write("ok");
                Response.Clear();
                Response.Charset = "utf-8";
                Response.Write(sHtmlText);
                

                //ChargeResult result = cb.Charge(order);
                //ViewBag.Message = result.Message;
            }

            return View();
        }

        [IsPhoneFilter(Message ="只能通过手机访问")]
        [HttpGet]
        public ActionResult SaoMa()
        {            
            SortedDictionary<string, string> paras = GetRequestGet();
            ViewBag.Paras = paras;
            return View();
        }

        [IsPhoneFilter(Message = "只能通过手机访问")]
        [HttpGet]
        public ActionResult GuanzhuSaoMa()
        {
            return View();
        }

        public ActionResult Agent()
        {
            return View();
        }

        public ActionResult Code()
        {
            return View();
        }

        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message != null ? message : "";
            return View();
        }
    }
}