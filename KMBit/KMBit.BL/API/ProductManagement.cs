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
        public APIChargeResult Charge(int agendId,int routeId,string mobile,string province,string city,string callBackUrl,string client_order_id)
        {
            ChargeResult result = null;
            ChargeBridge cb = new ChargeBridge();
            ChargeOrder order = new ChargeOrder() { ClientOrderId= client_order_id, Payed = false, OperateUserId = 0, AgencyId = agendId, Id = 0, Province = province, City = city, MobileNumber = mobile, OutOrderId = "", ResourceId = 0, ResourceTaocanId = 0, RouteId = routeId, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now), CallbackUrl=callBackUrl };

            OrderManagement orderMgt = new OrderManagement();
            try
            {
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
                result = new ChargeResult();
                result.Status = ChargeStatus.FAILED;
                result.Message = kex.Message;
            }catch(Exception ex)
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
                            where r.Enabled == true && t.Enabled == true && r.User_id == agentId
                            orderby t.Sp_id descending, t.Quantity ascending
                            select new AgentProduct
                            {
                                Id = r.Id,
                                Size = t.Quantity,
                                SP = t.Sp_id,
                                Price= t.Sale_price
                            };

                products = query.ToList<AgentProduct>();
            }
            return products;
        }
    }
}
