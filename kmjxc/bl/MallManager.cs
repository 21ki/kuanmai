using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;

namespace KM.JXC.BL
{
    /// <summary>
    /// Mall Manager
    /// </summary>
    public class MallManager
    {
        public MallManager()
        {

        }

        /// <summary>
        /// Get all mall types
        /// </summary>
        /// <returns></returns>
        public List<Mall_Type> GetMalls()
        {
            List<Mall_Type> malls = new List<Mall_Type>();
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                var ms = from malltypes in db.Mall_Type select malltypes;
                malls = ms.ToList<Mall_Type>();
            }
            catch
            {
            }
            finally
            {
            }

            return malls;
        }

        /// <summary>
        /// Update mall name or description
        /// </summary>
        /// <param name="mall"></param>
        /// <returns></returns>
        public bool UpdateMall(Mall_Type mall)
        {
            bool result = false;
            Mall_Type mall_type = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {

                if (mall.Mall_Type_ID > 0)
                {
                    mall_type = this.GetMallDetail(mall.Mall_Type_ID);
                }
                else if (!string.IsNullOrEmpty(mall.Name))
                {
                    mall_type = this.GetMallDetail(mall.Name);
                }

                if (mall_type != null)
                {
                    db.Mall_Type.Attach(mall_type);
                    mall_type.Name = mall.Name;
                    mall_type.Description = mall.Description;
                    db.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Get mall object by mall id
        /// </summary>
        /// <param name="mall_type_id"></param>
        /// <returns></returns>
        public Mall_Type GetMallDetail(long mall_type_id)
        {
            Mall_Type mall = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var mt = from malltype in db.Mall_Type where malltype.Mall_Type_ID == mall_type_id select malltype;
                if (mt != null)
                {
                    mall = mt.ToList<Mall_Type>()[0];
                }
            }
            return mall;
        }

        /// <summary>
        /// Get mall object by mall name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Mall_Type GetMallDetail(string name)
        {
            Mall_Type mall = null;           
            using(KuanMaiEntities db = new KuanMaiEntities())
            {
                var mt= from malltype in db.Mall_Type where malltype.Name == name select malltype;
                if (mt != null)
                {
                    mall = mt.ToList<Mall_Type>()[0];
                }
            } 
            return mall;
        }

        /// <summary>
        /// Add new mall in local db
        /// </summary>
        /// <param name="mall"></param>
        /// <returns></returns>
        public bool AddNewMall(Mall_Type mall)
        {
            bool result = false;
            if (mall == null)
            {
                return result;
            }
            if (string.IsNullOrEmpty(mall.Name)) {
                return result;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Mall_Type.Add(mall);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
