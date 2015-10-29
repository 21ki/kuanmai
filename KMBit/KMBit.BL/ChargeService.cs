using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.BL.Charge;
namespace KMBit.BL
{
    public class WebRequestParameters
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool URLEncodeParameter { get; set; }

        public WebRequestParameters()
        {
        }

        public WebRequestParameters(string name, string value, bool encode)
        {
            this.Name = name;
            this.Value = value;
            this.URLEncodeParameter = encode;
        }
    }

    public class ChargeService:HttpService
    {
        public ChargeService(string svrUrl):base(svrUrl)
        {

        }
        public ChargeService()
        {
        }      

        public virtual void ProceedOrder(ChargeOrder order,out ChargeResult result,bool isSOAPAPI=false)
        {
            result = new ChargeResult();
            using (chargebitEntities db = new chargebitEntities())
            {
                if (!isSOAPAPI)
                {
                    KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                    if (rInterface == null)
                    {
                        result.Status = ChargeStatus.FAILED;
                        result.Message = ChargeConstant.RESOURCE_INTERFACE_NOT_CONFIGURED;
                        return;
                    }

                    if (string.IsNullOrEmpty(rInterface.APIURL))
                    {
                        result.Status = ChargeStatus.FAILED;
                        result.Message = ChargeConstant.RESOURCE_INTERFACE_APIURL_EMPTY;
                        return;
                    }
                }
                Charge_Order cOrder = (from o in db.Charge_Order where o.Id==order.Id select o).FirstOrDefault<Charge_Order>();
                if(cOrder==null)
                {
                    result.Status = ChargeStatus.FAILED;
                    result.Message = ChargeConstant.ORDER_NOT_EXIST;
                    return;
                }
                cOrder.Process_time = KMBit.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id==order.ResourceTaocanId select t).FirstOrDefault<Resource_taocan>();
                if (order.AgencyId == 0)
                {
                    cOrder.Payed = order.Payed;
                    //cOrder.Sale_price = taocan.Sale_price;
                    //cOrder.Purchase_price = taocan.Sale_price;
                    //cOrder.Platform_Cost_Price = taocan.Purchase_price;
                    //cOrder.Platform_Sale_Price = taocan.Sale_price;                    
                    //cOrder.Revenue = taocan.Sale_price- taocan.Purchase_price;
                    cOrder.Status = 10;//等待充值
                    result.Status = ChargeStatus.SUCCEED;
                    db.SaveChanges();
                    return;
                }                

                Users agency = (from u in db.Users where u.Id == order.AgencyId select u).FirstOrDefault<Users>();
                Agent_route ruote = (from au in db.Agent_route where au.Id == order.RouteId select au).FirstOrDefault<Agent_route>();
                if (ruote != null)
                {   
                    //no need to verify agent remaining amount or credit amount if the charge is marketing order 
                    if(cOrder.MarketOrderId<=0)
                    {
                        float price = taocan.Sale_price * ruote.Discount;
                        if (agency.Pay_type == 1)
                        {
                            if (agency.Remaining_amount < price)
                            {
                                result.Message = ChargeConstant.AGENT_NOT_ENOUGH_MONEY;
                                result.Status = ChargeStatus.FAILED;
                                db.Charge_Order.Remove(cOrder);
                                db.SaveChanges();
                                return;
                            }
                            else
                            {
                                agency.Remaining_amount = agency.Remaining_amount - price;
                                result.Status = ChargeStatus.SUCCEED;
                            }
                        }
                        else if (agency.Pay_type == 2)
                        {
                            if (agency.Remaining_amount < price)
                            {
                                if (agency.Remaining_amount + agency.Credit_amount < price)
                                {
                                    result.Message = ChargeConstant.AGENT_NOT_ENOUGH_CREDIT;
                                    result.Status = ChargeStatus.FAILED;
                                    db.Charge_Order.Remove(cOrder);
                                    db.SaveChanges();
                                    return;
                                }
                                else
                                {
                                    agency.Remaining_amount = agency.Remaining_amount - price;
                                    agency.Credit_amount = agency.Credit_amount - (price - agency.Remaining_amount);
                                    result.Status = ChargeStatus.SUCCEED;
                                }
                            }
                            else
                            {
                                agency.Remaining_amount = agency.Remaining_amount - price;
                                result.Status = ChargeStatus.SUCCEED;
                            }
                        }
                    }             
                   
                    cOrder.Status = 10;
                    cOrder.Payed = true;
                    //cOrder.Sale_price = ruote.Sale_price;
                    //cOrder.Purchase_price = price;
                    //cOrder.Platform_Cost_Price = taocan.Purchase_price;
                    //cOrder.Platform_Sale_Price = taocan.Sale_price;
                    //cOrder.Revenue = price - taocan.Purchase_price;
                }
                if(result.Status== ChargeStatus.FAILED)
                {
                    db.Charge_Order.Remove(cOrder);
                }else
                {
                    if(cOrder.MarketOrderId>0)
                    {
                        Marketing_Orders mOrder = (from mo in db.Marketing_Orders where mo.Id==cOrder.MarketOrderId select mo).FirstOrDefault<Marketing_Orders>();
                        if(mOrder!=null)
                        {
                            mOrder.Used = true;
                            mOrder.Sent = true;
                            mOrder.UsedTime = KMBit.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            mOrder.PhoneNumber = cOrder.Phone_number;
                            mOrder.MacAddress = cOrder.DeviceMacAddress;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public virtual void ChangeOrderStatus(ChargeOrder order,ChargeResult result,bool isCallBack=false)
        {
            using (chargebitEntities db = new chargebitEntities())
            {
                Charge_Order cOrder = (from o in db.Charge_Order where o.Id== order.Id select o).FirstOrDefault<Charge_Order>();
                if(cOrder!=null)
                {
                    switch(result.Status)
                    {
                        case ChargeStatus.ONPROGRESS:
                            cOrder.Out_Order_Id = order.OutId;
                            cOrder.Status = 1;
                            cOrder.Message = result.Message;
                            break;
                        case ChargeStatus.SUCCEED:
                            cOrder.Out_Order_Id = order.OutId;
                            cOrder.Status = 2;
                            cOrder.Message = result.Message;
                            cOrder.Out_Order_Id = order.OutId;
                            cOrder.Completed_Time = KMBit.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            break;
                        case ChargeStatus.FAILED:
                            cOrder.Out_Order_Id = order.OutId;
                            cOrder.Status = 3;                           
                            cOrder.Completed_Time = KMBit.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            //Refound the money
                            if(cOrder.Agent_Id>0)
                            {
                                Users agency = (from u in db.Users where u.Id == cOrder.Agent_Id select u).FirstOrDefault<Users>();
                                if (agency != null)
                                {
                                    cOrder.Message = result.Message + ",充值订单金额已经退回代理商账户";
                                    agency.Remaining_amount += cOrder.Purchase_price;
                                    cOrder.Refound = true;
                                }
                            }else
                            {
                                if(cOrder.Operate_User>0)
                                {
                                    cOrder.Message = result.Message + ",管理员后台充值，无需退款";
                                }else
                                {
                                    cOrder.Message = result.Message + ",用户前台直充失败，需要手动退款给用户";
                                }
                            }
                            
                            break;                        
                    }
                    
                    db.SaveChanges();
                }
            }
        }        
    }
}
