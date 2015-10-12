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
                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == order.ResourceId select ri).FirstOrDefault<Resrouce_interface>();
                if(rInterface!=null)
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
