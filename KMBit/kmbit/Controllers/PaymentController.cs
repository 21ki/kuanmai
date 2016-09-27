using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMBit.BL.PayAPI.AliPay;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;
namespace KMBit.Controllers
{
    public class PaymentController : Controller
    {
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

        public JsonResult WeiChatPayBack()
        {
            return Json("", JsonRequestBehavior.AllowGet);
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