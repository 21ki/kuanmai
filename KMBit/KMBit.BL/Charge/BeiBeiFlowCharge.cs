using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL.Charge;
using KMBit.Util;
namespace KMBit.BL.Charge
{
    public class BeiBeiFlowCharge : ChargeService, ICharge, IStatus
    {       
        public void CallBack(List<WebRequestParameters> data)
        {
            if (data == null)
            {
                return;
            }
            string res = "";
            string resMessage = "";
            ChargeResult result = new ChargeResult();
            ChargeOrder order = new ChargeOrder();
            foreach (WebRequestParameters param in data)
            {
                switch (param.Name)
                {
                    case "resCode":
                        res = param.Value;
                        break;
                    case "redMsg":
                        resMessage = param.Value;
                        break;
                    case "orderNo":
                        int oid = 0;
                        int.TryParse(param.Value, out oid);
                        order.Id = oid;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(res))
            {
                switch (res.ToLower())
                {
                    case "00":
                        result.Status = ChargeStatus.SUCCEED;
                        result.Message = ChargeConstant.SUCCEED_CHARGE;
                        break;
                    default:
                        result.Status = ChargeStatus.FAILED;
                        result.Message = (resMessage!=null? resMessage:"充值失败");
                        break;
                }
            }
            else
            {
                //回调没有传入状态，本系统默认失败
                result.Message = "没有回调状态数据";
                result.Status = ChargeStatus.FAILED;
            }

            if (order.Id > 0)
            {
                ChangeOrderStatus(order, result, true);
                //sending back status if the invoked by agent api
                using (chargebitEntities db = new chargebitEntities())
                {
                    Charge_Order dbOrder = (from o in db.Charge_Order where o.Id == order.Id select o).FirstOrDefault<Charge_Order>();
                    if (dbOrder != null && !string.IsNullOrEmpty(dbOrder.CallBackUrl))
                    {
                        this.SendStatusBackToAgentCallback(dbOrder);
                    }
                }
            }
            else
            {
                throw new KMBitException("回调数据中没有本系统的订单号，所以不能更新本系统数据，此次调用为脏数据");
            }
        }

        public ChargeResult Charge(ChargeOrder order)
        {
            Logger.Info("KMBit.BL.Charge.BeiBeiFlowCharge.Charge...........................");
            ChargeResult result = new ChargeResult();
            ProceedOrder(order, out result);
            if (result.Status == ChargeStatus.FAILED)
            {
                return result;
            }
            List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
            bool succeed = false;
            result = new ChargeResult();
            chargebitEntities db = null;
            try
            {
                db = new chargebitEntities();
                Charge_Order corder = (from co in db.Charge_Order where co.Id == order.Id select co).FirstOrDefault<Charge_Order>();
                corder.Process_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id == order.ResourceTaocanId select t).FirstOrDefault<Resource_taocan>();
                if (string.IsNullOrEmpty(taocan.Serial))
                {
                    Logger.Info("Resource taocan serial number is empty");
                    result.Message = ChargeConstant.RESOURCE_TAOCAN_NO_PDTID;
                    result.Status = ChargeStatus.FAILED;
                    return result;
                }
                ServerUri = new Uri(rInterface.APIURL);
                Logger.Info("API URL:"+ rInterface.APIURL);
                string md5Password = UrlSignUtil.GetMD5(KMAes.DecryptStringAES(rInterface.Userpassword));
                if (!string.IsNullOrEmpty(md5Password) && md5Password.Length>32)
                {
                    md5Password = md5Password.Substring(0, 32).ToLower();
                }
                StringBuilder needSignStr = new StringBuilder();
                needSignStr.Append("userName=");
                needSignStr.Append(rInterface.Username);
                needSignStr.Append("&userPwd=");
                needSignStr.Append(md5Password);
                needSignStr.Append(rInterface.AppSecret);
                needSignStr.Append("&mobile=");
                needSignStr.Append(order.MobileNumber);
                needSignStr.Append("&proKey=");
                needSignStr.Append(taocan.Serial);
                needSignStr.Append("&orderNo=");
                needSignStr.Append(order.Id.ToString());
                needSignStr.Append("&bcallbackUrl=");
                needSignStr.Append(rInterface.CallBackUrl);
                Logger.Info("Need signature string: "+ needSignStr.ToString());
                string sign = UrlSignUtil.GetMD5(needSignStr.ToString());
                Logger.Info("Signature: "+ sign);
                parmeters.Add(new WebRequestParameters("orderNo", order.Id.ToString(), false));
                parmeters.Add(new WebRequestParameters("mobile", order.MobileNumber, false));
                parmeters.Add(new WebRequestParameters("userName", rInterface.Username, false));
                parmeters.Add(new WebRequestParameters("userPwd", md5Password, false));                
                parmeters.Add(new WebRequestParameters("proKey", taocan.Serial, false));
                parmeters.Add(new WebRequestParameters("bcallbackUrl", rInterface.CallBackUrl, false));
                parmeters.Add(new WebRequestParameters("sign", sign, false));
                parmeters.Add(new WebRequestParameters("proKey", taocan.Serial, false));
                parmeters.Add(new WebRequestParameters("f", "recharge", false));
                parmeters.Add(new WebRequestParameters("flowType", "AF", false));
                parmeters.Add(new WebRequestParameters("businessType", "2", false));
                Logger.Info("Going to send request");
                SendRequest(parmeters, false, out succeed);
                Logger.Info("Request is completed");
                if (!string.IsNullOrEmpty(Response))
                {
                    Logger.Info("Response :"+Response);
                    JObject jsonResult = JObject.Parse(Response);
                    if(jsonResult!=null)
                    {
                        object code = jsonResult["code"];
                        if (code != null)
                        {
                            switch(code.ToString())
                            {
                                case "100":
                                    result.Status = ChargeStatus.ONPROGRESS;
                                    result.Message = ChargeConstant.CHARGING;
                                    break;
                                case "107":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_NOT_ENOUGH_MONEY;
                                    break;
                                case "102":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_API_PARAM_ERROR;
                                    break;
                                case "108":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_MOBILE_NOT_SUPPORT;
                                    break;
                                case "120":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_ACCOUNT_DISABLED;
                                    break;
                                case "121":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_API_CLOSED;
                                    break;
                                case "122":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_MAINTANCE;
                                    break;
                                case "205":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_AUTH_ERROR;
                                    break;
                                case "203":
                                    result.Status = ChargeStatus.FAILED;
                                    result.Message = ChargeConstant.RESOURCE_AUTH_SIGN_ERROR;
                                    break;
                            }
                        }
                    }
                    ChangeOrderStatus(order, result);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send request to resource API.");
                result.Status = ChargeStatus.FAILED;
                result.Message = "未知错误";
                Logger.Fatal(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return result;
        }

        public void GetChargeStatus(int resourceId)
        {
            throw new NotImplementedException();
        }

        public void ImportProducts(int resourceId, int operate_user)
        {
            throw new NotImplementedException();
        }
    }
}
