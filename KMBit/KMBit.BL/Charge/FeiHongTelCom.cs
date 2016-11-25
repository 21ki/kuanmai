using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.Util;
using WeChat.Adapter.Requests;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft;
namespace KMBit.BL.Charge
{
    public class FeiHongTelCom : ChargeService,ICharge,IStatus
    {
        private static string token = null;
        private static long expiredTime = 0;
        private long expiredInterval = 7 * 60*1000;
        private static object telcom = new object();
        private static int lastMaxGetStatusOrderId = 0;
        private static object getStatusObj = new object();
            
        public FeiHongTelCom()
        {
            Logger = log4net.LogManager.GetLogger(this.GetType());
        }

        public void CallBack(List<WebRequestParameters> data)
        {
            throw new NotImplementedException();
        }

        private void GetToken(Resrouce_interface api)
        {   
            lock(telcom)
            {
                if (DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) < expiredTime)
                {
                    return;
                }
                NameValueCollection col = new NameValueCollection();
                col.Add("reqToken", UrlSignUtil.GetMD5(api.Username + api.AppSecret));
                col.Add("id", api.Username);              
                string resStr=HttpSercice.PostHttpRequest(api.GetTokenUrl, col, WeChat.Adapter.Requests.RequestType.POST, null);                
                if (!string.IsNullOrEmpty(resStr))
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject jsonResult = Newtonsoft.Json.Linq.JObject.Parse(resStr);
                        string result = jsonResult["result"].ToString();
                        string rtoken = jsonResult["token"]!=null? jsonResult["token"].ToString():null;
                        string message = jsonResult["resultMsg"].ToString();
                        if(!string.IsNullOrEmpty(result) && result.Trim()=="0")
                        {
                            token = rtoken;
                            expiredTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now)+expiredInterval;
                        }
                        else
                        {
                            Logger.Error(message!=null?message:"Failed to request token.");
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.Fatal(ex);
                    }
                }
            }
        }
        public ChargeResult Charge(ChargeOrder order)
        {
            Logger.Info("Charging...");
            ChargeResult result = new ChargeResult() { Status = ChargeStatus.FAILED };
            chargebitEntities db = null;
            ProceedOrder(order, out result);
            if (result.Status == ChargeStatus.FAILED)
            {
                return result;
            }

            List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
            bool succeed = false;
            try
            {
                db = new chargebitEntities();
                Charge_Order corder = (from co in db.Charge_Order where co.Id == order.Id select co).FirstOrDefault<Charge_Order>();
                corder.Process_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id == order.ResourceTaocanId select t).FirstOrDefault<Resource_taocan>();

                if(string.IsNullOrEmpty(rInterface.APIURL))
                {
                    result.Status = ChargeStatus.FAILED;
                    result.Message = "资源充值API URL没有配置";
                    Logger.Error("Cannot get token.");
                    ChangeOrderStatus(order, result);
                    return result;
                }
                ServerUri = new Uri(rInterface.APIURL);
                if(string.IsNullOrEmpty(token))
                {
                    Logger.Info("Token doesn't exist, try to request one.");
                    GetToken(rInterface);
                }
                else
                {
                    Logger.Info("Token is already existed.");
                }
                if(DateTimeUtil.ConvertDateTimeToInt(DateTime.Now)> expiredTime)
                {
                    Logger.Info("Existing token is already expired, try to request a new one.");
                    GetToken(rInterface);
                }

                if(string.IsNullOrEmpty(token))
                {
                    result.Status = ChargeStatus.FAILED;
                    Logger.Error("Failed to request token, please contact gatway administrator. The charge order will be marked as fail, refound the currency to agent.");
                    ChangeOrderStatus(order, result);
                    return result;
                }
                StringBuilder jsonParam = new StringBuilder();
                corder.Out_Order_Id = rInterface.Username + DateTimeUtil.ConvertDateTimeToInt(DateTime.Now).ToString("0");
                order.OutOrderId = corder.Out_Order_Id;
                db.SaveChanges();
                jsonParam.Append("[");
                jsonParam.Append("{");
                jsonParam.Append("\"orderId\":");
                jsonParam.Append("\""+corder.Out_Order_Id.Trim()+"\",");
                jsonParam.Append("\"accNumber\":");
                jsonParam.Append("\"" + corder.Phone_number.Trim() + "\",");
                jsonParam.Append("\"pricePlanCd\":");
                jsonParam.Append("\"" + taocan.Serial.Trim() + "\",");
                jsonParam.Append("\"orderType\":");
                jsonParam.Append("\"0\"");
                jsonParam.Append("}");
                jsonParam.Append("]");
                parmeters.Add(new WebRequestParameters() {Name="para",Value=jsonParam.ToString() });
                parmeters.Add(new WebRequestParameters() { Name = "token", Value = token });
                parmeters.Add(new WebRequestParameters() { Name = "id", Value = rInterface.Username.Trim() });
                SendRequest(parmeters, false, out succeed, RequestType.POST);

                if (!string.IsNullOrEmpty(Response))
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject jsonResult = Newtonsoft.Json.Linq.JObject.Parse(Response);
                        string ret = jsonResult["result"].ToString();                       
                        string message = jsonResult["resultMsg"].ToString();
                        if (!string.IsNullOrEmpty(ret) && ret.Trim() == "0")
                        {
                            result.Status = ChargeStatus.ONPROGRESS;
                        }
                        else
                        {
                            result.Status = ChargeStatus.FAILED;
                        }
                        result.Message = message;
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex);
                        result.Status = ChargeStatus.FAILED;
                        result.Message = "系统错误，请联系管理员";
                    }

                    if (result.Message != null && result.Message.Length > 5000)
                    {
                        result.Message = result.Message.Substring(0, 5000);
                    }
                    ChangeOrderStatus(order, result);
                }

            }
            catch(Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            Logger.Info("Charging done!");
            return result;
        }

        public void GetChargeStatus(int resourceId, Resrouce_interface api)
        {
            Logger.Info("GetChargeStatus...");
            chargebitEntities db = null;
            try
            {               
                List<Charge_Order> orders = null;
                lock (getStatusObj)
                {
                    db = new chargebitEntities();
                    orders = (from o in db.Charge_Order where o.Status == 1 && o.Resource_id == resourceId && o.Payed && o.Id > lastMaxGetStatusOrderId orderby o.Id ascending select o).ToList<Charge_Order>();
                    if (orders.Count <= 0)
                    {
                        Logger.Info("No orders need to sync status of resourceId:" + resourceId);
                        return;
                    }
                    else
                    {
                        lastMaxGetStatusOrderId = (from o in orders select o.Id).Max();
                        Logger.Info(string.Format("{0} orders need to sync status", orders.Count));
                    }
                }

                KMBit.DAL.Resrouce_interface rInterface = api;
                if(rInterface==null)
                {
                    rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == resourceId select ri).FirstOrDefault<Resrouce_interface>();
                }
                if(string.IsNullOrEmpty(rInterface.QueryStatusUrl))
                {
                    return;
                }
                ServerUri = new Uri(rInterface.QueryStatusUrl);
                if (string.IsNullOrEmpty(token))
                {
                    GetToken(rInterface);
                }
                if (DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) > expiredTime)
                {
                    Logger.Info("Token expired, get it again.");
                    GetToken(rInterface);
                }

                if (string.IsNullOrEmpty(token))
                {
                    Logger.Error("Cannot get token.");
                    return;
                }

                List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
                StringBuilder orderIds = new StringBuilder();
                foreach(Charge_Order o in orders)
                {
                    if(!string.IsNullOrEmpty(o.Out_Order_Id))
                    {
                        orderIds.Append(o.Out_Order_Id + ",");
                    }                    
                }
                string orderParamStr = orderIds.ToString();
                orderParamStr = orderParamStr.Substring(0, orderParamStr.Length-1);
                parmeters.Add(new WebRequestParameters() {Name="para",Value="{\"orderId\":\""+orderParamStr+"\"}" });
                parmeters.Add(new WebRequestParameters() { Name="token",Value=token});
                parmeters.Add(new WebRequestParameters() { Name="id",Value= rInterface.Username.Trim()});
                bool succeed = false;
                SendRequest(parmeters, false, out succeed, RequestType.POST);
                ChargeResult result = new ChargeResult();
                if (!string.IsNullOrEmpty(Response))
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject jsonResult = Newtonsoft.Json.Linq.JObject.Parse(Response);
                        string ret = jsonResult["result"].ToString();
                        string message = jsonResult["resultMsg"].ToString();
                        Logger.Info(string.Format("Message:{0} returned by {1}"+message!=null?message:"", rInterface.QueryStatusUrl));
                        if (!string.IsNullOrEmpty(ret) && ret.Trim() == "0")
                        {
                            if(jsonResult["orderResult"]!=null)
                            {
                                Newtonsoft.Json.Linq.JArray backOrders = JArray.Parse(jsonResult["orderResult"].ToString());
                                if(backOrders!=null && backOrders.Count>0)
                                {
                                    foreach(JObject ro in backOrders)
                                    {
                                        string roId = ro["orderId"]!=null? ro["orderId"].ToString():null;
                                        string roPhone= ro["accNumber"] != null ? ro["accNumber"].ToString() : null;
                                        string roResult= ro["orderResult"] != null ? ro["orderResult"].ToString() : null;
                                        string roMessage= ro["orderMsg"] != null ? ro["orderMsg"].ToString() : null;
                                        if(string.IsNullOrEmpty(roId) || string.IsNullOrEmpty(roPhone))
                                        {
                                            continue;
                                        }

                                        Charge_Order dbOrder = (from o in orders where o.Out_Order_Id == roId.Trim() && o.Phone_number == roPhone.Trim() select o).FirstOrDefault<Charge_Order>();
                                        if(dbOrder!=null)
                                        {
                                            if(!string.IsNullOrEmpty(roResult) && roResult.Trim()=="0")
                                            {
                                                result.Status = ChargeStatus.SUCCEED;
                                                result.Message = roMessage != null ? roMessage : "充值成功";
                                            }
                                            else
                                            {
                                                result.Status = ChargeStatus.FAILED;
                                                result.Message = roMessage != null ? roMessage : "充值失败";
                                            }
                                        }
                                        ChargeOrder blOrder = new ChargeOrder() { Id= dbOrder.Id,OutOrderId=dbOrder.Out_Order_Id,AgencyId=dbOrder.Agent_Id };
                                        ChangeOrderStatus(blOrder,result,true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Info(string.Format("No order status returned back by {0}",api.QueryStatusUrl));
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex);
                    } 
                }

            }
            catch(Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            Logger.Info("GetChargeStatus done!");
        }

        public void ImportProducts(int resourceId, int operate_user)
        {
            throw new NotImplementedException();
        }
    }
}
