﻿using System;
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
                { ChargeType = 0, AgencyId = 0, Id = 0, Province = model.Province, City = model.City, MobileSP = model.SPName, MobileNumber = model.Mobile, OutOrderId = "", ResourceId = 0, ResourceTaocanId = model.ResourceTaocanId, RouteId = 0, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                //
                OrderManagement orderMgt = new OrderManagement();
                ResourceManagement resourceMgr = new ResourceManagement(0);
                try
                {
                    order = orderMgt.GenerateOrder(order);
                }
                catch(KMBitException kex)
                {
                    ViewBag.Message = kex.Message;
                    return View();
                }
                catch(Exception ex)
                {
                    ViewBag.Message = "未知错误请联系系统管理员";
                    return View();
                }
                
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
            Dictionary<string, string> paras = new Dictionary<string, string>();
            ViewBag.Paras = paras;
            string p = Request.QueryString["p"];
            paras.Add("p", p);
            return View();
        }

        [IsPhoneFilter(Message = "只能通过手机访问")]
        [HttpPost]
        public ActionResult DoSaoMa()
        {
            string p = Request["p"];
            string number = Request["mobile_number"];
            string spName = Request["SPName"];
            string province= Request["Province"];
            string city = Request["City"];
           
            if (string.IsNullOrEmpty(p))
            {
                ViewBag.Message = "参数错误，请正确扫码，输入手机号码点充值";
            }           
            else
            {
                int agentId = 0;
                int customerId = 0;
                int activityId = 0;
                int activityOrderId = 0;
                string parameters = KMEncoder.Decode(p);
                if(!string.IsNullOrEmpty(parameters))
                {
                    string signature = string.Empty;                    
                    SortedDictionary<string, string> pvs = parseParameters(parameters,out signature);
                    if (string.IsNullOrEmpty(signature))
                    {
                        ViewBag.Message = "URL参数不正确，请重新扫码";
                        return View("SaoMa");
                    }
                    System.Text.StringBuilder pBuilder = new System.Text.StringBuilder();
                    if(pvs.Count>0)
                    {
                        int count = 1;
                        foreach(KeyValuePair<string,string> pair in pvs)
                        {
                            pBuilder.Append(pair.Key);
                            pBuilder.Append("=");
                            pBuilder.Append(pair.Value);
                            if(count<pvs.Count)
                            {
                                pBuilder.Append("&");
                            }
                            count++;                            
                            switch (pair.Key)
                            {
                                case "agentId":
                                    int.TryParse(pair.Value,out agentId);
                                    break;
                                case "customerId":
                                    int.TryParse(pair.Value, out customerId);
                                    break;
                                case "activityId":
                                    int.TryParse(pair.Value, out activityId);
                                    break;
                                case "activityOrderId":
                                    int.TryParse(pair.Value, out activityOrderId);
                                    break;
                            }
                        }
                        CustomerManagement customerMgr = new CustomerManagement(0);
                        int total;
                        List<BCustomer> customers = customerMgr.FindCustomers(0, customerId, out total);
                        if(total<=0 || total>1)
                        {
                            ViewBag.Message = "URL参数不正确，请重新扫码";
                            return View("SaoMa");
                        }
                        pBuilder.Append("&key=");
                        pBuilder.Append(customers[0].Token);
                        string sign = UrlSignUtil.GetMD5(pBuilder.ToString());
                        if(sign!=signature)
                        {
                            ViewBag.Message = "URL参数不正确，请重新扫码";
                            return View("SaoMa");
                        }
                        ActivityManagement activityMgr = new ActivityManagement(0);
                        BMarketOrderCharge order = new BMarketOrderCharge()
                        {
                            ActivityId = activityId,
                            ActivityOrderId = activityOrderId,
                            AgentId = agentId,
                            CustomerId = customerId,
                            City = city,
                            Province = province,
                            OpenId = "",
                            Phone = number,
                            SPName = spName
                        };
                        KMBit.BL.Charge.ChargeResult result = activityMgr.MarketingCharge(order);
                        ViewBag.Message = result.Message;
                        //if(result.Status == ChargeStatus.FAILED)
                        //{
                        //    ViewBag.Paras = pvs;
                        //    //paras.Add("p", p);
                        //}
                    }
                }else
                {
                    ViewBag.Message = "不能重复扫码，或者修改扫码后的URL地址";
                }
            }
           
            return View("SaoMa");
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

        public SortedDictionary<string,string> parseParameters(string paras,out string signature)
        {
            signature = string.Empty;
            SortedDictionary<string, string> ps = new SortedDictionary<string, string>();
            if(!string.IsNullOrEmpty(paras))
            {
                string[] array = paras.Split('&');
                if(array!=null && array.Length>0)
                {
                    foreach(string p in array)
                    {
                        string[] pv = p.Split('=');
                        if(pv!=null && pv.Length==2)
                        {
                            if(pv[0]== "signature")
                            {
                                signature = pv[1];
                            }
                            else
                            {
                                ps[pv[0]] = pv[1];
                            }                            
                        }
                    }
                }
            }
            return ps;
        }
    }
}