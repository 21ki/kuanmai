using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class ChongBaCharge : ChargeService, ICharge
    {
        public ChongBaCharge()
        {
        }

        public void CallBack(List<WebRequestParameters> data)
        {
            if(data==null)
            {
                return;
            }
            string res = "";
            string resMessage = "";
            ChargeResult result = new ChargeResult();
            ChargeOrder order = new ChargeOrder();
            foreach(WebRequestParameters param in data)
            {
                switch(param.Name)
                {
                    case "orderId":
                        order.OutId = param.Value;
                        break;
                    case "respCode":
                        res = param.Value;
                        break;
                    case "respMsg":
                        resMessage = param.Value;
                        break;
                    case "transNo":
                        order.Id = int.Parse(param.Value);
                        break;
                }
            }

            switch (res.ToLower())
            {
                case "jx0000":
                    result.Message = ChargeConstant.CHARGING;
                    result.Status = ChargeStatus.ONPROGRESS;
                    break;
                case "0000":
                case "00000":
                case "000000":
                    result.Message = ChargeConstant.SUCCEED_CHARGE;
                    result.Status = ChargeStatus.SUCCEED;
                    //change charge status
                    break;
                case "jx0001":
                    result.Message = ChargeConstant.AGENT_WRONG_PASSWORD;
                    result.Status = ChargeStatus.FAILED;
                    break;
                case "jx0002":
                    result.Message = ChargeConstant.AGENT_NOT_BIND_IP;
                    result.Status = ChargeStatus.FAILED;
                    break;
                case "jx0003":
                    result.Message = ChargeConstant.AGENT_IP_NOT_MATCH;
                    result.Status = ChargeStatus.FAILED;
                    break;
                case "jx0004":
                    result.Message = ChargeConstant.RESOURCE_PRODUCT_NOT_EXIST;
                    result.Status = ChargeStatus.FAILED;
                    break;
                case "jx0005":
                    result.Message = ChargeConstant.RESOURCE_NOT_ENOUGH_MONEY;
                    result.Status = ChargeStatus.FAILED;
                    break;
                default:
                    break;
            }

            ChangeOrderStatus(order, result,true);
        }

        public ChargeResult Charge(ChargeOrder order)
        {
            ChargeResult result = new ChargeResult();   
            VerifyCharge(order, out result);
            if(result.Status== ChargeStatus.FAILED)
            {
                return result;
            }
            List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
            bool succeed = false;
            result = new ChargeResult();
            chargebitEntities db=null;
            try
            {
                db = new chargebitEntities();
                Charge_history corder = (from co in db.Charge_history where co.Id==order.Id select co).FirstOrDefault<Charge_history>();
                corder.Process_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id==order.ResourceTaocanId select t).FirstOrDefault<Resource_taocan>();
                this.ServerUri = new Uri(rInterface.APIURL);
                parmeters.Add(new WebRequestParameters("appKey", rInterface.Username, false));
                parmeters.Add(new WebRequestParameters("appSecret", rInterface.Userpassword, false));
                parmeters.Add(new WebRequestParameters("phoneNo", order.MobileNumber, false));
                parmeters.Add(new WebRequestParameters("prodCode", taocan.Serial, false));
                parmeters.Add(new WebRequestParameters("backUrl", rInterface.CallBackUrl, false));
                parmeters.Add(new WebRequestParameters("transNo",order.Id.ToString(), false));
                SendRequest(parmeters, false, out succeed);
                if (!string.IsNullOrEmpty(this.Response))
                {
                    JObject jsonResult = JObject.Parse(this.Response);
                    order.OutId = jsonResult["orderId"]!=null? jsonResult["orderId"].ToString():"";
                    string res = jsonResult["respCode"].ToString();

                    switch(res.ToLower())
                    {
                        case "jx0000":
                            result.Message = ChargeConstant.CHARGING;
                            result.Status = ChargeStatus.ONPROGRESS;
                            break;
                        case "0000":
                        case "00000":
                        case "000000":
                            result.Message = ChargeConstant.SUCCEED_CHARGE;
                            result.Status = ChargeStatus.SUCCEED;
                            //change charge status
                            ChangeOrderStatus(order, result);
                            break;
                        case "jx0001":
                            result.Message = ChargeConstant.AGENT_WRONG_PASSWORD;
                            result.Status = ChargeStatus.FAILED;
                            break;
                        case "jx0002":
                            result.Message = ChargeConstant.AGENT_NOT_BIND_IP;
                            result.Status = ChargeStatus.FAILED;
                            break;
                        case "jx0003":
                            result.Message = ChargeConstant.AGENT_IP_NOT_MATCH;
                            result.Status = ChargeStatus.FAILED;
                            break;
                        case "jx0004":
                            result.Message = ChargeConstant.RESOURCE_PRODUCT_NOT_EXIST;
                            result.Status = ChargeStatus.FAILED;
                            break;
                        case "jx0005":
                            result.Message = ChargeConstant.RESOURCE_NOT_ENOUGH_MONEY;
                            result.Status = ChargeStatus.FAILED;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch(Exception ex)
            {

            }finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
            return result;
        }
    }
}
