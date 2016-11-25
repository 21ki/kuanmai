using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.BL.Charge
{
    public class HeNanUnionComCharge : ChargeService, ICharge, IStatus
    {
        public HeNanUnionComCharge()
        {
            this.Logger = log4net.LogManager.GetLogger(this.GetType());
        }

        public void CallBack(List<WebRequestParameters> data)
        {
            throw new NotImplementedException();
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
                ServerUri = new Uri(rInterface.APIURL);

                StringBuilder body = new StringBuilder();
                body.Append("{");
                body.Append("usernumber:\"" + rInterface.Username+"\"");
                body.Append(",gavingnumber:\"" + order.MobileNumber+"\"");
                body.Append(",packagetype:\"1\"");
                body.Append(",packagecode:\"" + taocan.Serial+"\"");
                body.Append(",validtype:\"0\"");
                body.Append("}");
                string appx = order.Id.ToString();
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string method = "ai.cuc.ll.method.gav";
                string appkey =rInterface.AppKey ;// "ai.cuc.ll.appkey.test";
                string appSecret = rInterface.AppSecret;// "8888";
                parmeters.Add(new WebRequestParameters("apptx", appx, false));
                parmeters.Add(new WebRequestParameters("timestamp", timestamp, false));
                parmeters.Add(new WebRequestParameters("method", method, false));
                parmeters.Add(new WebRequestParameters("appkey", appkey, false));
                parmeters.Add(new WebRequestParameters("msg", body.ToString(), false));

                SortedDictionary<string, string> paras = new SortedDictionary<string, string>();
                foreach (WebRequestParameters p in parmeters)
                {
                    paras.Add(p.Name, p.Value);
                }

                string urlStr = string.Empty;
                foreach (KeyValuePair<string, string> p in paras)
                {
                    if(urlStr==string.Empty)
                    {
                        urlStr=p.Key+"="+p.Value;
                    }else
                    {
                        urlStr +="&"+ p.Key + "="+p.Value;
                    }
                }

                urlStr += "&" + UrlSignUtil.GetMD5(appSecret);
                string sign = UrlSignUtil.GetMD5(urlStr);
                parmeters.Add(new WebRequestParameters("sign", sign, false));
                parmeters.Add(new WebRequestParameters("signMethod", "MD5", false));
                SendRequest(parmeters, false, out succeed, RequestType.POST);
                
                //parse Response
                if(!string.IsNullOrEmpty(Response))
                {
                    result.Message = ChargeConstant.SUCCEED_CHARGE;
                    result.Status = ChargeStatus.SUCCEED;
                    try
                    {
                        Newtonsoft.Json.Linq.JObject jsonResult = Newtonsoft.Json.Linq.JObject.Parse(Response);
                        string code = jsonResult["code"].ToString();
                        string message = jsonResult["detail"].ToString();
                        switch (code)
                        {
                            case "0000":
                                result.Status = ChargeStatus.SUCCEED;
                                break;
                            case "9999":
                                result.Status = ChargeStatus.FAILED;
                                result.Message = message;
                                break;
                            default:
                                result.Message = "未知错误";
                                result.Status = ChargeStatus.FAILED;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Message = ex.Message;
                        result.Status = ChargeStatus.FAILED;
                    }
                }
                else
                {
                    result.Message = "未知错误";
                    result.Status = ChargeStatus.FAILED;
                }
                if(result.Message!=null && result.Message.Length>5000)
                {
                    result.Message = result.Message.Substring(0,5000);
                }
                ChangeOrderStatus(order, result);
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
            Logger.Info("Charging done！");
            return result;
        }

        public void GetChargeStatus(int resourceId,Resrouce_interface api)
        {
            throw new NotImplementedException();
        }

        public void ImportProducts(int resourceId, int operate_user)
        {
            throw new NotImplementedException();
        }
    }
}
