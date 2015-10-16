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
            if (order.ResourceTaocanId<=0 && order.RouteId<=0)
            {
                throw new KMBitException("充值前请先选择套餐");
            }
            chargebitEntities db = null;
            try           
            {
                db = new chargebitEntities();
                Resource_taocan taocan = null;
                if(order.ResourceTaocanId<=0 && order.RouteId>0)
                {
                    var query = from ta in db.Agent_route 
                                join tc in db.Resource_taocan on ta.Resource_taocan_id equals tc.Id
                                where ta.Id == order.RouteId
                                select tc;
                    taocan = query.FirstOrDefault<Resource_taocan>();                    
                }else if(order.ResourceTaocanId>0)
                {
                    taocan = (from tc in db.Resource_taocan where tc.Id == order.ResourceTaocanId select tc).FirstOrDefault<Resource_taocan>();
                }                
                if (!taocan.Enabled)
                {
                    throw new KMBitException(ChargeConstant.RESOURCE_TAOCAN_DISABLED);
                }
                if(taocan==null)
                {
                    throw new KMBitException("套餐信息不存在，不能充值");
                }
                order.ResourceTaocanId = taocan.Id;
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

                Charge_Order history = new Charge_Order();
                history.Agent_Id = order.AgencyId;               
                history.Completed_Time = 0;
                history.Created_time = order.CreatedTime > 0 ? order.CreatedTime : DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                history.Operate_User = order.OperateUserId;
                history.Out_Order_Id = "";
                history.Payed = order.Payed;
                history.Phone_number = order.MobileNumber;
                history.Resource_id = resource.Id;
                history.Resource_taocan_id = order.ResourceTaocanId;
                history.RuoteId = order.RouteId;
                history.Charge_type = order.AgencyId>0?1:0;
                db.Charge_Order.Add(history);
                db.SaveChanges();
                order.Id = history.Id;
            }catch(Exception ex)
            {
                throw new KMBitException(ex.Message);
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }

            return order;
        }

        public bool UpdatePaymentStatus(string mobile,int orderId, bool payed)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Charge_Order cOrder = (from o in db.Charge_Order where o.Id==orderId && o.Phone_number==mobile select o).FirstOrDefault<Charge_Order>();
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
