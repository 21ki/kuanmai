using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.BL
{
    public class ChargeBridge
    {
        public ChargeResult Charge(ChargeOrder order)
        {
            ChargeResult result = null;
            ICharge chargeMgr = null;
            chargebitEntities db = null;
            try
            {
                db = new chargebitEntities();
                Resource_taocan taocan = (from tc in db.Resource_taocan where tc.Id == order.ResourceTaocanId select tc).FirstOrDefault<Resource_taocan>();
                if(!taocan.Enabled)
                {
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = ChargeConstant.RESOURCE_TAOCAN_DISABLED };
                    return result;
                }

                KMBit.DAL.Resource resource = (from ri in db.Resource
                                               join tr in db.Resource_taocan on ri.Id equals tr.Resource_id
                                               where tr.Id == order.ResourceTaocanId
                                               select ri).FirstOrDefault<Resource>();

                if (resource == null)
                {
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = "落地资源部存在，请联系平台管理员" };
                    return result;
                }
                if(!resource.Enabled)
                {
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = ChargeConstant.RESOURCE_DISABLED };
                    return result;
                }                
                if(order.ResourceId==0)
                {
                    order.ResourceId = resource.Id;
                }
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                if (rInterface == null)
                {
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = ChargeConstant.RESOURCE_INTERFACE_NOT_CONFIGURED };
                    return result;
                }
                Assembly assembly=Assembly.LoadFrom(rInterface.Interface_assemblyname);
                chargeMgr = (ICharge)assembly.CreateInstance(rInterface.Interface_classname);
               
                result = chargeMgr.Charge(order);
            }
            catch(Exception ex)
            {

            }
            finally
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
