using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Util;
using KMBit.Beans;
using KMBit.BL.Charge;
namespace KMBit.BL
{
    public class OrderManagement
    {
        public ChargeOrder GenerateOrder(ChargeOrder order)
        {            
            if (order.ResourceTaocanId<=0)
            {
                throw new KMBitException("充值前请先选择套餐");
            }

            using (chargebitEntities db = new chargebitEntities())
            {
                Resource_taocan taocan = (from tc in db.Resource_taocan where tc.Id == order.ResourceTaocanId select tc).FirstOrDefault<Resource_taocan>();
                if (!taocan.Enabled)
                {
                    throw new KMBitException(ChargeConstant.RESOURCE_TAOCAN_DISABLED);
                }

                KMBit.DAL.Resource resource = (from ri in db.Resource
                                               join tr in db.Resource_taocan on ri.Id equals tr.Resource_id
                                               where tr.Id == order.ResourceTaocanId
                                               select ri).FirstOrDefault<Resource>();

                if (resource == null)
                {
                    throw new KMBitException("落地资源不存在");
                }
                if (!resource.Enabled)
                {
                    throw new KMBitException(ChargeConstant.RESOURCE_DISABLED);
                }

                if(order.AgencyId>0 && order.RouteId>0)
                {
                    Agent_route route = (from r in db.Agent_route where r.Id== order.RouteId select r).FirstOrDefault<Agent_route>();
                    if(route==null)
                    {
                        throw new KMBitException("代理商路由不存在");
                    }

                    if(route.User_id!=order.AgencyId)
                    {
                        throw new KMBitException("当前代理商没有此路由");
                    }
                }

                Charge_history history = new Charge_history();
                history.Agent_Id = order.AgencyId;
                if (history.Agent_Id <= 0)
                {
                    history.Charge_type = 0;
                }
                else
                {
                    history.Charge_type = 1;
                }
                history.Completed_Time = 0;
                history.Created_time = order.CreatedTime > 0 ? order.CreatedTime : DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                history.Operate_User = order.OperateUserId;
                history.Out_Order_Id = "";
                history.Payed = order.Payed;
                history.Phone_number = order.MobileNumber;
                history.Resource_id = resource.Id;
                history.Resource_taocan_id = order.ResourceTaocanId;
                history.RuoteId = order.RouteId;
                db.Charge_history.Add(history);
                db.SaveChanges();
                order.Id = history.Id;
            }

            return order;
        }

        public bool UpdatePaymentStatus(string mobile,int orderId, bool payed)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Charge_history cOrder = (from o in db.Charge_history where o.Id==orderId && o.Phone_number==mobile select o).FirstOrDefault<Charge_history>();
                if(cOrder!=null)
                {
                    cOrder.Payed = payed;
                }else
                {
                    throw new KMBitException(string.Format("编号为{0}的充值记录不存在",orderId));
                }

                db.SaveChanges();
                result = true;
            }

            return result;
        }
    }
}
