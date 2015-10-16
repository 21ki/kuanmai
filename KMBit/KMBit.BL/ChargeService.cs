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

        public virtual void VerifyCharge(ChargeOrder order,out ChargeResult result,bool isSOAPAPI=false)
        {
            result = new ChargeResult();

            if(order.AgencyId==0)
            {
                result.Status = ChargeStatus.SUCCEED;
                return;
            }

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

                Users agency = (from u in db.Users where u.Id == order.AgencyId select u).FirstOrDefault<Users>();
                var query = from r in db.Agent_route
                            join u in db.Users on r.CreatedBy equals u.Id
                            join uu in db.Users on r.UpdatedBy equals uu.Id into luu
                            from lluu in luu.DefaultIfEmpty()
                            join re in db.Resource on r.Resource_Id equals re.Id
                            join tc in db.Resource_taocan on r.Resource_taocan_id equals tc.Id
                            join city in db.Area on tc.Area_id equals city.Id into lcity
                            from llcity in lcity.DefaultIfEmpty()
                            join sp in db.Sp on tc.Sp_id equals sp.Id into lsp
                            from llsp in lsp.DefaultIfEmpty()
                            where r.Id == order.RouteId
                            select new BAgentRoute
                            {
                                Route = r,
                                CreatedBy = u,
                                UpdatedBy = lluu,
                                Taocan = new BResourceTaocan
                                {
                                    Taocan = tc,
                                    Resource = new BResource { Resource = re },
                                    Province = llcity,
                                    SP = llsp
                                }
                            };
                BAgentRoute ruote = query.FirstOrDefault<BAgentRoute>();
                if (ruote != null)
                {
                    agency.Remaining_amount = agency.Remaining_amount - (ruote.Taocan.Taocan.Sale_price * ruote.Route.Discount);
                    float price = ruote.Taocan.Taocan.Sale_price * ruote.Route.Discount;
                    if (agency.Pay_type == 1)
                    {
                        if (agency.Remaining_amount < price)
                        {
                            result.Message = ChargeConstant.AGENT_NOT_ENOUGH_MONEY;
                            result.Status = ChargeStatus.FAILED;
                            return;
                        }
                    }
                    else if(agency.Pay_type==2)
                    {
                        if (agency.Remaining_amount < price)
                        {
                            if(agency.Remaining_amount+agency.Credit_amount<price)
                            {
                                result.Message = ChargeConstant.AGENT_NOT_ENOUGH_CREDIT;
                                result.Status = ChargeStatus.FAILED;
                                return;
                            }                           
                        }
                    }                    
                }
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
                            cOrder.Status = 1;
                            cOrder.Message = result.Message;
                            break;
                        case ChargeStatus.SUCCEED:
                            cOrder.Status = 2;
                            cOrder.Message = result.Message;
                            cOrder.Out_Order_Id = order.OutId;
                            //扣代理商的费用
                            if(cOrder.Agent_Id>0 && cOrder.RuoteId>0)
                            {
                                cOrder.Charge_type = 1;
                                Users agency = (from u in db.Users where u.Id==cOrder.Agent_Id select u).FirstOrDefault<Users>();
                                var query = from r in db.Agent_route
                                            join u in db.Users on r.CreatedBy equals u.Id
                                            join uu in db.Users on r.UpdatedBy equals uu.Id into luu
                                            from lluu in luu.DefaultIfEmpty()
                                            join re in db.Resource on r.Resource_Id equals re.Id
                                            join tc in db.Resource_taocan on r.Resource_taocan_id equals tc.Id
                                            join city in db.Area on tc.Area_id equals city.Id into lcity
                                            from llcity in lcity.DefaultIfEmpty()
                                            join sp in db.Sp on tc.Sp_id equals sp.Id into lsp
                                            from llsp in lsp.DefaultIfEmpty()
                                            where r.Id==cOrder.RuoteId
                                            select new BAgentRoute
                                            {
                                                Route = r,
                                                CreatedBy = u,
                                                UpdatedBy = lluu,
                                                Taocan = new BResourceTaocan
                                                {
                                                    Taocan = tc,
                                                    Resource = new BResource { Resource = re },
                                                    Province = llcity,
                                                    SP = llsp
                                                }
                                            };
                                BAgentRoute ruote = query.FirstOrDefault<BAgentRoute>();
                                float money = ruote.Taocan.Taocan.Sale_price * ruote.Route.Discount;
                                if (ruote!=null)
                                {
                                    if(agency.Pay_type==1)
                                    {
                                        agency.Remaining_amount = agency.Remaining_amount - money;
                                    }else if(agency.Pay_type==2)
                                    {
                                        agency.Remaining_amount = agency.Remaining_amount - money;
                                        if(agency.Remaining_amount<money)
                                        {
                                            agency.Credit_amount = agency.Credit_amount - (money - agency.Remaining_amount);
                                        }                                        
                                    }
                                    
                                    cOrder.Purchase_price = ruote.Taocan.Taocan.Sale_price * ruote.Route.Discount;
                                    cOrder.Sale_price = ruote.Route.Sale_price;
                                }
                            }else
                            {
                                //direct charge
                                var tmp = from rta in db.Resource_taocan
                                          join r in db.Resource on rta.Resource_id equals r.Id
                                          join cu in db.Users on rta.CreatedBy equals cu.Id into lcu
                                          from llcu in lcu.DefaultIfEmpty()
                                          join uu in db.Users on rta.UpdatedBy equals uu.Id into luu
                                          from lluu in luu.DefaultIfEmpty()
                                          join city in db.Area on rta.Area_id equals city.Id into lcity
                                          from llcity in lcity.DefaultIfEmpty()
                                          join sp in db.Sp on rta.Sp_id equals sp.Id into lsp
                                          from llsp in lsp.DefaultIfEmpty()
                                          join tt in db.Taocan on rta.Taocan_id equals tt.Id
                                          where rta.Id==cOrder.Resource_taocan_id
                                          select new BResourceTaocan
                                          {
                                              Taocan = rta,
                                              Taocan2 = tt,
                                              CreatedBy = llcu,
                                              UpdatedBy = lluu,
                                              Province = llcity,
                                              SP = llsp,
                                              Resource = new BResource() { Resource = r }
                                          };
                                BResourceTaocan taocan = tmp.FirstOrDefault<BResourceTaocan>();
                                cOrder.Charge_type = 0;
                                cOrder.Sale_price = 0;
                                cOrder.Purchase_price = taocan.Taocan.Sale_price;
                            }
                            break;
                        case ChargeStatus.FAILED:
                            cOrder.Status = 3;
                            cOrder.Message = result.Message;
                            break;
                    }
                    cOrder.Completed_Time = KMBit.Util.DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    db.SaveChanges();
                }
            }
        }
        
    }
}
