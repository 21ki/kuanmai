using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Net.Http;
using log4net;
using KMBit.BL.PayAPI.AliPay;
using KMBit.Beans;
using KMBit.BL;
using KMBit.Util;
using KMBit.BL.Charge;
using WeChat.Adapter;
using WeChat.Adapter.Responses;
namespace KMBit.Controllers
{
    public class PaymentController : Controller
    {
        ILog logger = KMLogger.GetLogger();
        // GET: PayBack此方法仅供直冲用户支付宝支付完成回调以及支付宝充值账户回调
        public ActionResult AlipayBack()
        {
            SortedDictionary<string, string> sPara = GetRequestGet();
            ChargeResult result = new ChargeResult() { Status= ChargeStatus.FAILED };
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                AlipayConfig config = new AlipayConfig(System.IO.Path.Combine(Request.PhysicalApplicationPath, "Config\\AliPayConfig.xml"));
                Notify aliNotify = new Notify(config);
                bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);
                
                if (verifyResult)//验证成功
                {
                    //本地系统支付号
                    string out_trade_no = Request.QueryString["out_trade_no"];
                    int paymentId = 0;
                    int.TryParse(out_trade_no, out paymentId);
                    
                    //支付宝交易号
                    string trade_no = Request.QueryString["trade_no"];

                    //买家支付宝账户
                    string buyerAccount = Request.QueryString["buyer_email"];
                    //交易状态
                    string trade_status = Request.QueryString["trade_status"];
                    if (Request.QueryString["trade_status"] == "TRADE_FINISHED" || Request.QueryString["trade_status"] == "TRADE_SUCCESS")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        OrderManagement orderMgr = new OrderManagement(0);
                        PaymentManagement payMgr = new PaymentManagement(0);
                        if(paymentId>0)
                        {                            
                            try
                            {
                                BPaymentHistory payment = null;
                                int total = 0;
                                List<BPaymentHistory> payments = payMgr.FindPayments(paymentId,0, 0,out total);
                                if(payments!=null && payments.Count==1)
                                {
                                    payment = payments[0];
                                    if(payment.PayType==0)//直冲用户支付
                                    {
                                        result = orderMgr.ProcessOrderAfterPaid(paymentId, trade_no, buyerAccount);
                                        result.Status = ChargeStatus.SUCCEED;
                                        result.Message = "支付成功，已经提交到充值系统，请耐心等待...";
                                        return Redirect("/Product/Charge?message=" + result.Message);
                                    }
                                    else if(payment.PayType==1)//代理商用户充值账户
                                    {
                                        payMgr.UpdateAccountMoneyAfterPayment(payment);
                                        return Redirect("/Agent/ChargeAccount?message=" + result.Message);
                                    }
                                   
                                }else
                                {
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = string.Format("支付号{0}在本系统中不存在",paymentId);
                                }
                               
                            }
                            catch(KMBitException e)
                            {
                                result.Message = e.Message;
                                result.Status = ChargeStatus.FAILED;
                            }
                            catch(Exception ex)
                            {
                                result.Message = ex.Message;
                                result.Status = ChargeStatus.FAILED;
                            }                            
                        }
                    }
                    else
                    {
                        result.Message = string.Format("支付宝支付失败：{0}",Request.QueryString["trade_status"]);
                        result.Status = ChargeStatus.FAILED;

                        //需要删除本地系统内的支付记录或者充值订单记录TBD
                    }                    
                }
                else//验证失败
                {
                    result.Message = "支付宝返回数据验证失败，请不要串改支付宝返回的数据";
                    result.Status = ChargeStatus.FAILED;
                    //需要删除本地系统内的支付记录或者充值订单记录TBD
                }
            }
            else
            {
                result.Message = "支付宝没有返回任何数据,充值失败";
                result.Status = ChargeStatus.FAILED;
            }
            return Redirect("/Product/Charge?message=" + result.Message);
        }

        public HttpResponseMessage WeChatPayBack()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            string returnXML = null;
            logger.Info("PaymentController.WeChatPayBack is being called by Wechat payment notify service.........................");
            Stream stream = Request.InputStream;
            if(stream!=null)
            {
                StreamReader rs = null;
                try
                {
                    rs = new StreamReader(stream);
                    string result = rs.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        logger.Info("Below is the data posted by wechat payment service");
                        logger.Info(result);
                        string paraValues = WeChatPaymentWrapper.ParsePaymentNotifySignParas(result);
                        logger.Info(string.Format("{0} needs to be signed",paraValues));
                        BaseResponse baseresponse = WeChatPaymentWrapper.ParsePaymentNotify(result);
                        PaymentNotifyResponse response = null;
                        if (baseresponse != null)
                        {
                            response = (PaymentNotifyResponse)baseresponse;
                        }
                        logger.Info(string.Format("Signature sent by wechat is {0}", response.sign));
                        WeChatPayConfig config = PersistentValueManager.config;
                        paraValues += "&key=" + config.ShopSecret;
                        string sign = UrlSignUtil.GetMD5(paraValues).ToUpper();
                        logger.Info(string.Format("Signature caculated by localsystem is {0}", sign));
                        if (sign!= response.sign)
                        {
                            logger.Error("Two signatures are different, the request was not sent by wechat payment system.");
                            returnXML = "<xml><return_code>FAIL</return_code><return_msg>签名不正确</return_msg></xml>";
                            resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                        }
                        logger.Info("Sign verification passed");
                        OrderManagement orderMgr = new OrderManagement(0);
                        PaymentManagement payMgr = new PaymentManagement(0);
                        int paymentId = 0;                        
                        int.TryParse(response.out_trade_no,out paymentId);
                        if(paymentId>0)
                        {
                            logger.Info("Going to process payment id"+paymentId);
                            ChargeResult cResult = null;
                            try
                            {
                                BPaymentHistory payment = null;
                                int total = 0;
                                List<BPaymentHistory> payments = payMgr.FindUnProcessedOnLinePayments(paymentId, 0, 0, out total);
                                if (payments != null && payments.Count == 1)
                                {
                                    payment = payments[0];
                                    if (payment.PayType == 0)//直冲用户支付
                                    {
                                        logger.Info("OpenId:"+response.openid);
                                        logger.Info("OpenTradeNo:" + response.transaction_id);
                                        cResult = orderMgr.ProcessOrderAfterPaid(paymentId, response.transaction_id, response.openid);
                                        logger.Info(cResult.Status.ToString());
                                        logger.Info(cResult.Message);
                                        if(cResult.Status== ChargeStatus.SUCCEED)
                                        {
                                            logger.Info("The payment status has been successfully synced to local system.");
                                            returnXML = "<xml><return_code>SUCCESS</return_code><return_msg>OK</return_msg></xml>";
                                            resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                                            return resp;
                                        }
                                        else
                                        {
                                            logger.Error(cResult.Message);
                                            returnXML = "<xml><return_code>FAIL</return_code><return_msg>unexpected error</return_msg></xml>";
                                            resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                                            return resp;
                                        }
                                    }                                   
                                }
                                else
                                {
                                    logger.Warn("Didn't find payment by id:"+paymentId);
                                    returnXML = "<xml><return_code>FAIL</return_code><return_msg>out_trade_no is wrong</return_msg></xml>";
                                    resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                                }

                            }
                            catch (KMBitException e)
                            {
                                returnXML = "<xml><return_code>FAIL</return_code><return_msg>unexpected error</return_msg></xml>";
                                resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                                logger.Error(e);   
                            }
                            catch (Exception ex)
                            {
                                returnXML = "<xml><return_code>FAIL</return_code><return_msg>unexpected error</return_msg></xml>";
                                resp.Content = new StringContent(returnXML, System.Text.Encoding.UTF8, "text/plain");
                                logger.Fatal(ex);
                            }
                        }
                    }
                    
                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                }
               
            }
            logger.Info("Done...................");
            return resp;
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
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
    }
}