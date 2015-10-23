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
    public class ChongBaCharge :ChargeService, ICharge
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
                        int oid = 0;
                        int.TryParse(param.Value,out oid);
                        order.Id = oid;
                        break;
                }
            }

            if(!string.IsNullOrEmpty(res))
            {
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
                        result.Message = resMessage!=null?res:"未知错误";
                        result.Status = ChargeStatus.FAILED;
                        break;
                }
            }else
            {
                //回调没有传入状态，本系统默认失败
                result.Message = "没有回调状态数据";
                result.Status = ChargeStatus.FAILED;
            }

            if (order.Id > 0)
            {
                ChangeOrderStatus(order, result, true);
            }
            else
            {
                throw new KMBitException("回调数据中没有本系统的订单号，所以不能更新本系统数据，此次调用为脏数据");
            }            
        }

        public ChargeResult Charge(ChargeOrder order)
        {
            ChargeResult result = new ChargeResult();
            ProceedOrder(order, out result);
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
                Charge_Order corder = (from co in db.Charge_Order where co.Id==order.Id select co).FirstOrDefault<Charge_Order>();
                corder.Process_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id==order.ResourceTaocanId select t).FirstOrDefault<Resource_taocan>();
                if(string.IsNullOrEmpty(taocan.Serial))
                {
                    result.Message = ChargeConstant.RESOURCE_TAOCAN_NO_PDTID;
                    result.Status = ChargeStatus.ONPROGRESS;
                    return result;
                }
                ServerUri = new Uri(rInterface.APIURL);
                parmeters.Add(new WebRequestParameters("appKey", rInterface.Username, false));
                parmeters.Add(new WebRequestParameters("appSecret", rInterface.Userpassword, false));
                parmeters.Add(new WebRequestParameters("phoneNo", order.MobileNumber, false));
                parmeters.Add(new WebRequestParameters("prodCode", taocan.Serial, false));
                parmeters.Add(new WebRequestParameters("backUrl", rInterface.CallBackUrl, false));
                parmeters.Add(new WebRequestParameters("transNo",order.Id.ToString(), false));
                SendRequest(parmeters, false, out succeed);
                if (!string.IsNullOrEmpty(Response))
                {
                    JObject jsonResult = JObject.Parse(Response);
                    order.OutId = jsonResult["orderId"]!=null? jsonResult["orderId"].ToString():"";
                    string res = jsonResult["respCode"]!=null?jsonResult["respCode"].ToString():"";
                    string resMsg = jsonResult["respMsg"]!=null?jsonResult["respMsg"].ToString():"";
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
                            result.Message = resMsg;
                            result.Status = ChargeStatus.FAILED;
                            break;
                    }

                    ChangeOrderStatus(order, result);
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

        public void ImportProducts(int resourceId,int operate_user)
        {
            chargebitEntities db = null;
            try
            {
                bool succeed = false;
                List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
                db = new chargebitEntities();
                db.Configuration.AutoDetectChangesEnabled = false;
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == resourceId select ri).FirstOrDefault<Resrouce_interface>();
                ServerUri = new Uri(rInterface.ProductApiUrl);
                parmeters.Add(new WebRequestParameters("appKey", rInterface.Username, false));
                parmeters.Add(new WebRequestParameters("appSecret", rInterface.Userpassword, false));
                SendRequest(parmeters, false, out succeed);
                if(succeed)
                {
                   if(!string.IsNullOrEmpty(Response))
                    {
                        JArray products = JArray.Parse(Response);
                        if(products!=null && products.Count>0)
                        {
                            for(int i=0;i<products.Count;i++)
                            {
                                Resource_taocan taocan = null;
                                string serial = products[i]["ccode"].ToString();
                                string name = products[i]["cname"].ToString();
                                int quantity = 0;
                                int spId = int.Parse(products[i]["iprotypeid"].ToString());
                                char[] names = name.ToCharArray();
                                StringBuilder qStr = new StringBuilder();
                                Regex regex = new Regex("[^0-9]");
                                for (int j=0;j<names.Length;j++)
                                {
                                    char tmp = names[j];
                                    if (!regex.IsMatch(tmp.ToString()))
                                    {
                                        qStr.Append(tmp.ToString());
                                    }
                                    
                                    if(tmp.ToString().ToLower()=="g")
                                    {
                                        quantity = int.Parse(qStr.ToString()) * 1024;
                                        break;
                                    }                                    
                                }
                                if (quantity==0 && qStr.ToString() != "")
                                {
                                    quantity = int.Parse(qStr.ToString());
                                }

                                taocan = (from t in db.Resource_taocan where t.Resource_id==resourceId && t.Serial==serial select t).FirstOrDefault<Resource_taocan>();

                                if (taocan==null)
                                {
                                    //create new product taocan
                                    taocan = new Resource_taocan()
                                    {
                                        Area_id = 0,
                                        CreatedBy = operate_user,
                                        Serial = serial,
                                        Purchase_price = float.Parse(products[i]["iprice"].ToString()),
                                        Quantity = quantity,
                                        Resource_id = resourceId,
                                        Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                                        Sale_price = 0,
                                        Enabled = false
                                    };
                                    if (spId == 2)
                                    {
                                        taocan.Sp_id = 1;
                                    }
                                    else if (spId == 3)
                                    {
                                        taocan.Sp_id = 3;
                                    }
                                    else if (spId == 4)
                                    {
                                        taocan.Sp_id = 2;
                                    }

                                    Taocan ntaocan = (from t in db.Taocan where t.Sp_id == taocan.Sp_id && t.Quantity == taocan.Quantity select t).FirstOrDefault<Taocan>();
                                    Sp sp = (from s in db.Sp where s.Id == taocan.Sp_id select s).FirstOrDefault<Sp>();
                                    if (ntaocan == null)
                                    {
                                        string taocanName = sp != null ? sp.Name + " " + taocan.Quantity.ToString() + "M" : "全网 " + taocan.Quantity.ToString() + "M";
                                        ntaocan = new Taocan() { Created_time = taocan.Created_time, Description = taocanName, Name = taocanName, Sp_id = taocan.Sp_id, Quantity = taocan.Quantity, Updated_time = 0 };
                                        db.Taocan.Add(ntaocan);
                                        db.SaveChanges();
                                    }
                                    if (ntaocan.Id > 0)
                                    {
                                        taocan.Taocan_id = ntaocan.Id;
                                        db.Resource_taocan.Add(taocan);
                                    }
                                    
                                }else
                                {
                                    //update product taocan
                                    taocan.Purchase_price = float.Parse(products[i]["iprice"].ToString());
                                    taocan.UpdatedBy = operate_user;
                                    taocan.Updated_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                                }  
                            }

                            db.SaveChanges();
                        }
                    }
                }
            }
            catch(KMBitException ex)
            { }
            catch(Exception ex)
            { }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
