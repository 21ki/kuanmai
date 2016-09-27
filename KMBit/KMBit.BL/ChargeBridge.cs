using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.DAL;
using KMBit.Util;
using log4net;
namespace KMBit.BL
{
    public class ChargeBridge
    {
        public ILog Logger { get; }
        public ChargeBridge()
        {
            Logger = log4net.LogManager.GetLogger(this.GetType());
        }

        public ChargeResult Charge(ChargeOrder order)
        {
            ChargeResult result = null;
            ICharge chargeMgr = null;
            chargebitEntities db = null;
            Charge_Order cOrder = null;
            try
            {
                db = new chargebitEntities();                
                cOrder = (from co in db.Charge_Order where co.Id == order.Id select co).FirstOrDefault<Charge_Order>();
                if(cOrder==null)
                {
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = ChargeConstant.ORDER_NOT_EXIST };
                    return result;
                }
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
                    db.Charge_Order.Remove(cOrder);
                    db.SaveChanges();
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = "落地资源部存在，请联系平台管理员" };
                    return result;
                }
                if(!resource.Enabled)
                {
                    db.Charge_Order.Remove(cOrder);
                    db.SaveChanges();
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
                    db.Charge_Order.Remove(cOrder);
                    db.SaveChanges();
                    result = new ChargeResult() { Status = ChargeStatus.FAILED, Message = ChargeConstant.RESOURCE_INTERFACE_NOT_CONFIGURED };
                    return result;
                }
                object o = null;
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Type type = assembly.GetType(rInterface.Interface_classname);
                o = Activator.CreateInstance(type);
                chargeMgr = (ICharge)o;
                result = chargeMgr.Charge(order);
            }
            catch(Exception ex)
            {
                if(cOrder!=null)
                {
                    db.Charge_Order.Remove(cOrder);
                    db.SaveChanges();
                }
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

        public ChargeResult ImportResourceProducts(int resourceId,int operate_user)
        {
            ChargeResult result = new ChargeResult() { Status= ChargeStatus.SUCCEED };
            ICharge chargeMgr = null;
            chargebitEntities db = new chargebitEntities();
            try
            {
                KMBit.DAL.Resource resource = (from ri in db.Resource
                                               where ri.Id==resourceId
                                               select ri).FirstOrDefault<Resource>();

                if (resource == null)
                {
                    throw new KMBitException(string.Format("编号为{0}的资源不存在",resourceId));
                }
                if (!resource.Enabled)
                {
                    throw new KMBitException(string.Format("编号为{0}的资源没有启用，请先启用在进行资源产品导入", resourceId));
                }

                KMBit.DAL.Resrouce_interface rInterface = (from ri in db.Resrouce_interface where ri.Resource_id == resourceId select ri).FirstOrDefault<Resrouce_interface>();
                if (rInterface == null)
                {
                    throw new KMBitException(string.Format("编号为{0}的资源没有配置资源接口，请先配置资源接口，并配置产品导入接口URL,然后进行资源产品导入", resourceId));
                }
                object o = null;                   
                Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Type type = assembly.GetType(rInterface.Interface_classname);
                o = Activator.CreateInstance(type);
                if (o!=null)
                {
                    chargeMgr = (ICharge)o;
                    chargeMgr.ImportProducts(resourceId, operate_user);
                }  
            }
            catch(Exception ex)
            {
                result.Status = ChargeStatus.FAILED;
                result.Message = ex.Message;

            }finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }

            return result;
        }

        public ChargeResult ChargeCallBack(SortedDictionary<string, string> parameters)
        {
            ChargeResult result = new ChargeResult();
            if(parameters==null)
            {
                result.Status = ChargeStatus.FAILED;
                result.Message = "回调参数错误";
                return result;
            }

            string orderStrId = null;
            if(parameters.ContainsKey("transNo"))
            {
                orderStrId = parameters["transNo"];
                ICharge chargeMgr = new ChongBaCharge();
                List<WebRequestParameters> paramters = new List<WebRequestParameters>();
                paramters.Add(new WebRequestParameters("orderId", parameters["orderId"], false));
                paramters.Add(new WebRequestParameters("respCode", parameters["respCode"], false));
                paramters.Add(new WebRequestParameters("respMsg", parameters["respMsg"], false));
                paramters.Add(new WebRequestParameters("transNo", parameters["transNo"], false));
                chargeMgr.CallBack(paramters);
                result.Status = ChargeStatus.SUCCEED;
                result.Message = "回调成功";
            }
            return result;
        }        

        public void SyncChargeStatus()
        {
            chargebitEntities db = new chargebitEntities();
            try
            {
                IStatus chargeMgr = null;
                List<Resrouce_interface> apis = (from api in db.Resrouce_interface where string.IsNullOrEmpty(api.CallBackUrl) orderby api.CallBackUrl select api).ToList<Resrouce_interface>();
                foreach(Resrouce_interface api in apis)
                {
                    object o = null;
                    Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    Type type = assembly.GetType(api.Interface_classname);
                    o = Activator.CreateInstance(type);                    
                    chargeMgr = o as IStatus;
                    if(chargeMgr!=null)
                    {
                        chargeMgr.GetChargeStatus(api.Resource_id);
                    }                   
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
        }
    }
}
