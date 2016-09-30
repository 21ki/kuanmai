using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using KMBit.Beans;
using KMBit.Beans.API;
using KMBit.BL.Charge;
using KMBit.BL;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.BL.API
{
    public class ProductManagement
    {
        ILog logger = KMLogger.GetLogger();
        public APIChargeResult Charge(int agentId,int routeId,string mobile,string spname,string province,string city,string callBackUrl,string client_order_id)
        {
            if(string.IsNullOrEmpty(mobile))
            {
                throw new KMBitException("手机号码不能为空");
            }
            if (string.IsNullOrEmpty(spname))
            {
                throw new KMBitException("手机号归属运行商不能为空");
            }
            if (string.IsNullOrEmpty(province))
            {
                throw new KMBitException("手机号归属省份不能为空");
            }
            if (string.IsNullOrEmpty(city))
            {
                throw new KMBitException("手机号归属城市不能为空");
            }
            ChargeResult result = null;
            ChargeBridge cb = new ChargeBridge();
            ChargeOrder order = new ChargeOrder()
            {
                ClientOrderId = client_order_id,
                Payed = false,
                OperateUserId = 0,
                AgencyId = agentId,
                Id = 0,
                Province = province,
                City = city,
                MobileNumber = mobile,
                MobileSP=spname,
                OutOrderId = "",
                ResourceId = 0,
                ResourceTaocanId = 0,
                RouteId = routeId,
                CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                CallbackUrl = callBackUrl
            };
            chargebitEntities db = null;
            OrderManagement orderMgt = new OrderManagement();
            try
            {
                db = new chargebitEntities();
                Users agent = (from u in db.Users where u.Id == agentId select u).FirstOrDefault<Users>();
                if(agent==null)
                {
                    throw new KMBitException(string.Format("编号为{0}的代理商不存在",agentId));
                }
                if(!agent.Enabled)
                {
                    throw new KMBitException(string.Format("代理商{0}被禁用",agent.Name));
                }
                //verify mobile sp
                Agent_route route = (from r in db.Agent_route where r.Id == routeId && r.User_id == agentId select r).FirstOrDefault<Agent_route>();
                if(route==null)
                {
                    throw new KMBitException(string.Format("代理商 {1} 编号为{0}的路由不存在", routeId,agent.Name));
                }
                if(!route.Enabled)
                {
                    throw new KMBitException(string.Format("代理商 {1} 编号为{0}的路由被禁用", routeId, agent.Name));
                }
                Resource_taocan taocan = (from t in db.Resource_taocan where t.Id==route.Resource_taocan_id select t).FirstOrDefault<Resource_taocan>();
                int spId = (from sp in db.Sp where sp.Name.Contains(spname.Trim()) select sp.Id).FirstOrDefault<int>();
                if(spId==0)
                {
                    throw new KMBitException("手机运营商的值必须是-中国移动，中国联通或者中国电信");
                }
                int provinceId = (from area in db.Area where area.Name.Contains(province) select area.Id).FirstOrDefault<int>();
                if(provinceId==0)
                {
                    throw new KMBitException("手机号码归属省份值不正确，例如 河南，海南，江苏，请以此种格式传入");
                }
                if(taocan.NumberProvinceId>0 && provinceId>0)
                {
                    if(provinceId!=taocan.NumberProvinceId)
                    {
                        throw new KMBitException(string.Format("当前路由不能充{0}-{1}的手机号码",spname,province));
                    }
                }
                order = orderMgt.GenerateOrder(order);
                //result = cb.Charge(order);
                if(order.Id>0)
                {
                    result = new ChargeResult();
                    result.Status = ChargeStatus.SUCCEED;
                    result.Message = "充值信息已提交到充值系统";
                }
            }
            catch(KMBitException kex)
            {
                throw kex;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                result = new ChargeResult();
                result.Status = ChargeStatus.FAILED;
                result.Message = "未知错误，请联系平台管理员";
            }

            APIChargeResult apiResult = new APIChargeResult();
            //apiResult.Message = result.Message;
            apiResult.OrderId = order.Id;
            switch (result.Status)
            {
                case ChargeStatus.SUCCEED:
                    apiResult.Status = ChargeStatus.SUCCEED.ToString();
                    apiResult.Message = result.Message;
                    break;
                case ChargeStatus.FAILED:
                    apiResult.Status = ChargeStatus.FAILED.ToString();
                    break;
                case ChargeStatus.ONPROGRESS:
                    apiResult.Status = ChargeStatus.SUCCEED.ToString();
                    break;
                case ChargeStatus.PENDIND:
                    apiResult.Status = ChargeStatus.SUCCEED.ToString();
                    break;
            }

            return apiResult;
        }

        public List<AgentProduct> GetAgentProducts(int agentId)
        {
            List<AgentProduct> products = new List<AgentProduct>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from r in db.Agent_route
                            join t in db.Resource_taocan on r.Resource_taocan_id equals t.Id
                            join a in db.Area on t.NumberProvinceId equals a.Id
                            join sp in db.Sp on t.Sp_id equals sp.Id
                            where r.Enabled == true && t.Enabled == true && r.User_id == agentId
                            orderby t.Sp_id descending, t.Quantity ascending
                            select new AgentProduct
                            {
                                Id = r.Id,
                                Size = t.Quantity,
                                SP = t.Sp_id,  
                                SPName = sp!=null?sp.Name:"",   
                                RestrictProvince=a!=null?a.Name:"",
                                PlatformSalePrice=t.Sale_price,
                                ClientDiscount=r.Discount,
                                ClientSalePrice=r.Sale_price
                            };

                products = query.ToList<AgentProduct>();
            }
            return products;
        }
    }
}
