using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;

namespace KM.JXC.BL.Open
{
    public class PropertyManager:BBaseManager
    {
        public PropertyManager(BUser user, int shop_id, Permission permission)
            : base(user,shop_id,permission)
        {

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Product_Spec CreateNewProperty(int categoryId, string propertyName, List<string> value)
        {
            Product_Spec ps=null;
            if (string.IsNullOrEmpty(propertyName)) 
            {
                throw new KMJXCException("属性名称不能为空");
            }

            
            KuanMaiEntities db = new KuanMaiEntities();
            List<Product_Spec> properties = new List<Product_Spec>();
            try
            {
                var propes=from props in db.Product_Spec where props.Name==propertyName && props.Shop_ID==this.Shop.Shop_ID select props;
                if (categoryId > 0)
                {
                    propes.Where(c=>c.Product_Class_ID==categoryId);
                }

                properties = propes.ToList<Product_Spec>();
                if (properties.Count > 0)
                {
                    throw new KMJXCException("名为"+propertyName+"已经存在");
                }

                ps = new Product_Spec();
                ps.Product_Class_ID = categoryId;
                ps.Shop_ID = this.Shop.Shop_ID;
                ps.User_ID = this.CurrentUser.ID;
                ps.Name = propertyName;
                ps.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                ps.Mall_PID = "";

                db.Product_Spec.Add(ps);
                db.SaveChanges();

                int psId = ps.Product_Spec_ID;
                if (psId == 0)
                {
                    throw new KMJXCException("产品属性创建失败");
                }

                if (value != null)
                {
                    foreach (string v in value)
                    {
                        Product_Spec_Value psv = new Product_Spec_Value();
                        psv.Product_Spec_ID = psId;
                        psv.Name = v;
                        psv.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        psv.User_ID = ps.User_ID;
                        psv.Mall_PVID = "";
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return ps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="value"></param>
        public void CreatePropertyValues(int propertyId, List<string> value)
        {
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                Product_Spec ps=(from prop in db.Product_Spec where prop.Product_Spec_ID==propertyId select prop).FirstOrDefault<Product_Spec>();
                if (ps == null)
                {
                    throw new KMJXCException("属性不存在，不能添加属性值");
                }

                List<Product_Spec_Value> psValues=(from psv in db.Product_Spec_Value where psv.Product_Spec_ID==propertyId select psv).ToList<Product_Spec_Value>();

                if (value != null && value.Count > 0)
                {
                    foreach (string v in value)
                    {
                        Product_Spec_Value propValue = (from propv in psValues where propv.Name == v select propv).FirstOrDefault<Product_Spec_Value>();
                        if (propValue == null)
                        {
                            propValue = new Product_Spec_Value();
                            propValue.Product_Spec_ID = propertyId;
                            propValue.Name = v;
                            propValue.Mall_PVID = "";
                            propValue.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            db.Product_Spec_Value.Add(propValue);
                        }
                    }

                    db.SaveChanges();
                }
            }
            catch
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SyncMallProperties()
        {
            if (string.IsNullOrEmpty(this.Shop.Mall_Shop_ID))
            {
                throw new KMJXCException("");
            }
        }
    }
}
